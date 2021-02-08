using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class AudienceNetworkSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(AudienceNetworkAdapter);

        public override bool IsAdSupported(AdType adType)
        {
            return true;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return true;
        }

        public override void SetupNetworkAdapter(Component networkAdapter)
        {
            var adapter = networkAdapter as AudienceNetworkAdapter;
        }

        public override AdInstanceParameters CreateBannerAdInstanceParameters(string projectNme, string name, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            AudienceNetworkAdInstanceBannerParameters parameters = AudienceNetworkAdInstanceBannerParameters.CreateParameters(projectNme, name);
            parameters.Name = name;
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
