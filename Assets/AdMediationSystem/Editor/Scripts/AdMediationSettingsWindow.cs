using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;
using Boomlagoon.JSON;

namespace Virterix.AdMediation.Editor
{
    public class AdMediationSettingsWindow : EditorWindow
    {
        public const string SETTINGS_PATH = "Assets/AdMediationSystem/Editor/Resources/";
        public const string SETTINGS_DIRECTORY_NAME = "AdMediationSettings";
        public const string PROJECT_SETTINGS_FILENAME = "AdMediationProjectSettings.asset";
        public const string PREFIX_SAVEKEY = "adm.";
        public const string PROJECT_NAME_SAVEKEY = "project_name";

        private int SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                int previousTab = _selectedTab;
                _selectedTab = value;
                if (_selectedTab != previousTab)
                {
                    AdType adType = ConvertTabIndexToAdType(_selectedTab);
                    if (adType != AdType.Unknown)
                    {
                        UpdateAdInstanceStorage(adType);
                        FixUnitSelectionInMediators(adType);
                    }
                }
            }
        }
        private int _selectedTab;

        private Vector2 _scrollPositioin;
        private List<BaseAdNetworkSettingsView> _networks = new List<BaseAdNetworkSettingsView>();
        private List<string> _activeNetworks = new List<string>();

        private int _selectedProject;
        private string[] _projectNames;
        private string _projectName;
        private string _createdProjectName;
        private AdMediationProjectSettings _projectSettings;
        private SerializedObject _serializedProjectSettings;

        private List<AdMediatorView> _bannerMediators = new List<AdMediatorView>();
        private List<AdMediatorView> _interstitialMediators = new List<AdMediatorView>();
        private List<AdMediatorView> _incentivizedMediators = new List<AdMediatorView>();
        private Dictionary<string, string[]> _adInstanceStorage = new Dictionary<string, string[]>();
        
        private SerializedProperty _isAndroidProp;
        private SerializedProperty _isIOSProp;

        public bool IsAndroid 
        {
            get { return _isAndroidProp.boolValue; }
            private set
            {
                _isAndroidProp.boolValue = value;
            }
        }
        public bool IsIOS
        {
            get { return _isIOSProp.boolValue; }
            private set
            {
                _isIOSProp.boolValue = value;
            }
        }

        public string[] ActiveNetworks
        {
            get { return _activeNetworks.ToArray(); }
        }

        public string CurrProjectName
        {
            get { return _projectName; }
            private set
            {
                bool isChanged = value != _projectName;
                _projectName = value;
                EditorPrefs.SetString(ProjectNameSaveKey, _projectName);
                if (isChanged)
                {
                    Init(_projectName);
                    InitNetworksSettings();
                    InitMediators();
                }
            }
        }

        public bool IsProjectValid
        {
            get { return !string.IsNullOrEmpty(CurrProjectName) && _serializedProjectSettings != null; }
        }

        private string ProjectNameSaveKey
        {
            get { return string.Format("{0}{1}", PREFIX_SAVEKEY, PROJECT_NAME_SAVEKEY);  }
        }

        public static string CommonAdSettingsFolderPath
        {
            get { return string.Format("{0}{1}", SETTINGS_PATH, SETTINGS_DIRECTORY_NAME); }
        }

        private void OnEnable()
        {
            _projectName = EditorPrefs.GetString(ProjectNameSaveKey, "");
            if (!string.IsNullOrEmpty(_projectName))
            {
                Init(CurrProjectName);
                InitNetworksSettings();
                InitMediators();
            }
            else
            {
                InitProjectNames();
            }
        }

        private void OnDisable()
        {
        }

        private void OnGUI()
        {
            _scrollPositioin = EditorGUILayout.BeginScrollView(_scrollPositioin, false, false);

            DrawTabs();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(580), GUILayout.Height(0));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            switch (SelectedTab)
            {
                case 0: // Settings
                    DrawProjectName();
                    if (IsProjectValid)
                    {
                        DrawProjectSettings();
                        DrawAdNetworks();
                    }
                    break;
                case 1: // Banner mediators
                    DrawMediators(_bannerMediators, "_bannerMediators");
                    break;
                case 2: // Interstitial mediators
                    DrawMediators(_interstitialMediators, "_interstitialMediators");
                    break;
                case 3: // Incentivized mediators
                    DrawMediators(_incentivizedMediators, "_incentivizedMediators");
                    break;
            }
            DrawBuild();           
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Tools/Ad Mediation/Ad Mediation Settings")]
        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(AdMediationSettingsWindow));
            editorWindow.titleContent = new GUIContent("Ad Mediation Settings");
            editorWindow.Show();
        }

        public static string GetProjectSettingsPath(string projectName)
        {
            return string.Format("{0}/{1}", GetProjectFolderPath(projectName), PROJECT_SETTINGS_FILENAME);
        }

        public static string GetProjectFolderPath(string projectName)
        {
            return string.Format("{0}/{1}", CommonAdSettingsFolderPath, projectName);
        }

        public BaseAdNetworkSettingsView GetNetworkView(string networkName)
        {
            BaseAdNetworkSettingsView foundView = null;
            foreach (var view in _networks)
            {
                if (view.Name == networkName)
                {
                    foundView = view;
                    break;
                }
            }
            return foundView;
        }

        public string GetNetworkIndentifier(int index)
        {
            string identifier = _networks[index].Identifier;
            return identifier;
        }

        public void GetActiveNetworks(AdType adType, ref List<string> networks)
        {
            networks.Clear();
            foreach (var network in _networks)
            {
                if (network.Enabled && network.IsAdSupported(adType))
                {
                    networks.Add(network.Name);
                }
            }
        }

        public string[] GetAdInstancesFromStorage(string network, AdType adType)
        {
            string key = network + adType.ToString();
            string[] instances = null;
            if (!_adInstanceStorage.TryGetValue(key, out instances))
            {
            }
            return instances;
        }

        public void UpdateAdInstanceStorage(AdType adType)
        {
            foreach(var network in _networks)
            {
                string key = network.Name + adType.ToString();
                _adInstanceStorage[key] = network.GetAdInstances(adType);
            }
        }

        private void Init(string projectName)
        {
            string projectPath = GetProjectFolderPath(projectName);           
            if (!Directory.Exists(CommonAdSettingsFolderPath))
            {
                Directory.CreateDirectory(CommonAdSettingsFolderPath);
            }
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }
            _projectSettings = Utils.GetOrCreateSettings<AdMediationProjectSettings>(GetProjectSettingsPath(projectName));
            _serializedProjectSettings = new SerializedObject(_projectSettings);
            _isAndroidProp = _serializedProjectSettings.FindProperty("_isAndroid");
            _isIOSProp = _serializedProjectSettings.FindProperty("_isIOS");
            InitProjectNames();
        }

        private void InitProjectNames()
        {
            if (!Directory.Exists(CommonAdSettingsFolderPath))
            {
                Directory.CreateDirectory(CommonAdSettingsFolderPath);
            }
            DirectoryInfo dir = new DirectoryInfo(CommonAdSettingsFolderPath);
            DirectoryInfo[] projectDirectories = dir.GetDirectories();
            _projectNames = new string[projectDirectories.Length];
            for (int i = 0; i < _projectNames.Length; i++)
            {
                _projectNames[i] = projectDirectories[i].Name;
                if (CurrProjectName == _projectNames[i])
                {
                    _selectedProject = i;
                }
            }
        }

        private void InitNetworksSettings()
        {
            _networks.Clear();
            BaseAdNetworkSettingsView network = new AdNetworkAdMobSettingsView(this, "AdMob", "admob");
            _networks.Add(network);

            network = new AdNetworkUnitySettingsView(this, "Unity Ads", "unityads");
            _networks.Add(network);
            
            UpdateActiveNetworks();
        }

        private void InitMediators()
        {
            _bannerMediators.Clear();
            _interstitialMediators.Clear();
            _incentivizedMediators.Clear();
            CreateAdMediatorViews(ref _bannerMediators, "_bannerMediators", AdType.Banner);
            CreateAdMediatorViews(ref _interstitialMediators, "_interstitialMediators", AdType.Interstitial);
            CreateAdMediatorViews(ref _incentivizedMediators, "_incentivizedMediators", AdType.Incentivized);
        }

        private void UpdateActiveNetworks()
        {
            _activeNetworks.Clear();
            foreach (var network in _networks)
            {
                if (network.Enabled)
                {
                    _activeNetworks.Add(network.Name);
                }
                else
                {
                    _activeNetworks.Remove(network.Name);
                }
            }
        }

        private void FixUnitSelectionInMediators(AdType adType)
        {
            List<AdMediatorView> mediators = null;
            switch(adType)
            {
                case AdType.Banner:
                    mediators = _bannerMediators;
                    break;
                case AdType.Interstitial:
                    mediators = _interstitialMediators;
                    break;
                case AdType.Incentivized:
                    mediators = _incentivizedMediators;
                    break;
            }
            foreach (var mediator in mediators)
            {
                mediator.FixPopupSelection();
            }
        }

        private AdType GetActiveTab()
        {
            return ConvertTabIndexToAdType(SelectedTab);
        }

        private AdType ConvertTabIndexToAdType(int tab)
        {
            AdType result = AdType.Unknown;
            switch (SelectedTab)
            {
                case 1:
                    result = AdType.Banner;
                    break;
                case 2:
                    result = AdType.Interstitial;
                    break;
                case 3:
                    result = AdType.Incentivized;
                    break;
            }
            return result;
        }

        private void CreateAdMediatorViews(ref List<AdMediatorView> mediatorList, string propertyName, AdType adType)
        {
            SerializedProperty mediatorsProp = _serializedProjectSettings.FindProperty(propertyName);
            for (int i = 0; i < mediatorsProp.arraySize; i++)
            {
                AdMediatorView mediatorView = new AdMediatorView(this, i, _serializedProjectSettings, 
                    mediatorsProp.GetArrayElementAtIndex(i), Repaint, adType);
                mediatorList.Add(mediatorView);
            }
        }

        private void DrawMediators(List<AdMediatorView> mediatorList, string propertyName)
        {
            if (!IsProjectValid)
            {
                EditorGUILayout.HelpBox("Project settings not found!", MessageType.Warning);
                return;
            }

            _serializedProjectSettings.Update();
            for (int i = 0; i < mediatorList.Count; i++)
            {
                var mediator = mediatorList[i];
                bool isDeletionPerform = mediator.DrawView();
                if (isDeletionPerform)
                {
                    var mediatorsProp = _serializedProjectSettings.FindProperty(propertyName);
                    mediatorsProp.DeleteArrayElementAtIndex(i);
                    mediatorList.RemoveAt(i);
                    i--;

                    for (int remainingIndex = i + 1; remainingIndex < mediatorList.Count && mediatorList.Count > 0; remainingIndex++)
                    {
                        SerializedProperty _mediatorProp = mediatorsProp.GetArrayElementAtIndex(remainingIndex);
                        mediatorList[remainingIndex].SetProperty(remainingIndex, _mediatorProp);
                    }
                }
            }

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Mediator", GUILayout.Height(30)))
            {
                var mediatorsProp = _serializedProjectSettings.FindProperty(propertyName);
                int insertIndex = mediatorsProp.arraySize;
                mediatorsProp.InsertArrayElementAtIndex(insertIndex);
                SerializedProperty _mediatorProp = mediatorsProp.GetArrayElementAtIndex(insertIndex);
                AdMediatorView mediatorView = new AdMediatorView(this, mediatorList.Count, 
                    _serializedProjectSettings, _mediatorProp, Repaint, GetActiveTab());
                mediatorList.Add(mediatorView);
            }
            GUILayout.EndHorizontal();
            _serializedProjectSettings.ApplyModifiedProperties();
        }

        private void DrawTabs()
        {
            EditorGUILayout.Space();
            string[] tabs = { "SETTINGS", "BANNERS", "INTERSTITIALS", "REWARDED ADS" };
            GUIStyle toolbarStyle = EditorStyles.toolbarButton;
            SelectedTab = GUILayout.Toolbar(SelectedTab, tabs, toolbarStyle);
            EditorGUILayout.Space();
        }

        private void DrawProjectName()
        {
            if (_projectNames != null && _projectNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("box");
                _selectedProject = EditorGUILayout.Popup("Select Project", _selectedProject, _projectNames);
                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    CurrProjectName = _projectNames[_selectedProject];
                }
            }

            GUILayout.BeginVertical("box");

            _createdProjectName = EditorGUILayout.TextField("Project Name", _createdProjectName);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Settings", GUILayout.Height(25)))
            {
                if (!string.IsNullOrEmpty(_createdProjectName))
                {
                    CurrProjectName = _createdProjectName;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawProjectSettings()
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            _serializedProjectSettings.Update();
            GUILayout.BeginHorizontal("box");
            IsAndroid = GUILayout.Toggle(IsAndroid, " Android");
            if (!IsAndroid && !IsIOS)
            {
                IsIOS = true;
            }
            GUILayout.Space(20);
            IsIOS = GUILayout.Toggle(IsIOS, " iOS");
            if (!IsAndroid && !IsIOS)
            {
                IsAndroid = true;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (_serializedProjectSettings.ApplyModifiedProperties())
            {
                foreach(var network in _networks)
                {
                    network.UpdateElementHeight();
                }
            }

            GUILayout.BeginVertical("box");
            Utils.DrawPropertyField(_serializedProjectSettings, "_initializeOnStart", GUILayout.ExpandWidth(true));
            Utils.DrawPropertyField(_serializedProjectSettings, "_personalizeAdsOnInit");
            GUILayout.EndVertical();
        }

        private void DrawAdNetworks()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Networks", EditorStyles.boldLabel);          
            foreach (BaseAdNetworkSettingsView network in _networks)
            {
                GUILayout.BeginVertical("helpbox");
                bool activationChanged = network.DrawUI();
                if (activationChanged)
                {
                    UpdateActiveNetworks();
                }
                GUILayout.EndVertical();
            }
        }

        private void DrawBuild()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("BUILD", GUILayout.Height(40)))
            {
                BuildSettings();
            }
        }

        private void BuildSettings()
        {
            string path = AdMediationSettingsGenerator.GetAdProjectSettingsPath(CurrProjectName, true);
            string fullPath = string.Format("{0}/{1}", Application.dataPath, AdMediationSettingsGenerator.GetAdProjectSettingsPath(CurrProjectName, false));
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                AssetDatabase.Refresh();
            }

            string androidAdSettingsPath = string.Format("{0}/{1}", path, "android_settings.json");
            string iosAdSettingsPath = string.Format("{0}/{1}", path, "ios_settings.json");

            BaseAdNetworkSettings[] networksSettings = new BaseAdNetworkSettings[_networks.Count];
            for(int i = 0; i < networksSettings.Length; i++)
            {
                networksSettings[i] = _networks[i].Settings;
            }

            List<AdUnitMediator> mediators = new List<AdUnitMediator>();
            AdMediationSettingsGenerator.FillMediators(ref mediators, _projectSettings._bannerMediators);
            AdMediationSettingsGenerator.FillMediators(ref mediators, _projectSettings._interstitialMediators);
            AdMediationSettingsGenerator.FillMediators(ref mediators, _projectSettings._incentivizedMediators);

            AdMediationSettingsGenerator.GenerateSystemPrefab(CurrProjectName, _projectSettings, mediators.ToArray());
            AdMediationSettingsGenerator.GenerateBannerAdInstanceParameters(CurrProjectName, networksSettings, _projectSettings._bannerMediators.ToArray());

            if (IsAndroid)
            {
                string androidJsonSettings = AdMediationSettingsGenerator.GenerateJson(CurrProjectName, AppPlatform.Android, _projectSettings, networksSettings, mediators.ToArray());
                File.WriteAllText(androidAdSettingsPath, androidJsonSettings);
            }
            else
            {
                if (File.Exists(androidAdSettingsPath))
                {
                    File.Delete(androidAdSettingsPath);
                }
            }

            if (IsIOS)
            {
                string iosJsonSettings = AdMediationSettingsGenerator.GenerateJson(CurrProjectName, AppPlatform.iOS, _projectSettings, networksSettings, mediators.ToArray());
                File.WriteAllText(iosAdSettingsPath, iosJsonSettings);
            }
            else
            {
                if (File.Exists(iosAdSettingsPath))
                {
                    File.Delete(iosAdSettingsPath);
                }
            }
            AssetDatabase.Refresh();
        }
    }
} // namespace Virterix.AdMediation.Editor