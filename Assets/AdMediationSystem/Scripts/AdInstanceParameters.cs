using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Virterix.AdMediation
{
    public interface IAdInstanceParameters
    {
        AdType AdvertiseType { get; }
        string Name { get; }
    }

    public class AdInstanceParameters : ScriptableObject, IAdInstanceParameters
    {
        public const string _ADINSTANCE_PARAMETERS_DEFAULT_NAME = "Default";
        [SerializeField]
        private string m_name = _ADINSTANCE_PARAMETERS_DEFAULT_NAME;

        public virtual AdType AdvertiseType
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

#if UNITY_EDITOR
        public static void CreateParameters<T>(string parametersFolder, string parametersFileName) where T : AdInstanceParameters
        {
            string fullPath = CreateAdInstanceDirectory(parametersFolder);
            string searchPattern = "*" + AdMediationSystem._AD_INSTANCE_PARAMETERS_FILE_EXTENSION;
            string[] files = Directory.GetFiles(fullPath, searchPattern, SearchOption.TopDirectoryOnly);
            string path = string.Format("Assets/{0}/{1}/{2}", AdMediationSystem.AdInstanceParametersPath, parametersFolder, parametersFileName);
            path += (files.Length == 0 ? "" : " " + (files.Length + 1)) + AdMediationSystem._AD_INSTANCE_PARAMETERS_FILE_EXTENSION;

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            AssetDatabase.Refresh();
        }

        public static string CreateAdInstanceDirectory(string specificPath)
        {
            string fullPath = Application.dataPath + "/" + AdMediationSystem.AdInstanceParametersPath + "/" + specificPath;
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }
#endif
    }
} // Virterix.AdMediation