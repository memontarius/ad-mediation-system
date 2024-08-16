using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public class AppodealView : BaseAdNetworkView
    {
        private SerializedProperty _requestedAdsTypeProp;
        
        private string[] _requestedAdsTypeNames;
        
        public AppodealView(AdMediationSettingsWindow settingsWindow, string name, string identifier) :
            base(settingsWindow, name, identifier)
        {
            BannerTypes = Enum.GetNames(typeof(AppodealAdapter.AppodealBannerSize));
            
            _requestedAdsTypeProp = _serializedSettings.FindProperty("_requestedAdsTypes");
            
            var requestedAdsTypes = Enum.GetValues(typeof(AppodealAdapter.RequestedAdsType)) as AppodealAdapter.RequestedAdsType[]; 
            _requestedAdsTypeNames = requestedAdsTypes.Select(t => t.ToString()).ToArray();
        }

        protected override bool IsAdInstanceIdsDisplayed(AdType adType) => false;
        
        protected override BaseAdNetworkSettings CreateSettingsModel()
        {
            var settings = Utils.GetOrCreateSettings<AppodealSettings>(SettingsFilePath);
            return settings;
        }
        
        protected override InstanceElementHeight CreateInstanceElementHeight(AdType adType)
        {
            var elementHeight = base.CreateInstanceElementHeight(adType);
            return elementHeight;
        }

        protected override void DrawSpecificSettings(AdMediationProjectSettings projectSettings)
        {
            GUILayout.BeginVertical("box");
            
            _requestedAdsTypeProp.intValue = EditorGUILayout.MaskField("Requested Ad Types", _requestedAdsTypeProp.intValue, _requestedAdsTypeNames);
            EditorGUILayout.Space();
            
            GUILayout.EndVertical();
        }

        /*
        protected override void DrawBannerSpecificSettings(Rect rect, SerializedProperty element, float elementWidth, int bannerType)
        {
        }*/
    }
}