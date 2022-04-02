using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    [Serializable]
    public class AdMobSettings : BaseAdNetworkSettings
    {
        public bool _useMediation;
        public int _mediationNetworkFlags;

        public override Type NetworkAdapterType => typeof(AdMobAdapter);

        protected override string AdapterScriptName => "AdMobAdapter";

        protected override string AdapterDefinePreprocessorKey => "_AMS_ADMOB";

        public override string GetNetworkSDKVersion() => AdMobAdapter.GetSDKVersion();

        public override bool IsAdSupported(AdType adType) => true;
        
        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => false;
        
        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            AdMobAdapter.SetupNetworkNativeSettings(_iosAppId, _androidAppId);
            ((AdMobAdapter)networkAdapter).m_useMediation = _useMediation;
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
                case BannerPosition.BottomLeft:
                    specificBannerPosition = (int)AdMobAdapter.AdMobBannerPosition.BottomLeft;
                    break;
                case BannerPosition.BottomRight:
                    specificBannerPosition = (int)AdMobAdapter.AdMobBannerPosition.BottomRight;
                    break;
                case BannerPosition.Top:
                    specificBannerPosition = (int)AdMobAdapter.AdMobBannerPosition.Top;
                    break;
                case BannerPosition.TopLeft:
                    specificBannerPosition = (int)AdMobAdapter.AdMobBannerPosition.TopLeft;
                    break;
                case BannerPosition.TopRight:
                    specificBannerPosition = (int)AdMobAdapter.AdMobBannerPosition.TopRight;
                    break;
            }
            return specificBannerPosition;
        }
    }
}