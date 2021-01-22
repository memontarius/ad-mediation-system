using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    [System.Serializable]
    public struct AdUnitInstance
    {
        public string _name;
        public string _androidId;
        public string _iosId;
        public float _timeout;
    }

    public class BaseAdNetworkSettingsModel : ScriptableObject
    {
        public List<AdUnitInstance> _bannerAdInstances = new List<AdUnitInstance>();
        public List<AdUnitInstance> _interstitialAdInstances = new List<AdUnitInstance>();
        public List<AdUnitInstance> _rewardAdInstances = new List<AdUnitInstance>();
    }
} // namespace Virterix.AdMediation.Editor