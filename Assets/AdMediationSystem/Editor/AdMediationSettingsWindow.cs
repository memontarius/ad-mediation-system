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
        private Vector2 _scrollPositioin;
        private int _selectedTab;
        private List<AdNetworkSettingsViewBase> _networks = new List<AdNetworkSettingsViewBase>();

        AnimBool m_ShowExtraFields;
        string m_String;
        Color m_Color = Color.white;
        int m_Number = 0;

        private void OnEnable()
        {
            m_ShowExtraFields = new AnimBool(true);
            m_ShowExtraFields.valueChanged.AddListener(Repaint);

            
            Init();
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

        private void Init()
        {
            AdNetworkSettingsViewBase network = new AdNetworkAdMobSettingsView("AdMob", Repaint);
            _networks.Add(network);

            /*
            network = new AdNetworkBase("AudienceNetwork", Repaint);
            _networks.Add(network);

            network = new AdNetworkBase("UnityAds", Repaint);
            _networks.Add(network);

            network = new AdNetworkBase("Applovin", Repaint);
            _networks.Add(network);

            network = new AdNetworkBase("Chartboost", Repaint);
            _networks.Add(network);*/
        }

        private void Save()
        {
        }
        private void DrawTabs()
        {
            string[] tabs = { "SETTINGS", "BANNERS", "INTERSTITIALS", "REWARDED" };
            GUIStyle toolbarStyle = EditorStyles.toolbarButton;
            _selectedTab = GUILayout.Toolbar(_selectedTab, tabs, toolbarStyle);
        }

        private void DrawAdNetworks()
        {
            EditorGUILayout.Space();
            foreach (AdNetworkSettingsViewBase network in _networks)
            {
                network.DrawUI();
            }
        }
    }
} // namespace Virterix.AdMediation.Editor
#endif
