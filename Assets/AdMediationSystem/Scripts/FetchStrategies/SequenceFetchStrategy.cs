using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Virterix.AdMediation
{
    public class SequenceFetchStrategy : IFetchStrategy
    {
        private const string _REPLACED_KEY = "replaced";
        private const string _IMPRESSION_KEY = "impressions";
        private const string _SKIP_FETCH_INDEX_KEY = "skipFetchIndex";
        private const string _REPLACEBLE_NETWORKS = "replaceableNetworks";
        private const string _SKIP_NEXT_UNIT_WHEN_SUCCESSFUL_KEY = "ifShowedSkipNextUnit";

        public class SequenceStrategyParams : IFetchStrategyParams
        {
            public int m_index;
            public bool m_replaced;
            public int m_impressions;
            public int m_skipFetchIndex;
            public string m_skipNextUnitNetworkName;
            public AdNetworkAdapter[] m_replacebleNetworks;

            public bool IsSkipNextUnitContains
            {
                get { return m_skipNextUnitNetworkName != null; }
            }
        }

        private int m_currFetchCount;
        private int m_currUnitIndex;
        private AdUnit m_currUnit;
        private int m_skipCount;
        private int m_maxSkipCount;

        public bool IsAllowAutoFillUnits()
        {
            return false;
        }

        public AdUnit Fetch(AdMediator mediator, AdUnit[] units)
        {
            m_skipCount = 0;
            m_maxSkipCount = 6;
            AdUnit unit = MoveToNextUnit(mediator, units);
            return unit;
        }

        /*
        /// <summary>
        /// Reset to start state
        /// </summary>
        /// <param name="mediator">Mediator for find index in fetch units array</param>
        /// <param name="unit">Set current unit</param>
        public void Reset(AdMediator mediator, AdUnit unit)
        {
            m_currUnit = unit;
            if (mediator != null)
            {
                m_currUnitIndex = mediator.FindIndexInFetchUnits(m_currUnit);
            }
            else
            {
                m_currUnitIndex = 0;
            }
            m_currFetchCount = 1;
        }
        */

        private bool IsSkipUnit(AdUnit unit)
        {
            bool isSkip = false;
            SequenceStrategyParams sequenceParams = unit.FetchStrategyParams as SequenceStrategyParams;

            if (sequenceParams.m_skipFetchIndex != 0)
            {
                isSkip = unit.FetchCount % sequenceParams.m_skipFetchIndex == 0;
            }
            if (!isSkip && sequenceParams.m_impressionsInSession != 0)
            {
                isSkip = unit.Impressions >= sequenceParams.m_impressionsInSession;
            }
            return isSkip;
        }

        private bool IsMoveNextUnit(AdUnit unit)
        {
            SequenceStrategyParams sequenceParams = GetStrategyParams(unit);
            bool isMoveNext = m_currFetchCount >= sequenceParams.m_impressions;
            return isMoveNext;
        }

        private AdUnit MoveToNextUnit(AdMediator mediator, AdUnit[] units)
        {

            if (units.Length == 0)
            {
                return null;
            }

            int nextUnitIndex = m_currUnitIndex;
            bool isNeedReset = false;
            AdUnit m_previousUnit = m_currUnit;

            if (m_currUnit == null)
            {
                nextUnitIndex = 0;
            }
            else
            {
                // If current unit not contained in fetch array then next unit index doesn't increment.
                if (m_currUnit.IsContainedInFetch)
                {
                    if (IsMoveNextUnit(m_currUnit))
                    {
                        nextUnitIndex++;
                        isNeedReset = true;
                    }
                }
                else
                {
                    isNeedReset = true;
                }
            }

            if (nextUnitIndex >= units.Length)
            {
                nextUnitIndex = 0;
                mediator.FillFetchUnits(true);
                units = mediator.FetchUnits.ToArray();
                if (units.Length == 0)
                {
                    return null;
                }

                if (!isNeedReset)
                {
                    nextUnitIndex = FindIndex(m_currUnit, units);
                    nextUnitIndex = nextUnitIndex == -1 ? 0 : nextUnitIndex;
                }
            }

            m_currUnitIndex = nextUnitIndex;
            m_currUnit = units[m_currUnitIndex];
            m_currUnit.IncrementFetchCount();
            m_currFetchCount++;

            if (isNeedReset)
            {
                //Reset(mediator, m_currUnit);
            }

            string networkName = "";
            if (m_currUnit != null)
            {
                networkName = m_currUnit.AdNetwork.m_networkName;
            }

            bool isSkipUnit = IsSkipUnit(m_currUnit);

            if (!isSkipUnit && m_currUnit != null)
            {
                SequenceStrategyParams sequenceStrategyParams = m_currUnit.FetchStrategyParams as SequenceStrategyParams;
                if (m_previousUnit != null)
                {
                    SequenceStrategyParams previousSequenceStrategyParams = m_previousUnit.FetchStrategyParams as SequenceStrategyParams;
                    if (previousSequenceStrategyParams.IsSkipNextUnitContains)
                    {
                        if (m_previousUnit.WasLastImpressionSuccessful)
                        {
                            isSkipUnit = previousSequenceStrategyParams.m_skipNextUnitNetworkName == "";
                            isSkipUnit = isSkipUnit ? isSkipUnit : previousSequenceStrategyParams.m_skipNextUnitNetworkName == m_currUnit.AdNetwork.m_networkName;
                        }
                    }
                }

                if (!isSkipUnit && sequenceStrategyParams.m_replacebleNetworks != null)
                {
                    foreach (AdNetworkAdapter replacebleNetwork in sequenceStrategyParams.m_replacebleNetworks)
                    {
                        AdInstanceData adInstance = replacebleNetwork.GetAdInstance(m_currUnit.AdapterAdType, m_currUnit.AdInstanceName);
                        if (replacebleNetwork.GetEnabledState(m_currUnit.AdapterAdType, adInstance))
                        {
                            bool prepared = replacebleNetwork.GetLastAdPreparedStatus(m_currUnit.AdapterAdType, adInstance);
                            isSkipUnit = true;
                            if (!prepared)
                            {
                                isSkipUnit = false;
                                break;
                            }
                        }
                        else
                        {
                            isSkipUnit = false;
                        }
                    }
                }
            }

            if (isSkipUnit && m_skipCount < m_maxSkipCount)
            {
                m_skipCount++;
                m_currUnit = MoveToNextUnit(mediator, units);
            }

            return m_currUnit;
        }

        private SequenceStrategyParams GetStrategyParams(AdUnit unit)
        {
            SequenceStrategyParams sequenceParams = unit.FetchStrategyParams as SequenceStrategyParams;
            return sequenceParams;
        }

        private int FindIndex(AdUnit unit, AdUnit[] units)
        {
            int index = -1;
            for (int i = 0; i < units.Length; i++)
            {
                if (units[i] == unit)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public static void SetupParameters(ref IFetchStrategyParams strategyParams, Dictionary<string, object> networkParams)
        {
            SequenceStrategyParams sequenceStrategyParams = strategyParams as SequenceStrategyParams;
            sequenceStrategyParams.m_index = System.Convert.ToInt32(networkParams["index"]);

            if (networkParams.ContainsKey(_REPLACED_KEY))
            {
                sequenceStrategyParams.m_replaced =   Convert.ToBoolean(networkParams[_REPLACED_KEY]);
            }

            /*
            int impressions = 1;
            if (networkParams.ContainsKey(_IMPRESSION_KEY))
            {
                impressions = Convert.ToInt32(networkParams[_IMPRESSION_KEY]);
            }
            sequenceStrategyParams.m_impressions = impressions;

            if (networkParams.ContainsKey(_SKIP_FETCH_INDEX_KEY))
            {
                sequenceStrategyParams.m_skipFetchIndex = System.Convert.ToInt32(networkParams[_SKIP_FETCH_INDEX_KEY]);
            }

            sequenceStrategyParams.m_skipNextUnitNetworkName = null;
            if (networkParams.ContainsKey(_SKIP_NEXT_UNIT_WHEN_SUCCESSFUL_KEY))
            {
                sequenceStrategyParams.m_skipNextUnitNetworkName = System.Convert.ToString(networkParams[_SKIP_NEXT_UNIT_WHEN_SUCCESSFUL_KEY]);
            }

            
            if (networkParams.ContainsKey(_REPLACEBLE_NETWORKS))
            {
                string[] networks = networkParams[_REPLACEBLE_NETWORKS].Split(',');
                int networkCount = networks.Length;
                sequenceStrategyParams.m_replacebleNetworks = networkCount > 0 ? new AdNetworkAdapter[networkCount] : null;
                for (int i = 0; i < networkCount; i++)
                {
                    sequenceStrategyParams.m_replacebleNetworks[i] = AdMediationSystem.Instance.GetNetwork(networks[i]);
                }
            }*/
        }

        private int m_currTierIndex;
        private int m_currTierUnitIndex;
        private int m_maxRecursionFetch;
        private int m_fetchCount;

        public AdUnit Fetch(List<AdUnit[]> tiers, int maxRecursionFetch)
        {
            AdUnit[] units = tiers[m_currTierIndex];
            m_maxRecursionFetch = maxRecursionFetch;
            m_fetchCount = 0;

            AdUnit fetchedUnit = InternalFetch(tiers);

            return fetchedUnit;
        }

        public void Reset(AdUnit unit, int tierIndex, int unitIndex)
        {
            m_currUnit = unit;
            m_currTierUnitIndex = tierIndex;
            m_currUnitIndex = unitIndex;
        }

        private AdUnit InternalFetch(List<AdUnit[]> tiers)
        {
            AdUnit fetchedUnit = null;
            int tiersCount = tiers.Count;
            AdUnit[] units = tiers[m_currTierIndex];
            AdUnit currUnit = units[m_currTierUnitIndex];

            m_fetchCount++;
            m_currTierUnitIndex++;
            if (m_currTierUnitIndex >= units.Length)
            {
                m_currTierUnitIndex = 0;
                m_currTierIndex++;
                if (m_currTierIndex >= tiersCount)
                {
                    m_currTierIndex = 0;
                }
            }

            SequenceStrategyParams sequenceParams = currUnit.FetchStrategyParams as SequenceStrategyParams;

            bool isUnitSkip = ResolveSkipUnit(currUnit, units, sequenceParams);
            bool isNextUnitFind = isUnitSkip && m_fetchCount < m_maxRecursionFetch;
            fetchedUnit = isUnitSkip ? fetchedUnit : currUnit;

            if (isNextUnitFind)
            {
                fetchedUnit = InternalFetch(tiers);
            }

            return fetchedUnit;
        }

        private bool ResolveSkipUnit(AdUnit unit, AdUnit[] tierUnits, SequenceStrategyParams unitSequenceParams)
        {
            bool isSkip = unit.IsTimeout;
            if (!isSkip && unitSequenceParams.m_replaced)
            {
                AdUnit passUnit = null;
                for (int i = 0; i < tierUnits.Length; i++)
                {
                    passUnit = tierUnits[i];
                    isSkip = passUnit.WasLastImpressionSuccessful;
                    if (isSkip)
                    {
                        break;
                    }
                }
            }
            return isSkip;
        }
    }
} // namespace Virterix.AdMediation
