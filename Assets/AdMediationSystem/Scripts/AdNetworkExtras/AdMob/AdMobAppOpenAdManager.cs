//#define _AMS_ADMOB

using System;
using UnityEngine;
#if _AMS_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Virterix.AdMediation
{
#if _AMS_ADMOB
    public class AdMobAppOpenAdManager: IAppOpenAdManager
    {
        private static AdMobAppOpenAdManager s_instance;
        private readonly string _adUnitId;
        private readonly AdNetworkAdapter _networkAdapter;
        
        private AppOpenAd _appOpenAd;
        private bool _isShowingAd = false;
        private DateTime _loadTime;
      
        public static AdMobAppOpenAdManager Instance => s_instance;
        public int DisplayCount { get; private set; }
        
        public Action<bool> LoadComplete { get; set; }

        public bool IsAdAvailable =>
            _appOpenAd != null && (DateTime.UtcNow - _loadTime).TotalMinutes < 240;
        
        public AdMobAppOpenAdManager(AdNetworkAdapter networkAdapter, string adUnitId)
        {
            _adUnitId = adUnitId;
            _networkAdapter = networkAdapter;
        }
        
        public void LoadAd()
        {
            DestroyAd();
            AdRequest request = new AdRequest();
            AppOpenAd.Load(_adUnitId, request, OnAppOpenAdLoad);
            
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdMobAppOpenAdManager LoadAd");
#endif
        }

        public bool ShowAdIfAvailable()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] AdMobAppOpenAdManager ShowAdIfAvailable() IsAdAvailable:{IsAdAvailable}");
#endif
            if (!IsAdAvailable || _isShowingAd) {
                return false;
            }

            _appOpenAd.Show();
            return true;
        }
        
        /// <summary>
        /// Destroys the ad.
        /// </summary>
        public void DestroyAd()
        {
            if (_appOpenAd != null)
            {
                _appOpenAd.OnAdFullScreenContentClosed -= HandleAdFullScreenContentClosed;
                _appOpenAd.OnAdFullScreenContentFailed -= HandleAdFullScreenContentFailed;
                _appOpenAd.OnAdFullScreenContentOpened -= HandleAdFullScreenContentOpened;
                
                _appOpenAd.Destroy();
                _appOpenAd = null;
            }
        }
        
        private void HandleAdFullScreenContentClosed()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdMobAppOpenAdManager Closed app open ad");
#endif
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            _appOpenAd = null;
            _isShowingAd = false;
            AdMediationSystem.NotifyAdNetworkEvent(null, _networkAdapter, AdType.AppOpen, AdEvent.Hiding, AdInstance.AD_INSTANCE_DEFAULT_NAME);
            LoadAd();
        }

        private void HandleAdFullScreenContentFailed(AdError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.LogFormat("[AMS] AdMobAppOpenAdManager Failed to present the ad (reason: {0})", error.GetMessage());
#endif
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            _appOpenAd = null;
            LoadAd();
        }

        private void HandleAdFullScreenContentOpened()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdMobAppOpenAdManager Displayed app open ad");
#endif
            DisplayCount++;
            _isShowingAd = true;
            AdMediationSystem.NotifyAdNetworkEvent(null, _networkAdapter, AdType.AppOpen, AdEvent.Showing, AdInstance.AD_INSTANCE_DEFAULT_NAME);
        }

        private void HandleAdImpressionRecorded()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdMobAppOpenAdManager Recorded ad impression");
#endif
        }

        private void HandleAdPaid(AdValue value)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.LogFormat("[AMS] AdMobAppOpenAdManager Received paid event. (currency: {0}, value: {1}", value.CurrencyCode, value.Value);
#endif
        }

        private void OnAppOpenAdLoad(AppOpenAd appOpenAd, LoadAdError error)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] AdMobAppOpenAdManager OnAppOpenAdLoad() {error}");
#endif
            
            LoadComplete?.Invoke(error == null);
            
            if (error != null)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogFormat("[AMS] AdMobAppOpenAdManager Failed to load the ad. (reason: {0})", error.GetMessage());
#endif
                return;
            }

            _appOpenAd = appOpenAd;
            
            _appOpenAd.OnAdFullScreenContentClosed += HandleAdFullScreenContentClosed;
            _appOpenAd.OnAdFullScreenContentFailed += HandleAdFullScreenContentFailed;
            _appOpenAd.OnAdFullScreenContentOpened += HandleAdFullScreenContentOpened;
            
            _loadTime = DateTime.UtcNow;
        }
    }
#endif
}