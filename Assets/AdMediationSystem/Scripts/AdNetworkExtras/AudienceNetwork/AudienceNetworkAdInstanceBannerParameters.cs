using System;

namespace Virterix.AdMediation
{
    public class AudienceNetworkAdInstanceBannerParameters : AdInstanceParameters
    {
        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "AudienceNetwork";
        public const string _PARAMETERS_FILE_NAME = "AN_AdInstanceBanner";

        public AudienceNetworkAdapter.AudienceNetworkBannerSize m_bannerSize;

        public override AdType AdvertiseType => AdType.Banner;

#if UNITY_EDITOR
        public static AdInstanceParameters CreateParameters()
        {
            var parameters = CreateParameters<AudienceNetworkAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static AudienceNetworkAdInstanceBannerParameters CreateParameters(string projectName, string postfixName)
        {
            var parameters = CreateParameters<AudienceNetworkAdInstanceBannerParameters>(projectName, _AD_INSTANCE_PARAMETERS_FOLDER,
                _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
} // namespace Virterix.AdMediation

