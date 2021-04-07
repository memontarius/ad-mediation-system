using System;

namespace Virterix.AdMediation
{
    public class AppLovinAdInstanceBannerParameters : AdInstanceParameters
    {
        [Serializable]
        public struct BannerPositionContainer
        {
            public string m_placementName;
            public AppLovinAdapter.AppLovinBannerPosition m_bannerPosition;
        }

        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "AppLovin";
        public const string _PARAMETERS_FILE_NAME = "AppLovin_AdInstanceBanner";

        public BannerPositionContainer[] m_bannerPositions;

        public override AdType AdvertiseType => AdType.Banner;

#if UNITY_EDITOR
        public static AdInstanceParameters CreateParameters()
        {
            var parameters = CreateParameters<AppLovinAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static AppLovinAdInstanceBannerParameters CreateParameters(string projectName, string postfixName)
        {
            var parameters = CreateParameters<AppLovinAdInstanceBannerParameters>(projectName, _AD_INSTANCE_PARAMETERS_FOLDER,
                _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
} // Virterix.AdMediation

