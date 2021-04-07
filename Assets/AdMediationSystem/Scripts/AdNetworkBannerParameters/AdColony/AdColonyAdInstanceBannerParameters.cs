using System;

namespace Virterix.AdMediation
{
    public class AdColonyAdInstanceBannerParameters : AdInstanceParameters
    {
        [Serializable]
        public struct BannerPositionContainer
        {
            public string m_placementName;
            public AdColonyAdapter.AdColonyAdPosition m_bannerPosition;
        }

        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "AdColony";
        public const string _PARAMETERS_FILE_NAME = "AdColony_AdInstanceBanner";

        public AdColonyAdapter.AdColonyAdSize m_bannerSize;
        public BannerPositionContainer[] m_bannerPositions;

        public override AdType AdvertiseType => AdType.Banner;

#if UNITY_EDITOR
        public static AdInstanceParameters CreateParameters()
        {
            var parameters = CreateParameters<AdColonyAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static AdColonyAdInstanceBannerParameters CreateParameters(string projectName, string postfixName)
        {
            var parameters = CreateParameters<AdColonyAdInstanceBannerParameters>(projectName, _AD_INSTANCE_PARAMETERS_FOLDER,
                _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
} // namespace Virterix.AdMediation