using System;

namespace Virterix.AdMediation.Editor
{
    public class AdColonyView : BaseAdNetworkView
    {
        public AdColonyView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(AdColonyAdapter.AdColonyAdSize));
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AdColonySettings>(SettingsFilePath);
            return settings;
        }

        protected override void DrawSpecificSettings(AdMediationProjectSettings projectSettings)
        {
        }
    }
}