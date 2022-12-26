using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

namespace Virterix.AdMediation.Editor
{
    public sealed class AdMobView : BaseAdNetworkView
    {
        public const string USE_MEDIATION_DEFMACROS = "_ADMOB_USE_MEDIATION";
        private const string MEDIATION_FILE_FILTER = "AdMobMediationBehavior";

        private const string USE_FAN_NETWORK_DEFMACROS = "_AMS_AUDIENCE_NETWORK";
        private const string FAN_UTILS_FILE_FILTER = "AudienceNetworkMediationUtils";
        
        private SerializedProperty _useMediationProp;
        private SerializedProperty _mediationNetworkFlagsProp;
        private string[] _mediationNetworkOptions;
        private string[] _mediationNetworkDefinitions;

        public AdMobView(AdMediationSettingsWindow settingsWindow, string name, string identifier) : 
            base(settingsWindow, name, identifier)
        {
            _mediationNetworkOptions = new string[] { 
                "Facebook Audience Network", 
                "UnityAds", 
                "AppLovin", 
                "Chartboost", 
                "AdColony" 
            };
            _mediationNetworkDefinitions = new string[] {
                "_ADMOB_MEDIATION_FAN",
                "_ADMOB_MEDIATION_UNITYADS",
                "_ADMOB_MEDIATION_APPLOVIN",
                "_ADMOB_MEDIATION_CHARTBOOST",
                "_ADMOB_MEDIATION_ADCOLONY"
            };

            BannerTypes = Enum.GetNames(typeof(AdMobAdapter.AdMobBannerSize));
            _useMediationProp = _serializedSettings.FindProperty("_useMediation");
            _mediationNetworkFlagsProp = _serializedSettings.FindProperty("_mediationNetworkFlags");

            bool defineUseMediationMacros = Enabled && _useMediationProp.boolValue;
            WriteDefinitionInScript(defineUseMediationMacros, USE_MEDIATION_DEFMACROS, MEDIATION_FILE_FILTER);

            int flags = _mediationNetworkFlagsProp.intValue;
            for (int i = 0; i < _mediationNetworkOptions.Length; i++)
            {
                bool enabled = (flags & (1 << i)) == (1 << i);
                WriteDefinitionInScript(enabled, _mediationNetworkDefinitions[i], MEDIATION_FILE_FILTER);
            }

            bool fanEnabled = Enabled && (_mediationNetworkFlagsProp.intValue & (1 << 0)) == (1 << 0);
            WriteDefinitionInScript(fanEnabled, USE_FAN_NETWORK_DEFMACROS, FAN_UTILS_FILE_FILTER);
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AdMobSettings>(SettingsFilePath);
            return settings;
        }

        protected override void SetupReorderableList(ReorderableList list, AdType adType)
        {
        }

        public override bool DrawUI() {
            bool activationChanged = base.DrawUI();
            if (activationChanged) {
                if (this.Enabled) {
                    WriteDefinitionInScript(_useMediationProp.boolValue, USE_MEDIATION_DEFMACROS, MEDIATION_FILE_FILTER);
                }
                else {
                    WriteDefinitionInScript(false, USE_MEDIATION_DEFMACROS, MEDIATION_FILE_FILTER);
                }
                
                bool fanEnabled = Enabled && (_mediationNetworkFlagsProp.intValue & (1 << 0)) == (1 << 0);
                WriteDefinitionInScript(fanEnabled, USE_FAN_NETWORK_DEFMACROS, FAN_UTILS_FILE_FILTER);
            }
            return activationChanged;
        }

        public override void WriteDefinitionInScript()
        {
            int flags = _mediationNetworkFlagsProp.intValue;
            for (int i = 0; i < _mediationNetworkOptions.Length; i++) {
                bool enabled = (flags & (1 << i)) == (1 << i);
                WriteDefinitionInScript(enabled, _mediationNetworkDefinitions[i], MEDIATION_FILE_FILTER);
            }
            bool fanEnabled = Enabled && (_mediationNetworkFlagsProp.intValue & (1 << 0)) == (1 << 0);
            WriteDefinitionInScript(fanEnabled, USE_FAN_NETWORK_DEFMACROS, FAN_UTILS_FILE_FILTER);
        }
        
        protected override void DrawSpecificSettings()
        {
            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            _useMediationProp.boolValue = EditorGUILayout.Toggle("Use Mediation", _useMediationProp.boolValue);
            bool changed = EditorGUI.EndChangeCheck();

            if (changed) {
                WriteDefinitionInScript(_useMediationProp.boolValue, USE_MEDIATION_DEFMACROS, MEDIATION_FILE_FILTER);
                AssetDatabase.Refresh();
            }

            if (_useMediationProp.boolValue) {
                EditorGUI.BeginChangeCheck();
                _mediationNetworkFlagsProp.intValue = EditorGUILayout.MaskField("Networks", _mediationNetworkFlagsProp.intValue, _mediationNetworkOptions);
                changed = EditorGUI.EndChangeCheck();

                if (changed)
                {
                    WriteDefinitionInScript();
                    AssetDatabase.Refresh();
                }
            }
            GUILayout.EndVertical();
        }
    }
}