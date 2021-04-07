using System;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class AdColonySettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(AdColonyAdapter);
        protected override string AdapterScriptName => "AdColonyAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_ADCOLONY";
 
        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized || adType == AdType.Banner;
            return isSupported;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return false;
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            AdColonyAdInstanceBannerParameters parameterHolder = AdColonyAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameterHolder.m_bannerSize = (AdColonyAdapter.AdColonyAdSize)bannerType;
            SetupBannerPositionContainers(parameterHolder, bannerPositions);
            return parameterHolder;
        }

        protected override int ConvertToSpecificBannerPosition(BannerPosition bannerPosition)
        {
            int specificBannerPosition = 0;
            switch (bannerPosition)
            {
                case BannerPosition.Bottom:
                    specificBannerPosition = (int)AdColonyAdapter.AdColonyAdPosition.Bottom;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)AdColonyAdapter.AdColonyAdPosition.Top;
                    break;
            }
            return specificBannerPosition;
        }
    }
} // Virterix.AdMediation.Editor
