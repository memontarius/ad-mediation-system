using UnityEngine;
using System.Runtime.InteropServices;

namespace Virterix.AdMediation {
    namespace AudienceNetworkMediationUtils {
        public static class AdSettings {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);
#endif
            public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled) {
#if UNITY_IOS
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
#endif
            }
        }
    }
}