using System;
using UnityEngine;
using Boomlagoon.JSON;

namespace Virterix.AdMediation
{
    public class AdRemoteSettingsProvider : MonoBehaviour
    {
        public enum LoadingStatus
        {
            None,
            Failed,
            Success
        }

        public event Action<LoadingStatus, JSONObject> OnSettingsReceived;
        public virtual bool IsUpdatingRequired => true;
        public virtual bool IsSelfCached => true;
        
        public virtual void Request()
        {
        }

        protected void NotifyOnSettingsReceived(LoadingStatus loadingStatus, JSONObject settings)
        {
            OnSettingsReceived?.Invoke(loadingStatus, settings);
        }
    }
}


