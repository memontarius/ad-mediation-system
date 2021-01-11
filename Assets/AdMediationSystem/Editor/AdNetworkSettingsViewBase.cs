using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace Virterix.AdMediation.Editor
{
    public abstract class AdNetworkSettingsViewBase
    {
        private const string SETTINGS_PATH = "Assets/AdMediationSystem/Editor/Resources/";

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

        private string SettingsFilePath
        {
            get
            {
                return String.Format("{0}{1}", SETTINGS_PATH, SettingsFileName);
            }
        }

        private AnimBool _showExtraFields;
        private UnityAction _repaint;

        public AdNetworkSettingsViewBase(string name, UnityAction action)
        {
            Name = name;
            _showExtraFields = new AnimBool(true);
            _showExtraFields.valueChanged.AddListener(action);
        }

        public void DrawUI()
        {
            Collapsed = EditorGUILayout.BeginFoldoutHeaderGroup(Collapsed, Name);

            _showExtraFields.target = Collapsed;
            if (EditorGUILayout.BeginFadeGroup(_showExtraFields.faded))
            {
                DrawSettings();
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected T CreateSettings<T>() where T : AdNetworkSettingsModelBase
        {
            T settings = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(settings, SettingsFilePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }

        protected T GetOrCreateSettings<T>() where T : AdNetworkSettingsModelBase
        {
            T settings = AssetDatabase.LoadAssetAtPath(SettingsFilePath, typeof(T)) as T; ;
            if (settings == null)
            {
                settings = CreateSettings<T>();
            }
            return settings;
        }

        protected virtual void DrawSettings()
        {
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
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index); //The element in the list

                EditorGUI.LabelField(new Rect(rect.x, rect.y + 2, 100, EditorGUIUtility.singleLineHeight), "Name");

                bool previousGuiEnabled = GUI.enabled;
                if (index == 0)
                {
                    GUI.enabled = false;
                }
                EditorGUI.PropertyField(
                    new Rect(rect.x + 100, rect.y + 5, 250, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("_name"),
                    GUIContent.none
                );
                GUI.enabled = previousGuiEnabled;

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
            };

            list.onCanRemoveCallback = (ReorderableList l) =>
            {
                return l.index != 0;
            };

            return list;
        }
    }
} // namespace Virterix.AdMediation.Editor
#endif
