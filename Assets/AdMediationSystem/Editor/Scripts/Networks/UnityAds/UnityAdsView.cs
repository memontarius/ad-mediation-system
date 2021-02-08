
namespace Virterix.AdMediation.Editor
{
    public class UnityAdsView : BaseAdNetworkView
    {
        protected override bool IsAppIdSupported => true;

        protected override string SettingsFileName => "UnityAdsSettings.asset";

        public UnityAdsView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<UnityAdsSettings>(SettingsFilePath);
            return settings;
        }

        protected override void DrawSpecificSettings()
        {
        }
    }
} // namespace Virterix.AdMediation.Editor
