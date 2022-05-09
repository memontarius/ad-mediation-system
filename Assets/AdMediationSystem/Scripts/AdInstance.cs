using System;
using UnityEngine;

namespace Virterix.AdMediation
{
    public class AdInstance
    {
        public const string AD_INSTANCE_DEFAULT_NAME = "Default";
        public const float MAX_TIMEOUT_MULTIPLAYER = float.MaxValue;
        
        public AdInstance(AdNetworkAdapter newtrok)
        {
            m_network = newtrok;
        }

        public AdInstance(AdNetworkAdapter newtrok, AdType adType, string adID, string name = AD_INSTANCE_DEFAULT_NAME)
        {
            m_enabledState = true;
            m_adType = adType;
            m_adId = adID;
            Name = name;
            m_network = newtrok;
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
                    m_wasLastPreparationFailed = false;
            }
        }

        public string CurrPlacement { get; set; }
        public int FailedLoadingCount { get; private set; }
        
        private AdNetworkAdapter.AdState m_state = AdNetworkAdapter.AdState.Uncertain;
        public AdType m_adType;
        public string m_adId;
        public AdNetworkAdapter.TimeoutParams? m_timeout;
        public bool m_bannerDisplayed;
        public bool m_enabledState;
        public object m_adView;
        public IAdInstanceParameters m_adInstanceParams;
        public float m_startImpressionTime;
        public float m_displayTime; 
        public Coroutine m_waitResponseHandler;
        
        private bool m_wasLastPreparationFailed;
        private AdNetworkAdapter m_network;
        
        public void SaveFailedPreparationTime(float timeoutMultiplier)
        {
            if (m_timeout != null)
            {
                AdNetworkAdapter.TimeoutParams timeoutParameters = m_timeout.Value;
                timeoutParameters.FailedLoadingTime = Time.realtimeSinceStartup;
                timeoutParameters.TimeoutMultiplier = timeoutMultiplier;
                m_timeout = timeoutParameters;
            }
        }
        
        public void RegisterFailedLoading()
        {
            FailedLoadingCount += 1;
            m_wasLastPreparationFailed = true;
        }
        
        public void ResetFailedLoading()
        {
            FailedLoadingCount = 0;
            m_wasLastPreparationFailed = false;
        }
    }
}
