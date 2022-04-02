
namespace Virterix.AdMediation.Editor
{
    public class UnityAdsView : BaseAdNetworkView
    {
        protected override InstanceElementHeight CreateInstanceElementHeight(AdType adType)
        {
            var elementHeight = base.CreateInstanceElementHeight(adType);
            if (adType == AdType.Banner)
            {
                elementHeight.androidHeight = elementHeight.iosHeight -= 22;
                elementHeight.height -= 22;
            }
            return elementHeight;
        }

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
}
