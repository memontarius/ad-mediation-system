/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2019 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using System;
using System.Runtime.InteropServices;

namespace YandexMobileAds.Platforms.iOS
{
    #if (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
    
    internal class RewardedAdBridge
    {
        [DllImport("__Internal")]
        internal static extern string YMAUnityCreateRewardedAd(IntPtr clientRef,
                                                               string adUnitId);

        [DllImport("__Internal")]
        internal static extern void YMAUnityLoadRewardedAd(string objectId, 
                                                           string adRequestId);

        [DllImport("__Internal")]
        internal static extern bool YMAUnityIsRewardedAdLoaded(string objectId);

        [DllImport("__Internal")]
        internal static extern void YMAUnityShowRewardedAd(string objectId);

        [DllImport("__Internal")]
        internal static extern void YMAUnitySetRewardedAdCallbacks(
            string objectId,
            RewardedAdClient.YMAUnityRewardedAdDidLoadAdCallback rewardedAdDidLoadAdCallback,
            RewardedAdClient.YMAUnityRewardedAdDidFailToLoadAdCallback rewardedAdDidFailToLoadAdCallback,
            RewardedAdClient.YMAUnityRewardedAdWillPresentScreenCallback willPresentScreenCallback,
            RewardedAdClient.YMAUnityRewardedAdWillLeaveApplicationCallback willLeaveCallback,
            RewardedAdClient.YMAUnityRewardedAdDidClickCallback didClickCallback,
            RewardedAdClient.YMAUnityRewardedAdWillAppearCallback rewardedAdWillAppearCallback,
            RewardedAdClient.YMAUnityRewardedAdDidDismissCallback rewardedAdDidDismissCallback,
            RewardedAdClient.YMAUnityRewardedAdDidImpressionTrackedCallback rewardedAdDidImpressionTracked,
            RewardedAdClient.YMAUnityRewardedAdDidFailToShowCallback rewardedAdFailedToShowCallback,
            RewardedAdClient.YMAUnityRewardedAdDidRewardCallback didRewardCallback
        );
    }

    #endif
}
