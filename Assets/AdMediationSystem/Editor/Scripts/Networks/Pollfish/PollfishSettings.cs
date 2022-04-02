using System;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class PollfishSettings : BaseAdNetworkSettings
    {
        public bool _prepareOnHidden = true;
        [Tooltip("Restore banners on hide survey.")]
        public bool _restoreBanners = true;
        [Tooltip("Time in minutes when the survey will be request after failed received (0 - disabled).")]
        public int _autoPrepareInterval = 0;
     
        public override Type NetworkAdapterType => typeof(PollfishAdapter);
        protected override string AdapterScriptName => "PollfishAdapter";
        protected override string AdapterDefinePreprocessorKey => "_AMS_POLLFISH";
        public override string JsonAppIdKey => "apiKey";
        public override bool IsCommonTimeroutSupported => true;

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

        public override void SetupNetworkAdapter(AdMediationProjectSettings settings, Component networkAdapter)
        {
            var pollfishAdapter = networkAdapter as PollfishAdapter;
            pollfishAdapter.m_prepareOnHidden = _prepareOnHidden;
            pollfishAdapter.m_restoreBannersOnHideSurvey = _restoreBanners;
            pollfishAdapter.m_autoPrepareIntervalInMinutes = _autoPrepareInterval;
            pollfishAdapter.m_timeout = _timeout;
        }
    }
}
