﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Boomlagoon.JSON;
using Virterix.Common;

namespace Virterix.AdMediation
{
    public enum AppPlatform
    {
        Android,
        iOS
    }

    public class AdMediationSystem : Singleton<AdMediationSystem>
    {
        public const string AD_SETTINGS_FOLDER = "AdmSettings";
        public const string PREFAB_NAME = "AdMediationSystem";
        public const string PLACEMENT_DEFAULT_NAME = "Default";

        public const string AD_INSTANCE_PARAMETERS_ROOT_FOLDER = "AdInstanceParameters";
        public const string AD_INSTANCE_PARAMETERS_FILE_EXTENSION = ".asset";

        public const int DEFAULT_NETWORK_RESPONSE_WAIT_TIME = 30;

        public enum AdSettingsCompareMode
        {
            None,
            Version,
            Hash
        }

        enum SetupSettingsState
        {
            Successful,
            RequiredCheckUpdate,
            Failure
        }

        struct NetworkParams
        {
            public Dictionary<string, string> m_parameters;
            public JSONArray m_adInstances;
        }

        //===============================================================================
        #region Configuration variables
        //-------------------------------------------------------------------------------

        private const string HASH_SAVE_KEY = "adm.settings.hash";
        private const string PERSONALIZED_ADS_SAVE_KEY = "adm.ads.personalized";
        private const string SETTINGS_VERSION_PARAM_KEY = "adm.settings.version";

        #endregion // Configuration variables

        //===============================================================================
        #region Variables
        //-------------------------------------------------------------------------------

        public string m_projectName;
        public bool m_isLoadOnlyDefaultSettings = true;
        [Tooltip("Compare settings loaded from server")]
        public AdSettingsCompareMode m_settingsCompareMode;
        public AdRemoteSettingsProvider m_remoteSettingsProvider;
        public AppPlatform m_defaultPlatformName;
        public string m_hashCryptKey;
        public bool m_isInitializeOnStart = true;
        public bool m_isPersonalizeAdsOnInit = true;
        // For GDPR Compliance
        private bool m_isPersonalizedAds;

        public static event Action OnInitializeComplete = delegate { };
        /// <summary>
        /// Callback all events of advertising networks.
        /// 5th parameter is the ad instance name
        /// </summary>
        public static event Action<AdMediator, AdNetworkAdapter, AdType, AdEvent, string> OnAdNetworkEvent = delegate { };

        private Hashtable m_userParameters = new Hashtable();
        private AdNetworkAdapter[] m_networkAdapters;
        private List<AdMediator> m_mediators = new List<AdMediator>();
        private JSONObject m_currSettings;

        /// <summary>
        /// Use a personal data of user. For GDPR Compliance
        /// </summary>
        public bool IsPersonalizedAds
        {
            get { return m_isPersonalizedAds; }
        }

        public bool IsInitialized
        {
            get { return m_isInitialized; }
        }
        bool m_isInitialized;

        public JSONObject CurrSettings
        {
            get { return m_currSettings; }
        }

        public string PlatfomName
        {
            get
            {
                string platformName = m_defaultPlatformName.ToString();
                
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        platformName = AppPlatform.Android.ToString();
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        platformName = AppPlatform.iOS.ToString();
                        break;
                }
                return platformName;
            }
        }

        public InternetChecker InternetChecker
        {
            get
            {
                if (m_internetChecker == null)
                {
                    m_internetChecker = InternetChecker.Create();
                }
                return m_internetChecker;
            }
        }
        InternetChecker m_internetChecker;

        public static string AdInstanceParametersPath
        {
            get
            {
                string path = GetAdInstanceParametersPath(AdMediationSystem.Instance.m_projectName);
                return path;
            }
        }

        public float DefaultNetworkResponseWaitTime
        {
            get;
            private set;
        } = AdMediationSystem.DEFAULT_NETWORK_RESPONSE_WAIT_TIME;

        private string SettingsFileName
        {
            get { return PlatfomName + "_settings"; }
        }

        private string DefaultSettingsFilePathInResources
        {
            get
            {
                string settingsFilePath = AD_SETTINGS_FOLDER + "/" + m_projectName + "/" + SettingsFileName;
                return settingsFilePath;
            }
        }

        // Path to settings file
        private string SettingsFilePath
        {
            get
            {
                string settingsFilePath = Application.persistentDataPath + "/" + SettingsFileName + ".json";
                return settingsFilePath;
            }
        }

        // Returns settings version
        private int CurrSettingsVersion
        {
            get
            {
                int settingsVersion = -1;
                if (m_currSettings != null)
                {
                    if (m_currSettings.ContainsKey(SETTINGS_VERSION_PARAM_KEY))
                    {
                        settingsVersion = Convert.ToInt32(m_currSettings.GetValue(SETTINGS_VERSION_PARAM_KEY).Number);
                    }
                }
                return settingsVersion;
            }
        }


        #endregion // Variables

        //===============================================================================
        #region MonoBehaviour methods
        //-------------------------------------------------------------------------------

        private void Awake()
        {
            m_isPersonalizedAds = PlayerPrefs.GetInt(PERSONALIZED_ADS_SAVE_KEY, 1) == 1 ? true : false;
            if (m_remoteSettingsProvider != null)
            {
                m_remoteSettingsProvider.OnSettingsReceived += OnRemoteSettingsReceived;
            }
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            if (m_isInitializeOnStart)
            {
                Initialize();
            }
        }
        #endregion MonoBehaviour methods

        //===============================================================================
        #region Get configure parameters
        //-------------------------------------------------------------------------------

        public bool GetUserParam<T>(string key, ref T value)
        {
            if (m_userParameters.ContainsKey(key))
            {
                value = (T)m_userParameters[key];
                return true;
            }
            return false;
        }

        public bool GetUserIntParam(string key, ref int value)
        {
            if (m_userParameters.ContainsKey(key))
            {
                try
                {
                    double val = (double)m_userParameters[key];
                    value = Convert.ToInt32(val);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public bool GetUserBooleanParam(string key, ref bool value)
        {
            if (m_userParameters.ContainsKey(key))
            {
                try
                {
                    value = (bool)m_userParameters[key];
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public bool GetUserDoubleParam(string key, ref double value)
        {
            if (m_userParameters.ContainsKey(key))
            {
                try
                {
                    value = (double)m_userParameters[key];
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public string GetUserParam(string key)
        {
            string result = "";
            if (m_userParameters.ContainsKey(key))
            {
                result = m_userParameters[key].ToString();
            }
            return result;
        }

        #endregion // Get configure parameters

        //===============================================================================
        #region Core methods
        //-------------------------------------------------------------------------------

        public static string GetAdInstanceParametersPath(string projectPath)
        {
            string path = String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Resources/{0}/{1}/{2}", AD_SETTINGS_FOLDER, projectPath,
                    AdMediationSystem.AD_INSTANCE_PARAMETERS_ROOT_FOLDER);
            return path;
        }

        public AdNetworkAdapter GetNetwork(string networkName)
        {
            AdNetworkAdapter foundNetwork = null;
            if (m_networkAdapters != null)
            {
                foreach (AdNetworkAdapter networkAdapter in m_networkAdapters)
                {
                    if (networkAdapter.m_networkName.Equals(networkName))
                    {
                        foundNetwork = networkAdapter;
                        break;
                    }
                }
            }
            return foundNetwork;
        }

        public AdMediator GetMediator(AdType adType, string placementName = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdMediator foundMediator = null;
            foreach (AdMediator mediator in m_mediators)
            {
                if (mediator.m_adType == adType && mediator.m_placementName == placementName)
                {
                    foundMediator = mediator;
                    break;
                }
            }
            return foundMediator;
        }

        public AdMediator[] GetAllMediators(AdType adType)
        {
            List<AdMediator> mediators = new List<AdMediator>();
            foreach (AdMediator mediator in m_mediators)
            {
                if (mediator.m_adType == adType)
                {
                    mediators.Add(mediator);
                }
            }
            return mediators.ToArray();
        }

        public static void Fetch(AdType adType, string placementName = AdMediationSystem.PLACEMENT_DEFAULT_NAME, Hashtable parameters = null)
        {
            AdMediator mediator = Instance.GetMediator(adType, placementName);
            if (mediator != null)
            {
                mediator.Fetch();
            }
            else
            {
                Debug.Log("AdMediationSystem.Fetch() Not found mediator: " + adType.ToString());
            }
        }

        public static void Show(AdType adType, string placementName = AdMediationSystem.PLACEMENT_DEFAULT_NAME, Hashtable parameters = null)
        {
            AdMediator mediator = Instance.GetMediator(adType, placementName);
            if (mediator != null)
            {
                mediator.Show();
            }
            else
            {
                Debug.Log("AdMediationSystem.Fetch() Not found mediator: " + adType.ToString());
            }
        }

        public static void Hide(AdType adType, string placementName = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdMediator mediator = Instance.GetMediator(adType, placementName);
            if (mediator != null)
            {
                mediator.Hide();
            }
            else
            {
                Debug.Log("AdMediationSystem.Hide() Not found mediator " + adType.ToString());
            }
        }

        public static void NotifyAdNetworkEvent(AdMediator mediator, AdNetworkAdapter network, AdType adType, AdEvent adEvent, string adInstanceName)
        {
            OnAdNetworkEvent(mediator, network, adType, adEvent, adInstanceName);
        }

        /// <summary>
        /// Sets personalized ads mode. GDPR Compliance
        /// </summary>
        /// <param name="isPersonalizedAds"></param>
        /// <param name="isAnalyticsControl"></param>
        public void SetPersonalizedAds(bool isPersonalizedAds, bool isAnalyticsControl = true)
        {
            m_isPersonalizedAds = isPersonalizedAds;
            PlayerPrefs.SetInt(PERSONALIZED_ADS_SAVE_KEY, isPersonalizedAds ? 1 : 0);

            if (m_networkAdapters != null)
            {
                foreach (AdNetworkAdapter network in m_networkAdapters)
                {
                    network.SetPersonalizedAds(isPersonalizedAds);
                }
            }

            if (isAnalyticsControl)
            {
                UnityEngine.Analytics.Analytics.enabled = isPersonalizedAds;
            }
        }

        #endregion // Mediation ad networks

        //===============================================================================
        #region Other internal methods
        //-------------------------------------------------------------------------------

        private void CalculateAndSaveSettingsHash(string settings)
        {
            string hash = AdUtils.GetHash(settings);
            string encodedHash = CryptString.Encode(hash, m_hashCryptKey);
            PlayerPrefs.SetString(HASH_SAVE_KEY, encodedHash);
        }

        private void SaveSettingsHash(string settingsHash)
        {
            string encodedHash = CryptString.Encode(settingsHash, m_hashCryptKey);
            PlayerPrefs.SetString(HASH_SAVE_KEY, encodedHash);
        }

        private bool IsSettingsHashValid(string settings)
        {
            string encodedHash = PlayerPrefs.GetString(HASH_SAVE_KEY, "");
            string savedHash = CryptString.Decode(encodedHash, m_hashCryptKey);
            string currHash = AdUtils.GetHash(settings);
            bool isValid = currHash == savedHash;
            return isValid;
        }

        private object JsonValueToObject(JSONValue jsonValue)
        {
            object result = null;

            switch(jsonValue.Type)
            {
                case JSONValueType.String:
                    result = jsonValue.Str;
                    break;
                case JSONValueType.Boolean:
                    result = jsonValue.Boolean;
                    break;
                case JSONValueType.Number:
                    result = jsonValue.Number;
                    break;
            }
            return result;
        }

        private string JsonValueToString(JSONValue jsonValue)
        {
            string valueStr = "";
            if (jsonValue.Type == JSONValueType.String)
            {
                valueStr = jsonValue.Str;
            }
            else
            {
                valueStr = jsonValue.ToString();
            }
            return valueStr;
        }

        private AdMediator GetOrCreateMediator(AdType adType, string placementName = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdMediator foundMediator = null;
            foreach (AdMediator mediator in m_mediators)
            {
                if (mediator.m_adType == adType && mediator.m_placementName == placementName)
                {
                    foundMediator = mediator;
                    break;
                }
            }

            if (foundMediator == null)
            {
                AdMediator createdMediator = this.gameObject.AddComponent<AdMediator>();
                createdMediator.m_adType = adType;
                createdMediator.m_placementName = placementName;
                m_mediators.Add(createdMediator);
                foundMediator = createdMediator;
            }
            return foundMediator;
        }

        private void NotifyInitializeCompleted()
        {
            m_isInitialized = true;
            OnInitializeComplete();
        }

        #endregion // Other internal methods

        //===============================================================================
        #region Initialize
        //-------------------------------------------------------------------------------

        public static AdMediationSystem Load(string projectName = "")
        {
            GameObject prefab = null;

            if (projectName.Length > 0)
            {
                string prefabPath = string.Format("{0}/{1}/{2}", AD_SETTINGS_FOLDER, projectName, PREFAB_NAME);
                prefab = Resources.Load<GameObject>(prefabPath);
            }
            else
            {
                AdMediationSystem[] preafbs = Resources.LoadAll<AdMediationSystem>(AD_SETTINGS_FOLDER);
                if (preafbs.Length > 0)
                {
                    prefab = preafbs[0].gameObject;
                }
            }

            AdMediationSystem mediationSystem = null;
            if (prefab != null)
            {
                mediationSystem = Instantiate(prefab).GetComponent<AdMediationSystem>();
                mediationSystem.name = PREFAB_NAME;
#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("[AdMediationSystem] Project settings loaded: " + mediationSystem.m_projectName);
#endif
            }
            return mediationSystem;
        }

        public void Initialize()
        {
            m_networkAdapters = GetComponentsInChildren<AdNetworkAdapter>(true);
            AdMediator[] mediators = GetComponentsInChildren<AdMediator>(true);
            m_mediators.AddRange(mediators);
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            if (!m_isLoadOnlyDefaultSettings && m_remoteSettingsProvider != null && m_remoteSettingsProvider.IsUpdateRequired)
            {
                m_remoteSettingsProvider.Request();
            }
            else
            {
                LoadJsonSettingsFromFile(ref m_currSettings, m_isLoadOnlyDefaultSettings);
                SetupCurrentSettings();
            }
        }

        /// <summary>
        /// Setup settings from json object
        /// </summary>
        private bool SetupSettings(JSONObject jsonSettings)
        {
            bool setupSettingsSuccess = false;

            string userParametersKey = "userParameters";
            string mediatorsKey = "mediators";
            string adTypeKey = "adType";
            string mediatorPlacementNameKey = "placement";
            string networkAdInstancesNameKey = "instances";
            string strategyKey = "strategy";
            string networkResponseWaitTimeKey = "networkResponseWaitTime";
            string typeInStrategyKey = "type";
            string networkNameInUnitKey = "network";
            string adInstanceNameInUnitKey = "instance";
            string unitAdTypeKey = "internalAdType";
            string unitAdPrepareOnExitKey = "prepareOnExit";

            string networksKey = "networks";
            string networkNameKey = "name";

            Dictionary<AdNetworkAdapter, NetworkParams> dictNetworks = new Dictionary<AdNetworkAdapter, NetworkParams>();
            Dictionary<AdMediator, List<AdUnit[]>> initMediators = new Dictionary<AdMediator, List<AdUnit[]>>();

            if (jsonSettings.ContainsKey(networkResponseWaitTimeKey))
            {
                DefaultNetworkResponseWaitTime = (float)jsonSettings.GetNumber(networkResponseWaitTimeKey);
            }

            try
            {
                if (jsonSettings.ContainsKey(userParametersKey))
                {
                    JSONArray userParametersJsonArray = jsonSettings.GetArray(userParametersKey);
                    foreach (JSONValue jsonParams in userParametersJsonArray)
                    {
                        string key = jsonParams.Obj["key"].Str;
                        object paramValue = null;
                        JSONValue jsonValue = jsonParams.Obj["value"];
                        switch (jsonValue.Type)
                        {
                            case JSONValueType.Boolean:
                                paramValue = jsonValue.Boolean;
                                break;
                            case JSONValueType.Number:
                                paramValue = jsonValue.Number;
                                break;
                            case JSONValueType.String:
                                paramValue = jsonValue.Str;
                                break;
                        }
                        m_userParameters[key] = paramValue;
                    }
                }

                // Parse networks
                JSONArray jsonArrNetwork = jsonSettings.GetArray(networksKey);
                AdNetworkAdapter networkAdapter = null;

                foreach (JSONValue jsonValNetworkParams in jsonArrNetwork)
                {
                    string networkName = jsonValNetworkParams.Obj.GetValue(networkNameKey).Str;
                    networkAdapter = GetNetwork(networkName);

                    if (networkAdapter != null)
                    {
                        if (jsonValNetworkParams.Obj.ContainsKey("enabled"))
                        {
                            networkAdapter.enabled = jsonValNetworkParams.Obj.GetBoolean("enabled");
                            if (!networkAdapter.enabled)
                            { 
                                continue;
                            }
                        }
                        Dictionary<string, string> dictNetworkParams = new Dictionary<string, string>();

                        // Parse parameters
                        foreach (KeyValuePair<string, JSONValue> pairValue in jsonValNetworkParams.Obj)
                        {
                            dictNetworkParams.Add(pairValue.Key, JsonValueToString(pairValue.Value));
                        }

                        NetworkParams networkParams = new NetworkParams();
                        networkParams.m_parameters = dictNetworkParams;

                        if (jsonValNetworkParams.Obj.ContainsKey(networkAdInstancesNameKey))
                        {
                            networkParams.m_adInstances = jsonValNetworkParams.Obj.GetArray(networkAdInstancesNameKey);
                        }

                        dictNetworks.Add(networkAdapter, networkParams);
                    }
                    else
                    {
                        Debug.LogWarning("AdMediationSystem.SetupNetworkParameters() Initializing networks. Not found Ad network adapter with name: " + networkName);
                    }
                }

                // Parse mediators
                JSONArray jsonArrMediators = jsonSettings.GetArray(mediatorsKey);
                Dictionary<string, object> dictUnitParams = new Dictionary<string, object>();
                
                foreach (JSONValue jsonMediationParams in jsonArrMediators)
                {
                    string adTypeName = jsonMediationParams.Obj.GetValue(adTypeKey).Str;
                    JSONObject jsonStrategy = jsonMediationParams.Obj.GetValue(strategyKey).Obj;
                    string strategyTypeName = jsonStrategy.GetValue(typeInStrategyKey).Str;
                    AdType adType = AdUtils.StringToAdType(adTypeName);
                    string mediatorPlacementName = AdMediationSystem.PLACEMENT_DEFAULT_NAME;
                    if (jsonMediationParams.Obj.ContainsKey(mediatorPlacementNameKey))
                    {
                        mediatorPlacementName = jsonMediationParams.Obj.GetValue(mediatorPlacementNameKey).Str;
                    }
                    AdMediator mediator = GetOrCreateMediator(adType, mediatorPlacementName);
                    List<AdUnit> units = new List<AdUnit>();

                    // Fetch Strategy
                    mediator.FetchStrategy = AdFactory.CreateFetchStrategy(strategyTypeName);

                    // Ad Units
                    List<AdUnit[]> tierUnits = new List<AdUnit[]>();
                    if (jsonStrategy.ContainsKey("tiers"))
                    {
                        JSONArray jsonArrTiers = jsonStrategy.GetArray("tiers");

                        for (int tierIndex = 0; tierIndex < jsonArrTiers.Length; tierIndex++)
                        {
                            JSONValue jsonTier = jsonArrTiers[tierIndex];
                            AdUnit[] arrUnits = new AdUnit[jsonTier.Array.Length];
                            tierUnits.Add(arrUnits);

                            for (int unitIndex = 0; unitIndex < jsonTier.Array.Length; unitIndex++)
                            {
                                JSONValue jsonNetworkUnits = jsonTier.Array[unitIndex];

                                string networkName = jsonNetworkUnits.Obj.GetValue(networkNameInUnitKey).Str;
                                string adInstanceName = AdInstanceData._AD_INSTANCE_DEFAULT_NAME;
                                if (jsonNetworkUnits.Obj.ContainsKey(adInstanceNameInUnitKey))
                                {
                                    adInstanceName = jsonNetworkUnits.Obj.GetValue(adInstanceNameInUnitKey).Str;
                                }

                                networkAdapter = GetNetwork(networkName);
                                AdType unitAdType = adType;
                                if (networkAdapter == null || !networkAdapter.enabled)
                                {
                                    continue;
                                }

                                // Internal ad type
                                string internalAdTypeName = "";
                                if (jsonNetworkUnits.Obj.ContainsKey(unitAdTypeKey))
                                {
                                    internalAdTypeName = jsonNetworkUnits.Obj.GetString(unitAdTypeKey);
                                    AdType convertedAdType = AdUtils.StringToAdType(internalAdTypeName);
                                    unitAdType = convertedAdType != AdType.Unknown ? convertedAdType : unitAdType;
                                }

                                bool isPrepareOnExit = false;
                                if (jsonNetworkUnits.Obj.ContainsKey(unitAdPrepareOnExitKey))
                                {
                                    isPrepareOnExit = jsonNetworkUnits.Obj.GetBoolean(unitAdPrepareOnExitKey);
                                }

                                // If the network enabled and support this type of advertising then add it to list 
                                if (networkAdapter != null)
                                {
                                    // Parse ad unit parameters
                                    foreach (KeyValuePair<string, JSONValue> pairValue in jsonNetworkUnits.Obj)
                                    {
                                        dictUnitParams.Add(pairValue.Key, JsonValueToObject(pairValue.Value));
                                    }
                                    // Create strategy parameters
                                    BaseFetchStrategyParams fetchStrategyParams = AdFactory.CreateFetchStrategyParams(strategyTypeName, dictUnitParams);
                                    if (fetchStrategyParams == null)
                                    {
                                        Debug.LogWarning("AdMediationSystem.SetupNetworkParameters() Not found fetch strategy parameters");
                                    }

                                    // Create ad unit
                                    AdUnit unit = new AdUnit(mediatorPlacementName, unitAdType, adInstanceName, 
                                        networkAdapter, fetchStrategyParams, isPrepareOnExit);

                                    arrUnits[unitIndex] = unit;
                                }
                                else
                                {
                                    Debug.LogWarning("AdMediationSystem.SetupNetworkParameters() Not found network adapter: " + networkName);
                                }
                                dictUnitParams.Clear();
                            }
                        }
                    }

                    initMediators.Add(mediator, tierUnits);
                }

                setupSettingsSuccess = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("AdMediationSystem.SetupSettings() Parse settings failed! Catch exception when setup settings. Message: " + e.Message + " __StackTrace__: " + e.StackTrace);
            }

            if (setupSettingsSuccess)
            {
                // Initialization networks
                foreach (KeyValuePair<AdNetworkAdapter, NetworkParams> pair in dictNetworks)
                {
                    AdNetworkAdapter netwrok = pair.Key;
                    Dictionary<string, string> networkParameters = (Dictionary<string, string>)pair.Value.m_parameters;
                    netwrok.Initialize(networkParameters, pair.Value.m_adInstances);

                    if (m_isPersonalizeAdsOnInit)
                    {
                        netwrok.SetPersonalizedAds(IsPersonalizedAds);
                    }
                }

                // Initialization mediators
                foreach (var pair in initMediators)
                {
                    AdMediator mediator = pair.Key;
                    mediator.Initialize(initMediators[mediator]);
                }
            }
            else
            {
                m_userParameters = new Hashtable();
                foreach (AdMediator mediator in m_mediators)
                {
                    mediator.FetchStrategy = new EmptyFetchStrategy();
                }
            }

            return setupSettingsSuccess;
        }

        private void SetupCurrentSettings()
        {
            if (m_currSettings != null)
            {
                bool setupSuccess = SetupSettings(m_currSettings);
                if (!setupSuccess)
                {
                    DeleteSavedJsonSettings();
                    bool isLoadedDefaultSettings = LoadJsonSettingsFromFile(ref m_currSettings, true);
                    if (isLoadedDefaultSettings)
                    {
                        SetupSettings(m_currSettings);
                    }
                }
            }
            NotifyInitializeCompleted();
        }

        private void DeleteSavedJsonSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                File.Delete(SettingsFilePath);
            }
        }

        #endregion // Initialize

        //===============================================================================
        #region Load
        //-------------------------------------------------------------------------------

        private bool LoadJsonSettingsFromFile(ref JSONObject resultSettings, bool ignoreLoadedSettings = false)
        {
            JSONObject settings = null;
            bool isLoadedSuccessfully = false;

            if (!ignoreLoadedSettings && File.Exists(SettingsFilePath))
            {
                string jsonString = File.ReadAllText(SettingsFilePath);

                if (IsSettingsHashValid(jsonString))
                {
                    settings = JSONObject.Parse(jsonString);
                    isLoadedSuccessfully = settings != null;
                }

                if (!isLoadedSuccessfully)
                {
                    File.Delete(SettingsFilePath);
                }

#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("AdMediationSystem.LoadJsonSettingsFromFile() " + (isLoadedSuccessfully ? " Valid settings" : " Not valid settings"));
#endif
            }

            if (!isLoadedSuccessfully)
            {
                TextAsset textAsset = Resources.Load<TextAsset>(DefaultSettingsFilePathInResources);
                if (textAsset != null)
                {
                    string jsonString = textAsset.text;
                    settings = JSONObject.Parse(jsonString);

#if AD_MEDIATION_DEBUG_MODE
                    Debug.Log("AdMediationSystem.LoadJsonSettingsFromFile() Loaded default settings file");
#endif
                }
            }

            resultSettings = settings;
            isLoadedSuccessfully = resultSettings != null;

            return isLoadedSuccessfully;
        }

        private void OnRemoteSettingsReceived(AdRemoteSettingsProvider.LoadingState loadingState, JSONObject remoteJsonSettings)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AdMediationSystem.OnRemoteSettingsReceived() Loading remote settings done. loadingState: " + loadingState);
#endif

            if (loadingState != AdRemoteSettingsProvider.LoadingState.Failed &&
                loadingState != AdRemoteSettingsProvider.LoadingState.UnmodifiedLoaded)
            {

                if (remoteJsonSettings != null)
                {
                    bool isRemoteSettingsModified = true;
                    string currSettingsStr = CurrSettings != null ? CurrSettings.ToString() : "";

                    string remoteHash = "";
                    string localHash = "";
                    int localVersion = CurrSettingsVersion;
                    int remoteVersion = -1;

                    switch (m_settingsCompareMode)
                    {
                        case AdSettingsCompareMode.Hash:
                            localHash = AdUtils.GetHash(currSettingsStr);
                            remoteHash = AdUtils.GetHash(remoteJsonSettings.ToString());
                            isRemoteSettingsModified = localHash != remoteHash;
                            break;
                        case AdSettingsCompareMode.Version:
                            if (remoteJsonSettings.ContainsKey(SETTINGS_VERSION_PARAM_KEY))
                            {
                                remoteVersion = Convert.ToInt32(remoteJsonSettings.GetValue(SETTINGS_VERSION_PARAM_KEY).Number);
                            }
                            isRemoteSettingsModified = remoteVersion > localVersion;
                            break;
                    }

#if AD_MEDIATION_DEBUG_MODE
                    if (m_settingsCompareMode == AdSettingsCompareMode.Hash)
                        Debug.Log("AdMediationSystem.OnRemoteSettingsReceived() Compare by hash. Is identically:" + (localHash == remoteHash));
                    else if (m_settingsCompareMode == AdSettingsCompareMode.Version)
                        Debug.Log("AdMediationSystem.OnRemoteSettingsReceived() Compare by version local:" + localVersion + " remote:" + remoteVersion);
                    else
                        Debug.Log("AdMediationSystem.OnRemoteSettingsReceived()");
#endif

                    if (isRemoteSettingsModified)
                    {
                        if (remoteHash.Length == 0)
                        {
                            remoteHash = AdUtils.GetHash(remoteJsonSettings.ToString());
                        }
                        SaveSettingsHash(remoteHash);

#if AD_MEDIATION_DEBUG_MODE
                        Debug.Log("AdMediationSystem.OnRemoteSettingsReceived() Save file: " + SettingsFilePath);
#endif
                        File.WriteAllText(this.SettingsFilePath, remoteJsonSettings.ToString());
                        m_currSettings = remoteJsonSettings;
                    }
                }
                else
                {
#if AD_MEDIATION_DEBUG_MODE
                    Debug.LogWarning("AdMediationSystem.OnRemoteSettingsReceived() Remote settings was received is NULL!");
#endif
                }
            }

            SetupCurrentSettings();
        }

        #endregion // Load
    }
} // namespace Virterix.AdMediation
