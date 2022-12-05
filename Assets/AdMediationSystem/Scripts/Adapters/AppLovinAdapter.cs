//#define _AMS_APPLOVIN

using System.Collections.Generic;
using UnityEngine;
using Boomlagoon.JSON;

namespace Virterix.AdMediation
{
    public class AppLovinAdapter : AdNetworkAdapter
    {
        public enum AppLovinBannerPosition
        {
            TopLeft,
            TopCenter,
            TopRight,
            Centered,
            CenterLeft,
            CenterRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        protected override string AdInstanceParametersFolder
        {
            get { return AppLovinAdInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER; }
        }

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
                UnityEditor.EditorUtility.SetDirty(networkSettings);
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
        public static MaxSdk.BannerPosition ConvertToNativeBanerPosition(AppLovinBannerPosition position)
        {
            MaxSdk.BannerPosition nativePosition = MaxSdkBase.BannerPosition.BottomCenter;
            switch (position)
            {
                case AppLovinBannerPosition.BottomCenter:
                    nativePosition = MaxSdk.BannerPosition.BottomCenter;
                    break;
                case AppLovinBannerPosition.BottomLeft:
                    nativePosition = MaxSdk.BannerPosition.BottomLeft;
                    break;
                case AppLovinBannerPosition.BottomRight:
                    nativePosition = MaxSdk.BannerPosition.BottomRight;
                    break;
                case AppLovinBannerPosition.TopCenter:
                    nativePosition = MaxSdk.BannerPosition.TopCenter;
                    break;
                case AppLovinBannerPosition.TopLeft:
                    nativePosition = MaxSdk.BannerPosition.TopLeft;
                    break;
                case AppLovinBannerPosition.TopRight:
                    nativePosition = MaxSdk.BannerPosition.TopRight;
                    break;
                case AppLovinBannerPosition.Centered:
                    nativePosition = MaxSdk.BannerPosition.Centered;
                    break;
            }
            return nativePosition;
        }

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
            if (AdMediationSystem.Instance.IsTestModeEnabled)
                MaxSdk.SetTestDeviceAdvertisingIdentifiers(AdMediationSystem.Instance.TestDevices);

            if (AdMediationSystem.Instance.ChildrenMode != ChildDirectedMode.NotAssign)
                MaxSdk.SetIsAgeRestrictedUser(AdMediationSystem.Instance.ChildrenMode == ChildDirectedMode.Directed);
            
            MaxSdk.SetSdkKey(sdkKey);
            SetUserConsentToPersonalizedAds(AdMediationSystem.UserPersonalisationConsent);
            MaxSdk.InitializeSdk();
#endif
        }

        protected override AdInstance CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            AdInstance adInstance = new AdInstance(this);
            return adInstance;
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitializedEvent;

            MaxSdkCallbacks.OnBannerAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.OnBannerAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.OnBannerAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.OnBannerAdCollapsedEvent += OnBannerAdCollapsedEvent;
            MaxSdkCallbacks.OnBannerAdExpandedEvent += OnBannerAdExpandedEvent;

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

            MaxSdkCallbacks.OnBannerAdClickedEvent -= OnBannerAdClickedEvent;
            MaxSdkCallbacks.OnBannerAdLoadedEvent -= OnBannerAdLoadedEvent;
            MaxSdkCallbacks.OnBannerAdLoadFailedEvent -= OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.OnBannerAdCollapsedEvent -= OnBannerAdCollapsedEvent;
            MaxSdkCallbacks.OnBannerAdExpandedEvent -= OnBannerAdExpandedEvent;

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

        public override bool Show(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            bool success = false;
            bool isPrevousBannerDisplayed = adInstance.m_bannerDisplayed;

            if (adInstance.m_adType == AdType.Banner)
            {
                adInstance.m_bannerDisplayed = true;
                adInstance.CurrPlacement = placement;
            }
            
            if (IsReady(adInstance))
            {
                switch (adType)
                {
                    case AdType.Banner:
                        MaxSdk.ShowBanner(adInstance.m_adId);
                        if (!isPrevousBannerDisplayed)
                            AddEvent(adInstance.m_adType, AdEvent.Showing, adInstance);
                        break;
                    case AdType.Interstitial:
                        MaxSdk.ShowInterstitial(adInstance.m_adId, placement);
                        break;
                    case AdType.Incentivized:
                        MaxSdk.ShowRewardedAd(adInstance.m_adId, placement);
                        break;
                }
                success = true;
            }
            return success;
        }

        public override void Hide(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            switch (adType)
            {
                case AdType.Banner:
                    
                    MaxSdk.HideBanner(adInstance.m_adId);
                    if (adInstance.m_bannerDisplayed)
                        NotifyEvent(AdEvent.Hiding, adInstance);
                    adInstance.m_bannerDisplayed = false;
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
                        isReady = adInstance.State == AdState.Received;
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
                        RequestBanner(adInstance, placement);
                        break;
                }
            }
#endif
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

        private void DestroyBanner(AdInstance adInstance)
        {
            MaxSdk.DestroyBanner(adInstance.m_adId);
            adInstance.State = AdState.Unavailable;
        }

        private void RequestBanner(AdInstance adInstance, string placement)
        {
            if (adInstance.State == AdState.Loading || adInstance.State == AdState.Received)
                DestroyBanner(adInstance);
  
            adInstance.State = AdState.Loading;
            adInstance.CurrPlacement = placement;
            var bannerPosition = ConvertToNativeBanerPosition((AppLovinBannerPosition)GetBannerPosition(adInstance, placement));
            MaxSdk.CreateBanner(adInstance.m_adId, bannerPosition);
            MaxSdk.SetBannerPlacement(adInstance.m_adId, placement);
        }

        //_______________________________________________________________________________
        #region Banner Callbacks

        private void OnBannerAdClickedEvent(string adUnitIdentifier)
        {
            var adInstance = GetAdInstanceByAdId(adUnitIdentifier);
            AddEvent(adInstance.m_adType, AdEvent.Clicked, adInstance);
        }

        private void OnBannerAdLoadedEvent(string adUnitIdentifier)
        {
            var adInstance = GetAdInstanceByAdId(adUnitIdentifier);
            adInstance.State = AdState.Received;

            if (adInstance.m_adType == AdType.Banner)
            {
                if (adInstance.m_bannerDisplayed)
                    MaxSdk.ShowBanner(adInstance.m_adId);
                else
                    MaxSdk.HideBanner(adInstance.m_adId);
            }
            AddEvent(adInstance.m_adType, AdEvent.Prepared, adInstance);
        }

        private void OnBannerAdLoadFailedEvent(string adUnitIdentifier, int errorCOde)
        {
            var adInstance = GetAdInstanceByAdId(adUnitIdentifier);
            DestroyBanner(adInstance);
            AddEvent(adInstance.m_adType, AdEvent.PreparationFailed, adInstance);
        }

        private void OnBannerAdCollapsedEvent(string adUnitIdentifier)
        {
        }

        private void OnBannerAdExpandedEvent(string adUnitIdentifier)
        {
        }

        #endregion // Banner Callbacks

        //_______________________________________________________________________________
        #region Interstitial Callbacks

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
            AddEvent(AdType.Interstitial, AdEvent.Showing, adInstance);
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
            AddEvent(AdType.Interstitial, AdEvent.Clicked, adInstance);
        }

        private void OnInterstitialDismissedEvent(string adUnitId)
        {
            // Interstitial ad is hidden. Pre-load the next ad
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            adInstance.State = AdState.Unavailable;
            AddEvent(AdType.Interstitial, AdEvent.Hiding, adInstance);
        }

        #endregion // Interstitial Callbacks

        //_______________________________________________________________________________
        #region Rewarded Callbacks

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
            AddEvent(AdType.Incentivized, AdEvent.Showing, adInstance);
        }

        private void OnRewardedAdClickedEvent(string adUnitId)
        {
            AdInstance adInstance = GetAdInstanceByAdId(adUnitId);
            AddEvent(AdType.Incentivized, AdEvent.Clicked, adInstance);
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
            AddEvent(AdType.Incentivized, AdEvent.IncentivizationCompleted, adInstance);
        }
        #endregion // Rewarded Callbacks
#endif
    }
}

