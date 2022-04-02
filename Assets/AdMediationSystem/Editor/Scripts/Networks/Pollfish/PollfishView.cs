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

        public PollfishView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            _prepareOnHiddenProp = _serializedSettings.FindProperty("_prepareOnHidden");
            _restoreBannersProp = _serializedSettings.FindProperty("_restoreBanners");
            _autoPrepareIntervalProp = _serializedSettings.FindProperty("_autoPrepareInterval");
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
            GUILayout.EndVertical();
        }
    }
}
