using System;

namespace Virterix.AdMediation.Editor
{
    public class AdColonySettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(AdColonyAdapter);
        protected override string AdapterScriptName => "AdColonyAdapter";
        protected override string AdapterDefinePreprocessorKey => "_AMS_ADCOLONY";
 
        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized || adType == AdType.Banner;
            return isSupported;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => true;

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, 
            int bannerType, BannerPositionContainer[] bannerPositions, AdInstance adInstance)
        {
            AdColonyAdInstanceBannerParameters parameterHolder = AdColonyAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameterHolder.m_bannerSize = (AdColonyAdapter.AdColonyAdSize)bannerType;
            SetupBannerPositionContainers(parameterHolder, bannerPositions);
            return parameterHolder;
        }

        protected override int ConvertToSpecificBannerPosition(BannerPosition bannerPosition)
        {
            int specificBannerPosition = 0;
            switch (bannerPosition)
            {
                case BannerPosition.Bottom:
                    specificBannerPosition = (int)AdColonyAdapter.AdColonyAdPosition.Bottom;
                    break;
                case BannerPosition.BottomLeft:
                    specificBannerPosition = (int)AdColonyAdapter.AdColonyAdPosition.BottomLeft;
                    break;
                case BannerPosition.BottomRight:
                    specificBannerPosition = (int)AdColonyAdapter.AdColonyAdPosition.BottomRight;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)AdColonyAdapter.AdColonyAdPosition.Top;
                    break;
                case BannerPosition.TopLeft:
                    specificBannerPosition = (int)AdColonyAdapter.AdColonyAdPosition.TopLeft;
                    break;
                case BannerPosition.TopRight:
                    specificBannerPosition = (int)AdColonyAdapter.AdColonyAdPosition.TopRight;
                    break;
            }
            return specificBannerPosition;
        }
    }
}
