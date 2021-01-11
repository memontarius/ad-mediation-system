using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation
{
    public enum AdType
    {
        Unknown = 0,
        Banner,
        Interstitial,
        Incentivized
    }

    public static class AdTypeConvert
    {
        public static AdType StringToAdType(string adTypeName)
        {

            AdType adType = AdType.Unknown;
            switch (adTypeName)
            {
                case "banner":
                    adType = AdType.Banner;
                    break;
                case "interstitial":
                    adType = AdType.Interstitial;
                    break;
                case "incentivized":
                    adType = AdType.Incentivized;
                    break;
            }

            return adType;
        }

        public static string AdTypeToString(AdType adType)
        {
            string adTypeName = "";
            switch (adType)
            {
                case AdType.Banner:
                    adTypeName = "banner";
                    break;
                case AdType.Interstitial:
                    adTypeName = "interstitial";
                    break;
                case AdType.Incentivized:
                    adTypeName = "incentivized";
                    break;
            }

            return adTypeName;
        }
    }
} // namespace Virterix.AdMediation