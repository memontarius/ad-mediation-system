
using System.Collections.Generic;

namespace Virterix.AdMediation
{
    public class BaseFetchStrategyParams
    {
    }

    public interface IFetchStrategy
    {
        int TierIndex { get; }
        int UnitIndex { get; }

        void Init(List<AdUnit[]> tiers, int totalunits);

        void Reset(List<AdUnit[]> tiers, int tierIndex, int unitIndex);

        /// <summary>
        /// Fetches ad unit from list
        /// </summary>
        /// <param name="tiers">List of ad units</param>
        /// <param name="maxRecursionFetch">Maximum number of fetch when the fetched unit cannot be impression</param>
        /// <returns></returns>
        AdUnit Fetch(List<AdUnit[]> tiers);

    }
} // namespace Virterix.AdMediation