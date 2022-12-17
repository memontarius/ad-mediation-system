//#define _AMS_YANDEX_MOBILE_ADS

using System;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.JSON;
using UnityEngine;
#if _AMS_YANDEX_MOBILE_ADS
using YandexMobileAds;
using YandexMobileAds.Base;
#endif

namespace Virterix.AdMediation
{
    public class YandexMobileAdsAdapter : AdNetworkAdapter
    {
        public enum YandexBannerSize
        {
            Flexible,
            Sticky
        }
            
        public enum YandexBannerPosition
        {
            TopLeft,
            TopCenter,
            TopRight,
            CenterLeft,
            Center,
            CenterRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        public class YandexAdInstanceData : AdInstance
        {
            public YandexAdInstanceData(AdNetworkAdapter network) : base(network)
            {
            }

            public YandexAdInstanceData(AdNetworkAdapter network, AdType adType, string adID, string name = AdInstance.AD_INSTANCE_DEFAULT_NAME) :
                base(network, adType, adID, name)
            {
            }
            
            public float BannerRefreshTime;
            public Coroutine ProcessBannerRefreshing;
            public float BannerDisplayTime;
            public float BannerLastShowingTime;

            public bool IsBannerRefreshProcessRunning => ProcessBannerRefreshing != null;
            
#if _AMS_YANDEX_MOBILE_ADS
            public Interstitial Interstitial;
            public RewardedAd RewardedAd;
            public Banner Banner;
            public AdPosition BannerPosition;
            
            public EventHandler<EventArgs> OnLoaded;
            public EventHandler<AdFailureEventArgs> OnFailedLoad;
            public EventHandler<EventArgs> OnShowed;
            public EventHandler<AdFailureEventArgs> OnFailedShow;
            public EventHandler<EventArgs> OnClosed;
            public EventHandler<Reward> OnRewarded;
#endif
        }
        
#if _AMS_YANDEX_MOBILE_ADS
        public static AdPosition ConvertToNativeBannerPosition(int bannerPosition)
        {
            return ConvertToNativeBannerPosition((YandexBannerPosition)bannerPosition);
        }

        public static AdPosition ConvertToNativeBannerPosition(YandexBannerPosition bannerPosition)
        {
            AdPosition nativeBannerPosition = AdPosition.BottomCenter;
            switch(bannerPosition)
            {
                case YandexBannerPosition.Center:
                    nativeBannerPosition = AdPosition.Center;
                    break;
                case YandexBannerPosition.BottomCenter:
                    nativeBannerPosition = AdPosition.BottomCenter;
                    break;
                case YandexBannerPosition.BottomLeft:
                    nativeBannerPosition = AdPosition.BottomLeft;
                    break;
                case YandexBannerPosition.BottomRight:
                    nativeBannerPosition = AdPosition.BottomRight;
                    break;
                case YandexBannerPosition.CenterLeft:
                    nativeBannerPosition = AdPosition.CenterLeft;
                    break;
                case YandexBannerPosition.CenterRight:
                    nativeBannerPosition = AdPosition.CenterRight;
                    break;
                case YandexBannerPosition.TopCenter:
                    nativeBannerPosition = AdPosition.TopCenter;
                    break;
                case YandexBannerPosition.TopLeft:
                    nativeBannerPosition = AdPosition.TopLeft;
                    break;
                case YandexBannerPosition.TopRight:
                    nativeBannerPosition = AdPosition.TopRight;
                    break;
            } 
            return nativeBannerPosition;
        }
#endif

        protected override string AdInstanceParametersFolder =>
            YandexAdInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER;

        public override bool UseSingleBannerInstance => false;

        public override bool RequiredWaitingInitializationResponse => true;

        public static string GetSDKVersion()
        {
            string version = string.Empty;
#if UNITY_EDITOR && _AMS_YANDEX_MOBILE_ADS
            version = MobileAdsPackageInfo.PackageVersion;
#endif
            return version;
        }
        
        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);
        
#if _AMS_YANDEX_MOBILE_ADS && !UNITY_EDITOR
            if (AdMediationSystem.Instance.ChildrenMode != ChildDirectedMode.NotAssign)
                MobileAds.SetAgeRestrictedUser(AdMediationSystem.Instance.ChildrenMode == ChildDirectedMode.Directed);
#endif            
            foreach (AdMediator mediator in AdMediationSystem.Instance.BannerMediators)
            {
                for (int i = 0; i < mediator.TotalUnits; i++)
                {
                    AdUnit unit = mediator.GetUnit(i);
                    if (unit.AdInstance.LoadingOnStart && unit.AdNetwork == this && unit.AdInstance.State == AdState.Uncertain)
                    {
                        Prepare(unit.AdInstance, mediator.m_placementName);
                    }
                }
            }
        }

        protected override void InitializeAdInstanceData(AdInstance adInstance, JSONValue jsonAdInstance)
        {
            base.InitializeAdInstanceData(adInstance, jsonAdInstance);
            if (adInstance.LoadingOnStart && adInstance.m_adType != AdType.Banner)
                Prepare(adInstance, "");
        }

        public override void Prepare(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            adInstance.CurrPlacement = placement;
            
            if (!IsReady(adInstance) && adInstance.State != AdState.Loading)
            {
                adInstance.State = AdState.Loading;
                YandexAdInstanceData yandexAdInstance = (YandexAdInstanceData)adInstance;
                
#if _AMS_YANDEX_MOBILE_ADS && !UNITY_EDITOR
                switch (adType)
                {
                    case AdType.Interstitial:
                        if (yandexAdInstance.Interstitial == null)
                            CreateInterstitialAd(yandexAdInstance);
                        yandexAdInstance.Interstitial.LoadAd(CreateRequest());
                        break;
                    case AdType.Incentivized:
                        if (yandexAdInstance.RewardedAd == null)
                            CreateRewardAd(yandexAdInstance);
                        yandexAdInstance.RewardedAd.LoadAd(CreateRequest());
                        break;
                    case AdType.Banner:
                        if (yandexAdInstance.Banner == null)
                            CreateBannerAd(yandexAdInstance, placement);
                        yandexAdInstance.Banner.LoadAd(CreateRequest());
                        break;
                }
#endif
            }
        }

#if _AMS_YANDEX_MOBILE_ADS
        private AdRequest CreateRequest()
        {
            return new AdRequest.Builder().Build();
        }
#endif    

        public override bool Show(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            YandexAdInstanceData yandexAdInstance = (YandexAdInstanceData)adInstance;
            adInstance.CurrPlacement = placement;
            bool wasBannerDisplay = adInstance.m_bannerDisplayed;
            
            if (adType == AdType.Banner)
                adInstance.m_bannerDisplayed = true;
            
            if (IsReady(adInstance))
            {
                ShowYandexAd(yandexAdInstance);
                if (adType == AdType.Banner)
                {
                    if (!wasBannerDisplay)
                    {
                        yandexAdInstance.BannerLastShowingTime = Time.unscaledTime;
                        StartRefreshBannerProcess(yandexAdInstance);
                    }
                    AddEvent(adInstance.m_adType, AdEvent.Showing, adInstance);
                }
                return true;
            }
            return false;
        }
        
        public override void Hide(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (adInstance.m_adType == AdType.Banner)
            {
                YandexAdInstanceData yandexAdInstance = (YandexAdInstanceData)adInstance;
                bool wasBannerDisplay = yandexAdInstance.m_bannerDisplayed;
                yandexAdInstance.m_bannerDisplayed = false;
#if _AMS_YANDEX_MOBILE_ADS && !UNITY_EDITOR
                yandexAdInstance.Banner?.Hide();
#endif
                if (yandexAdInstance.IsBannerRefreshProcessRunning)
                {
                    StopRefreshBannerProcess(yandexAdInstance);
                    yandexAdInstance.BannerDisplayTime += Time.unscaledTime - yandexAdInstance.BannerLastShowingTime;
                }
                
                if (wasBannerDisplay)
                    NotifyEvent(AdEvent.Hiding, adInstance);
            }
        }
        
        public override bool IsReady(AdInstance adInstance, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
#if UNITY_EDITOR
            return true;
#endif
            bool isReady = false;
            if (adInstance != null)
            {
                YandexAdInstanceData yandexAdInstance = (YandexAdInstanceData)adInstance;
#if _AMS_YANDEX_MOBILE_ADS && !UNITY_EDITOR
                switch (adInstance.m_adType)
                {
                    case AdType.Interstitial:
                        isReady = yandexAdInstance.Interstitial != null && yandexAdInstance.Interstitial.IsLoaded();
                        break;
                    case AdType.Incentivized:
                        isReady = yandexAdInstance.RewardedAd != null && yandexAdInstance.RewardedAd.IsLoaded();
                        break;
                    case AdType.Banner:
                        isReady = yandexAdInstance.Banner != null && yandexAdInstance.State == AdState.Received;
                        break;
                }
#endif
            }
            return isReady;
        }
        
        protected override void SetUserConsentToPersonalizedAds(PersonalisationConsent consent)
        {
#if _AMS_YANDEX_MOBILE_ADS && !UNITY_EDITOR
            if (consent != PersonalisationConsent.Undefined)
                MobileAds.SetUserConsent(consent == PersonalisationConsent.Accepted);
#endif
        }
        
        protected override AdInstance CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            AdInstance adInstance = new YandexAdInstanceData(this);
            return adInstance;
        }

        private void ShowYandexAd(YandexAdInstanceData yandexAdInstance)
        {
#if _AMS_YANDEX_MOBILE_ADS && !UNITY_EDITOR        
            switch (yandexAdInstance.m_adType)
            {
                case AdType.Interstitial:
                    yandexAdInstance.Interstitial.Show();
                    break; 
                case AdType.Incentivized:
                    yandexAdInstance.RewardedAd.Show();
                    break;
                case AdType.Banner:
                    yandexAdInstance.Banner.Show();
                    break;
            }
#endif
        }
        
#if _AMS_YANDEX_MOBILE_ADS
        private Interstitial CreateInterstitialAd(YandexAdInstanceData yandexAdInstance)
        {
            DestroyInterstitialAd(yandexAdInstance);
            
            yandexAdInstance.Interstitial = new Interstitial(yandexAdInstance.m_adId);;
            yandexAdInstance.OnLoaded = delegate(object sender, EventArgs args)
            {
                OnAdLoaded(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnFailedLoad = delegate(object sender, AdFailureEventArgs args)
            {
                OnAdFailedToLoad(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnShowed = delegate(object sender, EventArgs args)
            {
                OnAdShowed(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnFailedShow = delegate(object sender, AdFailureEventArgs args)
            {
                OnAdFailedToShow(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnClosed = delegate(object sender, EventArgs args)
            { 
                OnAdClosed(yandexAdInstance, sender, args);
            };
            
            yandexAdInstance.Interstitial.OnInterstitialLoaded += yandexAdInstance.OnLoaded;
            yandexAdInstance.Interstitial.OnInterstitialFailedToLoad += yandexAdInstance.OnFailedLoad;
            yandexAdInstance.Interstitial.OnInterstitialShown += yandexAdInstance.OnShowed;
            yandexAdInstance.Interstitial.OnInterstitialFailedToShow += yandexAdInstance.OnFailedShow;
            yandexAdInstance.Interstitial.OnInterstitialDismissed += yandexAdInstance.OnClosed;
            return yandexAdInstance.Interstitial;
        }
        
        private void DestroyInterstitialAd(YandexAdInstanceData yandexAdInstance)
        {
            yandexAdInstance.State = AdState.Uncertain;
            if (yandexAdInstance.Interstitial == null)
                return;
            
            yandexAdInstance.Interstitial.OnInterstitialLoaded -= yandexAdInstance.OnLoaded;
            yandexAdInstance.Interstitial.OnInterstitialFailedToLoad -= yandexAdInstance.OnFailedLoad;
            yandexAdInstance.Interstitial.OnInterstitialShown -= yandexAdInstance.OnShowed;
            yandexAdInstance.Interstitial.OnInterstitialFailedToShow -= yandexAdInstance.OnFailedShow;
            yandexAdInstance.Interstitial.OnInterstitialDismissed -= yandexAdInstance.OnClosed;
            
            yandexAdInstance.Interstitial.Destroy();
            yandexAdInstance.Interstitial = null;
        }

        private RewardedAd CreateRewardAd(YandexAdInstanceData yandexAdInstance)
        {
            DestroyRewardAd(yandexAdInstance);
            
            yandexAdInstance.RewardedAd =  new RewardedAd(yandexAdInstance.m_adId);
            yandexAdInstance.OnLoaded = delegate(object sender, EventArgs args)
            {
                OnAdLoaded(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnFailedLoad = delegate(object sender, AdFailureEventArgs args)
            {
                OnAdFailedToLoad(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnShowed = delegate(object sender, EventArgs args)
            {
                OnAdShowed(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnFailedShow = delegate(object sender, AdFailureEventArgs args)
            {
                OnAdFailedToShow(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnClosed = delegate(object sender, EventArgs args)
            { 
                OnAdClosed(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnRewarded = delegate(object sender, Reward reward)
            { 
                OnUserRewarded(yandexAdInstance, sender, reward);
            };
            
            yandexAdInstance.RewardedAd.OnRewardedAdLoaded += yandexAdInstance.OnLoaded;
            yandexAdInstance.RewardedAd.OnRewardedAdFailedToLoad += yandexAdInstance.OnFailedLoad;
            yandexAdInstance.RewardedAd.OnRewardedAdShown += yandexAdInstance.OnShowed;
            yandexAdInstance.RewardedAd.OnRewardedAdFailedToShow += yandexAdInstance.OnFailedShow;
            yandexAdInstance.RewardedAd.OnRewardedAdDismissed += yandexAdInstance.OnClosed;
            yandexAdInstance.RewardedAd.OnRewarded += yandexAdInstance.OnRewarded;
            
            return yandexAdInstance.RewardedAd;
        }

        private void DestroyRewardAd(YandexAdInstanceData yandexAdInstance)
        {
            yandexAdInstance.State = AdState.Uncertain;
            if (yandexAdInstance.RewardedAd == null)
                return;
            
            yandexAdInstance.RewardedAd.OnRewardedAdLoaded -= yandexAdInstance.OnLoaded;
            yandexAdInstance.RewardedAd.OnRewardedAdFailedToLoad -= yandexAdInstance.OnFailedLoad;
            yandexAdInstance.RewardedAd.OnRewardedAdShown -= yandexAdInstance.OnShowed;
            yandexAdInstance.RewardedAd.OnRewardedAdFailedToShow -= yandexAdInstance.OnFailedShow;
            yandexAdInstance.RewardedAd.OnRewardedAdDismissed -= yandexAdInstance.OnClosed;
            yandexAdInstance.RewardedAd.OnRewarded -= yandexAdInstance.OnRewarded;
            
            yandexAdInstance.RewardedAd.Destroy();
            yandexAdInstance.RewardedAd = null;
        }

        private Banner CreateBannerAd(YandexAdInstanceData yandexAdInstance, string placementName)
        {
            DestroyBanner(yandexAdInstance);
            
            YandexAdInstanceBannerParameters bannerParams = (YandexAdInstanceBannerParameters)yandexAdInstance.m_adInstanceParams;
            yandexAdInstance.BannerPosition = ConvertToNativeBannerPosition(GetBannerPosition(yandexAdInstance, placementName));
            AdSize bannerMaxSize = null;
            
            if (bannerParams.m_bannerSize == YandexBannerSize.Flexible)
                bannerMaxSize = AdSize.FlexibleSize(GetScreenWidthDp(), bannerParams.m_maxHeight);
            else if (bannerParams.m_bannerSize == YandexBannerSize.Sticky)
                bannerMaxSize = AdSize.StickySize(GetScreenWidthDp());
            
            yandexAdInstance.BannerRefreshTime = bannerParams.m_refreshTime;
            yandexAdInstance.BannerDisplayTime = 0.0f;
            yandexAdInstance.Banner = new Banner(yandexAdInstance.m_adId, bannerMaxSize, yandexAdInstance.BannerPosition);
            yandexAdInstance.OnLoaded = delegate(object sender, EventArgs args)
            {
                OnAdLoaded(yandexAdInstance, sender, args);
            };
            yandexAdInstance.OnFailedLoad = delegate(object sender, AdFailureEventArgs args)
            {
                OnAdFailedToLoad(yandexAdInstance, sender, args);
            };
            
            yandexAdInstance.Banner.OnAdLoaded += yandexAdInstance.OnLoaded;
            yandexAdInstance.Banner.OnAdFailedToLoad += yandexAdInstance.OnFailedLoad;
            return yandexAdInstance.Banner;
        }
        
        private void DestroyBanner(YandexAdInstanceData yandexAdInstance)
        {
            yandexAdInstance.State = AdState.Uncertain;
            StopRefreshBannerProcess(yandexAdInstance);
            if (yandexAdInstance.Banner == null)
                return;
            
            yandexAdInstance.Banner.OnAdLoaded -= yandexAdInstance.OnLoaded;
            yandexAdInstance.Banner.OnAdFailedToLoad -= yandexAdInstance.OnFailedLoad;
            yandexAdInstance.Banner.Destroy();
            yandexAdInstance.Banner = null;
        }

        private void DestroyYandexAd(YandexAdInstanceData adInstance)
        {
            switch (adInstance.m_adType)
            {
                case AdType.Interstitial:
                    DestroyInterstitialAd(adInstance);
                    break;
                case AdType.Incentivized:
                    DestroyRewardAd(adInstance);
                    break;
                case AdType.Banner:
                    DestroyBanner(adInstance);
                    break;
            }
        }
        
        private int GetScreenWidthDp()
        {
            int screenWidth = (int)Screen.safeArea.width;
            return ScreenUtils.ConvertPixelsToDp(screenWidth);
        }
#endif
        
        private void StartRefreshBannerProcess(YandexAdInstanceData adInstance)
        {
            StopRefreshBannerProcess(adInstance);
            float timeToBannerRefresh = adInstance.BannerRefreshTime - adInstance.BannerDisplayTime;
            timeToBannerRefresh = Mathf.Clamp(timeToBannerRefresh, 1, adInstance.BannerRefreshTime);
            adInstance.ProcessBannerRefreshing = StartCoroutine(RefreshBanner(adInstance, timeToBannerRefresh));
        }

        private void StopRefreshBannerProcess(YandexAdInstanceData adInstance)
        {
            if (adInstance.ProcessBannerRefreshing != null)
            {
                StopCoroutine(adInstance.ProcessBannerRefreshing);
                adInstance.ProcessBannerRefreshing = null;
            }
        }
        
        private IEnumerator RefreshBanner(YandexAdInstanceData adInstance, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
#if _AMS_YANDEX_MOBILE_ADS && !UNITY_EDITOR
            CreateBannerAd(adInstance, adInstance.CurrPlacement);
#endif
            Prepare(adInstance, adInstance.CurrPlacement);
        }
        
        //_______________________________________________________________________________
        #region Callback Event Methods

#if _AMS_YANDEX_MOBILE_ADS
        private void OnAdLoaded(YandexAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] YandexMobileAds Loaded. {adInstance.m_adType} Displayed: {adInstance.m_bannerDisplayed}");
#endif
            adInstance.State = AdState.Received;
            if (adInstance.m_adType == AdType.Banner)
            {
                if (adInstance.m_bannerDisplayed)
                {
                    adInstance.Banner.Show();
                    adInstance.BannerLastShowingTime = Time.unscaledTime;
                    if (adInstance.ProcessBannerRefreshing == null)
                        StartRefreshBannerProcess(adInstance);
                }
                else
                    adInstance.Banner.Hide();
            }
            AddEvent(adInstance.m_adType, AdEvent.Prepared, adInstance);
        }

        private void OnAdFailedToLoad(YandexAdInstanceData adInstance, object sender, AdFailureEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] YandexMobileAds Failed To Load. {adInstance.m_adType} Message: {args.Message}");
#endif
            DestroyYandexAd(adInstance);
            AddEvent(adInstance.m_adType, AdEvent.PreparationFailed, adInstance);
        }

        private void OnAdShowed(YandexAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] YandexMobileAds Showed. {adInstance.m_adType}");
#endif
            AddEvent(adInstance.m_adType, AdEvent.Showing, adInstance);
        }

        private void OnAdClosed(YandexAdInstanceData adInstance, object sender, EventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] YandexMobileAds Closed. {adInstance.m_adType}");
#endif
            DestroyYandexAd(adInstance);
            AddEvent(adInstance.m_adType, AdEvent.Hiding, adInstance);
        }
        
        private void OnAdFailedToShow(YandexAdInstanceData adInstance, object sender, AdFailureEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] YandexMobileAds Failed To Show. {adInstance.m_adType} Message: {args.Message}");
#endif
            DestroyYandexAd(adInstance);
            AddEvent(adInstance.m_adType, AdEvent.PreparationFailed, adInstance);
        }
        
        private void OnUserRewarded(YandexAdInstanceData adInstance, object sender, Reward reward)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] YandexMobileAds User Rewarded. {reward.type}: {reward.amount}");
#endif
            AddEvent(adInstance.m_adType, AdEvent.IncentivizationCompleted, adInstance);
        }
#endif
        #endregion
    }
}