/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for Unity (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using System;
using YandexMobileAds.Base;

namespace YandexMobileAds.Common
{
    public interface IInterstitialClient
    {
        /// <summary>
        /// Event fired when interstitial has been received.
        /// </summary>
        event EventHandler<EventArgs> OnInterstitialLoaded;

        /// <summary>
        /// Event fired when interstitial has failed to load.
        /// </summary>
        event EventHandler<AdFailureEventArgs> OnInterstitialFailedToLoad;

        /// <summary>
        /// Event fired when returned to application.
        /// </summary>
        event EventHandler<EventArgs> OnReturnedToApplication;

        /// <summary>
        /// Event fired when interstitial is leaving application.
        /// </summary>
        event EventHandler<EventArgs> OnLeftApplication;

        /// <summary>
        /// Event fired when interstitial is clicked.
        /// </summary>
        event EventHandler<EventArgs> OnAdClicked;

        /// <summary>
        /// Event fired when interstitial is shown.
        /// </summary>
        event EventHandler<EventArgs> OnInterstitialShown;

        /// <summary>
        /// Event fired when interstitial is dismissed.
        /// </summary>
        event EventHandler<EventArgs> OnInterstitialDismissed;

        /// <summary>
        /// Event fired when interstitial impression tracked.
        /// </summary>
        event EventHandler<ImpressionData> OnImpression;

        /// <summary>
        /// Event fired when interstitial has failed to show.
        /// </summary>
        event EventHandler<AdFailureEventArgs> OnInterstitialFailedToShow;

        /// <summary>
        /// Loads new interstitial ad.
        /// </summary>
        /// <param name="request"></param>
        void LoadAd(AdRequest request);

        /// <summary>
        /// Determines whether interstitial has loaded.
        /// </summary>
        /// <returns></returns>
        bool IsLoaded();

        /// <summary>
        /// Shows InterstitialAd.
        /// </summary>
        void Show();

        /// <summary>
        /// Destroys InterstitialAd.
        /// </summary>
        void Destroy();
    }
}