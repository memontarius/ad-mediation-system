Ad parameters in settings file at path Resources/AdSettings/YourProjectName/platform_settings.json

Ad unit:
network - (string) advertising network name witch is listed in AdNetworkAdapter prefab
impressions - (int) number of impressions
networkWaitingResponseTime - (int) waiting response time of network after the expiration of which next fetch will be called
impressionsInSession - (int) nuber of impressins in session

RandomFetchStrategy:
percentage - (int) show the percentage of probability (range 0-100)

SequenceFetchStrategy:
skipFetchIndex - (int) index fetch when an ad unit skip
replaceableNetworks - (string) Network names separated by comma. If the network with such name wasn't prepared or disabled then this unit will be next

Network:
timeout-banner, timeout-interstitial, timeout-incentivized, timeout-video, timeout-native - time in seconds of timeout when failed loading ads.
Don't forget enable useTimeout flag in mediation prefab.


After update ads plugins:

1. Comment in Chartboost.cs all changes Time.timeScale in method doUnityPause

2. After updated GooglePlayGames (tested on v0.9.64, 0.10.04) plugin insert following code in script Assets/GooglePlayGames/Editor/GPGSUpgrader.cs:114
if (EditorApplication.isPlayingOrWillChangePlaymode)
    return;

