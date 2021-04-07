#define _AMS_ADCOLONY

using UnityEngine;
using System.Collections.Generic;
using Boomlagoon.JSON;
using System.Linq;
#if _AMS_ADCOLONY
using AdColony;
#endif

namespace Virterix.AdMediation
{
    public class AdColonyAdapter : AdNetworkAdapter
    {
        public enum AdColonyAdSize
        {
            Banner = 0,
            MediumRectangle,
            Leaderboard,
            SKYSCRAPER
        }

        public enum AdColonyAdPosition
        {
            Top = 0,
            Bottom = 1,
            TopLeft = 2,
            TopRight = 3,
            BottomLeft = 4,
            BottomRight = 5,
            Center = 6
        }

        public bool m_useRewardVideoPrePopup;
        public bool m_useRewardVideoPostPopup;

        protected override string AdInstanceParametersFolder
        {
            get { return AdColonyAdInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER; }
        }

        public static AdSize ConvertToAdSize(AdColonyAdSize bannerSize)
        {
            AdSize nativeAdSize = AdSize.Banner;
            switch (bannerSize)
            {
                case AdColonyAdSize.Banner:
                    nativeAdSize = AdSize.Banner;
                    break;
                case AdColonyAdSize.Leaderboard:
                    nativeAdSize = AdSize.Leaderboard;
                    break;
                case AdColonyAdSize.MediumRectangle:
                    nativeAdSize = AdSize.MediumRectangle;
                    break;
                case AdColonyAdSize.SKYSCRAPER:
                    nativeAdSize = AdSize.SKYSCRAPER;
                    break;
            }
            return nativeAdSize;
        }

        public static AdPosition ConvertToAdPosition(AdColonyAdPosition bannerPosition)
        {
            AdPosition nativeAdPosition = (AdPosition)bannerPosition;
            return nativeAdPosition;
        }

#if _AMS_ADCOLONY
        bool m_isConfigured = false;

        private void OnEnable()
        {
            SubscribeAdEvents();
        }

        private new void OnDisable()
        {
            base.OnDisable();
            UnsubscribeAdEvents();
        }

        public static AdPosition GetBannerPosition(AdInstance adInstance, string placement)
        {
            AdPosition nativeBannerPosition = AdPosition.Bottom;          
            var adInstanceParams = adInstance.m_adInstanceParams as AdColonyAdInstanceBannerParameters;
            var bannerPosition = adInstanceParams.m_bannerPositions.FirstOrDefault(p => p.m_placementName == placement);
            nativeBannerPosition = ConvertToAdPosition(bannerPosition.m_bannerPosition);
            return nativeBannerPosition;
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonPlacements)
        {
            base.InitializeParameters(parameters, jsonPlacements);
#if UNITY_EDITOR
            m_isConfigured = true;
#endif
            string appId = parameters == null ? "" : parameters["appId"];
            Configure(appId);
        }

        private void Configure(string appId)
        {
            AppOptions appOptions = new AppOptions();
            appOptions.TestModeEnabled = AdMediationSystem.Instance.m_testModeEnabled;

            if (AdMediationSystem.Instance.m_isChildrenDirected)
                appOptions.SetPrivacyFrameworkRequired(AppOptions.COPPA, true);

            if (AdMediationSystem.UserPersonalisationConsent != PersonalisationConsent.Undefined)
            {
                appOptions.SetPrivacyFrameworkRequired(AppOptions.GDPR, true);
                string gdprConsent = AdMediationSystem.UserPersonalisationConsent == PersonalisationConsent.Accepted ? "1" : "0";
                appOptions.SetPrivacyConsentString(AppOptions.GDPR, gdprConsent);

                appOptions.SetPrivacyFrameworkRequired(AppOptions.CCPA, true);
                string ccpaConsent = AdMediationSystem.UserPersonalisationConsent == PersonalisationConsent.Accepted ? "1" : "0";
                appOptions.SetPrivacyConsentString(AppOptions.CCPA, ccpaConsent);
            }

            string[] zoneIDs = new string[m_adInstances.Count];
            for(int i = 0; i < m_adInstances.Count; i++)
            {
                zoneIDs[i] = m_adInstances[i].m_adId;
            }
#if !UNITY_EDITOR
            Ads.Configure(appId, appOptions, zoneIDs);
#endif
        }

        public override void Prepare(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (!m_isConfigured)
            {
                AddEvent(adInstance.m_adType, AdEvent.PreparationFailed, adInstance);
                return;
            }

            if (!IsReady(adInstance, placement) && adInstance.State != AdState.Loading)
            {
                RequestAd(adInstance, placement);
            }
        }

        public override bool Show(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (IsReady(adInstance.m_adType))
            {
                switch (adInstance.m_adType)
                {
                    case AdType.Interstitial:
                    case AdType.Incentivized:
                        var interstitial = adInstance.m_adView as InterstitialAd;
                        Ads.ShowAd(interstitial);
                        break;
                    case AdType.Banner:
                        Ads.ShowAdView(adInstance.m_adId);
                        break;
                }
                return true;
            }
            else
                return false;
        }

        public override bool IsReady(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            bool isReady = false;
            if (m_isConfigured)
            {
                switch (adInstance.m_adType)
                {
                    case AdType.Interstitial:
                    case AdType.Incentivized:
                        if (adInstance.State == AdState.Received)
                        {
                            var interstitial = adInstance.m_adView as InterstitialAd;
                            if (interstitial.Expired)
                                DestroyAd(adInstance);
                            else
                                isReady = true;
                        }
                        break;
                }
            }
            return isReady;
        }

        private void DestroyAd(AdInstance adInstance)
        {
            if (adInstance.m_adView != null)
            {
                if (adInstance.m_adType == AdType.Interstitial || adInstance.m_adType == AdType.Incentivized)
                {
                    InterstitialAd interstitial = adInstance.m_adView as InterstitialAd;
                    interstitial.DestroyAd();
                }
                else if (adInstance.m_adType == AdType.Banner)
                {
                    AdColonyAdView banner = adInstance.m_adView as AdColonyAdView;
                    banner.DestroyAdView();
                }
                adInstance.m_adView = null;
            }
            adInstance.State = AdState.Uncertain;
        }

        private void RequestAd(AdInstance adInstance, string placement)
        {
            DestroyAd(adInstance);

            adInstance.State = AdState.Loading;
            AdOptions adOptions = null;
            
#if !UNITY_EDITOR
            if (adInstance.m_adType == AdType.Banner)
            {
                AdPosition bannerPosition = GetBannerPosition(adInstance, placement);
                var adInstanceParams = adInstance.m_adInstanceParams as AdColonyAdInstanceBannerParameters;
                AdSize bannerSize = ConvertToAdSize(adInstanceParams.m_bannerSize);
                Ads.RequestAdView(adInstance.m_adId, bannerSize, bannerPosition, null);
            }
            else
            {
                if (adInstance.m_adType == AdType.Incentivized)
                {
                    adOptions = new AdOptions();
                    adOptions.ShowPrePopup = m_useRewardVideoPrePopup;
                    adOptions.ShowPostPopup = m_useRewardVideoPostPopup;
                }
                Ads.RequestInterstitialAd(adInstance.m_adId, adOptions);
            }
#endif
        }

        private void SubscribeAdEvents()
        {
            Ads.OnConfigurationCompleted += OnConfigurationCompleted;
            // Interstitial and rewarded
            Ads.OnOpened += OnOpened;
            Ads.OnClosed += OnClosed;
            Ads.OnRequestInterstitial += OnRequestInterstitial;
            Ads.OnRequestInterstitialFailed += OnRequestInterstitialFailed;
            Ads.OnRequestInterstitialFailedWithZone += OnRequestInterstitialFailedWithZone;
            Ads.OnRewardGranted += OnRewardGranted;
            Ads.OnExpiring += OnExpiring;
            // Banner
            Ads.OnAdViewLoaded += OnAdViewLoaded;
            Ads.OnAdViewFailedToLoad += OnAdViewFailedToLoad;
            Ads.OnAdViewOpened += OnAdViewOpened;
            Ads.OnAdViewClosed += OnAdViewClosed;
            Ads.OnAdViewClicked += OnAdViewClicked;
        }

        private void UnsubscribeAdEvents()
        {
            Ads.OnConfigurationCompleted -= OnConfigurationCompleted;
            // Interstitial and rewarded
            Ads.OnOpened -= OnOpened;
            Ads.OnClosed -= OnClosed;
            Ads.OnRequestInterstitial -= OnRequestInterstitial;
            Ads.OnRequestInterstitialFailed -= OnRequestInterstitialFailed;
            Ads.OnRequestInterstitialFailedWithZone += OnRequestInterstitialFailedWithZone;
            Ads.OnRewardGranted -= OnRewardGranted;
            Ads.OnExpiring -= OnExpiring;
            // Banner
            Ads.OnAdViewLoaded -= OnAdViewLoaded;
            Ads.OnAdViewFailedToLoad -= OnAdViewFailedToLoad;
            Ads.OnAdViewOpened -= OnAdViewOpened;
            Ads.OnAdViewClosed -= OnAdViewClosed;
            Ads.OnAdViewClicked -= OnAdViewClicked;
        }

        // ----------------------------------------------------------------
        #region Interstitial and Rewarded Callbacks

        private void OnConfigurationCompleted(List<Zone> zones)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdColonyAdapter OnConfigurationCompleted()");
#endif
            m_isConfigured = true;
        }

        private void OnOpened(InterstitialAd interstitial)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdColonyAdapter OnOpened()");
#endif
            var adInstance = GetAdInstanceByAdId(interstitial.ZoneId);
            AddEvent(adInstance.m_adType, AdEvent.Show, adInstance);
        }

        private void OnClosed(InterstitialAd interstitial)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdColonyAdapter OnClosed()");
#endif
            var adInstance = GetAdInstanceByAdId(interstitial.ZoneId);
            DestroyAd(adInstance);
            AddEvent(adInstance.m_adType, AdEvent.Hiding, adInstance);
        }

        private void OnRequestInterstitial(InterstitialAd interstitial)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdColonyAdapter OnRequestInterstitial()");
#endif
            var adInstance = GetAdInstanceByAdId(interstitial.ZoneId);
            adInstance.State = AdState.Received;
            adInstance.m_adView = interstitial;
            AddEvent(adInstance.m_adType, AdEvent.Prepared, adInstance);
        }

        private void OnRequestInterstitialFailed()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdColonyAdapter OnRequestInterstitialFailed()");
#endif
        }

        private void OnRequestInterstitialFailedWithZone(string zoneId)
        {
            var adInstance = GetAdInstanceByAdId(zoneId);
            DestroyAd(adInstance);
            AddEvent(adInstance.m_adType, AdEvent.PreparationFailed, adInstance);

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdColonyAdapter OnRequestInterstitialFailedWithZone() " + zoneId);
#endif
        }

        private void OnExpiring(InterstitialAd interstitial)
        {
            var adInstance = GetAdInstanceByAdId(interstitial.ZoneId);
            DestroyAd(adInstance);

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdColonyAdapter OnExpiring() " + interstitial.ZoneId);
#endif
        }

        private void OnRewardGranted(string zoneId, bool success, string name, int amount)
        {
            var adInstance = GetAdInstanceByAdId(zoneId);
            if (adInstance.m_adType == AdType.Incentivized)
            {
                AdEvent adEvent = success ? AdEvent.IncentivizedCompleted : AdEvent.IncentivizedUncompleted;
                AddEvent(adInstance.m_adType, adEvent, adInstance);
            }
        }

        #endregion // Interstitial and Rewarded Callbacks

        // ---------------------------------------------------------------- 
        #region Banner Callbacks

        private void OnAdViewLoaded(AdColonyAdView adView)
        {
            var adInstance = GetAdInstanceByAdId(adView.ZoneId);
            adInstance.State = AdState.Received;
            adInstance.m_adView = adView;

            Debug.Log("----AdColonyAdapter OnAdViewLoaded " + adView.Id + " " + adView.ZoneId);

            if (adInstance.m_adType == AdType.Banner && adInstance.m_bannerDisplayed)
                Ads.ShowAdView(adView.Id);

            AddEvent(adInstance.m_adType, AdEvent.Prepared, adInstance);
        }

        private void OnAdViewFailedToLoad(AdColonyAdView adView)
        {
            var adInstance = GetAdInstanceByAdId(adView.ZoneId);
            DestroyAd(adInstance);
            AddEvent(adInstance.m_adType, AdEvent.PreparationFailed, adInstance);
        }

        private void OnAdViewOpened(AdColonyAdView adView)
        {
            var adInstance = GetAdInstanceByAdId(adView.ZoneId);
            AddEvent(adInstance.m_adType, AdEvent.Show, adInstance);
        }

        private void OnAdViewClosed(AdColonyAdView adView)
        {
            var adInstance = GetAdInstanceByAdId(adView.ZoneId);
            AddEvent(adInstance.m_adType, AdEvent.Hiding, adInstance);
        }

        private void OnAdViewClicked(AdColonyAdView adView)
        {
            var adInstance = GetAdInstanceByAdId(adView.ZoneId);
            AddEvent(adInstance.m_adType, AdEvent.Click, adInstance);
        }

        #endregion // Banner Callbacks

#endif // _AMS_ADCOLONY
    }
} // namespace Virterix.AdMediation

