#define _AMS_UNITY_ADS

using System.Collections;
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
            TOP_LEFT,
            TOP_CENTER,
            TOP_RIGHT,
            BOTTOM_LEFT,
            BOTTOM_CENTER,
            BOTTOM_RIGHT,
            CENTER
        }

#if _AMS_UNITY_ADS
        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);
            try
            {
                m_appId = parameters["appId"];
            }
            catch
            {
                m_appId = "";
            }

            if (Advertisement.isSupported && !Advertisement.isInitialized)
            {
                if (m_isInitializeWhenStart)
                {
                    Advertisement.Initialize(m_appId, AdMediationSystem.Instance.m_testModeEnabled);
                }
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
                    if (m_isBannerDisplayed)
                    {
                        Hide(adInstance);
                    }
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
            if (IsReady(adInstance))
            {
                if (adType == AdType.Banner)
                {
                    UnityAdInstanceBannerParameters bannerParams = adInstance.m_adInstanceParams as UnityAdInstanceBannerParameters;
                    if (bannerParams != null)
                    {
                        Advertisement.Banner.SetPosition((BannerPosition)bannerParams.m_bannerPosition);
                    }
                    Advertisement.Banner.Show(adInstance.m_adId);
                    m_isBannerDisplayed = true;
                }
                else
                {
                    Advertisement.Show(adInstance.m_adId);
                }
                return true;
            }
            return false;
        }

        public override void Hide(AdInstance adInstance = null)
        {
            AdType adType = adInstance.m_adType;
            if (adType == AdType.Banner)
            {
                Advertisement.Banner.Hide();
                m_isBannerDisplayed = true;
            }
        }

        public override bool IsReady(AdInstance adInstance = null)
        {
            bool isReady = false;
            if (adInstance != null)
            {
                isReady = Advertisement.IsReady(adInstance.m_adId);
            }
            return isReady;
        }

        public override void SetPersonalizedAds(bool isPersonalizedAds)
        {
            MetaData privacyMetaData = new MetaData("privacy");
            privacyMetaData.Set("consent", isPersonalizedAds ? "true" : "false");
            Advertisement.SetMetaData(privacyMetaData);

            MetaData gdprMetaData = new MetaData("gdpr");
            gdprMetaData.Set("consent", isPersonalizedAds ? "true" : "false");
            Advertisement.SetMetaData(gdprMetaData);
        }

        //===============================================================================
#region Callback Event Methods
        //-------------------------------------------------------------------------------

        public void OnUnityAdsReady(string adId)
        {
            AdInstance adInstance = GetAdInstanceByAdId(adId);

            if (adInstance != null)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("UnityAdsAdapter.OnUnityAdsReady() " + adInstance.m_adId + " m_bannerVisibled:" + adInstance.m_bannerVisibled);
#endif
                adInstance.State = AdState.Received;
                if (adInstance.m_adType == AdType.Banner && adInstance.m_bannerVisibled)
                {
                    Show(adInstance);
                }
                AddEvent(adInstance.m_adType, AdEvent.Prepared, adInstance);
            }
        }

        public void OnUnityAdsDidError(string message)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("UnityAdsAdapter.OnUnityAdsDidError() " + message);
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
                Debug.Log("UnityAdsAdapter.OnUnityAdsDidFinish() " + adInstance.m_adId);
#endif

                if (adInstance.m_adType == AdType.Incentivized)
                {
                    switch (showResult)
                    {
                        case ShowResult.Finished:
                            AddEvent(adInstance.m_adType, AdEvent.IncentivizedCompleted, adInstance);
                            break;
                        case ShowResult.Skipped:
                        case ShowResult.Failed:
                            AddEvent(adInstance.m_adType, AdEvent.IncentivizedUncompleted, adInstance);
                            break;
                    }
                }
                AddEvent(adInstance.m_adType, AdEvent.Hiding, adInstance);
            }
        }

#endregion // Callback Event Methods

#endif // _AMS_UNITY_ADS
    }
} // namespace Virterix.AdMediation