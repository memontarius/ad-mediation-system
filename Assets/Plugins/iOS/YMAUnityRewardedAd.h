/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

#import <Foundation/Foundation.h>
#import "YMAUnityRewardedAdTypes.h"

@class YMAAdRequest;

@interface YMAUnityRewardedAd : NSObject

- (instancetype)initWithClientRef:(YMAUnityRewardedAdClientRef *)clientRef
                         adUnitID:(char *)adUnitID;

@property (nonatomic, assign, readonly) BOOL isLoaded;

@property (nonatomic, assign) YMAUnityRewardedAdDidLoadAdCallback didLoadAdCallback;
@property (nonatomic, assign) YMAUnityRewardedAdDidFailToLoadAdCallback didFailToLoadAdCallback;
@property (nonatomic, assign) YMAUnityRewardedAdWillPresentScreenCallback willPresentScreenCallback;
@property (nonatomic, assign) YMAUnityRewardedAdWillLeaveApplicationCallback willLeaveApplicationCallback;
@property (nonatomic, assign) YMAUnityRewardedAdDidClickCallback didClickCallback;
@property (nonatomic, assign) YMAUnityRewardedAdWillAppearCallback willAppearCallback;
@property (nonatomic, assign) YMAUnityRewardedAdDidDismissCallback didDismissCallback;
@property (nonatomic, assign) YMAUnityRewardedAdDidImpressionTrackedCallback didTrackImpressionCallback;
@property (nonatomic, assign) YMAUnityRewardedAdDidFailToShowCallback didFailToShowCallback;
@property (nonatomic, assign) YMAUnityRewardedAdDidRewardCallback didRewardCallback;

- (void)loadWithRequest:(YMAAdRequest *)adRequest;

- (void)show;

@end
