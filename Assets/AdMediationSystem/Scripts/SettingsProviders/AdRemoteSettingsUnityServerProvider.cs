//#define _AMS_USE_UNITY_REMOTE_CONFIG

using UnityEngine;
using Boomlagoon.JSON;
#if _AMS_USE_UNITY_REMOTE_CONFIG
using Unity.RemoteConfig;
#endif

namespace Virterix.AdMediation
{
    public class AdRemoteSettingsUnityServerProvider : AdRemoteSettingsProvider
    {
        [SerializeField] private bool m_autoFetching;
        [Tooltip("Will be add platform key (prefix + platform)")]
        [SerializeField] private string m_settingsPrefixKey;
        [Tooltip("If empty, default settings will be loaded")]
        [SerializeField] private string m_environmentID;
        
        public override bool IsSelfCached => true;
        
        public string UniqueUserId
        {
            get => m_uniqueUserId == "" ? SystemInfo.deviceUniqueIdentifier : m_uniqueUserId;
            set => m_uniqueUserId = value;
        }
        string m_uniqueUserId;
        
        string SettingsParamKey => 
            m_settingsPrefixKey + AdMediationSystem.Instance.PlatformName;

        public struct userAttributes
        {
        }

        public struct appAttributes
        {
        }
        
#if _AMS_USE_UNITY_REMOTE_CONFIG
        ConfigResponse m_lastConfigResponse;
        bool m_wasConfigResponse;

        private void Awake()
        {
            ConfigManager.FetchCompleted += ApplyRemoteSettings;
        }

        private void OnDestroy()
        {
            ConfigManager.FetchCompleted -= ApplyRemoteSettings;
        }

        public override void Request()
        {
            if (m_autoFetching)
            {
                if (m_wasConfigResponse)
                    ParseRemoteSettings(m_lastConfigResponse);
                else
                {
                    if (!string.IsNullOrEmpty(m_environmentID))
                        ConfigManager.SetEnvironmentID(m_environmentID);
                    ConfigManager.FetchConfigs(new userAttributes(), new appAttributes());
                }
            }
        }

        private void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            m_wasConfigResponse = true;
            m_lastConfigResponse = configResponse;
            ParseRemoteSettings(m_lastConfigResponse);
        }

        private void ParseRemoteSettings(ConfigResponse configResponse)
        {
            string settings = ConfigManager.appConfig.GetJson(SettingsParamKey, "");
            JSONObject jsonSettings = null;
            if (!string.IsNullOrEmpty(settings))
                jsonSettings = JSONObject.Parse(settings);
            LoadingStatus loadingStatus = jsonSettings != null ? LoadingStatus.Success : LoadingStatus.None;
            NotifyOnSettingsReceived(loadingStatus, jsonSettings);
        }
#endif
    }
}
