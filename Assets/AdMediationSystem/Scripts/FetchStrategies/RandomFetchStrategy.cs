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

        public class RandomStrategyParams : BaseFetchStrategyParams
        {
            public int m_percentage;
        }

        public int TierIndex => m_tierIndex;
        public int UnitIndex => m_unitIndex;

        private int m_tierIndex;
        private int m_unitIndex;

        private bool m_isFirstFetchSinceStartApplication;
        private int m_tierPassCount;
        private int m_maxTierPass;

        private int m_maxRecursionFetch;
        private int m_fetchCount;
        private List<AdUnit> m_readyUnits = new List<AdUnit>();

        public static void SetupParameters(ref BaseFetchStrategyParams strategyParams, Dictionary<string, object> networkParams)
        {
            RandomStrategyParams randomFetchParams = strategyParams as RandomStrategyParams;
            randomFetchParams.m_percentage = System.Convert.ToInt32(networkParams["percentage"]);
        }

        public void Init(List<AdUnit[]> tiers, int totalunits)
        {
            m_isFirstFetchSinceStartApplication = true;
            m_tierPassCount = 1;
            m_maxTierPass = 1;
            m_maxRecursionFetch = tiers.Count;
        }

        public void Reset(List<AdUnit[]> tiers, int tierIndex, int unitIndex)
        {
            if (tiers.Count > 0)
            {
                m_tierIndex = Mathf.Clamp(tierIndex, 0, tiers.Count - 1);
                m_tierIndex = m_maxTierPass > 1 ? m_tierIndex : m_tierIndex + 1;
                m_tierIndex = m_tierIndex >= tiers.Count ? 0 : m_tierIndex; 
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
            m_fetchCount++;

            int previousTierIndex = m_tierIndex;
            AdUnit[] units = tiers[m_tierIndex];
            FindUnitsForFetch(units, ref m_readyUnits);

            if (m_isFirstFetchSinceStartApplication)
            {
                m_isFirstFetchSinceStartApplication = false;
            }
            else
            {
                bool isMovingToNextTier = m_tierPassCount < m_maxTierPass ? ResolveMovingToNextTier(m_readyUnits.ToArray()) : true;

                if (isMovingToNextTier)
                {
                    m_tierIndex++;
                    m_tierPassCount = 1;
                }
                else
                {
                    m_tierPassCount++;
                }

                if (m_tierIndex >= tiers.Count)
                {
                    m_tierIndex = 0;
                }

                if (m_tierIndex != previousTierIndex)
                {
                    units = tiers[m_tierIndex];
                    FindUnitsForFetch(units, ref m_readyUnits);
                }
            }

            AdUnit unit = null;
            if (m_readyUnits.Count > 0)
            {
                m_unitIndex = FetchByRandom(m_readyUnits);
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
            m_readyUnits.Clear();
            for (int i = 0; i < units.Length; i++)
            {
                AdUnit unit = units[i];
                if (!unit.IsTimeout)
                {
                    foundUnits.Add(unit);
                }
            }
        }

        private bool ResolveMovingToNextTier(AdUnit[] units)
        {
            int unvalidCount = 0;
            for (int unitIndex = 0; unitIndex < units.Length; unitIndex++)
            {
                var unit = units[unitIndex];           
                if (unit.IsTimeout || unit.AdInstance.WasLastPreparationFailed)
                {
                    unvalidCount++;
                }
            }
            bool isMovingToNextTier = unvalidCount == units.Length;
            return isMovingToNextTier;
        }
    }
} // namespace Virterix.AdMediation