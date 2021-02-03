using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Virterix.AdMediation
{
    public class UnityAdInstanceBannerParameters : AdInstanceParameters
    {
        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "UnityAds";
        public const string _PARAMETERS_FILE_NAME = "UnityAds_AdInstanceBannerParameters";

        public UnityAdsAdapter.UnityAdsBannerPosition m_bannerPosition;

        public override AdType AdvertiseType
        {
            get
            {
                return AdType.Banner;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Ad Mediation/UnityAds/Create Banner Parameters")]
        public static AdInstanceParameters CreateParameters()
        {
            AdInstanceParameters parameters = AdInstanceParameters.CreateParameters<UnityAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static AdInstanceParameters CreateParameters(string postfixName)
        {
            AdInstanceParameters parameters = AdInstanceParameters.CreateParameters<UnityAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
} // namespace Virterix.AdMediation
