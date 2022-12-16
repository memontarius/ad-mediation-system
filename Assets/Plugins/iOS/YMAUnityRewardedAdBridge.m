/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

#import "YMAUnityRewardedAd.h"
#import "YMAUnityObjectsStorage.h"
#import "YMAUnityStringConverter.h"
#import "YMAUnityObjectIDProvider.h"

char *YMAUnityCreateRewardedAd(YMAUnityRewardedAdClientRef *clientRef, char *adUnitID)
{
    YMAUnityRewardedAd *rewardedAd = [[YMAUnityRewardedAd alloc] initWithClientRef:clientRef
                                                                          adUnitID:adUnitID];
    const char *objectID = [YMAUnityObjectIDProvider IDForObject:rewardedAd];
    [[YMAUnityObjectsStorage sharedInstance] setObject:rewardedAd withID:objectID];
    return [YMAUnityStringConverter copiedCString:objectID];
}

void YMAUnitySetRewardedAdCallbacks(char *objectID,
                                    YMAUnityRewardedAdDidLoadAdCallback didLoadAdCallback,
                                    YMAUnityRewardedAdDidFailToLoadAdCallback didFailToLoadAdCallback,
                                    YMAUnityRewardedAdWillPresentScreenCallback willPresentScreenCallback,
                                    YMAUnityRewardedAdWillLeaveApplicationCallback willLeaveApplicationCallback,
                                    YMAUnityRewardedAdDidClickCallback didClickCallback,
                                    YMAUnityRewardedAdWillAppearCallback willAppearCallback,
                                    YMAUnityRewardedAdDidDismissCallback didDismissCallback,
                                    YMAUnityRewardedAdDidImpressionTrackedCallback didTrackImpressionCallback,
                                    YMAUnityRewardedAdDidFailToShowCallback didFailToShowCallback,
                                    YMAUnityRewardedAdDidRewardCallback didRewardCallback)
{
    YMAUnityRewardedAd *rewardedAd = [[YMAUnityObjectsStorage sharedInstance] objectWithID:objectID];
    rewardedAd.didLoadAdCallback = didLoadAdCallback;
    rewardedAd.didFailToLoadAdCallback = didFailToLoadAdCallback;
    rewardedAd.willPresentScreenCallback = willPresentScreenCallback;
    rewardedAd.willLeaveApplicationCallback = willLeaveApplicationCallback;
    rewardedAd.didClickCallback = didClickCallback;
    rewardedAd.willAppearCallback = willAppearCallback;
    rewardedAd.didDismissCallback = didDismissCallback;
    rewardedAd.didTrackImpressionCallback = didTrackImpressionCallback;
    rewardedAd.didFailToShowCallback = didFailToShowCallback;
    rewardedAd.didRewardCallback = didRewardCallback;
}

void YMAUnityLoadRewardedAd(char *objectID, char *adRequestID)
{
    YMAAdRequest *adRequest = [[YMAUnityObjectsStorage sharedInstance] objectWithID:adRequestID];
    YMAUnityRewardedAd *rewardedAd = [[YMAUnityObjectsStorage sharedInstance] objectWithID:objectID];
    [rewardedAd loadWithRequest:adRequest];
}

void YMAUnityShowRewardedAd(char *objectID)
{
    YMAUnityRewardedAd *rewardedAd = [[YMAUnityObjectsStorage sharedInstance] objectWithID:objectID];
    [rewardedAd show];
}

BOOL YMAUnityIsRewardedAdLoaded(char *objectID)
{
    YMAUnityRewardedAd *rewardedAd = [[YMAUnityObjectsStorage sharedInstance] objectWithID:objectID];
    return rewardedAd.isLoaded;
}

void YMAUnityDestroyRewardedAd(char *objectID)
{
    [[YMAUnityObjectsStorage sharedInstance] removeObjectWithID:objectID];
}
