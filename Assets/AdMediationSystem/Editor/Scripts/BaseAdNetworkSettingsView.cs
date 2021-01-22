using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace Virterix.AdMediation.Editor
{
    public abstract class BaseAdNetworkSettingsView
    {
        public string Name
        {
            get; private set;
        }

        public bool Enabled
        {
            get; set;
        }

        public bool Collapsed
        {
            get; set;
        }

        protected abstract string SettingsFileName
        {
            get;
        }

        protected string SettingsFilePath
        {
            get
            {
                return String.Format("{0}/{1}", _settingsWindow.GetProjectFolderPath(_settingsWindow.CurrProjectName), SettingsFileName);
            }
        }

        private string CollapsedSaveKey
        {
            get { return string.Format("{0}{1}.collapsed", AdMediationSettingsWindow.PREFIX_SAVEKEY, Name); }
        }

        protected AdMediationSettingsWindow _settingsWindow;

        protected SerializedObject _serializedSettings;
        private BaseAdNetworkSettingsModel _settings;

        private AnimBool _showExtraFields;
        private UnityAction _repaint;
        private SerializedProperty _enabledProp;

        private List<ReorderableList> _adInstancesLists;
        private List<SerializedObject> _adInstancesProperties;

        private ReorderableList _banners;
        private SerializedProperty _bannerAdInstances;
        private ReorderableList _interstitials;
        private SerializedProperty _interstitialAdInstances;


        public BaseAdNetworkSettingsView(AdMediationSettingsWindow settingsWindow, string name, UnityAction action)
        {
            _settingsWindow = settingsWindow;
            Name = name;
            _showExtraFields = new AnimBool(true);
            _showExtraFields.valueChanged.AddListener(action);
            Collapsed = EditorPrefs.GetBool(CollapsedSaveKey, false);
            _adInstancesLists = new List<ReorderableList>();
            _adInstancesProperties = new List<SerializedObject>();

            _settings = CreateSettingsModel();
            _serializedSettings = new SerializedObject(_settings);

            _bannerAdInstances = _serializedSettings.FindProperty("_bannerAdInstances");
            _banners = CreateList(_serializedSettings, _bannerAdInstances, "Banner Instances");

            _interstitialAdInstances = _serializedSettings.FindProperty("_interstitialAdInstances");
            _interstitials = CreateList(_serializedSettings, _interstitialAdInstances, "Interstitial Instances");

            _enabledProp = _serializedSettings.FindProperty("_enabled");
            Enabled = _enabledProp.boolValue;
        }

        public void DrawUI()
        {
            bool previousCollapsed = Collapsed;
            Collapsed = EditorGUILayout.BeginFoldoutHeaderGroup(Collapsed, Name);
            if (Collapsed != previousCollapsed)
            {
                EditorPrefs.SetBool(CollapsedSaveKey, Collapsed);
            }

            _showExtraFields.target = Collapsed;
            if (EditorGUILayout.BeginFadeGroup(_showExtraFields.faded))
            {
                Enabled = EditorGUILayout.BeginToggleGroup("Enable", Enabled);
                _enabledProp.boolValue = Enabled;
                GUILayout.BeginVertical("helpbox");

                DrawSettings();
                DrawAdInstanceLists();

                GUILayout.EndVertical();
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected virtual BaseAdNetworkSettingsModel CreateSettingsModel()
        {
            return null;
        }

        protected virtual void DrawSettings()
        {
        }

        private void DrawAdInstanceLists()
        {
            for(int i = 0; i < _adInstancesLists.Count; i++)
            {
                EditorGUILayout.Space();
                _adInstancesProperties[i].Update();
                _adInstancesLists[i].DoLayoutList();
                _serializedSettings.ApplyModifiedProperties();
            }
        }

        protected ReorderableList CreateList(SerializedObject serializedObj, SerializedProperty serializedProp, string title)
        {
            ReorderableList list = new ReorderableList(serializedObj, serializedProp, false, true, true, true);
            list.headerHeight = 22;
            list.elementHeight = 68f;

            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, title);
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.LabelField(new Rect(rect.x, rect.y + 2, 100, EditorGUIUtility.singleLineHeight), "Name");

                EditorGUI.PropertyField(
                    new Rect(rect.x + 80, rect.y + 5, 180, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("_name"),
                    GUIContent.none
                );

                EditorGUI.LabelField(new Rect(rect.x + 280, rect.y + 5, 100, EditorGUIUtility.singleLineHeight), "Timeout");
                EditorGUI.PropertyField(
                    new Rect(rect.x + 340, rect.y + 5, 80, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("_timeout"),
                    GUIContent.none
                );

                EditorGUI.LabelField(new Rect(rect.x, rect.y + 22, 100, EditorGUIUtility.singleLineHeight), "Android Id");
                EditorGUI.PropertyField(
                    new Rect(rect.x + 80, rect.y + 25, 340, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("_androidId"),
                    GUIContent.none
                );

                EditorGUI.LabelField(new Rect(rect.x, rect.y + 42, 100, EditorGUIUtility.singleLineHeight), "iOS Id");
                EditorGUI.PropertyField(
                    new Rect(rect.x + 80, rect.y + 45, 340, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("_iosId"),
                    GUIContent.none
                );
            };

            list.onCanRemoveCallback = (ReorderableList l) =>
            {
                return true;
            };

            _adInstancesProperties.Add(serializedObj);
            _adInstancesLists.Add(list);
            return list;
        }
    }
} // namespace Virterix.AdMediation.Editor
