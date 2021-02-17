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
        public const string SETTINGS_DIRECTORY_NAME = "AdmSettings";
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
                    if (adType != AdType.Unknown && !string.IsNullOrEmpty(CurrProjectName))
                    {
                        UpdateAdInstanceStorage(adType);
                        FixUnitSelectionInMediators(adType);
                    }
                }
            }
        }
        private int _selectedTab;

        private BaseAdNetworkSettings[] NetworkSettings
        {
            get
            {
                BaseAdNetworkSettings[] networksSettings = new BaseAdNetworkSettings[_networks.Count];
                for (int i = 0; i < networksSettings.Length; i++)
                {
                    networksSettings[i] = _networks[i].Settings;
                }
                return networksSettings;
            }
        }

        private Vector2 _scrollPositioin;
        private List<BaseAdNetworkView> _networks = new List<BaseAdNetworkView>();
        private List<string> _activeNetworks = new List<string>();
        private bool[] _networkEnabledStates;
        private bool _wasFirstSetup;

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
        private SerializedProperty _enableTestModeProp;
        private SerializedProperty _testDevicesProp;

        public bool IsAndroid 
        {
            get { return _isAndroidProp.boolValue; }
            private set { _isAndroidProp.boolValue = value; }
        }
        public bool IsIOS
        {
            get { return _isIOSProp.boolValue; }
            private set { _isIOSProp.boolValue = value; }
        }

        public bool IsTestModeEnabled 
        {
            get { return _enableTestModeProp.boolValue; }
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
            string projectName = EditorPrefs.GetString(ProjectNameSaveKey, "");
            Init(projectName);
        }

        private void OnDisable()
        {
            if (_networkEnabledStates != null) 
            {
                for (int i = 0; i < _networkEnabledStates.Length; i++)
                {
                    EditorPrefs.SetBool(GetNetworkEnabledStateSaveKey(_networks[i]), _networkEnabledStates[i]);
                }
            }
            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            _scrollPositioin = EditorGUILayout.BeginScrollView(_scrollPositioin, false, false);
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 160.0f;

            DrawTabs();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(580), GUILayout.Height(0));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            switch (SelectedTab)
            {
                case 0: // Settings
                    _serializedProjectSettings.Update();
                    DrawProjectName();
                    if (IsProjectValid)
                    {
                        DrawProjectSettings();
                        DrawAdNetworks();
                    }
                    _serializedProjectSettings.ApplyModifiedProperties();

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

            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Tools/Ad Mediation System")]
        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(AdMediationSettingsWindow));
            editorWindow.titleContent = new GUIContent("Ad Mediation System");
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

        public BaseAdNetworkView GetNetworkView(string networkName)
        {
            BaseAdNetworkView foundView = null;
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

        public string GetNetworkIndentifier(string networkName)
        {
            string identifier = "";
            foreach(var network in _networks)
            {
                if (network.Name == networkName)
                {
                    identifier = network.Identifier;
                    break;
                }
            }
            return identifier;
        }

        public void GetActiveNetworks(AdType adType, ref List<string> networks)
        {
            networks.Clear();
            foreach (var network in _networks)
            {
                if (network.Enabled && network.Settings.IsAdSupported(adType))
                {
                    networks.Add(network.Name);
                }
            }
        }

        public string[] GetAdInstancesFromStorage(string networkIdentifier, AdType adType)
        {
            string key = networkIdentifier + adType.ToString();
            string[] instances = null;
            if (!_adInstanceStorage.TryGetValue(key, out instances))
            {
                Debug.LogWarning("Not found active network: " + key);
            }
            return instances;
        }

        public void UpdateAdInstanceStorage(AdType adType)
        {
            foreach(var network in _networks)
            {
                string key = network.Identifier + adType.ToString();
                _adInstanceStorage[key] = network.GetAdInstances(adType);
            }
        }

        private void DuplicateSettings(string currProjectSettings, string targetProjectSettings)
        {
            DeleteSettings(targetProjectSettings);
            string targetProjectPath = GetProjectFolderPath(targetProjectSettings);

            AssetDatabase.Refresh();
            AssetDatabase.CreateFolder(CommonAdSettingsFolderPath, targetProjectSettings);
            AssetDatabase.Refresh();

            string[] assets = AssetDatabase.FindAssets("t:ScriptableObject", new[] { GetProjectFolderPath(currProjectSettings) });
            foreach(var asset in assets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(asset);
                string[] splittedPath = assetPath.Split('/');
                string assetName = splittedPath[splittedPath.Length - 1];
                AssetDatabase.CopyAsset(assetPath, string.Format("{0}/{1}", targetProjectPath, assetName));
            }
            AssetDatabase.Refresh();
        }

        private void DeleteSettings(string projectName, bool isChangeOtherSettings = false)
        {
            string targetProjectPath = GetProjectFolderPath(projectName);
            if (AssetDatabase.IsValidFolder(targetProjectPath))
            {
                string deletedDirectoryPath = string.Format("{0}{1}{2}/{3}", Application.dataPath,
                    SETTINGS_PATH.Replace("Assets", ""), SETTINGS_DIRECTORY_NAME, projectName);
                Directory.Delete(deletedDirectoryPath, true); 
            }

            string projectBuildPath = AdMediationSettingsBuilder.GetAdProjectSettingsPath(projectName, true);
            if (AssetDatabase.IsValidFolder(projectBuildPath))
            {
                string deletedDirectoryPath = string.Format("{0}/{1}", Application.dataPath,
                    AdMediationSettingsBuilder.GetAdProjectSettingsPath(projectName, false));
                Directory.Delete(deletedDirectoryPath, true);
            }

            AssetDatabase.Refresh();

            if (isChangeOtherSettings && CurrProjectName == projectName)
            {
                CurrProjectName = _projectNames.FirstOrDefault(name => name != projectName);
            }
        }

        private void Init(string projectName)
        {
            _projectName = projectName;
            if (!string.IsNullOrEmpty(_projectName))
            {
                InitSettings(CurrProjectName);
                InitNetworksSettings();
                InitMediators();
            }
            else
            {
                _wasFirstSetup = true;
                InitProjectNames();
            }
        }

        private void InitSettings(string projectName)
        {
            string projectPath = GetProjectFolderPath(projectName);           
            if (!Directory.Exists(CommonAdSettingsFolderPath))
            {
                Directory.CreateDirectory(CommonAdSettingsFolderPath);
            }
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
                _wasFirstSetup = true;
            }
            _projectSettings = Utils.GetOrCreateSettings<AdMediationProjectSettings>(GetProjectSettingsPath(projectName));
            _serializedProjectSettings = new SerializedObject(_projectSettings);

            _isAndroidProp = _serializedProjectSettings.FindProperty("_isAndroid");
            _isIOSProp = _serializedProjectSettings.FindProperty("_isIOS");
            _enableTestModeProp = _serializedProjectSettings.FindProperty("_enableTestMode");
            _testDevicesProp = _serializedProjectSettings.FindProperty("_testDevices");

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

            AddNetwork(new AdMobView(this, "AdMob", "admob"));
            AddNetwork(new AudienceNetworkView(this, "Audience Network", "audiencenetwork"));
            AddNetwork(new UnityAdsView(this, "Unity Ads", "unityads"));
            AddNetwork(new ApplovinView(this, "AppLovin", "applovin"));
            AddNetwork(new ChartboostView(this, "Chartboost", "chartboost"));
            AddNetwork(new PollfishView(this, "Pollfish", "pollfish"));

            if (_networkEnabledStates == null)
            {
                _networkEnabledStates = new bool[_networks.Count];
                for(int i = 0; i < _networkEnabledStates.Length; i++)
                {
                    _networkEnabledStates[i] = _wasFirstSetup ? false : EditorPrefs.GetBool(GetNetworkEnabledStateSaveKey(_networks[i]));
                }
            }
            AdMediationSettingsBuilder.SetupNetworkScripts(NetworkSettings, _wasFirstSetup, _networkEnabledStates);
            for (int i = 0; i < _networks.Count; i++)
            {
                _networkEnabledStates[i] = _networks[i].Settings._enabled;
            }
            UpdateActiveNetworks();
            UpdateActiveNetworks();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private string GetNetworkEnabledStateSaveKey(BaseAdNetworkView networkView)
        {
            return string.Format("{0}{1}_enabled", AdMediationSettingsWindow.PREFIX_SAVEKEY, networkView.Identifier);
        }

        private void AddNetwork(BaseAdNetworkView networkView)
        {
            _networks.Add(networkView);
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

        private void FixUnitSelectionInAllMediators()
        {
            AdType[] adTypes = System.Enum.GetValues(typeof(AdType)) as AdType[];
            foreach(var adType in adTypes)
            {
                if (adType != AdType.Unknown)
                {
                    FixUnitSelectionInMediators(adType);
                }
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
            FixUnitSelectionInMediators(adType);
            UpdateAdInstanceStorage(adType);
        }

        private void DrawMediators(List<AdMediatorView> mediatorList, string propertyName)
        {
            if (!IsProjectValid)
            {
                EditorGUILayout.HelpBox("Project settings not found!", MessageType.Warning);
                return;
            }

            for (int i = 0; i < mediatorList.Count; i++)
            {
                var mediator = mediatorList[i];
                bool wasDeletionPerform;
                mediator.DrawView(out wasDeletionPerform);
                if (wasDeletionPerform)
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
                mediatorView.SetupDefaultParameters();
                mediatorList.Add(mediatorView);
                _serializedProjectSettings.ApplyModifiedProperties();
                EditorUtility.SetDirty(_projectSettings);
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();
            _serializedProjectSettings.ApplyModifiedProperties();
            _serializedProjectSettings.Update();
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
                GUILayout.BeginHorizontal("box");
                _selectedProject = EditorGUILayout.Popup("Select Project", _selectedProject, _projectNames);         
                if (GUILayout.Button("Delete", GUILayout.Height(18), GUILayout.Width(54)))
                {
                    bool isConfirmDeletion = EditorUtility.DisplayDialog("Deletion Project Settings",
                   "Are you sure you want to delete \"" + CurrProjectName + "\" settings?", "Delete", "No");
                    if (isConfirmDeletion)
                    {
                        DeleteSettings(CurrProjectName, true);
                    }
                }
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck() && _selectedProject < _projectNames.Length)
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
            if (GUILayout.Button("Duplicate Settings", GUILayout.Height(25)))
            {
                if (!string.IsNullOrEmpty(CurrProjectName) && !string.IsNullOrEmpty(_createdProjectName) &&
                    CurrProjectName != _createdProjectName)
                {
                    DuplicateSettings(CurrProjectName, _createdProjectName);
                    CurrProjectName = _createdProjectName;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawProjectSettings()
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);

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
                _serializedProjectSettings.Update();
                foreach (var network in _networks)
                {
                    network.UpdateElementHeight();
                }
            }

            GUILayout.BeginVertical("box");

            Utils.DrawPropertyField(_serializedProjectSettings, "_initializeOnStart");
            Utils.DrawPropertyField(_serializedProjectSettings, "_personalizeAdsOnInit");
            Utils.DrawPropertyField(_serializedProjectSettings, _enableTestModeProp);          
            if (_enableTestModeProp.boolValue)
            {
                Utils.DrawPropertyField(_serializedProjectSettings, _testDevicesProp);
            }
            GUILayout.EndVertical();
        }

        private void DrawAdNetworks()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Networks", EditorStyles.boldLabel);          
            for (int i = 0; i < _networks.Count; i++)
            {
                BaseAdNetworkView network = _networks[i];
                GUILayout.BeginVertical("helpbox");
                bool activationChanged = network.DrawUI();
                if (activationChanged)
                {
                    _networkEnabledStates[i] = network.Settings._enabled;
                    UpdateActiveNetworks();
                    FixUnitSelectionInAllMediators();
                    network.Settings.SetupNetworkAdapterScript();
                    AssetDatabase.Refresh(ImportAssetOptions.Default);
                }
                GUILayout.EndVertical();
            }
        }

        private void DrawBuild()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("BUILD", GUILayout.Height(40)))
            {
                if (!string.IsNullOrEmpty(CurrProjectName))
                {
                    BuildSettings();
                }
            }
        }

        private void BuildSettings()
        {
            string path = AdMediationSettingsBuilder.GetAdProjectSettingsPath(CurrProjectName, true);
            string fullPath = string.Format("{0}/{1}", Application.dataPath, AdMediationSettingsBuilder.GetAdProjectSettingsPath(CurrProjectName, false));
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                AssetDatabase.Refresh();
            }

            string androidAdSettingsPath = string.Format("{0}/{1}", path, "android_settings.json");
            string iosAdSettingsPath = string.Format("{0}/{1}", path, "ios_settings.json");
            BaseAdNetworkSettings[] networksSettings = NetworkSettings;

            List<AdUnitMediator> mediators = new List<AdUnitMediator>();
            AdMediationSettingsBuilder.FillMediators(ref mediators, _projectSettings._bannerMediators);
            AdMediationSettingsBuilder.FillMediators(ref mediators, _projectSettings._interstitialMediators);
            AdMediationSettingsBuilder.FillMediators(ref mediators, _projectSettings._incentivizedMediators);

            AdMediationSettingsBuilder.BuildBannerAdInstanceParameters(CurrProjectName, networksSettings, _projectSettings._bannerMediators.ToArray());
            var prefab = AdMediationSettingsBuilder.BuildSystemPrefab(CurrProjectName, _projectSettings, networksSettings, mediators.ToArray());
            //EditorUtility.FocusProjectWindow();
            //Selection.activeObject = prefab;

            if (IsAndroid)
            {
                string androidJsonSettings = AdMediationSettingsBuilder.BuildJson(CurrProjectName, AppPlatform.Android, _projectSettings, networksSettings, mediators.ToArray());
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
                string iosJsonSettings = AdMediationSettingsBuilder.BuildJson(CurrProjectName, AppPlatform.iOS, _projectSettings, networksSettings, mediators.ToArray());
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