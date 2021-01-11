
#define _MS_UNITY_ADS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boomlagoon.JSON;

#if _MS_UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace Virterix.AdMediation
{
    public class UnityAdsAdapter : AdNetworkAdapter, IUnityAdsListener
    {

        public bool m_isInitializeWhenStart = true;
        public bool m_isTestMode;

        string m_appId;
        string m_interstitialId;
        string m_rewardedId;
        string m_bannerId;

        bool m_isBannerDisplayed;

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

#if _MS_UNITY_ADS

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances)
        {
            base.InitializeParameters(parameters, jsonAdInstances);

            m_appId = parameters["appId"];
            try
            {
                m_interstitialId = parameters["interstitialId"];
                m_rewardedId = parameters["rewardedId"];
                m_bannerId = parameters["bannerId"];
            }
            catch
            {
                m_interstitialId = "";
                m_rewardedId = "";
                m_bannerId = "";
            }

            if (Advertisement.isSupported && !Advertisement.isInitialized)
            {
                if (m_isInitializeWhenStart)
                {
                    Advertisement.Initialize(m_appId, m_isTestMode);
                }
                Advertisement.AddListener(this);
            }

            if (IsSupported(AdType.Interstitial))
            {
                AdInstanceData adInstance = new AdInstanceData(AdType.Interstitial, m_interstitialId);
                AddAdInstance(adInstance);
            }

            if (IsSupported(AdType.Incentivized))
            {
                AdInstanceData adInstance = new AdInstanceData(AdType.Incentivized, m_rewardedId);
                AddAdInstance(adInstance);
            }

            if (IsSupported(AdType.Banner))
            {
                AdInstanceData adInstance = new AdInstanceData(AdType.Banner, m_bannerId);
                AddAdInstance(adInstance);
            }
        }

        protected override void InitializeAdInstanceData(AdInstanceData adInstance, JSONValue jsonAdInstances)
        {
            base.InitializeAdInstanceData(adInstance, jsonAdInstances);
        }

        protected override AdInstanceData CreateAdInstanceData(JSONValue jsonAdInstance)
        {
            AdInstanceData adInstance = new AdInstanceData();
            return adInstance;
        }

        public override void Prepare(AdType adType, AdInstanceData adInstance = null)
        {
            if (!IsReady(adType, adInstance))
            {
                if (adType == AdType.Banner)
                {
                    if (m_isBannerDisplayed)
                    {
                        Hide(AdType.Banner, adInstance);
                    }
                    Advertisement.Banner.Load(adInstance.m_adID);
                }
                else
                {
                    Advertisement.Load(adInstance.m_adID);
                }
            }
        }

        public override bool Show(AdType adType, AdInstanceData adInstance = null)
        {
            if (IsReady(adType, adInstance))
            {
                if (adType == AdType.Banner)
                {
                    UnityAdInstanceBannerParameters bannerParams = adInstance.m_adInstanceParams as UnityAdInstanceBannerParameters;
                    if (bannerParams != null)
                    {
                        Advertisement.Banner.SetPosition((BannerPosition)bannerParams.m_bannerPosition);
                    }
                    Advertisement.Banner.Show(adInstance.m_adID);
                    m_isBannerDisplayed = true;
                }
                else
                {
                    Advertisement.Show(adInstance.m_adID);
                }
                return true;
            }
            return false;
        }

        public override void Hide(AdType adType, AdInstanceData adInstance = null)
        {
            if (adType == AdType.Banner)
            {
                Advertisement.Banner.Hide();
                m_isBannerDisplayed = true;
            }
        }

        public override bool IsReady(AdType adType, AdInstanceData adInstance = null)
        {
            bool isReady = false;
            if (adInstance != null)
            {
                isReady = Advertisement.IsReady(adInstance.m_adID);
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
            AdInstanceData adInstance = GetAdInstanceByAdId(adId, false);

            if (adInstance != null)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("UnityAdsAdapter.OnUnityAdsReady() " + adInstance.m_adID + " m_isBannerAdTypeVisibled:" + adInstance.m_isBannerAdTypeVisibled);
#endif
                if (adInstance.m_adType == AdType.Banner && adInstance.m_isBannerAdTypeVisibled)
                {
                    Show(AdType.Banner, adInstance);
                }
                AddEvent(adInstance.m_adType, AdEvent.Prepared, adInstance);
            }
        }

        public void OnUnityAdsDidError(string message)
        {
            Debug.Log("UnityAdsAdapter.OnUnityAdsDidError() " + message);
        }

        public void OnUnityAdsDidStart(string adId)
        {
            AdInstanceData adInstance = GetAdInstanceByAdId(adId, false);

            if (adInstance != null)
            {
                AddEvent(adInstance.m_adType, AdEvent.Show, adInstance);
            }
        }

        public void OnUnityAdsDidFinish(string adId, ShowResult showResult)
        {
            AdInstanceData adInstance = GetAdInstanceByAdId(adId, false);

            if (adInstance != null)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("UnityAdsAdapter.OnUnityAdsDidFinish() " + adInstance.m_adID);
#endif

                if (adInstance.m_adType == AdType.Incentivized)
                {
                    switch (showResult)
                    {
                        case ShowResult.Finished:
                            AddEvent(adInstance.m_adType, AdEvent.IncentivizedComplete, adInstance);
                            break;
                        case ShowResult.Skipped:
                        case ShowResult.Failed:
                            AddEvent(adInstance.m_adType, AdEvent.IncentivizedIncomplete, adInstance);
                            break;
                    }
                }
                AddEvent(adInstance.m_adType, AdEvent.Hide, adInstance);
            }
        }

        #endregion // Callback Event Methods

#endif // _MS_UNITY_ADS
    }
} // namespace Virterix.AdMediation