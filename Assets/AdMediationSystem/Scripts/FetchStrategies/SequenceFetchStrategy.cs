using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Virterix.AdMediation
{
    public class SequenceFetchStrategy : IFetchStrategy
    {
        private const string _REPLACED_KEY = "replaced";

        public class SequenceStrategyParams : BaseFetchStrategyParams
        {
            public bool m_replaced;
        }

        public int TierIndex => m_currTierIndex;

        public int UnitIndex => m_currUnitIndex;

        private int m_nextTierIndex;
        private int m_nextUnitIndex;
        private int m_currTierIndex;
        private int m_currUnitIndex;

        private int m_maxRecursionFetch;
        private int m_fetchCount;
        private AdUnit m_currUnit;

        public static void SetupParameters(ref BaseFetchStrategyParams strategyParams, Dictionary<string, object> networkParams)
        {
            SequenceStrategyParams sequenceStrategyParams = strategyParams as SequenceStrategyParams;
            if (networkParams.ContainsKey(_REPLACED_KEY))
            {
                sequenceStrategyParams.m_replaced = Convert.ToBoolean(networkParams[_REPLACED_KEY]);
            }
        }

        public void Init(List<AdUnit[]> tiers, int totalunits)
        {
            m_maxRecursionFetch = totalunits;

            m_passTierCount = 1;
            m_maxPassTier = 3;
        }

        public void Reset(List<AdUnit[]> tiers, int tierIndex, int unitIndex)
        {
            if (tiers.Count == 0)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("[AdMediationSystem] Tier are empty!");
#endif
                return;
            }

            if (tierIndex >= tiers.Count || tierIndex < 0)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("[AdMediationSystem] Tier index out of range! " + tierIndex);
#endif
                tierIndex = 0;
            }

            AdUnit[] units = tiers[tierIndex];

            if (unitIndex >= units.Length)
            {
                unitIndex = 0;
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("[AdMediationSystem] Unit index out of range! Unit Index: " + unitIndex);
#endif
            }

            m_currUnit = units.Length == 0 ? null : units[unitIndex];
            m_nextTierIndex = tierIndex;
            m_nextUnitIndex = unitIndex;

            AdUnit nextUnit = null;
            SequenceStrategyParams sequenceParams = null;
            unitIndex++;
            for (; tierIndex < tiers.Count; tierIndex++)
            {
                units = tiers[tierIndex];
                for (; unitIndex < units.Length; unitIndex++)
                {
                    nextUnit = units[unitIndex];
                    sequenceParams = nextUnit.FetchStrategyParams as SequenceStrategyParams;
                    if (!sequenceParams.m_replaced || (tierIndex == m_nextTierIndex && unitIndex == m_nextUnitIndex))
                    {
                        m_nextTierIndex = tierIndex;
                        m_nextUnitIndex = unitIndex;
                        break;
                    }
                    else
                    {
                        nextUnit = null;
                    }
                }

                if (nextUnit != null || (tierIndex == m_nextTierIndex && unitIndex == m_nextUnitIndex))
                {
                    break;
                }
                else
                {
                    unitIndex = 0;
                    tierIndex = (tierIndex + 1 == tiers.Count) ? -1 : tierIndex;
                }
            }

            m_currUnit = nextUnit == null ? m_currUnit : nextUnit;
            m_currTierIndex = m_nextTierIndex;
            m_currUnitIndex = m_nextUnitIndex;
        }

        public AdUnit Fetch(List<AdUnit[]> tiers)
        {
            if (tiers.Count == 0)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("[AdMediationSystem] Tiers are empty!");
#endif
                return null;
            }

            m_fetchCount = 0;
            //AdUnit fetchedUnit = InternalFetch(tiers);
            AdUnit fetchedUnit = NewInternalFetch(tiers);
            return fetchedUnit;
        }

        private AdUnit InternalFetch(List<AdUnit[]> tiers)
        {
            AdUnit fetchedUnit = null;
            int tiersCount = tiers.Count;

            m_currTierIndex = m_nextTierIndex;
            m_currUnitIndex = m_nextUnitIndex;

            AdUnit[] units = tiers[m_currTierIndex];
            m_fetchCount++;

            AdUnit currUnit = null;
            if (m_currUnitIndex >= 0 && m_currUnitIndex < units.Length)
            {
                currUnit = units[m_currUnitIndex];
            }
            else
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("[AdMediationSystem] Unit is empty!");
#endif
            }
            SequenceStrategyParams sequenceParams = currUnit == null ? null : currUnit.FetchStrategyParams as SequenceStrategyParams;

            m_nextUnitIndex++;
            if (m_nextUnitIndex >= units.Length)
            {
                m_nextUnitIndex = 0;
                bool isMovingToNextTier = true;
                if (isMovingToNextTier)
                {
                    m_nextTierIndex++;
                }

                if (m_nextTierIndex >= tiersCount)
                {
                    m_nextTierIndex = 0;
                }
            }

            bool isUnitSkip = currUnit == null ? false : ResolveSkipUnit(currUnit, units, sequenceParams);
            bool isNextUnitFind = isUnitSkip && m_fetchCount < m_maxRecursionFetch;
            fetchedUnit = isUnitSkip ? fetchedUnit : currUnit;

            if (isNextUnitFind)
            {
                fetchedUnit = InternalFetch(tiers);
            }

            return fetchedUnit;
        }

        private bool m_isFirstFetchSinceStartApplication = true;
        private int m_passTierCount;
        private int m_maxPassTier;

        private int m_tierIndex;
        private int m_unitIndex;


        private AdUnit NewInternalFetch(List<AdUnit[]> tiers)
        {
            AdUnit fetchedUnit = null;
            int tiersCount = tiers.Count;
            m_fetchCount++;
            AdUnit[] units = tiers[m_tierIndex];

            if (m_isFirstFetchSinceStartApplication)
            {
                m_isFirstFetchSinceStartApplication = false;
            }
            else
            {
                m_unitIndex++;
                if (m_unitIndex >= units.Length)
                {
                    m_unitIndex = 0;
                    bool isMovingToNextTier = m_passTierCount < m_maxPassTier ? ResolveMovingToNextTier(units) : true;

                    Debug.LogWarning("isMovingToNextTier: "+ isMovingToNextTier + " " + m_passTierCount);

                    if (isMovingToNextTier)
                    {
                        m_tierIndex++;
                        m_passTierCount = 1;
                    }
                    else
                    {
                        m_passTierCount++;
                    }

                    if (m_tierIndex >= tiersCount)
                    {
                        m_tierIndex = 0;
                    }
                    units = tiers[m_tierIndex];
                }
            }

            AdUnit currUnit = null;
            if (m_unitIndex >= 0 && m_unitIndex < units.Length)
            {
                currUnit = units[m_unitIndex];
            }
            else
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("[AdMediationSystem] Unit is empty!");
#endif
            }
            SequenceStrategyParams sequenceParams = currUnit == null ? null : currUnit.FetchStrategyParams as SequenceStrategyParams;

            bool isSkipUnit = currUnit == null ? false : ResolveSkipUnit(currUnit, units, sequenceParams);
            bool isFindNextUnit = isSkipUnit && m_fetchCount < m_maxRecursionFetch;
            fetchedUnit = isSkipUnit ? fetchedUnit : currUnit;

            if (isFindNextUnit)
            {
                fetchedUnit = NewInternalFetch(tiers);
            }

            return fetchedUnit;
        }

        private bool ResolveSkipUnit(AdUnit unit, AdUnit[] units, SequenceStrategyParams unitSequenceParams)
        {
            bool skip = unit.IsTimeout;
            if (!skip && unitSequenceParams.m_replaced)
            {
                AdUnit passUnit = null;
                for (int i = 0; i < units.Length; i++)
                {
                    passUnit = units[i];
                    if (passUnit == unit)
                    {
                        continue;
                    } 
                    skip = passUnit.WasLastImpressionSuccessful;
                    if (skip)
                    {
                        break;
                    }
                }
            }
            return skip;
        }

        private bool ResolveMovingToNextTier(AdUnit[] units)
        {
            bool isMovingToNextTier = false;
            int failedCount = 0;
            for (int unitIndex = 0; unitIndex < units.Length; unitIndex++)
            {
                var unit = units[unitIndex];
                if (unit.IsTimeout || unit.AdInstance.WasLastPreparationFailed)
                {
                    failedCount++;
                }
            }
            isMovingToNextTier = failedCount == units.Length;
            return isMovingToNextTier;
        }
    }
} // namespace Virterix.AdMediation
