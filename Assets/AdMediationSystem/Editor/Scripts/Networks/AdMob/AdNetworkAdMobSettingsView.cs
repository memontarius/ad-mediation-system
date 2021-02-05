using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditorInternal;

namespace Virterix.AdMediation.Editor
{
    public class AdNetworkAdMobSettingsView : BaseAdNetworkSettingsView
    {
        private const string SETTINGS_FILE_NAME = "AdNetworkAdMobSettings.asset";

        protected override string SettingsFileName
        {
            get { return SETTINGS_FILE_NAME; }
        }

        protected override string[] BannerTypes 
        {
            get; set;
        }

        protected override bool IsAppIdSupported => true;

        public AdNetworkAdMobSettingsView(AdMediationSettingsWindow settingsWindow, string name, string identifier) : 
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(AdMobAdapter.AdMobBannerSize));
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AdNetworkAdMobSettings>(SettingsFilePath);
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

        protected override void DrawSettings()
        {  
        }
    }

} // namespace Virterix.AdMediation.Editor