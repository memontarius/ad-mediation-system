using System;

namespace Virterix.AdMediation.Editor
{
    public class AudienceNetworkView : BaseAdNetworkView
    {
        public AudienceNetworkView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(AudienceNetworkAdapter.AudienceNetworkBannerSize));
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AudienceNetworkSettings>(SettingsFilePath);
            return settings;
        }
    }
}