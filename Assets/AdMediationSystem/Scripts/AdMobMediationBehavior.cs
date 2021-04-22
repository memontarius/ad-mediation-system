//#define _ADMOB_MEDIATION

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if _ADMOB_MEDIATION
using GoogleMobileAds.Api;
using GoogleMobileAds.Api.Mediation.AppLovin;
using GoogleMobileAds.Api.Mediation.Chartboost;
using GoogleMobileAds.Api.Mediation.AdColony;
using GoogleMobileAds.Api.Mediation.UnityAds;
#endif

namespace Virterix.AdMediation
{
    public class AdMobMediationBehavior : MonoBehaviour
    {
        
    }
}
