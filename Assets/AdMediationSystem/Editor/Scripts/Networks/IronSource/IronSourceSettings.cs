using System;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class IronSourceSettings : BaseAdNetworkSettings
    {
        private const string PLACEMENT_DEFAULT = AdMediationSystem.PLACEMENT_DEFAULT_NAME;

        [Serializable]
        public struct OverriddenPlacement
        {
            public OverriddenPlacement(EditorAdType adType, string originPlacement, string targetPlacement)
            {
                AdvertisingType = adType;
                OriginPlacement = originPlacement;
                TargetPlacement = targetPlacement;
            }
            
            public EditorAdType AdvertisingType;
            public string OriginPlacement;
            public string TargetPlacement;
        }
        
        public IronSourceAdapter.IrnSrcAdType _useAdTypes;
        public List<OverriddenPlacement> _overriddenPlacements = new List<OverriddenPlacement>()
        {
            new OverriddenPlacement(EditorAdType.Banner, PLACEMENT_DEFAULT, "DefaultBanner"),
            new OverriddenPlacement(EditorAdType.Interstitial, PLACEMENT_DEFAULT, "DefaultInterstitial"),
            new OverriddenPlacement(EditorAdType.Incentivized, PLACEMENT_DEFAULT, "DefaultRewardedVideo")
        };
        
        public override Type NetworkAdapterType => typeof(IronSourceAdapter);
        protected override string AdapterScriptName => "IronSourceAdapter";
        protected override string AdapterDefinePreprocessorKey => "_AMS_IRONSOURCE";
        public override bool IsCommonTimeoutSupported => true;

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
            adapter.m_useAdTypes = _useAdTypes;
            
            var overriddenPlacements = _overriddenPlacements.ToArray();
            var adapterOverriddenPlacements = new IronSourceAdapter.OverriddenPlacement[overriddenPlacements.Length];
            for(int i = 0; i < overriddenPlacements.Length; i++)
            {
                var overriddenPlacement = overriddenPlacements[i];
                var adapterOverriddenPlacement = new IronSourceAdapter.OverriddenPlacement(
                    Utils.ConvertEditorAdType((EditorAdType)overriddenPlacement.AdvertisingType),
                    overriddenPlacement.OriginPlacement, overriddenPlacement.TargetPlacement);
                adapterOverriddenPlacements[i] = adapterOverriddenPlacement;
            }
            adapter.m_overriddenPlacements = adapterOverriddenPlacements;
        }

        protected override AdInstanceParameters CreateBannerSpecificAdInstanceParameters(string projectName, string instanceName, 
            int bannerType, BannerPositionContainer[] bannerPositions, AdInstance adInstance)
        {
            var parameterHolder = IronSourceAdInstanceBannerParameters.CreateParameters(projectName, instanceName);
            parameterHolder.m_bannerSize = (IronSourceAdapter.IrnSrcBannerSize)bannerType;
            SetupBannerPositionContainers(parameterHolder, bannerPositions);
            return parameterHolder;
        }

        protected override int ConvertToSpecificBannerPosition(BannerPosition bannerPosition)
        {
            int specificBannerPosition = (int)IronSourceAdapter.IrnSrcBannerPosition.Bottom;
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
}
