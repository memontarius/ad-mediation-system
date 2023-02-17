//#define _AMS_ADMOB

using System;
using UnityEngine;
#if _AMS_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Virterix.AdMediation
{
#if _AMS_ADMOB
    public class AppOpenAdManager
    {
        private static AppOpenAdManager s_instance;
        private readonly string _adUnitId;
        private readonly ScreenOrientation _screenOrientation;
        private readonly AdNetworkAdapter _networkAdapter;
        
        private AppOpenAd _appOpenAd;
        private bool _isShowingAd = false;
        private DateTime _loadTime;
        
        public static AppOpenAdManager Instance => s_instance;
        public int DisplayCount { get; private set; }
        
        private bool IsAdAvailable =>
            _appOpenAd != null && (DateTime.UtcNow - _loadTime).TotalMinutes < 240;

        public AppOpenAdManager(AdNetworkAdapter networkAdapter, string adUnitId, ScreenOrientation screenOrientation)
        {
            _adUnitId = adUnitId;
            _screenOrientation = screenOrientation;
            _networkAdapter = networkAdapter;
        }
        
        public void LoadAd()
        {
            AdRequest request = new AdRequest.Builder().Build();
            AppOpenAd.LoadAd(_adUnitId, _screenOrientation, request, OnAppOpenAdLoad);
        }

        public void ShowAdIfAvailable()
        {
            if (!IsAdAvailable || _isShowingAd)
                return;

            _appOpenAd.OnAdFullScreenContentClosed += HandleAdFullScreenContentClosed;
            _appOpenAd.OnAdFullScreenContentFailed += HandleAdFullScreenContentFailed;
            _appOpenAd.OnAdFullScreenContentOpened += HandleAdFullScreenContentOpened;
            //_appOpenAd.OnAdImpressionRecorded += HandleAdImpressionRecorded;
            //_appOpenAd.OnAdPaid += HandleAdPaid;
            
            _appOpenAd.Show();
        }
        
        private void HandleAdFullScreenContentClosed()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("Closed app open ad");
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
            Debug.LogFormat("Failed to present the ad (reason: {0})", error.GetMessage());
#endif
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            _appOpenAd = null;
            LoadAd();
        }

        private void HandleAdFullScreenContentOpened()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("Displayed app open ad");
#endif
            DisplayCount++;
            _isShowingAd = true;
            AdMediationSystem.NotifyAdNetworkEvent(null, _networkAdapter, AdType.AppOpen, AdEvent.Showing, AdInstance.AD_INSTANCE_DEFAULT_NAME);
        }

        private void HandleAdImpressionRecorded()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("Recorded ad impression");
#endif
        }

        private void HandleAdPaid(AdValue value)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.LogFormat("Received paid event. (currency: {0}, value: {1}", value.CurrencyCode, value.Value);
#endif
        }

        private void OnAppOpenAdLoad(AppOpenAd appOpenAd, AdFailedToLoadEventArgs error)
        {
            if (error != null)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogFormat("Failed to load the ad. (reason: {0})", error.LoadAdError.GetMessage());
#endif
                return;
            }

            _appOpenAd = appOpenAd;
            _loadTime = DateTime.UtcNow;
        }
    }
#endif
}