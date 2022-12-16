/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2019 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using UnityEngine;
using YandexMobileAds.Base;
using YandexMobileAds.Common;
using System;

namespace YandexMobileAds.Platforms.Android
{
    public class MobileAdsClient : AndroidJavaProxy, IMobileAdsClient
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

        private AndroidJavaClass mobileAdsClass;

        private MobileAdsClient() : base(Utils.MobileAdsClassName)
        {
            this.mobileAdsClass = new AndroidJavaClass(Utils.MobileAdsClassName);
        }

        public void SetUserConsent(bool consent)
        {
            this.mobileAdsClass.CallStatic("setUserConsent", consent);
        }

        public void SetLocationConsent(bool consent)
        {
            this.mobileAdsClass.CallStatic("setLocationConsent", consent);
        }

        public void SetAgeRestrictedUser(bool ageRestrictedUser)
        {
            this.mobileAdsClass.CallStatic("setAgeRestrictedUser", ageRestrictedUser);
        }
    }
}