using System.Collections;
using System.Collections.Generic;
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

        public virtual void Load()
        {
        }

        protected void NotifyOnSettingsReceived(LoadingState loadingState, JSONObject settings)
        {
            OnSettingsReceived(loadingState, settings);
        }
    }
} // namespace Virterix.AdMediation


