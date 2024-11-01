#define _AMS_YANDEX_MOBILE_ADS

#if _AMS_YANDEX_MOBILE_ADS
using System;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace Virterix.AdMediation
{
    public class YandexAppOpenAdManager: IAppOpenAdManager
    {
        public bool IsAdAvailable => _appOpenAd != null;

        public bool IsOpened { get; private set; }
        
        public Action<bool> LoadComplete { get; set; }
        
        private AppOpenAdLoader _appOpenAdLoader;
        private AppOpenAd _appOpenAd;
        private readonly string _adUnitId;
        private readonly AdNetworkAdapter _networkAdapter;
        
        public YandexAppOpenAdManager(AdNetworkAdapter networkAdapter, string adUnitId)
        {
            _adUnitId = adUnitId;
            _networkAdapter = networkAdapter;
            SetupLoader();
        }
        
        private void SetupLoader()
        {
            _appOpenAdLoader = new AppOpenAdLoader();
            _appOpenAdLoader.OnAdLoaded += HandleAdLoaded;
            _appOpenAdLoader.OnAdFailedToLoad += HandleAdFailedToLoad;
        }
        
        public void LoadAd()
        {
            RequestAppOpenAd();
        }

        public bool ShowAdIfAvailable()
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] YandexAppOpenAdManager ShowAdIfAvailable _adUnitId:{_adUnitId}");
#endif
            
            if (!IsAdAvailable || IsOpened) {
                return false;
            }
            
            IsOpened = true;
            _appOpenAd.Show();
            return true;
        }

        private void RequestAppOpenAd()
        {
            DestroyAd();
            
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] YandexAppOpenAdManager RequestAppOpenAd _adUnitId:{_adUnitId}");
#endif
            
            AdRequestConfiguration adRequestConfiguration = new AdRequestConfiguration.Builder(_adUnitId).Build();
            _appOpenAdLoader.LoadAd(adRequestConfiguration);
        }

        private void DestroyAd()
        {
            if (_appOpenAd != null)
            {
                IsOpened = false;
                _appOpenAd.OnAdClicked -= HandleAdClicked;
                _appOpenAd.OnAdShown -= HandleAdShown;
                _appOpenAd.OnAdFailedToShow -= HandleAdFailedToShow;
                _appOpenAd.OnAdDismissed -= HandleAdDismissed;
                _appOpenAd.OnAdImpression -= HandleImpression;
                
                _appOpenAd.Destroy();
                _appOpenAd = null;
            }
        }
        
        private void HandleAdLoaded(object sender, AppOpenAdLoadedEventArgs args)
        {
            _appOpenAd = args.AppOpenAd;

            _appOpenAd.OnAdClicked += HandleAdClicked;
            _appOpenAd.OnAdShown += HandleAdShown;
            _appOpenAd.OnAdFailedToShow += HandleAdFailedToShow;
            _appOpenAd.OnAdDismissed += HandleAdDismissed;
            _appOpenAd.OnAdImpression += HandleImpression;
            
            LoadComplete?.Invoke(true);
        }

        private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
#if AD_MEDIATION_DEBUG_MODE
            Debug.Log($"[AMS] YandexAppOpenAdManager HandleAdFailedToLoad Message:{args.Message} AdUnitId:{args.AdUnitId} curr: {_adUnitId}");
#endif
            LoadComplete?.Invoke(false);
        }

        private void HandleAdDismissed(object sender, EventArgs args)
        {
            DestroyAd();
            RequestAppOpenAd();
        }

        private void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
        {
            DestroyAd();
            RequestAppOpenAd();
        }

        private void HandleAdClicked(object sender, EventArgs args)
        {
        }

        private void HandleAdShown(object sender, EventArgs args)
        {
        }

        private void HandleImpression(object sender, ImpressionData impressionData)
        {
        }
    }
}
#endif