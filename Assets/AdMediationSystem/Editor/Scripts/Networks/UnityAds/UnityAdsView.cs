
using System;

namespace Virterix.AdMediation.Editor
{
    public class UnityAdsView : BaseAdNetworkView
    {
        protected override InstanceElementHeight CreateInstanceElementHeight(AdType adType)
        {
            var elementHeight = base.CreateInstanceElementHeight(adType);
            return elementHeight;
        }

        public UnityAdsView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(UnityAdsAdapter.UnityBannerSize));
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
