/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for Android (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using System;
using YandexMobileAds.Base;
using YandexMobileAds.Common;
using UnityEngine;

namespace YandexMobileAds.Platforms.Android
{
    public class InterstitialClient : AndroidJavaProxy, IInterstitialClient
    {
        private AndroidJavaObject interstitial;

        public event EventHandler<EventArgs> OnInterstitialLoaded;
        public event EventHandler<AdFailureEventArgs> OnInterstitialFailedToLoad;
        public event EventHandler<EventArgs> OnReturnedToApplication;
        public event EventHandler<EventArgs> OnLeftApplication;
        public event EventHandler<EventArgs> OnAdClicked;
        public event EventHandler<EventArgs> OnInterstitialShown;
        public event EventHandler<EventArgs> OnInterstitialDismissed;
        public event EventHandler<ImpressionData> OnImpression;
        public event EventHandler<AdFailureEventArgs> OnInterstitialFailedToShow;

        public InterstitialClient(string blockId) : base(Utils.UnityInterstitialAdListenerClassName)
        {
            AndroidJavaClass playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);

            AndroidJavaObject activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");

            this.interstitial = new AndroidJavaObject(
                Utils.InterstitialClassName,
                activity,
                blockId);
            this.interstitial.Call("setUnityInterstitialListener", this);
        }

        public void LoadAd(AdRequest request)
        {
            this.interstitial.Call("loadAd", Utils.GetAdRequestJavaObject(request));
        }

        public bool IsLoaded()
        {
            return this.interstitial.Call<bool>("isInterstitialLoaded");
        }

        public void Show()
        {
            this.interstitial.Call("showInterstitial");
        }

        public void Destroy()
        {
            this.interstitial.Call("clearUnityInterstitialListener");
            this.interstitial.Call("destroyInterstitial");
        }

        public void onInterstitialLoaded()
        {
            if (this.OnInterstitialLoaded != null)
            {
                this.OnInterstitialLoaded(this, EventArgs.Empty);
            }
        }

        public void onInterstitialFailedToLoad(string errorReason)
        {
            if (this.OnInterstitialFailedToLoad != null)
            {
                AdFailureEventArgs args = new AdFailureEventArgs()
                {
                    Message = errorReason
                };
                this.OnInterstitialFailedToLoad(this, args);
            }
        }

        public void onReturnedToApplication()
        {
            if (this.OnReturnedToApplication != null)
            {
                this.OnReturnedToApplication(this, EventArgs.Empty);
            }
        }

        public void onLeftApplication()
        {
            if (this.OnLeftApplication != null)
            {
                this.OnLeftApplication(this, EventArgs.Empty);
            }
        }

        public void onAdClicked()
        {
            if (this.OnAdClicked != null)
            {
                this.OnAdClicked(this, EventArgs.Empty);
            }
        }

        public void onInterstitialShown()
        {
            if (this.OnInterstitialShown != null)
            {
                this.OnInterstitialShown(this, EventArgs.Empty);
            }
        }

        public void onInterstitialDismissed()
        {
            if (this.OnInterstitialDismissed != null)
            {
                this.OnInterstitialDismissed(this, EventArgs.Empty);
            }
        }
        
        public void onImpression(string rawImpressionData)
        {
            if (this.OnImpression != null)
            {
                ImpressionData impressionData = new ImpressionData(rawImpressionData);
                this.OnImpression(this, impressionData);
            }
        }

        public void onInterstitialFailedToShow(string errorReason)
        {
            if (this.OnInterstitialFailedToShow != null)
            {
                AdFailureEventArgs args = new AdFailureEventArgs()
                {
                    Message = errorReason
                };
                this.OnInterstitialFailedToShow(this, args);
            }
        }
    }
}