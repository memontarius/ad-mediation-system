
#define _MS_APPLOVIN

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boomlagoon.JSON;
#if UNITY_ANDROID || UNITY_IPHONE
using AppLovinMax;
#endif

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

#if _MS_APPLOVIN

        private string m_rewardInfo;
        private bool m_isRewardRejected;
        private bool m_isBannerLoaded;

        private string _interstitialAdUnitId;
        private string _rewardAdUnitId;
        private string _bannerAdUnitId;

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

                parameters.TryGetValue("interstitialId", out _interstitialAdUnitId);
                parameters.TryGetValue("rewardVideoId", out _rewardAdUnitId);
                parameters.TryGetValue("bannerId", out _bannerAdUnitId);
            }

            SubscribeEvents();

#if UNITY_ANDROID || UNITY_IPHONE
            MaxSdk.SetSdkKey(sdkKey);
            //MaxSdk.SetUnityAdListener(this.name);
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

        public override void SetPersonalizedAds(bool isPersonalizedAds)
        {
#if UNITY_ANDROID || UNITY_IOS
            //AppLovin.SetHasUserConsent(isPersonalizedAds ? "true" : "false");
            MaxSdk.SetHasUserConsent(isPersonalizedAds ? true : false);
#endif
        }

        public override void Hide(AdType adType, AdInstanceData adInstance = null)
        {
            switch (adType)
            {
                case AdType.Banner:
                    //AppLovin.HideAd();
                    break;
            }
        }

        public override bool IsReady(AdType adType, AdInstanceData adInstance = null)
        {
            bool isReady = false;
#if UNITY_EDITOR
            return false;
#endif

#if UNITY_ANDROID || UNITY_IOS
            switch (adType)
            {
                case AdType.Interstitial:
                    //isReady = AppLovin.HasPreloadedInterstitial();
                    isReady = MaxSdk.IsInterstitialReady(_interstitialAdUnitId);
                    break;
                case AdType.Incentivized:
                    //isReady = AppLovin.IsIncentInterstitialReady();
                    isReady = MaxSdk.IsRewardedAdReady(_rewardAdUnitId);
                    break;
                case AdType.Banner:
                    isReady = m_isBannerLoaded;
                    break;
            }
#endif

            return isReady;
        }

        public override void Prepare(AdType adType, AdInstanceData adInstance = null, string placement = AdMediationSystem._PLACEMENT_DEFAULT_NAME)
        {
#if UNITY_ANDROID || UNITY_IOS
            if (!IsReady(adType))
            {
                switch (adType)
                {
                    case AdType.Interstitial:
                        //AppLovin.PreloadInterstitial();
                        MaxSdk.LoadInterstitial(_interstitialAdUnitId);
                        break;
                    case AdType.Incentivized:
                        //AppLovin.LoadRewardedInterstitial();
                        MaxSdk.LoadRewardedAd(_rewardAdUnitId);
                        break;
                    case AdType.Banner:
                        break;
                }
            }
#endif
        }

        public override bool Show(AdType adType, AdInstanceData adInstance = null, string placement = AdMediationSystem._PLACEMENT_DEFAULT_NAME)
        {
            bool success = false;
            if (IsReady(adType))
            {
                switch (adType)
                {
                    case AdType.Banner:
                        float posX = ConvertBanerPosition(m_bannerPlacementPosX);
                        float posY = ConvertBanerPosition(m_bannerPlacementPosY);
                        //AppLovin.ShowAd(posX, posY);
                        break;
                    case AdType.Interstitial:
                        //AppLovin.ShowInterstitial();
                        MaxSdk.ShowInterstitial(_interstitialAdUnitId);
                        break;
                    case AdType.Incentivized:
                        //AppLovin.ShowRewardedInterstitial();
                        MaxSdk.ShowRewardedAd(_rewardAdUnitId);
                        break;
                }
                success = true;
            }
            return success;
        }

        /*
        void onAppLovinEventReceived(string evnt) {

            // ----- INTERSTITIAL
            if (evnt.Contains("DISPLAYEDINTER")) {
                // An ad was shown.  Pause the game.
                AddEvent(AdType.Interstitial, AdEvent.Show);
            }
            else if (evnt.Contains("HIDDENINTER")) {
                // Ad ad was closed.  Resume the game.
                // If you're using PreloadInterstitial/HasPreloadedInterstitial, make a preload call here.
                AddEvent(AdType.Interstitial, AdEvent.Hide);
            }
            else if (evnt.Contains("LOADEDINTER")) {
                // An interstitial ad was successfully loaded.
                AddEvent(AdType.Interstitial, AdEvent.Prepared);
            }
            else if (string.Equals(evnt, "LOADINTERFAILED")) {
                // An interstitial ad failed to load.
                AddEvent(AdType.Interstitial, AdEvent.PrepareFailure);
            }
            // ----- REWARD VIDEO
            else if (evnt.Contains("DISPLAYEDREWARDED")) {
                m_isRewardRejected = false;
                AddEvent(AdType.Incentivized, AdEvent.Show);
            }
            else if (evnt.Contains("HIDDENREWARDED")) {
                // A rewarded video was closed.  Preload the next rewarded video.
                if (!m_isRewardRejected) {
                    AddEvent(AdType.Incentivized, AdEvent.IncentivizedComplete);
                } else {
                    AddEvent(AdType.Incentivized, AdEvent.IncentivizedIncomplete);
                }
                AddEvent(AdType.Incentivized, AdEvent.Hide);
            }
            else if (evnt.Contains("REWARDAPPROVEDINFO")) {
                m_rewardInfo = evnt;
            }
            else if (evnt.Contains("REWARDTIMEOUT")) {
                m_isRewardRejected = true;
            }
            else if (evnt.Contains("USERCLOSEDEARLY")) {
                m_isRewardRejected = true;
            }
            else if (evnt.Contains("REWARDREJECTED")) {
                m_isRewardRejected = true;
            }
            else if (evnt.Contains("LOADEDREWARDED")) {
                // A rewarded video was successfully loaded.
                AddEvent(AdType.Incentivized, AdEvent.Prepared);
            }
            else if (evnt.Contains("LOADREWARDEDFAILED")) {
                // A rewarded video failed to load.
                AddEvent(AdType.Incentivized, AdEvent.PrepareFailure);
            }
            // ------ BANNER
            else if (evnt.Contains("LOADEDBANNER")) {
                AddEvent(AdType.Banner, AdEvent.Prepared);
            }
            else if (evnt.Contains("LOADBANNERFAILED")) {
                AddEvent(AdType.Banner, AdEvent.PrepareFailure);
            }
            else if (evnt.Contains("DISPLAYEDBANNER")) {
                AddEvent(AdType.Banner, AdEvent.Show);
            }
            else if (evnt.Contains("HIDDENBANNER")) {
                AddEvent(AdType.Banner, AdEvent.Hide);
            }
            else if (evnt.Contains("LEFTAPPLICATION")) {

            }
            else if (evnt.Contains("DISPLAYFAILED")) {

            }
        }
        */

        private void OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            Debug.Log("AppLovinAdapter OnSdkInitializedEvent()");
        }

        // _____________________________________________
        // INTERSTITIAL

        private void OnInterstitialLoadedEvent(string adUnitId)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(adUnitId) will now return 'true'
            AddEvent(AdType.Interstitial, AdEvent.Prepared);

            Debug.Log("AppLovinAdapter OnInterstitialLoadedEvent() " + adUnitId);
        }

        private void OnInterstitialFailedEvent(string adUnitId, int errorCode)
        {
            // Interstitial ad failed to load 
            // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)
            AddEvent(AdType.Interstitial, AdEvent.PrepareFailure);

            Debug.Log("AppLovinAdapter OnInterstitialFailedEvent() " + adUnitId + " errorCode:" + errorCode);
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, int errorCode)
        {
            // Interstitial ad failed to display. We recommend loading the next ad  
            AddEvent(AdType.Interstitial, AdEvent.Hide);
        }

        private void OnInterstitialDismissedEvent(string adUnitId)
        {
            // Interstitial ad is hidden. Pre-load the next ad
            AddEvent(AdType.Interstitial, AdEvent.Hide);
        }

        // _____________________________________________
        // REWARDED

        private void OnRewardedAdLoadedEvent(string adUnitId)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(adUnitId) will now return 'true'
            AddEvent(AdType.Incentivized, AdEvent.Prepared);
        }

        private void OnRewardedAdFailedEvent(string adUnitId, int errorCode)
        {
            // Rewarded ad failed to load 
            // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)
            AddEvent(AdType.Incentivized, AdEvent.PrepareFailure);
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
            AddEvent(AdType.Incentivized, AdEvent.Hide);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId)
        {
            AddEvent(AdType.Incentivized, AdEvent.Show);
        }

        private void OnRewardedAdClickedEvent(string adUnitId)
        {
            AddEvent(AdType.Incentivized, AdEvent.Click);
        }

        private void OnRewardedAdDismissedEvent(string adUnitId)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            AddEvent(AdType.Incentivized, AdEvent.Hide);
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward)
        {
            // Rewarded ad was displayed and user should receive the reward
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedComplete);
        }

#endif // _MS_APPLOVIN

    }
} // namespace Virterix.AdMediation

