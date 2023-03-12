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
    public class RewardedAdClient : AndroidJavaProxy, IRewardedAdClient
    {
        private AndroidJavaObject rewardedAd;
        
        public event EventHandler<EventArgs> OnRewardedAdLoaded;
        public event EventHandler<AdFailureEventArgs> OnRewardedAdFailedToLoad;
        public event EventHandler<EventArgs> OnReturnedToApplication;
        public event EventHandler<EventArgs> OnLeftApplication;
        public event EventHandler<EventArgs> OnAdClicked;
        public event EventHandler<EventArgs> OnRewardedAdShown;
        public event EventHandler<EventArgs> OnRewardedAdDismissed;
        public event EventHandler<ImpressionData> OnImpression;
        public event EventHandler<AdFailureEventArgs> OnRewardedAdFailedToShow;
        public event EventHandler<Reward> OnRewarded;

        public RewardedAdClient(string blockId) : base(Utils.UnityRewardedAdListenerClassName)
        {
            AndroidJavaClass playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);

            AndroidJavaObject activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");

            this.rewardedAd = new AndroidJavaObject(
                Utils.RewardedAdClassName,
                activity,
                blockId);
            this.rewardedAd.Call("setUnityRewardedAdListener", this);
        }

        public void LoadAd(AdRequest request)
        {
            this.rewardedAd.Call("loadAd", Utils.GetAdRequestJavaObject(request));
        }

        public bool IsLoaded()
        {
            return this.rewardedAd.Call<bool>("isRewardedAdLoaded");
        }

        public void Show()
        {
            this.rewardedAd.Call("showRewardedAd");
        }

        public void Destroy()
        {
            this.rewardedAd.Call("clearUnityRewardedAdListener");
            this.rewardedAd.Call("destroyRewardedAd");
        }

        public void onRewardedAdLoaded()
        {
            if (this.OnRewardedAdLoaded != null)
            {
                this.OnRewardedAdLoaded(this, EventArgs.Empty);
            }
        }

        public void onRewardedAdFailedToLoad(string errorReason)
        {
            if (this.OnRewardedAdFailedToLoad != null)
            {
                AdFailureEventArgs args = new AdFailureEventArgs()
                {
                    Message = errorReason
                };
                this.OnRewardedAdFailedToLoad(this, args);
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

        public void onRewardedAdShown()
        {
            if (this.OnRewardedAdShown != null)
            {
                this.OnRewardedAdShown(this, EventArgs.Empty);
            }
        }

        public void onRewardedAdDismissed()
        {
            if (this.OnRewardedAdDismissed != null)
            {
                this.OnRewardedAdDismissed(this, EventArgs.Empty);
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

        public void onRewardedAdFailedToShow(string errorReason)
        {
            if (this.OnRewardedAdFailedToShow != null)
            {
                AdFailureEventArgs args = new AdFailureEventArgs()
                {
                    Message = errorReason
                };
                this.OnRewardedAdFailedToShow(this, args);
            }
        }

        public void onRewarded(int amount, string type)
        {
            if (this.OnRewarded != null)
            {
                Reward reward = new Reward(amount, type);
                this.OnRewarded(this, reward);
            }
        }
    }
}