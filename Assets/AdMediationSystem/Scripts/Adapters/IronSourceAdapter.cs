//#define _AMS_IRONSOURCE

using System;
using UnityEngine;
using System.Collections.Generic;
using Boomlagoon.JSON;
using System.Linq;
using System.Collections;
using UnityEngine.PlayerLoop;

namespace Virterix.AdMediation
{
    public class IronSourceAdapter : AdNetworkAdapter
    {
        [Serializable]
        public struct OverriddenPlacement
        {
            public OverriddenPlacement(AdType adType, string originPlacement, string targetPlacement)
            {
                AdvertisingType = adType;
                OriginPlacement = originPlacement;
                TargetPlacement = targetPlacement;
            }
            
            public AdType AdvertisingType;
            public string OriginPlacement;
            public string TargetPlacement;
        }

        public enum IrnSrcBannerSize
        {
            Banner,
            Large,
            Rectangle,
            Smart
        }

        public enum IrnSrcBannerPosition
        {
            Top,
            Bottom
        }

        public int m_timeout = 120;
        public bool m_validateIntegration;
        public bool m_useOfferwall;
        [SerializeField]
        public OverriddenPlacement[] m_overriddenPlacements;
        
        public string UserId { get; set; }
        
        private AdInstance m_interstitialInstance;
        private AdInstance m_incentivizedInstance;
  
        private AdInstance m_currBannerInstance;
        private bool m_bannerDisplayed;
        private AdState m_bannerState;

        public override bool UseSingleBannerInstance => true;

        public static string GetSDKVersion()
        {
            string version = string.Empty;
#if UNITY_EDITOR && _AMS_IRONSOURCE
            version = IronSource.pluginVersion();
#endif
            return version;
        }

#if _AMS_IRONSOURCE
        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeEvents();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnsubscribeEvents();
        }

        private void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }

        public static IronSourceBannerSize ConvertToNativeBannerSize(IrnSrcBannerSize bannerSize)
        {
            IronSourceBannerSize nativeAdSize = IronSourceBannerSize.SMART;
            switch (bannerSize)
            {
                case IrnSrcBannerSize.Banner:
                    nativeAdSize = IronSourceBannerSize.BANNER;
                    break;
                case IrnSrcBannerSize.Large:
                    nativeAdSize = IronSourceBannerSize.LARGE;
                    break;
                case IrnSrcBannerSize.Rectangle:
                    nativeAdSize = IronSourceBannerSize.RECTANGLE;
                    break;
                case IrnSrcBannerSize.Smart:
                    nativeAdSize = IronSourceBannerSize.SMART;
                    break;
            }
            return nativeAdSize;
        }

        public static IronSourceBannerPosition ConvertToNativeBannerPosition(IrnSrcBannerPosition bannerPosition)
        {
            IronSourceBannerPosition nativeAdPosition = IronSourceBannerPosition.BOTTOM;
            switch (bannerPosition)
            {
                case IrnSrcBannerPosition.Bottom:
                    nativeAdPosition = IronSourceBannerPosition.BOTTOM;
                    break;
                case IrnSrcBannerPosition.Top:
                    nativeAdPosition = IronSourceBannerPosition.TOP;
                    break;
            }
            return nativeAdPosition;
        }

        private static IronSourceBannerSize GetBannerSize(AdInstance adInstance)
        {
            var bannerParameters = adInstance.m_adInstanceParams as IronSourceAdInstanceBannerParameters;
            var nativeAdSize = ConvertToNativeBannerSize(bannerParameters.m_bannerSize);
            return nativeAdSize;
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

        protected override string AdInstanceParametersFolder
        {
            get { return IronSourceAdInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER; }
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
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

            if (m_validateIntegration)
                IronSource.Agent.validateIntegration();
            
            if (!string.IsNullOrEmpty(UserId))
                IronSource.Agent.setUserId(UserId);
            
            SetUserConsentToPersonalizedAds(AdMediationSystem.UserPersonalisationConsent);
            if (AdMediationSystem.Instance.ChildrenMode != ChildDirectedMode.NotAssign)
                SetChildDirected(AdMediationSystem.Instance.ChildrenMode);

            if (m_useOfferwall)
                IronSource.Agent.init(appKey, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.BANNER, IronSourceAdUnits.OFFERWALL);
            else
                IronSource.Agent.init(appKey, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.BANNER);
        }
        
        public override void Prepare(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            switch (adInstance.m_adType)
            {
                case AdType.Banner:
                    if (m_bannerState != AdState.Loading)
                    {
                        RequestBanner(adInstance, placement);
                    }
                    break;
                case AdType.Interstitial:
                    IronSource.Agent.loadInterstitial();
                    break;
                case AdType.Incentivized:
                    break;
            }
        }

        private void RequestBanner(AdInstance adInstance, string placement)
        {
            float requestDelay = 0.0f;
            if (m_bannerState == AdState.Received)
            {
                IronSource.Agent.destroyBanner();
                requestDelay = 0.5f;
            }
            m_bannerState = AdState.Loading;
            StartCoroutine(DeferredRequestBanner(adInstance, placement, requestDelay));
        }

        public override bool Show(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            bool isPreviousBannerDisplayed = m_bannerDisplayed;

            if (adInstance.m_adType == AdType.Banner)
            {
                adInstance.m_bannerDisplayed = true;
                m_bannerDisplayed = true;
                adInstance.CurrPlacement = placement;
            }

            if (IsReady(adInstance))
            {
                string actualPlacementName = GetOverridenPlacement(adInstance.m_adType, placement);
                switch (adInstance.m_adType)
                {
                    case AdType.Banner:
                        bool isPreparationRequired = false;
                        if (m_currBannerInstance != null)
                        {
                            isPreparationRequired = !CompareBannerPosition(m_currBannerInstance, m_currBannerPlacement, adInstance, placement);
                            if (!isPreparationRequired)
                            {
                                var currBannerParams = m_currBannerInstance.m_adInstanceParams as IronSourceAdInstanceBannerParameters;
                                var nextBannerParams = adInstance.m_adInstanceParams as IronSourceAdInstanceBannerParameters;
                                isPreparationRequired = currBannerParams.m_bannerSize != nextBannerParams.m_bannerSize;
                            }
                        }

                        if (isPreparationRequired)
                            RequestBanner(adInstance, placement);
                        else
                            IronSource.Agent.displayBanner();

                        if (!isPreviousBannerDisplayed)
                            AddEvent(adInstance.m_adType, AdEvent.Show, adInstance);
                        break;
                    case AdType.Interstitial:
                        IronSource.Agent.showInterstitial(actualPlacementName);
                        break;
                    case AdType.Incentivized:
                        IronSource.Agent.showRewardedVideo(actualPlacementName);
                        break;
                }              
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Hide(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (adInstance.m_adType == AdType.Banner)
            {         
                IronSource.Agent.hideBanner();
                if (m_bannerDisplayed)
                    NotifyEvent(AdEvent.Hiding, adInstance);
                adInstance.m_bannerDisplayed = false;
                m_bannerDisplayed = false;
            }
        }

        public override bool IsReady(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
#if UNITY_EDITOR
            return false;
#endif
            bool isReady = false;

            switch (adInstance.m_adType)
            {
                case AdType.Banner:
                    isReady = m_bannerState == AdState.Received;
                    break;
                case AdType.Interstitial:
                    isReady = IronSource.Agent.isInterstitialReady();
                    break;
                case AdType.Incentivized:
                    isReady = IronSource.Agent.isRewardedVideoAvailable();
                    break;
            }
            return isReady;
        }

        private IEnumerator DeferredRequestBanner(AdInstance adInstance, string placement, float delay)
        {
            adInstance.CurrPlacement = placement;
            m_currBannerInstance = adInstance;
            m_currBannerPlacement = placement;
            yield return new WaitForSecondsRealtime(delay);
            IronSourceBannerPosition bannerPos = ConvertToNativeBannerPosition((IrnSrcBannerPosition)GetBannerPosition(adInstance, placement));
            IronSourceBannerSize bannerSize = GetBannerSize(adInstance);         
            IronSource.Agent.loadBanner(bannerSize, bannerPos, GetOverridenPlacement(adInstance.m_adType, placement));
            yield break;
        }

        protected override void SetUserConsentToPersonalizedAds(PersonalisationConsent consent)
        {
            if (consent != PersonalisationConsent.Undefined)
            {
                IronSource.Agent.setConsent(consent == PersonalisationConsent.Accepted);
                IronSource.Agent.setMetaData("do_not_sell", consent == PersonalisationConsent.Accepted ? "false" : "true");
            }
        }

        private static void SetChildDirected(ChildDirectedMode mode)
        {
            string childDirectedMetaValue = mode == ChildDirectedMode.Directed ? "true" : "false";
            IronSource.Agent.setMetaData("is_deviceid_optout", childDirectedMetaValue);
            IronSource.Agent.setMetaData("is_child_directed", childDirectedMetaValue);
        }
        
        public override void NotifyEvent(AdEvent adEvent, AdInstance adInstance)
        {
            if (adInstance.m_adType == AdType.Banner && adEvent == AdEvent.PreparationFailed)
                m_bannerState = AdState.Unavailable;
            base.NotifyEvent(adEvent, adInstance);
        }

        private string GetOverridenPlacement(AdType adType, string placementName)
        {
            for (int i = 0; i < m_overriddenPlacements.Length; i++)
            {
                OverriddenPlacement overridenPlacement = m_overriddenPlacements[i];
                if (overridenPlacement.AdvertisingType == adType && overridenPlacement.OriginPlacement == placementName)
                {
                    placementName = overridenPlacement.TargetPlacement;
                    break;
                }
            }
            return placementName;
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
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.InterstitialAdShowSucceededEvent()");
#endif
        }

        void InterstitialAdShowFailedEvent(IronSourceError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.InterstitialAdShowFailedEvent()");
#endif
            m_interstitialInstance.State = AdState.Unavailable;
        }

        void InterstitialAdClickedEvent()
        {
            AddEvent(m_interstitialInstance.m_adType, AdEvent.Click, m_interstitialInstance);
        }

        void InterstitialAdOpenedEvent()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.InterstitialAdOpenedEvent()");
#endif
            AddEvent(m_interstitialInstance.m_adType, AdEvent.Show, m_interstitialInstance);
        }

        void InterstitialAdClosedEvent()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.InterstitialAdClosedEvent()");
#endif
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
            AddEvent(m_incentivizedInstance.m_adType, AdEvent.IncentivizationCompleted, m_incentivizedInstance);
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
        private void BannerAdLoadedEvent()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.BannerAdLoadedEvent()");
#endif
            
            m_bannerState = AdState.Received;
            if (m_bannerDisplayed)
                IronSource.Agent.displayBanner();
            else
                IronSource.Agent.hideBanner();
            AddEvent(AdType.Banner, AdEvent.Prepared, m_currBannerInstance);
        }

        private void BannerAdLoadFailedEvent(IronSourceError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.BannerAdLoadedEvent() code: " + error.getCode() + ", description: " + error.getDescription());
#endif
            m_bannerState = AdState.Unavailable;
            AddEvent(AdType.Banner, AdEvent.PreparationFailed, m_currBannerInstance);
        }

        private void BannerAdClickedEvent()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.BannerAdClickedEvent()");
#endif
            AddEvent(AdType.Banner, AdEvent.Click, m_currBannerInstance);
        }

        private void BannerAdScreenPresentedEvent()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.BannerAdScreenPresentedEvent()");
#endif
            AddEvent(AdType.Banner, AdEvent.Show, m_currBannerInstance);
        }

        private void BannerAdScreenDismissedEvent()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.BannerAdScreenDismissedEvent()");
#endif
        }

        private void BannerAdLeftApplicationEvent()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.BannerAdLeftApplicationEvent()");
#endif
        }

        #endregion // Banner callback handlers

        //------------------------------------------------------------------------
        #region ImpressionSuccess callback handler

        void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] IronSourceAdapter.ImpressionSuccessEvent()");
            Debug.Log("[AMS] unity - script: I got ImpressionSuccessEvent ToString(): " + impressionData.ToString());
            Debug.Log("[AMS] unity - script: I got ImpressionSuccessEvent allData: " + impressionData.allData);
#endif
        }

        #endregion
#endif 
    }
}
