using System;

namespace Virterix.AdMediation.Editor
{
    public class AudienceNetworkView : BaseAdNetworkView
    {
        protected override string SettingsFileName => "AudienceNetworkSettings.asset";

        protected override bool IsAppIdSupported => false;

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
} // namespace Virterix.AdMediation.Editor