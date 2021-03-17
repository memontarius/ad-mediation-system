using System;
using UnityEditorInternal;

namespace Virterix.AdMediation.Editor
{
    public class IronSourceView : BaseAdNetworkView
    {
        protected override string SettingsFileName => "AdmIronSourceSettings.asset";

        protected override bool IsAdInstanceIdsDisplayed => false;

        public IronSourceView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(IronSourceAdapter.IrnSrcBannerSize));
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<IronSourceSettings>(SettingsFilePath);
            return settings;
        }
    }
} // namespace Virterix.AdMediation.Editor
