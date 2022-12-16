using UnityEngine;
using YandexMobileAds.Base;

namespace YandexMobileAds.Platforms.Android
{
    internal class AdSizeUtils
    {
        public const string AdSizeClassName = "com.yandex.mobile.ads.banner.AdSize";
        public const string FlexibleSizeMethodName = "flexibleSize";
        public const string StickySizeMethodName = "stickySize";

        public static AndroidJavaObject GetAdSizeJavaObject(AdSize adSize)
        {
            AndroidJavaClass adSizeClass = new AndroidJavaClass(AdSizeClassName);
            AndroidJavaObject adSizeJavaObject = null;
            if (adSize.AdSizeType == AdSizeType.Sticky)
            {
                adSizeJavaObject = adSizeClass.CallStatic<AndroidJavaObject>(StickySizeMethodName, adSize.Width);
            }
            else if (adSize.AdSizeType == AdSizeType.Flexible)
            {
                adSizeJavaObject = adSizeClass.CallStatic<AndroidJavaObject>(FlexibleSizeMethodName, adSize.Width, adSize.Height);
            }
            else if (adSize.AdSizeType == AdSizeType.Fixed)
            {
                adSizeJavaObject = new AndroidJavaObject(AdSizeClassName, adSize.Width, adSize.Height);
            }

            return adSizeJavaObject;
        }
    }
}