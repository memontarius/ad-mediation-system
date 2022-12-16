/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using System;
using System.Runtime.InteropServices;
using YandexMobileAds.Base;
using YandexMobileAds.Common;

namespace YandexMobileAds.Platforms.iOS
{
    #if (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
    
    public class RewardedAdClient : IRewardedAdClient, IDisposable
    {
        private readonly IntPtr selfPointer;

        public string ObjectId { get; private set; }

        internal delegate void YMAUnityRewardedAdDidLoadAdCallback(
            IntPtr rewardedAdClient);

        internal delegate void YMAUnityRewardedAdDidFailToLoadAdCallback(
            IntPtr rewardedAdClient, string error);

        internal delegate void YMAUnityRewardedAdWillPresentScreenCallback(
            IntPtr rewardedAdClient);

        internal delegate void YMAUnityRewardedAdWillLeaveApplicationCallback(
            IntPtr rewardedAdClient);

        internal delegate void YMAUnityRewardedAdDidClickCallback(
            IntPtr rewardedAdClient);

        internal delegate void YMAUnityRewardedAdWillAppearCallback(
            IntPtr rewardedAdClient);

        internal delegate void YMAUnityRewardedAdDidDismissCallback(
            IntPtr rewardedAdClient);

        internal delegate void YMAUnityRewardedAdDidImpressionTrackedCallback(
            IntPtr rewardedAdClient, string rawImpressionData);

        internal delegate void YMAUnityRewardedAdDidFailToShowCallback(
            IntPtr rewardedAdClient, string error);

        internal delegate void YMAUnityRewardedAdDidRewardCallback(
            IntPtr rewardedAdClient, int amount, string type);

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

        public RewardedAdClient(string blockId)
        {
            this.selfPointer = GCHandle.ToIntPtr(GCHandle.Alloc(this));
            this.ObjectId = RewardedAdBridge.YMAUnityCreateRewardedAd(
                this.selfPointer, blockId);
            RewardedAdBridge.YMAUnitySetRewardedAdCallbacks(
                this.ObjectId,
                RewardedAdDidLoadAdCallback,
                RewardedAdDidFailToLoadAdCallback,
                RewardedAdWillPresentScreenCallback,
                RewardedAdWillLeaveApplicationCallback,
                RewardedAdDidClickCallback,
                RewardedAdWillAppearCallback,
                RewardedAdDidDismissCallback,
                RewardedAdDidImpressionTracked,
                RewardedAdDidFailToShowCallback,
                RewardedAdDidRewardCallback);
        }

        public void LoadAd(AdRequest request)
        {
            AdRequestClient adRequest = null;
            if (request != null)
            {
                adRequest = new AdRequestClient(request);
            }
            RewardedAdBridge.YMAUnityLoadRewardedAd(
                this.ObjectId, adRequest.ObjectId);
        }

        public bool IsLoaded()
        {
            return RewardedAdBridge.YMAUnityIsRewardedAdLoaded(this.ObjectId);
        }

        public void Show()
        {
            RewardedAdBridge.YMAUnityShowRewardedAd(this.ObjectId);
        }

        public void Destroy()
        {
            ObjectBridge.YMAUnityDestroyObject(this.ObjectId);
        }

        public void Dispose()
        {
            this.Destroy();
        }

        ~RewardedAdClient()
        {
            this.Destroy();
        }

        private static RewardedAdClient IntPtrToRewardedAdClient(
            IntPtr rewardedAdClient)
        {
            GCHandle handle = GCHandle.FromIntPtr(rewardedAdClient);
            return handle.Target as RewardedAdClient;
        }

        #region RewardedAd callback methods

        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdDidLoadAdCallback))]
        private static void RewardedAdDidLoadAdCallback(
            IntPtr rewardedAdClient)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(
                rewardedAdClient);
            if (client.OnRewardedAdLoaded != null)
            {
                client.OnRewardedAdLoaded(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdDidFailToLoadAdCallback))]
        private static void RewardedAdDidFailToLoadAdCallback(
            IntPtr rewardedAdClient, string error)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(
                rewardedAdClient);
            if (client.OnRewardedAdFailedToLoad != null)
            {
                AdFailureEventArgs args = new AdFailureEventArgs()
                {
                    Message = error
                };
                client.OnRewardedAdFailedToLoad(client, args);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdWillPresentScreenCallback))]
        private static void RewardedAdWillPresentScreenCallback(
            IntPtr rewardedAdClient)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(
                rewardedAdClient);
            if (client.OnLeftApplication != null)
            {
                client.OnLeftApplication(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdWillLeaveApplicationCallback))]
        private static void RewardedAdWillLeaveApplicationCallback(
            IntPtr rewardedAdClient)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(
                rewardedAdClient);
            if (client.OnLeftApplication != null)
            {
                client.OnLeftApplication(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdDidClickCallback))]
        private static void RewardedAdDidClickCallback(
            IntPtr rewardedAdClient)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(
                rewardedAdClient);
            if (client.OnAdClicked != null)
            {
                client.OnAdClicked(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdWillAppearCallback))]
        private static void RewardedAdWillAppearCallback(
            IntPtr rewardedAdClient)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(
                rewardedAdClient);
            if (client.OnRewardedAdShown != null)
            {
                client.OnRewardedAdShown(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdDidDismissCallback))]
        private static void RewardedAdDidDismissCallback(
            IntPtr rewardedAdClient)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(
                rewardedAdClient);
            if (client.OnRewardedAdDismissed != null)
            {
                client.OnRewardedAdDismissed(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdDidImpressionTrackedCallback))]
        private static void RewardedAdDidImpressionTracked(
            IntPtr rewardedAdClient, string rawImpressionData)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(
                rewardedAdClient);
            if (client.OnImpression != null)
            {
                ImpressionData impressionData = new ImpressionData(rawImpressionData == null ? "" : rawImpressionData);
                client.OnImpression(client, impressionData);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdDidFailToShowCallback))]
        private static void RewardedAdDidFailToShowCallback(
            IntPtr rewardedAdClient, string error)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(
                rewardedAdClient);
            if (client.OnRewardedAdFailedToShow != null)
            {
                AdFailureEventArgs args = new AdFailureEventArgs()
                {
                    Message = error
                };
                client.OnRewardedAdFailedToShow(client, args);
            }
        }
       
        [MonoPInvokeCallback(typeof(YMAUnityRewardedAdDidRewardCallback))]
        private static void RewardedAdDidRewardCallback(
            IntPtr rewardedAdClient, int amount, string type)
        {
            RewardedAdClient client = IntPtrToRewardedAdClient(rewardedAdClient);
            Reward reward = new Reward(amount, type);
            if (client.OnRewarded != null)
            {
                client.OnRewarded(client, reward);
            }
        }

        #endregion
    }

    #endif
}
