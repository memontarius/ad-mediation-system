//#define _AMS_CHARTBOOST

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
        public int m_timeout;

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

        private AdInstance m_interstitialInstance;
        private AdInstance m_incentivizedInstance;

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

            m_interstitialInstance = AdFactory.CreateAdInstacne(this, AdType.Interstitial, AdInstance.AD_INSTANCE_DEFAULT_NAME, "", m_timeout);
            AddAdInstance(m_interstitialInstance);
            m_incentivizedInstance= AdFactory.CreateAdInstacne(this, AdType.Incentivized, AdInstance.AD_INSTANCE_DEFAULT_NAME, "", m_timeout);
            AddAdInstance(m_incentivizedInstance);

            if (appId != null && appSignature != null)
            {
                Chartboost.CreateWithAppId(appId, appSignature);
            }
            Chartboost.setAutoCacheAds(autocache);
            SetUserConsentToPersonalizedAds(AdMediationSystem.UserPersonalisationConsent);
        }

        public override void Prepare(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (!IsReady(adInstance))
            {
                adInstance.State = AdState.Loading;
                switch (adInstance.m_adType)
                {
                    case AdType.Interstitial:
                        Chartboost.cacheInterstitial(CBLocation.locationFromName(placement));
                        break;
                    case AdType.Incentivized:
                        Chartboost.cacheRewardedVideo(CBLocation.locationFromName(placement));
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
                        Chartboost.showInterstitial(CBLocation.locationFromName(placement));
                        break;
                    case AdType.Incentivized:
                        Chartboost.showRewardedVideo(CBLocation.locationFromName(placement));
                        break;
                }
                return true;
            }
            return false;
        }

        public override void Hide(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
        }

        public override bool IsReady(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            bool isReady = false;
            switch (adInstance.m_adType)
            {
                case AdType.Interstitial:
                    isReady = Chartboost.hasInterstitial(CBLocation.locationFromName(placement));
                    break;
                case AdType.Incentivized:
                    isReady = Chartboost.hasRewardedVideo(CBLocation.locationFromName(placement));
                    break;
            }
            return isReady;
        }

        protected override void SetUserConsentToPersonalizedAds(PersonalisationConsent consent)
        {
            if (consent != PersonalisationConsent.Undefined)
            {
                bool accepted = consent == PersonalisationConsent.Accepted;
                Chartboost.addDataUseConsent(accepted ? CBCCPADataUseConsent.OptInSale : CBCCPADataUseConsent.OptOutSale);
                Chartboost.addDataUseConsent(accepted ? CBGDPRDataUseConsent.Behavioral : CBGDPRDataUseConsent.NoBehavioral);
            }
        }

        // Interstitial
        private void DidCacheInterstitial(CBLocation location)
        {
            m_interstitialInstance.State = AdState.Received;
            AddEvent(AdType.Interstitial, AdEvent.Prepared, m_interstitialInstance);
        }

        private void DidFailToLoadInterstitial(CBLocation location, CBImpressionError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] ChartboostAdapter.DidFailToLoadInterstitial() error:" + error.ToString());
#endif
            m_interstitialInstance.State = AdState.Unavailable;
            AddEvent(AdType.Interstitial, AdEvent.PreparationFailed, m_interstitialInstance);
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
            m_incentivizedInstance.State = AdState.Received;
            AddEvent(AdType.Incentivized, AdEvent.Prepared, m_incentivizedInstance);
        }

        private void DidFailToLoadRewardedVideo(CBLocation location, CBImpressionError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] ChartboostAdapter.DidFailToLoadRewardedVideo() error:" + error.ToString());
#endif
            m_incentivizedInstance.State = AdState.Unavailable;
            AddEvent(AdType.Incentivized, AdEvent.PreparationFailed, m_incentivizedInstance);
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
