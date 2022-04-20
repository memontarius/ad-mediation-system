using UnityEngine;
using Virterix.Common;
using UnityEngine.Networking;
using Boomlagoon.JSON;

namespace Virterix.AdMediation
{
    public class AdRemoteSettingsCustomServerProvider : AdRemoteSettingsProvider
    {
        public string m_apiVersion = "1.0";
        public string m_apiUrl = "";
        public int m_periodUpdateInHours = 24;

        private const string _UPDATE_DATA_SAVE_KEY = "adm.settings.update.date";
        private const float _LOAD_SETTINGS_WAITING_TIME = 30.0f;

        private TimeSnapshot LastUpdateData
        {
            get
            {
                if (m_lastUpdateData != null)
                {
                    m_lastUpdateData = new TimeSnapshot(_UPDATE_DATA_SAVE_KEY, m_periodUpdateInHours, TimeSnapshot.PeriodType.Hours);
                }
                return m_lastUpdateData;
            }
        }
        private TimeSnapshot m_lastUpdateData;

        public override bool IsUpdatingRequired => LastUpdateData.IsPeriodOver;

        public override void Request()
        {
            if (IsRequiredCheckUpdateSettingsFile())
            {
                StartLoadSettingsFromServer();
            }
            else
            {
                NotifyOnSettingsReceived(LoadingState.UnmodifiedLoaded, null);
            }
        }

        private bool IsRequiredCheckUpdateSettingsFile()
        {
            bool isRequiredCheckUpdate = LastUpdateData.IsPeriodOver || !LastUpdateData.WasSaved;

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AdMediationSystem.IsRequiredCheckUpdateSettingsFile() Elapsed hours:" + LastUpdateData.PassedHoursSinceLastSave +
                " requiredHours:" + LastUpdateData.m_period);
#endif

            return isRequiredCheckUpdate;
        }

        private string GetCustomizationRequestUrl(string methodName)
        {
            string requestUrl = m_apiUrl + "customization." + methodName + "?" +
                "platform=" + AdMediationSystem.Instance.PlatformName +
                "&project=" + AdMediationSystem.Instance.ProjectName +
                "&v=" + m_apiVersion;
            return requestUrl;
        }
 
        private void SaveCheckUpdateDateTimeSettings()
        {
            LastUpdateData.Save();
        }

        private void StartLoadSettingsFromServer()
        {
            string requestUrl = GetCustomizationRequestUrl("get");

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AdMediationSystem.StartLoadSettingsFromServer() Request url:" + requestUrl);
#endif
            RemoteLoader.Load(requestUrl,
                _LOAD_SETTINGS_WAITING_TIME,
                RemoteLoader.CheckMode.EveryFrame,
                RemoteLoader.DestroyMode.DestroyObject,
                OnLoadingCompleteSettingsFromServer);
        }

        private void OnLoadingCompleteSettingsFromServer(bool success, UnityWebRequest www)
        {
            JSONObject remoteJsonSettings = null;
            if (success)
            {
                string receivedContent = www.downloadHandler.text.Trim();
                remoteJsonSettings = JSONObject.Parse(receivedContent);

                if (remoteJsonSettings != null)
                {
                    SaveCheckUpdateDateTimeSettings();
                }

#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("AdMediationSystem.OnLoadingCompleteSettingsFromServer() Loading remote settings done. Success:" + (remoteJsonSettings != null).ToString());
#endif
            }
            else
            {
                Debug.Log("AdMediationSystem.OnLoadingCompleteSettingsFromServer() Not response from server. Error: " + www.error);
            }

            LoadingState state = success ? LoadingState.RemoteLoaded : LoadingState.Failed;
            NotifyOnSettingsReceived(state, remoteJsonSettings);
        }
    }
}


