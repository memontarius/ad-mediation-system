using System;
using System.Collections.Generic;
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

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            ChartboostAdapter.SetupNetworkNativeSettings(_androidAppId, _androidAppSignature, _iosAppId, _iosAppSignature);
        }

        public override Dictionary<string, object> GetSpecificNetworkParameters(AppPlatform platform)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            string appSignature = "";
            switch (platform)
            {
                case AppPlatform.Android:
                    appSignature = _androidAppSignature;
                    break;
                case AppPlatform.iOS:
                    appSignature = _iosAppSignature;
                    break;
            }

            parameters.Add("appSignature", appSignature);
            parameters.Add("autocache", false);
            return parameters;
        }
    }
} // namespace Virterix.AdMediation.Editor
