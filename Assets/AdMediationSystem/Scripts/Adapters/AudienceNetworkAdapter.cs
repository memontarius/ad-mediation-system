#define _AMS_AUDIENCE_NETWORK

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
            BANNER_HEIGHT_50,
            BANNER_HEIGHT_90,
            RECTANGLE_HEIGHT_250,
            CUSTOM
        }

        public enum AudienceNetworkBannerPosition
        {
            Bottom,
            Top
        }

        [Tooltip("In Seconds")]
        public float m_defaultBannerRefreshTime = 60f;
        
        public bool m_isDefaultServerValidation;
        public bool m_isAddTestDevices = false;
        public string m_testDeviceId;

        protected override string AdInstanceParametersFolder
        {
            get
            {
                return AudienceNetworkAdInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER;
            }
        }

#if _AMS_AUDIENCE_NETWORK
        class AudienceNetworkAdInstanceData : AdInstanceData
        {
            public AudienceNetworkAdInstanceData() : base()
            {
            }
            public AudienceNetworkAdInstanceData(AdType adType, string adID, string adInstanceName = AdInstanceData._AD_INSTANCE_DEFAULT_NAME) :
                base(adType, adID, adInstanceName)
            {
            }

            public Vector2 GetBannerPosition(string placement)
            {
                Vector2 nativeBannerCoordinates = Vector2.zero;
                var adMobAdInstanceParams = m_adInstanceParams as AudienceNetworkAdInstanceBannerParameters;
                var bannerPosition = adMobAdInstanceParams.m_bannerPositions.FirstOrDefault(p => p.m_placementName == placement);
                nativeBannerCoordinates = CalculateBannerPosition(this, placement);
                return nativeBannerCoordinates;
            }

            public Coroutine m_coroutineRefresh;
            public float m_refreshTime;
            public bool m_isServerValidation; // Is S2S validation
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);
            if (m_isAddTestDevices)
            {
                AdSettings.AddTestDevice(m_testDeviceId);
            }
        }

        protected override void InitializeAdInstanceData(AdInstanceData adInstance, JSONValue jsonAdInstance)
        {
            base.InitializeAdInstanceData(adInstance, jsonAdInstance);

            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;
            audienceNetworkAdInstance.m_refreshTime = m_defaultBannerRefreshTime;

            if (adInstance.m_adType == AdType.Banner)
            {
                if (jsonAdInstance.Obj.ContainsKey(_REFRESH_TIME_KEY))
                {
                    audienceNetworkAdInstance.m_refreshTime = Convert.ToInt32(jsonAdInstance.Obj.GetNumber(_REFRESH_TIME_KEY));
                }
            }
        }

        protected override AdInstanceData CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            AdInstanceData adInstance = new AudienceNetworkAdInstanceData();
            return adInstance;
        }

        public override void Prepare(AdInstanceData adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;
            AdType adType = adInstance.m_adType;

            if (adInstance.m_state != AdState.Loading)
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

        public override bool Show(AdInstanceData adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance == null ? null : adInstance as AudienceNetworkAdInstanceData;
            AdType adType = adInstance.m_adType;

            bool isShowSuccessful = false;
            switch (adType)
            {
                case AdType.Banner:
                    audienceNetworkAdInstance.m_isBannerAdTypeVisibled = true;
                    if (adInstance.m_state == AdState.Received)
                    {
                        AdView bannerView = audienceNetworkAdInstance.m_adView as AdView;
                        Vector2 bannerPosition = CalculateBannerPosition(audienceNetworkAdInstance, placement);

                        isShowSuccessful = bannerView.Show(bannerPosition.x, bannerPosition.y);
                        if (isShowSuccessful)
                        {
                            NotifyEvent(adType, AdEvent.Show, audienceNetworkAdInstance);
                        }
                    }
                    break;
                case AdType.Interstitial:
                    if (adInstance.m_state == AdState.Received)
                    {
                        InterstitialAd interstitialAd = audienceNetworkAdInstance.m_adView as InterstitialAd;
                        isShowSuccessful = interstitialAd.Show();
                        if (isShowSuccessful)
                        {
                            NotifyEvent(AdType.Interstitial, AdEvent.Show, audienceNetworkAdInstance);
                        }
                        else
                        {
                            DestroyInterstitial(audienceNetworkAdInstance);
                        }
                    }
                    break;
                case AdType.Incentivized:
                    if (adInstance.m_state == AdState.Received)
                    {
                        RewardedVideoAd rewardVideo = audienceNetworkAdInstance.m_adView as RewardedVideoAd;
                        isShowSuccessful = rewardVideo.Show();
                        if (isShowSuccessful)
                        {
                            NotifyEvent(AdType.Incentivized, AdEvent.Show, audienceNetworkAdInstance);
                        }
                        else
                        {
                            DestroyRewardVideo(audienceNetworkAdInstance);
                        }
                    }
                    break;
            }
            return isShowSuccessful;
        }

        public override void Hide(AdInstanceData adInstance = null)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;
            AdType adType = adInstance.m_adType;

            switch (adType)
            {
                case AdType.Banner:
                    //case AdType.Native:
                    audienceNetworkAdInstance.m_isBannerAdTypeVisibled = false;

                    if (adType == AdType.Banner)
                    {
                        if (adInstance.m_state == AdState.Received)
                        {
                            AdView bannerView = adInstance.m_adView as AdView;
                            bannerView.Show(-10000);
                        }
                    }
                    NotifyEvent(adType, AdEvent.Hide, audienceNetworkAdInstance);
                    break;
            }
        }

        public override void HideBannerTypeAdWithoutNotify(AdInstanceData adInstance = null)
        {
            AdType adType = adInstance.m_adType;
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;
            if (audienceNetworkAdInstance == null)
            {
                return;
            }
            audienceNetworkAdInstance.m_isBannerAdTypeVisibled = false;

            switch (adType)
            {
                case AdType.Banner:
                    if (adInstance.m_state == AdState.Received)
                    {
                        AdView bannerView = adInstance.m_adView as AdView;
                        bannerView.Show(-10000);
                    }
                    break;
            }
        }

        public override bool IsReady(AdInstanceData adInstance = null)
        {
            AdType adType = adInstance.m_adType;
            bool isReady = adInstance.m_state == AdState.Received;
            return isReady;
        }

        private AdSize ConvertToAdSize(AudienceNetworkBannerSize bannerSize)
        {
            AdSize nativeAdSize = AdSize.BANNER_HEIGHT_50;
            switch (bannerSize)
            {
                case AudienceNetworkBannerSize.BANNER_HEIGHT_50:
                    nativeAdSize = AdSize.BANNER_HEIGHT_50;
                    break;
                case AudienceNetworkBannerSize.BANNER_HEIGHT_90:
                    nativeAdSize = AdSize.BANNER_HEIGHT_90;
                    break;
                case AudienceNetworkBannerSize.RECTANGLE_HEIGHT_250:
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
            var bannerPosition = adInstanceParams.m_bannerPositions.FirstOrDefault(p => p.m_placementName == placement);

            switch (adInstanceParams.m_bannerSize)
            {
                case AudienceNetworkBannerSize.BANNER_HEIGHT_50:
                    bannerHight = 50f;
                    break;
                case AudienceNetworkBannerSize.BANNER_HEIGHT_90:
                    bannerHight = 90f;
                    break;
                case AudienceNetworkBannerSize.RECTANGLE_HEIGHT_250:
                    bannerHight = 250f;
                    break;
            }

            switch (bannerPosition.m_bannerPosition)
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

        private IEnumerator CoroutineRefreshBanner(AudienceNetworkAdInstanceData adInstance, float refreshTime)
        {
            float lifeTime = 0.0f;
            float period = 0.5f;
            WaitForSecondsRealtime waitInstruction = new WaitForSecondsRealtime(period);
 
            while (true)
            {
                yield return waitInstruction;
                if (adInstance.m_state == AdState.Received && adInstance.m_isBannerAdTypeVisibled)
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
                                adInstance.m_state = AdState.Loading;
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
            adInstance.m_state = AdState.Loading;

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
            AdView bannerView = new AdView(adInstance.m_adId, ConvertToAdSize(adInstanceParams.m_bannerSize));
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
            adInstance.m_state = AdState.Uncertain;
        }

        void RequestInterstitial(AudienceNetworkAdInstanceData adInstance)
        {
            DestroyInterstitial(adInstance);
            adInstance.m_state = AdState.Loading;

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
                Debug.Log("AudienceNetworkAdapter.DestroyInterstitial() state:" + adInstance.m_state);
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
            adInstance.m_state = AdState.Uncertain;
        }

        void RequestRewardVideo(AudienceNetworkAdInstanceData adInstance)
        {
            DestroyRewardVideo(adInstance);
            adInstance.m_state = AdState.Loading;

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
            adInstance.m_state = AdState.Uncertain;
        }

        //------------------------------------------------------------------------
        #region Banner callback handlers

        void BannerAdViewDidLoad(AudienceNetworkAdInstanceData adInstance, string placement)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.BannerAdViewDidLoad() adInstanceName:" + adInstance.Name);
#endif

            adInstance.m_state = AdState.Received;
            if (adInstance.m_isBannerAdTypeVisibled && adInstance.m_adView != null)
            {
                AudienceNetworkAdInstanceBannerParameters adInstanceParams = adInstance.m_adInstanceParams as AudienceNetworkAdInstanceBannerParameters;
                AdView bannerView = adInstance.m_adView as AdView;
                Vector2 bannerPosition = CalculateBannerPosition(adInstance, placement);
                bool success = bannerView.Show(bannerPosition.x, bannerPosition.y);
            }

            AddEvent(AdType.Banner, AdEvent.Prepare, adInstance);
        }

        void BannerAdViewDidFailWithError(AudienceNetworkAdInstanceData adInstance, string error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.BannerAdViewDidFailWithError() adInstanceName:" + adInstance.Name + " error: " + error);
#endif

            DestroyBanner(adInstance);
            AddEvent(AdType.Banner, AdEvent.FailedPreparation, adInstance);
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
            adInstance.m_state = AdState.Received;
            AddEvent(AdType.Interstitial, AdEvent.Prepare, adInstance);
        }

        void InterstitialAdDidFailWithError(AudienceNetworkAdInstanceData adInstance, string error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.InterstitialAdDidFailWithError() error: " + error);
#endif
            DestroyInterstitial(adInstance);
            AddEvent(AdType.Interstitial, AdEvent.FailedPreparation, adInstance);
        }

        void InterstitialAdDidClose(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.InterstitialAdDidClose()");
#endif
            DestroyInterstitial(adInstance);
            AddEvent(AdType.Interstitial, AdEvent.Hide, adInstance);
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
            adInstance.m_state = AdState.Received;
            AddEvent(AdType.Incentivized, AdEvent.Prepare, adInstance);
        }

        void RewardedVideoAdDidFailWithError(AudienceNetworkAdInstanceData adInstance, string error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidFailWithError() error: " + error);
#endif
            DestroyRewardVideo(adInstance);
            AddEvent(AdType.Incentivized, AdEvent.FailedPreparation, adInstance);
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
            AddEvent(AdType.Incentivized, AdEvent.Hide, adInstance);
        }

        void RewardedVideoAdComplete(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdComplete()");
#endif
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedComplete, adInstance);
        }

        // S2S validation result
        void RewardedVideoAdDidSucceed(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidSucceed()");
#endif
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedComplete, adInstance);
        }

        // S2S validation result
        void RewardedVideoAdDidFail(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidFail()");
#endif
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedIncomplete, adInstance);
        }

        #endregion // Reward Video callback handlers

#endif // _AMS_AUDIENCE_NETWORK

    }

} // namespace Virterix.AdMediation

