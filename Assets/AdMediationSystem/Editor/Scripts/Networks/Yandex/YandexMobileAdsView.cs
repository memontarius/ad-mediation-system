using System;
using UnityEditor;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class YandexMobileAdsView: BaseAdNetworkView
    {
        private SerializedProperty _useAppOpenAdProp;
        private SerializedProperty _androidAppOpenAdUnitIdProp;
        private SerializedProperty _iOSAppOpenAdUnitIdProp;

        private SerializedProperty _selfControlImpressionAppOpenAdProp;
        private SerializedProperty _appOpenAdShowingFrequencyProp;
        private SerializedProperty _appOpenAdDisplayCooldownProp;
        private SerializedProperty _appOpenAdLoadAttemptMaxNumberProp;
        
        protected override InstanceElementHeight CreateInstanceElementHeight(AdType adType)
        {
            var elementHeight = base.CreateInstanceElementHeight(adType);
            return elementHeight;
        }

        public YandexMobileAdsView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(YandexMobileAdsAdapter.YandexBannerSize));
            
            _useAppOpenAdProp = _serializedSettings.FindProperty("_useAppOpenAd");
            _iOSAppOpenAdUnitIdProp = _serializedSettings.FindProperty("_iOSAppOpenAdUnitId");
            _androidAppOpenAdUnitIdProp = _serializedSettings.FindProperty("_androidAppOpenAdUnitId");
            _selfControlImpressionAppOpenAdProp = _serializedSettings.FindProperty("_selfControlImpressionAppOpenAd");
            _appOpenAdShowingFrequencyProp = _serializedSettings.FindProperty("_appOpenAdShowingFrequency");
            _appOpenAdDisplayCooldownProp =  _serializedSettings.FindProperty("_appOpenAdDisplayCooldown");
            _appOpenAdLoadAttemptMaxNumberProp = _serializedSettings.FindProperty("_appOpenAdLoadAttemptMaxNumber");
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<YandexMobileAdsSettings>(SettingsFilePath);
            return settings;
        }

        protected override void DrawSpecificSettings(AdMediationProjectSettings projectSettings)
        {
            GUILayout.BeginVertical("box");
            _useAppOpenAdProp.boolValue = EditorGUILayout.Toggle("Use App Open Ad", _useAppOpenAdProp.boolValue);
            if (_useAppOpenAdProp.boolValue)
            {
                if (projectSettings.IsAndroid)
                {
                    EditorGUILayout.PropertyField(_androidAppOpenAdUnitIdProp, new GUIContent("Android Ad Unit ID"));
                }

                if (projectSettings.IsIOS)
                {
                    EditorGUILayout.PropertyField(_iOSAppOpenAdUnitIdProp, new GUIContent("iOS Ad Unit ID"));
                }

                EditorGUILayout.PropertyField(_selfControlImpressionAppOpenAdProp, new GUIContent("Self Control Impression"));
                if (_selfControlImpressionAppOpenAdProp.boolValue)
                {
                    EditorGUILayout.PropertyField(_appOpenAdShowingFrequencyProp, new GUIContent("Showing Frequency"));
                    EditorGUILayout.PropertyField(_appOpenAdDisplayCooldownProp, new GUIContent("Display Cooldown"));
                    EditorGUILayout.PropertyField(_appOpenAdLoadAttemptMaxNumberProp, new GUIContent("Load Attempt Max Number"));
                }
            }
            GUILayout.EndVertical();
        }

        protected override int GetBannerSizeDropdownRightPadding(int bannerType)
        {
            return bannerType == 1 ? 140 : 280;
        }
        
        protected override void DrawBannerSpecificSettings(Rect rect, SerializedProperty element, float elementWidth, int bannerType)
        {
            float addX = 0;
            if (bannerType is 0 or 2)
            {
                addX = Mathf.Clamp(elementWidth - 90, 0, 2800);
                EditorGUI.LabelField(new Rect(rect.x + addX - 175, rect.y, 80, EditorGUIUtility.singleLineHeight),
                    "Max Height");
                EditorGUI.PropertyField(
                    new Rect(rect.x + addX - 100, rect.y, 50, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("_bannerMaxHeight"),
                    GUIContent.none
                );
            }

            addX = Mathf.Clamp(elementWidth - 90, 0, 2800);
            EditorGUI.LabelField(new Rect(rect.x + addX - 40, rect.y, 80, EditorGUIUtility.singleLineHeight),
                "Refresh Time");
            EditorGUI.PropertyField(
                new Rect(rect.x + addX + 40, rect.y, 50, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("_bannerRefreshTime"),
                GUIContent.none
            );
        }
    }
}