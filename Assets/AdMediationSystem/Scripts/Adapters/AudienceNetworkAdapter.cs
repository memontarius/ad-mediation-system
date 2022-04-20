//#define _AMS_AUDIENCE_NETWORK

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.JSON;
using System.Linq;

#if _AMS_AUDIENCE_NETWORK
using AudienceNetwork;
#endif

namespace Virterix.AdMediation
{
    public class AudienceNetworkAdapter : AdNetworkAdapter
    {
        public const string _BANNER_ID_KEY = "bannerId";
        public const string _INTERSTITIAL_ID_KEY = "interstitialId";
        public const string _REWARDED_ID_KEY = "rewardedId";
        public const string _BANNER_REFRESH_TIME_KEY = "bannerRefreshTime";
        public const string _REFRESH_TIME_KEY = "refreshTime";

        public enum AudienceNetworkBannerSize
        {
            BannerHeight50,
            BannerHeight90,
            RectangleHeight250,
            Custom
        }

        public enum AudienceNetworkBannerPosition
        {
            Bottom,
            Top
        }

        public bool m_isDefaultServerValidation = false;
   
        protected override string AdInstanceParametersFolder
        {
            get
            {
                return AudienceNetworkAdInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER;
            }
        }

        public static string GetSDKVersion()
        {
            string version = string.Empty;
#if UNITY_EDITOR && _AMS_AUDIENCE_NETWORK
            version = SdkVersion.Build;
#endif
            return version;
        }

#if _AMS_AUDIENCE_NETWORK
        private class AudienceNetworkAdInstanceData : AdInstance
        {
            public AudienceNetworkAdInstanceData(AdNetworkAdapter network) : base(network)
            {
            }

            public AudienceNetworkAdInstanceData(AdNetworkAdapter network, AdType adType, string adID, 
                string adInstanceName = AdInstance.AD_INSTANCE_DEFAULT_NAME) :
                base(network, adType, adID, adInstanceName)
            {
            }

            public Coroutine m_coroutineRefresh;
            public float m_refreshTime;
            public bool m_isServerValidation; // Is S2S validation
        }

        public static AdSize ConvertToNativeBannerSize(AudienceNetworkBannerSize bannerSize)
        {
            AdSize nativeAdSize = AdSize.BANNER_HEIGHT_50;
            switch (bannerSize)
            {
                case AudienceNetworkBannerSize.BannerHeight50:
                    nativeAdSize = AdSize.BANNER_HEIGHT_50;
                    break;
                case AudienceNetworkBannerSize.BannerHeight90:
                    nativeAdSize = AdSize.BANNER_HEIGHT_90;
                    break;
                case AudienceNetworkBannerSize.RectangleHeight250:
                    nativeAdSize = AdSize.RECTANGLE_HEIGHT_250;
                    break;
            }
            return nativeAdSize;
        }

        private static Vector2 CalculateBannerPosition(AudienceNetworkAdInstanceData adInstance, string placement)
        {
#if UNITY_EDITOR
            return Vector2.zero;
#endif
            Vector2 bannerCoordinates = Vector2.zero;
            float bannerHight = 0;
            AudienceNetworkAdInstanceBannerParameters adInstanceParams = adInstance.m_adInstanceParams as AudienceNetworkAdInstanceBannerParameters;
            var bannerPosition = (AudienceNetworkBannerPosition)GetBannerPosition(adInstance, placement);

            switch (adInstanceParams.m_bannerSize)
            {
                case AudienceNetworkBannerSize.BannerHeight50:
                    bannerHight = 50f;
                    break;
                case AudienceNetworkBannerSize.BannerHeight90:
                    bannerHight = 90f;
                    break;
                case AudienceNetworkBannerSize.RectangleHeight250:
                    bannerHight = 250f;
                    break;
            }

            switch (bannerPosition)
            {
                case AudienceNetworkBannerPosition.Bottom:
                    bannerCoordinates.x = 0f;
#if UNITY_IOS || UNITY_ANDROID
                    bannerCoordinates.y = (float)AudienceNetwork.Utility.AdUtility.Height() - bannerHight;
#endif
                    break;
                case AudienceNetworkBannerPosition.Top:
                    bannerCoordinates.x = 0f;
                    bannerCoordinates.y = 0f;
                    break;
            }
            return bannerCoordinates;
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);
            if (AdMediationSystem.Instance.IsTestModeEnabled)
            {
                foreach(string device in AdMediationSystem.Instance.TestDevices)
                {
                    AdSettings.AddTestDevice(device);
                }
            }
        }

        protected override void InitializeAdInstanceData(AdInstance adInstance, JSONValue jsonAdInstance)
        {
            base.InitializeAdInstanceData(adInstance, jsonAdInstance);

            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;
            audienceNetworkAdInstance.m_refreshTime = 900;

            if (adInstance.m_adType == AdType.Banner)
            {
                if (jsonAdInstance.Obj.ContainsKey(_REFRESH_TIME_KEY))
                {
                    audienceNetworkAdInstance.m_refreshTime = Convert.ToInt32(jsonAdInstance.Obj.GetNumber(_REFRESH_TIME_KEY));
                }
            }
        }

        protected override AdInstance CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            AdInstance adInstance = new AudienceNetworkAdInstanceData(this);
            return adInstance;
        }

        public override void Prepare(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;
            AdType adType = adInstance.m_adType;

            if (adInstance.State != AdState.Loading)
            {
                switch (adType)
                {
                    case AdType.Banner:
                        RequestBanner(audienceNetworkAdInstance, placement);
                        break;
                    case AdType.Interstitial:
                        RequestInterstitial(audienceNetworkAdInstance);
                        break;
                    case AdType.Incentivized:
                        RequestRewardVideo(audienceNetworkAdInstance);
                        break;
                }
            }
        }

        public override bool Show(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance == null ? null : adInstance as AudienceNetworkAdInstanceData;
            AdType adType = adInstance.m_adType;

            if (adType == AdType.Banner)
            {
                audienceNetworkAdInstance.m_bannerDisplayed = true;
                audienceNetworkAdInstance.CurrPlacement = placement;
            }
            
            bool isShowSuccessful = false;
            switch (adType)
            {
                case AdType.Banner:
                    audienceNetworkAdInstance.m_bannerDisplayed = true;
                    if (adInstance.State == AdState.Received)
                    {
                        AdView bannerView = audienceNetworkAdInstance.m_adView as AdView;
                        Vector2 bannerPosition = CalculateBannerPosition(audienceNetworkAdInstance, placement);

                        isShowSuccessful = bannerView.Show(bannerPosition.x, bannerPosition.y);
                        if (isShowSuccessful)
                            NotifyEvent(AdEvent.Show, audienceNetworkAdInstance);
                    }
                    break;
                case AdType.Interstitial:
                    if (adInstance.State == AdState.Received)
                    {
                        InterstitialAd interstitialAd = audienceNetworkAdInstance.m_adView as InterstitialAd;
                        isShowSuccessful = interstitialAd.Show();
                        if (isShowSuccessful)
                            NotifyEvent(AdEvent.Show, audienceNetworkAdInstance);
                        else
                            DestroyInterstitial(audienceNetworkAdInstance);
                    }
                    break;
                case AdType.Incentivized:
                    if (adInstance.State == AdState.Received)
                    {
                        RewardedVideoAd rewardVideo = audienceNetworkAdInstance.m_adView as RewardedVideoAd;
                        isShowSuccessful = rewardVideo.Show();
                        if (isShowSuccessful)
                            NotifyEvent(AdEvent.Show, audienceNetworkAdInstance);
                        else
                            DestroyRewardVideo(audienceNetworkAdInstance);
                    }
                    break;
            }
            return isShowSuccessful;
        }

        public override void Hide(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;
            AdType adType = adInstance.m_adType;

            switch (adType)
            {
                case AdType.Banner:
                    audienceNetworkAdInstance.m_bannerDisplayed = false;
                    if (adInstance.State == AdState.Received)
                    {
                        AdView bannerView = adInstance.m_adView as AdView;
                        bannerView.Show(-10000);
                    }
                    NotifyEvent(AdEvent.Hiding, audienceNetworkAdInstance);
                    break;
            }
        }

        public override bool IsReady(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            bool isReady = adInstance.State == AdState.Received;
            return isReady;
        }

        private IEnumerator CoroutineRefreshBanner(AudienceNetworkAdInstanceData adInstance, float refreshTime)
        {
            float lifeTime = 0.0f;
            float period = 0.5f;
            WaitForSecondsRealtime waitInstruction = new WaitForSecondsRealtime(period);
 
            while (true)
            {
                yield return waitInstruction;
                if (adInstance.State == AdState.Received && adInstance.m_bannerDisplayed)
                {
                    lifeTime += period;
                }

                if (lifeTime >= refreshTime)
                {
                    switch (adInstance.m_adType)
                    {
                        case AdType.Banner:
                            lifeTime = 0.0f;
                            if (adInstance.m_adView != null)
                            {
                                adInstance.State = AdState.Loading;
                                AdView adView = adInstance.m_adView as AdView;
                                adView.LoadAd();
                            }
                            break;
                    }
                }
            }
        }

        private void StartRefreshBannerProcess(AudienceNetworkAdInstanceData adInstance)
        {
            StopRefreshBannerProcess(adInstance);
            adInstance.m_coroutineRefresh = StartCoroutine(CoroutineRefreshBanner(adInstance, adInstance.m_refreshTime));
        }

        private void StopRefreshBannerProcess(AudienceNetworkAdInstanceData adInstance)
        {
            if (adInstance.m_coroutineRefresh != null)
            {
                StopCoroutine(adInstance.m_coroutineRefresh);
                adInstance.m_coroutineRefresh = null;
            }
        }

        void RequestBanner(AudienceNetworkAdInstanceData adInstance, string placement)
        {
            DestroyBanner(adInstance);
            adInstance.State = AdState.Loading;
            adInstance.CurrPlacement = placement;

            //StartRefreshBannerProcess(adInstance);

            if (adInstance.m_adView != null)
            {
                AdView currBannerView = adInstance.m_adView as AdView;
                currBannerView.LoadAd();
                return;
            }

#if UNITY_EDITOR
            return;
#endif

#if UNITY_IOS || UNITY_ANDROID
            AudienceNetworkAdInstanceBannerParameters adInstanceParams = adInstance.m_adInstanceParams as AudienceNetworkAdInstanceBannerParameters;
            AdView bannerView = new AdView(adInstance.m_adId, ConvertToNativeBannerSize(adInstanceParams.m_bannerSize));
            adInstance.m_adView = bannerView;
            bannerView.Register(this.gameObject);

            bannerView.AdViewDidLoad += delegate { BannerAdViewDidLoad(adInstance, placement); };
            bannerView.AdViewDidFailWithError += delegate (string error) { BannerAdViewDidFailWithError(adInstance, error); };
            bannerView.AdViewWillLogImpression += delegate { BannerAdViewWillLogImpression(adInstance); };
            bannerView.AdViewDidClick += delegate { BannerAdViewDidClick(adInstance); };
            bannerView.LoadAd();
#endif

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RequestBanner()");
#endif

        }

        void DestroyBanner(AudienceNetworkAdInstanceData adInstance)
        {
            StopRefreshBannerProcess(adInstance);

            if (adInstance.m_adView != null)
            {
                AdView bannerView = adInstance.m_adView as AdView;
                bannerView.Show(-10000);

                adInstance.m_adView = null;
                bannerView.AdViewDidLoad = null;
                bannerView.AdViewDidFailWithError = null;
                bannerView.AdViewWillLogImpression = null;
                bannerView.AdViewDidClick = null;

                try
                {
                    bannerView.Dispose();
                }
                catch (Exception exp)
                {
                    Debug.Log("AudienceNetworkAdapter.DestroyBanner() Catch error when Dispose: " + exp.Message);
                }

            }
            adInstance.State = AdState.Uncertain;
        }

        void RequestInterstitial(AudienceNetworkAdInstanceData adInstance)
        {
            DestroyInterstitial(adInstance);
            adInstance.State = AdState.Loading;

#if UNITY_EDITOR
            return;
#endif

#if UNITY_IOS || UNITY_ANDROID
            InterstitialAd interstitialAd = new InterstitialAd(adInstance.m_adId);
            interstitialAd.Register(this.gameObject);

            interstitialAd.InterstitialAdDidLoad += delegate { InterstitialAdDidLoad(adInstance); };
            interstitialAd.InterstitialAdDidFailWithError += delegate (string error) { InterstitialAdDidFailWithError(adInstance, error); };
            interstitialAd.InterstitialAdDidClose += delegate { InterstitialAdDidClose(adInstance); };
            interstitialAd.InterstitialAdDidClick += delegate { InterstitialAdDidClick(adInstance); };

            // Initiate the request to load the ad.
            interstitialAd.LoadAd();
            adInstance.m_adView = interstitialAd;
#endif

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RequestInterstitial()");
#endif
        }

        void DestroyInterstitial(AudienceNetworkAdInstanceData adInstance)
        {
            if (adInstance.m_adView != null)
            {
                InterstitialAd interstitialAd = adInstance.m_adView as InterstitialAd;
                adInstance.m_adView = null;

                interstitialAd.InterstitialAdDidLoad = null;
                interstitialAd.InterstitialAdDidFailWithError = null;
                interstitialAd.InterstitialAdDidClose = null;
                interstitialAd.InterstitialAdDidClick = null;

#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("AudienceNetworkAdapter.DestroyInterstitial() state:" + adInstance.State);
#endif

                //if (GetAdState(AdType.Interstitial, adInstance) == AdState.Loading) {
                try
                {
                    interstitialAd.Dispose();
                }
                catch (Exception exp)
                {
                    Debug.Log("AudienceNetworkAdapter.DestroyInterstitial() Catch error when Dispose: " + exp.Message);
                }
                //}
            }
            adInstance.State = AdState.Uncertain;
        }

        void RequestRewardVideo(AudienceNetworkAdInstanceData adInstance)
        {
            DestroyRewardVideo(adInstance);
            adInstance.State = AdState.Loading;

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RequestRewardVideo()");
#endif

#if UNITY_EDITOR
            return;
#endif

#if UNITY_IOS || UNITY_ANDROID
            RewardedVideoAd rewardVideo = new RewardedVideoAd(adInstance.m_adId);
            adInstance.m_adView = rewardVideo;
            rewardVideo.Register(this.gameObject);

            rewardVideo.RewardedVideoAdDidLoad += delegate { RewardedVideoAdDidLoad(adInstance); };
            rewardVideo.RewardedVideoAdDidFailWithError += delegate (string error) { RewardedVideoAdDidFailWithError(adInstance, error); };
            rewardVideo.RewardedVideoAdDidClick += delegate { RewardedVideoAdDidClick(adInstance); };
            rewardVideo.RewardedVideoAdDidClose += delegate { RewardedVideoAdDidClose(adInstance); };

            if (adInstance.m_isServerValidation)
            {
                // For S2S validation you need to register the following two callback
                rewardVideo.RewardedVideoAdDidSucceed += delegate { RewardedVideoAdDidSucceed(adInstance); };
                rewardVideo.RewardedVideoAdDidFail += delegate { RewardedVideoAdDidFail(adInstance); };
            }
            else
            {
                rewardVideo.RewardedVideoAdComplete += delegate { RewardedVideoAdComplete(adInstance); };
            }

            // Initiate the request to load the ad.
            rewardVideo.LoadAd();
#endif
        }

        void DestroyRewardVideo(AudienceNetworkAdInstanceData adInstance)
        {
#if UNITY_EDITOR
            return;
#endif

            if (adInstance.m_adView != null)
            {
                RewardedVideoAd rewardVideo = adInstance.m_adView as RewardedVideoAd;
                adInstance.m_adView = null;

                rewardVideo.RewardedVideoAdDidLoad = null;
                rewardVideo.RewardedVideoAdDidFailWithError = null;
                rewardVideo.RewardedVideoAdComplete = null;
                rewardVideo.RewardedVideoAdDidClick = null;
                rewardVideo.RewardedVideoAdDidClose = null;
                rewardVideo.RewardedVideoAdDidSucceed = null;
                rewardVideo.RewardedVideoAdDidFail = null;

                try
                {
                    rewardVideo.Dispose();
                }
                catch (Exception exp)
                {
                    Debug.Log("AudienceNetworkAdapter.DestroyRewardVideo() Catch error when Dispose: " + exp.Message + " trace: " +
                        exp.StackTrace);
                }
            }
            adInstance.State = AdState.Uncertain;
        }

        //------------------------------------------------------------------------
        #region Banner callback handlers

        void BannerAdViewDidLoad(AudienceNetworkAdInstanceData adInstance, string placement)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.BannerAdViewDidLoad() adInstanceName:" + adInstance.Name);
#endif

            adInstance.State = AdState.Received;
            if (adInstance.m_bannerDisplayed && adInstance.m_adView != null)
            {
                AudienceNetworkAdInstanceBannerParameters adInstanceParams = adInstance.m_adInstanceParams as AudienceNetworkAdInstanceBannerParameters;
                AdView bannerView = adInstance.m_adView as AdView;
                Vector2 bannerPosition = CalculateBannerPosition(adInstance, placement);
                bool success = bannerView.Show(bannerPosition.x, bannerPosition.y);
            }
            AddEvent(AdType.Banner, AdEvent.Prepared, adInstance);
        }

        void BannerAdViewDidFailWithError(AudienceNetworkAdInstanceData adInstance, string error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.BannerAdViewDidFailWithError() adInstanceName:" + adInstance.Name + " error: " + error);
#endif

            DestroyBanner(adInstance);
            AddEvent(AdType.Banner, AdEvent.PreparationFailed, adInstance);
        }

        void BannerAdViewWillLogImpression(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.BannerAdViewWillLogImpression()");
#endif
        }

        void BannerAdViewDidClick(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.BannerAdViewDidClick()");
#endif
            AddEvent(AdType.Banner, AdEvent.Click, adInstance);
        }

        #endregion //Banner callback handlers

        //------------------------------------------------------------------------
        #region Interstitial callback handlers

        void InterstitialAdDidLoad(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.InterstitialAdDidLoad()");
#endif
            adInstance.State = AdState.Received;
            AddEvent(AdType.Interstitial, AdEvent.Prepared, adInstance);
        }

        void InterstitialAdDidFailWithError(AudienceNetworkAdInstanceData adInstance, string error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.InterstitialAdDidFailWithError() error: " + error);
#endif
            DestroyInterstitial(adInstance);
            AddEvent(AdType.Interstitial, AdEvent.PreparationFailed, adInstance);
        }

        void InterstitialAdDidClose(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.InterstitialAdDidClose()");
#endif
            DestroyInterstitial(adInstance);
            AddEvent(AdType.Interstitial, AdEvent.Hiding, adInstance);
        }

        void InterstitialAdDidClick(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.InterstitialAdDidClick()");
#endif
            AddEvent(AdType.Interstitial, AdEvent.Click, adInstance);
        }

        #endregion // Interstitial callback handlers

        //------------------------------------------------------------------------
        #region Reward Video callback handlers

        void RewardedVideoAdDidLoad(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidLoad()");
#endif
            adInstance.State = AdState.Received;
            AddEvent(AdType.Incentivized, AdEvent.Prepared, adInstance);
        }

        void RewardedVideoAdDidFailWithError(AudienceNetworkAdInstanceData adInstance, string error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidFailWithError() error: " + error);
#endif
            DestroyRewardVideo(adInstance);
            AddEvent(AdType.Incentivized, AdEvent.PreparationFailed, adInstance);
        }

        void RewardedVideoAdDidClick(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidClick()");
#endif
            AddEvent(AdType.Incentivized, AdEvent.Click, adInstance);
        }

        void RewardedVideoAdDidClose(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidClose()");
#endif
            DestroyRewardVideo(adInstance);
            AddEvent(AdType.Incentivized, AdEvent.Hiding, adInstance);
        }

        void RewardedVideoAdComplete(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdComplete()");
#endif
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedCompleted, adInstance);
        }

        // S2S validation result
        void RewardedVideoAdDidSucceed(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidSucceed()");
#endif
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedCompleted, adInstance);
        }

        // S2S validation result
        void RewardedVideoAdDidFail(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidFail()");
#endif
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedUncompleted, adInstance);
        }

        #endregion // Reward Video callback handlers
#endif
    }
}

