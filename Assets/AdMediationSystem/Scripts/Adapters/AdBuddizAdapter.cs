﻿
//#define _MS_ABUDDIZ

using UnityEngine;
using System.Collections;

namespace Virterix.AdMediation
{
    public class AdBuddizAdapter : AdNetworkAdapter
    {

        public bool m_isTestMode;
        public string m_defaultAndroidPublisherKey;
        public string m_defaultIOSPublisherKey;

#if _MS_ABUDDIZ
        protected override void InitializeParameters(System.Collections.Generic.Dictionary<string, string> parameters)
        {
            base.InitializeParameters(parameters);

            // Listen to AdBuddiz events
            AdBuddizManager.didFailToShowAd += OnDidFailToShowAd;
            AdBuddizManager.didCacheAd += OnDidCacheAd;
            AdBuddizManager.didShowAd += OnDidShowAd;
            AdBuddizManager.didClick += OnDidClick;
            AdBuddizManager.didHideAd += OnDidHideAd;

            AdBuddizRewardedVideoManager.didFetch += OnRewardedVideoDidFetch;
            AdBuddizRewardedVideoManager.didNotComplete += OnRewardedVideoDidNotComplete;
            AdBuddizRewardedVideoManager.didComplete += OnRewardedVideoDidComplete;

            string publisherKey = "";

            if (parameters != null)
            {
                publisherKey = parameters["publisherKey"];
            }
            else
            {
#if UNITY_ANDROID
                publisherKey = m_defaultAndroidPublisherKey;
#elif UNITY_IOS
                    publisherKey = m_defaultIOSPublisherKey;
#endif
            }

            AdBuddizBinding.SetLogLevel(AdBuddizBinding.ABLogLevel.Error);

            if (m_isTestMode)
            {
                AdBuddizBinding.SetTestModeActive();
            }
#if UNITY_ANDROID
            AdBuddizBinding.SetAndroidPublisherKey(publisherKey);
#elif UNITY_IOS
                  AdBuddizBinding.SetIOSPublisherKey(publisherKey);
#endif
        }

        public override void Prepare(AdType adType)
        {
            if (!IsReady(adType))
            {
                if (adType == AdType.Interstitial)
                {
                    AdBuddizBinding.CacheAds();
                }
                else
                {
                    AdBuddizBinding.RewardedVideo.Fetch();
                }
            }
        }

        public override bool Show(AdType adType)
        {
            if (IsReady(adType))
            {
                switch (adType)
                {
                    case AdType.Interstitial:
                        AdBuddizBinding.ShowAd();
                        break;
                    case AdType.Incentivized:
                        AdBuddizBinding.RewardedVideo.Show();
                        AddEvent(AdType.Incentivized, AdEvent.Show);
                        break;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Hide(AdType adType)
        {
        }

        public override bool IsReady(AdType adType)
        {
            if (IsSupported(adType))
            {
                switch (adType)
                {
                    case AdType.Interstitial:
                        return AdBuddizBinding.IsReadyToShowAd();
                        break;
                    case AdType.Incentivized:
                        return AdBuddizBinding.RewardedVideo.IsReadyToShow();
                        break;
                }
            }
            return false;
        }

        private void OnDidFailToShowAd(string adBuddizError)
        {
            //AddEvent(AdType.Interstitial, AdEvent.PrepareFailure);
        }

        private void OnDidCacheAd()
        {
            //AddEvent(AdType.Interstitial, AdEvent.Prepared);
        }

        private void OnDidShowAd()
        {
            AddEvent(AdType.Interstitial, AdEvent.Show);
        }

        private void OnDidClick()
        {
            AddEvent(AdType.Interstitial, AdEvent.Click);
        }

        private void OnDidHideAd()
        {
            AddEvent(AdType.Interstitial, AdEvent.Hide);
        }

        private void OnRewardedVideoDidFetch()
        {
            AddEvent(AdType.Incentivized, AdEvent.Prepared);
        }

        private void OnRewardedVideoDidNotComplete()
        {
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedIncomplete);
            AddEvent(AdType.Incentivized, AdEvent.Hide);
        }

        private void OnRewardedVideoDidComplete()
        {
            AddEvent(AdType.Incentivized, AdEvent.IncentivizedComplete);
            AddEvent(AdType.Incentivized, AdEvent.Hide);
        }

#endif // _MS_ABUDDIZ

    }
} // namespace Virterix.AdMediation