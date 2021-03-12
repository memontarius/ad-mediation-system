using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class IronSourceView : BaseAdNetworkView
    {
        protected override string SettingsFileName => "AdmIronSourceSettings.asset";

        public IronSourceView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<IronSourceSettings>(SettingsFilePath);
            return settings;
        }
    }
} // namespace Virterix.AdMediation.Editor
