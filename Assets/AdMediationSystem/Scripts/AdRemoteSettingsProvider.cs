using System;
using UnityEngine;
using Boomlagoon.JSON;

namespace Virterix.AdMediation
{
    public class AdRemoteSettingsProvider : MonoBehaviour
    {
        public enum LoadingState
        {
            Failed,
            UnmodifiedLoaded,
            CacheLoaded,
            RemoteLoaded
        }

        public event Action<LoadingState, JSONObject> OnSettingsReceived;

        public virtual bool IsUpdateRequired => true;

        public virtual void Request()
        {
        }

        protected void NotifyOnSettingsReceived(LoadingState loadingState, JSONObject settings)
        {
            OnSettingsReceived?.Invoke(loadingState, settings);
        }
    }
}


