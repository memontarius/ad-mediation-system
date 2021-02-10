using System;

namespace Virterix.AdMediation.Editor
{
    public class AdmApplovinSettings : BaseAdNetworkSettings
    {
        public string _sdkKey;

        public override Type NetworkAdapterType => typeof(AppLovinAdapter);

        protected override string AdapterScriptName => "AppLovinAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_APPLOVIN";

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
} // namespace Virterix.AdMediation.Editor
