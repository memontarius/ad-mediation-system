using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

namespace Virterix.AdMediation.Editor
{
    public class AdMobView : BaseAdNetworkView
    {
        private SerializedProperty _useMediationProp;

        public AdMobView(AdMediationSettingsWindow settingsWindow, string name, string identifier) : 
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(AdMobAdapter.AdMobBannerSize));
            _useMediationProp = _serializedSettings.FindProperty("_useMediation");
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AdMobSettings>(SettingsFilePath);
            return settings;
        }

        protected override void SetupReorderableList(ReorderableList list, AdType adType)
        {
        }

        protected override void DrawSpecificSettings()
        {
            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            _useMediationProp.boolValue = EditorGUILayout.Toggle("Use Mediation", _useMediationProp.boolValue);
            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                WriteMediationDefinitionInScript(_useMediationProp.boolValue);
                AssetDatabase.Refresh();
            }
            GUILayout.EndVertical();
        }

        public void WriteMediationDefinitionInScript(bool define)
        {
            string[] findFolders = new[] { "Assets/AdMediationSystem/Scripts" };
            string[] assets = AssetDatabase.FindAssets("AdMobMediationBehavior", findFolders);

            if (assets.Length == 1)
            {
                var assetLocalPath = AssetDatabase.GUIDToAssetPath(assets[0]).Remove(0, 6);
                string scriptPath = string.Format("{0}/{1}", Application.dataPath, assetLocalPath);

                string content = File.ReadAllText(scriptPath);
                if (content.Length > 0)
                {
                    string defineMacros = "#define _ADMOB_MEDIATION";
                    string undefineMacros = "//#define _ADMOB_MEDIATION";

                    if (define)
                    {
                        content = content.Replace(undefineMacros, defineMacros);
                    }
                    else
                    {
                        if (!content.Contains(undefineMacros))
                            content = content.Replace(defineMacros, undefineMacros);
                    }
                    File.WriteAllText(scriptPath, content);
                }
            }


        }
    }

} // namespace Virterix.AdMediation.Editor