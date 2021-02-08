using UnityEditor;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class ApplovinView : BaseAdNetworkView
    {
        private SerializedProperty _sdkKeyProp;

        protected override bool IsAppIdSupported => false;

        protected override string SettingsFileName => "ApplovinSettings.asset";

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

        protected override void DrawSpecificSettings()
        {
            GUILayout.BeginVertical("box");
            _sdkKeyProp.stringValue = EditorGUILayout.TextField("Sdk Key", _sdkKeyProp.stringValue);
            GUILayout.EndVertical();
        }
    }
} // namespace Virterix.AdMediation.Editor