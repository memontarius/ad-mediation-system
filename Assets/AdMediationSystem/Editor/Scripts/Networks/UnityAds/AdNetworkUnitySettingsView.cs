using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditorInternal;

namespace Virterix.AdMediation.Editor
{
    public class AdNetworkUnitySettingsView : BaseAdNetworkSettingsView
    {
        private const string SETTINGS_FILE_NAME = "AdNetworkUnitySettings.asset";

        protected override bool IsAppIdSupported => true;

        protected override string SettingsFileName
        {
            get { return SETTINGS_FILE_NAME; }
        }

        public AdNetworkUnitySettingsView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AdNetworkUnitySettings>(SettingsFilePath);
            return settings;
        }

        protected override void DrawSettings()
        {
        }
    }
} // namespace Virterix.AdMediation.Editor
