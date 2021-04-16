using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Virterix.AdMediation
{
    public interface IAdInstanceParameters
    {
        AdType AdvertiseType { get; }
        string Name { get; set; }
    }

    [Serializable]
    public struct BannerPositionContainer
    {
        public string m_placementName;
        public int m_bannerPosition;
    }

    public class AdInstanceParameters : ScriptableObject, IAdInstanceParameters
    {
        public const string AD_INSTANCE_PARAMETERS_DEFAULT_NAME = "Default";
        [SerializeField]
        private string m_name = AD_INSTANCE_PARAMETERS_DEFAULT_NAME;

        public BannerPositionContainer[] m_bannerPositions;

        public virtual AdType AdvertiseType
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

#if UNITY_EDITOR
        public static T CreateParameters<T>(string projectName, string parameterFolder, string parameterFileName) where T : AdInstanceParameters
        {
            string fullPath = CreateAdInstanceDirectory(projectName, parameterFolder);
            string searchPattern = "*" + AdMediationSystem.AD_INSTANCE_PARAMETERS_FILE_EXTENSION;
            string[] files = Directory.GetFiles(fullPath, searchPattern, SearchOption.TopDirectoryOnly);
            string path = string.Format(System.Globalization.CultureInfo.InvariantCulture, 
                "Assets/{0}/{1}/{2}", AdMediationSystem.GetAdInstanceParametersPath(projectName), parameterFolder, parameterFileName);
            //path += (files.Length == 0 ? "" : " " + (files.Length + 1)) + AdMediationSystem._AD_INSTANCE_PARAMETERS_FILE_EXTENSION;
            path += AdMediationSystem.AD_INSTANCE_PARAMETERS_FILE_EXTENSION;

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return asset;
        }

        public static T CreateParameters<T>(string parameterFolder, string parameterFileName) where T : AdInstanceParameters
        {
            T result = CreateParameters<T>(AdMediationSystem.Instance.m_projectName, parameterFolder, parameterFileName);
            return result;
        }

        public static string CreateAdInstanceDirectory(string projectName, string specificPath)
        {
            string fullPath = Application.dataPath + "/" + AdMediationSystem.GetAdInstanceParametersPath(projectName) + "/" + specificPath;
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }
#endif
    }
} // Virterix.AdMediation