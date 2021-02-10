using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class ChartboostSettings : BaseAdNetworkSettings
    {
        public string _androidAppSignature;
        public string _iosAppSignature;

        public override Type NetworkAdapterType => typeof(ChartboostAdapter);
        protected override string AdapterScriptName => "ChartboostAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_CHARTBOOST";

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized;
            return isSupported;
        }

        public override bool IsAdInstanceSupported(AdType adType)
        {
            return false;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return false;
        }

        public override void SetupNetworkAdapter(Component networkAdapter)
        {
            ChartboostAdapter.SetupNetworkSettings(_androidAppId, _androidAppSignature, _iosAppId, _iosAppSignature);
        }
    }
} // namespace Virterix.AdMediation.Editor
