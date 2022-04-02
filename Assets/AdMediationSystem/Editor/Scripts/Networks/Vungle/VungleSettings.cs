using System;

namespace Virterix.AdMediation.Editor
{
    public class VungleSettings : BaseAdNetworkSettings
    {
        public override bool IsAppIdSupported => true;
        public override Type NetworkAdapterType => typeof(VungleAdapter);
        protected override string AdapterScriptName => "VungleAdapter";
        protected override string AdapterDefinePreprocessorKey => "_AMS_VUNGLE";

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized;
            return isSupported;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return false;
        }
    }
}