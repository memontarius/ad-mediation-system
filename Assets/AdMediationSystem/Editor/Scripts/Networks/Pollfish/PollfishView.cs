using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Virterix.AdMediation.Editor
{
    public class PollfishView : BaseAdNetworkView
    {
        private SerializedProperty _prepareOnHiddenProp;
        private SerializedProperty _restoreBannersProp;
        private SerializedProperty _autoPrepareIntervalProp;
        private SerializedProperty _timeoutInMediatorProp;
        
        protected override string SettingsFileName => "AdmPollfishSettings.asset";

        public PollfishView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            _prepareOnHiddenProp = _serializedSettings.FindProperty("_prepareOnHidden");
            _restoreBannersProp = _serializedSettings.FindProperty("_restoreBanners");
            _autoPrepareIntervalProp = _serializedSettings.FindProperty("_autoPrepareInterval");
            _timeoutInMediatorProp = _serializedSettings.FindProperty("_timeoutInMediator");
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<PollfishSettings>(SettingsFilePath);
            return settings;
        }

        protected override void DrawSpecificSettings()
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(_prepareOnHiddenProp);
            EditorGUILayout.PropertyField(_restoreBannersProp);
            EditorGUILayout.PropertyField(_autoPrepareIntervalProp);
            EditorGUILayout.PropertyField(_timeoutInMediatorProp);

            GUILayout.EndVertical();
        }
    }
} // namespace Virterix.AdMediation.Editor
