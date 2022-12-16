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
    
    internal class InterstitialBridge
    {
        [DllImport("__Internal")]
        internal static extern string YMAUnityCreateInterstitial(
            IntPtr clientRef, string adUnitId);

        [DllImport("__Internal")]
        internal static extern void YMAUnityLoadInterstitial(
            string objectId, string adRequestId);

        [DllImport("__Internal")]
        internal static extern bool YMAUnityIsInterstitialLoaded(
            string objectId);

        [DllImport("__Internal")]
        internal static extern void YMAUnityShowInterstitial(string objectId);

        [DllImport("__Internal")]
        internal static extern void YMAUnitySetInterstitialCallbacks(
            string objectId,
            InterstitialClient.YMAUnityInterstitialDidLoadAdCallback interstitialDidLoadAdCallback,
            InterstitialClient.YMAUnityInterstitialDidFailToLoadAdCallback interstitialDidFailToLoadAdCallback,
            InterstitialClient.YMAUnityInterstitialWillPresentScreenCallback willPresentScreenCallback,
            InterstitialClient.YMAUnityInterstitialWillLeaveApplicationCallback willLeaveCallback,
            InterstitialClient.YMAUnityInterstitialDidClickCallback didClickCallback,
            InterstitialClient.YMAUnityInterstitialWillAppearCallback interstitialWillAppearCallback,
            InterstitialClient.YMAUnityInterstitialDidDismissCallback interstitialDidDismissCallback,
            InterstitialClient.YMAUnityInterstitialDidTrackImpressionCallback interstitialDidImpressionTracked,
            InterstitialClient.YMAUnityInterstitialDidFailToShowCallback interstitialFailedToShowCallback);
    }

    #endif
}
