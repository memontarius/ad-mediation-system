using System;
using UnityEngine;

namespace Virterix.AdMediation 
{
    public class BaseAdController : MonoBehaviour 
    {
        public int InterstitialCount { get; protected set; }
        private BaseAdController _inheritor;

        private void OnEnable() 
        {
            SubscribeEvents();
        }

        private void OnDisable() 
        {
            UnsubscribeEvents();
        }
        
        protected virtual bool GetUserInteractsWithApp()
        {
            bool userInteractsWithApp = false;
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Touchscreen.current != null)
                userInteractsWithApp = UnityEngine.InputSystem.Touchscreen.current.touches.Count > 0;
#elif ENABLE_LEGACY_INPUT_MANAGER
            userInteractsWithApp = Input.touches.Length > 0;
#endif
            return userInteractsWithApp;
        }
        
        protected virtual void SubscribeEvents()
        {
            AdMediationSystem.OnAdNetworkEvent += OnAdNetworkEvent;
            AdMediationSystem.OnInitialized += OnAdMediationSystemInitialized;
        }

        protected virtual void UnsubscribeEvents() 
        {
            AdMediationSystem.OnAdNetworkEvent -= OnAdNetworkEvent;
            AdMediationSystem.OnInitialized -= OnAdMediationSystemInitialized;
        }

        public T GetAdController<T>() where T : BaseAdController 
        {
            if (_inheritor == null)
                _inheritor = FindObjectOfType<T>();
            return _inheritor as T;
        }
        
        private void OnAdMediationSystemInitialized()
        {
            HandleAdMediationSystemInitializedEvent();
        }
        
        private void OnAdNetworkEvent(AdMediator mediator, AdNetworkAdapter network, AdType adType, AdEvent adEvent, string adInstanceName)
        {
            if (AdMediationSystem.IsAdFullscreen(adType)) 
            {
                switch (adEvent) 
                {
                    case AdEvent.Showing:
                        if (adType == AdType.Interstitial)
                            InterstitialCount++;
                        HandleFullscreenAdShowingEvent();
                        break;
                    case AdEvent.Hiding:
                        HandleFullscreenAdHidingEvent();
                        break;
                }
            }
            HandleNetworkEvent(mediator, network, adType, adEvent, adInstanceName);
        }

        protected virtual void HandleAdMediationSystemInitializedEvent() 
        {
        }
        
        protected virtual void HandleNetworkEvent(AdMediator mediator, AdNetworkAdapter network, AdType adType, AdEvent adEvent, string adInstanceName) 
        {
        }

        protected virtual void HandleFullscreenAdShowingEvent() 
        {
#if UNITY_IOS
            AudioListener.volume = 0.0f;
#endif
        }
        
        protected virtual void HandleFullscreenAdHidingEvent() 
        {
#if UNITY_IOS
            AudioListener.volume = 1.0f;
#endif
        }
    }
}

