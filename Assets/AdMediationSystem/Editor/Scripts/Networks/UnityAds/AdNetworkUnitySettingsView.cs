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

        private SerializedProperty _androidAppIdProp;
        private SerializedProperty _iosAppIdProp;

        protected override string SettingsFileName
        {
            get { return SETTINGS_FILE_NAME; }
        }

        public AdNetworkUnitySettingsView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            // Android
            _androidAppIdProp = _serializedSettings.FindProperty("_androidAppId");
            // iOS
            _iosAppIdProp = _serializedSettings.FindProperty("_iosAppId");
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AdNetworkUnitySettings>(SettingsFilePath);
            return settings;
        }

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized;
            return isSupported;
        }

        protected override void DrawSettings()
        {
            GUILayout.BeginVertical("box");
            if (_settingsWindow.IsAndroid)
            {
                _androidAppIdProp.stringValue = EditorGUILayout.TextField("Android App Id", _androidAppIdProp.stringValue);
            }
            if (_settingsWindow.IsIOS)
            {
                _iosAppIdProp.stringValue = EditorGUILayout.TextField("iOS App Id", _iosAppIdProp.stringValue);
            }
            GUILayout.EndVertical();
        }
    }
} // namespace Virterix.AdMediation.Editor
