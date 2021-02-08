using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Virterix.AdMediation.Editor
{
    [Serializable]
    public class AdMobSettings : BaseAdNetworkSettings
    {
        public override Type NetworkAdapterType => typeof(AdMobAdapter);

        public override bool IsAdSupported(AdType adType)
        {
            return true;
        }

        public override bool IsCheckAvailabilityWhenPreparing(AdType adType)
        {
            return false;
        }

        public override void SetupNetworkAdapter(Component networkAdapter)
        {
            var adapter = networkAdapter as AdMobAdapter;
        }

        public override void SetupNetworkAdapterScript()
        {
            string adapterPath = string.Format("{0}/{1}", Application.dataPath, "AdMediationSystem/Scripts/Adapters/AdMobAdapter.cs");

            Debug.Log(adapterPath);

            string content = File.ReadAllText(adapterPath);
            if (content.Length > 0)
            {
                string define = "#define _MS_ADMOB";
                string undefine = "//#define _MS_ADMOB";

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

        public override AdInstanceParameters CreateBannerAdInstanceParameters(string projectNme, string name, int bannerType, BannerPositionContainer[] bannerPositions)
        {
            AdMobAdInstanceBannerParameters parameters = AdMobAdInstanceBannerParameters.CreateParameters(projectNme, name);
            parameters.Name = name;
            parameters.m_bannerSize = (AdMobAdapter.AdMobBannerSize)bannerType;

            var specificBannerPositions = new AdMobAdInstanceBannerParameters.BannerPosition[bannerPositions.Length];
            for(int i = 0; i < specificBannerPositions.Length; i++)
            {
                var specificPosition = new AdMobAdInstanceBannerParameters.BannerPosition();
                specificPosition.m_placementName = bannerPositions[i].m_placementName;

                switch (bannerPositions[i].m_bannerPosition)
                {
                    case BannerPosition.Bottom:
                        specificPosition.m_bannerPosition = AdMobAdapter.AdMobBannerPosition.Bottom;
                        break;
                    case BannerPosition.Top:
                        specificPosition.m_bannerPosition = AdMobAdapter.AdMobBannerPosition.Top;
                        break;
                }

                specificBannerPositions[i] = specificPosition;
            }
            parameters.m_bannerPositions = specificBannerPositions;

            return parameters;
        }
    }
} // namespace Virterix.AdMediation.Editor