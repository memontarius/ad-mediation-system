using System;

namespace Virterix.AdMediation.Editor
{
    [Serializable]
    public class AdNetworkAdMobSettingsModel : BaseAdNetworkSettingsModel
    {
        public bool _enabled;
        public string _androidAppId;
        public string _iosAppId;
        public string _androidRewardVideoUnitId;
        public string _iosRewardVideoUnitId;
    }
} // namespace Virterix.AdMediation.Editor