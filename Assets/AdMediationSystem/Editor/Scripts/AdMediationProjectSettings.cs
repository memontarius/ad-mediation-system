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
        public int _networkIndex;
        public int _instanceIndex;
        public bool _replaced;
        public int _percentage = 100;
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
        public FetchStrategyType _fetchStrategyType;
        public List<AdTier> _tiers;
    }

    public class AdMediationProjectSettings : ScriptableObject
    {
        public bool _initializeOnStart = true;
        public bool _personalizeAdsOnInit = true;       
        public List<AdUnitMediator> _bannerMediators;
        public List<AdUnitMediator> _interstitialMediators;
        public List<AdUnitMediator> _incentivizedMediators;
        public bool _isIOS = true;
        public bool _isAndroid = true;
    }
} // Virterix.AdMediation.Editor
