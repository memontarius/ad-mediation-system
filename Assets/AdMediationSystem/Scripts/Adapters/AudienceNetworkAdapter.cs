
#define _MS_AUDIENCE_NETWORK

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.JSON;

#if _MS_AUDIENCE_NETWORK
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
        //public const string _NATIVE_REFRESH_TIME_KEY = "nativeRefreshTime";
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

        [System.Serializable]
        public struct AudienceNetworkIDs
        {
            public string m_bannerId;
            public string m_interstitialId;
            public string m_rewardVideoId;
            public string m_nativeId;
        }

        [SerializeField]
        public AudienceNetworkIDs m_defaultAndroidIds;
        [SerializeField]
        public AudienceNetworkIDs m_defaultIOSIds;
        [Tooltip("In Seconds")]
        public float m_defaultBannerRefreshTime = 60f;
        [Tooltip("In Seconds")]
        public float m_defaultNativeRefreshTime = 60f;

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

#if _MS_AUDIENCE_NETWORK

        class AudienceNetworkAdInstanceData : AdInstanceData
        {
            public AudienceNetworkAdInstanceData() : base()
            {
            }
            public AudienceNetworkAdInstanceData(AdType adType, string adID, string adInstanceName = AdInstanceData._AD_INSTANCE_DEFAULT_NAME) :
                base(adType, adID, adInstanceName)
            {
            }
            public Coroutine m_procRefresh;
            public float m_refreshTime;
            public bool m_isServerValidation; // Is S2S validation
        }

        string m_bannerId;
        string m_interstitialId;
        string m_rewardVideoId;

        // Default instances
        AudienceNetworkAdInstanceData m_bannerAdInstance;
        AudienceNetworkAdInstanceData m_interstitialAdInstance;
        AudienceNetworkAdInstanceData m_rewardVideoAdInstance;

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);

            float bannerRefreshTime = m_defaultBannerRefreshTime;
            float nativeRefreshTime = m_defaultNativeRefreshTime;

            if (parameters != null)
            {
                if (!parameters.TryGetValue(_BANNER_ID_KEY, out m_bannerId))
                {
                    m_bannerId = "";
                }
                if (!parameters.TryGetValue(_INTERSTITIAL_ID_KEY, out m_interstitialId))
                {
                    m_interstitialId = "";
                }
                if (!parameters.TryGetValue(_REWARDED_ID_KEY, out m_rewardVideoId))
                {
                    m_rewardVideoId = "";
                }

                if (parameters.ContainsKey(_BANNER_REFRESH_TIME_KEY))
                {
                    bannerRefreshTime = Convert.ToInt32(parameters[_BANNER_REFRESH_TIME_KEY]);
                }
            }
            else
            {
#if UNITY_ANDROID
                m_bannerId = m_defaultAndroidIds.m_bannerId;
                m_interstitialId = m_defaultAndroidIds.m_interstitialId;
                m_rewardVideoId = m_defaultAndroidIds.m_rewardVideoId;
#elif UNITY_IOS
                m_bannerId = m_defaultIOSIds.m_bannerId;
                m_interstitialId = m_defaultIOSIds.m_interstitialId;
                m_rewardVideoId = m_defaultIOSIds.m_rewardVideoId;
#endif
            }

            if (IsSupported(AdType.Banner))
            {
                m_bannerAdInstance = new AudienceNetworkAdInstanceData(AdType.Banner, m_bannerId);
                m_bannerAdInstance.m_adInstanceParams = GetAdInstanceParams(AdType.Banner, AdInstanceData._AD_INSTANCE_DEFAULT_NAME);

                if (bannerRefreshTime > 0)
                {
                    m_bannerAdInstance.m_refreshTime = bannerRefreshTime;
                }
                AddAdInstance(m_bannerAdInstance);
            }

            if (IsSupported(AdType.Interstitial))
            {
                m_interstitialAdInstance = new AudienceNetworkAdInstanceData(AdType.Interstitial, m_interstitialId);
                AddAdInstance(m_interstitialAdInstance);
            }

            if (IsSupported(AdType.Incentivized))
            {
                m_rewardVideoAdInstance = new AudienceNetworkAdInstanceData(AdType.Incentivized, m_rewardVideoId);
                m_rewardVideoAdInstance.m_isServerValidation = m_isDefaultServerValidation;
                AddAdInstance(m_rewardVideoAdInstance);
            }

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

            if (jsonAdInstance.Obj.ContainsKey(_REFRESH_TIME_KEY))
            {
                audienceNetworkAdInstance.m_refreshTime = Convert.ToInt32(jsonAdInstance.Obj.GetNumber(_REFRESH_TIME_KEY));
            }
        }

        protected override AdInstanceData CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            AdInstanceData adInstance = new AudienceNetworkAdInstanceData();
            return adInstance;
        }

        public override void Prepare(AdType adType, AdInstanceData adInstance = null)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;

            if (GetAdState(adType, audienceNetworkAdInstance) != AdState.Loading)
            {
                switch (adType)
                {
                    case AdType.Banner:
                        RequestBanner(audienceNetworkAdInstance);
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

        public override bool Show(AdType adType, AdInstanceData adInstance = null)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance == null ? null : adInstance as AudienceNetworkAdInstanceData;

            bool isShowSuccessful = false;
            switch (adType)
            {
                case AdType.Banner:
                    audienceNetworkAdInstance.m_isBannerAdTypeVisibled = true;

                    if (GetAdState(adType, audienceNetworkAdInstance) == AdState.Received)
                    {
                        AdView bannerView = audienceNetworkAdInstance.m_adView as AdView;
                        Vector2 bannerPosition = audienceNetworkAdInstance.m_bannerCoordinates;
                        isShowSuccessful = bannerView.Show(bannerPosition.x, bannerPosition.y);
                        if (isShowSuccessful)
                        {
                            NotifyEvent(adType, AdEvent.Show, audienceNetworkAdInstance);
                        }
                    }
                    break;
                case AdType.Interstitial:
                    if (GetAdState(adType, audienceNetworkAdInstance) == AdState.Received)
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
                    if (GetAdState(adType, audienceNetworkAdInstance) == AdState.Received)
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

        public override void Hide(AdType adType, AdInstanceData adInstance = null)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;

            switch (adType)
            {
                case AdType.Banner:
                    //case AdType.Native:
                    audienceNetworkAdInstance.m_isBannerAdTypeVisibled = false;

                    if (adType == AdType.Banner)
                    {
                        if (GetAdState(AdType.Banner, audienceNetworkAdInstance) == AdState.Received)
                        {
                            AdView bannerView = adInstance.m_adView as AdView;
                            bannerView.Show(-10000);
                        }
                    }
                    NotifyEvent(adType, AdEvent.Hide, audienceNetworkAdInstance);
                    break;
            }
        }

        public override void HideBannerTypeAdWithoutNotify(AdType adType, AdInstanceData adInstance = null)
        {
            AudienceNetworkAdInstanceData audienceNetworkAdInstance = adInstance as AudienceNetworkAdInstanceData;
            if (audienceNetworkAdInstance == null)
            {
                return;
            }
            audienceNetworkAdInstance.m_isBannerAdTypeVisibled = false;

            switch (adType)
            {
                case AdType.Banner:
                    if (GetAdState(adType, audienceNetworkAdInstance) == AdState.Received)
                    {
                        AdView bannerView = adInstance.m_adView as AdView;
                        bannerView.Show(-10000);
                    }
                    break;
            }
        }

        public override bool IsReady(AdType adType, AdInstanceData adInstance = null)
        {
            bool isReady = GetAdState(adType, adInstance) == AdState.Received;
            return isReady;
        }

        AdSize ConvertToAdSize(AudienceNetworkBannerSize bannerSize)
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

        void CalculateBannerPosition(AudienceNetworkAdInstanceData adInstance)
        {
            float bannerHight = 0;
            AudienceNetworkAdInstanceBannerParameters adInstanceParams = adInstance.m_adInstanceParams as AudienceNetworkAdInstanceBannerParameters;

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

            Vector2 bannerCoordinates = Vector2.zero;

            switch (adInstanceParams.m_bannerPosition)
            {
                case AudienceNetworkBannerPosition.Bottom:
                    bannerCoordinates.x = 0f;
#if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
                        bannerCoordinates.y = (float)AudienceNetwork.Utility.AdUtility.Height() - bannerHight;
#endif
                    break;
                case AudienceNetworkBannerPosition.Top:
                    bannerCoordinates.x = 0f;
                    bannerCoordinates.y = 0f;
                    break;
            }

            adInstance.m_bannerCoordinates = bannerCoordinates;
        }

        IEnumerator ProcRefreshBannerAdType(AdType adType, AudienceNetworkAdInstanceData adInstance, float refreshTime)
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
                    switch (adType)
                    {
                        case AdType.Banner:
                            lifeTime = 0.0f;
                            if (adInstance.m_adView != null)
                            {
                                AdView adView = adInstance.m_adView as AdView;
                                adView.LoadAd();
                            }
                            break;
                    }
                }
            }
        }

        void RequestBanner(AudienceNetworkAdInstanceData adInstance)
        {
            DestroyBanner(adInstance);

            CalculateBannerPosition(adInstance);

            SetAdState(AdType.Banner, adInstance, AdState.Loading);

            adInstance.m_procRefresh = StartCoroutine(ProcRefreshBannerAdType(AdType.Banner, adInstance, adInstance.m_refreshTime));

            if (adInstance.m_adView != null)
            {
                AdView currBannerView = adInstance.m_adView as AdView;
                currBannerView.LoadAd();
                return;
            }

#if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
                AudienceNetworkAdInstanceBannerParameters adInstanceParams = adInstance.m_adInstanceParams as AudienceNetworkAdInstanceBannerParameters;
                AdView bannerView = new AdView(adInstance.m_adID, ConvertToAdSize(adInstanceParams.m_bannerSize));
                adInstance.m_adView = bannerView;
                bannerView.Register(this.gameObject);
 
                bannerView.AdViewDidLoad += delegate { BannerAdViewDidLoad(adInstance); };
                bannerView.AdViewDidFailWithError += delegate(string error) { BannerAdViewDidFailWithError(adInstance, error); };
                bannerView.AdViewWillLogImpression += delegate { BannerAdViewWillLogImpression(adInstance); };
                bannerView.AdViewDidClick += delegate { BannerAdViewDidClick(adInstance); };
                bannerView.LoadAd();

#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("AudienceNetworkAdapter.RequestBanner()");
#endif
#endif
        }

        void DestroyBanner(AudienceNetworkAdInstanceData adInstance)
        {
            if (adInstance.m_procRefresh != null)
            {
                StopCoroutine(adInstance.m_procRefresh);
                adInstance.m_procRefresh = null;
            }

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
            SetAdState(AdType.Banner, adInstance, AdState.Uncertain);
        }

        void RequestInterstitial(AudienceNetworkAdInstanceData adInstance)
        {
            DestroyInterstitial(adInstance);
            SetAdState(AdType.Interstitial, adInstance, AdState.Loading);
#if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
            InterstitialAd interstitialAd = new InterstitialAd(adInstance.m_adID);
            interstitialAd.Register(this.gameObject);

            interstitialAd.InterstitialAdDidLoad += delegate { InterstitialAdDidLoad(adInstance); };
            interstitialAd.InterstitialAdDidFailWithError += delegate (string error) { InterstitialAdDidFailWithError(adInstance, error); };
            interstitialAd.InterstitialAdDidClose += delegate { InterstitialAdDidClose(adInstance); };
            interstitialAd.InterstitialAdDidClick += delegate { InterstitialAdDidClick(adInstance); };

            // Initiate the request to load the ad.
            interstitialAd.LoadAd();
            adInstance.m_adView = interstitialAd;

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RequestInterstitial()");
#endif
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
                Debug.Log("AudienceNetworkAdapter.DestroyInterstitial() state:" + GetAdState(AdType.Interstitial, adInstance));
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
            SetAdState(AdType.Interstitial, adInstance, AdState.Uncertain);
        }

        void RequestRewardVideo(AudienceNetworkAdInstanceData adInstance)
        {
            DestroyRewardVideo(adInstance);
            SetAdState(AdType.Incentivized, adInstance, AdState.Loading);

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RequestRewardVideo()");
#endif

#if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
                RewardedVideoAd rewardVideo = new RewardedVideoAd(adInstance.m_adID);
                adInstance.m_adView = rewardVideo;
                rewardVideo.Register(this.gameObject);

                rewardVideo.RewardedVideoAdDidLoad += delegate { RewardedVideoAdDidLoad(adInstance); };
                rewardVideo.RewardedVideoAdDidFailWithError += delegate(string error) { RewardedVideoAdDidFailWithError(adInstance, error); };
                rewardVideo.RewardedVideoAdDidClick += delegate { RewardedVideoAdDidClick(adInstance); };
                rewardVideo.RewardedVideoAdDidClose += delegate { RewardedVideoAdDidClose(adInstance); };

                if (adInstance.m_isServerValidation) {
                    // For S2S validation you need to register the following two callback
                    rewardVideo.RewardedVideoAdDidSucceed += delegate { RewardedVideoAdDidSucceed(adInstance); };
                    rewardVideo.RewardedVideoAdDidFail += delegate { RewardedVideoAdDidFail(adInstance); };
                }
                else {
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
            SetAdState(AdType.Incentivized, adInstance, AdState.Uncertain);
        }

        //------------------------------------------------------------------------
        #region Banner callback handlers

        void BannerAdViewDidLoad(AudienceNetworkAdInstanceData adInstance)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.BannerAdViewDidLoad() adInstanceName:" + adInstance.Name);
#endif

            SetAdState(AdType.Banner, adInstance, AdState.Received);
            if (adInstance.m_isBannerAdTypeVisibled && adInstance.m_adView != null)
            {
                AudienceNetworkAdInstanceBannerParameters adInstanceParams = adInstance.m_adInstanceParams as AudienceNetworkAdInstanceBannerParameters;
                AdView bannerView = adInstance.m_adView as AdView;
                Vector2 bannerPosition = adInstance.m_bannerCoordinates;
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
            AddEvent(AdType.Banner, AdEvent.PrepareFailure, adInstance);
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
            SetAdState(AdType.Interstitial, adInstance, AdState.Received);
            AddEvent(AdType.Interstitial, AdEvent.Prepared, adInstance);
        }

        void InterstitialAdDidFailWithError(AudienceNetworkAdInstanceData adInstance, string error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.InterstitialAdDidFailWithError() error: " + error);
#endif
            DestroyInterstitial(adInstance);
            AddEvent(AdType.Interstitial, AdEvent.PrepareFailure, adInstance);
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
            SetAdState(AdType.Incentivized, adInstance, AdState.Received);
            AddEvent(AdType.Incentivized, AdEvent.Prepared, adInstance);
        }

        void RewardedVideoAdDidFailWithError(AudienceNetworkAdInstanceData adInstance, string error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AudienceNetworkAdapter.RewardedVideoAdDidFailWithError() error: " + error);
#endif
            DestroyRewardVideo(adInstance);
            AddEvent(AdType.Incentivized, AdEvent.PrepareFailure, adInstance);
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

#endif // _MS_AUDIENCE_NETWORK

    }

} // namespace Virterix.AdMediation

