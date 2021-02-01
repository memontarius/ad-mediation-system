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

        private SerializedProperty _androidAppIdProp;   
        private SerializedProperty _iosAppIdProp;

        protected override string SettingsFileName
        {
            get { return SETTINGS_FILE_NAME; }
        }

        protected override string[] BannerTypes 
        {
            get; set;
        }
        
        public AdNetworkAdMobSettingsView(AdMediationSettingsWindow settingsWindow, string name, string identifier) : 
            base(settingsWindow, name, identifier)
        {
            // Android
            _androidAppIdProp = _serializedSettings.FindProperty("_androidAppId");
            // iOS
            _iosAppIdProp = _serializedSettings.FindProperty("_iosAppId");
            BannerTypes = Enum.GetNames(typeof(AdMobAdapter.AdMobBannerSize));
        }

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = false;
            switch(adType)
            {
                case AdType.Banner:
                    isSupported = true;
                    break;
                case AdType.Interstitial:
                    isSupported = true;
                    break;
                case AdType.Incentivized:
                    isSupported = true;
                    break;
            }
            return isSupported;
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