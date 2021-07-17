using UnityEngine;

namespace Virterix.AdMediation {
    public class BaseAdController : MonoBehaviour {
        public bool m_isTimeScaleControl;

        public int InterstitialCount {
            get; set;
        }

        private BaseAdController m_inheritor;
        protected float m_lastTimeScale;
        protected int m_interstitialCount;

        private void OnEnable() {
            SubscribeEvents();
        }

        private void OnDisable() {
            UnsubscribeEvents();
        }

        protected virtual void SubscribeEvents() {
            AdMediationSystem.OnAdNetworkEvent += OnAdNetworkEvent;
            AdMediationSystem.OnInitialized += OnAdMediationSystemInitialized;
        }

        protected virtual void UnsubscribeEvents() {
            AdMediationSystem.OnAdNetworkEvent -= OnAdNetworkEvent;
            AdMediationSystem.OnInitialized -= OnAdMediationSystemInitialized;
        }

        public T GetAdController<T>() where T : BaseAdController {
            if (m_inheritor == null) {
                m_inheritor = FindObjectOfType<T>();
            }
            return m_inheritor as T;
        }

        private void OnAdNetworkEvent(AdMediator mediator, AdNetworkAdapter network, AdType adType, AdEvent adEvent, string adInstanceName) {
            if (adType == AdType.Interstitial || adType == AdType.Incentivized) {
                switch (adEvent) {
                    case AdEvent.Show:
                        if (adType == AdType.Interstitial) {
                            m_interstitialCount++;
                        }
#if UNITY_IOS
                            AudioListener.volume = 0.0f;
#endif
                        if (m_isTimeScaleControl)
                            m_lastTimeScale = Time.timeScale;
                        break;
                    case AdEvent.Hiding:
#if UNITY_IOS
                            AudioListener.volume = 1.0f;
#endif
                        if (m_isTimeScaleControl)
                            Time.timeScale = m_lastTimeScale;
                        break;
                }
            }
            HandleNetworkEvent(mediator, network, adType, adEvent, adInstanceName);
        }

        protected virtual void HandleNetworkEvent(AdMediator mediator, AdNetworkAdapter network, AdType adType, AdEvent adEvent, string adInstanceName) {
        }

        private void OnAdMediationSystemInitialized() {

        }
    }
} // namespace Virterix.AdMediation

