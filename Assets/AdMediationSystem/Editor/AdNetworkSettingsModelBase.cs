using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    [System.Serializable]
    public struct AdUnit
    {
        public string _name;
        public string _androidId;
        public string _iosId;
    }

    public class AdNetworkSettingsModelBase : ScriptableObject
    {
        public List<AdUnit> _bannerUnits = new List<AdUnit>();
        public List<AdUnit> _interstitialUnits = new List<AdUnit>();
        public List<AdUnit> _rewardUnits = new List<AdUnit>();
    }
} // namespace Virterix.AdMediation.Editor