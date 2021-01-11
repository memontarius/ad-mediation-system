using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation
{
    public class EmptyFetchStrategy : IFetchStrategy
    {
        public AdUnit Fetch(AdMediator mediator, AdUnit[] units)
        {
            return null;
        }

        public bool IsAllowAutoFillUnits()
        {
            return false;
        }

        public void Reset(AdMediator mediator, AdUnit unit)
        {
        }
    }
} // namespace Virterix.AdMediation
