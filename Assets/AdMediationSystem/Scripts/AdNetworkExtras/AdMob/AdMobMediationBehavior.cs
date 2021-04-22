//#define _ADMOB_MEDIATION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if _ADMOB_MEDIATION
using GoogleMobileAds.Api;
using GoogleMobileAds.Api.Mediation.AppLovin;
using GoogleMobileAds.Api.Mediation.Chartboost;
using GoogleMobileAds.Api.Mediation.AdColony;
using GoogleMobileAds.Api.Mediation.UnityAds;
#endif

namespace Virterix.AdMediation
{
    public class AdMobMediationBehavior : MonoBehaviour
    {
        private void Awake()
        {
            var adMob = AdMediationSystem.Instance.GetComponentInChildren<AdMobAdapter>();
            adMob.OnWillInitialize += OnAdMobWillInitialize;
#if _ADMOB_MEDIATION
            adMob.OnInitializationComplete += OnAdMobInitializationComplete;
#endif
            AdMediationSystem.OnUserConsentToPersonalizedAdsChanged += OnUserConsentToPersonalizedAdsChanged;
        }

        private void UpdateUserPersonalizedAdsConsent()
        {
#if _ADMOB_MEDIATION
            if (AdMediationSystem.UserPersonalisationConsent != PersonalisationConsent.Undefined)
            {
                bool userAccepts = AdMediationSystem.UserPersonalisationConsent == PersonalisationConsent.Accepted;
                AppLovin.SetHasUserConsent(userAccepts);
                Chartboost.AddDataUseConsent(userAccepts ? CBGDPRDataUseConsent.Behavioral : CBGDPRDataUseConsent.NonBehavioral);
                Chartboost.AddDataUseConsent(userAccepts ? CBCCPADataUseConsent.OptInSale : CBCCPADataUseConsent.OptOutSale);
                AdColonyAppOptions.SetGDPRRequired(userAccepts);
                AdColonyAppOptions.SetGDPRConsentString(userAccepts ? "1" : "0");
                UnityAds.SetGDPRConsentMetaData(userAccepts);
            }
            AppLovin.SetIsAgeRestrictedUser(AdMediationSystem.Instance.m_isChildrenDirected);
#endif
        }

        private void OnAdMobWillInitialize()
        {
            UpdateUserPersonalizedAdsConsent();
        }

#if _ADMOB_MEDIATION
        private void OnAdMobInitializationComplete(InitializationStatus initStatus)
        {
#if AD_MEDIATION_DEBUG_MODE
            Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
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
    }
}
