using System;
using System.IO;
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
        protected override string UsingAdapterPreprocessorDirective => "_AMS_CHARTBOOST";
        public override bool IsCommonTimeoutSupported => true;

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
            ChartboostAdapter adapter = networkAdapter as ChartboostAdapter;
            adapter.m_timeout = _timeout;
            ChartboostAdapter.SetupNetworkNativeSettings(_androidAppId, _androidAppSignature, _iosAppId, _iosAppSignature);
            FixChartboostNativeScript();
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

        public void FixChartboostNativeScript()
        {
            string chartboostPath = string.Format("{0}/{1}", Application.dataPath, "/Chartboost/Scripts/Chartboost.cs");
            if (File.Exists(chartboostPath))
            {
                string content = File.ReadAllText(chartboostPath);
                if (content.Length > 0)
                {
                    string expectedString = "//Time.timeScale =";
                    string replaceableString = "Time.timeScale =";

                    if (!content.Contains(expectedString))
                    {
                        content = content.Replace(replaceableString, expectedString);
                    }
                    File.WriteAllText(chartboostPath, content);
                }
            }
        }
    }
}
