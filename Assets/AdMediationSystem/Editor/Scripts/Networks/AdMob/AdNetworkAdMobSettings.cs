using System;
using UnityEngine;
using System.Collections.Generic;

namespace Virterix.AdMediation.Editor
{
    [Serializable]
    public class AdNetworkAdMobSettings : BaseAdNetworkSettings
    {
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