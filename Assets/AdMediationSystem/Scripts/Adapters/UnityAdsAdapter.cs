//#define _AMS_UNITY_ADS

using System.Collections.Generic;
using UnityEngine;
using Boomlagoon.JSON;
#if _AMS_UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace Virterix.AdMediation
{
    public class UnityAdsAdapter : AdNetworkAdapter
#if _AMS_UNITY_ADS
    ,IUnityAdsListener
#endif
    {
        public bool m_isInitializeWhenStart = true; 

        private string m_appId;
        private bool m_isBannerDisplayed;

        public enum UnityAdsBannerPosition
        {
            TopLeft,
            TopCenter,
            TopRight,
            BottomLeft,
            BottomCenter,
            BottomRight,
            Center
        }

        protected override string AdInstanceParametersFolder
        {
            get { return UnityAdInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER; }
        }

        public override bool UseSingleBannerInstance => true;

        public static string GetSDKVersion()
        {
            string version = string.Empty;
#if UNITY_EDITOR && _AMS_UNITY_ADS
#endif
            return version;
        }

#if _AMS_UNITY_ADS
        public static BannerPosition ConvertToNativeBannerPosition(UnityAdsBannerPosition bannerPosition)
        {
            BannerPosition nativeBannerPosition = BannerPosition.BOTTOM_CENTER;
            switch(bannerPosition)
            {
                case UnityAdsBannerPosition.BottomCenter:
                    nativeBannerPosition = BannerPosition.BOTTOM_CENTER;
                    break;
                case UnityAdsBannerPosition.BottomLeft:
                    nativeBannerPosition = BannerPosition.BOTTOM_LEFT;
                    break;
                case UnityAdsBannerPosition.BottomRight:
                    nativeBannerPosition = BannerPosition.BOTTOM_RIGHT;
                    break;
                case UnityAdsBannerPosition.TopCenter:
                    nativeBannerPosition = BannerPosition.TOP_CENTER;
                    break;
                case UnityAdsBannerPosition.TopLeft:
                    nativeBannerPosition = BannerPosition.TOP_LEFT;
                    break;
                case UnityAdsBannerPosition.TopRight:
                    nativeBannerPosition = BannerPosition.TOP_RIGHT;
                    break;
            } 
            return nativeBannerPosition;
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);

            if (!parameters.TryGetValue("appId", out m_appId))
                m_appId = "";
            
            SetUserConsentToPersonalizedAds(AdMediationSystem.UserPersonalisationConsent);
            
            if (Advertisement.isSupported && !Advertisement.isInitialized)
            {
                if (m_isInitializeWhenStart)
                    Advertisement.Initialize(m_appId, AdMediationSystem.Instance.IsTestModeEnabled);
                Advertisement.AddListener(this);
            }
        }

        protected override void InitializeAdInstanceData(AdInstance adInstance, JSONValue jsonAdInstances)
        {
            base.InitializeAdInstanceData(adInstance, jsonAdInstances);
        }

        protected override AdInstance CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            AdInstance adInstance = new AdInstance(this);
            return adInstance;
        }

        public override void Prepare(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            if (!IsReady(adInstance))
            {
                adInstance.State = AdState.Loading;
               
                if (adType == AdType.Banner)
                {
                    adInstance.CurrPlacement = placement;
                    Advertisement.Banner.Hide(true);
                    Advertisement.Banner.Load(adInstance.m_adId);
                }
                else
                {
                    Advertisement.Load(adInstance.m_adId);
                }
            }
        }

        public override bool Show(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            AdType adType = adInstance.m_adType;
            bool isPreviousBannerDisplayed = m_isBannerDisplayed;

            if (adType == AdType.Banner)
            {
                adInstance.m_bannerDisplayed = true;
                m_isBannerDisplayed = true;
                m_currBannerPlacement = placement;
                adInstance.CurrPlacement = placement;
            }

            if (IsReady(adInstance))
            {
                if (adType == AdType.Banner)
                {
                    UnityAdInstanceBannerParameters bannerParams = adInstance.m_adInstanceParams as UnityAdInstanceBannerParameters;
                    if (bannerParams != null)
                    {
#if UNITY_EDITOR
                        Advertisement.Banner.Hide(true);
#endif
                        var bannerPosition = ConvertToNativeBannerPosition((UnityAdsBannerPosition)GetBannerPosition(adInstance, placement));
                        Advertisement.Banner.SetPosition(bannerPosition);
                    }
                    Advertisement.Banner.Show(adInstance.m_adId);

                    if (!isPreviousBannerDisplayed)
                        AddEvent(adInstance.m_adType, AdEvent.Show, adInstance);
                }
                else
                {
                    Advertisement.Show(adInstance.m_adId);
                }
                return true;
            }
            return false;
        }

        public override void Hide(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            if (adInstance.m_adType == AdType.Banner)
            {
                adInstance.m_bannerDisplayed = false;
                Advertisement.Banner.Hide();
                if (m_isBannerDisplayed)
                    NotifyEvent(AdEvent.Hiding, adInstance);
                m_isBannerDisplayed = false;
            }
        }

        public override bool IsReady(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            bool isReady = false;
            if (adInstance != null)
                isReady = Advertisement.IsReady(adInstance.m_adId);
            return isReady;
        }

        protected override void SetUserConsentToPersonalizedAds(PersonalisationConsent consent)
        {
            if (consent != PersonalisationConsent.Undefined)
            {
                string value = consent == PersonalisationConsent.Accepted ? "true" : "false";
                SetMetaData("user", "nonbehavioral", value);
                SetMetaData("gdpr", "consent", value);
                SetMetaData("pipl", "consent", value);
                SetMetaData("privacy", "consent", value);
            }
        }
        
        private void SetMetaData<TValue>(string category, string key, TValue value)
        {
            MetaData coppaUserMetaData = new MetaData(category);
            coppaUserMetaData.Set(key, value);
            Advertisement.SetMetaData(coppaUserMetaData);
        }
        
        //_______________________________________________________________________________
        #region Callback Event Methods
 
        public void OnUnityAdsReady(string adId)
        {
            AdInstance adInstance = GetAdInstanceByAdId(adId);

            if (adInstance != null)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("[AMS] UnityAdsAdapter.OnUnityAdsReady() adId: " + adInstance.m_adId + " bannerVisibled:" + adInstance.m_bannerDisplayed);
#endif
                adInstance.State = AdState.Received;
                if (adInstance.m_adType == AdType.Banner)
                {
                    if (adInstance.m_bannerDisplayed)
                    {
                        if (!string.IsNullOrEmpty(m_currBannerPlacement))
                        {
                            var pos = ConvertToNativeBannerPosition((UnityAdsBannerPosition)GetBannerPosition(adInstance, m_currBannerPlacement));
                            Advertisement.Banner.SetPosition(pos);
                        }
                        Advertisement.Banner.Show(adInstance.m_adId);
                    }
                    else
                        Advertisement.Banner.Hide();
                }
                AddEvent(adInstance.m_adType, AdEvent.Prepared, adInstance);
            }
        }

        public void OnUnityAdsDidError(string message)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] UnityAdsAdapter.OnUnityAdsDidError() message: " + message);
#endif
        }

        public void OnUnityAdsDidStart(string adId)
        {
            AdInstance adInstance = GetAdInstanceByAdId(adId);
            if (adInstance != null)
            {
                AddEvent(adInstance.m_adType, AdEvent.Show, adInstance);
            }
        }

        public void OnUnityAdsDidFinish(string adId, ShowResult showResult)
        {
            AdInstance adInstance = GetAdInstanceByAdId(adId);

            if (adInstance != null)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("[AMS] UnityAdsAdapter.OnUnityAdsDidFinish() adId: " + adInstance.m_adId);
#endif
                if (adInstance.m_adType == AdType.Incentivized)
                {
                    switch (showResult)
                    {
                        case ShowResult.Finished:
                            AddEvent(adInstance.m_adType, AdEvent.IncentivizationCompleted, adInstance);
                            break;
                        case ShowResult.Skipped:
                        case ShowResult.Failed:
                            AddEvent(adInstance.m_adType, AdEvent.IncentivizationUncompleted, adInstance);
                            break;
                    }
                }

                if (adInstance.m_adType != AdType.Banner)
                    AddEvent(adInstance.m_adType, AdEvent.Hiding, adInstance);
            }
        }
        #endregion // Callback Event Methods
#endif
    }
}