using System;
using UnityEngine;
using UnityEditor;

namespace Virterix.AdMediation.Editor
{
    [Serializable]
    public class AdMobSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(AdMobAdapter);

        protected override string AdapterScriptName => "AdMobAdapter";

        protected override string AdapterDefinePeprocessorKey => "_AMS_ADMOB";

        public override string GetNetworkSDKVersion() => AdMobAdapter.GetSDKVersion();

        public override bool IsAdSupported(AdType adType) => true;
        
        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => false;
        
        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            AdMobAdapter.SetupNetworkNativeSettings(_iosAppId, _androidAppId);
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            AdMobAdInstanceBannerParameters parameterHolder = AdMobAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameterHolder.m_bannerSize = (AdMobAdapter.AdMobBannerSize)bannerType;
            SetupBannerPositionContainers(parameterHolder, bannerPositions);
            return parameterHolder;
        }

        protected override int ConvertToSpecificBannerPosition(BannerPosition bannerPosition)
        {
            int specificBannerPosition = 0;
            switch (bannerPosition)
            {
                case BannerPosition.Bottom:
                    specificBannerPosition = (int)AdMobAdapter.AdMobBannerPosition.Bottom;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)AdMobAdapter.AdMobBannerPosition.Top;
                    break;
            }
            return specificBannerPosition;
        }
    }
} // namespace Virterix.AdMediation.Editor