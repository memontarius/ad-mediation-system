using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Virterix.AdMediation.Editor
{
    public class PollfishView : BaseAdNetworkView
    {
        private SerializedProperty _autoPrepareOnHideProp;
        private SerializedProperty _restoreBannersProp;
        private SerializedProperty _autoPrepareIntervalProp;

        protected override string SettingsFileName => "AdmPollfishSettings.asset";

        public PollfishView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            _autoPrepareOnHideProp = _serializedSettings.FindProperty("_autoPrepareOnHide");
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
            Utils.DrawPropertyField(_serializedSettings, _autoPrepareOnHideProp);
            Utils.DrawPropertyField(_serializedSettings, _restoreBannersProp);
            Utils.DrawPropertyField(_serializedSettings, _autoPrepareIntervalProp);
            GUILayout.EndVertical();
        }
    }
} // namespace Virterix.AdMediation.Editor
