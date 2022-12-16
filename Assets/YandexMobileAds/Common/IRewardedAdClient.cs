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
    public interface IRewardedAdClient
    {
        /// <summary>
        /// Event fired when rewarded ad has been received.
        /// </summary>
        event EventHandler<EventArgs> OnRewardedAdLoaded;

        /// <summary>
        /// Event fired when rewarded ad has failed to load.
        /// </summary>
        event EventHandler<AdFailureEventArgs> OnRewardedAdFailedToLoad;

        /// <summary>
        /// Event fired when returned to application.
        /// </summary>
        event EventHandler<EventArgs> OnReturnedToApplication;

        /// <summary>
        /// Event fired when rewarded ad is leaving application.
        /// </summary>
        event EventHandler<EventArgs> OnLeftApplication;

        /// <summary>
        /// Event fired when rewarded is clicked.
        /// </summary>
        event EventHandler<EventArgs> OnAdClicked;

        /// <summary>
        /// Event fired when rewarded ad is shown.
        /// </summary>
        event EventHandler<EventArgs> OnRewardedAdShown;

        /// <summary>
        /// Event fired when rewarded ad is dismissed.
        /// </summary>
        event EventHandler<EventArgs> OnRewardedAdDismissed;

        /// <summary>
        /// Event fired when rewarded ad impression tracked.
        /// </summary>
        event EventHandler<ImpressionData> OnImpression;

        /// <summary>
        /// Event fired when rewarded ad has failed to show.
        /// </summary>
        event EventHandler<AdFailureEventArgs> OnRewardedAdFailedToShow;

        /// <summary>
        /// Event fired when the rewarded ad has rewarded the user.
        /// </summary>
        event EventHandler<Reward> OnRewarded;

        /// <summary>
        /// Loads new rewarded ad.
        /// </summary>
        /// <param name="request"></param>
        void LoadAd(AdRequest request);

        /// <summary>
        /// Determines whether rewarded ad has loaded.
        /// </summary>
        /// <returns></returns>
        bool IsLoaded();

        /// <summary>
        /// Shows RewardedAd.
        /// </summary>
        void Show();

        /// <summary>
        /// Destroys RewardedAd.
        /// </summary>
        void Destroy();
    }
}