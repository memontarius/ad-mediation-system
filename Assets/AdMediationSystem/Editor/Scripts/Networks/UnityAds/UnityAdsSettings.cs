using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class UnityAdsSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(UnityAdsAdapter);
        protected override string AdapterScriptName => "UnityAdsAdapter";
        protected override string AdapterDefinePreprocessorKey => "_AMS_UNITY_ADS";

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized || adType == AdType.Banner;
            return isSupported;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => true;

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
        }

        public override string GetNetworkSDKVersion()
        {
            return UnityAdsAdapter.GetSDKVersion();
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            var parameterHolder = UnityAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            SetupBannerPositionContainers(parameterHolder, bannerPositions);
            return parameterHolder;
        }

        protected override int ConvertToSpecificBannerPosition(BannerPosition bannerPosition)
        {
            int specificBannerPosition = (int)UnityAdsAdapter.UnityAdsBannerPosition.BottomCenter;
            switch (bannerPosition)
            {
                case BannerPosition.Bottom:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityAdsBannerPosition.BottomCenter;
                    break;
                case BannerPosition.BottomLeft:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityAdsBannerPosition.BottomLeft;
                    break;
                case BannerPosition.BottomRight:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityAdsBannerPosition.BottomRight;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityAdsBannerPosition.TopCenter;
                    break;
                case BannerPosition.TopLeft:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityAdsBannerPosition.TopLeft;
                    break;
                case BannerPosition.TopRight:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityAdsBannerPosition.TopRight;
                    break;
            }
            return specificBannerPosition;
        }
    }
}