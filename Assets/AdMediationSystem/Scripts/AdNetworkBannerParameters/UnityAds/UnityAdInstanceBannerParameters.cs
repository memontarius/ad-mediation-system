using System;

namespace Virterix.AdMediation
{
    public class UnityAdInstanceBannerParameters : AdInstanceParameters
    {
        [Serializable]
        public struct BannerPositionContainer
        {
            public string m_placementName;
            public UnityAdsAdapter.UnityAdsBannerPosition m_bannerPosition;
        }

        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "UnityAds";
        public const string _PARAMETERS_FILE_NAME = "UnityAds_AdInstanceBanner";

        public BannerPositionContainer[] m_bannerPositions;

        public override AdType AdvertiseType => AdType.Banner;

#if UNITY_EDITOR
        public static AdInstanceParameters CreateParameters()
        {
            var parameters = CreateParameters<UnityAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static UnityAdInstanceBannerParameters CreateParameters(string projectName, string postfixName)
        {
            var parameters = CreateParameters<UnityAdInstanceBannerParameters>(projectName, _AD_INSTANCE_PARAMETERS_FOLDER, 
                _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
} // namespace Virterix.AdMediation
