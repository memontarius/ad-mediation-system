using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Virterix.AdMediation.Editor
{
    public class AppodealSettings : BaseAdNetworkSettings
    {
        public AppodealAdapter.RequestedAdsType _requestedAdsTypes;
        
        public override Type NetworkAdapterType => typeof(AppodealAdapter);
        protected override string AdapterScriptName => "AppodealAdapter";
        protected override string UsingAdapterPreprocessorDirective => "_AMS_APPODEAL";

        public override string GetNetworkSDKVersion() => AppodealAdapter.GetSDKVersion();

        public override bool IsAdSupported(AdType adType) =>
            adType == AdType.Interstitial || adType == AdType.Incentivized || adType == AdType.Banner;

        public override bool IsAdInstanceSupported(AdType adType) => adType == AdType.Banner;
        
        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => true;

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            AppodealAdapter adapter = networkAdapter as AppodealAdapter;
            adapter.m_timeout = _timeout;
            adapter.m_requestedAdsType = _requestedAdsTypes;
            
            AppodealAdapter.SetupNetworkNativeSettings(_androidAppId, _iosAppId);
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName,
            string instanceName,
            int bannerType, BannerPositionContainer[] bannerPositions, AdInstance adInstance)
        {
            var parameterHolder = AppodealAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameterHolder.m_bannerSize = (AppodealAdapter.AppodealBannerSize)bannerType;
            SetupBannerPositionContainers(parameterHolder, bannerPositions);
            return parameterHolder;
        }

        protected override int ConvertToSpecificBannerPosition(BannerPosition bannerPosition)
        {
            switch (bannerPosition)
            {
                case BannerPosition.BottomLeft:
                    return (int)AppodealAdapter.AppodealBannerPosition.BottomLeft;
                case BannerPosition.TopLeft:
                    return (int)AppodealAdapter.AppodealBannerPosition.TopLeft;
                case BannerPosition.BottomRight:
                    return (int)AppodealAdapter.AppodealBannerPosition.BottomRight;
                case BannerPosition.TopRight:
                    return (int)AppodealAdapter.AppodealBannerPosition.TopRight;
                case BannerPosition.Top:
                    return (int)AppodealAdapter.AppodealBannerPosition.Top;
                case BannerPosition.Bottom:
                default:
                    return (int)AppodealAdapter.AppodealBannerPosition.Bottom;
            }
        }
    }
}