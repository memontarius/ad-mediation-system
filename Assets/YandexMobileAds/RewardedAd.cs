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
using YandexMobileAds.Common;
using YandexMobileAds.Platforms;

namespace YandexMobileAds
{
    /// <summary>
    /// A class is responsible for loading rewarded video ads.
    /// </summary>
    public class RewardedAd
    {
        private AdRequestCreator adRequestFactory;
        private IRewardedAdClient client;
        private volatile bool loaded;

        /// <summary>
        /// Notifies that the ad has been loaded successfully.
        /// </summary>
        public event EventHandler<EventArgs> OnRewardedAdLoaded;

        /// <summary>
        /// Notifies that the ad failed to load.
        /// </summary>
        public event EventHandler<AdFailureEventArgs> OnRewardedAdFailedToLoad;

        /// <summary>
        /// Called when user returned to application after click.
        /// </summary>
        public event EventHandler<EventArgs> OnReturnedToApplication;

        /// <summary>
        /// Notifies that the app will run in the background now because the user clicked on the ad and is about to switch to a different app.
        /// </summary>
        public event EventHandler<EventArgs> OnLeftApplication;

        /// <summary>
        /// Notifies that the user has clicked on the ad.
        /// </summary>
        public event EventHandler<EventArgs> OnAdClicked;

        /// <summary>
        /// Called after the rewarded ad appears.
        /// </summary>
        public event EventHandler<EventArgs> OnRewardedAdShown;

        /// <summary>
        /// Called after hiding the rewarded ad.
        /// </summary>
        public event EventHandler<EventArgs> OnRewardedAdDismissed;

        /// <summary>
        /// Notifies delegate when an impression was tracked.
        /// </summary>
        public event EventHandler<ImpressionData> OnImpression;

        /// <summary>
        /// Notifies that the ad can’t be displayed.
        /// </summary>
        public event EventHandler<AdFailureEventArgs> OnRewardedAdFailedToShow;

        /// <summary>
        /// Notifies that the user should be rewarded for viewing an ad (impression counted).
        /// </summary>
        public event EventHandler<Reward> OnRewarded;

        /// <summary>
        /// Initializes an object of the YMARewardedAd class with a rewarded ad.
        /// </summary>
        /// <param name="blockId">A unique identifier in the R-M-XXXXXX-Y format, which is assigned in the Partner interface.</param>
        public RewardedAd(string blockId)
        {
            this.adRequestFactory = new AdRequestCreator();
            this.client = YandexMobileAdsClientFactory.BuildRewardedAdClient(blockId);
            
            MainThreadDispatcher.initialize();
            ConfigureRewardedAdEvents();
        }

        /// <summary>
        /// Preloads the ad by setting the data for targeting.
        /// </summary>
        /// <param name="request">Data for targeting</param>
        public void LoadAd(AdRequest request)
        {
            this.loaded = false;
            client.LoadAd(adRequestFactory.CreateAdRequest(request));
        }

        /// <summary>
        /// Notifies that the ad is loaded and ready to be displayed.
        /// After the property takes the YES value, the OnRewardedAdLoaded delegate method is called.
        /// </summary>
        /// <returns>
        /// true if this rewarded ad has been successfully loaded
        /// and is ready to be shown, otherwise false.
        /// </returns>
        public bool IsLoaded()
        {
            return loaded;
        }

        /// <summary>
        /// Shows rewarded ad, only if it has been loaded.
        /// </summary>
        public void Show()
        {
            client.Show();
        }

        /// <summary>
        /// Destroys Rewarded entirely and cleans up resources.
        /// </summary>
        public void Destroy()
        {
            client.Destroy();
        }

        private void ConfigureRewardedAdEvents()
        {
            this.client.OnRewardedAdLoaded += (sender, args) =>
            {
                this.loaded = true;
                if (this.OnRewardedAdLoaded != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnRewardedAdLoaded(this, args);
                    });
                }
            };

            this.client.OnRewardedAdFailedToLoad += (sender, args) =>
            {
                if (this.OnRewardedAdFailedToLoad != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnRewardedAdFailedToLoad(this, args);
                    });
                }
            };

            this.client.OnReturnedToApplication += (sender, args) =>
            {
                if (this.OnReturnedToApplication != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnReturnedToApplication(this, args);
                    });
                }
            };

            this.client.OnLeftApplication += (sender, args) =>
            {
                if (this.OnLeftApplication != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnLeftApplication(this, args);
                    });
                }
            };

            this.client.OnAdClicked += (sender, args) =>
            {
                if (this.OnAdClicked != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnAdClicked(this, args);
                    });
                }
            };

            this.client.OnRewardedAdShown += (sender, args) =>
            {
                if (this.OnRewardedAdShown != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnRewardedAdShown(this, args);
                    });
                }
            };

            this.client.OnRewardedAdDismissed += (sender, args) =>
            {
                if (this.OnRewardedAdDismissed != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnRewardedAdDismissed(this, args);
                    });
                }
            };

            this.client.OnImpression += (sender, args) =>
            {
                if (this.OnImpression != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnImpression(this, args);
                    });
                }
            };

            this.client.OnRewardedAdFailedToShow += (sender, args) =>
            {
                if (this.OnRewardedAdFailedToShow != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnRewardedAdFailedToShow(this, args);
                    });
                }
            };

            this.client.OnRewarded += (sender, args) =>
            {
                if (this.OnRewarded != null)
                {
                    MainThreadDispatcher.EnqueueAction(() =>
                    {
                        this.OnRewarded(this, args);
                    });
                }
            };
        }
    }
}