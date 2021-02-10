using UnityEditor;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class ChartboostView : BaseAdNetworkView
    {
        private SerializedProperty _androidAppSignatureProp;
        private SerializedProperty _iosAppSignatureProp;

        protected override bool IsAppIdSupported => true;

        protected override string SettingsFileName => "AdmChartboostSettings.asset";

        public ChartboostView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            // Android
            _androidAppSignatureProp = _serializedSettings.FindProperty("_androidAppSignature");
            // iOS
            _iosAppSignatureProp = _serializedSettings.FindProperty("_iosAppSignature");
        }
        
        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AdmChartboostSettings>(SettingsFilePath);
            return settings;
        }

        protected override void DrawSpecificSettings()
        {
            GUILayout.BeginVertical("box");
            if (_settingsWindow.IsAndroid)
            {
                _androidAppSignatureProp.stringValue = EditorGUILayout.TextField("Android App Signature", _androidAppSignatureProp.stringValue);
            }
            if (_settingsWindow.IsIOS)
            {
                _iosAppSignatureProp.stringValue = EditorGUILayout.TextField("iOS App Signature", _iosAppSignatureProp.stringValue);
            }
            GUILayout.EndVertical();
        }
    }
} // namespace Virterix.AdMediation.Editor
