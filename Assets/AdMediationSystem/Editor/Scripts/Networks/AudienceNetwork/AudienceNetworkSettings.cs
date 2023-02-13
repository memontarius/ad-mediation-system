using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class AudienceNetworkSettings : BaseAdNetworkSettings
    {
        public override bool IsAppIdSupported => false;

        public override Type NetworkAdapterType => typeof(AudienceNetworkAdapter);
        protected override string AdapterScriptName => "AudienceNetworkAdapter";
        protected override string UsingAdapterPreprocessorDirective => "_AMS_AUDIENCE_NETWORK";

        public override string GetNetworkSDKVersion() => AudienceNetworkAdapter.GetSDKVersion();
        
        public override bool IsAdSupported(AdType adType) => true;
        
        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => true;
        
        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, 
            int bannerType, BannerPositionContainer[] bannerPositions, AdInstance adInstance)
        {
            var parameterHolder = AudienceNetworkAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameterHolder.m_bannerSize = (AudienceNetworkAdapter.AudienceNetworkBannerSize)bannerType;
            SetupBannerPositionContainers(parameterHolder, bannerPositions);
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
}
