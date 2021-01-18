using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Virterix.AdMediation
{
    public class RandomFetchStrategy : IFetchStrategy
    {
        struct Range
        {
            public int min;
            public int max;
        }

        public class RandomStrategyParams : IFetchStrategyParams
        {
            public int m_percentage;
        }

        public int TierIndex => m_currTierIndex;
        public int UnitIndex => m_unitIndex;

        private int m_tierIndex;
        private int m_currTierIndex;
        private int m_unitIndex;
        private int m_maxRecursionFetch;
        private int m_fetchCount;
        private List<AdUnit> m_fetchedUnits = new List<AdUnit>();

        public static void SetupParameters(ref IFetchStrategyParams strategyParams, Dictionary<string, object> networkParams)
        {
            RandomStrategyParams randomFetchParams = strategyParams as RandomStrategyParams;
            randomFetchParams.m_percentage = System.Convert.ToInt32(networkParams["percentage"]);
        }

        public void Init(List<AdUnit[]> tiers, int totalunits)
        {
            m_maxRecursionFetch = tiers.Count;
        }

        public void Reset(List<AdUnit[]> tiers, int tierIndex, int unitIndex)
        {
            if (tiers.Count > 0)
            {
                tierIndex++;
                tierIndex = tierIndex >= tiers.Count ? 0 : tierIndex;
                m_currTierIndex = tierIndex;
                m_tierIndex = tierIndex;
            }
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
            AdUnit unit = InternalFetch(tiers);
            return unit;
        }

        private int FetchByRandom(List<AdUnit> units)
        {
            int unitIndex = -1;

            int unitCount = units.Count;
            if (unitCount == 1)
            {
                unitIndex = 0;
                return unitIndex;
            }

            int positionInRange = 0;
            Range[] unitRanges = new Range[units.Count];

            for (int i = 0; i < unitCount; i++)
            {
                RandomStrategyParams parameters = units[i].FetchStrategyParams as RandomStrategyParams;

                Range unitRange = new Range();
                unitRange.min = positionInRange;
                unitRange.max = positionInRange + parameters.m_percentage;
                unitRanges[i] = unitRange;
                positionInRange += parameters.m_percentage;
            }
            int randomNumber = Random.Range(0, positionInRange);

            for (int i = 0; i < unitCount; i++)
            {
                Range unitRange = unitRanges[i];
                if (randomNumber >= unitRange.min && randomNumber < unitRange.max)
                {
                    unitIndex = i;
                    break;
                }
            }
            return unitIndex;
        }

        private AdUnit InternalFetch(List<AdUnit[]> tiers)
        {
            m_currTierIndex = m_tierIndex;
            AdUnit[] units = tiers[m_tierIndex];
            m_fetchCount++;

            m_tierIndex++;
            m_tierIndex = m_tierIndex >= tiers.Count ? 0 : m_tierIndex;

            FindUnitsForFetch(units, ref m_fetchedUnits);
            AdUnit unit = null;
            if (m_fetchedUnits.Count > 0)
            {
                m_unitIndex = FetchByRandom(m_fetchedUnits);
                unit = units[m_unitIndex];
            }
            else
            {
                if (m_fetchCount < m_maxRecursionFetch)
                {
                    unit = InternalFetch(tiers);
                }
            }
            return unit;
        }

        private void FindUnitsForFetch(AdUnit[] units, ref List<AdUnit> foundUnits)
        {
            m_fetchedUnits.Clear();
            for (int i = 0; i < units.Length; i++)
            {
                AdUnit unit = units[i];
                if (!unit.IsTimeout)
                {
                    foundUnits.Add(unit);
                }
            }
        }
    }
} // namespace Virterix.AdMediation