using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class UnityAdsSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(UnityAdsAdapter);
        protected override string AdapterScriptName => "UnityAdsAdapter";
        protected override string AdapterDefinePreprocessorKey => "_AMS_UNITY_ADS";

        public override bool IsAdSupported(AdType adType) => true;

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => true;

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
        }

        public override string GetNetworkSDKVersion() => UnityAdsAdapter.GetSDKVersion();

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName,
            int bannerType, BannerPositionContainer[] bannerPositions, AdInstance adInstance)
        {
            var parameterHolder = UnityAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            SetupBannerPositionContainers(parameterHolder, bannerPositions);
            return parameterHolder;
        }

        protected override int ConvertToSpecificBannerPosition(BannerPosition bannerPosition)
        {
            int specificBannerPosition = (int)UnityAdsAdapter.UnityBannerAnchor.BottomCenter;
            switch (bannerPosition)
            {
                case BannerPosition.Bottom:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityBannerAnchor.BottomCenter;
                    break;
                case BannerPosition.BottomLeft:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityBannerAnchor.BottomLeft;
                    break;
                case BannerPosition.BottomRight:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityBannerAnchor.BottomRight;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityBannerAnchor.TopCenter;
                    break;
                case BannerPosition.TopLeft:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityBannerAnchor.TopLeft;
                    break;
                case BannerPosition.TopRight:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityBannerAnchor.TopRight;
                    break;
            }
            return specificBannerPosition;
        }
    }
}