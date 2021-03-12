using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class IronSourceSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(IronSourceAdapter);
        protected override string AdapterScriptName => "IronSourceAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_IRONSOURCE";
        public override bool IsCommonTimeroutSupported => true;

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized;
            return isSupported;
        }
        public override string GetNetworkSDKVersion() => IronSourceAdapter.GetSDKVersion();

        public override bool IsAdInstanceSupported(AdType adType) => false;

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType) => adType == AdType.Incentivized;

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            IronSourceAdapter adapter = networkAdapter as IronSourceAdapter;
            adapter.m_timeout = _timeout;
        }
    }
} // namespace Virterix.AdMediation.Editor
