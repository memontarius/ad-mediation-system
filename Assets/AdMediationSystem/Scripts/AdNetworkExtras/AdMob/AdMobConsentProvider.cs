//#define _AMS_ADMOB

#if _AMS_ADMOB
using GoogleMobileAds.Ump.Api;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation
{
    public class AdMobConsentProvider
    {
        public enum FormState
        {
            Undefined,
            Loading,
            Loaded
        }

        public enum RequirementStatus
        {
            Unknown,
            NotRequired,
            Required
        }

        public enum ConsentStatus
        {
            Unknown,
            NotRequired,
            Required,
            Obtained
        }

        public event Action OnConsentFormLoaded = null;
        public event Action OnConsentFormShown = null;

        public ConsentStatus ClientConsentStatus
        {
            get
            {
#if _AMS_ADMOB
                switch (ConsentInformation.ConsentStatus)
                {
                    case GoogleMobileAds.Ump.Api.ConsentStatus.Unknown:
                    default:
                        return ConsentStatus.Unknown;
                    case GoogleMobileAds.Ump.Api.ConsentStatus.NotRequired:
                        return ConsentStatus.NotRequired;
                    case GoogleMobileAds.Ump.Api.ConsentStatus.Obtained:
                        return ConsentStatus.Obtained;
                    case GoogleMobileAds.Ump.Api.ConsentStatus.Required:
                        return ConsentStatus.Required;
                }
#else
                return ConsentStatus.Unknown;
#endif
            }
        }

        /// <summary>
        /// If true, it is safe to call MobileAds.Initialize() and load Ads.
        /// </summary>
        public bool CanRequestAds
        {
            get
            {
#if _AMS_ADMOB
                return ConsentInformation.ConsentStatus == GoogleMobileAds.Ump.Api.ConsentStatus.Obtained ||
                       ConsentInformation.ConsentStatus == GoogleMobileAds.Ump.Api.ConsentStatus.NotRequired;
#else
                return false;
#endif
            }
        }

        public RequirementStatus PrivacyRequirementStatus
        {
            get
            {
#if _AMS_ADMOB
                switch (ConsentInformation.PrivacyOptionsRequirementStatus)
                {
                    case PrivacyOptionsRequirementStatus.Unknown:
                    default:
                        return RequirementStatus.Unknown;
                    case PrivacyOptionsRequirementStatus.NotRequired:
                        return RequirementStatus.NotRequired;
                    case PrivacyOptionsRequirementStatus.Required:
                        return RequirementStatus.Required;
                }
#else
                return RequirementStatus.Unknown;
#endif
            }
        }

        public FormState ConsentFormState { get; private set; } = FormState.Undefined;

        /// <summary>
        /// Startup method for the Google User Messaging Platform (UMP) SDK
        /// which will run all startup logic including loading any required
        /// updates and displaying any required forms.
        /// </summary>
        public void GatherConsent(Action<string> onComplete, List<string> testDeviceIds = null)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AdMobConsentProvider] Gathering consent.");
#endif

#if _AMS_ADMOB
            var requestParameters = new ConsentRequestParameters();

            if (AdMediationSystem.Instance.ChildrenMode != ChildDirectedMode.NotAssign)
            {
                requestParameters.TagForUnderAgeOfConsent =
                    AdMediationSystem.Instance.ChildrenMode == ChildDirectedMode.NotDirected;
            }

            if (AdMediationSystem.Instance.IsTestModeEnabled)
            {
                requestParameters.ConsentDebugSettings = new ConsentDebugSettings
                {
                    // For debugging consent settings by geography.
                    DebugGeography = DebugGeography.Disabled,
                    TestDeviceHashedIds = testDeviceIds ?? new List<string>(),
                };
            }

            // The Google Mobile Ads SDK provides the User Messaging Platform (Google's
            // IAB Certified consent management platform) as one solution to capture
            // consent for users in GDPR impacted countries. This is an example and
            // you can choose another consent management platform to capture consent.
            ConsentInformation.Update(requestParameters, (FormError updateError) =>
            {
                if (updateError != null)
                {
                    LoadForm();
                    onComplete(updateError.Message);
                    return;
                }

                // Determine the consent-related action to take based on the ConsentStatus.
                if (CanRequestAds)
                {
                    // Consent has already been gathered or not required.
                    // Return control back to the user.
                    LoadForm();
                    onComplete(null);
                    return;
                }

                // Consent not obtained and is required.
                // Load the initial consent request form for the user.
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError showError) =>
                {
                    if (showError != null)
                    {
                        // Form showing failed.
                        if (onComplete != null)
                        {
                            onComplete(showError.Message);
                        }
                    }
                    // Form showing succeeded.
                    else
                    {
                        OnConsentFormShown?.Invoke();
                        onComplete?.Invoke(null);
                    }
                });
            });
#endif
        }

        /// <summary>
        /// Shows the privacy options form to the user.
        /// </summary>
        /// <remarks>
        /// Your app needs to allow the user to change their consent status at any time.
        /// Load another form and store it to allow the user to change their consent status
        /// </remarks>
        public void ShowPrivacyOptionsForm(Action<string> onComplete, bool autoLoading = true)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AdMobConsentProvider] Showing privacy options form.");
#endif

#if _AMS_ADMOB
            ConsentForm.ShowPrivacyOptionsForm((FormError showError) =>
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("[AdMobConsentProvider] Options form dismissed");
#endif

                if (showError != null)
                {
#if AD_MEDIATION_DEBUG_MODE
                    Debug.Log(
                        $"[AdMobConsentProvider] Options form show error. ErrorCode: {showError.ErrorCode} Message: {showError.Message} ConsentFormState: {ConsentFormState}");
#endif
                    onComplete?.Invoke(showError.Message);

                    if (showError.ErrorCode == 7 && ConsentFormState != FormState.Loading)
                    {
                        LoadForm();
                    }
                }
                // Form showing succeeded.
                else
                {
                    OnConsentFormShown?.Invoke();
                    onComplete?.Invoke(null);
                    LoadForm();
                }
            });
#endif
        }

        /// <summary>
        /// Reset ConsentInformation for the user.
        /// </summary>
        public void ResetConsentInformation()
        {
#if _AMS_ADMOB
            ConsentFormState = FormState.Undefined;
            ConsentInformation.Reset();
#endif
        }

        public void LoadForm(Action<int> onComplete = null)
        {
            ConsentFormState = FormState.Loading;
#if _AMS_ADMOB
            ConsentForm.Load((ConsentForm form, FormError formError) =>
            {
                if (formError == null)
                {
                    ConsentFormState = FormState.Loaded;
                    OnConsentFormLoaded?.Invoke();
                }
                else
                {
                    ConsentFormState = FormState.Undefined;
                }

                onComplete?.Invoke(formError?.ErrorCode ?? -1);
            });
#endif
        }
    }
}