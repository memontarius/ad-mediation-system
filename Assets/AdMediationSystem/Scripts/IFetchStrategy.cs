
using System.Collections.Generic;

namespace Virterix.AdMediation
{
    public class IFetchStrategyParams
    {
        public IFetchStrategyParams()
        {
            m_waitingResponseTime = 60f;
        }
        public AdType m_adsType;
        public float m_waitingResponseTime;
        public int m_impressionsInSession;
    }

    public interface IFetchStrategy
    {
        int TierIndex { get; }
        int UnitIndex { get; }

        /// <summary>
        /// Makes fetch from the array of units
        /// </summary>
        AdUnit Fetch(AdMediator mediator, AdUnit[] units);
        /// <summary>
        /// Resets to a start state
        /// </summary>
        /// <param name="unit">New current ad unit</param>
        //void Reset(AdMediator mediator, AdUnit unit);
        bool IsAllowAutoFillUnits();

        void Reset(AdUnit unit, int tierIndex, int unitIndex);

        /// <summary>
        /// Fetches ad unit from list
        /// </summary>
        /// <param name="tiers">List of ad units</param>
        /// <param name="maxRecursionFetch">Maximum number of fetch when the fetched unit cannot be impression</param>
        /// <returns></returns>
        AdUnit Fetch(List<AdUnit[]> tiers, int maxRecursionFetch);

    }
} // namespace Virterix.AdMediation