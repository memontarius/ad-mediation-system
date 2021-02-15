using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    [Serializable]
    public class AdMobSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(AdMobAdapter);

        protected override string AdapterScriptName => "AdMobAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_ADMOB";
        public override bool IsTestDeviceSupported => true;

        public override bool IsAdSupported(AdType adType)
        {
            return true;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return false;
        }

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            AdMobAdapter.SetupNetworkNativeSettings(_iosAppId, _androidAppId);
        }

        public override AdInstanceParameters CreateBannerAdInstanceParameters(string projectName, string instanceName, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            AdMobAdInstanceBannerParameters parameters = AdMobAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameters.Name = instanceName;
            parameters.m_bannerSize = (AdMobAdapter.AdMobBannerSize)bannerType;

            var specificBannerPositions = new AdMobAdInstanceBannerParameters.BannerPosition[bannerPositions.Length];
            for(int i = 0; i < specificBannerPositions.Length; i++)
            {
                var specificPosition = new AdMobAdInstanceBannerParameters.BannerPosition();
                specificPosition.m_placementName = bannerPositions[i].m_placementName;

                switch (bannerPositions[i].m_bannerPosition)
                {
                    case BannerPosition.Bottom:
                        specificPosition.m_bannerPosition = AdMobAdapter.AdMobBannerPosition.Bottom;
                        break;
                    case BannerPosition.Top:
                        specificPosition.m_bannerPosition = AdMobAdapter.AdMobBannerPosition.Top;
                        break;
                }

                specificBannerPositions[i] = specificPosition;
            }
            parameters.m_bannerPositions = specificBannerPositions;

            return parameters;
        }
    }
} // namespace Virterix.AdMediation.Editor