using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Virterix.AdMediation
{
    public class AdMobAdInstanceBannerParameters : AdInstanceParameters
    {
        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "AdMob";
        public const string _PARAMETERS_FILE_NAME = "AdMob_AdInstanceBannerParameters";

        public AdMobAdapter.AdMobBannerSize m_bannerSize;
        public AdMobAdapter.AdMobBannerPosition m_bannerPosition;

        public override AdType AdvertiseType
        {
            get
            {
                return AdType.Banner;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Ad Mediation/AdMob/Create Banner Parameters")]
        public static void CreateParameters()
        {
            AdInstanceParameters.CreateParameters<AdMobAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
        }
#endif
    }
} // namespace Virterix.AdMediation
