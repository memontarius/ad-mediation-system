﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Virterix.AdMediation
{
    public class AudienceNetworkAdInstanceBannerParameters : AdInstanceParameters
    {
        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "AudienceNetwork";
        public const string _PARAMETERS_FILE_NAME = "AN_AdInstanceBannerParameters";

        public AudienceNetworkAdapter.AudienceNetworkBannerSize m_bannerSize;
        public AudienceNetworkAdapter.AudienceNetworkBannerPosition m_bannerPosition;

        public override AdType AdvertiseType
        {
            get
            {
                return AdType.Banner;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Ad Mediation/Audience Network/Create Banner Parameters")]
        public static void CreateParameters()
        {
            AdInstanceParameters.CreateParameters<AudienceNetworkAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
        }
#endif
    }
} // namespace Virterix.AdMediation

