#define _AMS_USE_UNITY_REMOTE_CONFIG

using UnityEngine;
using Boomlagoon.JSON;
#if _AMS_USE_UNITY_REMOTE_CONFIG
using Unity.RemoteConfig;
#endif

namespace Virterix.AdMediation
{
    public class AdRemoteSettingsUnityServerProvider : AdRemoteSettingsProvider
    {
        public bool m_autoFetching;
        [Tooltip("Will be add platform key (prefix + platform)")]
        public string m_settingsPrefixKey;

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

        string m_assignmentId;

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
                {
                    ParseRemoteSettings(m_lastConfigResponse);
                }
                else
                {
                    ConfigManager.SetCustomUserID(UniqueUserId);
                    // Fetch configuration setting from the remote service: 
                    ConfigManager.FetchConfigs(new userAttributes(), new appAttributes());
                }
            }
        }

        private void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            m_wasConfigResponse = true;
            m_lastConfigResponse = configResponse;

            // Conditionally update settings, depending on the response's origin:
            switch (configResponse.requestOrigin)
            {
                case ConfigOrigin.Default:
                    break;
                case ConfigOrigin.Cached:
                    break;
                case ConfigOrigin.Remote:
                    m_assignmentId = ConfigManager.appConfig.assignmentId;
                    break;
            }

            ParseRemoteSettings(m_lastConfigResponse);
        }

        private void ParseRemoteSettings(ConfigResponse configResponse)
        {
            string settings = ConfigManager.appConfig.GetString(SettingsParamKey);
            JSONObject jsonSettings = null;

            if (settings != "")
            {
                jsonSettings = JSONObject.Parse(settings);
            }

            LoadingState loadingState = LoadingState.Failed;
            switch (configResponse.requestOrigin)
            {
                case ConfigOrigin.Default:
                    loadingState = LoadingState.UnmodifiedLoaded;
                    break;
                case ConfigOrigin.Cached:
                    loadingState = LoadingState.UnmodifiedLoaded;
                    break;
                case ConfigOrigin.Remote:
                    loadingState = LoadingState.RemoteLoaded;
                    break;
            }

            NotifyOnSettingsReceived(loadingState, jsonSettings);
        }
#endif
    }
}
