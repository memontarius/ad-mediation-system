using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation
{
    public class EmptyFetchStrategy : IFetchStrategy
    {
        public int TierIndex => 0;

        public int UnitIndex => 0;

        public void Init(List<AdUnit[]> tiers, int totalunits)
        {
        }

        public void Reset(List<AdUnit[]> tiers, int tierIndex, int unitIndex)
        {
        }

        public AdUnit Fetch(List<AdUnit[]> tiers)
        {
            return null;
        }
    }
} // namespace Virterix.AdMediation
