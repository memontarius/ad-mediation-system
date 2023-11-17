using System;
using UnityEngine;
using UnityEngine.UI;
using Virterix.AdMediation;

public class ExampleAdController : BaseAdController
{
    public Text m_interstitialInfoText;
    public Text m_honeInterstitialInfoText;
    public Text m_rewardVideoInfoText;
    public Text m_bannerInfoText;
    public Text m_bannerTopInfoText;
    public Text m_eventLogText;
    public Text m_adInterstitialCountText;
    public Text m_adPersonalizedText;

    private int m_adInterstitialCount;
    private PollfishAdapter m_pollfishNetwork;

    // Use this for initialization
    private void Awake()
    {
        AdMediationSystem.OnInitialized += OnMediationSystemInitialized;
        AdMediationSystem.OnAllNetworksInitializeResponseReceived += OnAllNetworksInitializeResponseReceived;
        AdMediationSystem.Load("DefaultProject");
        m_adPersonalizedText.rectTransform.parent.GetComponent<Button>().interactable = false;
    }

    private void OnMediationSystemInitialized()
    {
        m_adPersonalizedText.rectTransform.parent.GetComponent<Button>().interactable = true;
        UpdatePersonalizedAdsButtonText();

        m_pollfishNetwork = AdMediationSystem.Instance.GetNetwork<PollfishAdapter>();
        if (m_pollfishNetwork != null) {
            m_pollfishNetwork.OnEvent += OnAdPollfishNetworkEvent;
        }
    }

    private void OnAllNetworksInitializeResponseReceived()
    {
        //FetchAllAds();
        Debug.Log("AllNetworksInitializeResponseReceived");
    }

    public void FetchAllAds()
    {
        AdMediationSystem.Fetch(AdType.Interstitial);
        AdMediationSystem.Fetch(AdType.Incentivized);
        AdMediationSystem.Fetch(AdType.Banner);
        AdMediationSystem.Fetch(AdType.Banner, "Top");
    }

    public void HideAllBanners()
    {
        AdMediationSystem.HideAllBanners();
    }

    public void ToggleNonRewardAdsDisabledStatus(Text text)
    {
        AdMediationSystem.NonRewardAdsDisabled = !AdMediationSystem.NonRewardAdsDisabled;
        text.text = AdMediationSystem.NonRewardAdsDisabled
            ? "Toggle To Enabled Non-Reward Ads"
            : "Toggle To Disabled Non-Reward Ads";
    }

    public void ShowGoogleConsentForm()
    {
        AdMobAdapter adMobNetwork = AdMediationSystem.Instance.GetNetwork<AdMobAdapter>();
        adMobNetwork.ShowConsentOptionsForm();
    }

    public void ResetGoogleConsentInformation()
    {
        AdMobAdapter adMobNetwork = AdMediationSystem.Instance.GetNetwork<AdMobAdapter>();
        adMobNetwork.ResetConsentInformation();
    }

    public void FetchInterstitial(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
    {
        AdMediationSystem.Fetch(AdType.Interstitial, placement);
        UpdateAdInfo(null, AdType.Interstitial, placement);
    }

    public void FetchRewardVideo(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
    {
        AdMediationSystem.Fetch(AdType.Incentivized, placement);
        UpdateAdInfo(null, AdType.Incentivized, placement);
    }

    public void FetchBanner(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
    {
        AdMediationSystem.Fetch(AdType.Banner, placement);
        UpdateAdInfo(null, AdType.Banner, placement);
    }

    public void ShowInterstitial(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
    {
        AdMediationSystem.Show(AdType.Interstitial, placement);
    }

    public void ShowRewardVideo(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
    {
        AdMediationSystem.Show(AdType.Incentivized, placement);
    }

    public void ShowBanner(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
    {
        AdMediationSystem.Show(AdType.Banner, placement);
    }

    public void HideBanner(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME)
    {
        AdMediationSystem.Hide(AdType.Banner, placement);
    }

    public void PrepareSurvey()
    {
        if (m_pollfishNetwork != null) {
            m_pollfishNetwork.Prepare(AdType.Incentivized);
        }
    }

    public void ShowSurvey()
    {
        if (m_pollfishNetwork != null) {
            m_pollfishNetwork.Show(AdType.Incentivized);
        }
    }

    public void HideSurvey()
    {
        if (m_pollfishNetwork != null) {
            m_pollfishNetwork.Hide(AdType.Incentivized);
        }
    }

    public void ToggleChangePersonalizedAds()
    {
        if (AdMediationSystem.InitStatus == InitializedStatus.Initialized) {
            PersonalisationConsent consent = AdMediationSystem.UserPersonalisationConsent;
            if (consent == PersonalisationConsent.Accepted || consent == PersonalisationConsent.Undefined)
                consent = PersonalisationConsent.Denied;
            else
                consent = PersonalisationConsent.Accepted;
            AdMediationSystem.SetUserConsentToPersonalizedAds(consent);
            UpdatePersonalizedAdsButtonText();
        }
    }

    void UpdatePersonalizedAdsButtonText()
    {
        string buttonText = "";
        if (AdMediationSystem.UserPersonalisationConsent == PersonalisationConsent.Accepted ||
            AdMediationSystem.UserPersonalisationConsent == PersonalisationConsent.Undefined)
            buttonText = "Change to Non-Personalized Ads";
        else
            buttonText = "Change to Personalized Ads ";
        m_adPersonalizedText.text = buttonText;
    }

    void OnAdPollfishNetworkEvent(AdNetworkAdapter network, AdType adType, AdEvent adEvent, AdInstance adInstance)
    {
    }

    protected override void HandleNetworkEvent(AdMediator mediator, AdNetworkAdapter network, AdType adType,
        AdEvent adEvent, string adInstanceName)
    {
        string placementName = mediator == null ? "" : mediator.m_placementName;

        UpdateAdInfo(mediator, adType, placementName);

        string log = string.Format(
            "<i>{0}</i> - Placement: <color=blue>{1}</color> Instance: <color=blue>{2}</color> <b>{3}</b> {4} \n{5}",
            adType.ToString(), placementName, adInstanceName, network.m_networkName, adEvent.ToString(),
            m_eventLogText.text);

        m_eventLogText.text = log;

        if (adEvent == AdEvent.Showing && network.m_networkName != "pollfish" &&
            (adType == AdType.Interstitial || adType == AdType.Incentivized)) {
            m_adInterstitialCount++;
        }

        m_adInterstitialCountText.text = m_adInterstitialCount.ToString();
    }

    void UpdateAdInfo(AdMediator mediator, AdType adType, string placement)
    {
        if (mediator == null)
            mediator = AdMediationSystem.Instance.GetMediator(adType, placement);

        Text guiText = null;
        switch (adType) {
            case AdType.Interstitial:
                if (placement == AdMediationSystem.PLACEMENT_DEFAULT_NAME)
                    guiText = m_interstitialInfoText;
                else
                    guiText = m_honeInterstitialInfoText;
                break;
            case AdType.Incentivized:
                guiText = m_rewardVideoInfoText;
                break;
            case AdType.Banner:
                if (placement == AdMediationSystem.PLACEMENT_DEFAULT_NAME)
                    guiText = m_bannerInfoText;
                else if (placement == "Top")
                    guiText = m_bannerTopInfoText;
                break;
        }

        string networkName = "";
        bool isReady = false;

        if (mediator != null) {
            networkName = mediator.CurrentNetworkName;
            isReady = mediator.IsCurrentNetworkReadyToShow;
        }

        if (guiText != null) {
            guiText.text = "network: " + networkName + "\n" +
                           "ready: " + isReady.ToString() + "\n" +
                           "ready mediator: " + mediator?.IsReadyToShow.ToString();
        }
    }

    bool[] BannerDisplayStates
    {
        get
        {
            if (_bannerDisplayStates == null) {
                var bannerMediators = AdMediationSystem.Instance.GetAllMediators(AdType.Banner);
                _bannerDisplayStates = new bool[bannerMediators.Length];
            }

            return _bannerDisplayStates;
        }
    }

    bool[] _bannerDisplayStates;

    public void HideBanners()
    {
        int index = 0;
        var bannerMediators = AdMediationSystem.Instance.GetAllMediators(AdType.Banner);
        foreach (AdMediator bannerMediator in bannerMediators) {
            if (bannerMediator.CurrentUnit.AdNetwork.UseSingleBannerInstance) {
                var placement = bannerMediator.CurrentUnit.AdNetwork.CurrBannerPlacement;
                BannerDisplayStates[index] = (placement == bannerMediator.m_placementName)
                    ? bannerMediator.IsBannerDisplayed
                    : false;
            }
            else
                BannerDisplayStates[index] = bannerMediator.IsBannerDisplayed;

            index++;
            bannerMediator.Hide();
        }
    }

    public void RestoreBanners()
    {
        int index = 0;
        var bannerMediators = AdMediationSystem.Instance.GetAllMediators(AdType.Banner);
        foreach (AdMediator bannerMediator in bannerMediators) {
            if (BannerDisplayStates[index++])
                bannerMediator.Show();
        }
    }
}