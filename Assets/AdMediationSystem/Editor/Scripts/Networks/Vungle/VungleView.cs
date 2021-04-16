using UnityEditor;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class VungleView : BaseAdNetworkView
    {
        public VungleView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<VungleSettings>(SettingsFilePath);
            return settings;
        }

    }
}
