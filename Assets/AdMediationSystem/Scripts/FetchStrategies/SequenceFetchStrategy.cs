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

        public int TierIndex => m_tierIndex;

        public int UnitIndex => m_unitIndex;

        private bool m_isFirstFetchSinceStartApplication;
        private int m_tierPassCount;
        private int m_maxTierPass;

        private int m_tierIndex;
        private int m_unitIndex;

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
            m_isFirstFetchSinceStartApplication = true;
            m_tierPassCount = 1;
            m_maxTierPass = 2;
        }

        public void Reset(List<AdUnit[]> tiers, int tierIndex, int unitIndex)
        {
            Debug.Log(" ===== RESTORE " + tierIndex + " " + unitIndex);

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
            m_tierIndex = tierIndex;
            m_unitIndex = unitIndex;

            AdUnit restoredUnit = null;
            SequenceStrategyParams sequenceParams = null;
            unitIndex++;
            int tierPassCount = 0;

            for (; tierIndex < tiers.Count;)
            {
                units = tiers[tierIndex];
                for (; unitIndex < units.Length; unitIndex++)
                {
                    restoredUnit = units[unitIndex];
                    sequenceParams = restoredUnit.FetchStrategyParams as SequenceStrategyParams;
                    if (!sequenceParams.m_replaced || (tierIndex == m_tierIndex && unitIndex == m_unitIndex))
                    {
                        m_tierIndex = tierIndex;
                        m_unitIndex = unitIndex;
                        break;
                    }
                    else
                    {
                        restoredUnit = null;
                    }
                }

                if (restoredUnit != null || (tierIndex == m_tierIndex && unitIndex == m_unitIndex))
                {
                    break;
                }
                else
                {
                    unitIndex = 0;

                    bool isMovingToNextTier = true;
                    if (m_maxTierPass > 1)
                    {
                        tierPassCount++;
                        isMovingToNextTier = tierPassCount >= m_maxTierPass;
                    }

                    if (isMovingToNextTier) 
                    {
                        tierIndex = (tierIndex + 1 == tiers.Count) ? 0 : tierIndex + 1;
                        tierPassCount = 0;
                    }
                }
            }

            m_currUnit = restoredUnit == null ? m_currUnit : restoredUnit;
            
            Debug.Log(" ----- RESTORED " + m_tierIndex + " " + m_unitIndex);

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
            AdUnit fetchedUnit = InternalFetch(tiers);
            return fetchedUnit;
        }

        /*
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
        */
        private AdUnit InternalFetch(List<AdUnit[]> tiers)
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
                    bool isMovingToNextTier = m_tierPassCount < m_maxTierPass ? ResolveMovingToNextTier(units) : true;

                    if (isMovingToNextTier)
                    {
                        m_tierIndex++;
                        m_tierPassCount = 1;
                    }
                    else
                    {
                        m_tierPassCount++;
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
                fetchedUnit = InternalFetch(tiers);
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
            int unvalidCount = 0;
            int replacedCount = 0;
            SequenceStrategyParams unitSequenceParams;

            for (int unitIndex = 0; unitIndex < units.Length; unitIndex++)
            {
                var unit = units[unitIndex];
                unitSequenceParams = unit.FetchStrategyParams as SequenceStrategyParams;
                if (unitSequenceParams.m_replaced)
                {
                    replacedCount++;
                }
                else
                {
                    if (unit.IsTimeout || unit.AdInstance.WasLastPreparationFailed)
                    {
                        unvalidCount++;
                    }
                }
            }
            bool isMovingToNextTier = unvalidCount == (units.Length - replacedCount);
            return isMovingToNextTier;
        }
    }
} // namespace Virterix.AdMediation
