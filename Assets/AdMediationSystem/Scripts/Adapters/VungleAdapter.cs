//#define _AMS_VUNGLE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.JSON;

namespace Virterix.AdMediation
{
    public class VungleAdapter : AdNetworkAdapter
    {
        private string m_appId;

#if _AMS_VUNGLE
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                Vungle.onPause();
            else
                Vungle.onResume();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeEvents();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnsubscribeEvents();
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonPlacements)
        {
            base.InitializeParameters(parameters, jsonPlacements);
            if (!parameters.TryGetValue("appId", out m_appId))
                m_appId = "";
            SetUserConsentToPersonalizedAds(AdMediationSystem.UserPersonalisationConsent);
            Vungle.init(m_appId);
        }

        public override void Prepare(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (!IsReady(adInstance, placement))
            {
                adInstance.State = AdState.Loading;
                Vungle.loadAd(adInstance.m_adId);  
            }
        }

        public override bool Show(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (IsReady(adInstance, placement))
            {             
                Vungle.playAd(adInstance.m_adId);
                return true;
            }
            return false;
        }

        public override void Hide(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
        }

        public override bool IsReady(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            bool isReady = false;
            if (Vungle.isInitialized())
                isReady = Vungle.isAdvertAvailable(adInstance.m_adId);
            return isReady;
        }

        protected override void SetUserConsentToPersonalizedAds(PersonalisationConsent consent)
        {
            if (consent != PersonalisationConsent.Undefined)
            {
                var vungleConsent = consent == PersonalisationConsent.Accepted ? Vungle.Consent.Accepted : Vungle.Consent.Denied;
                Vungle.updateCCPAStatus(vungleConsent);
                Vungle.updateConsentStatus(vungleConsent);
            }
        }

        private void SubscribeEvents()
        {
            Vungle.onLogEvent += OnLogEvent;
            Vungle.adPlayableEvent += OnAdPlayableEvent;
            Vungle.onAdStartedEvent += OnAdStartedEvent;
            Vungle.onAdEndEvent += OnAdEndEvent;
            Vungle.onAdRewardedEvent += OnAdRewardedEvent;
        }

        private void UnsubscribeEvents()
        {
            Vungle.onLogEvent -= OnLogEvent;
            Vungle.adPlayableEvent -= OnAdPlayableEvent;
            Vungle.onAdStartedEvent -= OnAdStartedEvent;
            Vungle.onAdEndEvent -= OnAdEndEvent;
            Vungle.onAdRewardedEvent -= OnAdRewardedEvent;
        }

        //_______________________________________________________________________________
        #region Callbacks

        private void OnLogEvent(string message)
        {
        }

        private void OnAdPlayableEvent(string placementId, bool playable)
        {
            var adInstance = GetAdInstanceByAdId(placementId);
            if (playable)
                adInstance.State = AdState.Received;
            AddEvent(adInstance.m_adType, AdEvent.Prepared, adInstance);
        }

        private void OnAdStartedEvent(string placementId)
        {
            var adInstance = GetAdInstanceByAdId(placementId);
            AddEvent(adInstance.m_adType, AdEvent.Show, adInstance);
        }

        private void OnAdEndEvent(string placementId)
        {
            var adInstance = GetAdInstanceByAdId(placementId);
            adInstance.State = AdState.Unavailable;
            AddEvent(adInstance.m_adType, AdEvent.Hiding, adInstance);
        }

        private void OnAdRewardedEvent(string placementId)
        {
            var adInstance = GetAdInstanceByAdId(placementId);
            AddEvent(adInstance.m_adType, AdEvent.IncentivizationCompleted, adInstance);
        }

        #endregion // Callbacks
#endif
    }
}
