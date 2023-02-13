//#define _AMS_AUDIENCE_NETWORK

#if UNITY_IOS && _AMS_AUDIENCE_NETWORK
using System.Runtime.InteropServices;
#endif

namespace Virterix.AdMediation
{
    namespace AudienceNetworkMediationUtils
    {
        public static class AdSettings
        {
#if UNITY_IOS && _AMS_AUDIENCE_NETWORK
            [DllImport("__Internal")]
            private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);
#endif
            public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
            {
#if UNITY_IOS && _AMS_AUDIENCE_NETWORK
                FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
#endif
            }
        }
    }
}