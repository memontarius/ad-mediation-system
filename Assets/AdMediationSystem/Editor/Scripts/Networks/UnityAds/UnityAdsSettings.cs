using System;

namespace Virterix.AdMediation.Editor
{
    public class UnityAdsSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(UnityAdsAdapter);

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized;
            return isSupported;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return true;
        }
    }
} // namespace Virterix.AdMediation.Editor