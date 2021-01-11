using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

namespace Virterix.AdMediation.Editor
{
    public class AdNetworkAdMobSettingsView : AdNetworkSettingsViewBase
    {
        private const string SETTINGS_FILE_NAME = "AdNetworkAdMobSettings.asset";

        private string _androidAppId;
        private string _iosAppId;

        private string _androidRewardVideoUnitId;
        private string _iosRewardVideoUnitId;

        private bool _androidEnabled;
        private bool _iosEnabled;

        private SerializedObject _settingsProp;
        private AdNetworkAdMobSettingsModel _settings;

        //The array property we will edit
        SerializedProperty _bannerUnits;

        //The Reorderable list we will be working with
        ReorderableList _list;

        protected override string SettingsFileName
        {
            get
            {
                return SETTINGS_FILE_NAME;
            }
        }

        public AdNetworkAdMobSettingsView(string name, UnityAction action) : base(name, action)
        {
            _settings = GetOrCreateSettings<AdNetworkAdMobSettingsModel>();
            _settingsProp = new SerializedObject(_settings);
            
            _bannerUnits = _settingsProp.FindProperty("_bannerUnits");

            _list = CreateList(_settingsProp, _bannerUnits, "Banner Instances");
            //_list.drawElementCallback = DrawListItem;

            /*
            _list = new ReorderableList(_settingsProp, _bannerUnits, false, true, true, true);
            _list.drawElementCallback = DrawListItem;
            _list.drawHeaderCallback = DrawHeader;
            _list.elementHeight = 68f;*/
        }

        protected override void DrawSettings()
        {
            Enabled = EditorGUILayout.BeginToggleGroup("Enable", Enabled);
            
            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Android", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _androidAppId = EditorGUILayout.TextField("App Id", _androidAppId);
            _androidRewardVideoUnitId = EditorGUILayout.TextField("Reward Unit Id", _androidRewardVideoUnitId);
            EditorGUI.indentLevel--;

            GUILayout.EndVertical();

            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label("iOS", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            _iosAppId = EditorGUILayout.TextField("App Id", _iosAppId);
            _iosRewardVideoUnitId = EditorGUILayout.TextField("Reward Unit Id", _iosRewardVideoUnitId);

            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            EditorGUILayout.Space();
            
            _settingsProp.Update();
            _list.DoLayoutList();
            _settingsProp.ApplyModifiedProperties();

            EditorGUILayout.EndToggleGroup();
        }

        private void ProcessPropertyField(SerializedObject serializedObject, string fieldName)
        {
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            EditorGUILayout.PropertyField(property, true);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawListItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index); //The element in the list

            EditorGUI.LabelField(new Rect(rect.x, rect.y + 2, 100, EditorGUIUtility.singleLineHeight), "Name");
            EditorGUI.PropertyField(
                new Rect(rect.x + 100, rect.y + 5, 250, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_name"),
                GUIContent.none
            );

            EditorGUI.LabelField(new Rect(rect.x, rect.y + 22, 100, EditorGUIUtility.singleLineHeight), "Android Id");
            EditorGUI.PropertyField(
                new Rect(rect.x + 100, rect.y + 25, 250, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_androidId"),
                GUIContent.none
            );

            EditorGUI.LabelField(new Rect(rect.x, rect.y + 42, 100, EditorGUIUtility.singleLineHeight), "iOS Id");
            EditorGUI.PropertyField(
                new Rect(rect.x + 100, rect.y + 45, 250, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_iosId"),
                GUIContent.none
            );
        }

        private void DrawHeader(Rect rect)
        {
            string name = "Banner Ad Units";
            EditorGUI.LabelField(rect, name);
        }
    }

} // namespace Virterix.AdMediation.Editor

#endif