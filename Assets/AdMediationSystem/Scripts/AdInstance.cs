using UnityEngine;

namespace Virterix.AdMediation
{
    public class AdInstance
    {
        public const string AD_INSTANCE_DEFAULT_NAME = "Default";

        public AdInstance(AdNetworkAdapter newtrok)
        {
            m_network = newtrok;
            m_network.OnEvent += OnNetworkEvent;
        }

        public AdInstance(AdNetworkAdapter newtrok, AdType adType, string adID, string name = AD_INSTANCE_DEFAULT_NAME)
        {
            m_enabledState = true;
            m_adType = adType;
            m_adId = adID;
            Name = name;
            m_network = newtrok;
            m_network.OnEvent += OnNetworkEvent;
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }
        string m_name;

        public string NetworkName
        {
            get { return m_network.m_networkName; }
        }

        public string ParametersName
        {
            get { return m_adInstanceParams != null ? m_adInstanceParams.Name : AdInstanceParameters.AD_INSTANCE_PARAMETERS_DEFAULT_NAME; }
        }

        public bool IsDefault
        {
            get { return Name.Length == 0 || Name == AD_INSTANCE_DEFAULT_NAME; }
        }

        public bool WasLastPreparationFailed
        {
            get { return m_wasLastPreparationFailed; }
        }

        public AdNetworkAdapter.AdState State
        {
            get { return m_state; }
            set
            {
                m_state = value;
                if (m_state == AdNetworkAdapter.AdState.Loading)
                {
                    m_wasLastPreparationFailed = false;
                }
            }
        }

        private AdNetworkAdapter.AdState m_state = AdNetworkAdapter.AdState.Uncertain;
        public AdType m_adType;
        public string m_adId;
        public AdNetworkAdapter.TimeoutParams? m_timeout;
        public bool m_bannerVisibled;
        public bool m_enabledState;
        public object m_adView;
        public IAdInstanceParameters m_adInstanceParams;
        public float m_startImpressionTime;
        public float m_displayTime;
        public float m_responseWaitTime = 30f;
        public Coroutine m_waitResponseHandler;

        private bool m_wasLastPreparationFailed;
        private AdNetworkAdapter m_network;

        public void SaveFailedPreparationTime()
        {
            if (m_timeout != null)
            {
                AdNetworkAdapter.TimeoutParams timeoutParameters = m_timeout.Value;
                timeoutParameters.FailedLoadingTime = Time.realtimeSinceStartup;
                m_timeout = timeoutParameters;
            }
        }

        public void Cleanup()
        {
            if (m_network != null)
            {
                m_network.OnEvent += OnNetworkEvent;
            }
        }

        private void OnNetworkEvent(AdNetworkAdapter network, AdType adType, AdEvent adEvent, AdInstance adInstance)
        {
            if (adInstance == this)
            {
                if (adEvent == AdEvent.PreparationFailed)
                {
                    m_wasLastPreparationFailed = true;
                }
                else if (adEvent == AdEvent.Prepared)
                {
                    m_wasLastPreparationFailed = false;
                }
            }
        }
    }
} // namespace Virterix.AdMediation
