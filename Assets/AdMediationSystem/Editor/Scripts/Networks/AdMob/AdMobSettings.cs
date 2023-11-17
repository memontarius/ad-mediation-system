using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    [Serializable]
    public class AdMobSettings : BaseAdNetworkSettings
    {
        public bool _useMediation;
        public int _mediationNetworkFlags;

        public bool _useAppOpenAd;
        public string _androidAppOpenAdUnitId;
        public string _iOSAppOpenAdUnitId;
        public int _appOpenAdDisplayMultiplicity;
        public int _appOpenAdDisplayCooldown;
        public int _appOpenAdLoadAttemptMaxNumber;
        public string _appOpenAdAlternativeNetwork;
        
        public override Type NetworkAdapterType => typeof(AdMobAdapter);

        protected override string AdapterScriptName => "AdMobAdapter";

        protected override string[] AdditionalScriptPaths { get; } = 
        {
            "AdNetworkExtras/AdMob/AdMobAppOpenAdManager",
            "AdNetworkExtras/AdMob/AdMobConsentProvider"
        };

        protected override string UsingAdapterPreprocessorDirective => "_AMS_ADMOB";

        public override string GetNetworkSDKVersion() => AdMobAdapter.GetSDKVersion();

        public override bool IsAdSupported(AdType adType) => true;
        
        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => false;
        
        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            AdMobAdapter.SetupNetworkNativeSettings(_iosAppId, _androidAppId);
            AdMobAdapter adMobAdapter = (AdMobAdapter)networkAdapter;
            adMobAdapter.m_useMediation = _useMediation;
            adMobAdapter.m_useAppOpenAd = _useAppOpenAd;
            if (_useAppOpenAd)
            {
                adMobAdapter.m_androidAppOpenAdId = _androidAppOpenAdUnitId;
                adMobAdapter.m_iOSAppOpenAdId = _iOSAppOpenAdUnitId;
                adMobAdapter.m_appOpenAdDisplayMultiplicity = _appOpenAdDisplayMultiplicity;
                adMobAdapter.m_appOpenAdDisplayCooldown = _appOpenAdDisplayCooldown;
                adMobAdapter.m_appOpenAdLoadAttemptMaxNumber = _appOpenAdLoadAttemptMaxNumber;
                adMobAdapter.m_appOpenAdAlternativeNetwork = _appOpenAdAlternativeNetwork;
            }
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, 
            int bannerType, BannerPositionContainer[] bannerPositions, AdInstance adInstance)
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