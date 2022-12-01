#define _ADMOB_USE_MEDIATION
//#define _ADMOB_MEDIATION_FAN
//#define _ADMOB_MEDIATION_UNITYADS
#define _ADMOB_MEDIATION_APPLOVIN
//#define _ADMOB_MEDIATION_CHARTBOOST
#define _ADMOB_MEDIATION_ADCOLONY

using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

#if _ADMOB_USE_MEDIATION
using GoogleMobileAds.Api;

#if _ADMOB_MEDIATION_APPLOVIN
using GoogleMobileAds.Api.Mediation.AppLovin;
#endif
#if _ADMOB_MEDIATION_CHARTBOOST
using GoogleMobileAds.Api.Mediation.Chartboost;
#endif
#if _ADMOB_MEDIATION_ADCOLONY
using GoogleMobileAds.Api.Mediation.AdColony;
#endif
#if _ADMOB_MEDIATION_UNITYADS
using GoogleMobileAds.Api.Mediation.UnityAds;
#endif
#endif

namespace Virterix.AdMediation
{
    public class AdMobMediationBehavior
    {
#if _ADMOB_USE_MEDIATION
#if _ADMOB_MEDIATION_ADCOLONY
        private AdColonyMediationExtras _adColonyExtras;
#endif
#endif
        private bool _isIOSAppTrackingSupported;

        public AdMobMediationBehavior(AdMobAdapter adMob)
        {
            adMob.OnWillInitialize += OnAdMobWillInitialize;
#if _ADMOB_USE_MEDIATION
            adMob.OnInitializationComplete += OnAdMobInitializationComplete;
            adMob.OnAdRequest += OnAdRequest;

#if _ADMOB_MEDIATION_ADCOLONY
            _adColonyExtras = new AdColonyMediationExtras();
            _adColonyExtras.SetShowPrePopup(false);
            _adColonyExtras.SetShowPostPopup(false);
#endif
#endif
            AdMediationSystem.OnUserConsentToPersonalizedAdsChanged += OnUserConsentToPersonalizedAdsChanged;

#if UNITY_IOS && !UNITY_EDITOR
            float numericSystemVersion = 0;
            string systemVersion = UnityEngine.iOS.Device.systemVersion;
            var numberStyles = NumberStyles.Any;
            var culture = CultureInfo.InvariantCulture;
            if (float.TryParse(systemVersion, numberStyles, culture, out numericSystemVersion))
            {
                _isIOSAppTrackingSupported = numericSystemVersion >= 14f;
            }
#endif
        }

        private void UpdateUserPersonalizedAdsConsent()
        {
#if _ADMOB_USE_MEDIATION

            if (AdMediationSystem.UserPersonalisationConsent != PersonalisationConsent.Undefined)
            {
                bool userAccepts = AdMediationSystem.UserPersonalisationConsent == PersonalisationConsent.Accepted;

#if _ADMOB_MEDIATION_APPLOVIN
                AppLovin.SetHasUserConsent(userAccepts);
#endif

#if _ADMOB_MEDIATION_CHARTBOOST
                Chartboost.AddDataUseConsent(userAccepts ? CBGDPRDataUseConsent.Behavioral : CBGDPRDataUseConsent.NonBehavioral);
                Chartboost.AddDataUseConsent(userAccepts ? CBCCPADataUseConsent.OptInSale : CBCCPADataUseConsent.OptOutSale);
#endif

#if _ADMOB_MEDIATION_ADCOLONY
                AdColonyAppOptions.SetGDPRRequired(userAccepts);
                AdColonyAppOptions.SetGDPRConsentString(userAccepts ? "1" : "0");
#endif

#if _ADMOB_MEDIATION_UNITYADS
                UnityAds.SetGDPRConsentMetaData(userAccepts);
#endif

#if UNITY_IOS && !UNITY_EDITOR && _ADMOB_MEDIATION_FAN
                if (_isIOSAppTrackingSupported)
                    AudienceNetworkMediationUtils.AdSettings.SetAdvertiserTrackingEnabled(userAccepts);
#endif
            }

#if _ADMOB_MEDIATION_APPLOVIN
            if (AdMediationSystem.Instance.ChildrenMode != ChildDirectedMode.NotAssign)
            {
                AppLovin.SetIsAgeRestrictedUser(AdMediationSystem.Instance.ChildrenMode == ChildDirectedMode.Directed);
            }
#endif

#endif
        }

        private void OnAdMobWillInitialize()
        {
            UpdateUserPersonalizedAdsConsent();
        }

#if _ADMOB_USE_MEDIATION
        private void OnAdMobInitializationComplete(AdMobAdapter.InitializationStatusContainer initStatus)
        {
#if AD_MEDIATION_DEBUG_MODE
            Dictionary<string, AdapterStatus> map = initStatus._status.getAdapterStatusMap();
            foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
            {
                string className = keyValuePair.Key;
                AdapterStatus status = keyValuePair.Value;
                switch (status.InitializationState)
                {
                    case AdapterState.NotReady:
                        // The adapter initialization did not complete.
                        MonoBehaviour.print("Mediation Adapter: " + className + " not ready.");
                        break;
                    case AdapterState.Ready:
                        // The adapter was successfully initialized.
                        MonoBehaviour.print("Mediation Adapter: " + className + " is initialized.");
                        break;
                }
            }
#endif
        }
#endif
        private void OnUserConsentToPersonalizedAdsChanged()
        {
            UpdateUserPersonalizedAdsConsent();
        }

#if _ADMOB_USE_MEDIATION
        private void OnAdRequest(AdType adType, AdMobAdapter.AdRequestBuilderContainer requestBuilder)
        {
#if _ADMOB_MEDIATION_ADCOLONY
            if (adType == AdType.Incentivized)
            {
                requestBuilder._builder.AddMediationExtras(_adColonyExtras);
            }
#endif
        }
#endif
    }
}
