/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

// Unity rewardedAd client reference is needed to pass banner client in callback.
typedef const void *YMAUnityRewardedAdClientRef;

typedef void (*YMAUnityRewardedAdDidLoadAdCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient);
typedef void (*YMAUnityRewardedAdDidFailToLoadAdCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient, char *error);
typedef void (*YMAUnityRewardedAdWillPresentScreenCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient);
typedef void (*YMAUnityRewardedAdDidDismissScreenCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient);
typedef void (*YMAUnityRewardedAdWillLeaveApplicationCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient);
typedef void (*YMAUnityRewardedAdDidClickCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient);
typedef void (*YMAUnityRewardedAdWillAppearCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient);
typedef void (*YMAUnityRewardedAdDidDismissCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient);
typedef void (*YMAUnityRewardedAdDidImpressionTrackedCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient, char *rawData);
typedef void (*YMAUnityRewardedAdDidFailToShowCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient, char *error);
typedef void (*YMAUnityRewardedAdDidRewardCallback)(YMAUnityRewardedAdClientRef *rewardedAdClient, int amount, char *type);
