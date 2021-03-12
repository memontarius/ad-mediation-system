#define _AMS_IRONSOURCE

using UnityEngine;
using System.Collections.Generic;
using Boomlagoon.JSON;

namespace Virterix.AdMediation
{
    public class IronSourceAdapter : AdNetworkAdapter
    {
        public int m_timeout = 120;

        private AdInstance m_interstitialInstance;
        private AdInstance m_incentivizedInstance;

        public static string GetSDKVersion()
        {
            string version = string.Empty;
#if UNITY_EDITOR && _AMS_IRONSOURCE
            version = IronSource.pluginVersion();
#endif
            return version;
        }

#if _AMS_IRONSOURCE

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private new void OnDisable()
        {
            base.OnDisable();
            UnsubscribeEvents();
        }

        private void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }

        private void SubscribeEvents()
        {
            //Add Rewarded Video Events
            IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;

            // Add Interstitial Events
            IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
            IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
            IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
            IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;

            // Add Banner Events
            IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
            IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
            IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent;
            IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
            IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
            IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;

            //Add ImpressionSuccess Event
            IronSourceEvents.onImpressionSuccessEvent += ImpressionSuccessEvent;
        }

        private void UnsubscribeEvents()
        {
            //Add Rewarded Video Events
            IronSourceEvents.onRewardedVideoAdOpenedEvent -= RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent -= RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent -= RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent -= RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent -= RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent -= RewardedVideoAdShowFailedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent -= RewardedVideoAdClickedEvent;

            // Add Interstitial Events
            IronSourceEvents.onInterstitialAdReadyEvent -= InterstitialAdReadyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent -= InterstitialAdLoadFailedEvent;
            IronSourceEvents.onInterstitialAdShowSucceededEvent -= InterstitialAdShowSucceededEvent;
            IronSourceEvents.onInterstitialAdShowFailedEvent -= InterstitialAdShowFailedEvent;
            IronSourceEvents.onInterstitialAdClickedEvent -= InterstitialAdClickedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent -= InterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent -= InterstitialAdClosedEvent;

            // Add Banner Events
            IronSourceEvents.onBannerAdLoadedEvent -= BannerAdLoadedEvent;
            IronSourceEvents.onBannerAdLoadFailedEvent -= BannerAdLoadFailedEvent;
            IronSourceEvents.onBannerAdClickedEvent -= BannerAdClickedEvent;
            IronSourceEvents.onBannerAdScreenPresentedEvent -= BannerAdScreenPresentedEvent;
            IronSourceEvents.onBannerAdScreenDismissedEvent -= BannerAdScreenDismissedEvent;
            IronSourceEvents.onBannerAdLeftApplicationEvent -= BannerAdLeftApplicationEvent;

            //Add ImpressionSuccess Event
            IronSourceEvents.onImpressionSuccessEvent -= ImpressionSuccessEvent;
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances, bool isPersonalizedAds = true)
        {
            base.InitializeParameters(parameters, jsonAdInstances);

            string appKey = "";
            if (parameters != null)
            {
                if (!parameters.TryGetValue("appId", out appKey))
                {
                    appKey = "";
                }
            }

            m_interstitialInstance = AdFactory.CreateAdInstacne(this, AdType.Interstitial, AdInstance.AD_INSTANCE_DEFAULT_NAME, "", m_timeout);
            AddAdInstance(m_interstitialInstance);
            m_incentivizedInstance = AdFactory.CreateAdInstacne(this, AdType.Incentivized, AdInstance.AD_INSTANCE_DEFAULT_NAME, "", m_timeout);
            AddAdInstance(m_incentivizedInstance);

            SetPersonalizedAds(isPersonalizedAds);
            IronSource.Agent.init(appKey, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.BANNER);

            IronSource.Agent.validateIntegration();
        }

        public override void Prepare(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (!IsReady(adInstance))
            {
                switch (adInstance.m_adType)
                {
                    case AdType.Banner:
                        
                        break;
                    case AdType.Interstitial:
                        IronSource.Agent.loadInterstitial();
 
                        break;
                    case AdType.Incentivized:
                        
                        break;
                }
            }
        }

        public override bool Show(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (IsReady(adInstance))
            {
                switch (adInstance.m_adType)
                {
                    case AdType.Interstitial:
                        IronSource.Agent.showInterstitial();
                        break;
                    case AdType.Incentivized:
                        IronSource.Agent.showRewardedVideo();
                        break;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Hide(AdInstance adInstance = null)
        {
        }

        public override bool IsReady(AdInstance adInstance = null)
        {
            bool isReady = false;
            switch (adInstance.m_adType)
            {
                case AdType.Interstitial:
                    isReady = IronSource.Agent.isInterstitialReady();
                    break;
                case AdType.Incentivized:
                    isReady = IronSource.Agent.isRewardedVideoAvailable();
                    break;
            }
            return isReady;
        }

        protected override void SetPersonalizedAds(bool isPersonalizedAds)
        {
            IronSource.Agent.setConsent(isPersonalizedAds);
            IronSource.Agent.setMetaData("do_not_sell", isPersonalizedAds ? "false" : "true");
        }

        //------------------------------------------------------------------------
        #region Interstitial callback handlers

        void InterstitialAdReadyEvent()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.InterstitialAdReadyEvent()");
#endif
            m_interstitialInstance.State = AdState.Received;
            AddEvent(m_interstitialInstance.m_adType, AdEvent.Prepared, m_interstitialInstance);
        }

        void InterstitialAdLoadFailedEvent(IronSourceError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.InterstitialAdLoadFailedEvent() code:" + error.getErrorCode() + " desc:" + error.getDescription());
#endif
            m_interstitialInstance.State = AdState.Unavailable;
            AddEvent(m_interstitialInstance.m_adType, AdEvent.PreparationFailed, m_interstitialInstance);
        }

        void InterstitialAdShowSucceededEvent()
        {
            AddEvent(m_interstitialInstance.m_adType, AdEvent.Show, m_interstitialInstance);
        }

        void InterstitialAdShowFailedEvent(IronSourceError error)
        {
            m_interstitialInstance.State = AdState.Unavailable;
        }

        void InterstitialAdClickedEvent()
        {
            AddEvent(m_interstitialInstance.m_adType, AdEvent.Click, m_interstitialInstance);
        }

        void InterstitialAdOpenedEvent()
        {
        }

        void InterstitialAdClosedEvent()
        {
            AddEvent(m_interstitialInstance.m_adType, AdEvent.Hiding, m_interstitialInstance);
        }

        #endregion // Interstitial callback handlers

        //------------------------------------------------------------------------
        #region Rewarded Video callback handlers

        void RewardedVideoAvailabilityChangedEvent(bool canShowAd)
        {
            m_incentivizedInstance.State = AdState.Received;
            AddEvent(m_incentivizedInstance.m_adType, AdEvent.Prepared, m_incentivizedInstance);
        }

        void RewardedVideoAdOpenedEvent()
        {
            AddEvent(m_incentivizedInstance.m_adType, AdEvent.Show, m_incentivizedInstance);
        }

        void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp)
        {
            m_lastReward.label = ssp.getRewardName();
            m_lastReward.amount = ssp.getRewardAmount();
            AddEvent(m_incentivizedInstance.m_adType, AdEvent.IncentivizedCompleted, m_incentivizedInstance);
        }

        void RewardedVideoAdClosedEvent()
        {
            AddEvent(m_incentivizedInstance.m_adType, AdEvent.Hiding, m_incentivizedInstance);
        }

        void RewardedVideoAdStartedEvent()
        {
        }

        void RewardedVideoAdEndedEvent()
        {
        }

        void RewardedVideoAdShowFailedEvent(IronSourceError error)
        {
            m_incentivizedInstance.State = AdState.Unavailable;
        }

        void RewardedVideoAdClickedEvent(IronSourcePlacement ssp)
        {
            AddEvent(m_incentivizedInstance.m_adType, AdEvent.Click, m_incentivizedInstance);
        }

        #endregion // Rewarded Video callback handlers

        //------------------------------------------------------------------------
        #region Banner callback handlers

        void BannerAdLoadedEvent()
        {
            Debug.Log("unity-script: I got BannerAdLoadedEvent");
        }

        void BannerAdLoadFailedEvent(IronSourceError error)
        {
            Debug.Log("unity-script: I got BannerAdLoadFailedEvent, code: " + error.getCode() + ", description : " + error.getDescription());
        }

        void BannerAdClickedEvent()
        {
            Debug.Log("unity-script: I got BannerAdClickedEvent");
        }

        void BannerAdScreenPresentedEvent()
        {
            Debug.Log("unity-script: I got BannerAdScreenPresentedEvent");
        }

        void BannerAdScreenDismissedEvent()
        {
            Debug.Log("unity-script: I got BannerAdScreenDismissedEvent");
        }

        void BannerAdLeftApplicationEvent()
        {
            Debug.Log("unity-script: I got BannerAdLeftApplicationEvent");
        }


        #endregion // Banner callback handlers

        //------------------------------------------------------------------------
        #region ImpressionSuccess callback handler

        void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
        {
            Debug.Log("unity - script: I got ImpressionSuccessEvent ToString(): " + impressionData.ToString());
            Debug.Log("unity - script: I got ImpressionSuccessEvent allData: " + impressionData.allData);
        }

        #endregion

#endif // _AMS_IRONSOURCE

    }
} // namespace Virterix.AdMediation
