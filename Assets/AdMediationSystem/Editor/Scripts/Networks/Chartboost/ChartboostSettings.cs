using System;

namespace Virterix.AdMediation.Editor
{
    public class ChartboostSettings : BaseAdNetworkSettings
    {
        public string _androidAppSignature;
        public string _iosAppSignature;

        public override Type NetworkAdapterType => typeof(ChartboostAdapter);

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
