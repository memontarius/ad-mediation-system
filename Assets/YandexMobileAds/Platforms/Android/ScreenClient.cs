/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for Unity (C) 2020 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using UnityEngine;
using YandexMobileAds.Common;

namespace YandexMobileAds.Platforms.Android
{
    public class ScreenClient : IScreenClient
    {
        private static ScreenClient instance;

        private static object lockObject = new object();

        public static ScreenClient GetInstance()
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                        instance = new ScreenClient();
                }
            }
            return instance;
        }

        public float GetScreenScale()
        {
            var playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            var activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            var resources = activity.Call<AndroidJavaObject>("getResources");
            var metrics = resources.Call<AndroidJavaObject>("getDisplayMetrics");
            return metrics.Get<float>("density");
        }
    }
}