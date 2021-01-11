using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Virterix.Common;

namespace Virterix.AdMediation
{
    public class BaseAdController : MonoBehaviour
    {
        public bool m_isTimeScaleControl;

        public int InterstitialCount
        {
            get; set;
        }

        private BaseAdController m_inheritor;
        protected float m_lastTimeScale;
        protected int m_interstitialCount;

        private void Awake()
        {
            m_inheritor = this;
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        protected virtual void SubscribeEvents()
        {
            AdMediationSystem.OnAdNetworkEvent += OnAdNetworkEvent;
        }

        protected virtual void UnsubscribeEvents()
        {
            AdMediationSystem.OnAdNetworkEvent -= OnAdNetworkEvent;
        }

        protected virtual void OnAdNetworkEvent(AdMediator mediator, AdNetworkAdapter network, AdType adType, AdEvent adEvent, string adInstanceName)
        {
            if (adType == AdType.Interstitial || adType == AdType.Incentivized)
            {
                switch (adEvent)
                {
                    case AdEvent.Show:
                        if (adType == AdType.Interstitial)
                        {
                            m_interstitialCount++;
                        }
#if UNITY_IOS
                            AudioListener.volume = 0.0f;
#endif
                        if (m_isTimeScaleControl)
                        {
                            m_lastTimeScale = Time.timeScale;
                        }

                        break;
                    case AdEvent.Hide:
#if UNITY_IOS
                            AudioListener.volume = 1.0f;
#endif
                        if (m_isTimeScaleControl)
                        {
                            Time.timeScale = m_lastTimeScale;
                        }
                        break;
                }
            }
        }

        public T GetAdController<T>() where T : BaseAdController
        {
            if (m_inheritor == null)
            {
                m_inheritor = FindObjectOfType<T>();
            }
            return m_inheritor as T;
        }

    }
} // namespace Virterix.AdMediation

