using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public enum FetchStrategyType
    {
        Sequence,
        Random
    }

    [System.Serializable]
    public class AdUnit
    {
        public string _networkName;
        public string _instanceName;
    }

    [System.Serializable]
    public class AdSequencedUnit : AdUnit
    {
        public bool _replaced;
    }

    [System.Serializable]
    public class AdRandomUnit : AdUnit
    {
        public float _percentage;
    }

    [System.Serializable]
    public struct AdTier
    {
        public List<AdUnit> _units;
    }

    [System.Serializable]
    public struct AdUnitMediator
    {
        public string _name;
        public int _fetchStrategyType;
        public List<AdTier> _tiers;
    }

    public class AdMediationProjectSettings : ScriptableObject
    {
        public bool _initializeOnStart = true;
        public bool _personalizeAdsOnInit = true;       
        public List<AdUnitMediator> _bannerMediators;
        public List<AdUnitMediator> _interstitialMediators;
        public List<AdUnitMediator> _incentivizedMediators;
    }
} // Virterix.AdMediation.Editor
