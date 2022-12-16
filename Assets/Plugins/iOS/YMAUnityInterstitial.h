/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

#import <Foundation/Foundation.h>
#import "YMAUnityInterstitialTypes.h"

@class YMAAdRequest;

@interface YMAUnityInterstitial : NSObject

- (instancetype)initWithClientRef:(YMAUnityInterstitialClientRef *)clientRef
                         adUnitID:(char *)adUnitID;

@property (nonatomic, assign, readonly) BOOL isLoaded;

@property (nonatomic, assign) YMAUnityInterstitialDidLoadAdCallback didLoadAdCallback;
@property (nonatomic, assign) YMAUnityInterstitialDidFailToLoadAdCallback didFailToLoadAdCallback;
@property (nonatomic, assign) YMAUnityInterstitialWillPresentScreenCallback willPresentScreenCallback;
@property (nonatomic, assign) YMAUnityInterstitialWillLeaveApplicationCallback willLeaveApplicationCallback;
@property (nonatomic, assign) YMAUnityInterstitialDidClickCallback didClickCallback;
@property (nonatomic, assign) YMAUnityInterstitialWillAppearCallback willAppearCallback;
@property (nonatomic, assign) YMAUnityInterstitialDidDismissCallback didDismissCallback;
@property (nonatomic, assign) YMAUnityInterstitialDidTrackImpressionCallback didTrackImpressionCallback;
@property (nonatomic, assign) YMAUnityInterstitialDidFailToShowCallback didFailToShowCallback;

- (void)loadWithRequest:(YMAAdRequest *)adRequest;

- (void)show;

@end
