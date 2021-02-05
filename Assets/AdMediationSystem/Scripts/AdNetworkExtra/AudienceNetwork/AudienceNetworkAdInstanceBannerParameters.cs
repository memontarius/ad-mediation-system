using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Virterix.AdMediation
{
    public class AudienceNetworkAdInstanceBannerParameters : AdInstanceParameters
    {
        [Serializable]
        public struct BannerPosition
        {
            public string m_placementName;
            public AudienceNetworkAdapter.AudienceNetworkBannerPosition m_bannerPosition;
        }

        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "AudienceNetwork";
        public const string _PARAMETERS_FILE_NAME = "AN_AdInstanceBanner";

        public AudienceNetworkAdapter.AudienceNetworkBannerSize m_bannerSize;
        public BannerPosition[] m_bannerPositions;

        public override AdType AdvertiseType
        {
            get
            {
                return AdType.Banner;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Ad Mediation/Audience Network/Create Banner Parameters")]
        public static AdInstanceParameters CreateParameters()
        {
            var parameters = CreateParameters<AudienceNetworkAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static AdInstanceParameters CreateParameters(string projectName, string postfixName)
        {
            var parameters = CreateParameters<AudienceNetworkAdInstanceBannerParameters>(projectName, _AD_INSTANCE_PARAMETERS_FOLDER,
                _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
} // namespace Virterix.AdMediation

