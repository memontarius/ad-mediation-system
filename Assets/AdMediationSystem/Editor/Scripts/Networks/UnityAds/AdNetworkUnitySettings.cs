using System;

namespace Virterix.AdMediation.Editor
{
    [Serializable]
    public class AdNetworkUnitySettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(UnityAdsAdapter);

        public override bool IsAdSupported(AdType adType)
        {
            bool isSupported = adType == AdType.Interstitial || adType == AdType.Incentivized;
            return isSupported;
        }
    }
} // namespace Virterix.AdMediation.Editor