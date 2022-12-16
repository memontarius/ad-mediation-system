/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2019 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using System;
using System.Runtime.InteropServices;
using YandexMobileAds.Base;
using YandexMobileAds.Common;

namespace YandexMobileAds.Platforms.iOS
{
    #if (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
    
    public class InterstitialClient : IInterstitialClient, IDisposable
    {
        private readonly IntPtr selfPointer;

        public string ObjectId { get; private set; }

        internal delegate void YMAUnityInterstitialDidLoadAdCallback(
            IntPtr bannerClient);

        internal delegate void YMAUnityInterstitialDidFailToLoadAdCallback(
            IntPtr bannerClient, string error);

        internal delegate void YMAUnityInterstitialWillPresentScreenCallback(
            IntPtr bannerClient);

        internal delegate void YMAUnityInterstitialWillLeaveApplicationCallback(
            IntPtr bannerClient);

        internal delegate void YMAUnityInterstitialDidClickCallback(
            IntPtr bannerClient);

        internal delegate void YMAUnityInterstitialWillAppearCallback(
            IntPtr bannerClient);

        internal delegate void YMAUnityInterstitialDidDismissCallback(
            IntPtr bannerClient);

        internal delegate void YMAUnityInterstitialDidTrackImpressionCallback(
            IntPtr bannerClient, string rawImpressionData);

        internal delegate void YMAUnityInterstitialDidFailToShowCallback(
            IntPtr bannerClient, string error);

        public event EventHandler<EventArgs> OnInterstitialLoaded;
        public event EventHandler<AdFailureEventArgs> OnInterstitialFailedToLoad;
        public event EventHandler<EventArgs> OnReturnedToApplication;
        public event EventHandler<EventArgs> OnLeftApplication;
        public event EventHandler<EventArgs> OnAdClicked;
        public event EventHandler<EventArgs> OnInterstitialShown;
        public event EventHandler<EventArgs> OnInterstitialDismissed;
        public event EventHandler<ImpressionData> OnImpression;
        public event EventHandler<AdFailureEventArgs> OnInterstitialFailedToShow;

        public InterstitialClient(string blockId)
        {
            this.selfPointer = GCHandle.ToIntPtr(GCHandle.Alloc(this));
            this.ObjectId = InterstitialBridge.YMAUnityCreateInterstitial(
                this.selfPointer, blockId);
            InterstitialBridge.YMAUnitySetInterstitialCallbacks(
                this.ObjectId,
                InterstitialDidLoadAdCallback,
                InterstitialDidFailToLoadAdCallback,
                InterstitialWillPresentScreenCallback,
                InterstitialWillLeaveApplicationCallback,
                InterstitialDidClickCallback,
                InterstitialWillAppearCallback,
                InterstitialDidDismissCallback, 
                InterstitialDidTrackImpression, 
                InterstitialDidFailToShowCallback);
        }

        public void LoadAd(AdRequest request)
        {
            AdRequestClient adRequest = null;
            if (request != null)
            {
                adRequest = new AdRequestClient(request);
            }
            InterstitialBridge.YMAUnityLoadInterstitial(
                this.ObjectId, adRequest.ObjectId);
        }

        public bool IsLoaded()
        {
            return InterstitialBridge.YMAUnityIsInterstitialLoaded(this.ObjectId);
        }

        public void Show()
        {
            InterstitialBridge.YMAUnityShowInterstitial(this.ObjectId);
        }

        public void Destroy()
        {
            ObjectBridge.YMAUnityDestroyObject(this.ObjectId);
        }

        public void Dispose()
        {
            this.Destroy();
        }

        ~InterstitialClient()
        {
            this.Destroy();
        }

        private static InterstitialClient IntPtrToInterstitialClient(
            IntPtr interstitialClient)
        {
            GCHandle handle = GCHandle.FromIntPtr(interstitialClient);
            return handle.Target as InterstitialClient;
        }

        #region Interstitial callback methods

        [MonoPInvokeCallback(typeof(YMAUnityInterstitialDidLoadAdCallback))]
        private static void InterstitialDidLoadAdCallback(
            IntPtr interstitialClient)
        {
            InterstitialClient client = IntPtrToInterstitialClient(
                interstitialClient);
            if (client.OnInterstitialLoaded != null)
            {
                client.OnInterstitialLoaded(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityInterstitialDidFailToLoadAdCallback))]
        private static void InterstitialDidFailToLoadAdCallback(
            IntPtr interstitialClient, string error)
        {
            InterstitialClient client = IntPtrToInterstitialClient(
                interstitialClient);
            if (client.OnInterstitialFailedToLoad != null)
            {
                AdFailureEventArgs args = new AdFailureEventArgs()
                {
                    Message = error
                };
                client.OnInterstitialFailedToLoad(client, args);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityInterstitialWillPresentScreenCallback))]
        private static void InterstitialWillPresentScreenCallback(
            IntPtr interstitialClient)
        {
            InterstitialClient client = IntPtrToInterstitialClient(
                interstitialClient);
            if (client.OnLeftApplication != null)
            {
                client.OnLeftApplication(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityInterstitialWillLeaveApplicationCallback))]
        private static void InterstitialWillLeaveApplicationCallback(
            IntPtr interstitialClient)
        {
            InterstitialClient client = IntPtrToInterstitialClient(
                interstitialClient);
            if (client.OnLeftApplication != null)
            {
                client.OnLeftApplication(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityInterstitialDidClickCallback))]
        private static void InterstitialDidClickCallback(
            IntPtr interstitialClient)
        {
            InterstitialClient client = IntPtrToInterstitialClient(
                interstitialClient);
            if (client.OnAdClicked != null)
            {
                client.OnAdClicked(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityInterstitialWillAppearCallback))]
        private static void InterstitialWillAppearCallback(
            IntPtr interstitialClient)
        {
            InterstitialClient client = IntPtrToInterstitialClient(
                interstitialClient);
            if (client.OnInterstitialShown != null)
            {
                client.OnInterstitialShown(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityInterstitialDidDismissCallback))]
        private static void InterstitialDidDismissCallback(
            IntPtr interstitialClient)
        {
            InterstitialClient client = IntPtrToInterstitialClient(
                interstitialClient);
            if (client.OnInterstitialDismissed != null)
            {
                client.OnInterstitialDismissed(client, EventArgs.Empty);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityInterstitialDidTrackImpressionCallback))]
        private static void InterstitialDidTrackImpression(
            IntPtr interstitialClient, string rawImpressionData)
        {
            InterstitialClient client = IntPtrToInterstitialClient(
                interstitialClient);
            if (client.OnImpression != null)
            {

                ImpressionData impressionData = new ImpressionData(rawImpressionData == null ? "" : rawImpressionData);
                client.OnImpression(client, impressionData);
            }
        }

        [MonoPInvokeCallback(typeof(YMAUnityInterstitialDidFailToShowCallback))]
        private static void InterstitialDidFailToShowCallback(
            IntPtr interstitialClient, string error)
        {
            InterstitialClient client = IntPtrToInterstitialClient(
                interstitialClient);
            if (client.OnInterstitialFailedToShow != null)
            {
                AdFailureEventArgs args = new AdFailureEventArgs()
                {
                    Message = error
                };
                client.OnInterstitialFailedToShow(client, args);
            }
        }

        #endregion
    }

    #endif
}
