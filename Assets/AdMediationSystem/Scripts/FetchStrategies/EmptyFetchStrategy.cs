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

        public AdUnit Fetch(AdMediator mediator, AdUnit[] units)
        {
            return null;
        }

        public bool IsAllowAutoFillUnits()
        {
            return false;
        }

        public void Reset(AdUnit unit, int tierIndex, int unitIndex)
        {
        }

        public AdUnit Fetch(List<AdUnit[]> tiers, int maxRecursionFetch)
        {
            return null;
        }
    }
} // namespace Virterix.AdMediation
