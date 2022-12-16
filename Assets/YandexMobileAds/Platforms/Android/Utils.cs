/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for Android (C) 2019 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using UnityEngine;
using System.Collections.Generic;
using YandexMobileAds.Base;

namespace YandexMobileAds.Platforms.Android
{
    internal class Utils
    {
        public const string AdRequestBuilderClassName = "com.yandex.mobile.ads.common.AdRequest$Builder";

        public const string BannerViewClassName = "com.yandex.mobile.ads.unity.wrapper.banner.BannerWrapper";

        public const string InterstitialClassName =
            "com.yandex.mobile.ads.unity.wrapper.interstitial.InterstitialWrapper";

        public const string RewardedAdClassName =
            "com.yandex.mobile.ads.unity.wrapper.rewarded.RewardedAdWrapper";

        public const string LocationClassName = "android.location.Location";

        public const string MobileAdsClassName = "com.yandex.mobile.ads.common.MobileAds";

        public const string UnityBannerAdListenerClassName =
            "com.yandex.mobile.ads.unity.wrapper.banner.UnityBannerListener";

        public const string UnityInterstitialAdListenerClassName =
            "com.yandex.mobile.ads.unity.wrapper.interstitial.UnityInterstitialListener";

        public const string UnityRewardedAdListenerClassName =
            "com.yandex.mobile.ads.unity.wrapper.rewarded.UnityRewardedAdListener";

        public const string UnityActivityClassName = "com.unity3d.player.UnityPlayer";

        public static AndroidJavaObject GetAdRequestJavaObject(AdRequest request)
        {
            if (request == null)
            {
                return null;
            }

            AndroidJavaObject adRequestBuilder = new AndroidJavaObject(AdRequestBuilderClassName);

            if (request.ContextQuery != null)
            {
                adRequestBuilder.Call<AndroidJavaObject>("setContextQuery", request.ContextQuery);
            }

            if (request.ContextTags != null)
			{
                adRequestBuilder.Call<AndroidJavaObject>("setContextTags",
                    stringListToJavaStringArrayList(request.ContextTags));
            }
            
            if (request.Location != null)
            {
                adRequestBuilder.Call<AndroidJavaObject>("setLocation",
                    locationToJavaLocation(request.Location));
            }

            Dictionary<string, string> parameters = request.Parameters;
            if (parameters != null)
            {
                adRequestBuilder.Call<AndroidJavaObject>("setParameters",
                    dictionaryToJavaHashMap(parameters));
            }

            if (request.Age != null)
            {
                adRequestBuilder.Call<AndroidJavaObject>("setAge",
                    request.Age);
            }

            if (request.Gender != null)
            {
                adRequestBuilder.Call<AndroidJavaObject>("setGender",
                    request.Gender);
            }

            return adRequestBuilder.Call<AndroidJavaObject>("build");
        }

        private static AndroidJavaObject dictionaryToJavaHashMap(Dictionary<string, string> parameters)
        {
            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");

            foreach (KeyValuePair<string, string> entry in parameters)
            {
                map.Call<string>("put", entry.Key, entry.Value);
            }

            return map;
        }

        private static AndroidJavaObject locationToJavaLocation(Location location)
        {
            AndroidJavaObject locationObject = new AndroidJavaObject(LocationClassName, "");

            locationObject.Call("setLatitude", location.Latitude);
            locationObject.Call("setLongitude", location.Longitude);
            locationObject.Call("setAccuracy", (float) location.HorizontalAccuracy);

            return locationObject;
        }

        private static AndroidJavaObject stringListToJavaStringArrayList(List<string> list)
        {
            AndroidJavaObject javaList = new AndroidJavaObject("java.util.ArrayList");

            foreach (string item in list)
            {
                javaList.Call<bool>("add", new object[] {item});
            }

            return javaList;
        }
    }
}