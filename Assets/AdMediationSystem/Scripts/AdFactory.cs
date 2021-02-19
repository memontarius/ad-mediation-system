using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Virterix.AdMediation
{
    public class AdFactory
    {
        public static IFetchStrategy CreateFetchStrategy(string strategyTypeName)
        {
            IFetchStrategy fetchStrategy = null;
            switch (strategyTypeName)
            {
                case "random":
                    fetchStrategy = new RandomFetchStrategy();
                    break;
                case "sequence":
                    fetchStrategy = new SequenceFetchStrategy();
                    break;
            }
            return fetchStrategy;
        }

        public static BaseFetchStrategyParams CreateFetchStrategyParams(string strategyTypeName, Dictionary<string, object> networkParams)
        {
            BaseFetchStrategyParams fetchStrategyParams = null;

            switch (strategyTypeName)
            {
                case "random":
                    fetchStrategyParams = new RandomFetchStrategy.RandomStrategyParams();
                    try
                    {
                        RandomFetchStrategy.SetupParameters(ref fetchStrategyParams, networkParams);
                    }
                    catch
                    {
                        Debug.LogWarning("AdFactory: not found key in network parameter dictionary");
                    }
                    break;
                case "sequence":
                    fetchStrategyParams = new SequenceFetchStrategy.SequenceStrategyParams();
                    try
                    {
                        SequenceFetchStrategy.SetupParameters(ref fetchStrategyParams, networkParams);
                    }
                    catch
                    {
                        Debug.LogWarning("AdFactory: not found key in network parameter dictionary");
                    }
                    break;
            }
            return fetchStrategyParams;
        }

        public static AdInstance CreateAdInstacne(AdType adType, string instanceName = AdInstance.AD_INSTANCE_DEFAULT_NAME, 
            string adId = "", float timeout = 0.0f)
        {
            AdInstance adInstance = new AdInstance(adType, adId, instanceName);
            if (timeout > 0.0001f)
            {
                AdNetworkAdapter.TimeoutParams timeoutParameters = new AdNetworkAdapter.TimeoutParams();
                timeoutParameters.m_timeout = timeout;
                timeoutParameters.m_adType = adInstance.m_adType;
                adInstance.m_timeout = timeoutParameters;
            }
            adInstance.m_responseWaitTime = AdMediationSystem.Instance.DefaultNetworkResponseWaitTime;
            return adInstance;
        }
    }
} // namespace Virterix.AdMediation
