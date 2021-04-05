using UnityEditor;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class ChartboostView : BaseAdNetworkView
    {
        private SerializedProperty _androidAppSignatureProp;
        private SerializedProperty _iosAppSignatureProp;
  
        protected override bool IsSeparatedPlatformSettings => true;

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                if (!base.Enabled && value)
                {
                    ((ChartboostSettings)_settings).FixChartboostNativeScript();
                }
                base.Enabled = value;
            }
        }

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
            var settings = Utils.GetOrCreateSettings<ChartboostSettings>(SettingsFilePath);
            return settings;
        }

        protected override void DrawSpecificPlatformSettings(AppPlatform platform)
        {
            switch(platform)
            {
                case AppPlatform.Android:
                    _androidAppSignatureProp.stringValue = EditorGUILayout.TextField("Android App Signature", _androidAppSignatureProp.stringValue);
                    break;
                case AppPlatform.iOS:
                    _iosAppSignatureProp.stringValue = EditorGUILayout.TextField("iOS App Signature", _iosAppSignatureProp.stringValue);
                    break;
            }
        }
    }
} // namespace Virterix.AdMediation.Editor
