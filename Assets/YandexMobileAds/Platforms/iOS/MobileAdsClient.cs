/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2019 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using YandexMobileAds.Common;

namespace YandexMobileAds.Platforms.iOS
{
    #if (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
    
    public class MobileAdsClient : IMobileAdsClient
    {
        private static MobileAdsClient instance;

        private static object lockObject = new object();

        public static MobileAdsClient GetInstance()
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                        instance = new MobileAdsClient();
                }
            }
            return instance;
        }

        private MobileAdsClient() { }

        public void SetUserConsent(bool consent) 
        {
            MobileAdsBridge.YMAUnitySetUserConsent(consent);
        }

        public void SetLocationConsent(bool consent) 
        {
            MobileAdsBridge.YMAUnitySetLocationConsent(consent);
        }

        public void SetAgeRestrictedUser(bool ageRestrictedUser)
        {
            // TODO(delank): ADLIB-13105 add COPPA for iOS
        }
    }

    #endif
}