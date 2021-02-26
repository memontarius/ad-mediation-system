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
        public string _networkIdentifier;
        public int _instanceIndex;
        public string _instanceName;
        public bool _replaced;
        public bool _prepareOnExit;
        public int _percentage = 100;
    }

    [System.Serializable]
    public struct AdTier
    {
        public int _maxPassages;
        public List<AdUnit> _units;
    }
    
    [System.Serializable]
    public struct AdUnitMediator
    {
        public AdType _adType;
        public string _name;
        public bool _continueAfterEndSession;
        public bool _fetchOnAdUnitHidden;
        public bool _fetchOnStart;
        public int _bannerMinDisplayTime;
        public int _deferredFetchDelay;
        public FetchStrategyType _fetchStrategyType;
        public BannerPosition _bannerPosition;
        public List<AdTier> _tiers;
    }

    public class AdMediationProjectSettings : ScriptableObject
    {
        public bool _initializeOnStart = true;
        public bool _personalizeAdsOnInit = true;
        public bool _enableTestMode = false;
        public bool _enableExtraLogging = false;
        public bool _directedChildren = false;
        public string[] _testDevices;
        public List<AdUnitMediator> _bannerMediators = new List<AdUnitMediator>();
        public List<AdUnitMediator> _interstitialMediators = new List<AdUnitMediator>();
        public List<AdUnitMediator> _incentivizedMediators = new List<AdUnitMediator>();
        public bool _isIOS = true;
        public bool _isAndroid = true;
    }
} // Virterix.AdMediation.Editor
