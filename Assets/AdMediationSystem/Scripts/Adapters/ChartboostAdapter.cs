#define _AMS_CHARTBOOST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.JSON;
#if _AMS_CHARTBOOST
using ChartboostSDK;
#endif

namespace Virterix.AdMediation
{
    public class ChartboostAdapter : AdNetworkAdapter
    {
        public static void SetupNetworkNativeSettings(string androidAppId, string androidAppSignatre, string iOSAppId, string iOSAppSignatre)
        {
#if UNITY_EDITOR && _AMS_CHARTBOOST
            CBSettings networkSettings = null;
            string[] assets = UnityEditor.AssetDatabase.FindAssets("t:CBSettings");
            if (assets.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[0]);
                networkSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<CBSettings>(path);
            }

            if (networkSettings != null)
            {
                networkSettings.SetAndroidPlatformIndex(0);
                networkSettings.SetAndroidAppId(androidAppId);
                networkSettings.SetAndroidAppSecret(androidAppSignatre);
                networkSettings.SetIOSAppId(iOSAppId);
                networkSettings.SetIOSAppSecret(iOSAppSignatre);
            }
            else
            {
                Debug.LogWarning("Chartboost Settings not found!");
            }
#endif
        }

#if _AMS_CHARTBOOST

        private AdInstanceData m_interstitialInstance;
        private AdInstanceData m_incentivizedInstance;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private new void OnDisable()
        {
            base.OnDisable();
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
            Chartboost.shouldDisplayRewardedVideo += ShouldDisplayRewardedVideo;
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
            Chartboost.shouldDisplayRewardedVideo -= ShouldDisplayRewardedVideo;
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

            m_interstitialInstance = AdFactory.CreateAdInstacne(AdType.Interstitial);
            AddAdInstance(m_interstitialInstance);
            m_incentivizedInstance= AdFactory.CreateAdInstacne(AdType.Incentivized);
            AddAdInstance(m_incentivizedInstance);

            if (appId != null && appSignature != null)
            {
                Chartboost.CreateWithAppId(appId, appSignature);
            }
            Chartboost.setAutoCacheAds(autocache);
        }

        public override void Prepare(AdInstanceData adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            switch (adInstance.m_adType)
            {
                case AdType.Interstitial:
                    Chartboost.cacheInterstitial(CBLocation.Default);
                    break;
                case AdType.Incentivized:
                    Chartboost.cacheRewardedVideo(CBLocation.Default);
                    break;
            }
        }

        public override bool Show(AdInstanceData adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (IsReady(adInstance))
            {
                switch (adInstance.m_adType)
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

        public override void Hide(AdInstanceData adInstance = null)
        {
        }

        public override bool IsReady(AdInstanceData adInstance = null)
        {
            bool isReady = false;
            switch (adInstance.m_adType)
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
            AddEvent(AdType.Interstitial, AdEvent.Prepared, m_interstitialInstance);
        }

        private void DidFailToLoadInterstitial(CBLocation location, CBImpressionError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("ChartboostAdapter.DidFailToLoadInterstitial() error:" + error.ToString());
#endif
            AddEvent(AdType.Interstitial, AdEvent.FailedPreparation, m_interstitialInstance);
        }

        private bool ShouldDisplayInterstitial(CBLocation location)
        {
            AddEvent(AdType.Interstitial, AdEvent.Show, m_interstitialInstance);
            bool showInterstitial = true;
            return showInterstitial;
        }

        private void DidCloseInterstitial(CBLocation location)
        {
            
        }

        void DidDismissInterstitial(CBLocation location)
        {
            AddEvent(AdType.Interstitial, AdEvent.Hiding, m_interstitialInstance);
        }

        // Reward Video
        private void DidCacheRewardedVideo(CBLocation location)
        {
            AddEvent(AdType.Incentivized, AdEvent.Prepared, m_incentivizedInstance);
        }

        private void DidFailToLoadRewardedVideo(CBLocation location, CBImpressionError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("ChartboostAdapter.DidFailToLoadRewardedVideo() error:" + error.ToString());
#endif
            AddEvent(AdType.Incentivized, AdEvent.FailedPreparation, m_incentivizedInstance);
        }

        private bool ShouldDisplayRewardedVideo(CBLocation location)
        {
            AddEvent(AdType.Incentivized, AdEvent.Show, m_incentivizedInstance);
            bool showIncentivized = true;
            return showIncentivized;
        }

        private void DidCloseRewardedVideo(CBLocation location)
        {
            
        }

        private void DidCompleteRewardedVideo(CBLocation location, int count)
        {
            m_lastReward.label = location.ToString();
            m_lastReward.amount = count;
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedCompleted, m_incentivizedInstance);
        }

        private void DidDismissRewardedVideo(CBLocation location)
        {
            AddEvent(AdType.Incentivized, AdEvent.Hiding, m_incentivizedInstance);
        }

#endif // _AMS_CHARTBOOST
    }
} // namespace Virterix.AdMediation
