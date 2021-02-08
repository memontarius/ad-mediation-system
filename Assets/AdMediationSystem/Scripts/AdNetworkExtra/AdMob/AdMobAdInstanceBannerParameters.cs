using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Virterix.AdMediation
{
    public class AdMobAdInstanceBannerParameters : AdInstanceParameters
    {
        [Serializable]
        public struct BannerPosition
        {
            public string m_placementName;
            public AdMobAdapter.AdMobBannerPosition m_bannerPosition;
        }

        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "AdMob";
        public const string _PARAMETERS_FILE_NAME = "AdMob_AdInstanceBanner";

        public AdMobAdapter.AdMobBannerSize m_bannerSize;
        public BannerPosition[] m_bannerPositions;

        public override AdType AdvertiseType
        {
            get
            {
                return AdType.Banner;
            }
        }

#if UNITY_EDITOR
        //[MenuItem("Tools/Ad Mediation/AdMob/Create Banner Parameters")]
        public static AdInstanceParameters CreateParameters()
        {
            var parameters = CreateParameters<AdMobAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static AdMobAdInstanceBannerParameters CreateParameters(string projectName, string postfixName)
        {
            var parameters = CreateParameters<AdMobAdInstanceBannerParameters>(projectName, _AD_INSTANCE_PARAMETERS_FOLDER,
                _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
} // namespace Virterix.AdMediation
