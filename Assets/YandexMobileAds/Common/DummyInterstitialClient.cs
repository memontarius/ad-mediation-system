/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for Unity (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using System;
using System.Reflection;
using YandexMobileAds.Base;
using YandexMobileAds.Platforms;
using UnityEngine;

namespace YandexMobileAds.Common
{
    public class DummyInterstitialClient : IInterstitialClient
    {
        private static string TAG = "Dummy Interstitial ";

        public event EventHandler<EventArgs> OnInterstitialLoaded;
        public event EventHandler<AdFailureEventArgs> OnInterstitialFailedToLoad;
        public event EventHandler<EventArgs> OnReturnedToApplication;
        public event EventHandler<EventArgs> OnLeftApplication;
        public event EventHandler<EventArgs> OnAdClicked;
        public event EventHandler<EventArgs> OnInterstitialShown;
        public event EventHandler<EventArgs> OnInterstitialDismissed;
        public event EventHandler<ImpressionData> OnImpression;
        public event EventHandler<AdFailureEventArgs> OnInterstitialFailedToShow;

        public DummyInterstitialClient()
        {
            Debug.Log(TAG + MethodBase.GetCurrentMethod().Name);
        }

        public void LoadAd(AdRequest request)
        {
            Debug.Log(TAG + MethodBase.GetCurrentMethod().Name);
        }

        public bool IsLoaded()
        {
            Debug.Log(TAG + MethodBase.GetCurrentMethod().Name);
            return false;
        }

        public void Show()
        {
            Debug.Log(TAG + MethodBase.GetCurrentMethod().Name);
        }

        public void Destroy()
        {
            Debug.Log(TAG + MethodBase.GetCurrentMethod().Name);
        }
    }
}