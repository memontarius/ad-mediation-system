
using System.Security.Cryptography;
using System.Text;

namespace Virterix.AdMediation
{
    public static class AdUtils
    {
        public static string GetHash(string text)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(text));
            string hashStr = Encoding.ASCII.GetString(hash);
            return hashStr;
        }

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