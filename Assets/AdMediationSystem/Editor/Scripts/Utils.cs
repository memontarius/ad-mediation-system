using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Virterix.AdMediation.Editor
{
    public static class Utils
    {
        public static T GetOrCreateSettings<T>(string assetPath) where T : ScriptableObject
        {
            T settings = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T;
            if (settings == null)
            {
                settings = CreateSettings<T>(assetPath);
            }
            return settings;
        }

        public static T CreateSettings<T>(string assetPath) where T : ScriptableObject
        {
            T settings = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }

        public static void DrawPropertyField(SerializedObject serializedObject, string fieldName, GUILayoutOption options = null)
        {
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            DrawPropertyField(serializedObject, property, options);
        }

        public static void DrawPropertyField(SerializedObject serializedObject, SerializedProperty property, GUILayoutOption options = null)
        {
            if (options == null)
            {
                EditorGUILayout.PropertyField(property, true);
            }
            else
            {
                EditorGUILayout.PropertyField(property, true, options);
            }
            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawGuiLine(int height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
    }
}
