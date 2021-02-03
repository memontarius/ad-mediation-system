using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Boomlagoon.JSON;

namespace Virterix.AdMediation.Editor
{
    public class AdMediationSettingsGenerator
    {
        public const string PREFAB_NAME = "AdMediationSystem.prefab";

        //-------------------------------------------------------------
        #region Helpers

        public static string GetAdProjectSettingsPath(string projectName)
        {
            string resourceFolder = "Resources";
            string resourcePath = "Assets/" + resourceFolder;
            string settingDirectoryPath = string.Format("{0}/{1}", resourcePath, AdMediationSystem._AD_SETTINGS_FOLDER);
            string projectSettingsPath = string.Format("{0}/{1}", settingDirectoryPath, projectName);
        
            if (!AssetDatabase.IsValidFolder(resourcePath))
            {
                AssetDatabase.CreateFolder("Assets", resourceFolder);
            }
            if (!AssetDatabase.IsValidFolder(settingDirectoryPath))
            {
                AssetDatabase.CreateFolder(resourcePath, AdMediationSystem._AD_SETTINGS_FOLDER);
            }
            if (!AssetDatabase.IsValidFolder(projectSettingsPath))
            {
                AssetDatabase.CreateFolder(settingDirectoryPath, projectName);
                AssetDatabase.Refresh();
            }
            return projectSettingsPath;
        }

        private static void FillMediators(ref List<AdUnitMediator> mediators, List<AdUnitMediator> specificMediators)
        {
            if (specificMediators.Count > 0)
            {
                mediators.AddRange(specificMediators);
            }
        }

        #endregion // Helpers

        public static string Generate(string projectName, AdMediationProjectSettings projectSettings, BaseAdNetworkSettings[] networkSettings)
        {
            List<AdUnitMediator> mediators = new List<AdUnitMediator>();
            FillMediators(ref mediators, projectSettings._bannerMediators);
            FillMediators(ref mediators, projectSettings._interstitialMediators);
            FillMediators(ref mediators, projectSettings._incentivizedMediators);

            // Json Settings
            JSONObject json = new JSONObject();
            json.Add("projectName", projectName);
            json.Add("networkResponseWaitTime", 30);
            json.Add("mediators", CreateMediators(projectName, projectSettings, mediators));
            json.Add("networks", CreateNetworks(networkSettings));

            // System Prefab
            GameObject systemObject = CreateSystemObject(projectName, projectSettings, mediators);
            SavePrefab(systemObject);
            GameObject.DestroyImmediate(systemObject);

            return json.ToString();
        }

        //-------------------------------------------------------------
        #region Json Settings

        private static JSONArray CreateMediators(string projectName, AdMediationProjectSettings settings, List<AdUnitMediator> mediators)
        {
            JSONArray jsonMediators = new JSONArray();
            foreach (AdUnitMediator mediator in mediators)
            {
                JSONObject jsonMediator = new JSONObject();
                jsonMediator.Add("adType", AdTypeConvert.AdTypeToString(mediator._adType));
                jsonMediator.Add("strategy", CreateStrategy(mediator));
                jsonMediators.Add(jsonMediator);
            }
            return jsonMediators;
        }

        private static JSONObject CreateStrategy(AdUnitMediator mediator)
        {
            JSONObject mediationStrategy = new JSONObject();
            mediationStrategy.Add("type", mediator._fetchStrategyType.ToString().ToLower());

            return mediationStrategy;
        }

        private static JSONArray CreateNetworks(BaseAdNetworkSettings[] networkSettings)
        {
            JSONArray jsonNetworks = new JSONArray();
            foreach(var settings in networkSettings)
            {
                if (settings._enabled)
                {
                    JSONObject jsonNetwork = new JSONObject();
                    //jsonNetwork.Add("name", network.Identifier);

                    jsonNetworks.Add(jsonNetwork);
                }
            }
            return jsonNetworks;
        }

        #endregion // Json Settings

        //-------------------------------------------------------------
        #region System Prefab

        private static GameObject CreateSystemObject(string projectName, AdMediationProjectSettings settings, List<AdUnitMediator> mediators)
        {
            GameObject mediationSystemObject = new GameObject(PREFAB_NAME);
            AdMediationSystem adSystem = mediationSystemObject.AddComponent<AdMediationSystem>();
            adSystem.m_projectName = projectName;
            adSystem.m_isLoadOnlyDefaultSettings = true;
            adSystem.m_isInitializeOnStart = settings._initializeOnStart;
            adSystem.m_isPersonalizeAdsOnInit = settings._personalizeAdsOnInit;

            GameObject networkAdapterHolder = new GameObject("NetworkAdapters");
            networkAdapterHolder.transform.SetParent(mediationSystemObject.transform);

            GameObject mediatorHolder = new GameObject("Mediators");
            mediatorHolder.transform.SetParent(mediationSystemObject.transform);

            GameObject bannerMediatorHolder = new GameObject("Banner");
            bannerMediatorHolder.transform.SetParent(mediatorHolder.transform);
            GameObject interstitialMediatorHolder = new GameObject("Interstitial");
            interstitialMediatorHolder.transform.SetParent(mediatorHolder.transform);
            GameObject incentivizedMediatorHolder = new GameObject("Incentivized");
            incentivizedMediatorHolder.transform.SetParent(mediatorHolder.transform);

            return mediationSystemObject;
        }

        private static void SavePrefab(GameObject mediationSystemObject)
        {
            string settingsPath = GetAdProjectSettingsPath(mediationSystemObject.GetComponent<AdMediationSystem>().m_projectName);
            PrefabUtility.SaveAsPrefabAsset(mediationSystemObject, settingsPath + "/" + PREFAB_NAME);
            AssetDatabase.Refresh();
        }

        #endregion // System Prefab
    }
} // namespace Virterix.AdMediation.Editor
