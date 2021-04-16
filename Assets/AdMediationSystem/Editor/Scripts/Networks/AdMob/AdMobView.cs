using System;
using UnityEditorInternal;

namespace Virterix.AdMediation.Editor
{
    public class AdMobView : BaseAdNetworkView
    {
        
        public AdMobView(AdMediationSettingsWindow settingsWindow, string name, string identifier) : 
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(AdMobAdapter.AdMobBannerSize));
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AdMobSettings>(SettingsFilePath);
            return settings;
        }

        protected override void SetupReorderableList(ReorderableList list, AdType adType)
        {
        }

        protected override void DrawSpecificSettings()
        {  
        }
    }

} // namespace Virterix.AdMediation.Editor