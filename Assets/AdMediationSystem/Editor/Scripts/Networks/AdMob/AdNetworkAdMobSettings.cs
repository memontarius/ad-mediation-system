using System;

namespace Virterix.AdMediation.Editor
{
    [Serializable]
    public class AdNetworkAdMobSettings : BaseAdNetworkSettings
    {
        public bool _enabled;
        public string _androidAppId;
        public string _iosAppId;
    }
} // namespace Virterix.AdMediation.Editor