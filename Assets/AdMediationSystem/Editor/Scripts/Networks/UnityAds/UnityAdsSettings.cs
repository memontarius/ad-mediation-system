using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class UnityAdsSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(UnityAdsAdapter);
        protected override string AdapterScriptName => "UnityAdsAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_UNITY_ADS";

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized || adType == AdType.Banner;
            return isSupported;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return true;
        }

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
           
            var specificPositions = new UnityAdInstanceBannerParameters.BannerPositionContainer[bannerPositions.Length];
            for (int i = 0; i < specificPositions.Length; i++)
            {
                var specificPosition = new UnityAdInstanceBannerParameters.BannerPositionContainer();
                specificPosition.m_placementName = bannerPositions[i].m_placementName;
                specificPosition.m_bannerPosition = (UnityAdsAdapter.UnityAdsBannerPosition)ConvertToSpecificBannerPosition(bannerPositions[i].m_bannerPosition);
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
                    specificBannerPosition = (int)UnityAdsAdapter.UnityAdsBannerPosition.BottomCenter;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)UnityAdsAdapter.UnityAdsBannerPosition.TopCenter;
                    break;
            }
            return specificBannerPosition;
        }
    }
} // namespace Virterix.AdMediation.Editor