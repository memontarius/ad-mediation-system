using System;

namespace Virterix.AdMediation.Editor
{
    public class ApplovinSettings : BaseAdNetworkSettings
    {
        public string _sdkKey;

        public override Type NetworkAdapterType => typeof(AppLovinAdapter);

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
