using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class UnityAdsSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(UnityAdsAdapter);
        protected override string AdapterScriptName => "UnityAdsAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_UNITY_ADS";

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized;
            return isSupported;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return true;
        }

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
        }

        public override string GetNetworkSDKVersion()
        {
            return UnityAdsAdapter.GetSDKVersion();
        }
    }
} // namespace Virterix.AdMediation.Editor