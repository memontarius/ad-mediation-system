/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

// Unity interstitial client reference is needed to pass banner client in callback.
typedef const void *YMAUnityInterstitialClientRef;

typedef void (*YMAUnityInterstitialDidLoadAdCallback)(YMAUnityInterstitialClientRef *interstitialClient);
typedef void (*YMAUnityInterstitialDidFailToLoadAdCallback)(YMAUnityInterstitialClientRef *interstitialClient, char *error);
typedef void (*YMAUnityInterstitialWillPresentScreenCallback)(YMAUnityInterstitialClientRef *interstitialClient);
typedef void (*YMAUnityInterstitialDidDismissScreenCallback)(YMAUnityInterstitialClientRef *interstitialClient);
typedef void (*YMAUnityInterstitialWillLeaveApplicationCallback)(YMAUnityInterstitialClientRef *interstitialClient);
typedef void (*YMAUnityInterstitialDidClickCallback)(YMAUnityInterstitialClientRef *interstitialClient);
typedef void (*YMAUnityInterstitialWillAppearCallback)(YMAUnityInterstitialClientRef *interstitialClient);
typedef void (*YMAUnityInterstitialDidDismissCallback)(YMAUnityInterstitialClientRef *interstitialClient);
typedef void (*YMAUnityInterstitialDidTrackImpressionCallback)(YMAUnityInterstitialClientRef *interstitialClient, char *rawData);
typedef void (*YMAUnityInterstitialDidFailToShowCallback)(YMAUnityInterstitialClientRef *interstitialClient, char *error);
