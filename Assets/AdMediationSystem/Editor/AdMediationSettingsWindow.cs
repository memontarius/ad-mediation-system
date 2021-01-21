using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Virterix.AdMediation.Editor
{
    public enum AdType
    {
        Banner,
        Interstitial,
        Incentivized
    }

    public class AdMediationSettingsWindow : EditorWindow
    {
        public const string SETTINGS_PATH = "Assets/AdMediationSystem/Editor/Resources/";
        public const string PROJECT_SETTINGS_FILENAME = "AdMediationProjectSettings.asset";
        public const string PREFIX_SAVEKEY = "adm.";
        public const string PROJECT_NAME_SAVEKEY = "project_name";

        private Vector2 _scrollPositioin;
        private int _selectedTab;
        private List<BaseAdNetworkSettingsView> _networks = new List<BaseAdNetworkSettingsView>();

        private AnimBool m_ShowExtraFields;
        private static string m_projectName;
        private AdMediationProjectSettings m_projectNameSettings;
        
        public static string ProjectName
        {
            get { return m_projectName; }
        }

        private string ProjectNameSaveKey
        {
            get { return string.Format("{0}{1}", PREFIX_SAVEKEY, PROJECT_NAME_SAVEKEY); }
        }

        private void OnEnable()
        {
            m_ShowExtraFields = new AnimBool(true);
            m_ShowExtraFields.valueChanged.AddListener(Repaint);
            m_projectName = EditorPrefs.GetString(ProjectNameSaveKey, "");
            if (!string.IsNullOrEmpty(m_projectName))
            {
                Init();
                InitNetworksSettings();
            }
        }

        private void OnDisable()
        {
            Save();
        }

        private void OnGUI()
        {
            _scrollPositioin = EditorGUILayout.BeginScrollView(_scrollPositioin, false, false);
            EditorGUILayout.Space();
            DrawTabs();   
            EditorGUILayout.Space();

            switch (_selectedTab)
            {
                case 0:
                    DrawProjectName();
                    DrawAdNetworks();
                    break;
                case 1:
                    break;
            }
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Tools/Ad Mediation/Ad Mediation Settings")]
        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(AdMediationSettingsWindow));
            editorWindow.titleContent = new GUIContent("Ad Mediation Settings");
            editorWindow.Show();
        }

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

        private void Init()
        {
            m_projectNameSettings = GetOrCreateSettings<AdMediationProjectSettings>(GetProjectSettingsPath(m_projectName));
        }

        private void InitNetworksSettings()
        {
            _networks.Clear();
            BaseAdNetworkSettingsView network = new AdNetworkAdMobSettingsView("AdMob", Repaint);
            _networks.Add(network);
        }

        private void Save()
        {
        }

        private void DrawTabs()
        {
            string[] tabs = { "SETTINGS", "BANNERS", "INTERSTITIALS", "REWARDED ADS" };
            GUIStyle toolbarStyle = EditorStyles.toolbarButton;
            _selectedTab = GUILayout.Toolbar(_selectedTab, tabs, toolbarStyle);
        }

        private void DrawProjectName()
        {
            //GUILayout.Label("Project Name", EditorStyles.boldLabel);
            m_projectName = EditorGUILayout.TextField("Project Name", m_projectName);
            if (GUILayout.Button("Load or Create Settings", GUILayout.Height(40)))
            {
                if (!string.IsNullOrEmpty(m_projectName))
                {
                    EditorPrefs.SetString(ProjectNameSaveKey, m_projectName);

                    Init();
                    InitNetworksSettings();
                }
            }
        }

        private void DrawAdNetworks()
        {
            EditorGUILayout.Space();
            foreach (BaseAdNetworkSettingsView network in _networks)
            {
                network.DrawUI();
            }
        }

        private string GetProjectSettingsPath(string projectName)
        {
            return string.Format("{0}{1}/{2}", SETTINGS_PATH, projectName, PROJECT_SETTINGS_FILENAME);
        }
    }
} // namespace Virterix.AdMediation.Editor
#endif
