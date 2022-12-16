//#define _AMS_UNITY_ADS

using System;
using System.Collections.Generic;
using UnityEngine;
using Boomlagoon.JSON;
using Unity.Services.Core;
using Debug = UnityEngine.Debug;
#if _AMS_UNITY_ADS
using System.Collections;
using Unity.Services.Mediation;
#endif

namespace Virterix.AdMediation
{
    public class UnityAdsAdapter : AdNetworkAdapter
    {
        private string m_appId;
        private bool m_isBannerDisplayed;
        public Vector2 m_bannerHidingOffset = new Vector2(100000f, 100000f);
#if _AMS_UNITY_ADS
        private InterstitialAdShowOptions m_interstitialShowOptions;
        private RewardedAdShowOptions m_rewardedShowOptions;
#endif
        private AdMediator[] m_bannerMediators;
        private bool m_unityAdsInitialized;
        private float m_deferredInitializeDelay = 30;
        
        public enum UnityBannerAnchor
        {
            TopCenter = 0,
            TopLeft = 1,
            TopRight = 2,
            Center = 3,
            MiddleLeft = 4,
            MiddleRight = 5,
            BottomCenter = 6,
            BottomLeft = 7,
            BottomRight = 8,
            None = 9,
            Default = BottomCenter
        }
        
        public enum UnityBannerSize
        {
            Banner,
            LargeBanner,
            MediumRectangle,
            Leaderboard
        }
        
        public class UnityAdInstanceData : AdInstance
        {
            public UnityAdInstanceData(AdNetworkAdapter network) : base(network)
            {
            }

            public UnityAdInstanceData(AdNetworkAdapter network, AdType adType, string adID, string name = AdInstance.AD_INSTANCE_DEFAULT_NAME) :
                base(network, adType, adID, name)
            {
            }

#if _AMS_UNITY_ADS  
            public IInterstitialAd InterstitialAd;
            public IRewardedAd RewardedAd;
            public IBannerAd BannerAd;
            public BannerAdSize BannerSize;
            public BannerAdAnchor BannerAnchor;
            
            public EventHandler OnLoaded;
            public EventHandler<LoadErrorEventArgs> OnFailedLoad;
            public EventHandler OnShowed;
            public EventHandler OnClicked;
            public EventHandler OnClosed;
            public EventHandler<ShowErrorEventArgs> OnFailedShow;
            public EventHandler<RewardEventArgs> OnUserRewarded;
            public EventHandler<LoadErrorEventArgs> OnRefreshed;
#endif
        }
        
        protected override string AdInstanceParametersFolder
        {
            get { return UnityAdInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER; }
        }

        public override bool UseSingleBannerInstance => true;

        public override bool RequiredWaitingInitializationResponse => true;

        public static string GetSDKVersion()
        {
            string version = string.Empty;
#if UNITY_EDITOR && _AMS_UNITY_ADS
#endif
            return version;
        }

#if _AMS_UNITY_ADS
        public static BannerAdAnchor ConvertToNativeBannerPosition(UnityBannerAnchor bannerAnchor)
        {
            BannerAdAnchor nativeBannerAnchor = BannerAdAnchor.BottomCenter;
            switch(bannerAnchor)
            {
                case UnityBannerAnchor.Center:
                    nativeBannerAnchor = BannerAdAnchor.Center;
                    break;
                case UnityBannerAnchor.None:
                    nativeBannerAnchor = BannerAdAnchor.None;
                    break;
                case UnityBannerAnchor.BottomCenter:
                    nativeBannerAnchor = BannerAdAnchor.BottomCenter;
                    break;
                case UnityBannerAnchor.BottomLeft:
                    nativeBannerAnchor = BannerAdAnchor.BottomLeft;
                    break;
                case UnityBannerAnchor.BottomRight:
                    nativeBannerAnchor = BannerAdAnchor.BottomRight;
                    break;
                case UnityBannerAnchor.MiddleLeft:
                    nativeBannerAnchor = BannerAdAnchor.MiddleLeft;
                    break;
                case UnityBannerAnchor.MiddleRight:
                    nativeBannerAnchor = BannerAdAnchor.MiddleRight;
                    break;
                case UnityBannerAnchor.TopCenter:
                    nativeBannerAnchor = BannerAdAnchor.TopCenter;
                    break;
                case UnityBannerAnchor.TopLeft:
                    nativeBannerAnchor = BannerAdAnchor.TopLeft;
                    break;
                case UnityBannerAnchor.TopRight:
                    nativeBannerAnchor = BannerAdAnchor.TopRight;
                    break;
                default:
                    nativeBannerAnchor = BannerAdAnchor.Default;
                    break;
            } 
            return nativeBannerAnchor;
        }

        public static BannerAdSize ConvertToNativeBannerSize(UnityBannerSize bannerAnchor)
        {
            BannerAdSize nativeBannerSize = null;
            switch(bannerAnchor)
            {
                case UnityBannerSize.Banner:
                    nativeBannerSize = new BannerAdSize(BannerAdPredefinedSize.Banner);
                    break;
                case UnityBannerSize.Leaderboard:
                    nativeBannerSize = new BannerAdSize(BannerAdPredefinedSize.Leaderboard);
                    break;
                case UnityBannerSize.LargeBanner:
                    nativeBannerSize = new BannerAdSize(BannerAdPredefinedSize.LargeBanner);
                    break;
                case UnityBannerSize.MediumRectangle:
                    nativeBannerSize = new BannerAdSize(BannerAdPredefinedSize.MediumRectangle);
                    break;
            } 
            return nativeBannerSize;
        }
        
        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);
            
            if (!parameters.TryGetValue("appId", out m_appId))
                m_appId = "";
            
            m_interstitialShowOptions = new InterstitialAdShowOptions { AutoReload = true };
            m_rewardedShowOptions = new RewardedAdShowOptions { AutoReload = true };
            
            m_bannerMediators = AdMediationSystem.Instance.BannerMediators;
            StartCoroutine(DeferredInitializeUnity());
        }

        private IEnumerator DeferredInitializeUnity()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            InitializeUnity(m_appId);
        }
        
        private async void InitializeUnity(string appId)
        {
            try
            {
                await UnityServices.InitializeAsync(GetGameOptions(appId));
                OnInitializationComplete();
            }
            catch (Exception e)
            {
                OnInitializationFailed(e);
            }
        }
        
        private InitializationOptions GetGameOptions(string appId)
        {
            var initializationOptions = new InitializationOptions();
#if UNITY_IOS
            if (!string.IsNullOrEmpty(appId))
            {
                initializationOptions.SetGameId(appId);
            }
#elif UNITY_ANDROID
            if (!string.IsNullOrEmpty(appId))
            {
                initializationOptions.SetGameId(appId);
            }
#endif
            return initializationOptions;
        }
        
        protected override void InitializeAdInstanceData(AdInstance adInstance, JSONValue jsonAdInstances)
        {
            base.InitializeAdInstanceData(adInstance, jsonAdInstances);
        }

        protected override AdInstance CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            AdInstance adInstance = new UnityAdInstanceData(this);
            return adInstance;
        }

        public override void Prepare(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            adInstance.CurrPlacement = placement;
            
            if (!IsReady(adInstance) && adInstance.State != AdState.Loading && m_unityAdsInitialized)
            {
                if (!m_unityAdsInitialized)
                    return;
                
                adInstance.State = AdState.Loading;
                UnityAdInstanceData unityAdInstance = (UnityAdInstanceData)adInstance;
                
                try
                {
                    switch (adType)
                    {
                        case AdType.Interstitial:
                            if (unityAdInstance.InterstitialAd.AdState != Unity.Services.Mediation.AdState.Loading)
                                unityAdInstance.InterstitialAd.LoadAsync();
                            break;
                        case AdType.Incentivized:
                            if (unityAdInstance.RewardedAd.AdState != Unity.Services.Mediation.AdState.Loading)
                                unityAdInstance.RewardedAd.LoadAsync();
                            break;
                        case AdType.Banner:
                            if (unityAdInstance.BannerAd.AdState != Unity.Services.Mediation.AdState.Loading) 
                                unityAdInstance.BannerAd.LoadAsync();
                            break;
                    }
                }
                catch (LoadFailedException)
                {
                    adInstance.State = AdState.Unavailable;
                }
            }
        }

        public override bool Show(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            bool isPreviousBannerDisplayed = m_isBannerDisplayed;
            UnityAdInstanceData unityAdInstance = (UnityAdInstanceData)adInstance;
            adInstance.CurrPlacement = placement;
            
            if (adType == AdType.Banner)
            {
                adInstance.m_bannerDisplayed = true;
                m_isBannerDisplayed = true;
                m_currBannerPlacement = placement;
            }

            if (IsReady(adInstance))
            {
                switch (adType)
                {
                    case AdType.Interstitial:
                        try
                        {
                            unityAdInstance.InterstitialAd?.ShowAsync(m_interstitialShowOptions);
                        }
                        catch (ShowFailedException e)
                        {
                            Debug.Log($"[AMS] UnityAds Interstitial failed to show : {e.Message}");
                        }
                        break; 
                    case AdType.Incentivized:
                        try
                        {
                            unityAdInstance.RewardedAd?.ShowAsync(m_rewardedShowOptions);
                        }
                        catch (ShowFailedException e)
                        {
                            Debug.LogWarning($"[AMS] UnityAds Rewarded failed to show: {e.Message}");
                        }
                        break;
                    case AdType.Banner:
                        BannerAdAnchor bannerAnchor = ConvertToNativeBannerPosition((UnityBannerAnchor)GetBannerPosition(adInstance, placement));
                        unityAdInstance.BannerAd?.SetPosition(bannerAnchor);
                        if (!isPreviousBannerDisplayed)
                            AddEvent(adInstance.m_adType, AdEvent.Showing, adInstance);
                        break;
                }
                return true;
            }
            return false;
        }

        public override void Hide(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (adInstance.m_adType == AdType.Banner)
            {
                UnityAdInstanceData unityAdInstance = (UnityAdInstanceData)adInstance;
                bool wasShownInOtherPlacement = false;
                
                foreach (var mediator in m_bannerMediators)
                {
                    if (IsAdBannerInstanceUsedInMediator(adInstance, mediator))
                    {
                        BannerAdAnchor bannerAnchor = ConvertToNativeBannerPosition(
                            (UnityBannerAnchor)GetBannerPosition(adInstance, mediator.m_placementName));
                        unityAdInstance.BannerAd?.SetPosition(bannerAnchor);
                        wasShownInOtherPlacement = true;
                        break;
                    }
                }

                if (!wasShownInOtherPlacement)
                {
                    bool isBannerDisplayed = adInstance.m_bannerDisplayed;
                    adInstance.m_bannerDisplayed = false;
                    m_isBannerDisplayed = false;
                    unityAdInstance.BannerAd?.SetPosition(unityAdInstance.BannerAnchor, m_bannerHidingOffset);
                    if (isBannerDisplayed)
                        NotifyEvent(AdEvent.Hiding, adInstance);
                }
            }
        }
        
        public override bool IsReady(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            bool isReady = false;
            if (adInstance != null && m_unityAdsInitialized)
            {
                UnityAdInstanceData unityAdInstance = (UnityAdInstanceData)adInstance;
                switch (adInstance.m_adType)
                {
                    case AdType.Interstitial:
                        isReady = unityAdInstance.InterstitialAd.AdState == Unity.Services.Mediation.AdState.Loaded;
                        break;
                    case AdType.Incentivized:
                        isReady = unityAdInstance.RewardedAd.AdState == Unity.Services.Mediation.AdState.Loaded;
                        break;
                    case AdType.Banner:
                        isReady = unityAdInstance.State == AdState.Received;
                        break;
                }
            }
            return isReady;
        }

        protected override void SetUserConsentToPersonalizedAds(PersonalisationConsent consent)
        {
            if (consent != PersonalisationConsent.Undefined && MediationService.Instance != null)
            {
                // GDPR 
                var gdprConsent = consent == PersonalisationConsent.Accepted ? ConsentStatus.Given : ConsentStatus.Denied;
                MediationService.Instance.DataPrivacy.UserGaveConsent(gdprConsent, DataPrivacyLaw.GDPR);
                // CCPA
                var ccpaConsent =  consent == PersonalisationConsent.Accepted ? ConsentStatus.Given : ConsentStatus.Denied;
                MediationService.Instance.DataPrivacy.UserGaveConsent(ccpaConsent, DataPrivacyLaw.CCPA);
                // PIPL 
                var piplConsent =  consent == PersonalisationConsent.Accepted ? ConsentStatus.Given : ConsentStatus.Denied;
                MediationService.Instance.DataPrivacy.UserGaveConsent(piplConsent, DataPrivacyLaw.PIPLAdPersonalization);
                MediationService.Instance.DataPrivacy.UserGaveConsent(piplConsent, DataPrivacyLaw.PIPLDataTransport);
            }
        }

        private IInterstitialAd CreateInterstitialAd(UnityAdInstanceData unityAdInstance)
        {
            IInterstitialAd interstitial = MediationService.Instance.CreateInterstitialAd(unityAdInstance.m_adId);
            unityAdInstance.State = AdState.Uncertain;
            
            unityAdInstance.OnLoaded = delegate(object sender, EventArgs args)
            {
                OnAdLoaded(unityAdInstance, sender, args);
            };
            unityAdInstance.OnFailedLoad = delegate(object sender, LoadErrorEventArgs args)
            {
                OnAdFailedLoad(unityAdInstance, sender, args);
            };
            unityAdInstance.OnShowed = delegate(object sender, EventArgs args)
            {
                OnAdShowed(unityAdInstance, sender, args);
            };
            unityAdInstance.OnClosed = delegate(object sender, EventArgs args)
            {
                OnAdClosed(unityAdInstance, sender, args);
            };
            unityAdInstance.OnFailedShow = delegate(object sender, ShowErrorEventArgs args)
            {
                OnAdFailedShow(unityAdInstance, sender, args);
            };

            interstitial.OnLoaded += unityAdInstance.OnLoaded;
            interstitial.OnFailedLoad += unityAdInstance.OnFailedLoad;
            interstitial.OnShowed += unityAdInstance.OnShowed;
            interstitial.OnClosed += unityAdInstance.OnClosed;
            interstitial.OnFailedShow += unityAdInstance.OnFailedShow;
            return interstitial;
        }
        
        private void DisposeInterstitialAd(UnityAdInstanceData unityAdInstance)
        {
            unityAdInstance.State = AdState.Uncertain;
            unityAdInstance.InterstitialAd.OnLoaded -= unityAdInstance.OnLoaded;
            unityAdInstance.InterstitialAd.OnFailedLoad -= unityAdInstance.OnFailedLoad;
            unityAdInstance.InterstitialAd.OnShowed -= unityAdInstance.OnShowed;
            unityAdInstance.InterstitialAd.OnClosed -= unityAdInstance.OnClosed;
            unityAdInstance.InterstitialAd.OnFailedShow -= unityAdInstance.OnFailedShow;
            unityAdInstance.InterstitialAd.Dispose();
            unityAdInstance.InterstitialAd = null;
        }
        
        private IRewardedAd CreateRewardedAd(UnityAdInstanceData unityAdInstance)
        {
            IRewardedAd rewardedAd = MediationService.Instance.CreateRewardedAd(unityAdInstance.m_adId);
            unityAdInstance.State = AdState.Uncertain;
            
            unityAdInstance.OnLoaded = delegate(object sender, EventArgs args)
            {
                OnAdLoaded(unityAdInstance, sender, args);
            };
            unityAdInstance.OnFailedLoad = delegate(object sender, LoadErrorEventArgs args)
            {
                OnAdFailedLoad(unityAdInstance, sender, args);
            };
            unityAdInstance.OnShowed = delegate(object sender, EventArgs args)
            {
                OnAdShowed(unityAdInstance, sender, args);
            };
            unityAdInstance.OnClosed = delegate(object sender, EventArgs args)
            {
                OnAdClosed(unityAdInstance, sender, args);
            };
            unityAdInstance.OnFailedShow = delegate(object sender, ShowErrorEventArgs args)
            {
                OnAdFailedShow(unityAdInstance, sender, args);
            };
            unityAdInstance.OnUserRewarded = delegate(object sender, RewardEventArgs args)
            {
                OnAdUserRewarded(unityAdInstance, sender, args);
            };
            
            rewardedAd.OnLoaded += unityAdInstance.OnLoaded;
            rewardedAd.OnFailedLoad += unityAdInstance.OnFailedLoad;
            rewardedAd.OnShowed += unityAdInstance.OnShowed;
            rewardedAd.OnClosed += unityAdInstance.OnClosed;
            rewardedAd.OnFailedShow += unityAdInstance.OnFailedShow;
            rewardedAd.OnUserRewarded += unityAdInstance.OnUserRewarded;
            return rewardedAd;
        }
        
        private void DisposeRewardedAd(UnityAdInstanceData unityAdInstance)
        {
            unityAdInstance.State = AdState.Uncertain;
            unityAdInstance.RewardedAd.OnLoaded -= unityAdInstance.OnLoaded;
            unityAdInstance.RewardedAd.OnFailedLoad -= unityAdInstance.OnFailedLoad;
            unityAdInstance.RewardedAd.OnShowed -= unityAdInstance.OnShowed;
            unityAdInstance.RewardedAd.OnClosed -= unityAdInstance.OnClosed;
            unityAdInstance.RewardedAd.OnFailedShow -= unityAdInstance.OnFailedShow;
            unityAdInstance.RewardedAd.OnUserRewarded -= unityAdInstance.OnUserRewarded;
            unityAdInstance.RewardedAd.Dispose();
            unityAdInstance.RewardedAd = null;
        }
        
        private IBannerAd CreateBannerAd(UnityAdInstanceData unityAdInstance)
        {
            UnityAdInstanceBannerParameters bannerParams = unityAdInstance.m_adInstanceParams as UnityAdInstanceBannerParameters;
            unityAdInstance.BannerSize = ConvertToNativeBannerSize(bannerParams.m_bannerSize);
            unityAdInstance.BannerAnchor = ConvertToNativeBannerPosition(bannerParams.m_bannerAnchor);
            
            IBannerAd bannerAd = MediationService.Instance.CreateBannerAd(unityAdInstance.m_adId,
                unityAdInstance.BannerSize, unityAdInstance.BannerAnchor, m_bannerHidingOffset);
            unityAdInstance.State = AdState.Uncertain;
            
            unityAdInstance.OnLoaded = delegate(object sender, EventArgs args)
            {
                OnAdLoaded(unityAdInstance, sender, args);
            };
            unityAdInstance.OnFailedLoad = delegate(object sender, LoadErrorEventArgs args)
            {
                OnAdFailedLoad(unityAdInstance, sender, args);
            };
            unityAdInstance.OnRefreshed = delegate(object sender, LoadErrorEventArgs args)
            {
                OnAdRefreshed(unityAdInstance, sender, args);
            };
            
            bannerAd.OnLoaded += unityAdInstance.OnLoaded;
            bannerAd.OnFailedLoad += unityAdInstance.OnFailedLoad;
            bannerAd.OnRefreshed += unityAdInstance.OnRefreshed;
            return bannerAd;
        }
        
        private void DisposeBannerAd(UnityAdInstanceData unityAdInstance)
        {
            unityAdInstance.State = AdState.Uncertain;
            unityAdInstance.BannerAd.OnLoaded -= unityAdInstance.OnLoaded;
            unityAdInstance.BannerAd.OnFailedLoad -= unityAdInstance.OnFailedLoad;
            unityAdInstance.BannerAd.OnRefreshed -= unityAdInstance.OnRefreshed;
            unityAdInstance.BannerAd.Dispose();
            unityAdInstance.BannerAd = null;
        }

        private void OnDestroy()
        {
            foreach (AdInstance adInstance in m_adInstances)
            {
                UnityAdInstanceData unityAdInstance = (UnityAdInstanceData)adInstance;
                switch (adInstance.m_adType)
                {
                    case AdType.Interstitial:
                        DisposeInterstitialAd(unityAdInstance);
                        break;
                    case AdType.Incentivized:
                        DisposeRewardedAd(unityAdInstance);
                        break;
                    case AdType.Banner:
                        DisposeBannerAd(unityAdInstance);
                        break;
                }
            }
        }
        
        private IEnumerator DeferredInitializeUnityAds()
        {
            yield return new WaitForSecondsRealtime(m_deferredInitializeDelay);
            m_deferredInitializeDelay = Mathf.Clamp(m_deferredInitializeDelay * 2, 30f, 60 * 5);
            InitializeUnity(m_appId);
        }

        //_______________________________________________________________________________
        #region Callback Event Methods
 
        private void OnInitializationComplete()
        {
            WasInitializationResponse = true;
            m_unityAdsInitialized = true;
            SetUserConsentToPersonalizedAds(AdMediationSystem.UserPersonalisationConsent);
            MediationService.Instance.ImpressionEventPublisher.OnImpression += OnInterstitialImpressionEvent;
            foreach (var instance in m_adInstances)
            {
                UnityAdInstanceData unityAdInstance = (UnityAdInstanceData)instance;
                switch (unityAdInstance.m_adType)
                {
                    case AdType.Interstitial:
                        unityAdInstance.InterstitialAd = CreateInterstitialAd(unityAdInstance);
                        break;
                    case AdType.Incentivized:
                        unityAdInstance.RewardedAd = CreateRewardedAd(unityAdInstance);
                        break;
                    case AdType.Banner:
                        unityAdInstance.BannerAd = CreateBannerAd(unityAdInstance);
                        break;
                }
                
                if (unityAdInstance.LoadingOnStart)
                    Prepare(unityAdInstance);
            }
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] UnityAds Initialization Success");
#endif
        }

        private void OnInitializationFailed(Exception error)
        {
            WasInitializationResponse = true;
            SdkInitializationError initializationError = SdkInitializationError.Unknown;
            if (error is InitializeFailedException initializeFailedException)
            {
                initializationError = initializeFailedException.initializationError;
            }

            StartCoroutine(DeferredInitializeUnityAds());
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] UnityAds Initialization Failed: {initializationError}:{error.Message}");
#endif
        }

        private void OnInterstitialImpressionEvent(object sender, ImpressionEventArgs args)
        {
            var impressionData = args.ImpressionData != null ? JsonUtility.ToJson(args.ImpressionData, true) : "null";
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] UnityAds Impression event from ad unit id {args.AdUnitId} : {impressionData}");
#endif
        }

        // UnityAds Callbacks
        private void OnAdLoaded(UnityAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] UnityAds Ad loaded. AdType: {adInstance.m_adType} Name: {adInstance.Name}");
#endif
            adInstance.State = AdState.Received;
            if (adInstance.m_adType == AdType.Banner)
            {
                BannerAdAnchor bannerAnchor = BannerAdAnchor.Default;
                if (!string.IsNullOrEmpty(adInstance.CurrPlacement))
                    bannerAnchor = ConvertToNativeBannerPosition((UnityBannerAnchor)GetBannerPosition(adInstance, adInstance.CurrPlacement));

                if (adInstance.m_bannerDisplayed)
                    adInstance.BannerAd.SetPosition(bannerAnchor);
                else
                    adInstance.BannerAd.SetPosition(bannerAnchor, m_bannerHidingOffset);
            }
            AddEvent(adInstance.m_adType, AdEvent.Prepared, adInstance);
        }

        private void OnAdShowed(UnityAdInstanceData adInstance,object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] UnityAds Showed. {adInstance.m_adType}");
#endif
            AddEvent(adInstance.m_adType, AdEvent.Showing, adInstance);
        }

        private void OnAdClosed(UnityAdInstanceData adInstance,object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] UnityAds Closed. {adInstance.m_adType}");
#endif
            if (adInstance.m_adType != AdType.Banner)
                adInstance.State = AdState.Unavailable;
            AddEvent(adInstance.m_adType, AdEvent.Hiding, adInstance);
        }

        private void OnAdClicked(UnityAdInstanceData adInstance,object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] UnityAds Clicked.");
#endif
            AddEvent(adInstance.m_adType, AdEvent.Clicked, adInstance);
        }
        
        private void OnAdFailedShow(UnityAdInstanceData adInstance,object sender, ShowErrorEventArgs args)
        {
            if (adInstance.m_adType != AdType.Banner)
                adInstance.State = AdState.Unavailable;
            AddEvent(adInstance.m_adType, AdEvent.Hiding, adInstance);
        }

        private void OnAdFailedLoad(UnityAdInstanceData adInstance, object sender, LoadErrorEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] UnityAds Failed to load ad. {args.Message}");
#endif
            adInstance.State = AdState.Unavailable;
            AddEvent(adInstance.m_adType, AdEvent.PreparationFailed, adInstance);
        }
        
        private void OnAdUserRewarded(UnityAdInstanceData adInstance, object sender, RewardEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] UnityAds User Rewarded ad.");
#endif
            AddEvent(adInstance.m_adType, AdEvent.IncentivizationCompleted, adInstance);
        }
        
        private void OnAdRefreshed(UnityAdInstanceData adInstance,object sender, LoadErrorEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] UnityAds Refreshed. {adInstance.m_adType}");
#endif
        }
        
        #endregion // Callback Event Methods
#endif
    }
}