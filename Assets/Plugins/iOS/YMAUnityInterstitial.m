/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

#import <YandexMobileAds/YandexMobileAds.h>
#import "YMAUnityInterstitial.h"
#import "YMAUnityStringConverter.h"

@interface YMAUnityInterstitial() <YMAInterstitialAdDelegate>

@property (nonatomic, assign, readonly) YMAUnityInterstitialClientRef *clientRef;
@property (nonatomic, strong, readonly) YMAInterstitialAd *interstitial;

@end

@implementation YMAUnityInterstitial

- (instancetype)initWithClientRef:(YMAUnityInterstitialClientRef *)clientRef
                         adUnitID:(char *)adUnitID
{
    self = [super init];
    if (self != nil) {
        NSString *adUnitIDStrig = [[NSString alloc] initWithUTF8String:adUnitID];
        _interstitial = [[YMAInterstitialAd alloc] initWithAdUnitID:adUnitIDStrig];
        _interstitial.delegate = self;
        _clientRef = clientRef;
    }
    return self;
}

- (void)loadWithRequest:(YMAAdRequest *)adRequest
{
    [self.interstitial loadWithRequest:adRequest];
}

- (void)show
{
    UIViewController *viewController = [UIApplication sharedApplication].keyWindow.rootViewController;
    [self.interstitial presentFromViewController:viewController];
}

- (BOOL)isLoaded
{
    return self.interstitial.loaded;
}

- (void)interstitialAdDidLoad:(YMAInterstitialAd *)interstitial
{
    if (self.didLoadAdCallback != NULL) {
        self.didLoadAdCallback(self.clientRef);
    }
}

- (void)interstitialAdDidFailToLoad:(YMAInterstitialAd *)interstitial error:(NSError *)error
{
    if (self.didFailToLoadAdCallback != NULL) {
        char *message = [YMAUnityStringConverter copiedCStringFromObjCString:error.localizedDescription];
        self.didFailToLoadAdCallback(self.clientRef, message);
    }
}

- (void)interstitialWillLeaveApplication:(YMAInterstitialAd *)interstitial
{
    if (self.willLeaveApplicationCallback != NULL) {
        self.willLeaveApplicationCallback(self.clientRef);
    }
}

- (void)interstitialAdDidClick:(YMAInterstitialAd *)interstitialAd
{
    if (self.didClickCallback != NULL) {
        self.didClickCallback(self.clientRef);
    }
}

- (void)interstitialAdWillAppear:(YMAInterstitialAd *)interstitial
{
    if (self.willAppearCallback != NULL) {
        self.willAppearCallback(self.clientRef);
    }
}

- (void)interstitialAdDidDisappear:(YMAInterstitialAd *)interstitial
{
    if (self.didDismissCallback != NULL) {
        self.didDismissCallback(self.clientRef);
    }
}

- (void)interstitialAd:(YMAInterstitialAd *)interstitialAd
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

- (void)interstitialAdDidFailToPresent:(YMAInterstitialAd *)interstitial error:(NSError *)error
{
    if (self.didFailToShowCallback != NULL) {
        char *message = [YMAUnityStringConverter copiedCStringFromObjCString:error.localizedDescription];
        self.didFailToShowCallback(self.clientRef, message);
    }
}

- (void)interstitialAd:(YMAInterstitialAd *)interstitialAd willPresentScreen:(UIViewController *)webBrowser
{
    if (self.willPresentScreenCallback != NULL) {
        self.willPresentScreenCallback(self.clientRef);
    }
}

@end
