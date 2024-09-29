using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;

namespace Virterix.AdMediation
{
    /// <summary>
    /// Handles specific a type advertising
    /// </summary>
    public class AdMediator : MonoBehaviour
    {
        private const string _PREFIX_LAST_UNITED_SAVE_KEY = "adm.last.unit.";

        //===============================================================================

        #region Properties

        public IFetchStrategy FetchStrategy
        {
            set { m_fetchStrategy = value; }
            get { return m_fetchStrategy; }
        }

        IFetchStrategy m_fetchStrategy;

        public AdType m_adType;
        public string m_placementName = AdMediationSystem.PLACEMENT_DEFAULT_NAME;
        public bool m_fetchOnStart;
        public bool m_fetchOnAdUnitHidden;

        [Tooltip("Is continue show ad after restart the app from the interrupt place.")]
        public bool m_continueAfterEndSession;

        [Tooltip(
            "If a banner type ad is displayed longer than set value, when ad hide then performs the fetch. (In Seconds)")]
        public float m_bannerMinDisplayTime = 0f;

        [Tooltip("When all networks don't fill ad then the fetch will be performed automatically after the delay. " +
                 "Negative value is disabled. (In Seconds)")]
        public float m_deferredFetchDelay = -1;

        public AdUnit CurrentUnit => m_currUnit;

        public bool IsBannerDisplayed => m_isBannerDisplayed;

        public string CurrentNetworkName
        {
            get
            {
                if (CurrentUnit != null)
                    return CurrentUnit.AdNetwork.m_networkName;
                else
                    return null;
            }
        }

        public bool IsReadyToShow
        {
            get
            {
                bool ready = false;
                if (m_tiers == null)
                    return false;

                for (int tierIndex = 0; tierIndex < m_tiers.Length; tierIndex++)
                {
                    AdUnit[] units = m_tiers[tierIndex];
                    for (int unitIndex = 0; unitIndex < units.Length; unitIndex++)
                    {
                        try
                        {
                            if (units[unitIndex].IsReady)
                            {
                                ready = true;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[AMS] Exception at get Ad Unit Ready. Message: {e.Message}");
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
                    ready = CurrentUnit.IsReady;
                return ready;
            }
        }

        public int UnitNonTimeoutCount
        {
            get
            {
                int count = 0;
                AdUnit[] units = null;
                for (int tierIndex = 0; tierIndex < m_tiers.Length; tierIndex++)
                {
                    units = m_tiers[tierIndex];
                    for (int unitIndex = 0; unitIndex < units.Length; unitIndex++)
                    {
                        count += units[unitIndex].IsTimeout ? 0 : 1;
                    }
                }

                return count;
            }
        }

        public int TotalUnits => m_totalUnits;
        public bool WasLastNetworkPreparationSuccessfully => m_isLastNetworkSuccessfullyPrepared;
        private bool m_isLastNetworkSuccessfullyPrepared;

        private string LastAdUnitIdSaveKey
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "{0}{1}.{2}", _PREFIX_LAST_UNITED_SAVE_KEY, m_adType.ToString(), m_placementName).ToLower();
            }
        }

        private bool DeferredFetchEnabled => m_deferredFetchDelay > 0.01f;
        private bool DeferredFetchActive => m_deferredFetchCoroutine != null;
        private float DeferredFetchDelay => m_deferredFetchCallCount * m_deferredFetchDelay;

        #endregion // Properties

        private AdUnit[][] m_tiers;

        private int m_totalUnits;
        protected AdUnit m_currUnit;
        private int m_lastActiveTierId;
        private int m_lastActiveUnitId;
        private bool m_isBannerDisplayed = false;
        private Coroutine m_deferredFetchCoroutine;
        private int m_failedPreparationCount;
        private int m_nonTimeoutUnitCountSinceFirstFailed;
        private int m_deferredFetchCallCount;
        private List<AdNetworkAdapter> m_networks = new();

        //===============================================================================

        #region MonoBehavior Methods

        //-------------------------------------------------------------------------------
        private void OnDestroy()
        {
            for (int i = 0; i < m_networks.Count; i++)
            {
                AdNetworkAdapter network = m_networks[i];
                if (network != null)
                    network.OnEvent -= OnCurrentNetworkEvent;
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (m_continueAfterEndSession)
                    SaveLastActiveAdUnit();
            }
        }

        private void OnApplicationQuit()
        {
            if (m_continueAfterEndSession)
                SaveLastActiveAdUnit();
        }

        #endregion // MonoBehavior Methods

        //===============================================================================

        #region Methods

        //-------------------------------------------------------------------------------

        /// <summary>
        /// Should be called only once when initialize
        /// </summary>
        public void Initialize(AdUnit[][] tiers, int[] tierMaxPassages)
        {
            m_lastActiveUnitId = -1;
            m_deferredFetchCallCount = 1;
            m_tiers = tiers;

            for (int tierIndex = 0; tierIndex < m_tiers.Length; tierIndex++)
            {
                m_totalUnits += m_tiers[tierIndex].Length;
                for (int unitIndex = 0; unitIndex < m_tiers[tierIndex].Length; unitIndex++)
                {
                    AdUnit unit = m_tiers[tierIndex][unitIndex];
                    if (!m_networks.Contains(unit.AdNetwork))
                        m_networks.Add(unit.AdNetwork);
                }
            }

            foreach (var network in m_networks)
                network.OnEvent += OnCurrentNetworkEvent;

            m_fetchStrategy.Init(tiers, m_totalUnits, tierMaxPassages);
            if (m_continueAfterEndSession)
                RestoreLastActiveAdUnit();
        }

        public virtual void Fetch()
        {
            if (m_fetchStrategy == null)
            {
                Debug.LogWarning("[AMS] AdMediator.Fetch() Not strategy of fetch! adType:" + m_adType);
                return;
            }

            KillDeferredFetch();
            AdUnit unit = m_fetchStrategy.Fetch(m_tiers);
            
            if (unit != null)
            {
                unit.ResetDisplayTime();
                if ((m_adType == AdType.Banner) && m_isBannerDisplayed)
                {
                    unit.Show();
                }

                SetCurrentUnit(unit);
            }
            else
            {
                if (DeferredFetchEnabled && !DeferredFetchActive)
                {
                    StartDeferredFetch(DeferredFetchDelay, true);
                }
            }

#if AD_MEDIATION_DEBUG_MODE
            if (unit == null)
            {
                Debug.Log("[AMS] AdMediator.Fetch() Not fetched ad unit. Placement: " + m_placementName);
            }
#endif
        }

        public virtual void Show()
        {
            if (m_adType == AdType.Banner)
            {
                m_isBannerDisplayed = true;
            }

            if (m_currUnit != null)
            {
                bool wasShowSuccessfully = false;
                if (m_currUnit.AdType == AdType.Banner)
                {
                    wasShowSuccessfully = m_currUnit.Show();
                }
                else
                {
                    wasShowSuccessfully = m_currUnit.IsReady && m_currUnit.Show();
                }

                if (!wasShowSuccessfully && !ShowAnyReadyNetwork())
                {
                    Fetch();
                }
            }
            else
            {
                if (!ShowAnyReadyNetwork())
                {
                    Fetch();
                }

#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("[AMS] AdMediator.Show() Not current unit");
#endif
            }
        }

        public virtual void Refresh()
        {
            if (m_currUnit != null)
                m_currUnit.Prepare();
        }

        public virtual void Hide()
        {
            if (m_adType == AdType.Banner)
                m_isBannerDisplayed = false;

            if (m_currUnit != null)
                m_currUnit.Hide();
        }

        public AdUnit GetUnit(int index)
        {
            int unitPassedCount = 0;
            AdUnit foundUnit = null;
            for (int tierIndex = 0; tierIndex < m_tiers.Length; tierIndex++)
            {
                AdUnit[] tier = m_tiers[tierIndex];
                if (index < tier.Length + unitPassedCount)
                {
                    foundUnit = tier[index - unitPassedCount];
                    break;
                }
                else
                    unitPassedCount += tier.Length;
            }

            return foundUnit;
        }

        private bool ShowAnyReadyNetwork()
        {
            if (m_totalUnits == 0)
            {
                return false;
            }

            int currTierIndex = m_currUnit != null ? m_currUnit.TierIndex : 0;
            int currUnitIndex = m_currUnit != null ? m_currUnit.Index : 0;
            int startUnitIndex = currUnitIndex + 1;

            AdUnit readyUnit = null;
            int readyTierIndex = 0;
            int readyUnitIndex = 0;
            bool isFindNext = true;

            for (int tierIndex = currTierIndex; isFindNext; tierIndex++)
            {
                if (tierIndex >= m_tiers.Length)
                {
                    tierIndex = 0;
                }

                AdUnit[] units = m_tiers[tierIndex];

                for (int unitIndex = startUnitIndex; isFindNext && unitIndex < units.Length; unitIndex++)
                {
                    readyUnit = units[unitIndex];

                    if (readyUnit.IsReady)
                    {
                        readyTierIndex = tierIndex;
                        readyUnitIndex = unitIndex;
                        isFindNext = false;
                        break;
                    }

                    bool isCurrentUnit = tierIndex == currTierIndex && unitIndex == currUnitIndex;
                    if (isCurrentUnit)
                    {
                        isFindNext = false;
                        readyUnit = null;
                    }
                }

                startUnitIndex = 0;
            }

            if (readyUnit != null)
            {
                SetCurrentUnit(readyUnit);
                m_fetchStrategy.Reset(m_tiers, readyTierIndex, readyUnitIndex);
                return readyUnit.Show();
            }

            return false;
        }

        private void RequestPreparation(AdUnit unit)
        {
            unit.AdNetwork.StartWaitResponseHandling(unit.AdInstance);
            unit.Prepare();
        }

        private void StartDeferredFetch(float delay, bool increaseCallCounter = false)
        {
            if (increaseCallCounter)
            {
                m_deferredFetchCallCount++;
            }

            KillDeferredFetch();
            m_deferredFetchCoroutine = StartCoroutine(DeferredFetch(delay));
        }

        private void KillDeferredFetch()
        {
            if (m_deferredFetchCoroutine != null)
            {
                StopCoroutine(m_deferredFetchCoroutine);
                m_deferredFetchCoroutine = null;
            }
        }

        private IEnumerator DeferredFetch(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            m_deferredFetchCoroutine = null;
            Fetch();
        }

        private void ResetCurrentUnit(AdUnit nextUnit)
        {
            if (m_currUnit != null)
            {
                AdUnit currUnit = m_currUnit;
                m_currUnit = null;
                if (m_adType == AdType.Banner)
                    currUnit.Hide();

                if (currUnit.IsPrepareOnExit)
                    currUnit.Prepare();
            }
        }

        private void SetCurrentUnit(AdUnit unit)
        {
            unit?.ResetLastImpressionSuccessfulState();

            if (unit != m_currUnit)
            {
                bool isNetworkSame = IsUnitContainsSameNetwork(unit, m_currUnit);
                ResetCurrentUnit(unit);
                if (!isNetworkSame && m_adType == AdType.Banner && !m_isBannerDisplayed)
                {
                    unit.Hide();
                }

                m_currUnit = unit;
                m_currUnit?.SetupAdInstanceCurrentPlacement();
            }

            m_currUnit.AdNetwork.NotifyEvent(AdEvent.Selected, m_currUnit.AdInstance);

            if (m_currUnit.IsReady)
            {
                m_currUnit.AdNetwork.NotifyEvent(AdEvent.Prepared, m_currUnit.AdInstance);
            }
            else
            {
                RequestPreparation(m_currUnit);
            }
        }

        private void SaveLastActiveAdUnit()
        {
            if (m_currUnit != null && m_lastActiveUnitId >= 0)
            {
                string savedData = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", m_lastActiveTierId,
                    m_lastActiveUnitId);
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

        private bool SolveNeedingAdEventHandling(AdType adType, AdEvent adEvent, AdInstance adInstance)
        {
            if (m_currUnit == null)
                return false;

            if (m_adType == AdType.Incentivized &&
                (adEvent == AdEvent.IncentivizationCompleted || adEvent == AdEvent.IncentivizationUncompleted))
            {
                return true;
            }

            bool needEventHandling = adType == m_currUnit.AdType;
            if (needEventHandling)
            {
                needEventHandling = m_currUnit.AdInstance == adInstance &&
                                    (!string.IsNullOrEmpty(m_currUnit.AdInstance.CurrPlacement) &&
                                     m_currUnit.AdInstance.CurrPlacement == m_placementName);
            }

            return needEventHandling;
        }

        private void OnCurrentNetworkEvent(AdNetworkAdapter network, AdType adType, AdEvent adEvent,
            AdInstance adInstance)
        {
            if (!SolveNeedingAdEventHandling(adType, adEvent, adInstance))
            {
                return;
            }

#if AD_MEDIATION_DEBUG_MODE
            Debug.Log("[AMS] AdMediator.OnNetworkEvent() Type:" + m_adType + " placementName: " +
                      m_currUnit.PlacementName +
                      "; Ad Instance Name:" + m_currUnit.AdInstanceName +
                      "; Intrnl Type:" + m_currUnit.AdType + "; Network:" + network.m_networkName + "; Event:" +
                      adEvent);
#endif

            string adInstanceName = adInstance != null ? adInstance.Name : "";
            AdMediationSystem.NotifyAdNetworkEvent(this, network, m_adType, adEvent, adInstanceName);

            if (adEvent == AdEvent.PreparationFailed || adEvent == AdEvent.Hiding || adEvent == AdEvent.Showing)
            {
                if (m_currUnit != null)
                {
                    m_lastActiveTierId = m_fetchStrategy.TierIndex;
                    m_lastActiveUnitId = m_fetchStrategy.UnitIndex;
                }
            }

            switch (adEvent)
            {
                case AdEvent.PreparationFailed:
                    m_isLastNetworkSuccessfullyPrepared = false;
                    if (m_failedPreparationCount == 0)
                    {
                        m_nonTimeoutUnitCountSinceFirstFailed = UnitNonTimeoutCount;
                    }

                    m_failedPreparationCount++;
                    if (m_failedPreparationCount > m_nonTimeoutUnitCountSinceFirstFailed)
                    {
                        m_failedPreparationCount = 0;
                        if (DeferredFetchEnabled && !DeferredFetchActive)
                        {
                            StartDeferredFetch(DeferredFetchDelay, true);
                        }
                    }
                    else
                    {
                        StartDeferredFetch(0.3f);
                    }

                    break;
                case AdEvent.Prepared:
                    m_isLastNetworkSuccessfullyPrepared = true;
                    m_failedPreparationCount = 0;
                    m_deferredFetchCallCount = 1;
                    break;
                case AdEvent.Hiding:
                    if (m_fetchOnAdUnitHidden)
                    {
                        bool isPerformFetch = true;

                        if (m_currUnit != null && m_bannerMinDisplayTime > 0.1f)
                        {
                            isPerformFetch = m_currUnit.DisplayTime >= m_bannerMinDisplayTime;
                            if (isPerformFetch)
                            {
                                m_currUnit.ResetDisplayTime();
                            }
                        }

                        if (isPerformFetch)
                        {
                            StartDeferredFetch(0.3f);
                        }
                    }

                    break;
            }
        }

        private static bool IsUnitContainsSameNetwork(AdUnit unit, AdUnit otherUnit)
        {
            if (unit != null && otherUnit != null)
                return unit.AdNetwork == otherUnit.AdNetwork;
            return false;
        }
    }

    #endregion
}