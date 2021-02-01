using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boomlagoon.JSON;

namespace Virterix.AdMediation.Editor
{
    public class AdMediationSettingsGenerator
    {
        public static string Generate(string projectName, List<AdMediatorView> mediatorViews)
        {
            JSONObject json = new JSONObject();
            json.Add("projectName", projectName);
            json.Add("networkResponseWaitTime", 30);
            json.Add("mediators", CreateMediators(mediatorViews));
            return json.ToString();
        }

        private static JSONArray CreateMediators(List<AdMediatorView> mediatorViews)
        {
            JSONArray jsonMediators = new JSONArray();
            foreach (AdMediatorView mediatorView in mediatorViews)
            {
                JSONObject jsonMediator = new JSONObject();
                jsonMediator.Add("adType", AdTypeConvert.AdTypeToString(mediatorView.AdType));
                jsonMediator.Add("strategy", CreateStrategy(mediatorView));
                jsonMediators.Add(jsonMediator);
            }
            return jsonMediators;
        }

        private static JSONObject CreateStrategy(AdMediatorView mediatorView)
        {
            JSONObject mediationStrategy = new JSONObject();
            mediationStrategy.Add("type", mediatorView.StrategyType.ToString().ToLower());




            return mediationStrategy;
        }
    }
} // namespace Virterix.AdMediation.Editor
