/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for Unity (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using YandexMobileAds.Common;
using YandexMobileAds.Base;
using YandexMobileAds.Platforms.Android;

namespace YandexMobileAds.Platforms
{
    public class YandexMobileAdsClientFactory
    {
        public static IBannerClient BuildBannerClient(string blockId, AdSize adSize, AdPosition position)
        {
            #if UNITY_EDITOR
                return new YandexMobileAds.Common.DummyBannerClient();
            #elif UNITY_ANDROID
                return new YandexMobileAds.Platforms.Android.BannerClient(blockId, adSize, position);
            #elif (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
                return new YandexMobileAds.Platforms.iOS.BannerClient(blockId, adSize, position);
            #else
                return new YandexMobileAds.Common.DummyBannerClient();
            #endif
        }

        public static IInterstitialClient BuildInterstitialClient(string blockId)
        {
            #if UNITY_EDITOR
                return new YandexMobileAds.Common.DummyInterstitialClient();
            #elif UNITY_ANDROID
                return new YandexMobileAds.Platforms.Android.InterstitialClient(blockId);
            #elif (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
                return new YandexMobileAds.Platforms.iOS.InterstitialClient(blockId);
            #else
                return new YandexMobileAds.Common.DummyInterstitialClient();
            #endif
        }

        public static IRewardedAdClient BuildRewardedAdClient(string blockId)
        {
            #if UNITY_EDITOR
                return new YandexMobileAds.Common.DummyRewardedAdClient();
            #elif UNITY_ANDROID
                return new YandexMobileAds.Platforms.Android.RewardedAdClient(blockId);
            #elif (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
                return new YandexMobileAds.Platforms.iOS.RewardedAdClient(blockId);
            #else
                return new YandexMobileAds.Common.DummyRewardedAdClient();
            #endif
        }

        public static IMobileAdsClient CreateMobileAdsClient()
        {
            #if UNITY_EDITOR
                return new YandexMobileAds.Common.DummyMobileAdsClient();
            #elif UNITY_ANDROID
                return YandexMobileAds.Platforms.Android.MobileAdsClient.GetInstance();
            #elif (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
                return YandexMobileAds.Platforms.iOS.MobileAdsClient.GetInstance();
            #else
                return new YandexMobileAds.Common.DummyMobileAdsClient();
            #endif
        }

        public static IScreenClient CreateScreenClient()
        {
            #if UNITY_EDITOR
                return new YandexMobileAds.Common.DummyScreenClient();
            #elif UNITY_ANDROID
                return YandexMobileAds.Platforms.Android.ScreenClient.GetInstance();
            #elif (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
                return YandexMobileAds.Platforms.iOS.ScreenClient.GetInstance();
            #else
                return new YandexMobileAds.Common.DummyScreenClient();
            #endif
        }
    }
}