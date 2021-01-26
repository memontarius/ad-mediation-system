﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;

namespace Virterix.AdMediation.Editor
{
    public class AdMediationSettingsWindow : EditorWindow
    {
        public const string SETTINGS_PATH = "Assets/AdMediationSystem/Editor/Resources/";
        public const string SETTINGS_DIRECTORY_NAME = "AdMediationSettings";
        public const string PROJECT_SETTINGS_FILENAME = "AdMediationProjectSettings.asset";
        public const string PREFIX_SAVEKEY = "adm.";
        public const string PROJECT_NAME_SAVEKEY = "project_name";

        private Vector2 _scrollPositioin;
        private int _selectedTab;
        private List<BaseAdNetworkSettingsView> _networks = new List<BaseAdNetworkSettingsView>();
        private List<string> _activeNetworks = new List<string>();

        private string _projectName;
        private string _createdProjectName;
        private AdMediationProjectSettings _projectSettings;
        private SerializedObject _serializedProjectSettings;

        private List<AdMediatorView> _bannerMediators = new List<AdMediatorView>();
        private List<AdMediatorView> _interstitialMediators = new List<AdMediatorView>();
        private List<AdMediatorView> _incentivizedMediators = new List<AdMediatorView>();

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
            get
            {
                return _activeNetworks.ToArray();
            }
        }

        public string CurrProjectName
        {
            get { return _projectName; }
            private set
            {
                _projectName = value;
                EditorPrefs.SetString(ProjectNameSaveKey, _projectName);
            }
        }

        private string ProjectNameSaveKey
        {
            get { return string.Format("{0}{1}", PREFIX_SAVEKEY, PROJECT_NAME_SAVEKEY);  }
        }

        public string CommonAdSettingsFolderPath
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
        }

        private void OnDisable()
        {
        }

        private void OnGUI()
        {
            _scrollPositioin = EditorGUILayout.BeginScrollView(_scrollPositioin, false, false);
            DrawTabs();
            switch (_selectedTab)
            {
                case 0: // Settings
                    DrawProjectName();
                    DrawProjectSettings();
                    DrawAdNetworks();
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

        public string GetProjectSettingsPath(string projectName)
        {
            return string.Format("{0}/{1}", GetProjectFolderPath(projectName), PROJECT_SETTINGS_FILENAME);
        }

        public string GetProjectFolderPath(string projectName)
        {
            return string.Format("{0}/{1}", CommonAdSettingsFolderPath, projectName);
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
            BaseAdNetworkSettingsView network = new AdNetworkAdMobSettingsView(this, "AdMob", Repaint);
            _networks.Add(network);

            UpdateActiveNetworks();
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

        private void InitMediators()
        {
            CreateAdMediatorViews(ref _bannerMediators, "_bannerMediators");
        }

        private void CreateAdMediatorViews(ref List<AdMediatorView> mediatorList, string propertyName)
        {
            SerializedProperty mediatorsProp = _serializedProjectSettings.FindProperty(propertyName);
            for(int i = 0; i < mediatorsProp.arraySize; i++)
            {
                //SerializedProperty _tierListProp = mediatorsProp.GetArrayElementAtIndex(i).FindPropertyRelative("_tiers");
                AdMediatorView mediatorView = new AdMediatorView(this, i, _serializedProjectSettings, mediatorsProp.GetArrayElementAtIndex(i), Repaint);
                mediatorList.Add(mediatorView);
            }
        }

        private void DrawMediators(List<AdMediatorView> mediatorList, string propertyName)
        {
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
                AdMediatorView mediatorView = new AdMediatorView(this, mediatorList.Count, _serializedProjectSettings, _mediatorProp, Repaint);
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
            _selectedTab = GUILayout.Toolbar(_selectedTab, tabs, toolbarStyle);
            EditorGUILayout.Space();
        }

        private int _selectedProject;
        private string[] _projectNames;

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
                    Init(CurrProjectName);
                    InitNetworksSettings();
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
                    Init(CurrProjectName);
                    InitNetworksSettings();
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
            Utils.DrawGuiLine(2);
            if (GUILayout.Button("BUILD", GUILayout.Height(40)))
            {
                
            }
        }

    }
} // namespace Virterix.AdMediation.Editor