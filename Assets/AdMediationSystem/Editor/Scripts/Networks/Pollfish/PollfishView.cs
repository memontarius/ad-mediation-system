using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class PollfishView : BaseAdNetworkView
    {
        protected override string SettingsFileName => "AdmPollfishSettings.asset";

        public PollfishView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<PollfishSettings>(SettingsFilePath);
            return settings;
        }
    }
} // namespace Virterix.AdMediation.Editor
