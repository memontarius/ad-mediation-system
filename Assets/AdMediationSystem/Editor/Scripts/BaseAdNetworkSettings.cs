using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace Virterix.AdMediation.Editor
{
    public enum BannerPosition
    {
        Top,
        Bottom
    }

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

        public virtual System.Type NetworkAdapterType
        {
            get;
        }

        public bool IsTotallyAdInstanceUnsupported
        {
            get
            {
                bool isTotallyUnsupported = true;
                AdType[] adTypes = System.Enum.GetValues(typeof(AdType)) as AdType[];
                foreach(var adType in adTypes)
                {
                    if (adType != AdType.Unknown && IsAdInstanceSupported(adType))
                    {
                        isTotallyUnsupported = false;
                        break;
                    }
                }
                return isTotallyUnsupported;
            }
        }

        protected virtual string AdapterScriptName { get; }
        protected virtual string AdapterDefinePeprocessorKey { get; }
        public virtual string JsonAppIdKey { get { return "appId"; } }
        public virtual bool IsAppIdSupported { get; } = true;

        public virtual Dictionary<string, object> GetSpecificNetworkParameters(AppPlatform platform)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            return parameters;
        }

        public virtual AdInstanceParameters CreateBannerAdInstanceParameters(string projectName, string name, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            return null;
        }

        public virtual void SetupNetworkAdapter(Component networkAdapter)
        {
        }

        public void SetupNetworkAdapterScript()
        {
            string adapterPath = string.Format("{0}/{1}/{2}.cs", Application.dataPath, "AdMediationSystem/Scripts/Adapters", AdapterScriptName);

            string content = File.ReadAllText(adapterPath);
            if (content.Length > 0)
            {
                string define = "#define " + AdapterDefinePeprocessorKey;
                string undefine = "//#define " + AdapterDefinePeprocessorKey;

                if (_enabled)
                {
                    content = content.Replace(undefine, define);
                }
                else
                {
                    if (!content.Contains(undefine))
                    {
                        content = content.Replace(define, undefine);
                    }
                }
                File.WriteAllText(adapterPath, content);
            }
        }

        public virtual bool IsAdSupported(AdType adType)
        {
            return false;
        }

        public virtual bool IsAdInstanceSupported(AdType adType)
        {
            return true;
        } 

        public virtual bool IsCheckAvailabilityWhenPreparing(AdType adType)
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