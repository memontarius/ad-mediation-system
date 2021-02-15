using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class AudienceNetworkSettings : BaseAdNetworkSettings
    {
        public override bool IsAppIdSupported => false;

        public override Type NetworkAdapterType => typeof(AudienceNetworkAdapter);
        protected override string AdapterScriptName => "AudienceNetworkAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_AUDIENCE_NETWORK";
        public override bool IsTestDeviceSupported => true;

        public override bool IsAdSupported(AdType adType)
        {
            return true;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return true;
        }

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
        }

        public override AdInstanceParameters CreateBannerAdInstanceParameters(string projectName, string instanceName, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            AudienceNetworkAdInstanceBannerParameters parameters = AudienceNetworkAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameters.Name = instanceName;
            parameters.m_bannerSize = (AudienceNetworkAdapter.AudienceNetworkBannerSize)bannerType;

            var specificBannerPositions = new AudienceNetworkAdInstanceBannerParameters.BannerPosition[bannerPositions.Length];
            for (int i = 0; i < specificBannerPositions.Length; i++)
            {
                var specificPosition = new AudienceNetworkAdInstanceBannerParameters.BannerPosition();
                specificPosition.m_placementName = bannerPositions[i].m_placementName;

                switch (bannerPositions[i].m_bannerPosition)
                {
                    case BannerPosition.Bottom:
                        specificPosition.m_bannerPosition = AudienceNetworkAdapter.AudienceNetworkBannerPosition.Bottom;
                        break;
                    case BannerPosition.Top:
                        specificPosition.m_bannerPosition = AudienceNetworkAdapter.AudienceNetworkBannerPosition.Top;
                        break;
                }
                specificBannerPositions[i] = specificPosition;
            }
            parameters.m_bannerPositions = specificBannerPositions;
            return parameters;
        }
    }
} // namespace Virterix.AdMediation.Editor
