using System;
using UnityEditorInternal;

namespace Virterix.AdMediation.Editor
{
    public class AdMobView : BaseAdNetworkView
    {
        protected override string SettingsFileName => "AdMobSettings.asset";

        protected override string[] BannerTypes 
        {
            get; set;
        }

        protected override bool IsAppIdSupported => true;

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
            if (adType == AdType.Incentivized)
            {
                list.onCanRemoveCallback = (ReorderableList l) =>
                {
                    return true;
                };
                list.onCanAddCallback = (ReorderableList l) =>
                {
                    return list.count < 1;
                };           
            }
        }

        protected override void DrawSpecificSettings()
        {  
        }
    }

} // namespace Virterix.AdMediation.Editor