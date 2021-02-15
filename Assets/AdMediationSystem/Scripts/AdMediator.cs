using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Virterix.AdMediation
{
    /// <summary>
    /// Handles specific a type advertising
    /// </summary>
    public class AdMediator : MonoBehaviour
    {
        private const string _PREFIX_LAST_UNITID_SAVE_KEY = "adm.last.unit.";

        //===============================================================================
        #region Properties
        //-------------------------------------------------------------------------------

        public IFetchStrategy FetchStrategy
        {
            set
            {
                m_fetchStrategy = value;
            }
            get
            {
                return m_fetchStrategy;
            }
        }
        IFetchStrategy m_fetchStrategy;

        public AdType m_adType;
        public string m_placementName = AdMediationSystem.PLACEMENT_DEFAULT_NAME;
        public bool m_isAutoFetchWhenHide;
        [Tooltip("If a banner type ad is displayed longer than set value, when ad hide then performs the fetch. (In Seconds)")]
        public float m_minDisplayTimeBannerAdType = 0f;
        [Tooltip("Is continue show ad after restart the app from the interrupt place.")]
        public bool m_isContinueAfterEndSession;
        [Tooltip("When all networks don't fill ad then the fetch will be performed automatically after the delay. " +
            "Negative value is disabled. (In Seconds)")]
        public float m_deferredFetchDelay = -1;

        public AdUnit CurrentUnit
        {
            get { return m_currUnit; }
        }

        public bool IsBannerDisplayed
        {
            get { return m_isBannerTypeAdViewDisplayed; }
        }

        public string CurrentNetworkName
        {
            get
            {
                if (CurrentUnit != null)
                {
                    return CurrentUnit.AdNetwork.m_networkName;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsReadyToShow
        {
            get
            {
                bool ready = false;
                for (int tierIndex = 0; tierIndex < m_tiers.Count; tierIndex++)
                {
                    AdUnit[] units = m_tiers[tierIndex];
                    for (int unitIndex = 0; unitIndex < units.Length; unitIndex++)
                    {
                        if (units[unitIndex].IsAdReady)
                        {
                            ready = true;
                            break;
                        }
                    }
                }
                return ready;
            }
        }

        public bool IsCurrentNetworkReadyToShow
        {
            get
            {
                bool ready = false;
                if (CurrentUnit != null)
                {
                    ready = CurrentUnit.IsAdReady;
                }
                return ready;
            }
        }

        public int UnitWithoutTimeoutCount
        {
            get
            {
                int count = 0;
                AdUnit[] units = null;
                for (int tierIndex = 0; tierIndex < m_tiers.Count; tierIndex++)
                {
                    units = m_tiers[tierIndex];
                    for (int unitIndex = 0; unitIndex < units.Length; unitIndex++)
                    {
                        AdUnit unit = units[unitIndex];
                        unit.TierIndex = tierIndex;
                        unit.Index = unitIndex;
                        count += unit.IsTimeout ? 0 : 1;
                    }
                }
                return count;
            }
        }

        public bool IsLastNetworkSuccessfullyPrepared
        {
            get { return m_isLastNetworkSuccessfullyPrepared; }
        }
        private bool m_isLastNetworkSuccessfullyPrepared;

        private string LastAdUnitIdSaveKey
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, 
                    "{0}{1}.{2}", _PREFIX_LAST_UNITID_SAVE_KEY, m_adType.ToString(), m_placementName).ToLower();
            }
        }

        #endregion // Properties

        private List<AdUnit[]> m_tiers;
        private int m_totalUnits;

        protected AdUnit m_currUnit;
        private int m_lastActiveTierId;
        private int m_lastActiveUnitId;
        private bool m_isBannerTypeAdViewDisplayed = false;
        private Coroutine m_coroutineWaitNetworkPreparation;
        private Coroutine m_coroutineDeferredFetch;
        private int m_adPreparationFailureCount;
        private int m_nonTimeoutUnitCountAtFirstFailedPreparation;

        //===============================================================================
        #region MonoBehavior Methods
        //-------------------------------------------------------------------------------

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (m_isContinueAfterEndSession)
                {
                    SaveLastActiveAdUnit();
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (m_isContinueAfterEndSession)
            {
                SaveLastActiveAdUnit();
            }
        }

        #endregion // MonoBehavior Methods

        //===============================================================================
        #region Methods
        //-------------------------------------------------------------------------------

        /// <summary>
        /// Should be called only once when initialize
        /// </summary>
        public void Initialize(List<AdUnit[]> tiers)
        {
            m_lastActiveUnitId = -1;
            m_tiers = tiers;
            for(int i = 0; i < m_tiers.Count; i++)
            {
                m_totalUnits += m_tiers[i].Length;
            }
            
            m_fetchStrategy.Init(tiers, m_totalUnits);
            if (m_isContinueAfterEndSession)
            {
                RestoreLastActiveAdUnit();
            }
        }

        public virtual void Fetch()
        {
            if (m_fetchStrategy == null)
            {
                Debug.LogWarning("AdMediator.Fetch() Not strategy of fetch! adType:" + m_adType);
                return;
            }

            if (m_coroutineDeferredFetch != null)
            {
                StopCoroutine(m_coroutineDeferredFetch);
                m_coroutineDeferredFetch = null;
            }

            AdUnit unit = m_fetchStrategy.Fetch(m_tiers);

            if (unit != null)
            {
                SetCurrentUnit(unit);

                if (CurrentUnit != null)
                {
                    CurrentUnit.ResetDisplayTime();
                    if ((m_adType == AdType.Banner) && m_isBannerTypeAdViewDisplayed)
                    {
                        CurrentUnit.ShowAd();
                    }
                }
            }

#if AD_MEDIATION_DEBUG_MODE
            if (unit == null)
            {
                Debug.Log("AdMediator.Fetch() Not fetched ad unit. Placement: " + m_placementName);
            }
#endif
        }

        public virtual void Show()
        {
            if (m_adType == AdType.Banner)
            {
                m_isBannerTypeAdViewDisplayed = true;
            }

            if (m_currUnit != null)
            {
                ShowAdUnit();
            }
            else
            {
                if (!ShowAnyReadyNetwork())
                {
                    Fetch();
                }

#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("AdMediator.Show() Not current unit");
#endif
            }
        }

        public virtual void Refresh()
        {
            if (m_currUnit != null)
            {
                m_currUnit.PrepareAd();
            }
        }

        private void ShowAdUnit()
        {
            if (m_currUnit != null)
            {
                bool isShowSuccessful = false;
                if (m_currUnit.AdapterAdType == AdType.Banner)
                {
                    isShowSuccessful = m_currUnit.ShowAd();
                }
                else
                {
                    isShowSuccessful = m_currUnit.IsAdReady ? m_currUnit.ShowAd() : false;
                }

                if (!isShowSuccessful)
                {
                    if (!ShowAnyReadyNetwork())
                    {
                        Fetch();
                    }
                }
            }
        }

        public virtual void Hide()
        {
            if (m_adType == AdType.Banner)
            {
                m_isBannerTypeAdViewDisplayed = false;
            }

            if (m_currUnit != null)
            {
                m_currUnit.HideAd();
            }
        }

        private bool ShowAnyReadyNetwork()
        {
            if (m_totalUnits == 0)
            {
                return false;
            }

            int currTierIndex = m_currUnit != null ? m_currUnit.TierIndex : 0;
            int currUnitIndex = m_currUnit != null ? m_currUnit.Index : 0;

            AdUnit readyUnit = null;
            AdUnit[] units = null;
            int readyTierIndex = 0;
            int readyUnitIndex = 0;

            for (int tierIndex = currTierIndex; ; tierIndex++)
            {
                if (tierIndex >= m_tiers.Count)
                {
                    tierIndex = 0;
                }

                units = m_tiers[tierIndex];
                bool isEnded = false;

                for (int unitIndex = currUnitIndex + 1; ; unitIndex++)
                {
                    if (unitIndex >= units.Length)
                    {
                        unitIndex = 0;
                    }

                    readyUnit = units[unitIndex];
                    if (readyUnit.IsAdReady)
                    {
                        readyTierIndex = tierIndex;
                        readyUnitIndex = unitIndex;
                        isEnded = true;
                        break;
                    }
                    else
                    {
                        readyUnit = null;
                    }

                    isEnded = tierIndex == currTierIndex && unitIndex == currUnitIndex;
                    if (isEnded)
                        break;
                }

                if (isEnded)
                {
                    Debug.Log("tierIndex: " + tierIndex + " isEnded: " + isEnded);
                    break;
                }
            }

            if (readyUnit != null)
            {
                SetCurrentUnit(readyUnit);
                m_fetchStrategy.Reset(m_tiers, readyTierIndex, readyUnitIndex);
                return readyUnit.ShowAd();
            }
            return false;
        }

        private void RequestToPrepare(AdUnit unit)
        {
            CancelWaitNetworkPreparing();
            float waitingTime = unit.NetworkResponseWaitTime;
            m_coroutineWaitNetworkPreparation = StartCoroutine(WaitingNetworkPreparation(unit, waitingTime));
            unit.PrepareAd();
        }

        private IEnumerator WaitingNetworkPreparation(AdUnit unit, float waitingTime)
        {
            float passedTime = 0.0f;
            float passedTimeForCheckAvailability = 0.0f;
            bool isCheckAvailabilityWhenPreparing = unit.AdNetwork.IsCheckAvailabilityWhenPreparing(unit.AdapterAdType);
            float interval = 0.4f;
            WaitForSeconds waitInstruction = new WaitForSeconds(interval);

            while (true)
            {
                yield return waitInstruction;
                passedTime += interval;
                passedTimeForCheckAvailability += interval;

                if (passedTime > waitingTime)
                {
                    unit.AdNetwork.NotifyEvent(unit.AdapterAdType, AdEvent.FailedPreparation, unit.AdInstance);
                    break;
                }
                else if (isCheckAvailabilityWhenPreparing && passedTimeForCheckAvailability > 2.0f)
                {
                    if (unit.IsAdReady)
                    {
                        unit.AdNetwork.NotifyEvent(unit.AdapterAdType, AdEvent.Prepare, unit.AdInstance);
                        break;
                    }
                }
            }
            yield return null;
        }

        private void StartDeferredFetch(float delay)
        {
            KillDefferedFetch();
            m_coroutineDeferredFetch = StartCoroutine(DeferredFetch(delay));
        }

        private void KillDefferedFetch()
        {
            if (m_coroutineDeferredFetch != null)
            {
                StopCoroutine(m_coroutineDeferredFetch);
                m_coroutineDeferredFetch = null;
            }
        }

        private IEnumerator DeferredFetch(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            m_coroutineDeferredFetch = null;
            Fetch();
            yield break;
        }

        private void CancelWaitNetworkPreparing()
        {
            if (m_coroutineWaitNetworkPreparation != null)
            {
                StopCoroutine(m_coroutineWaitNetworkPreparation);
                m_coroutineWaitNetworkPreparation = null;
            }
        }

        private void ResetCurrentUnit(AdUnit nextUnit)
        {
            if (m_currUnit != null)
            {
                m_currUnit.AdNetwork.OnEvent -= OnCurrentNetworkEvent;

                if (m_adType == AdType.Banner)
                {
                    m_currUnit?.HideBannerTypeAdWithoutNotify();
                }

                if (m_currUnit.IsPrepareOnExit)
                {
                    m_currUnit.PrepareAd();
                }

                m_currUnit = null;
            }
        }

        private void SetCurrentUnit(AdUnit unit)
        {
            unit?.ResetLastImpressionSuccessfulState();

            if (unit != m_currUnit)
            {
                ResetCurrentUnit(unit);
                m_currUnit = unit;

                if ((m_adType == AdType.Banner) && !m_isBannerTypeAdViewDisplayed)
                {
                    m_currUnit.HideBannerTypeAdWithoutNotify();
                }

                m_currUnit.AdNetwork.OnEvent += OnCurrentNetworkEvent;
            }

            m_currUnit.AdNetwork.NotifyEvent(m_currUnit.AdapterAdType, AdEvent.Select, m_currUnit.AdInstance);

            if (m_currUnit.IsAdReady)
            {
                m_currUnit.AdNetwork.NotifyEvent(m_currUnit.AdapterAdType, AdEvent.Prepare, m_currUnit.AdInstance);
            }
            else
            {
                RequestToPrepare(m_currUnit);
            }
        }

        private void SaveLastActiveAdUnit()
        {
            if (m_currUnit != null && m_lastActiveUnitId >= 0)
            {
                string savedData = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", m_lastActiveTierId, m_lastActiveUnitId);
                PlayerPrefs.SetString(LastAdUnitIdSaveKey, savedData);
            }
        }

        private void RestoreLastActiveAdUnit()
        {
            string savedData = PlayerPrefs.GetString(LastAdUnitIdSaveKey, "");
            if (!string.IsNullOrEmpty(savedData))
            {
                string[] savedValues = savedData.Split('-');
                if (savedValues.Length == 2)
                {
                    int tierIndex = Convert.ToInt32(savedValues[0]);
                    int unitIndex = Convert.ToInt32(savedValues[1]);
                    m_fetchStrategy.Reset(m_tiers, tierIndex, unitIndex);
                }
            }
        }

        private void OnCurrentNetworkEvent(AdNetworkAdapter network, AdType adType, AdEvent adEvent, AdInstanceData adInstance)
        {
            if (adType != m_currUnit.AdapterAdType)
            {
                return;
            }
            else if (adInstance != null && m_currUnit.AdInstance != adInstance)
            {
                return;
            }

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("AdMediator.OnNetworkEvent() Type:" + m_adType + " placementName: " + m_currUnit.PlacementName +
                "; Ad Instance Name:" + m_currUnit.AdInstanceName +
                "; Intrnl Type:" + m_currUnit.AdapterAdType + "; Network:" + network.m_networkName + "; Event:" + adEvent);
#endif

            string adInstanceName = adInstance != null ? adInstance.Name : "";
            
            AdMediationSystem.NotifyAdNetworkEvent(this, network, m_adType, adEvent, adInstanceName);
            
            if (adEvent == AdEvent.FailedPreparation || adEvent == AdEvent.Hide || adEvent == AdEvent.Show)
            {
                if (m_currUnit != null)
                {
                    m_lastActiveTierId = m_fetchStrategy.TierIndex;
                    m_lastActiveUnitId = m_fetchStrategy.UnitIndex;
                }
            }

            switch (adEvent)
            {
                case AdEvent.FailedPreparation:
                    m_isLastNetworkSuccessfullyPrepared = false;
                    CancelWaitNetworkPreparing();
                    adInstance?.SaveFailedLoadingTime();
                    if (m_adPreparationFailureCount == 0)
                    {
                        m_nonTimeoutUnitCountAtFirstFailedPreparation = UnitWithoutTimeoutCount;
                    }
                    
                    m_adPreparationFailureCount++;
                    if (m_adPreparationFailureCount > m_nonTimeoutUnitCountAtFirstFailedPreparation)
                    {
                        m_adPreparationFailureCount = 0;
                        if (m_deferredFetchDelay >= 0.0001f)
                        {
                            StartDeferredFetch(m_deferredFetchDelay);
                        }
                    }
                    else
                    {
                        StartDeferredFetch(0.2f);
                    }
                    break;
                case AdEvent.Prepare:
                    m_isLastNetworkSuccessfullyPrepared = true;
                    CancelWaitNetworkPreparing();
                    m_adPreparationFailureCount = 0;
                    break;
                case AdEvent.Hide:
                    AdUnit currAdUnit = m_currUnit;

                    if (m_isAutoFetchWhenHide)
                    {
                        bool isPerformFetch = true;

                        if (m_currUnit != null && m_minDisplayTimeBannerAdType > 0.1f)
                        {
                            isPerformFetch = m_currUnit.DisplayTime >= m_minDisplayTimeBannerAdType;
                            if (isPerformFetch)
                            {
                                m_currUnit.ResetDisplayTime();
                            }
                        }

                        if (isPerformFetch)
                        {
                            Fetch();
                        }
                    }
                    break;
            }
        }

    }

    #endregion // Methods

} // namespace Virterix.AdMediation
