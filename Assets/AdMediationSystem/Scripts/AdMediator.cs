using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.JSON;

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
        public string m_placementName = AdMediationSystem._PLACEMENT_DEFAULT_NAME;
        public bool m_isAutoFetchWhenHide;
        [Tooltip("If banner type ad is displayed longer than set value when ad hide, then performs Fetch. (In Seconds)")]
        public float m_minDisplayTimeBannerAdType = 0f;
        [Tooltip("Is continue show ad after restart the app from the interrupt place.")]
        public bool m_isContinueAfterEndSession;
        [Tooltip("When all networks don't fill ad, then Fetch will be performed automatically after the delay. " +
            "Negative value is disabled. (In Seconds)")]
        public float m_deferredFetchDelay = -1;

        public List<AdUnit> FetchUnits
        {
            get { return m_fetchUnits; }
        }

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
                foreach (AdUnit unit in m_fetchUnits)
                {
                    if (unit.IsAdReady)
                    {
                        ready = true;
                        break;
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

        public bool IsLastNetworkSuccessfullyPrepared
        {
            get { return m_isLastNetworkSuccessfullyPrepared; }
        }
        private bool m_isLastNetworkSuccessfullyPrepared;

        private string LastAdUnitIdSaveKey
        {
            get
            {
                return string.Format("{0}{1}.{2}", _PREFIX_LAST_UNITID_SAVE_KEY, m_adType.ToString(), m_placementName).ToLower();
            }
        }

        #endregion // Properties

        private AdUnit[] m_units;
        private List<AdUnit> m_fetchUnits = new List<AdUnit>();

        private List<AdUnit[]> m_tiers;
        private int m_totalUnits;

        protected AdUnit m_currUnit;
        private int m_lastActiveUnitId;
        private bool m_isBannerTypeAdViewDisplayed = false;
        private Coroutine m_coroutineWaitNetworkPrepare;
        private Coroutine m_coroutineDeferredFetch;
       

        //===============================================================================
        #region MonoBehavior Methods
        //-------------------------------------------------------------------------------

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveLastActiveAdUnit();
            }
        }

        private void OnApplicationQuit()
        {
            SaveLastActiveAdUnit();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion // MonoBehavior Methods

        //===============================================================================
        #region Methods
        //-------------------------------------------------------------------------------

        /// <summary>
        /// Should be called only once when initialize
        /// </summary>
        public void Initialize(AdUnit[] units, List<AdUnit[]> tiers)
        {
            m_tiers = tiers;

            for(int i = 0; i < m_tiers.Count; i++)
            {
                m_totalUnits += m_tiers[i].Length;
            }

            m_units = units;
            m_lastActiveUnitId = -1;
            int index = 0;

            foreach (AdUnit unit in m_units)
            {
                unit.OnEnable += OnUnitEnable;
                unit.OnDisable += OnUnitDisable;
                unit.Index = index++;
            }
            FillFetchUnits(true);

            if (m_isContinueAfterEndSession)
            {
                m_lastActiveUnitId = PlayerPrefs.GetInt(LastAdUnitIdSaveKey, -1);
                if (m_lastActiveUnitId != -1 && m_lastActiveUnitId < m_units.Length)
                {
                    m_fetchStrategy.Reset(this, m_units[m_lastActiveUnitId]);
                }
            }
        }

        public virtual void Fetch()
        {
            if (m_fetchStrategy == null)
            {
                Debug.LogWarning("AdMediator.Fetch() Not strategy of fetch! adType:" + m_adType);
                return;
            }

            if (FetchUnits.Count == 0)
            {
                FillFetchUnits(true);
            }

            if (m_coroutineDeferredFetch != null)
            {
                StopCoroutine(m_coroutineDeferredFetch);
                m_coroutineDeferredFetch = null;
            }

            //AdUnit unit = FetchUnits.Count == 0 ? null : m_fetchStrategy.Fetch(this, FetchUnits.ToArray());
            AdUnit unit = m_fetchStrategy.Fetch(m_tiers, m_totalUnits);

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
            else
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("AdMediator.Fetch() Not fetched ad unit. Placement: " + m_placementName + " Unit count:" + m_fetchUnits.Count);
#endif
            }
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

        public int FindIndexInFetchUnits(AdUnit unit)
        {
            int currIndex = 0;
            for (int i = 0; i < m_fetchUnits.Count; i++)
            {
                if (m_fetchUnits[i] == unit)
                {
                    currIndex = i;
                    break;
                }
            }
            return currIndex;
        }

        private bool ShowAnyReadyNetwork()
        {
            if (m_fetchUnits.Count == 0)
            {
                return false;
            }

            AdUnit readyUnit = null;
            int currIndex = m_currUnit != null ? FindIndexInFetchUnits(m_currUnit) : 0;

            int startIndex = currIndex + 1;
            startIndex = startIndex >= m_fetchUnits.Count ? 0 : startIndex;

            for (int i = startIndex; i != currIndex;)
            {
                AdUnit unit = m_fetchUnits[i];
                if (unit.IsAdReady)
                {
                    readyUnit = unit;
                    break;
                }

                i++;
                if (i >= m_fetchUnits.Count)
                {
                    i = 0;
                }
            }

            if (readyUnit != null)
            {
                SetCurrentUnit(readyUnit);
                m_fetchStrategy.Reset(this, readyUnit);
                return readyUnit.ShowAd();
            }

            return false;
        }

        /// <summary>
        /// Call with ignore auto fill from a fetch strategy. 
        /// Call with check auto fill from the mediator to keep an actual data in a fetch strategy.
        /// </summary>
        public void FillFetchUnits(bool ignoreAutoFillFlag = false)
        {
            if (m_fetchUnits.Count == 0)
            {
                m_fetchStrategy.Reset(null, null);
            }

            if (ignoreAutoFillFlag || m_fetchStrategy.IsAllowAutoFillUnits())
            {
                if (m_units != null && m_fetchUnits.Count != m_units.Length)
                {
                    m_fetchUnits.Clear();
                    foreach (AdUnit unit in m_units)
                    {
                        if (unit.IsEnabled && !unit.AdNetwork.IsTimeout(unit.AdapterAdType, unit.AdInstance))
                        {
                            AddUnitToFetch(unit);
                        }
                    }
                }
            }
        }

        private void AddUnitToFetch(AdUnit unit)
        {
            unit.IsContainedInFetch = true;
            m_fetchUnits.Add(unit);
        }

        private void RemoveUnitFromFetch(AdUnit unit)
        {
            if (m_adType == AdType.Banner)
            {
                unit.HideBannerTypeAdWithoutNotify();
            }
            unit.IsContainedInFetch = false;
            m_fetchUnits.Remove(unit);
        }

        private void RequestToPrepare(AdUnit unit)
        {
            CancelWaitNetworkPrepare();
            float waitingTime = unit.FetchStrategyParams.m_waitingResponseTime;
            m_coroutineWaitNetworkPrepare = StartCoroutine(WaitingNetworkPrepare(unit, waitingTime));
            unit.PrepareAd();
        }

        private IEnumerator WaitingNetworkPrepare(AdUnit unit, float waitingTime)
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
                    unit.AdNetwork.NotifyEvent(unit.AdapterAdType, AdEvent.PrepareFailure, unit.AdInstance);
                    break;
                }
                else if (isCheckAvailabilityWhenPreparing && passedTimeForCheckAvailability > 2.0f)
                {
                    if (unit.IsAdReady)
                    {
                        unit.AdNetwork.NotifyEvent(unit.AdapterAdType, AdEvent.Prepared, unit.AdInstance);
                        break;
                    }
                }
            }
            yield return null;
        }

        private IEnumerator DeferredFetch(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            m_coroutineDeferredFetch = null;
            Fetch();
            yield return null;
        }

        private void CancelWaitNetworkPrepare()
        {
            if (m_coroutineWaitNetworkPrepare != null)
            {
                StopCoroutine(m_coroutineWaitNetworkPrepare);
                m_coroutineWaitNetworkPrepare = null;
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

                if ((m_currUnit.IsPepareWhenChangeNetwork == null && m_currUnit.AdNetwork.IsPrepareWhenChangeNetwork(m_currUnit.AdapterAdType)) ||
                    (m_currUnit.IsPepareWhenChangeNetwork != null && m_currUnit.IsPepareWhenChangeNetwork.Value))
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

#if AD_MEDIATION_DEBUG_MODE
                Debug.Log("AdMediator.SetCurrentUnit() Type:" + m_adType + " Placement:" + m_currUnit.PlacementName +
                    " AdInstance:" + m_currUnit.AdInstance +
                    " Network:" + unit.AdNetwork.m_networkName + " IsReady: " + unit.IsAdReady);
#endif

                if ((m_adType == AdType.Banner) && !m_isBannerTypeAdViewDisplayed)
                {
                    m_currUnit.HideBannerTypeAdWithoutNotify();
                }

                m_currUnit.AdNetwork.OnEvent += OnCurrentNetworkEvent;
            }

            m_currUnit.AdNetwork.NotifyEvent(m_currUnit.AdapterAdType, AdEvent.Selected, m_currUnit.AdInstance);

            if (m_currUnit.IsAdReady)
            {
                m_currUnit.AdNetwork.NotifyEvent(m_currUnit.AdapterAdType, AdEvent.Prepared, m_currUnit.AdInstance);
            }
            else
            {
                RequestToPrepare(m_currUnit);
            }
        }

        private void SaveLastActiveAdUnit()
        {
            if (m_isContinueAfterEndSession)
            {
                if (m_currUnit != null && m_lastActiveUnitId >= 0)
                {
                    PlayerPrefs.SetInt(LastAdUnitIdSaveKey, m_lastActiveUnitId);
                }
            }
        }

        private void OnUnitEnable(AdUnit enabledUnit)
        {
            AddUnitToFetch(enabledUnit);
        }

        private void OnUnitDisable(AdUnit disabledUnit)
        {
            RemoveUnitFromFetch(disabledUnit);
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

            string adInstanceName = adInstance != null ? adInstance.Name : AdInstanceData._AD_INSTANCE_DEFAULT_NAME;
            
            AdMediationSystem.NotifyAdNetworkEvent(this, network, m_adType, adEvent, adInstanceName);

            if (adEvent == AdEvent.PrepareFailure || adEvent == AdEvent.Hide || adEvent == AdEvent.Show)
            {
                if (m_currUnit != null)
                {
                    m_lastActiveUnitId = m_currUnit.Index;
                }
            }

            switch (adEvent)
            {
                case AdEvent.PrepareFailure:
                    m_isLastNetworkSuccessfullyPrepared = false;
                    CancelWaitNetworkPrepare();
                    network.SaveFailedLoadingTime(m_currUnit.AdapterAdType, adInstance);
                    RemoveUnitFromFetch(m_currUnit);

                    if (m_fetchUnits.Count == 0)
                    {
                        FillFetchUnits();

                        if (m_deferredFetchDelay >= 0.0f)
                        {
                            if (m_coroutineDeferredFetch != null)
                            {
                                StopCoroutine(m_coroutineDeferredFetch);
                            }
                            m_coroutineDeferredFetch = StartCoroutine(DeferredFetch(m_deferredFetchDelay));
                        }
                    }
                    else
                    {
                        Fetch();
                    }

                    break;
                case AdEvent.Prepared:
                    m_isLastNetworkSuccessfullyPrepared = true;
                    CancelWaitNetworkPrepare();
                    FillFetchUnits();

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
