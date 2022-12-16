/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

#import <YandexMobileAds/YandexMobileAds.h>
#import "YMAUnityRewardedAd.h"
#import "YMAUnityStringConverter.h"

@interface YMAUnityRewardedAd() <YMARewardedAdDelegate>

@property (nonatomic, assign, readonly) YMAUnityRewardedAdClientRef *clientRef;
@property (nonatomic, strong, readonly) YMARewardedAd *rewardedAd;

@end

@implementation YMAUnityRewardedAd

- (instancetype)initWithClientRef:(YMAUnityRewardedAdClientRef *)clientRef
                         adUnitID:(char *)adUnitID
{
    self = [super init];
    if (self != nil) {
        NSString *adUnitIDString = [[NSString alloc] initWithUTF8String:adUnitID];
        _rewardedAd = [[YMARewardedAd alloc] initWithAdUnitID:adUnitIDString];
        _rewardedAd.delegate = self;
        _clientRef = clientRef;
    }
    return self;
}

- (void)loadWithRequest:(YMAAdRequest *)adRequest
{
    [self.rewardedAd loadWithRequest:adRequest];
}

- (void)show
{
    UIViewController *viewController = [UIApplication sharedApplication].keyWindow.rootViewController;
    [self.rewardedAd presentFromViewController:viewController];
}

- (BOOL)isLoaded
{
    return self.rewardedAd.loaded;
}

- (void)rewardedAdDidLoad:(YMARewardedAd *)rewardedAd
{
    if (self.didLoadAdCallback != NULL) {
        self.didLoadAdCallback(self.clientRef);
    }
}

- (void)rewardedAdDidFailToLoad:(YMARewardedAd *)rewardedAd error:(NSError *)error
{
    if (self.didFailToLoadAdCallback != NULL) {
        char *message = [YMAUnityStringConverter copiedCStringFromObjCString:error.localizedDescription];
        self.didFailToLoadAdCallback(self.clientRef, message);
    }
}

- (void)rewardedAdWillLeaveApplication:(YMARewardedAd *)rewardedAd
{
    if (self.willLeaveApplicationCallback != NULL) {
        self.willLeaveApplicationCallback(self.clientRef);
    }
}

- (void)rewardedAdDidClick:(YMARewardedAd *)rewardedAd
{
    if (self.didClickCallback != NULL) {
        self.didClickCallback(self.clientRef);
    }
}

- (void)rewardedAdWillAppear:(YMARewardedAd *)rewardedAd
{
    if (self.willAppearCallback != NULL) {
        self.willAppearCallback(self.clientRef);
    }
}

- (void)rewardedAdDidDisappear:(YMARewardedAd *)rewardedAd
{
    if (self.didDismissCallback != NULL) {
        self.didDismissCallback(self.clientRef);
    }
}

- (void)rewardedAd:(YMARewardedAd *)rewardedAd
        didTrackImpressionWithData:(nullable id<YMAImpressionData>)impressionData
{
    if (self.didTrackImpressionCallback != NULL) {
        if (impressionData != nil) {
            char *rawData = [YMAUnityStringConverter copiedCStringFromObjCString:impressionData.rawData];
            self.didTrackImpressionCallback(self.clientRef, rawData);
        }
        else {
            self.didTrackImpressionCallback(self.clientRef, nil);
        }
    }
}

- (void)rewardedAdDidFailToPresent:(YMARewardedAd *)rewardedAd error:(NSError *)error
{
    if (self.didFailToShowCallback != NULL) {
        char *message = [YMAUnityStringConverter copiedCStringFromObjCString:error.localizedDescription];
        self.didFailToShowCallback(self.clientRef, message);
    }
}

- (void)rewardedAd:(YMARewardedAd *)rewardedAd willPresentScreen:(UIViewController *)viewController
{
    if (self.willPresentScreenCallback != NULL) {
        self.willPresentScreenCallback(self.clientRef);
    }
}

- (void)rewardedAd:(YMARewardedAd *)rewardedAd didReward:(id<YMAReward>)reward
{
    if (self.didRewardCallback != NULL) {
        char *type = [YMAUnityStringConverter copiedCStringFromObjCString:reward.type];
        int amount = (int)reward.amount;
        self.didRewardCallback(self.clientRef, amount, type);
    }
}

@end
