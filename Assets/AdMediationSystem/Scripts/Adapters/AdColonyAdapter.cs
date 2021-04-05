#define _AMS_ADCOLONY

using UnityEngine;
using System.Collections.Generic;
using Boomlagoon.JSON;
#if _AMS_ADCOLONY
using AdColony;
#endif

namespace Virterix.AdMediation
{
    public class AdColonyAdapter : AdNetworkAdapter
    {
        public bool m_useRewardVideoPrePopup;
        public bool m_useRewardVideoPostPopup;

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

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonPlacements)
        {
            base.InitializeParameters(parameters, jsonPlacements);
#if UNITY_EDITOR
            m_isConfigured = true;
#endif
            string appId = parameters == null ? "" : parameters["appId"];
            ConfigureAds(appId);
        }

        private void ConfigureAds(string appId)
        {
            AppOptions appOptions = new AppOptions();
            appOptions.UserId = SystemInfo.deviceUniqueIdentifier;
            appOptions.TestModeEnabled = AdMediationSystem.Instance.m_testModeEnabled;
            if (AdMediationSystem.Instance.m_isChildrenDirected)
                appOptions.SetPrivacyFrameworkRequired(AdColony.AppOptions.COPPA, true);

            string[] zoneIDs = new string[0];

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
                switch (adInstance.m_adType)
                {
                    case AdType.Interstitial:
                    case AdType.Incentivized:
                        RequestAd(adInstance);
                        break;
                }
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
                InterstitialAd interstitial = adInstance.m_adView as InterstitialAd;
                adInstance.m_adView = null;
                interstitial.DestroyAd();
            }
            adInstance.State = AdState.Uncertain;
        }

        private void RequestAd(AdInstance adInstance)
        {
            DestroyAd(adInstance);

            adInstance.State = AdState.Loading;
            AdOptions adOptions = new AdColony.AdOptions();

#if !UNITY_EDITOR
            if (adInstance.m_adType == AdType.Banner)
            {
                //Ads.RequestAdView(adInstance.m_adId, adOptions);
            }
            else
            {
                if (adInstance.m_adType == AdType.Incentivized)
                {
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
            // Interstitial
            Ads.OnOpened += OnOpened;
            Ads.OnClosed += OnClosed;
            Ads.OnRequestInterstitial += OnRequestInterstitial;
            Ads.OnRequestInterstitialFailed += OnRequestInterstitialFailed;
            Ads.OnRequestInterstitialFailedWithZone += OnRequestInterstitialFailedWithZone;
            Ads.OnRewardGranted += OnRewardGranted;
            Ads.OnExpiring += OnExpiring;
            // Banner
        }

        private void UnsubscribeAdEvents()
        {
            Ads.OnConfigurationCompleted -= OnConfigurationCompleted;
            // Interstitial
            Ads.OnOpened -= OnOpened;
            Ads.OnClosed -= OnClosed;
            Ads.OnRequestInterstitial -= OnRequestInterstitial;
            Ads.OnRequestInterstitialFailed -= OnRequestInterstitialFailed;
            Ads.OnRequestInterstitialFailedWithZone += OnRequestInterstitialFailedWithZone;
            Ads.OnRewardGranted -= OnRewardGranted;
            Ads.OnExpiring += OnExpiring;
            // Banner
        }

        // _____________________________________________ 
        #region Interstitial Callbacks
 
        private void OnConfigurationCompleted(List<Zone> zones)
        {
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

        #endregion // Interstitial Callbacks

#endif // _AMS_ADCOLONY
    }
} // namespace Virterix.AdMediation

