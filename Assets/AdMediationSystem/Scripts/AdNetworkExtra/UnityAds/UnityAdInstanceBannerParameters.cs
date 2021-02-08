using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Virterix.AdMediation
{
    public class UnityAdInstanceBannerParameters : AdInstanceParameters
    {
        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "UnityAds";
        public const string _PARAMETERS_FILE_NAME = "UnityAds_AdInstanceBanner";

        public UnityAdsAdapter.UnityAdsBannerPosition m_bannerPosition;

        public override AdType AdvertiseType
        {
            get
            {
                return AdType.Banner;
            }
        }

#if UNITY_EDITOR
        //[MenuItem("Tools/Ad Mediation/UnityAds/Create Banner Parameters")]
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
