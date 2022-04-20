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
        public string NetworkName;
        public int NetworkIndex;
        public string NetworkIdentifier;
        public int InstanceIndex;
        public string InstanceName;
        public bool Replaced;
        public bool PrepareOnExit;
        public int Percentage = 100;
    }

    [System.Serializable]
    public struct AdTier
    {
        public int MaxPassages;
        public List<AdUnit> Units;
    }
    
    [System.Serializable]
    public struct AdUnitMediator
    {
        public AdType AdvertisingType;
        public string Name;
        public bool ContinueAfterEndSession;
        public bool FetchOnAdUnitHidden;
        public bool FetchOnStart;
        public int BannerMinDisplayTime;
        public int DeferredFetchDelay;
        public FetchStrategyType FetchStrategyType;
        public BannerPosition BannerPosition;
        public List<AdTier> Tiers;
    }

    public class AdMediationProjectSettings : ScriptableObject
    {
        public bool InitializeOnStart = true;
        public bool EnableTestMode = false;
        public bool EnableExtraLogging = false;
        public ChildDirectedMode ChildrenMode = ChildDirectedMode.NotAssign;
        public string[] TestDevices;
        public List<AdUnitMediator> BannerMediators = new List<AdUnitMediator>();
        public List<AdUnitMediator> InterstitialMediators = new List<AdUnitMediator>();
        public List<AdUnitMediator> IncentivizedMediators = new List<AdUnitMediator>();
        public bool IsIOS = true;
        public bool IsAndroid = true;
        public bool EnableUnityRemoteConfigProvider;
        public string RemoteConfigPrefixKey;
        public bool RemoteConfigAutoFetching;
    }
}
