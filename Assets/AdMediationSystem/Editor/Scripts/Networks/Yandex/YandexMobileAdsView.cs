using System;
using UnityEditor;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class YandexMobileAdsView: BaseAdNetworkView
    {
        protected override InstanceElementHeight CreateInstanceElementHeight(AdType adType)
        {
            var elementHeight = base.CreateInstanceElementHeight(adType);
            return elementHeight;
        }

        public YandexMobileAdsView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(YandexMobileAdsAdapter.YandexBannerSize));
        }

        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<YandexMobileAdsSettings>(SettingsFilePath);
            return settings;
        }

        protected override void DrawSpecificSettings(AdMediationProjectSettings projectSettings)
        {
        }

        protected override int GetBannerSizeDropdownRightPadding(int bannerType)
        {
            return bannerType == 0 ? 280 : 140;
        }
        
        protected override void DrawBannerSpecificSettings(Rect rect, SerializedProperty element, float elementWidth, int bannerType)
        {
            float addX = 0;
            if (bannerType == 0)
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