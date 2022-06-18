using System;
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

        protected override bool IsAdInstanceIdsDisplayed => false;

        private SerializedProperty _overriddenPlacementsProp;
        private SerializedProperty _useAdTypesProp;
        private ReorderableList _overriddenPlacementList;
        private string[] _irnSrcAdTypeNames;
        
        public IronSourceView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(IronSourceAdapter.IrnSrcBannerSize));
            
            var irnSrcAdTypes = Enum.GetValues(typeof(IronSourceAdapter.IrnSrcAdType)) as IronSourceAdapter.IrnSrcAdType[]; 
            _irnSrcAdTypeNames = new string[irnSrcAdTypes.Length];
            for(int i = 0; i < _irnSrcAdTypeNames.Length; i++)
                _irnSrcAdTypeNames[i] = irnSrcAdTypes[i].ToString();

            _isOverriddenPlacementUncollapsed = EditorPrefs.GetBool(OVERRIDDEN_PLACEMENT_LIST_COLLAPSED_SAVEKEY, false);
            _overriddenPlacementFoldAnimation = new AnimBool(_isOverriddenPlacementUncollapsed);
            _overriddenPlacementFoldAnimation.valueChanged.AddListener(settingsWindow.Repaint);
            
            _useAdTypesProp = _serializedSettings.FindProperty("_useAdTypes");
            _overriddenPlacementsProp = _serializedSettings.FindProperty("_overriddenPlacements");
            _overriddenPlacementList = new ReorderableList(_serializedSettings, _overriddenPlacementsProp, false, false, true, true);
            
            _overriddenPlacementList.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = _overriddenPlacementList.serializedProperty.GetArrayElementAtIndex(index);
                float elementWidth = rect.width;
                float width = Mathf.Clamp(elementWidth * 0.5f, 100, 2800) - 120;
                rect.y += 2;

                SerializedProperty adTypeProp = element.FindPropertyRelative("AdvertisingType");
                adTypeProp.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
                    adTypeProp.intValue, Utils.EditorAdTypes);

                rect.x += 100 + 10;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Current");
                rect.x += 50;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("OriginPlacement"),
                    GUIContent.none
                );

                rect.x += width + 10;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight), "Overridden");
                rect.x += 70;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("TargetPlacement"),
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
            
            _useAdTypesProp.intValue = EditorGUILayout.MaskField("Use Ad Types", _useAdTypesProp.intValue, _irnSrcAdTypeNames);
            EditorGUILayout.Space();
            
            char collapsedSymbol = _isOverriddenPlacementUncollapsed ? AdMediationSettingsWindow.SYMBOL_BOTTOM_ARROW : AdMediationSettingsWindow.SYMBOL_LEFT_ARROW;
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
}
