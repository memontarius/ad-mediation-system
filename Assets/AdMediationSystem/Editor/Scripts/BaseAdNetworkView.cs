using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace Virterix.AdMediation.Editor
{
    public abstract class BaseAdNetworkView
    {
        public struct InstanceElementHeight
        {
            public float height;
            public float androidHeight;
            public float iosHeight;
        }

        public class AdInstanceBlockData
        {
            public string _blockName;
            public AdType _adType;
            public ReorderableList _instances;
            public SerializedProperty _instanceProperty;
            public bool _isCollapsed;
            public AnimBool _foldAnimation;
            public InstanceElementHeight _elementHeight;
        }

        public string Name { get; private set; }

        public string Identifier { get; private set; }

        public bool Enabled { get; set; }

        public bool Collapsed { get; set; }

        public BaseAdNetworkSettings Settings
        {
            get { return _settings; }
        }

        protected abstract string SettingsFileName { get; }

        protected virtual string[] BannerTypes { get; set; }

        protected virtual bool IsAppIdSupported { get; set; }

        protected string SettingsFilePath
        {
            get
            {
                return String.Format("{0}/{1}", AdMediationSettingsWindow.GetProjectFolderPath(_settingsWindow.CurrProjectName), SettingsFileName);
            }
        }

        private string CollapsedSaveKey
        {
            get
            {
                return string.Format("{0}{1}.collapsed", AdMediationSettingsWindow.PREFIX_SAVEKEY, Name);
            }
        }

        protected AdMediationSettingsWindow _settingsWindow;
        protected SerializedObject _serializedSettings;
        protected BaseAdNetworkSettings _settings;

        private SerializedProperty _androidAppIdProp;
        private SerializedProperty _iosAppIdProp;
        private SerializedProperty _responseWaitTimeProp;

        private AnimBool _showSettings;
        private SerializedProperty _enabledProp;
        private List<AdInstanceBlockData> _instanceBlocks = new List<AdInstanceBlockData>();

        private GUIStyle InstanceFoldoutButtonStyle
        {
            get
            {
                if (_instanceFoldoutButtonStyle == null)
                {
                    _instanceFoldoutButtonStyle = new GUIStyle(GUI.skin.GetStyle("button"));
                    _instanceFoldoutButtonStyle.alignment = TextAnchor.MiddleLeft;
                    _instanceFoldoutButtonStyle.margin = new RectOffset(1, 1, 0, 1);
                    _instanceFoldoutButtonStyle.padding = new RectOffset(6, 6, 3, 3);
                }
                return _instanceFoldoutButtonStyle;
            }
        }
        private GUIStyle _instanceFoldoutButtonStyle;

        public BaseAdNetworkView(AdMediationSettingsWindow settingsWindow, string name, string identifier)
        {
            _settingsWindow = settingsWindow;
            Name = name;
            Identifier = identifier;
            _showSettings = new AnimBool(true);
            _showSettings.valueChanged.AddListener(settingsWindow.Repaint);

            Collapsed = EditorPrefs.GetBool(CollapsedSaveKey, false);
            _settings = GetSettings();
            _settings._networkIdentifier = identifier;

            InitAdInstanceBlock(AdType.Banner, "Banners", "_bannerAdInstances");
            InitAdInstanceBlock(AdType.Interstitial, "Interstitials", "_interstitialAdInstances");
            InitAdInstanceBlock(AdType.Incentivized, "Reward Units", "_rewardAdInstances");

            _enabledProp = _serializedSettings.FindProperty("_enabled");
            _responseWaitTimeProp = _serializedSettings.FindProperty("_responseWaitTime");
            // Android
            _androidAppIdProp = _serializedSettings.FindProperty("_androidAppId");
            // iOS
            _iosAppIdProp = _serializedSettings.FindProperty("_iosAppId");

            UpdateElementHeight();
            Enabled = _enabledProp.boolValue;
        }

        private void InitAdInstanceBlock(AdType adType, string title, string propertyName)
        {
            if (_settings.IsAdSupported(adType) && _settings.IsAdInstanceSupported(adType))
            {
                InstanceElementHeight elementHeight = CreateInstanceElementHeight(adType);
                CreateAdInstanceBlock(title, propertyName, adType, elementHeight);
            }
        }

        public bool DrawUI()
        {
            bool previousCollapsed = Collapsed;
            bool activationChanged = false;

            Collapsed = EditorGUILayout.BeginFoldoutHeaderGroup(Collapsed, Name);
            if (Collapsed != previousCollapsed)
            {
                EditorPrefs.SetBool(CollapsedSaveKey, Collapsed);
            }

            _showSettings.target = Collapsed;
            if (EditorGUILayout.BeginFadeGroup(_showSettings.faded))
            {
                Enabled = EditorGUILayout.BeginToggleGroup("Enable", Enabled);
                if (_enabledProp.boolValue != Enabled)
                {
                    activationChanged = true;
                }
                _enabledProp.boolValue = Enabled;
                DrawCommonSettigns();
                DrawAppIds();
                DrawSpecificSettings();
                DrawAdInstanceLists();
                _serializedSettings.ApplyModifiedProperties();
                _serializedSettings.Update();
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();

            return activationChanged;
        }

        public void UpdateElementHeight()
        {
            foreach (var instanceBlock in _instanceBlocks)
            {
                instanceBlock._instances.elementHeight = CalculateElementHeight(instanceBlock);
            }
        }

        public string[] GetAdInstances(AdType adType)
        {
            string[] instances = null;
            switch (adType)
            {
                case AdType.Banner:
                    instances = new string[_settings._bannerAdInstances.Count];
                    for (int i = 0; i < instances.Length; i++)
                    {
                        instances[i] = _settings._bannerAdInstances[i]._name;
                    }
                    break;
                case AdType.Interstitial:
                    instances = new string[_settings._interstitialAdInstances.Count];
                    for (int i = 0; i < instances.Length; i++)
                    {
                        instances[i] = _settings._interstitialAdInstances[i]._name;
                    }
                    break;
                case AdType.Incentivized:
                    instances = new string[_settings._rewardAdInstances.Count];
                    for (int i = 0; i < instances.Length; i++)
                    {
                        instances[i] = _settings._rewardAdInstances[i]._name;
                    }
                    break;
            }
            return instances;
        }

        private string GetInstanceBlockCollapsedKey(string network, AdType adType)
        {
            string saveKey = string.Format("{0}{1}{2}", AdMediationSettingsWindow.PREFIX_SAVEKEY, network, adType.ToString());
            return saveKey;
        }

        protected float CalculateElementHeight(AdInstanceBlockData blockData)
        {
            float solvedHeight = 22;
            if (blockData._instances != null && blockData._instances.count > 0) {
                if (_settingsWindow.IsAndroid && _settingsWindow.IsIOS)
                {
                    solvedHeight = blockData._elementHeight.height;
                }
                else if (_settingsWindow.IsAndroid)
                {
                    solvedHeight = blockData._elementHeight.androidHeight;
                }
                else if (_settingsWindow.IsIOS)
                {
                    solvedHeight = blockData._elementHeight.iosHeight;
                }
            }
            return solvedHeight;
        }

        protected abstract BaseAdNetworkSettings CreateSettingsModel();

        private BaseAdNetworkSettings GetSettings()
        {
            string filePath = string.Format("{0}", SettingsFilePath);
            bool isFileDidNotExist = !System.IO.File.Exists(filePath);
            
            var settings = CreateSettingsModel();
            _serializedSettings = new SerializedObject(settings);

            if (isFileDidNotExist)
            {                
                var responseWaitTimeProp = _serializedSettings.FindProperty("_responseWaitTime");
                responseWaitTimeProp.intValue = 30;
            }
            return settings;
        }

        protected virtual InstanceElementHeight CreateInstanceElementHeight(AdType adType)
        {
            InstanceElementHeight elementHeight = new InstanceElementHeight();
            switch(adType)
            {
                case AdType.Banner:
                    elementHeight.height = 88;
                    elementHeight.androidHeight = 68;
                    elementHeight.iosHeight = 68;
                    break;
                case AdType.Interstitial:
                case AdType.Incentivized:
                    elementHeight.height = 68;
                    elementHeight.androidHeight = 48;
                    elementHeight.iosHeight = 48;
                    break;
            }
            return elementHeight;
        }

        protected virtual void SetupReorderableList(ReorderableList list, AdType adType)
        {
        }

        protected virtual void DrawSpecificSettings()
        {
        }

        private void DrawCommonSettigns()
        {
            GUILayout.BeginVertical("box");
            Utils.DrawPropertyField(_serializedSettings, _responseWaitTimeProp, GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();
        }

        private void DrawAppIds()
        {
            if (IsAppIdSupported)
            {
                GUILayout.BeginVertical("box");
                if (_settingsWindow.IsAndroid)
                {
                    _androidAppIdProp.stringValue = EditorGUILayout.TextField("Android App Id", _androidAppIdProp.stringValue);
                }
                if (_settingsWindow.IsIOS)
                {
                    _iosAppIdProp.stringValue = EditorGUILayout.TextField("iOS App Id", _iosAppIdProp.stringValue);
                }
                GUILayout.EndVertical();
            }
        }

        private void DrawAdInstanceLists()
        {
            for (int i = 0; i < _instanceBlocks.Count; i++)
            {
                AdInstanceBlockData blockData = _instanceBlocks[i];
                EditorGUILayout.Space(2);
                AnimBool foldAnimation = blockData._foldAnimation;

                //char collapsedSymbol = blockData._isCollapsed ? '\u25B7' : '\u25BD';
                char collapsedSymbol = blockData._isCollapsed ? '\u21A6' : '\u21A7';
                
                string buttonTitle = string.Format("{0}  {1}", collapsedSymbol, blockData._blockName);

                if (GUILayout.Button(buttonTitle, InstanceFoldoutButtonStyle))
                {
                    blockData._isCollapsed = !blockData._isCollapsed;
                    EditorPrefs.SetBool(GetInstanceBlockCollapsedKey(Name, blockData._adType), blockData._isCollapsed);
                }

                foldAnimation.target = !blockData._isCollapsed;
                if (EditorGUILayout.BeginFadeGroup(foldAnimation.faded))
                {
                    blockData._instances.DoLayoutList();
                }
                EditorGUILayout.EndFadeGroup();
            }
        }

        private void CreateAdInstanceBlock(string title, string propertyName, AdType adType, InstanceElementHeight elementHeight)
        {
            AdInstanceBlockData instanceBlock = new AdInstanceBlockData();
            instanceBlock._adType = adType;
            instanceBlock._blockName = title;
            instanceBlock._instanceProperty = _serializedSettings.FindProperty(propertyName);
            instanceBlock._isCollapsed = EditorPrefs.GetBool(GetInstanceBlockCollapsedKey(Name, adType), true);
            instanceBlock._foldAnimation = new AnimBool();
            instanceBlock._foldAnimation.valueChanged.AddListener(_settingsWindow.Repaint);
            instanceBlock._elementHeight = elementHeight;
            instanceBlock._instances = CreateList(_serializedSettings, instanceBlock);
            SetupReorderableList(instanceBlock._instances, adType);
            _instanceBlocks.Add(instanceBlock);
        }

        protected ReorderableList CreateList(SerializedObject serializedObj, AdInstanceBlockData instanceBlock)
        {
            ReorderableList list = new ReorderableList(serializedObj, instanceBlock._instanceProperty, false, true, true, true);
            list.headerHeight = 1;

            list.drawHeaderCallback = rect =>
            {
            };
            list.drawNoneElementCallback = (Rect rect) =>
            {
                list.elementHeight = CalculateElementHeight(instanceBlock);
                EditorGUI.LabelField(rect, "List is Empty");
            };
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                float elementWidth = rect.width;
                float width = Mathf.Clamp(elementWidth - 215, 180, 2800);

                rect.y += 5;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "Name");
                EditorGUI.PropertyField(
                    new Rect(rect.x + 80, rect.y, width, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("_name"),
                    GUIContent.none
                );

                float addX = Mathf.Clamp(elementWidth - 395, 0, 2800);
                EditorGUI.LabelField(new Rect(rect.x + 270 + addX, rect.y, 80, EditorGUIUtility.singleLineHeight), "Timeout");
                EditorGUI.PropertyField(
                    new Rect(rect.x + 325 + addX, rect.y, 70, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("_timeout"),
                    GUIContent.none
                );

                width = Mathf.Clamp(elementWidth - 80, 315, 2800);
                if (_settingsWindow.IsAndroid)
                {
                    rect.y += 20;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "Android Id");
                    EditorGUI.PropertyField(
                        new Rect(rect.x + 80, rect.y, width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("_androidId"),
                        GUIContent.none
                    );
                }

                if (_settingsWindow.IsIOS)
                {
                    rect.y += 20;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "iOS Id");
                    EditorGUI.PropertyField(
                        new Rect(rect.x + 80, rect.y, width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("_iosId"),
                        GUIContent.none
                    );
                }

                if (instanceBlock._adType == AdType.Banner && BannerTypes != null)
                {
                    rect.y += 20;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "Type");
                    SerializedProperty bannerTypeProp = element.FindPropertyRelative("_bannerType");
                    bannerTypeProp.intValue = EditorGUI.Popup(new Rect(rect.x + 80, rect.y, width, EditorGUIUtility.singleLineHeight),
                        bannerTypeProp.intValue, BannerTypes);
                }
            };
            list.onAddCallback = (ReorderableList l) =>
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);
                list.elementHeight = CalculateElementHeight(instanceBlock);
                var property = list.serializedProperty.GetArrayElementAtIndex(list.index);
                property.FindPropertyRelative("_timeout").floatValue = 90;
                if (list.index == 0)
                {
                    property.FindPropertyRelative("_name").stringValue = "Default";
                }
                else
                {
                    property.FindPropertyRelative("_name").stringValue = "";
                }
            };
            list.onRemoveCallback = (ReorderableList l) =>
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                list.elementHeight = CalculateElementHeight(instanceBlock);
            };

            return list;
        }
    }
} // namespace Virterix.AdMediation.Editor
