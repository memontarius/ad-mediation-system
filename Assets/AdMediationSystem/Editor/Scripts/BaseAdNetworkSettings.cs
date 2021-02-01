using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    [System.Serializable]
    public struct AdInstance
    {
        public string _name;
        public string _androidId;
        public string _iosId;
        public float _timeout;
        public int _bannerType;
    }

    public class BaseAdNetworkSettings : ScriptableObject
    {
        public bool _enabled;
        public List<AdInstance> _bannerAdInstances = new List<AdInstance>();
        public List<AdInstance> _interstitialAdInstances = new List<AdInstance>();
        public List<AdInstance> _rewardAdInstances = new List<AdInstance>();
    }
} // namespace Virterix.AdMediation.Editor