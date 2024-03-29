﻿using UnityEditor;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class ApplovinView : BaseAdNetworkView
    {
        private SerializedProperty _sdkKeyProp;

        public ApplovinView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            _sdkKeyProp = _serializedSettings.FindProperty("_sdkKey");
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<ApplovinSettings>(SettingsFilePath);
            return settings;
        }

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

        protected override void DrawSpecificSettings(AdMediationProjectSettings projectSettings)
        {
            GUILayout.BeginVertical("box");
            _sdkKeyProp.stringValue = EditorGUILayout.TextField("Sdk Key", _sdkKeyProp.stringValue);
            GUILayout.EndVertical();
        }
    }
}