using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Boomlagoon.JSON;

namespace Virterix.AdMediation.Editor
{
    public class AdMediationSettingsBuilder
    {
        //-------------------------------------------------------------
        #region Helpers

        public static string GetAdProjectSettingsPath(string projectName, bool includeAssets)
        {
            string resourceFolder = "Resources";
            string resourcePath = "Assets/" + resourceFolder;
            string settingDirectoryPath = string.Format("{0}/{1}", resourcePath, AdMediationSystem.AD_SETTINGS_FOLDER);
            string projectSettingsPath = string.Format("{0}/{1}", settingDirectoryPath, projectName);
            string resultPath = string.Format("{0}/{1}/{2}", includeAssets ? resourcePath : resourceFolder,
                AdMediationSystem.AD_SETTINGS_FOLDER, projectName);

            if (!AssetDatabase.IsValidFolder(resourcePath))
            {
                AssetDatabase.CreateFolder("Assets", resourceFolder);
            }
            if (!AssetDatabase.IsValidFolder(settingDirectoryPath))
            {
                AssetDatabase.CreateFolder(resourcePath, AdMediationSystem.AD_SETTINGS_FOLDER);
            }
            if (!AssetDatabase.IsValidFolder(projectSettingsPath))
            {
                AssetDatabase.CreateFolder(settingDirectoryPath, projectName);
                AssetDatabase.Refresh();
            }
            return resultPath;
        }

        public static void FillMediators(ref List<AdUnitMediator> mediators, List<AdUnitMediator> specificMediators)
        {
            if (specificMediators.Count > 0)
            {
                mediators.AddRange(specificMediators);
            }
        }

        private static BannerPositionContainer[] FindBannerPositions(AdInstanceGenerateDataContainer adInstanceContainer, AdUnitMediator[] mediators)
        {
            List<BannerPositionContainer> positions = new List<BannerPositionContainer>();
            foreach (var mediator in mediators)
            {
                bool isJumpNextMediator = false;
                foreach (var tier in mediator._tiers)
                {
                    foreach (var unit in tier._units)
                    {
                        if (unit._instanceName == adInstanceContainer._adInstance._name)
                        {
                            BannerPositionContainer position = new BannerPositionContainer();
                            position.m_placementName = mediator._name;
                            position.m_bannerPosition = mediator._bannerPosition;
                            positions.Add(position);
                            isJumpNextMediator = true;
                            break;
                        }
                    }
                    if (isJumpNextMediator)
                    {
                        break;
                    }
                }
            }
            return positions.ToArray();
        }

        #endregion // Helpers

        public static string BuildJson(string projectName, AppPlatform platform, AdMediationProjectSettings projectSettings, 
            BaseAdNetworkSettings[] networkSettings, AdUnitMediator[] mediators)
        {
            // Json Settings
            JSONObject json = new JSONObject();
            json.Add("projectName", projectName);
            json.Add("networkResponseWaitTime", 30);
            json.Add("mediators", CreateMediators(projectName, projectSettings, mediators, networkSettings));
            json.Add("networks", CreateNetworks(networkSettings, platform));

            return json.ToString();
        }

        public static GameObject BuildSystemPrefab(string projectName, AdMediationProjectSettings projectSettings, 
            BaseAdNetworkSettings[] networksSettings, AdUnitMediator[] mediators)
        {
            // System Prefab
            GameObject systemObject = CreateSystemObject(projectName, projectSettings, networksSettings, mediators);
            GameObject prefab = SavePrefab(systemObject);
            GameObject.DestroyImmediate(systemObject);
            return prefab;
        }

        public static void BuildBannerAdInstanceParameters(string projectName, BaseAdNetworkSettings[] networkSettings, AdUnitMediator[] mediators)
        {
            string parametersPath = AdMediationSystem.GetAdInstanceParametersPath(projectName);
            foreach (var network in networkSettings)
            {
                if (network._enabled)
                {
                    AdInstanceGenerateDataContainer[] adInstanceDataHolders = network.GetAllAdInstanceDataHolders();
                    foreach (var adInstanceDataHolder in adInstanceDataHolders)
                    {
                        if (adInstanceDataHolder._adType == AdType.Banner)
                        {
                            string instanceName = adInstanceDataHolder._adInstance._name;
                            int bannerType = adInstanceDataHolder._adInstance._bannerType;
                            var bannerPositions = FindBannerPositions(adInstanceDataHolder, mediators);
                            network.CreateBannerAdInstanceParameters(projectName, instanceName, bannerType, bannerPositions);
                        }
                    }
                }
            }
        }

        public static void SetupNetworkScripts(BaseAdNetworkSettings[] networkSettings, bool forceSetup, bool[] networkPreviousEnabledStates = null)
        {
            for (int i = 0; i < networkSettings.Length && i < networkPreviousEnabledStates.Length; i++)
            {
                var network = networkSettings[i];
                if (forceSetup || network._enabled != networkPreviousEnabledStates[i])
                {
                    network.SetupNetworkAdapterScript();
                }             
            }
        }

        //-------------------------------------------------------------
        #region Json Settings

        private static JSONArray CreateMediators(string projectName, AdMediationProjectSettings settings, 
            AdUnitMediator[] mediators, BaseAdNetworkSettings[] networkSettings)
        {
            JSONArray jsonMediators = new JSONArray();
            foreach (AdUnitMediator mediator in mediators)
            {
                JSONObject jsonMediator = new JSONObject();
                jsonMediator.Add("adType", AdUtils.AdTypeToString(mediator._adType));
                jsonMediator.Add("placement", mediator._name);
                jsonMediator.Add("strategy", CreateStrategy(mediator));
                jsonMediators.Add(jsonMediator);
            }
            return jsonMediators;
        }

        private static JSONObject CreateStrategy(AdUnitMediator mediator)
        {
            JSONObject mediationStrategy = new JSONObject();
            mediationStrategy.Add("type", mediator._fetchStrategyType.ToString().ToLower());
            mediationStrategy.Add("tiers", CreateTiers(mediator));
            return mediationStrategy;
        }

        public static JSONArray CreateTiers(AdUnitMediator mediator)
        {
            JSONArray jsonTiers = new JSONArray();
            foreach(var tier in mediator._tiers)
            {
                JSONArray jsonTier = new JSONArray();
                foreach(var unit in tier._units)
                {
                    jsonTier.Add(CreateAdUnit(mediator, unit));
                }
                jsonTiers.Add(jsonTier);
            }
            return jsonTiers;
        }

        private static JSONObject CreateAdUnit(AdUnitMediator mediator, AdUnit adUnit)
        {
            JSONObject jsonUnit = new JSONObject();

            jsonUnit.Add("network", adUnit._networkIdentifier);
            if (adUnit._instanceName != AdMediation.AdInstance.AD_INSTANCE_DEFAULT_NAME)
            {
                jsonUnit.Add("instance", adUnit._instanceName);
            }
            if (adUnit._prepareOnExit)
            {
                jsonUnit.Add("prepareOnExit", adUnit._prepareOnExit);
            }         
            
            switch (mediator._fetchStrategyType)
            {
                case FetchStrategyType.Sequence:
                    if (adUnit._replaced)
                    {
                        jsonUnit.Add("replaced", adUnit._replaced);
                    }
                    break;
                case FetchStrategyType.Random:
                    jsonUnit.Add("percentage", adUnit._percentage);
                    break;
            }

            return jsonUnit;
        }

        private static JSONArray CreateNetworks(BaseAdNetworkSettings[] networkSettings, AppPlatform platform)
        {
            JSONArray jsonNetworks = new JSONArray();

            foreach(var settings in networkSettings)
            { 
                if (settings._enabled)
                {
                    // Common parameters
                    JSONObject jsonNetwork = new JSONObject();
                    jsonNetwork.Add("name", settings._networkIdentifier);
                    if (settings.IsAppIdSupported)
                    {
                        string appId = "";
                        switch (platform)
                        {
                            case AppPlatform.Android:
                                appId = settings._androidAppId;
                                break;
                            case AppPlatform.iOS:
                                appId = settings._iosAppId;
                                break;
                        }
                        jsonNetwork.Add(settings.JsonAppIdKey, appId);
                    }
                    
                    // Insert specifiec paramters
                    Dictionary<string, object> specificParameters = settings.GetSpecificNetworkParameters(platform);
                    foreach (KeyValuePair<string, object> parametersPair in specificParameters)
                    {
                        string valueType = parametersPair.Value.GetType().Name.ToString();
                        switch (valueType)
                        {
                            case "String":
                                jsonNetwork.Add(parametersPair.Key, (string)parametersPair.Value);
                                break;
                            case "Int32":
                                jsonNetwork.Add(parametersPair.Key, (int)parametersPair.Value);
                                break;
                            case "Boolean":
                                jsonNetwork.Add(parametersPair.Key, (bool)parametersPair.Value);
                                break;
                            case "Single":
                                jsonNetwork.Add(parametersPair.Key, (float)parametersPair.Value);
                                break;
                        }
                    }

                    // Ad instances
                    if (!settings.IsTotallyAdInstanceUnsupported)
                    {
                        jsonNetwork.Add("instances", CreateAdnstances(settings, platform));
                    }

                    jsonNetworks.Add(jsonNetwork);
                }
            }
            return jsonNetworks;
        }

        private static JSONArray CreateAdnstances(BaseAdNetworkSettings networkSettings, AppPlatform platform)
        {
            JSONArray jsonInstances = new JSONArray();
            AdInstanceGenerateDataContainer[] allAdInstanceDataHolders = networkSettings.GetAllAdInstanceDataHolders();
            
            foreach(var adInstanceHolder in allAdInstanceDataHolders)
            {
                JSONObject jsonAdInstance = new JSONObject();
                jsonAdInstance.Add("adType", AdUtils.AdTypeToString(adInstanceHolder._adType));
                if (adInstanceHolder._adInstance._name != AdMediation.AdInstance.AD_INSTANCE_DEFAULT_NAME)
                {
                    jsonAdInstance.Add("name", adInstanceHolder._adInstance._name);
                }
                if (adInstanceHolder._adType == AdType.Banner)
                {
                    jsonAdInstance.Add("param", adInstanceHolder._adInstance._name);
                }
                if (networkSettings._responseWaitTime != AdMediationSystem.DEFAULT_NETWORK_RESPONSE_WAIT_TIME)
                {
                    jsonAdInstance.Add("responseWaitTime", networkSettings._responseWaitTime);
                }

                string adUnitId = "";
                switch(platform)
                {
                    case AppPlatform.Android:
                        adUnitId = adInstanceHolder._adInstance._androidId;
                        break;
                    case AppPlatform.iOS:
                        adUnitId = adInstanceHolder._adInstance._iosId;
                        break;
                }
                jsonAdInstance.Add("id", adUnitId);
                if (adInstanceHolder._adInstance._timeout > 0)
                {
                    jsonAdInstance.Add("timeout", adInstanceHolder._adInstance._timeout);
                }
                jsonInstances.Add(jsonAdInstance);
            }

            return jsonInstances;
        }

        #endregion // Json Settings

        //-------------------------------------------------------------
        #region System Prefab

        private static GameObject CreateSystemObject(string projectName, AdMediationProjectSettings commonSettings, BaseAdNetworkSettings[] networksSettings, AdUnitMediator[] mediators)
        {
            GameObject mediationSystemObject = new GameObject(AdMediationSystem.PREFAB_NAME + ".prefab");
            AdMediationSystem admSystem = mediationSystemObject.AddComponent<AdMediationSystem>();
            admSystem.m_projectName = projectName;
            admSystem.m_isLoadOnlyDefaultSettings = true;
            admSystem.m_initializeOnStart = commonSettings._initializeOnStart;
            admSystem.m_personalizeAdsOnInit = commonSettings._personalizeAdsOnInit;
            admSystem.m_testModeEnabled = commonSettings._enableTestMode;
            admSystem.m_testDevices = commonSettings._testDevices;

            GameObject networkAdapterHolder = new GameObject("NetworkAdapters");
            networkAdapterHolder.transform.SetParent(mediationSystemObject.transform);
            FillNetworkHolder(networkAdapterHolder, commonSettings, networksSettings);

            GameObject mediatorHolder = new GameObject("Mediators");
            mediatorHolder.transform.SetParent(mediationSystemObject.transform);
            
            GameObject bannerMediatorHolder = new GameObject("Banner");
            bannerMediatorHolder.transform.SetParent(mediatorHolder.transform);
            FillMediatorHolder(bannerMediatorHolder, commonSettings._bannerMediators);

            GameObject interstitialMediatorHolder = new GameObject("Interstitial");
            interstitialMediatorHolder.transform.SetParent(mediatorHolder.transform);
            FillMediatorHolder(interstitialMediatorHolder, commonSettings._interstitialMediators);

            GameObject incentivizedMediatorHolder = new GameObject("Incentivized");
            incentivizedMediatorHolder.transform.SetParent(mediatorHolder.transform);
            FillMediatorHolder(incentivizedMediatorHolder, commonSettings._incentivizedMediators);

            return mediationSystemObject;
        }

        private static void FillNetworkHolder(GameObject networkHolder, AdMediationProjectSettings commonSettings, BaseAdNetworkSettings[] networksSettings)
        {
            AdType[] adTypeArray = System.Enum.GetValues(typeof(AdType)) as AdType[];
            List<AdType> adTypes = adTypeArray.ToList();
            adTypes.Remove(AdType.Unknown);

            foreach (var settings in networksSettings)
            {
                if (settings._enabled)
                {
                    AdNetworkAdapter adapter = networkHolder.AddComponent(settings.NetworkAdapterType) as AdNetworkAdapter;
                    List<AdNetworkAdapter.AdParam> adSupportedParams = new List<AdNetworkAdapter.AdParam>();
                    for (int i = 0; i < adTypes.Count; i++)
                    {
                        if (settings.IsAdSupported(adTypes[i]))
                        {
                            var supportedParam = new AdNetworkAdapter.AdParam();
                            supportedParam.m_adType = adTypes[i];
                            supportedParam.m_isCheckAvailabilityWhenPreparing = settings.IsCheckAvailabilityWhenPreparing(supportedParam.m_adType);
                            adSupportedParams.Add(supportedParam);
                        }
                    }
                    adapter.m_networkName = settings._networkIdentifier;
                    adapter.m_adSupportParams = adSupportedParams.ToArray();
                    settings.SetupNetworkAdapter(commonSettings, adapter);
                }
            }
        }

        private static void FillMediatorHolder(GameObject mediatorHolder, List<AdUnitMediator> mediators)
        {
            foreach (var model in mediators)
            {
                AdMediator mediator = mediatorHolder.AddComponent<AdMediator>();
                mediator.m_adType = model._adType;
                mediator.m_placementName = model._name;
                mediator.m_fetchOnAdUnitHidden = model._fetchOnAdUnitHidden;
                mediator.m_continueAfterEndSession = model._continueAfterEndSession;
                mediator.m_fetchOnStart = model._fetchOnStart;
                mediator.m_bannerMinDisplayTime = model._adType == AdType.Banner ? model._bannerMinDisplayTime : 0;
                mediator.m_deferredFetchDelay = model._adType == AdType.Incentivized ? model._deferredFetchDelay : -1;
            }
        }

        private static GameObject SavePrefab(GameObject mediationSystemObject)
        {
            string settingsPath = GetAdProjectSettingsPath(mediationSystemObject.GetComponent<AdMediationSystem>().m_projectName, true);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(mediationSystemObject, settingsPath + "/" + AdMediationSystem.PREFAB_NAME + ".prefab");
            AssetDatabase.Refresh();
            return prefab;
        }

        #endregion // System Prefab
    }
} // namespace Virterix.AdMediation.Editor
