using System;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class AppLovinSettings : BaseAdNetworkSettings
    {
        public string _sdkKey;

        public override bool IsAppIdSupported => false;
        public override Type NetworkAdapterType => typeof(AppLovinAdapter);
        protected override string AdapterScriptName => "AppLovinAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_APPLOVIN";

        public override string GetNetworkSDKVersion()
        {
            return AppLovinAdapter.GetSDKVersion();
        }

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized || adType == AdType.Banner;
            return isSupported;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return false;
        }

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            AppLovinAdapter.SetupNetworkNativeSettings(_sdkKey);
        }

        public override Dictionary<string, object> GetSpecificNetworkParameters(AppPlatform platform)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("sdkKey", _sdkKey);
            return parameters;
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            var parameterHolder = AppLovinAdInstanceBannerParameters.CreateParameters(projectName, instanceName);

            var specificPositions = new AppLovinAdInstanceBannerParameters.BannerPositionContainer[bannerPositions.Length];
            for (int i = 0; i < specificPositions.Length; i++)
            {
                var specificPosition = new AppLovinAdInstanceBannerParameters.BannerPositionContainer();
                specificPosition.m_placementName = bannerPositions[i].m_placementName;
                specificPosition.m_bannerPosition = (AppLovinAdapter.AppLovinBannerPosition)ConvertToSpecificBannerPosition(bannerPositions[i].m_bannerPosition);
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
                    specificBannerPosition = (int)AppLovinAdapter.AppLovinBannerPosition.BottomCenter;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)AppLovinAdapter.AppLovinBannerPosition.TopCenter;
                    break;
            }
            return specificBannerPosition;
        }
    }
} // namespace Virterix.AdMediation.Editor
