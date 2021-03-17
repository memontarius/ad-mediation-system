using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class IronSourceSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(IronSourceAdapter);
        protected override string AdapterScriptName => "IronSourceAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_IRONSOURCE";
        public override bool IsCommonTimeroutSupported => true;

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized || adType == AdType.Banner;
            return isSupported;
        }
        public override string GetNetworkSDKVersion() => IronSourceAdapter.GetSDKVersion();

        public override bool IsAdInstanceSupported(AdType adType) 
        {
            return adType == AdType.Banner;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return adType == AdType.Incentivized;
        }

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            IronSourceAdapter adapter = networkAdapter as IronSourceAdapter;
            adapter.m_timeout = _timeout;
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            var parameterHolder = IronSourceAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameterHolder.m_bannerSize = (IronSourceAdapter.IrnSrcBannerSize)bannerType;

            var specificPositions = new IronSourceAdInstanceBannerParameters.BannerPosition[bannerPositions.Length];
            for (int i = 0; i < specificPositions.Length; i++)
            {
                var specificPosition = new IronSourceAdInstanceBannerParameters.BannerPosition();
                specificPosition.m_placementName = bannerPositions[i].m_placementName;
                specificPosition.m_bannerPosition = (IronSourceAdapter.IrnSrcBannerPosition)ConvertToSpecificBannerPosition(bannerPositions[i].m_bannerPosition);
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
                    specificBannerPosition = (int)IronSourceAdapter.IrnSrcBannerPosition.Bottom;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)IronSourceAdapter.IrnSrcBannerPosition.Top;
                    break;
            }
            return specificBannerPosition;
        }
    }
} // namespace Virterix.AdMediation.Editor
