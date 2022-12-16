/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2020 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using YandexMobileAds.Common;

namespace YandexMobileAds.Platforms.iOS
{
#if (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
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

        private ScreenClient() { }

        public float GetScreenScale()
        {
            return ScreenBridge.YMAUnityScreenScale();
        }
    }
#endif
}
