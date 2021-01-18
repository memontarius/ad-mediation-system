using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Virterix.AdMediation
{
    public class SequenceFetchStrategy : IFetchStrategy
    {
        private const string _REPLACED_KEY = "replaced";

        public class SequenceStrategyParams : IFetchStrategyParams
        {
            public bool m_replaced;
        }

        public int TierIndex => m_currTierIndex;

        public int UnitIndex => m_currUnitIndex;

        private int m_tierIndex;
        private int m_unitIndex;
        private int m_currTierIndex;
        private int m_currUnitIndex;

        private int m_maxRecursionFetch;
        private int m_fetchCount;
        private AdUnit m_currUnit;

        public static void SetupParameters(ref IFetchStrategyParams strategyParams, Dictionary<string, object> networkParams)
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
        }

        public void Reset(List<AdUnit[]> tiers, int tierIndex, int unitIndex)
        {
            if (tiers.Count == 0)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("Tier are empty!");
#endif
                return;
            }

            if (tierIndex >= tiers.Count || tierIndex < 0)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("Tier index out of range! " + tierIndex);
#endif
                tierIndex = 0;
            }

            AdUnit[] units = tiers[tierIndex];

            if (unitIndex >= units.Length)
            {
                unitIndex = 0;
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("Unit index out of range! Unit Index: " + unitIndex);
#endif
            }

            m_currUnit = units.Length == 0 ? null : units[unitIndex];
            m_tierIndex = tierIndex;
            m_unitIndex = unitIndex;

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
                    if (!sequenceParams.m_replaced || (tierIndex == m_tierIndex && unitIndex == m_unitIndex))
                    {
                        m_tierIndex = tierIndex;
                        m_unitIndex = unitIndex;
                        break;
                    }
                    else
                    {
                        nextUnit = null;
                    }
                }

                if (nextUnit != null || (tierIndex == m_tierIndex && unitIndex == m_unitIndex))
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
            m_currTierIndex = m_tierIndex;
            m_currUnitIndex = m_unitIndex;
        }

        public AdUnit Fetch(List<AdUnit[]> tiers)
        {
            if (tiers.Count == 0)
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("Tiers are empty!");
#endif
                return null;
            }

            m_fetchCount = 0;
            AdUnit fetchedUnit = InternalFetch(tiers);
            return fetchedUnit;
        }

        private AdUnit InternalFetch(List<AdUnit[]> tiers)
        {
            AdUnit fetchedUnit = null;
            int tiersCount = tiers.Count;
  
            AdUnit[] units = tiers[m_tierIndex];
            m_fetchCount++;

            AdUnit currUnit = null;
            if (m_unitIndex >= 0 && m_unitIndex < units.Length)
            {
                currUnit = units[m_unitIndex];
            }
            else
            {
#if AD_MEDIATION_DEBUG_MODE
                Debug.LogWarning("Unit are empty!");
#endif
            }
            SequenceStrategyParams sequenceParams = currUnit == null ? null : currUnit.FetchStrategyParams as SequenceStrategyParams;

            m_currTierIndex = m_tierIndex;
            m_currUnitIndex = m_unitIndex;

            m_unitIndex++;
            if (m_unitIndex >= units.Length)
            {
                m_unitIndex = 0;
                m_tierIndex++;
                if (m_tierIndex >= tiersCount)
                {
                    m_tierIndex = 0;
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
