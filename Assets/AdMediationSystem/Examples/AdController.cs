using UnityEngine;
using UnityEngine.UI;
using Virterix.AdMediation;

public class AdController : BaseAdController
{
    public Text m_interstitialInfoText;
    public Text m_rewardVideoInfoText;
    public Text m_bannerInfoText;
    public Text m_bannerTopInfoText;
    public Text m_eventLogText;
    public Text m_adInterstitialCountText;
    public Text m_adPersonalizedText;
 
    private int m_adInterstitialCount;
    private PollfishAdapter m_pollfishNetwork;

    // Use this for initialization
    void Awake() {
        AdMediationSystem.OnInitialized += OnMediationSystemInitialized;
        AdMediationSystem.Load("DefaultProject");
        m_adPersonalizedText.rectTransform.parent.GetComponent<Button>().interactable = false;
    }

    private void OnMediationSystemInitialized() {
        m_adPersonalizedText.rectTransform.parent.GetComponent<Button>().interactable = true;
        UpdatePersonalizedAdsButtonText();

        m_pollfishNetwork = AdMediationSystem.Instance.GetNetwork("pollfish") as PollfishAdapter;
        if (m_pollfishNetwork != null) {
            m_pollfishNetwork.OnEvent += OnAdPollfishNetworkEvent;
        }
    }

    private void FetchAllAds()
    {
        AdMediationSystem.Fetch(AdType.Interstitial);
        AdMediationSystem.Fetch(AdType.Incentivized);
        AdMediationSystem.Fetch(AdType.Banner);
        AdMediationSystem.Fetch(AdType.Banner, "Top");
    }

    public void FetchInterstitial() {
        AdMediationSystem.Fetch(AdType.Interstitial);
        UpdateAdInfo(null, AdType.Interstitial, AdMediationSystem.PLACEMENT_DEFAULT_NAME);
    }

    public void FetchRewardVideo() {
        AdMediationSystem.Fetch(AdType.Incentivized);
        UpdateAdInfo(null, AdType.Incentivized, AdMediationSystem.PLACEMENT_DEFAULT_NAME);
    }

    public void FetchBanner(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME) {
        AdMediationSystem.Fetch(AdType.Banner, placement);
        UpdateAdInfo(null, AdType.Banner, placement);
    }

    public void ShowInterstitial() {
        AdMediationSystem.Show(AdType.Interstitial);
    }

    public void ShowRewardVideo() {
        AdMediationSystem.Show(AdType.Incentivized);
    }

    public void ShowBanner(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME) {
        AdMediationSystem.Show(AdType.Banner, placement);
    }

    public void HideBanner(string placement = AdMediationSystem.PLACEMENT_DEFAULT_NAME) {
        AdMediationSystem.Hide(AdType.Banner, placement);
    }

    public void PrepareSurvey() {
        if (m_pollfishNetwork != null) {
            m_pollfishNetwork.Prepare(AdType.Incentivized);
        }
    }

    public void ShowSurvey() {
        if (m_pollfishNetwork != null) {
            m_pollfishNetwork.Show(AdType.Incentivized);
        }
    }

    public void HideSurvey() {
        if (m_pollfishNetwork != null) {
            m_pollfishNetwork.Hide(AdType.Incentivized);
        }
    }

    public void ToggleChangePersonalizedAds() {
        if (AdMediationSystem.IsInitialized) {
            AdMediationSystem.SetPersonalizedAds(!AdMediationSystem.IsAdsPersonalized);
            UpdatePersonalizedAdsButtonText();
        }
    }

    void UpdatePersonalizedAdsButtonText() {
        string buttonText = "";
        if (AdMediationSystem.IsAdsPersonalized) {
            buttonText = "Change to Non-Personalized Ads";
        }
        else {
            buttonText = "Change to Personalized Ads ";
        }
        m_adPersonalizedText.text = buttonText;
    }

    void OnAdPollfishNetworkEvent(AdNetworkAdapter network, AdType adType, AdEvent adEvent, AdInstance adInstance) {
        HandleNetworkEvent(null, network, adType, adEvent, adInstance == null ? "" : adInstance.Name);
    }

    protected override void HandleNetworkEvent(AdMediator mediator, AdNetworkAdapter network, AdType adType, AdEvent adEvent, string adInstanceName)
    {
        string placementName = mediator == null ? "" : mediator.m_placementName;

        if (network.m_networkName != "pollfish")
        {
            UpdateAdInfo(mediator, adType, placementName);
        }

        string log = string.Format("{0} Placement: <color=blue>{1}</color> Instance: <color=blue>{2}</color> <b>{3}</b> {4} \n{5}",
            adType.ToString(), placementName, adInstanceName, network.m_networkName, adEvent.ToString(), m_eventLogText.text);

        m_eventLogText.text = log;

        if (adEvent == AdEvent.Show && network.m_networkName != "pollfish" &&
            (adType == AdType.Interstitial || adType == AdType.Incentivized))
        {
            m_adInterstitialCount++;
        }
        m_adInterstitialCountText.text = m_adInterstitialCount.ToString();
    }

    void UpdateAdInfo(AdMediator mediator, AdType adType, string placement) {
        if (mediator == null)
        {
            mediator = AdMediationSystem.Instance.GetMediator(adType, placement);
        }

        Text guiText = null;
        switch(adType) {
            case AdType.Interstitial:
                guiText = m_interstitialInfoText;
                break;
            case AdType.Incentivized:
                guiText = m_rewardVideoInfoText;
                break;
            case AdType.Banner:
                if (placement == AdMediationSystem.PLACEMENT_DEFAULT_NAME) {
                    guiText = m_bannerInfoText;
                }
                else if (placement == "Top") {
                    guiText = m_bannerTopInfoText;
                }
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

}

