using System;
using UnityEngine;
using UnityEditor;

namespace Virterix.AdMediation.Editor
{
    public class AudienceNetworkSettings : BaseAdNetworkSettings
    {
        public override bool IsAppIdSupported => false;

        public override Type NetworkAdapterType => typeof(AudienceNetworkAdapter);
        protected override string AdapterScriptName => "AudienceNetworkAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_AUDIENCE_NETWORK";

        public override string GetNetworkSDKVersion() => AudienceNetworkAdapter.GetSDKVersion();
        
        public override bool IsAdSupported(AdType adType) => true;
        
        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => true;
        
        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            var parameterHolder = AudienceNetworkAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameterHolder.m_bannerSize = (AudienceNetworkAdapter.AudienceNetworkBannerSize)bannerType;

            var specificPositions = new AudienceNetworkAdInstanceBannerParameters.BannerPositionContainer[bannerPositions.Length];
            for (int i = 0; i < specificPositions.Length; i++)
            {
                var specificPosition = new AudienceNetworkAdInstanceBannerParameters.BannerPositionContainer();
                specificPosition.m_placementName = bannerPositions[i].m_placementName;
                specificPosition.m_bannerPosition = (AudienceNetworkAdapter.AudienceNetworkBannerPosition)ConvertToSpecificBannerPosition(bannerPositions[i].m_bannerPosition);
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
                    specificBannerPosition = (int)AudienceNetworkAdapter.AudienceNetworkBannerPosition.Bottom;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)AudienceNetworkAdapter.AudienceNetworkBannerPosition.Top;
                    break;
            }
            return specificBannerPosition;
        }
    }
} // namespace Virterix.AdMediation.Editor
