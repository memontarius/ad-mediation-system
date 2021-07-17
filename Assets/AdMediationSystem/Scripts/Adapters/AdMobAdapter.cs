//#define _AMS_ADMOB

using UnityEngine;
using System;
using System.Collections.Generic;
using Boomlagoon.JSON;
using System.Linq;
#if UNITY_EDITOR
using System.Reflection;
#endif
#if _AMS_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Virterix.AdMediation
{
    public class AdMobAdapter : AdNetworkAdapter
    {
        public const string _BANNER_ID_KEY = "bannerId";
        public const string _INTERSTITIAL_ID_KEY = "interstitialId";
        public const string _REWARDED_ID_KEY = "rewardedId";
        public const string _APP_ID_KEY = "appId";

        public enum AdMobBannerSize
        {
            SmartBanner,
            Banner,
            MediumRectangle,
            Leaderboard,
            IABBanner
        }

        public enum AdMobBannerPosition
        {
            Center,
            Top,
            TopLeft,
            TopRight,
            Bottom,
            BottomLeft,
            BottomRight
        }

        public enum AdMobTagForUnderAgeOfConsent 
        {
            Unspecified = 0,
            False,
            True
        }

        public enum AdMobMaxAdContentRating
        {
            Unspecified = 0,
            G,
            MA,
            PG,
            T
        }

        public struct AdRequestBuilderContainer {
#if _AMS_ADMOB
            public AdRequestBuilderContainer(AdRequest.Builder builder) {
                _builder = builder;
            }
            public readonly AdRequest.Builder _builder;
#endif
        }

        public struct InitializationStatusContainer {
#if _AMS_ADMOB
            public InitializationStatusContainer(InitializationStatus status) {
                _status = status;
            }
            public readonly InitializationStatus _status;
#endif
        }

        public event Action OnWillInitialize = delegate { };
        public event Action OnDidInitialize = delegate { };

        public event Action<AdType, AdRequestBuilderContainer> OnAdRequest = delegate { };
        public event Action<InitializationStatusContainer> OnInitializationComplete = delegate { };

        private const string UnderAgeOfConsentSaveKey = AdMediationSystem.PREFIX + "abmob.uac";
        private const string MaxContentRatingSaveKey = AdMediationSystem.PREFIX + "abmob.mcr";

        public bool m_useMediation;

        /// <summary>
        /// Should be assigned before initialization
        /// </summary>
        public AdMobTagForUnderAgeOfConsent UnderAgeOfConsent { get; set; }
        /// <summary>
        /// Should be assigned before initialization
        /// </summary>
        public AdMobMaxAdContentRating MaxContentRating { get; set; }

        protected override string AdInstanceParametersFolder
        {
            get {  return AdMobAdInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER; }
        }

        public static void SetupNetworkNativeSettings(string iOSAppId, string androidAppId)
        {
#if UNITY_EDITOR && _AMS_ADMOB
            string path = "";
            string[] foundAssets = UnityEditor.AssetDatabase.FindAssets("t:GoogleMobileAdsSettings");
            if (foundAssets.Length == 1)
            {
                path = UnityEditor.AssetDatabase.GUIDToAssetPath(foundAssets[0]);
            }
            ScriptableObject adMobSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            
            if (adMobSettings != null)
            {
                Type settingsType = adMobSettings.GetType();
                
                PropertyInfo prop = settingsType.GetProperty("GoogleMobileAdsAndroidAppId");
                prop.SetValue(adMobSettings, androidAppId);

                prop = settingsType.GetProperty("GoogleMobileAdsIOSAppId");
                prop.SetValue(adMobSettings, iOSAppId);

                prop = settingsType.GetProperty("DelayAppMeasurementInit");
                prop.SetValue(adMobSettings, true);

                UnityEditor.EditorUtility.SetDirty(adMobSettings);
            }
            else
            {
                GoogleMobileAds.Editor.GoogleMobileAdsSettingsEditor.OpenInspector();
                Debug.LogWarning("AdMob Settings not found! Try again!");
            }
#endif
        }

        public static string GetSDKVersion()
        {
            string version = string.Empty;
#if UNITY_EDITOR && _AMS_ADMOB
            version = AdRequest.Version;
#endif
            return version;
        }

#if _AMS_ADMOB
        public class AdMobAdInstanceData : AdInstance
        {
            public AdMobAdInstanceData(AdNetworkAdapter network) : base(network)
            {
            }

            public AdMobAdInstanceData(AdNetworkAdapter network, AdType adType, string adID, string name = AdInstance.AD_INSTANCE_DEFAULT_NAME) :
                base(network, adType, adID, name)
            {
            }

            public EventHandler<EventArgs> onAdLoadedHandler;
            public EventHandler<AdFailedToLoadEventArgs> onAdFailedToLoadHandler;
            public EventHandler<EventArgs> onAdOpeningHandler;
            public EventHandler<AdErrorEventArgs> onAdRewardVideoFailedToShowHandler;
            public EventHandler<EventArgs> onAdClosedHandler;
            public EventHandler<AdFailedToLoadEventArgs> onAdRewardVideoFailedToLoadHandler;
            public EventHandler<Reward> onAdRewardVideoEarnedHandler;
            public EventHandler<AdValueEventArgs> onAdRewardVideoPaidHandler;
        }

        private void OnApplicationPause(bool pause)
        {
#if UNITY_IOS
                MobileAds.SetiOSAppPauseOnBackground(pause);
#endif
        }

        public static AdSize ConvertToNativeBannerSize(AdMobBannerSize bannerSize)
        {
            AdSize admobAdSize = AdSize.Banner;

            switch (bannerSize)
            {
                case AdMobBannerSize.Banner:
                    admobAdSize = AdSize.Banner;
                    break;
                case AdMobBannerSize.IABBanner:
                    admobAdSize = AdSize.IABBanner;
                    break;
                case AdMobBannerSize.SmartBanner:
                    admobAdSize = AdSize.SmartBanner;
                    break;
                case AdMobBannerSize.Leaderboard:
                    admobAdSize = AdSize.Leaderboard;
                    break;
                case AdMobBannerSize.MediumRectangle:
                    admobAdSize = AdSize.MediumRectangle;
                    break;
            }
            return admobAdSize;
        }

        public static AdPosition ConvertToNativeBannerPosition(AdMobBannerPosition bannerPosition)
        {
            AdPosition admobAdPosition = AdPosition.Center;

            switch (bannerPosition)
            {
                case AdMobBannerPosition.Bottom:
                    admobAdPosition = AdPosition.Bottom;
                    break;
                case AdMobBannerPosition.BottomLeft:
                    admobAdPosition = AdPosition.BottomLeft;
                    break;
                case AdMobBannerPosition.BottomRight:
                    admobAdPosition = AdPosition.BottomRight;
                    break;
                case AdMobBannerPosition.Top:
                    admobAdPosition = AdPosition.Top;
                    break;
                case AdMobBannerPosition.TopLeft:
                    admobAdPosition = AdPosition.TopLeft;
                    break;
                case AdMobBannerPosition.TopRight:
                    admobAdPosition = AdPosition.TopRight;
                    break;
                case AdMobBannerPosition.Center:
                    admobAdPosition = AdPosition.Center;
                    break;
            }
            return admobAdPosition;
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);

            ConfigureAdMob();

            OnWillInitialize();
            MobileAds.Initialize(OnInitComplete);
            OnDidInitialize();
        }

        protected override AdInstance CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            AdInstance adInstance = new AdMobAdInstanceData(this);
            return adInstance;
        }

        private AdMobMediationBehavior _adMobMediationBehavior;

        private void ConfigureAdMob()
        {
            RequestConfiguration.Builder builder = new RequestConfiguration.Builder();
            if (m_useMediation) {
                _adMobMediationBehavior = new AdMobMediationBehavior(this);
            }

            if (AdMediationSystem.Instance.m_isChildrenDirected)
                builder.SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.True);
            if (AdMediationSystem.Instance.m_testModeEnabled)
                builder.SetTestDeviceIds(new List<string>(AdMediationSystem.Instance.m_testDevices));

            if (UnderAgeOfConsent != AdMobTagForUnderAgeOfConsent.Unspecified)
            {
                var tagConsent = UnderAgeOfConsent == AdMobTagForUnderAgeOfConsent.True ? TagForUnderAgeOfConsent.True : TagForUnderAgeOfConsent.False;
                builder.SetTagForUnderAgeOfConsent(tagConsent);
            }

            if (MaxContentRating != AdMobMaxAdContentRating.Unspecified)
            {
                MaxAdContentRating maxAdContentRating = MaxAdContentRating.Unspecified;
                switch(MaxContentRating)
                {
                    case AdMobMaxAdContentRating.G:
                        maxAdContentRating = MaxAdContentRating.G;
                        break;
                    case AdMobMaxAdContentRating.MA:
                        maxAdContentRating = MaxAdContentRating.MA;
                        break;
                    case AdMobMaxAdContentRating.PG:
                        maxAdContentRating = MaxAdContentRating.PG;
                        break;
                    case AdMobMaxAdContentRating.T:
                        maxAdContentRating = MaxAdContentRating.T;
                        break;
                }
                builder.SetMaxAdContentRating(MaxAdContentRating.G);
            }

            RequestConfiguration requestConfiguration = builder.build();
            MobileAds.SetRequestConfiguration(requestConfiguration);
        }

        public void SaveContentConfig()
        {
            PlayerPrefs.SetInt(UnderAgeOfConsentSaveKey, (int)UnderAgeOfConsent);
            PlayerPrefs.SetInt(MaxContentRatingSaveKey, (int)MaxContentRating);
        }
        
        public void RestoreContentConfig()
        {
            UnderAgeOfConsent = (AdMobTagForUnderAgeOfConsent)PlayerPrefs.GetInt(UnderAgeOfConsentSaveKey, (int)AdMobTagForUnderAgeOfConsent.Unspecified);
            MaxContentRating = (AdMobMaxAdContentRating)PlayerPrefs.GetInt(MaxContentRatingSaveKey, (int)AdMobMaxAdContentRating.Unspecified);
        }

        public override void Prepare(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdMobAdInstanceData adMobAdInstance = adInstance == null ? null : adInstance as AdMobAdInstanceData;
            if (!IsReady(adMobAdInstance))
            {
                AdType adType = adInstance.m_adType;

                if (adInstance.State != AdState.Loading)
                {
                    switch (adType)
                    {
                        case AdType.Banner:
                            RequestBanner(adMobAdInstance, placement);
                            break;
                        case AdType.Interstitial:
                            RequestInterstitial(adMobAdInstance);
                            break;
                        case AdType.Incentivized:
                            RequestRewardVideo(adMobAdInstance);
                            break;
                    }
                }
            }
        }

        public override bool Show(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdMobAdInstanceData adMobAdInstance = adInstance == null ? null : adInstance as AdMobAdInstanceData;
            AdType adType = adInstance.m_adType;
            bool isAdAvailable = IsReady(adInstance, placement);
            bool isPreviousBannerDisplayed = adMobAdInstance.m_bannerDisplayed;

            if (adType == AdType.Banner)
            {
                adMobAdInstance.m_bannerDisplayed = true;
                adMobAdInstance.CurrPlacement = placement;
            }

            if (isAdAvailable)
            {
                switch (adType)
                {
                    case AdType.Banner:
                        BannerView bannerView = adInstance.m_adView as BannerView;
                        isAdAvailable = bannerView != null;
                        if (isAdAvailable)
                        {
#if UNITY_EDITOR
                            bannerView.Hide();
#endif
                            bannerView.Show();
                            bannerView.SetPosition(ConvertToNativeBannerPosition((AdMobBannerPosition)GetBannerPosition(adInstance, placement)));
                        }
                        if (!isPreviousBannerDisplayed)
                            AddEvent(adInstance.m_adType, AdEvent.Show, adInstance);
                        break;
                    case AdType.Interstitial:
                        InterstitialAd interstitial = adInstance.m_adView as InterstitialAd;
                        interstitial.Show();
                        break;
                    case AdType.Incentivized:
                        RewardedAd rewardedAd = adInstance.m_adView as RewardedAd;
                        rewardedAd.Show();
                        break;
                }
            }
            return isAdAvailable;
        }

        public override void Hide(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdMobAdInstanceData adMobAdInstance = adInstance == null ? null : adInstance as AdMobAdInstanceData;
            AdType adType = adInstance.m_adType;

            switch (adType)
            {
                case AdType.Banner:
                    if (adInstance.State == AdState.Received)
                    {
                        BannerView bannerView = adInstance.m_adView as BannerView;
                        bannerView.Hide();
                        if (adMobAdInstance.m_bannerDisplayed)
                            NotifyEvent(AdEvent.Hiding, adInstance);
                    }
                    adMobAdInstance.m_bannerDisplayed = false;
                    break;
            }
        }

        public override bool IsReady(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
#if UNITY_EDITOR
            //return false;
#endif
            AdType adType = adInstance.m_adType;
            bool isReady = adInstance.State == AdState.Received;
            AdMobAdInstanceData adMobAdInstance = adInstance == null ? null : adInstance as AdMobAdInstanceData;

            switch (adType)
            {
                case AdType.Incentivized:
                    if (adInstance.m_adView != null)
                    {
                        RewardedAd rewardedAd = adInstance.m_adView as RewardedAd;
                        isReady = rewardedAd.IsLoaded();
                    }
                    break;
            }

            return isReady;
        }

        private void RequestBanner(AdMobAdInstanceData adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            DestroyBanner(adInstance);

            adInstance.State = AdState.Loading;
            adInstance.CurrPlacement = placement;

            AdMobAdInstanceBannerParameters bannerParams = adInstance.m_adInstanceParams as AdMobAdInstanceBannerParameters;
            AdPosition bannerPosition = ConvertToNativeBannerPosition((AdMobBannerPosition)GetBannerPosition(adInstance, placement));
            
            BannerView bannerView = new BannerView(adInstance.m_adId, ConvertToNativeBannerSize(bannerParams.m_bannerSize), bannerPosition);
            adInstance.m_adView = bannerView;
            bannerView.Hide();

            // Register for ad events.
            adInstance.onAdLoadedHandler = delegate (object sender, EventArgs args)
            {
                HandleAdLoaded(adInstance, sender, args);
            };
            bannerView.OnAdLoaded += adInstance.onAdLoadedHandler;

            adInstance.onAdFailedToLoadHandler = delegate (object sender, AdFailedToLoadEventArgs args)
            {
                HandleAdFailedToLoad(adInstance, sender, args);
            };
            bannerView.OnAdFailedToLoad += adInstance.onAdFailedToLoadHandler;

            adInstance.onAdOpeningHandler = delegate (object sender, EventArgs args)
            {
                HandleAdOpened(adInstance, sender, args);
            };
            bannerView.OnAdOpening += adInstance.onAdOpeningHandler;

            adInstance.onAdClosedHandler = delegate (object sender, EventArgs args)
            {
                HandleAdClosed(adInstance, sender, args);
            };
            bannerView.OnAdClosed += adInstance.onAdClosedHandler;

            // Load a banner ad.
            bannerView.LoadAd(CreateAdRequest(AdType.Banner));
        }

        void DestroyBanner(AdMobAdInstanceData adInstance)
        {
            if (adInstance.m_adView != null)
            {
                BannerView bannerView = adInstance.m_adView as BannerView;
                adInstance.m_adView = null;

                bannerView.OnAdLoaded -= adInstance.onAdLoadedHandler;
                bannerView.OnAdFailedToLoad -= adInstance.onAdFailedToLoadHandler;
                bannerView.OnAdOpening -= adInstance.onAdOpeningHandler;
                bannerView.OnAdClosed -= adInstance.onAdClosedHandler;

                bannerView.Destroy();
                adInstance.State = AdState.Unavailable;
            }
        }

        private void RequestInterstitial(AdMobAdInstanceData adInstance)
        {
            DestroyInterstitial(adInstance);

            adInstance.State = AdState.Loading;

            // Create an interstitial.
            InterstitialAd interstitial = new InterstitialAd(adInstance.m_adId);
            adInstance.m_adView = interstitial;

            // Register for ad events.
            adInstance.onAdLoadedHandler = delegate (object sender, EventArgs args)
            {
                HandleInterstitialLoaded(adInstance, sender, args);
            };
            interstitial.OnAdLoaded += adInstance.onAdLoadedHandler;

            adInstance.onAdFailedToLoadHandler = delegate (object sender, AdFailedToLoadEventArgs args)
            {
                HandleInterstitialFailedToLoad(adInstance, sender, args);
            };
            interstitial.OnAdFailedToLoad += adInstance.onAdFailedToLoadHandler;

            adInstance.onAdOpeningHandler = delegate (object sender, EventArgs args)
            {
                HandleInterstitialOpened(adInstance, sender, args);
            };
            interstitial.OnAdOpening += adInstance.onAdOpeningHandler;

            adInstance.onAdClosedHandler = delegate (object sender, EventArgs args)
            {
                HandleInterstitialClosed(adInstance, sender, args);
            };
            interstitial.OnAdClosed += adInstance.onAdClosedHandler;

            interstitial.LoadAd(CreateAdRequest(AdType.Interstitial));
        }

        private void DestroyInterstitial(AdMobAdInstanceData adInstance)
        {
            if (adInstance.m_adView != null)
            {
                InterstitialAd interstitial = adInstance.m_adView as InterstitialAd;
                adInstance.m_adView = null;

                interstitial.OnAdLoaded -= adInstance.onAdLoadedHandler;
                interstitial.OnAdFailedToLoad -= adInstance.onAdFailedToLoadHandler;
                interstitial.OnAdOpening -= adInstance.onAdOpeningHandler;
                interstitial.OnAdClosed -= adInstance.onAdClosedHandler;

                interstitial.Destroy();
                adInstance.State = AdState.Uncertain;
            }
        }

        private void RequestRewardVideo(AdMobAdInstanceData adInstance)
        {
            DestroyRewardVideo(adInstance);

            adInstance.State = AdState.Loading;
            RewardedAd rewardedAd = new RewardedAd(adInstance.m_adId);
            adInstance.m_adView = rewardedAd;

            adInstance.onAdLoadedHandler = delegate (object sender, EventArgs args)
            {
                HandleRewardVideoLoaded(adInstance, sender, args);
            };
            rewardedAd.OnAdLoaded += adInstance.onAdLoadedHandler;

            adInstance.onAdRewardVideoFailedToLoadHandler = delegate (object sender, AdFailedToLoadEventArgs args)
            {
                HandleRewardVideoFailedToLoad(adInstance, sender, args);
            };
            rewardedAd.OnAdFailedToLoad += adInstance.onAdRewardVideoFailedToLoadHandler;

            adInstance.onAdOpeningHandler = delegate (object sender, EventArgs args)
            {
                HandleRewardVideoOpened(adInstance, sender, args);
            };
            rewardedAd.OnAdOpening += adInstance.onAdOpeningHandler;

            adInstance.onAdRewardVideoFailedToShowHandler = delegate (object sender, AdErrorEventArgs args)
            {
                HandleRewardVideoFailedToShow(adInstance, sender, args);
            };
            rewardedAd.OnAdFailedToShow += adInstance.onAdRewardVideoFailedToShowHandler;

            adInstance.onAdClosedHandler = delegate (object sender, EventArgs args)
            {
                HandleRewardVideoClosed(adInstance, sender, args);
            };
            rewardedAd.OnAdClosed += adInstance.onAdClosedHandler;

            adInstance.onAdRewardVideoEarnedHandler = delegate (object sender, Reward reward)
            {
                HandleRewardVideoEarned(adInstance, sender, reward);
            };
            rewardedAd.OnUserEarnedReward += adInstance.onAdRewardVideoEarnedHandler;

            adInstance.onAdRewardVideoPaidHandler = delegate (object sender, AdValueEventArgs args)
            {
                HandleRewardVideoPaidEvent(adInstance, sender, args);
            };
            rewardedAd.OnPaidEvent += adInstance.onAdRewardVideoPaidHandler;

            rewardedAd.LoadAd(CreateAdRequest(AdType.Incentivized));
        }

        private void DestroyRewardVideo(AdMobAdInstanceData adInstance)
        {
            if (adInstance.m_adView != null)
            {
                RewardedAd rewardedAd = adInstance.m_adView as RewardedAd;
                rewardedAd.OnAdLoaded -= adInstance.onAdLoadedHandler;
                rewardedAd.OnAdFailedToLoad -= adInstance.onAdRewardVideoFailedToLoadHandler;
                rewardedAd.OnAdOpening -= adInstance.onAdOpeningHandler;
                rewardedAd.OnAdFailedToShow -= adInstance.onAdRewardVideoFailedToShowHandler;
                rewardedAd.OnAdClosed -= adInstance.onAdClosedHandler;
                rewardedAd.OnUserEarnedReward -= adInstance.onAdRewardVideoEarnedHandler;
                rewardedAd.OnPaidEvent -= adInstance.onAdRewardVideoPaidHandler;
                adInstance.m_adView = null;

                adInstance.State = AdState.Uncertain;
            }
        }

        // Returns an ad request with custom ad targeting.
        private AdRequest CreateAdRequest(AdType adType)
        {
            AdRequest.Builder requestBuilder = new AdRequest.Builder();

            if (AdMediationSystem.UserPersonalisationConsent == PersonalisationConsent.Denied)
            {
                requestBuilder.AddExtra("npa", "1");
                requestBuilder.AddExtra("rdp", "1");
            }

            OnAdRequest(adType, new AdRequestBuilderContainer(requestBuilder));

            AdRequest request = requestBuilder.Build();
            return request;
        }

        //------------------------------------------------------------------------
        // AdMob Callbacks
        private void OnInitComplete(InitializationStatus initStatus)
        {
            OnInitializationComplete(new InitializationStatusContainer(initStatus));     
        }

        //------------------------------------------------------------------------
#region Banner callback handlers

        public void HandleAdLoaded(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleAdLoaded() " + " adInstance: " + adInstance.Name +
                " isVisibled: " + adInstance.m_bannerDisplayed);
#endif

            adInstance.State = AdState.Received;
            BannerView bannerView = adInstance.m_adView as BannerView;
            AddEvent(AdType.Banner, AdEvent.Prepared, adInstance);
            if (adInstance.m_bannerDisplayed)
            {
#if UNITY_EDITOR
                bannerView.Hide();
#endif
                bannerView.Show();
            }
            else
                bannerView.Hide();
        }

        public void HandleAdFailedToLoad(AdMobAdInstanceData adInstance, object sender, AdFailedToLoadEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleAdFailedToLoad() " + " adInstance: " + adInstance.Name +
                " message: " + args.LoadAdError.GetMessage());
#endif
            DestroyBanner(adInstance);
            AddEvent(AdType.Banner, AdEvent.PreparationFailed, adInstance);
        }

        public void HandleAdOpened(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleAdOpened() " + " adInstance: " + adInstance.Name);
#endif
        }

        void HandleAdClosing(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleAdClosing() " + " adInstance: " + adInstance.Name);
#endif
        }

        public void HandleAdClosed(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleAdClosed() " + " adInstance: " + adInstance.Name);
#endif
        }

#endregion // Banner callback handlers

        //------------------------------------------------------------------------
#region Interstitial callback handlers

        public void HandleInterstitialLoaded(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleInterstitialLoaded()");
#endif
            adInstance.State = AdState.Received;
            AddEvent(AdType.Interstitial, AdEvent.Prepared, adInstance);
        }

        public void HandleInterstitialFailedToLoad(AdMobAdInstanceData adInstance, object sender, AdFailedToLoadEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleInterstitialFailedToLoad() message: " + args.LoadAdError.GetMessage());
#endif
            DestroyInterstitial(adInstance);
            AddEvent(AdType.Interstitial, AdEvent.PreparationFailed, adInstance);
        }

        public void HandleInterstitialOpened(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleInterstitialOpened()");
#endif
            AddEvent(AdType.Interstitial, AdEvent.Show, adInstance);
        }

        void HandleInterstitialClosing(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleInterstitialClosing()");
#endif
        }

        public void HandleInterstitialClosed(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            print("[AMS] AdMobAdapter.HandleInterstitialClosed()");
#endif
            DestroyInterstitial(adInstance);
            AddEvent(AdType.Interstitial, AdEvent.Hiding, adInstance);
        }

#endregion // Interstitial callback handlers

        //------------------------------------------------------------------------
#region Reward Video callback handlers

        public void HandleRewardVideoLoaded(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            MonoBehaviour.print("[AMS] AdMobAdapter.HandleRewardVideoLoaded()");
#endif
            adInstance.State = AdState.Received;
            AddEvent(AdType.Incentivized, AdEvent.Prepared, adInstance);
        }

        public void HandleRewardVideoFailedToLoad(AdMobAdInstanceData adInstance, object sender, AdFailedToLoadEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            MonoBehaviour.print("[AMS] AdMobAdapter.HandleRewardVideoFailedToLoad() message: " + args.LoadAdError.GetMessage());
#endif
            adInstance.State = AdState.Uncertain;
            AddEvent(AdType.Incentivized, AdEvent.PreparationFailed, adInstance);
        }

        public void HandleRewardVideoFailedToShow(AdMobAdInstanceData adInstance, object sender, AdErrorEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            MonoBehaviour.print("[AMS] AdMobAdapter.HandleRewardVideoFailedToLoad() message: " + args.Message);
#endif
            DestroyRewardVideo(adInstance);
            AddEvent(AdType.Incentivized, AdEvent.Hiding, adInstance);
        }

        public void HandleRewardVideoOpened(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            MonoBehaviour.print("[AMS] AdMobAdapter.HandleRewardVideoOpened()");
#endif
            AddEvent(AdType.Incentivized, AdEvent.Show, adInstance);
        }

        public void HandleRewardVideoStarted(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            MonoBehaviour.print("[AMS] AdMobAdapter.HandleRewardVideoStarted()");
#endif
        }

        public void HandleRewardVideoClosed(AdMobAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            MonoBehaviour.print("[AMS] AdMobAdapter.HandleRewardVideoClosed()");
#endif
            DestroyRewardVideo(adInstance);
            AddEvent(AdType.Incentivized, AdEvent.Hiding, adInstance);
        }

        public void HandleRewardVideoEarned(AdMobAdInstanceData adInstance, object sender, Reward reward)
        {
#if AD_MEDIATION_DEBUG_MODE
            MonoBehaviour.print("[AMS] AdMobAdapter.HandleRewardVideoEarned() " + reward.Amount.ToString() + " " + reward.Type);
#endif
            m_lastReward.label = reward.Type;
            m_lastReward.amount = reward.Amount;
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedCompleted, adInstance);
        }

        private void HandleRewardVideoPaidEvent(AdMobAdInstanceData adInstance, object sender, AdValueEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            MonoBehaviour.print("[AMS] AdMobAdapter.HandleRewardVideoPaidEvent()");
#endif
        }

#endregion // Reward Video callback handlers

#endif // _AMS_ADMOB
    }
} // namespace Virterix.AdMediation
