using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;

namespace Virterix.AdMediation.Editor
{
    public class IronSourceView : BaseAdNetworkView
    {
        private const string OVERRIDDEN_PLACEMENT_LIST_COLLAPSED_SAVEKEY = AdMediationSettingsWindow.PREFIX_SAVEKEY + "irnsrc_plac_collapsed";

        private bool _isOverriddenPlacementUncollapsed;
        private AnimBool _overriddenPlacementFoldAnimation;

        protected override string SettingsFileName => "AdmIronSourceSettings.asset";
        protected override bool IsAdInstanceIdsDisplayed => false;

        private SerializedProperty _overiddenPlacementsProp;
        private ReorderableList _overriddenPlacementList;

        public IronSourceView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(IronSourceAdapter.IrnSrcBannerSize));
            
            _isOverriddenPlacementUncollapsed = EditorPrefs.GetBool(OVERRIDDEN_PLACEMENT_LIST_COLLAPSED_SAVEKEY, false);
            _overriddenPlacementFoldAnimation = new AnimBool(_isOverriddenPlacementUncollapsed);
            _overriddenPlacementFoldAnimation.valueChanged.AddListener(settingsWindow.Repaint);

            _overiddenPlacementsProp = _serializedSettings.FindProperty("_overiddenPlacements");
            _overriddenPlacementList = new ReorderableList(_serializedSettings, _overiddenPlacementsProp, false, false, true, true);
            
            _overriddenPlacementList.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = _overriddenPlacementList.serializedProperty.GetArrayElementAtIndex(index);
                float elementWidth = rect.width;
                float width = Mathf.Clamp(elementWidth * 0.5f, 100, 2800) - 120;
                rect.y += 2;

                SerializedProperty adTypeProp = element.FindPropertyRelative("adType");
                adTypeProp.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
                    adTypeProp.intValue, Utils.EditorAdTypes);

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
 
            char collapsedSymbol = _isOverriddenPlacementUncollapsed ? AdMediationSettingsWindow.SYMBOL_LEFT_ARROW : AdMediationSettingsWindow.SYMBOL_BOTTOM_ARROW;
            string buttonTitle = string.Format("{0}  {1}", collapsedSymbol, "Override Placement Names");

            if (GUILayout.Button(buttonTitle, InstanceFoldoutButtonStyle))
            {
                _isOverriddenPlacementUncollapsed = !_isOverriddenPlacementUncollapsed;
                EditorPrefs.SetBool(OVERRIDDEN_PLACEMENT_LIST_COLLAPSED_SAVEKEY, _isOverriddenPlacementUncollapsed);
            }

            _overriddenPlacementFoldAnimation.target = _isOverriddenPlacementUncollapsed;
            if (EditorGUILayout.BeginFadeGroup(_overriddenPlacementFoldAnimation.faded))
            {
                _overriddenPlacementList.DoLayoutList();
            }
            EditorGUILayout.EndFadeGroup();

            GUILayout.EndVertical();
        }
    }
} // namespace Virterix.AdMediation.Editor
