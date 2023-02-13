using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class YandexMobileAdsSettings: BaseAdNetworkSettings
    {
        public override bool IsAdSupported(AdType adType) => true;
        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => true;
        public override bool IsAppIdSupported => false;
        public override Type NetworkAdapterType => typeof(YandexMobileAdsAdapter);
        protected override string AdapterScriptName => "YandexMobileAdsAdapter";
        protected override string UsingAdapterPreprocessorDirective => "_AMS_YANDEX_MOBILE_ADS";
        
        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
        }
        
        public override string GetNetworkSDKVersion() => YandexMobileAdsAdapter.GetSDKVersion();
        
        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName,
            int bannerType, BannerPositionContainer[] bannerPositions, AdInstance adInstance)
        {
            YandexAdInstanceBannerParameters parameterHolder = YandexAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            SetupBannerPositionContainers(parameterHolder, bannerPositions);
            parameterHolder.m_maxHeight = adInstance._bannerMaxHeight;
            parameterHolder.m_refreshTime = adInstance._bannerRefreshTime;
            return parameterHolder;
        }
        
        protected override int ConvertToSpecificBannerPosition(BannerPosition bannerPosition)
        {
            int specificBannerPosition = (int)YandexMobileAdsAdapter.YandexBannerPosition.BottomCenter;
            switch (bannerPosition)
            {
                case BannerPosition.Bottom:
                    specificBannerPosition = (int)YandexMobileAdsAdapter.YandexBannerPosition.BottomCenter;
                    break;
                case BannerPosition.BottomLeft:
                    specificBannerPosition = (int)YandexMobileAdsAdapter.YandexBannerPosition.BottomLeft;
                    break;
                case BannerPosition.BottomRight:
                    specificBannerPosition = (int)YandexMobileAdsAdapter.YandexBannerPosition.BottomRight;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)YandexMobileAdsAdapter.YandexBannerPosition.TopCenter;
                    break;
                case BannerPosition.TopLeft:
                    specificBannerPosition = (int)YandexMobileAdsAdapter.YandexBannerPosition.TopLeft;
                    break;
                case BannerPosition.TopRight:
                    specificBannerPosition = (int)YandexMobileAdsAdapter.YandexBannerPosition.TopRight;
                    break;
            }
            return specificBannerPosition;
        }
    }
}