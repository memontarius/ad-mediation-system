
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

        void Init(AdUnit[][] tiers, int totalunits, int[] tierMaxPassages);

        void Reset(AdUnit[][] tiers, int tierIndex, int unitIndex);

        /// <summary>
        /// Fetches ad unit from list
        /// </summary>
        /// <param name="tiers">List of ad units</param>
        /// <param name="maxRecursionFetch">Maximum number of fetch when the fetched unit cannot be impression</param>
        /// <returns></returns>
        AdUnit Fetch(AdUnit[][] tiers);

    }
} // namespace Virterix.AdMediation