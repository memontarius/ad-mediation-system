using System;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class AdColonySettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(AdColonyAdapter);
        protected override string AdapterScriptName => "AdColonyAdapter";
        protected override string AdapterDefinePeprocessorKey => "_AMS_ADCOLONY";
 
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
} // Virterix.AdMediation.Editor
