#define _AMS_UNITY_ADS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boomlagoon.JSON;
using System.Linq;
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
            get { return UnityAdsInstanceBannerParameters._AD_INSTANCE_PARAMETERS_FOLDER; }
        }

        public static string GetSDKVersion()
        {
            string version = string.Empty;
#if UNITY_EDITOR && _AMS_UNITY_ADS
#endif
            return version;
        }

#if _AMS_UNITY_ADS
        public static BannerPosition ConvertToAdPosition(UnityAdsBannerPosition bannerPosition)
        {
            BannerPosition nativeBannerPosition = (BannerPosition)bannerPosition;
            return nativeBannerPosition;
        }

        public static BannerPosition GetBannerPosition(AdInstance adInstance, string placement)
        {
            BannerPosition nativeBannerPosition = BannerPosition.BOTTOM_CENTER;
            var adMobAdInstanceParams = adInstance.m_adInstanceParams as UnityAdsInstanceBannerParameters;
            var bannerPosition = adMobAdInstanceParams.m_bannerPositions.FirstOrDefault(p => p.m_placementName == placement);
            nativeBannerPosition = ConvertToAdPosition(bannerPosition.m_bannerPosition);
            return nativeBannerPosition;
        }

        protected override void InitializeParameters(Dictionary<string, string> parameters, JSONArray jsonAdInstances, bool isPersonalizedAds = true)
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
            SetPersonalizedAds(isPersonalizedAds);
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
                        Hide(adInstance);
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

            if (adType == AdType.Banner)
                m_isBannerDisplayed = true;

            Debug.Log("====== UNITY SHOW: " + adInstance.Name + " - placement:" + placement);

            if (IsReady(adInstance))
            {
                if (adType == AdType.Banner)
                {
                    UnityAdsInstanceBannerParameters bannerParams = adInstance.m_adInstanceParams as UnityAdsInstanceBannerParameters;
                    if (bannerParams != null)
                    {
#if UNITY_EDITOR
                        Advertisement.Banner.Hide(true);
#endif
                        Advertisement.Banner.SetPosition(GetBannerPosition(adInstance, placement));
                    }
                    Advertisement.Banner.Show(adInstance.m_adId);
                }
                else
                {
                    Advertisement.Show(adInstance.m_adId);
                }
                return true;
            }
            return false;
        }

        public override void Hide(AdInstance adInstance = null, string adInstanceName = AdInstance.AD_INSTANCE_DEFAULT_NAME)
        {
            if (adInstance.m_adType == AdType.Banner)
            {
                adInstance.m_bannerDisplayed = false;
                m_isBannerDisplayed = false;
                Advertisement.Banner.Hide();
            }
        }

        public override bool IsReady(AdInstance adInstance = null, string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
        {
            bool isReady = false;
            if (adInstance != null)
            {
                isReady = Advertisement.IsReady(adInstance.m_adId);
            }
            return isReady;
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
                Debug.Log("[AMS] UnityAdsAdapter.OnUnityAdsReady() adId: " + adInstance.m_adId + " bannerVisibled:" + adInstance.m_bannerDisplayed);
#endif
                adInstance.State = AdState.Received;
                if (adInstance.m_adType == AdType.Banner && adInstance.m_bannerDisplayed)
                {
                    Show(adInstance);
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
                            AddEvent(adInstance.m_adType, AdEvent.IncentivizedCompleted, adInstance);
                            break;
                        case ShowResult.Skipped:
                        case ShowResult.Failed:
                            AddEvent(adInstance.m_adType, AdEvent.IncentivizedUncompleted, adInstance);
                            break;
                    }
                }

                if (adInstance.m_adType != AdType.Banner)
                    AddEvent(adInstance.m_adType, AdEvent.Hiding, adInstance);
            }
        }

#endregion // Callback Event Methods

#endif // _AMS_UNITY_ADS
    }
} // namespace Virterix.AdMediation