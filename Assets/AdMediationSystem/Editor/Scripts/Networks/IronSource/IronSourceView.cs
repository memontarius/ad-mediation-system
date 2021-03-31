using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class IronSourceView : BaseAdNetworkView
    {
        protected override string SettingsFileName => "AdmIronSourceSettings.asset";

        protected override bool IsAdInstanceIdsDisplayed => false;

        private SerializedProperty _overiddenPlacementsProp;
        private ReorderableList _overriddenPlacementList;

        public IronSourceView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(IronSourceAdapter.IrnSrcBannerSize));
            List<string> adTypes = new List<string>(Enum.GetNames(typeof(AdType)));
            adTypes.Remove(AdType.Unknown.ToString());

            _overiddenPlacementsProp = _serializedSettings.FindProperty("_overiddenPlacements");
            _overriddenPlacementList = new ReorderableList(_serializedSettings, _overiddenPlacementsProp, false, true, true, true);
            
            _overriddenPlacementList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "Override Placement Names");
            };
            _overriddenPlacementList.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = _overriddenPlacementList.serializedProperty.GetArrayElementAtIndex(index);
                float elementWidth = rect.width;
                float width = Mathf.Clamp(elementWidth * 0.5f, 100, 2800) - 120;
                rect.y += 2;

                SerializedProperty adTypeProp = element.FindPropertyRelative("adType");
                adTypeProp.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
                    adTypeProp.intValue, adTypes.ToArray());

                rect.x += 100 + 10;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Current");
                rect.x += 50;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("originPlacement"),
                    GUIContent.none
                );

                rect.x += width + 10;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight), "Overridden");
                rect.x += 70;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("overriddenPlacement"),
                    GUIContent.none
                );
            };
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<IronSourceSettings>(SettingsFilePath);
            return settings;
        }

        protected override void DrawSpecificSettings()
        {
            GUILayout.BeginVertical("box");
            _overriddenPlacementList.DoLayoutList();
            GUILayout.EndVertical();
        }
    }
} // namespace Virterix.AdMediation.Editor
