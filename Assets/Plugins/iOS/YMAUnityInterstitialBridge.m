/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

#import "YMAUnityInterstitial.h"
#import "YMAUnityObjectsStorage.h"
#import "YMAUnityStringConverter.h"
#import "YMAUnityObjectIDProvider.h"

char *YMAUnityCreateInterstitial(YMAUnityInterstitialClientRef *clientRef, char *adUnitID)
{
    YMAUnityInterstitial *interstitial = [[YMAUnityInterstitial alloc] initWithClientRef:clientRef
                                                                                adUnitID:adUnitID];
    const char *objectID = [YMAUnityObjectIDProvider IDForObject:interstitial];
    [[YMAUnityObjectsStorage sharedInstance] setObject:interstitial withID:objectID];
    return [YMAUnityStringConverter copiedCString:objectID];
}

void YMAUnitySetInterstitialCallbacks(char *objectID,
                                      YMAUnityInterstitialDidLoadAdCallback didLoadAdCallback,
                                      YMAUnityInterstitialDidFailToLoadAdCallback didFailToLoadAdCallback,
                                      YMAUnityInterstitialWillPresentScreenCallback willPresentScreenCallback,
                                      YMAUnityInterstitialWillLeaveApplicationCallback willLeaveApplicationCallback,
                                      YMAUnityInterstitialDidClickCallback didClickCallback,
                                      YMAUnityInterstitialWillAppearCallback willAppearCallback,
                                      YMAUnityInterstitialDidDismissCallback didDismissCallback,
                                      YMAUnityInterstitialDidTrackImpressionCallback didTrackImpressionCallback,
                                      YMAUnityInterstitialDidFailToShowCallback didFailToShowCallback)
{
    YMAUnityInterstitial *interstitial = [[YMAUnityObjectsStorage sharedInstance] objectWithID:objectID];
    interstitial.didLoadAdCallback = didLoadAdCallback;
    interstitial.didFailToLoadAdCallback = didFailToLoadAdCallback;
    interstitial.willPresentScreenCallback = willPresentScreenCallback;
    interstitial.willLeaveApplicationCallback = willLeaveApplicationCallback;
    interstitial.didClickCallback = didClickCallback;
    interstitial.willAppearCallback = willAppearCallback;
    interstitial.didDismissCallback = didDismissCallback;
    interstitial.didTrackImpressionCallback = didTrackImpressionCallback;
    interstitial.didFailToShowCallback = didFailToShowCallback;
}

void YMAUnityLoadInterstitial(char *objectID, char *adRequestID)
{
    YMAAdRequest *adRequest = [[YMAUnityObjectsStorage sharedInstance] objectWithID:adRequestID];
    YMAUnityInterstitial *interstitial = [[YMAUnityObjectsStorage sharedInstance] objectWithID:objectID];
    [interstitial loadWithRequest:adRequest];
}

void YMAUnityShowInterstitial(char *objectID)
{
    YMAUnityInterstitial *interstitial = [[YMAUnityObjectsStorage sharedInstance] objectWithID:objectID];
    [interstitial show];
}

BOOL YMAUnityIsInterstitialLoaded(char *objectID)
{
    YMAUnityInterstitial *interstitial = [[YMAUnityObjectsStorage sharedInstance] objectWithID:objectID];
    return interstitial.isLoaded;
}

void YMAUnityDestroyInterstitial(char *objectID)
{
    [[YMAUnityObjectsStorage sharedInstance] removeObjectWithID:objectID];
}
