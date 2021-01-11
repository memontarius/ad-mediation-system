
#define _MS_CHARTBOOST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.JSON;

#if _MS_CHARTBOOST
using ChartboostSDK;
#endif

namespace Virterix.AdMediation
{
    public class ChartboostAdapter : AdNetworkAdapter
    {

#if _MS_CHARTBOOST

        private new void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private new void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnApplicationPause(bool pause)
        {
        }

        private void SubscribeEvents()
        {
            // Interstitial
            Chartboost.didCacheInterstitial += DidCacheInterstitial;
            Chartboost.didFailToLoadInterstitial += DidFailToLoadInterstitial;
            Chartboost.shouldDisplayInterstitial += ShouldDisplayInterstitial;
            Chartboost.didCloseInterstitial += DidCloseInterstitial;
            Chartboost.didDismissInterstitial += DidDismissInterstitial;
            // RewardedVideo
            Chartboost.didCacheRewardedVideo += DidCacheRewardedVideo;
            Chartboost.didFailToLoadRewardedVideo += DidFailToLoadRewardedVideo;
            Chartboost.shouldDisplayRewardedVideo += shouldDisplayRewardedVideo;
            Chartboost.didCloseRewardedVideo += DidCloseRewardedVideo;
            Chartboost.didDismissRewardedVideo += DidDismissRewardedVideo;
            Chartboost.didCompleteRewardedVideo += DidCompleteRewardedVideo;
        }

        private void UnsubscribeEvents()
        {
            // Interstitial
            Chartboost.didCacheInterstitial -= DidCacheInterstitial;
            Chartboost.didFailToLoadInterstitial -= DidFailToLoadInterstitial;
            Chartboost.shouldDisplayInterstitial -= ShouldDisplayInterstitial;
            Chartboost.didCloseInterstitial -= DidCloseInterstitial;
            Chartboost.didDismissInterstitial -= DidDismissInterstitial;
            // RewardedVideo
            Chartboost.didCacheRewardedVideo -= DidCacheRewardedVideo;
            Chartboost.didFailToLoadRewardedVideo -= DidFailToLoadRewardedVideo;
            Chartboost.shouldDisplayRewardedVideo -= shouldDisplayRewardedVideo;
            Chartboost.didCloseRewardedVideo -= DidCloseRewardedVideo;
            Chartboost.didDismissRewardedVideo -= DidDismissRewardedVideo;
            Chartboost.didCompleteRewardedVideo -= DidCompleteRewardedVideo;
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);

            bool autocache = false;
            string autocacheStr = "";
            string appId = "";
            string appSignature = "";

            if (!parameters.TryGetValue("autocache", out autocacheStr))
            {
                autocacheStr = "";
            }
            autocache = autocacheStr == "true";

            if (!parameters.TryGetValue("appId", out appId))
            {
                appId = "";
            }
            if (!parameters.TryGetValue("appSignature", out appSignature))
            {
                appSignature = "";
            }

            if (appId != null && appSignature != null)
            {
                Chartboost.CreateWithAppId(appId, appSignature);
            }
            Chartboost.setAutoCacheAds(autocache);
        }

        public override void Prepare(AdType adType, AdInstanceData adInstance = null)
        {
            switch (adType)
            {
                case AdType.Interstitial:
                    Chartboost.cacheInterstitial(CBLocation.Default);
                    break;
                case AdType.Incentivized:
                    Chartboost.cacheRewardedVideo(CBLocation.Default);
                    break;
            }
        }

        public override bool Show(AdType adType, AdInstanceData adInstance = null)
        {
            if (IsReady(adType))
            {
                switch (adType)
                {
                    case AdType.Interstitial:
                        Chartboost.showInterstitial(CBLocation.Default);
                        break;
                    case AdType.Incentivized:
                        Chartboost.showRewardedVideo(CBLocation.Default);
                        break;
                }
                return true;
            }
            return false;
        }

        public override void Hide(AdType adType, AdInstanceData adInstance = null)
        {

        }

        public override bool IsReady(AdType adType, AdInstanceData adInstance = null)
        {
            bool isReady = false;
            switch (adType)
            {
                case AdType.Interstitial:
                    isReady = Chartboost.hasInterstitial(CBLocation.Default);
                    break;
                case AdType.Incentivized:
                    isReady = Chartboost.hasRewardedVideo(CBLocation.Default);
                    break;
            }
            return isReady;
        }

        public override void SetPersonalizedAds(bool isPersonalizedAds)
        {
            Chartboost.addDataUseConsent(isPersonalizedAds ? CBGDPRDataUseConsent.Behavioral : CBGDPRDataUseConsent.NoBehavioral);
            Chartboost.addDataUseConsent(isPersonalizedAds ? CBCCPADataUseConsent.OptInSale : CBCCPADataUseConsent.OptOutSale);
        }

        // Interstitial

        private void DidCacheInterstitial(CBLocation location)
        {
            AddEvent(AdType.Interstitial, AdEvent.Prepared);
        }

        private void DidFailToLoadInterstitial(CBLocation location, CBImpressionError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("ChartboostAdapter.DidFailToLoadInterstitial() error:" + error.ToString());
#endif
            AddEvent(AdType.Interstitial, AdEvent.PrepareFailure);
        }

        private bool ShouldDisplayInterstitial(CBLocation location)
        {
            AddEvent(AdType.Interstitial, AdEvent.Show);
            bool showInterstitial = true;
            return showInterstitial;
        }

        private void DidCloseInterstitial(CBLocation location)
        {
        }

        void DidDismissInterstitial(CBLocation location)
        {
            AddEvent(AdType.Interstitial, AdEvent.Hide);
        }

        // Reward Video

        private void DidCacheRewardedVideo(CBLocation location)
        {
            AddEvent(AdType.Incentivized, AdEvent.Prepared);
        }

        private void DidFailToLoadRewardedVideo(CBLocation location, CBImpressionError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("ChartboostAdapter.DidFailToLoadRewardedVideo() error:" + error.ToString());
#endif
            AddEvent(AdType.Incentivized, AdEvent.PrepareFailure);
        }

        private bool shouldDisplayRewardedVideo(CBLocation location)
        {
            AddEvent(AdType.Incentivized, AdEvent.Show);
            bool showIncentivized = true;
            return showIncentivized;
        }

        private void DidCloseRewardedVideo(CBLocation location)
        {
        }

        private void DidCompleteRewardedVideo(CBLocation location, int count)
        {
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedComplete);
        }

        private void DidDismissRewardedVideo(CBLocation location)
        {
            AddEvent(AdType.Incentivized, AdEvent.Hide);
        }

#endif // _MS_CHARTBOOST
    }
} // namespace Virterix.AdMediation
