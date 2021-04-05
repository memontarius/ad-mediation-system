#define _AMS_APPLOVIN

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boomlagoon.JSON;

namespace Virterix.AdMediation
{
    public class AppLovinAdapter : AdNetworkAdapter
    {
        public enum AppLovinBannerPosition
        {
            Center,
            Top,
            Bottom,
            Left,
            Right
        }

        public AppLovinBannerPosition m_bannerPlacementPosX;
        public AppLovinBannerPosition m_bannerPlacementPosY;

        public static void SetupNetworkNativeSettings(string sdkKey)
        {
#if UNITY_EDITOR && _AMS_APPLOVIN
            AppLovinSettings networkSettings = null;

            string[] assets = UnityEditor.AssetDatabase.FindAssets("t:AppLovinSettings");
            if (assets.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[0]);
                networkSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<AppLovinSettings>(path);
            }

            if (networkSettings != null)
            {
                networkSettings.QualityServiceEnabled = true;
                networkSettings.SdkKey = sdkKey;
            }
            else
            {
                Debug.LogWarning("[AMS] AppLovin Settings not found!");
            }
#endif
        }

        public static string GetSDKVersion()
        {
#if _AMS_APPLOVIN
            return MaxSdk.Version;
#else
            return string.Empty;
#endif
        }

#if _AMS_APPLOVIN
        private bool m_isBannerLoaded;

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonPlacements)
        {
            base.InitializeParameters(parameters, jsonPlacements);

            string sdkKey = "";

            if (parameters != null)
            {
                if (!parameters.TryGetValue("sdkKey", out sdkKey))
                {
                    sdkKey = "";
                }
            }

            SubscribeEvents();
#if UNITY_ANDROID || UNITY_IPHONE
            if (AdMediationSystem.Instance.m_testModeEnabled)
                MaxSdk.SetTestDeviceAdvertisingIdentifiers(AdMediationSystem.Instance.m_testDevices);
 
            if (AdMediationSystem.Instance.m_isChildrenDirected)
                MaxSdk.SetIsAgeRestrictedUser(AdMediationSystem.Instance.m_isChildrenDirected);

            MaxSdk.SetSdkKey(sdkKey);
            SetUserConsentToPersonalizedAds(AdMediationSystem.UserPersonalisationConsent);
            MaxSdk.InitializeSdk();
#endif
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitializedEvent;

            MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.OnInterstitialDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.OnInterstitialClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialDismissedEvent;

            MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.OnRewardedAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.OnRewardedAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        }

        private void UnsubscribeEvents()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent -= OnSdkInitializedEvent;

            MaxSdkCallbacks.OnInterstitialLoadedEvent -= OnInterstitialLoadedEvent;
            MaxSdkCallbacks.OnInterstitialLoadFailedEvent -= OnInterstitialFailedEvent;
            MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent -= InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.OnInterstitialDisplayedEvent -= OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.OnInterstitialClickedEvent -= OnInterstitialClickedEvent;
            MaxSdkCallbacks.OnInterstitialHiddenEvent -= OnInterstitialDismissedEvent;

            MaxSdkCallbacks.OnRewardedAdLoadedEvent -= OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.OnRewardedAdLoadFailedEvent -= OnRewardedAdFailedEvent;
            MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent -= OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.OnRewardedAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.OnRewardedAdClickedEvent -= OnRewardedAdClickedEvent;
            MaxSdkCallbacks.OnRewardedAdHiddenEvent -= OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
        }

        float ConvertBanerPosition(AppLovinBannerPosition placement)
        {
            float convertedPlacement = 0;

            switch (placement)
            {
                case AppLovinBannerPosition.Bottom:
                    //convertedPlacement = AppLovin.AD_POSITION_BOTTOM;
                    break;
                case AppLovinBannerPosition.Center:
                    //convertedPlacement = AppLovin.AD_POSITION_CENTER;
                    break;
                case AppLovinBannerPosition.Left:
                    //convertedPlacement = AppLovin.AD_POSITION_LEFT;
                    break;
                case AppLovinBannerPosition.Right:
                    //convertedPlacement = AppLovin.AD_POSITION_RIGHT;
                    break;
                case AppLovinBannerPosition.Top:
                    //convertedPlacement = AppLovin.AD_POSITION_TOP;
                    break;
            }

            return convertedPlacement;
        }

        public override void Hide(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            switch (adType)
            {
                case AdType.Banner:
                    break;
            }
        }

        public override bool IsReady(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            bool isReady = false;
            AdType adType = adInstance.m_adType;
#if UNITY_ANDROID || UNITY_IOS
            if (adInstance.State == AdState.Received)
            {
                switch (adType)
                {
                    case AdType.Interstitial:
                        isReady = MaxSdk.IsInterstitialReady(adInstance.m_adId);
                        break;
                    case AdType.Incentivized:
                        isReady = MaxSdk.IsRewardedAdReady(adInstance.m_adId);
                        break;
                    case AdType.Banner:
                        isReady = m_isBannerLoaded;
                        break;
                }
            }   
#endif
            return isReady;
        }

        public override void Prepare(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
#if UNITY_EDITOR
            return;
#endif
            AdType adType = adInstance.m_adType;
#if UNITY_ANDROID || UNITY_IOS
            if (!IsReady(adInstance))
            {
                adInstance.State = AdState.Loading;
                switch (adType)
                {
                    case AdType.Interstitial:
                        MaxSdk.LoadInterstitial(adInstance.m_adId);
                        break;
                    case AdType.Incentivized:
                        MaxSdk.LoadRewardedAd(adInstance.m_adId);
                        break;
                    case AdType.Banner:
                        break;
                }
            }
#endif
        }

        public override bool Show(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            bool success = false;
            if (IsReady(adInstance))
            {
                switch (adType)
                {
                    case AdType.Banner:
                        float posX = ConvertBanerPosition(m_bannerPlacementPosX);
                        float posY = ConvertBanerPosition(m_bannerPlacementPosY);
                        break;
                    case AdType.Interstitial:
                        MaxSdk.ShowInterstitial(adInstance.m_adId);
                        break;
                    case AdType.Incentivized:
                        MaxSdk.ShowRewardedAd(adInstance.m_adId);
                        break;
                }
                success = true;
            }
            return success;
        }

        protected override void SetUserConsentToPersonalizedAds(PersonalisationConsent consent)
        {
            if (consent != PersonalisationConsent.Undefined)
            {
#if UNITY_ANDROID || UNITY_IOS
                MaxSdk.SetHasUserConsent(consent == PersonalisationConsent.Accepted ? true : false);
#endif
            }
        }

        private void OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AppLovinAdapter OnSdkInitializedEvent()");
#endif
        }

        // _____________________________________________
        // INTERSTITIAL
        private void OnInterstitialLoadedEvent(string adUnitId)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(adUnitId) will now return 'true'
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            adInstance.State = AdState.Received;
            AddEvent(AdType.Interstitial, AdEvent.Prepared, adInstance);
        }

        private void OnInterstitialFailedEvent(string adUnitId, int errorCode)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AppLovinAdapter OnInterstitialFailedEvent() " + adUnitId + " errorCode:" + errorCode);
#endif
            // Interstitial ad failed to load 
            // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            adInstance.State = AdState.Unavailable;
            AddEvent(AdType.Interstitial, AdEvent.PreparationFailed, adInstance);
        }

        private void OnInterstitialDisplayedEvent(string adUnitId)
        {
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            AddEvent(AdType.Interstitial, AdEvent.Show, adInstance);
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, int errorCode)
        {
            // Interstitial ad failed to display. We recommend loading the next ad  
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            adInstance.State = AdState.Unavailable;
            AddEvent(AdType.Interstitial, AdEvent.Hiding, adInstance);
        }

        private void OnInterstitialClickedEvent(string adUnitId)
        {
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            AddEvent(AdType.Interstitial, AdEvent.Click, adInstance);
        }

        private void OnInterstitialDismissedEvent(string adUnitId)
        {
            // Interstitial ad is hidden. Pre-load the next ad
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            adInstance.State = AdState.Unavailable;
            AddEvent(AdType.Interstitial, AdEvent.Hiding, adInstance);
        }

        // _____________________________________________
        // REWARDED

        private void OnRewardedAdLoadedEvent(string adUnitId)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(adUnitId) will now return 'true'
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            adInstance.State = AdState.Received;
            AddEvent(AdType.Incentivized, AdEvent.Prepared, adInstance);
        }

        private void OnRewardedAdFailedEvent(string adUnitId, int errorCode)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AppLovinAdapter OnRewardedAdFailedEvent() " + adUnitId + " errorCode:" + errorCode);
#endif
            // Rewarded ad failed to load 
            // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            adInstance.State = AdState.Unavailable;
            AddEvent(AdType.Incentivized, AdEvent.PreparationFailed, adInstance);
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            adInstance.State = AdState.Unavailable;
            AddEvent(AdType.Incentivized, AdEvent.Hiding, adInstance);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId)
        {
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            AddEvent(AdType.Incentivized, AdEvent.Show, adInstance);
        }

        private void OnRewardedAdClickedEvent(string adUnitId)
        {
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            AddEvent(AdType.Incentivized, AdEvent.Click, adInstance);
        }

        private void OnRewardedAdDismissedEvent(string adUnitId)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            adInstance.State = AdState.Unavailable;
            AddEvent(AdType.Incentivized, AdEvent.Hiding, adInstance);
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward)
        {
            // Rewarded ad was displayed and user should receive the reward
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            m_lastReward.label = reward.Label;
            m_lastReward.amount = reward.Amount;
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedCompleted, adInstance);
        }

#endif // _AMS_APPLOVIN

    }
} // namespace Virterix.AdMediation

