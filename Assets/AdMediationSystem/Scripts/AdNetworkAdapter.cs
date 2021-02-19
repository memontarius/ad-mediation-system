using UnityEngine;
using System;
using System.Collections.Generic;
using Boomlagoon.JSON;

namespace Virterix.AdMediation
{
    public enum AdEvent
    {
        Selected,
        Prepared,
        Show,
        Click,
        Hiding,
        PreparationFailed,
        IncentivizedCompleted,
        IncentivizedUncompleted
    }

    public struct IncentivizedReward
    {
        public string label;
        public double amount;
    }

    public partial class AdNetworkAdapter : MonoBehaviour
    {
        //_______________________________________________________________________________
        #region Classes & Structs
        //-------------------------------------------------------------------------------
        /// <summary>
        /// Describes the parameters of the disabling network from handling when failed load
        /// </summary>
        public struct TimeoutParams
        {
            public AdType m_adType;
            public float m_timeout;

            public float FailedLoadingTime
            {
                set
                {
                    m_failedLoadTime = value;
                    m_isSetupFailedLoadTime = true;
                }
                get { return m_failedLoadTime; }
            }

            bool m_isSetupFailedLoadTime;
            float m_failedLoadTime;

            public bool IsTimeout
            {
                get
                {
                    bool active = false;
                    bool canUsed = m_adType != AdType.Unknown && m_timeout > 0.01f;

                    if (canUsed && m_isSetupFailedLoadTime)
                    {
                        float elapsedTime = Time.realtimeSinceStartup - m_failedLoadTime;
                        active = elapsedTime < m_timeout;
                        m_isSetupFailedLoadTime = active;
                    }
                    return active;
                }
            }
        }

        [System.Serializable]
        public struct AdParam
        {
            public AdType m_adType;
            public bool m_isCheckAvailabilityWhenPreparing;
        }

        public struct EventParam
        {
            public AdType m_adType;
            public AdInstance m_adInstance;
            public AdEvent m_adEvent;
        }

        public enum AdState
        {
            Uncertain = 0,
            Loading,
            Received,
            NotAvailable
        }
        #endregion Classes & Structs

        public event Action<AdNetworkAdapter, AdType, AdEvent, AdInstance> OnEvent = delegate { };

        public string m_networkName;
        public AdParam[] m_adSupportParams;

        //_______________________________________________________________________________
        #region Properties
        //-------------------------------------------------------------------------------

        public IncentivizedReward LastReward
        {
            get { return m_lastReward; }
        }

        public string BannerPlacement
        {
            set; get;
        } = null;

        private string AdInstanceParametersPath
        {
            get
            {
                string path = "";
                if (AdInstanceParametersFolder.Length > 0)
                {
                    path = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}/{1}/{2}/{3}/", AdMediationSystem.AD_SETTINGS_FOLDER,
                        AdMediationSystem.Instance.m_projectName, AdMediationSystem.AD_INSTANCE_PARAMETERS_ROOT_FOLDER, AdInstanceParametersFolder);
                }
                return path;
            }
        }

        protected virtual string AdInstanceParametersFolder
        {
            get
            {
                return "";
            }
        }

        #endregion Properties

        private List<EventParam> m_events = new List<EventParam>();
        protected List<IAdInstanceParameters> m_adInstanceParameters = new List<IAdInstanceParameters>();
        private List<AdInstance> m_adInstances = new List<AdInstance>();
        protected IncentivizedReward m_lastReward;

        //_______________________________________________________________________________
        #region MonoBehavior Methods
        //-------------------------------------------------------------------------------

        protected void Update()
        {
            UpdateEvents();
        }

        protected void OnDisable()
        {
            UpdateEvents();
            StopAllCoroutines();
        }
        #endregion MonoBehavior Methods

        //_______________________________________________________________________________
        #region Public Methods
        //-------------------------------------------------------------------------------

        public virtual void Initialize(Dictionary<string, string> parameters = null, JSONArray adInstances = null)
        {
            if (parameters != null)
            {
                InitializeParameters(parameters, adInstances);
            }
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AdNetworkAdapter.Initialize() Initialize network adapter: " + m_networkName + " adInstances:" + m_adInstances.Count);
#endif
        }

        /// <summary>
        /// Not working!
        /// </summary>
        public virtual void DisableWhenInitialize()
        {

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AdNetworkAdapter.DisableWhenInitialize() " + m_networkName);
#endif

            this.enabled = false;
        }

        public virtual bool IsReady(AdInstance adInstance) { return false; }

        public virtual void Prepare(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME) { }

        public virtual bool Show(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME) { return false; }

        public virtual void Hide(AdInstance adInstance) { }

        public bool IsReady(AdType adType, string adInstanceName = AdInstance.AD_INSTANCE_DEFAULT_NAME)
        {
            AdInstance adInstance = GetAdInstance(adType, adInstanceName);
            bool result = IsReady(adInstance);
            return result;
        }

        public void Prepare(AdType adType, string adInstanceName = AdInstance.AD_INSTANCE_DEFAULT_NAME) 
        {
            AdInstance adInstance = GetAdInstance(adType, adInstanceName);
            Prepare(adInstance);
        }

        public bool Show(AdType adType, string adInstanceName = AdInstance.AD_INSTANCE_DEFAULT_NAME) 
        {
            AdInstance adInstance = GetAdInstance(adType, adInstanceName);
            bool result = Show(adInstance);
            return result; 
        }

        public void Hide(AdType adType, string adInstanceName = AdInstance.AD_INSTANCE_DEFAULT_NAME) 
        {
            AdInstance adInstance = GetAdInstance(adType, adInstanceName);
            Hide(adInstance);
        }

        public virtual void HideBannerTypeAdWithoutNotify(AdInstance adInstance)
        {
        }

        public virtual void ResetAd(AdInstance adInstance)
        {
        }

        public virtual bool IsSupported(AdType adType)
        {
            AdParam adSupportParam = GetAdParam(adType);
            bool isSupported = adSupportParam.m_adType != AdType.Unknown;
            return isSupported;
        }

        public bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            AdParam adSupportParam = GetAdParam(adType);
            return adSupportParam.m_isCheckAvailabilityWhenPreparing;
        }

        public void AddEvent(AdType adType, AdEvent adEvent, AdInstance adInstance)
        {
            EventParam eventParam = new EventParam();
            eventParam.m_adType = adType;
            eventParam.m_adInstance = adInstance;
            eventParam.m_adEvent = adEvent;
            m_events.Add(eventParam);
        }

        public void NotifyEvent(AdType adType, AdEvent adEvent, AdInstance adInstance)
        {
            string adInstanceName = adInstance != null ? adInstance.Name : AdInstance.AD_INSTANCE_DEFAULT_NAME;
            if (adInstance != null && (adEvent == AdEvent.PreparationFailed || adEvent == AdEvent.Prepared))
            {
                adInstance.m_lastAdPrepared = adEvent == AdEvent.Prepared;
            }
            OnEvent(this, adType, adEvent, adInstance);
        }

        public IAdInstanceParameters GetAdInstanceParams(AdType adType, string adInstanceName)
        {
            IAdInstanceParameters foundParams = null;
            foreach (IAdInstanceParameters itemParameters in m_adInstanceParameters)
            {
                if (itemParameters.AdvertiseType == adType && itemParameters.Name == adInstanceName)
                {
                    foundParams = itemParameters;
                }
            }
            return foundParams;
        }

        public void AddAdInstance(AdInstance adInstance)
        {
            m_adInstances.Add(adInstance);
        }

        public AdInstance GetAdInstance(AdType adType, string adInstanceName)
        {
            AdInstance foundData = null;

            if (m_adInstances.Count > 0)
            {
                foreach (AdInstance data in m_adInstances)
                {
                    if (data.m_adType != adType)
                    {
                        continue;
                    }
                    if (data.Name == adInstanceName)
                    {
                        foundData = data;
                        break;
                    }
                }
            }
            return foundData;
        }

        public AdInstance GetAdInstance(string adInstanceName)
        {
            AdInstance foundData = null;

            if (m_adInstances.Count > 0)
            {
                foreach (AdInstance data in m_adInstances)
                {
                    if (data.Name == adInstanceName)
                    {
                        foundData = data;
                        break;
                    }
                }
            }
            return foundData;
        }

        public AdInstance GetAdInstanceByAdId(string adId)
        {
            AdInstance foundData = null;

            if (m_adInstances.Count > 0)
            {
                foreach (AdInstance data in m_adInstances)
                {
                    if (data.m_adId == adId)
                    {
                        foundData = data;
                        break;
                    }
                }
            }
            return foundData;
        }

        public AdType GetAdTypeByAdInstanceId(string adId)
        {
            AdType instanceAdType = AdType.Unknown;

            foreach (AdInstance adInstance in m_adInstances)
            {
                if (adInstance.m_adId == adId)
                {
                    instanceAdType = adInstance.m_adType;
                    break;
                }
            }
            return instanceAdType;
        }

        /// <summary>
        /// GDPR Compliance
        /// </summary>
        /// <param name="isPersonalizedAds"></param>
        public virtual void SetPersonalizedAds(bool isPersonalizedAds)
        {
        }

        #endregion Public Methods

        //_______________________________________________________________________________
        #region Internal Methods
        //-------------------------------------------------------------------------------

        private void UpdateEvents()
        {
            if (m_events.Count > 0)
            {
                for (int i = 0; i < m_events.Count; i++)
                {
                    EventParam eventParam = m_events[i];
                    NotifyEvent(eventParam.m_adType, eventParam.m_adEvent, eventParam.m_adInstance);
                }
                m_events.Clear();
            }
        }

        /// <summary>
        /// Initialises ad instance from config file (Default instance initialise manually). When overriding it the base method call required or setup parameters
        /// </summary>
        protected virtual void InitializeAdInstanceData(AdInstance adInstance, JSONValue jsonAdInstance)
        {
            adInstance.Name = jsonAdInstance.Obj.ContainsKey("name") ? jsonAdInstance.Obj.GetString("name") : AdInstance.AD_INSTANCE_DEFAULT_NAME;
            string parametersName = jsonAdInstance.Obj.ContainsKey("param") ? jsonAdInstance.Obj.GetString("param") : AdInstanceParameters.AD_INSTANCE_PARAMETERS_DEFAULT_NAME;
            adInstance.m_adType = AdUtils.StringToAdType(jsonAdInstance.Obj.GetString("adType"));
            adInstance.m_adId = jsonAdInstance.Obj.GetString("id");
            adInstance.m_adInstanceParams = GetAdInstanceParams(adInstance.m_adType, parametersName);
            if (adInstance.m_adType == AdType.Banner && adInstance.m_adInstanceParams == null)
            {
                Debug.LogWarning("[AdMediationSystem] Ad instance banner " + m_networkName + " parameters in NULL! It needs to be fixed.");
            }
            if (jsonAdInstance.Obj.ContainsKey("timeout"))
            {
                TimeoutParams timeoutParameters = new TimeoutParams();
                timeoutParameters.m_timeout = (float)jsonAdInstance.Obj.GetNumber("timeout");
                timeoutParameters.m_adType = adInstance.m_adType;
                adInstance.m_timeout = timeoutParameters;
            }

            string responseWaitTimeKey = "responseWaitTime";
            if (jsonAdInstance.Obj.ContainsKey(responseWaitTimeKey))
            {
                adInstance.m_responseWaitTime = (float)jsonAdInstance.Obj.GetNumber(responseWaitTimeKey);
            }
            else
            {
                adInstance.m_responseWaitTime = AdMediationSystem.Instance.DefaultNetworkResponseWaitTime;
            }

            m_adInstances.Add(adInstance);
        }

        public static string GetAdInstanceFailedLoadingTimeSaveKey(AdNetworkAdapter network, AdInstance adInstance)
        {
            string saveKey = String.Format("adm.timeout.date.{0}.{1}.{2}", network.m_networkName, adInstance.m_adType.ToString(), adInstance.Name);
            saveKey = saveKey.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            return saveKey;
        }

        protected virtual void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            InitializeAdInstanceParameters();
            if (jsonAdInstances != null)
            {
                foreach (JSONValue jsonAdInstance in jsonAdInstances)
                {
                    AdInstance adInstance = CreateAdInstanceData(jsonAdInstance);
                    InitializeAdInstanceData(adInstance, jsonAdInstance);
                }
            }
        }

        /// <summary>
        /// Should implementation in inheritors (Fabric method)
        /// </summary>
        protected virtual AdInstance CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            return new AdInstance();
        }

        private AdParam GetAdParam(AdType adType)
        {
            AdParam adSupportParam = new AdParam();
            foreach (AdParam param in m_adSupportParams)
            {
                if (param.m_adType == adType)
                {
                    adSupportParam = param;
                    break;
                }
            }
            return adSupportParam;
        }

        /// <summary>
        /// Initialises parameters from srciptable objects
        /// </summary>
        private void InitializeAdInstanceParameters()
        {
            if (AdInstanceParametersPath.Length > 0)
            {
                string path = AdInstanceParametersPath;
                UnityEngine.Object[] parameters = Resources.LoadAll(path);
                foreach (UnityEngine.Object itemParameters in parameters)
                {
                    IAdInstanceParameters adInstanceParameters = itemParameters as IAdInstanceParameters;
                    if (adInstanceParameters != null)
                    {
                        m_adInstanceParameters.Add(adInstanceParameters);
                    }
                }
            }
        }
        #endregion Internal Methods

    }
} // namespace Virterix.AdMediation
