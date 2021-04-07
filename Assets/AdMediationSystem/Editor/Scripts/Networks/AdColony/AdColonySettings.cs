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

            var specificPositions = new AdColonyAdInstanceBannerParameters.BannerPositionContainer[bannerPositions.Length];
            for (int i = 0; i < specificPositions.Length; i++)
            {
                var specificPosition = new AdColonyAdInstanceBannerParameters.BannerPositionContainer();
                specificPosition.m_placementName = bannerPositions[i].m_placementName;
                specificPosition.m_bannerPosition = (AdColonyAdapter.AdColonyAdPosition)ConvertToSpecificBannerPosition(bannerPositions[i].m_bannerPosition);
                specificPositions[i] = specificPosition;
            }
            parameterHolder.m_bannerPositions = specificPositions;
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
