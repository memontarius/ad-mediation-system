using System.Collections.Generic;
using UnityEngine;

namespace Virterix.AdMediation.Editor
{
    public struct BannerPositionContainer
    {
        public string m_placementName;
        public BannerPosition m_bannerPosition;
    }

    public struct AdInstanceGenerateDataContainer 
    {
        public AdInstance _adInstance;
        public AdType _adType;
    }

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
        public string _networkIdentifier;
        public string _androidAppId;
        public string _iosAppId;
        public int _responseWaitTime;
        public List<AdInstance> _bannerAdInstances = new List<AdInstance>();
        public List<AdInstance> _interstitialAdInstances = new List<AdInstance>();
        public List<AdInstance> _rewardAdInstances = new List<AdInstance>();

        public virtual bool IsAdInstanceSupported
        {
            get;
        } = true;

        public virtual System.Type NetworkAdapterType
        {
            get;
        }

        public virtual Dictionary<string, object> GetSpecificNetworkParameters()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            return parameters;
        }

        public virtual AdInstanceParameters CreateBannerAdInstanceParameters(string projectNme, string name, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            return null;
        }

        public virtual void SetupNetworkAdapter(Component networkAdapter)
        {
        }

        public virtual bool IsAdSupported(AdType adType)
        {
            return false;
        }


        public AdInstanceGenerateDataContainer[] GetAllAdInstanceDataHolders()
        {
            int adInstancesCount = _bannerAdInstances.Count + _interstitialAdInstances.Count + _rewardAdInstances.Count;
            AdInstanceGenerateDataContainer[] adInstanceDataHolders = new AdInstanceGenerateDataContainer[adInstancesCount];
            int adInstanceIndex = 0;
            FillAdInstances(ref adInstanceDataHolders, ref adInstanceIndex, _bannerAdInstances, AdType.Banner);
            FillAdInstances(ref adInstanceDataHolders, ref adInstanceIndex, _interstitialAdInstances, AdType.Interstitial);
            FillAdInstances(ref adInstanceDataHolders, ref adInstanceIndex, _rewardAdInstances, AdType.Incentivized);
            return adInstanceDataHolders;
        }

        private void FillAdInstances(ref AdInstanceGenerateDataContainer[] adInstanceDataHolders, ref int index, List<AdInstance> _specificAdInstances, AdType adType)
        {
            foreach(var adInstance in _specificAdInstances)
            {
                AdInstanceGenerateDataContainer adInstanceDataHolder = new AdInstanceGenerateDataContainer();
                adInstanceDataHolder._adInstance = adInstance;
                adInstanceDataHolder._adType = adType;
                adInstanceDataHolders[index++] = adInstanceDataHolder;  
            }
        }
    }
} // namespace Virterix.AdMediation.Editor