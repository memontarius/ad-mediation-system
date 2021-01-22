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
        private SerializedProperty _androidRewardUnitProp;
        private SerializedProperty _iosAppIdProp;
        private SerializedProperty _iosRewardVideoUnitProp;

        protected override string SettingsFileName
        {
            get { return SETTINGS_FILE_NAME; }
        }

        public AdNetworkAdMobSettingsView(AdMediationSettingsWindow settingsWindow, string name, UnityAction action) : 
            base(settingsWindow, name, action)
        {
            // Android
            _androidAppIdProp = _serializedSettings.FindProperty("_androidAppId");
            _androidRewardUnitProp = _serializedSettings.FindProperty("_androidRewardVideoUnitId");
            // iOS
            _iosAppIdProp = _serializedSettings.FindProperty("_iosAppId");
            _iosRewardVideoUnitProp = _serializedSettings.FindProperty("_iosRewardVideoUnitId");
        }

        protected override BaseAdNetworkSettingsModel CreateSettingsModel()
        {
            return Utils.GetOrCreateSettings<AdNetworkAdMobSettingsModel>(SettingsFilePath);
        }

        protected override void DrawSettings()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Android", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            _androidAppIdProp.stringValue = EditorGUILayout.TextField("App Id", _androidAppIdProp.stringValue);
            _androidRewardUnitProp.stringValue = EditorGUILayout.TextField("Reward Unit Id", _androidRewardUnitProp.stringValue);
            _serializedSettings.ApplyModifiedProperties();

            EditorGUI.indentLevel--;

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.Label("iOS", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;

            _iosAppIdProp.stringValue = EditorGUILayout.TextField("App Id", _iosAppIdProp.stringValue);
            _iosRewardVideoUnitProp.stringValue = EditorGUILayout.TextField("Reward Unit Id", _iosRewardVideoUnitProp.stringValue);
            _serializedSettings.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
        }
    }

} // namespace Virterix.AdMediation.Editor