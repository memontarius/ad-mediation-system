
using UnityEngine;
using System.Collections;
using System;

namespace Virterix.AdMediation
{
    /// <summary>
    /// Defines advertising unit
    /// </summary>
    public class AdUnit
    {
        private string m_placementName;
        private AdType m_adapterAdType;
        private AdNetworkAdapter m_network;
        private IFetchStrategyParams m_fetchStrategyParams;
        private float m_startImpressionTime;
        private bool m_isShown;

        public AdType AdapterAdType { get { return m_adapterAdType; } }
        public AdNetworkAdapter AdNetwork { get { return m_network; } }
        public int Index { get; set; }
        public string PlacementName
        {
            get { return m_placementName; }
        }

        public bool? IsPepareWhenChangeNetwork
        {
            get { return m_prepareWhenChangeNetwork; }
        }
        private bool? m_prepareWhenChangeNetwork;

        public string AdInstanceName
        {
            get { return m_adInstanceName; }
        }
        private string m_adInstanceName;

        public IFetchStrategyParams FetchStrategyParams
        {
            get { return m_fetchStrategyParams; }
        }

        public AdInstanceData AdInstance
        {
            get
            {
                if (m_adInstance == null && !m_isAdInstanceSetted)
                {
                    m_adInstance = AdNetwork.GetAdInstance(AdapterAdType, AdInstanceName);
                    m_isAdInstanceSetted = true;
                }
                return m_adInstance;
            }
        }
        AdInstanceData m_adInstance;
        bool m_isAdInstanceSetted;

        public bool IsAdReady
        {
            get
            {
                return IsTimeout ? false : m_network.IsReady(m_adapterAdType, AdInstance);
            }
        }

        public int Impressions
        {
            get { return m_impressions; }
            set { m_impressions = value; }
        }
        int m_impressions;

        public int FetchCount
        {
            get { return m_fetchCount; }
        }
        int m_fetchCount;

        public bool WasLastImpressionSuccessful
        {
            get 
            {
                return m_wasLastImpressionSuccessful; 
            }
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
                return AdNetwork.IsTimeout(AdapterAdType, AdInstance);
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
        
        public AdUnit(string placementName, string adInstanceName, AdType adType, AdNetworkAdapter network,
           IFetchStrategyParams strategyParams, bool? isPepareWhenChangeNetwork)
        {
            m_adapterAdType = adType;
            m_network = network;
            m_fetchStrategyParams = strategyParams;
            m_placementName = placementName;
            m_prepareWhenChangeNetwork = isPepareWhenChangeNetwork;
            m_adInstanceName = adInstanceName;
        }

        /// <returns>True when successfully shown ad</returns>
        public bool ShowAd()
        {
            bool showed = m_network.Show(m_adapterAdType, AdInstance, PlacementName);
            Impressions = showed ? Impressions + 1 : Impressions;
            m_wasLastImpressionSuccessful = showed;

            if (m_adapterAdType == AdType.Banner)
            {
                if (showed)
                {
                    m_isShown = true;
                    StartImpressionTime = Time.unscaledTime;
                }
            }
            return showed;
        }

        public void HideAd()
        {
            UpdateDisplayTimeWhenAdHidden();
            m_network.Hide(m_adapterAdType, AdInstance);
        }

        public void HideBannerTypeAdWithoutNotify()
        {
            UpdateDisplayTimeWhenAdHidden();
            m_network.HideBannerTypeAdWithoutNotify(m_adapterAdType, AdInstance);
        }

        public void PrepareAd()
        {
            m_network.Prepare(m_adapterAdType, AdInstance, PlacementName);
        }

        public void ResetAd()
        {
            m_network.ResetAd(m_adapterAdType, AdInstance);
        }

        public void ResetLastImpressionSuccessfulState()
        {
            m_wasLastImpressionSuccessful = false;
        }

        private void UpdateDisplayTimeWhenAdHidden()
        {
            if (m_adapterAdType == AdType.Banner)
            {
                if (m_isShown)
                {
                    m_isShown = false;
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