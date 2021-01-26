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
    public abstract class BaseAdNetworkSettingsView
    {
        public string Name
        {
            get; private set;
        }

        public bool Enabled
        {
            get; set;
        }

        public bool Collapsed
        {
            get; set;
        }

        protected abstract string SettingsFileName
        {
            get;
        }

        protected string SettingsFilePath
        {
            get
            {
                return String.Format("{0}/{1}", _settingsWindow.GetProjectFolderPath(_settingsWindow.CurrProjectName), SettingsFileName);
            }
        }

        private string CollapsedSaveKey
        {
            get { return string.Format("{0}{1}.collapsed", AdMediationSettingsWindow.PREFIX_SAVEKEY, Name); }
        }

        protected virtual string[] BannerTypes
        {
            get; set;
        }

        protected AdMediationSettingsWindow _settingsWindow;

        protected SerializedObject _serializedSettings;
        protected BaseAdNetworkSettings _settings;

        private AnimBool _showSettings;
        private UnityAction _repaint;
        private SerializedProperty _enabledProp;

        private List<ReorderableList> _adInstancesLists;
        private List<SerializedObject> _adInstancesProperties;

        private ReorderableList _banners;
        private SerializedProperty _bannerAdInstancesProp;
        private ReorderableList _interstitials;
        private SerializedProperty _interstitialAdInstancesProp;
        private ReorderableList _rewardAdUnits;
        private SerializedProperty _rewardAdInstancesProp;

        public virtual bool IsBannerListSupported
        {
            get { return false; }
        }

        public virtual bool IsInterstitialListSupported
        {
            get { return false; }
        }

        public virtual bool IsIncentivizedListSupported
        {
            get { return false; }
        }

        public BaseAdNetworkSettingsView(AdMediationSettingsWindow settingsWindow, string name, UnityAction repaint)
        {
            _settingsWindow = settingsWindow;
            Name = name;
            _showSettings = new AnimBool(true);
            _showSettings.valueChanged.AddListener(repaint);
            Collapsed = EditorPrefs.GetBool(CollapsedSaveKey, false);
            _adInstancesLists = new List<ReorderableList>();
            _adInstancesProperties = new List<SerializedObject>();

            _settings = CreateSettingsModel();
            _serializedSettings = new SerializedObject(_settings);

            if (IsBannerListSupported)
            {
                _bannerAdInstancesProp = _serializedSettings.FindProperty("_bannerAdInstances");
                _banners = CreateList(_serializedSettings, _bannerAdInstancesProp, "Banners", AdType.Banner);
                SetupReorderableList(_banners, AdType.Banner);
            }
            if (IsInterstitialListSupported)
            {
                _interstitialAdInstancesProp = _serializedSettings.FindProperty("_interstitialAdInstances"); 
                _interstitials = CreateList(_serializedSettings, _interstitialAdInstancesProp, "Interstitials", AdType.Interstitial);
                SetupReorderableList(_interstitials, AdType.Interstitial);
            }
            if (IsIncentivizedListSupported)
            {
                _rewardAdInstancesProp = _serializedSettings.FindProperty("_rewardAdInstances");
                _rewardAdUnits = CreateList(_serializedSettings, _rewardAdInstancesProp, "Reward Units", AdType.Incentivized);
                SetupReorderableList(_rewardAdUnits, AdType.Incentivized);
            }

            UpdateElementHeight();
            _enabledProp = _serializedSettings.FindProperty("_enabled");
            Enabled = _enabledProp.boolValue;
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
                DrawSettings();
                DrawAdInstanceLists();
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();

            return activationChanged;
        }

        public void UpdateElementHeight()
        {
            if (_banners != null)
            {
                _banners.elementHeight = CalculateElementHeight(AdType.Banner);
            }
            if (_interstitials != null)
            {
                _interstitials.elementHeight = CalculateElementHeight(AdType.Interstitial);
            }
            if (_rewardAdUnits != null)
            {
                _rewardAdUnits.elementHeight = CalculateElementHeight(AdType.Incentivized);
            }
        }

        private float CalculateElementHeight(AdType adType)
        {
            float resultHeight = 0;

            float commonElementHeight = 28;
            float bannerElementHeight = 48;
            if (_settingsWindow.IsAndroid && _settingsWindow.IsIOS)
            {
                bannerElementHeight = 88;
                commonElementHeight = 68;
            }
            else if (_settingsWindow.IsAndroid || _settingsWindow.IsIOS)
            {
                bannerElementHeight = 68f;
                commonElementHeight = 48;
            }

            if (adType == AdType.Banner) 
            {
                resultHeight = bannerElementHeight;
            }
            else
            {
                resultHeight = commonElementHeight;
            }

            return resultHeight;
        }

        protected virtual BaseAdNetworkSettings CreateSettingsModel()
        {
            return null;
        }

        protected virtual void SetupReorderableList(ReorderableList list, AdType adType)
        {
        }

        protected virtual void DrawSettings()
        {
        }

        private void DrawAdInstanceLists()
        {
            for(int i = 0; i < _adInstancesLists.Count; i++)
            {
                EditorGUILayout.Space();
                _adInstancesProperties[i].Update();
                _adInstancesLists[i].DoLayoutList();
                _serializedSettings.ApplyModifiedProperties();
            }
        }

        protected ReorderableList CreateList(SerializedObject serializedObj, SerializedProperty serializedProp, string title, AdType adType)
        {
            ReorderableList list = new ReorderableList(serializedObj, serializedProp, false, true, true, true);
            list.headerHeight = 22;
            list.elementHeight = list.count == 0 ? 22 : CalculateElementHeight(adType);

            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, title);
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

                if (adType == AdType.Banner && BannerTypes != null)
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
                list.elementHeight = list.count == 0 ? 22 : CalculateElementHeight(adType);
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
                list.elementHeight = list.count == 0 ? 22 : CalculateElementHeight(adType);
            };

            _adInstancesProperties.Add(serializedObj);
            _adInstancesLists.Add(list);
            return list;
        }
    }
} // namespace Virterix.AdMediation.Editor
