using System;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class PollfishSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(PollfishAdapter);
        protected override string AdapterScriptName => "PollfishAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_POLLFISH";
        public override string JsonAppIdKey => "apiKey"; 

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Incentivized;
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
    }
} // namespace Virterix.AdMediation.Editor
