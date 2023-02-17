using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Linq;

namespace Virterix.AdMediation.Editor
{
    public static class Utils
    {
        public static AdType[] SupportedMediationAdTypes
        {
            get
            {
                AdType[] adTypeArray = System.Enum.GetValues(typeof(AdType)) as AdType[];
                List<AdType> adTypes = adTypeArray.ToList();
                for (int i = 0; i < adTypes.Count; i++)
                {
                    if (!AdMediationSettingsBuilder.IsMediationSupport(adTypes[i]))
                    {
                        adTypes.Remove(adTypes[i]);
                        i--;
                            
                    }
                }
                return adTypes.ToArray();
            }
        }
        
        public static string[] EditorAdTypes
        {
            get
            {
                if (_editorAdTypes == null)
                    _editorAdTypes = Enum.GetNames(typeof(EditorAdType));
                return _editorAdTypes;
            }
        }
        private static string[] _editorAdTypes;

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
            AssetDatabase.ImportAsset(assetPath);
            //AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }

        public static void DrawPropertyField(SerializedObject serializedObject, string fieldName, params GUILayoutOption[] options)
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

        public static void DrawGuiLine(int height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        public static void DeleteMetaFile(string assetPath)
        {
            string metaFilePath = assetPath + ".meta";
            if (File.Exists(metaFilePath))
            {
                File.Delete(metaFilePath);
            }
        }

        public static AdType ConvertEditorAdType(EditorAdType adType)
        {
            AdType result = AdType.Unknown;
            switch (adType)
            {
                case EditorAdType.Banner:
                    result = AdType.Banner;
                    break;
                case EditorAdType.Interstitial:
                    result = AdType.Interstitial;
                    break;
                case EditorAdType.Incentivized:
                    result = AdType.Incentivized;
                    break;
            }
            return result;
        }

        public static bool RewriteScriptDefinition(string path, string scriptName, string definitionName, bool active)
        {
            string scriptPath = string.Format("{0}/{1}/{2}.cs", Application.dataPath, path, scriptName);
            bool replaced = false;
            
            string content = File.ReadAllText(scriptPath);
            if (content.Length > 0)
            {
                string define = "#define " + definitionName;
                string undefine = "//#define " + definitionName;

                if (active)
                {
                    replaced = content.Contains(undefine);
                    if (replaced)
                        content = content.Replace(undefine, define);
                }
                else
                {
                    replaced = !content.Contains(undefine);
                    if (replaced)
                        content = content.Replace(define, undefine);
                }
                File.WriteAllText(scriptPath, content);
            }
            return replaced;
        }
    }
}
