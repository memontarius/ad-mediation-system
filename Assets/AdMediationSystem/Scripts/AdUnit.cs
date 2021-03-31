using UnityEngine;

namespace Virterix.AdMediation
{
    /// <summary>
    /// Defines advertising unit
    /// </summary>
    public class AdUnit
    {
        private AdType m_adType;
        private string m_placementName;
        private AdNetworkAdapter m_network;
        private BaseFetchStrategyParams m_fetchStrategyParams;
        private float m_startImpressionTime;
        private bool m_wasImpression;

        public AdType AdType 
        { 
            get { return m_adType; } 
        }

        public AdNetworkAdapter AdNetwork 
        { 
            get { return m_network; } 
        }

        public int Index { get; private set; }
        public int TierIndex { get; private set; }

        public string PlacementName
        {
            get { return m_placementName; }
        }

        public float NetworkResponseWaitTime
        {
            get { return AdInstance.m_responseWaitTime; }
        }
        
        public bool IsPrepareOnExit
        {
            get { return m_isPrepareOnExit; }
        }
        private bool m_isPrepareOnExit;

        public string AdInstanceName
        {
            get { return m_adInstanceName; }
        }
        private string m_adInstanceName;

        public BaseFetchStrategyParams FetchStrategyParams
        {
            get { return m_fetchStrategyParams; }
        }

        public AdInstance AdInstance
        {
            get
            {
                if (m_adInstance == null && !m_isAdInstanceSetted)
                {
                    m_adInstance = AdNetwork.GetAdInstance(AdType, AdInstanceName);
                    m_isAdInstanceSetted = true;
                }
                return m_adInstance;
            }
        }
        private AdInstance m_adInstance;
        private bool m_isAdInstanceSetted;

        public bool IsReady
        {
            get
            {
                return IsTimeout ? false : m_network.IsReady(AdInstance);
            }
        }

        public int Impressions
        {
            get { return m_impressions; }
            set { m_impressions = value; }
        }
        int m_impressions;

        public bool WasLastImpressionSuccessful
        {
            get {  return m_wasLastImpressionSuccessful; }
        }
        bool m_wasLastImpressionSuccessful;

        public float DisplayTime
        {
            get
            {
                return AdInstance == null ? m_displayTime : AdInstance.m_displayTime;
            }
            private set
            {
                if (AdInstance == null)
                {
                    m_displayTime = value;
                }
                else
                {
                    AdInstance.m_displayTime = value;
                }
            }
        }
        float m_displayTime;

        public bool IsTimeout
        {
            get
            {
                bool isTimeout = AdInstance.m_timeout != null ? AdInstance.m_timeout.Value.IsTimeout : false;
                return isTimeout;
            }
        }

        private float StartImpressionTime 
        {
            get 
            {
                return AdInstance == null ? m_startImpressionTime : AdInstance.m_startImpressionTime;
            }
            set
            {
                if (AdInstance == null)
                {
                    m_startImpressionTime = value;
                }
                else
                {
                    AdInstance.m_startImpressionTime = value;
                }
            }
        }
        
        public AdUnit(string placementName, AdType adType, string adInstanceName, AdNetworkAdapter network,
           BaseFetchStrategyParams strategyParams, int tierIndex, int unitIndex, bool isPrepareOnExit = false)
        {
            m_adType = adType;
            m_network = network;
            m_fetchStrategyParams = strategyParams;
            m_placementName = placementName;
            m_adInstanceName = adInstanceName;
            m_isPrepareOnExit = isPrepareOnExit;
            TierIndex = tierIndex;
            Index = unitIndex;          
        }

        /// <returns>True when successfully shown ad</returns>
        public bool Show()
        {
            bool showed = m_network.Show(AdInstance, PlacementName);
            Impressions = showed ? Impressions + 1 : Impressions;
            m_wasLastImpressionSuccessful = showed;

            if (AdType == AdType.Banner)
            {
                if (showed)
                {
                    m_wasImpression = true;
                    StartImpressionTime = Time.unscaledTime;
                }
            }
            return showed;
        }

        public void Hide()
        {
            UpdateDisplayTimeWhenAdHidden();
            m_network.Hide(AdInstance, PlacementName);
        }

        public void Prepare()
        {
            m_network.Prepare(AdInstance, PlacementName);
        }

        public void ResetLastImpressionSuccessfulState()
        {
            m_wasLastImpressionSuccessful = false;
        }

        private void UpdateDisplayTimeWhenAdHidden()
        {
            if (AdType == AdType.Banner)
            {
                if (m_wasImpression)
                {
                    m_wasImpression = false;
                    float passedTime = Time.unscaledTime - StartImpressionTime;
                    DisplayTime += passedTime;
                }
            }
        }

        public void ResetDisplayTime()
        {
            DisplayTime = 0.0f;
        }
    }
} // namespace Virterix.AdMediation