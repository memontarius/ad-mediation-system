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
            T settings = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T; ;
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
    }
}
