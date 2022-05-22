using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class CBCleanup : AssetPostprocessor
{
	private static string[] filesToRemove = {
		"Editor/CBAndroidSetupUI.cs",
		"Plugins/Chartboost/CBBinding.cs",
		"Plugins/Chartboost/CBJSON.cs",
		"Plugins/Chartboost/CBManager.cs",
		"Plugins/Chartboost/demo/CBEventListener.cs",
		"Plugins/Chartboost/demo/CBUIManager.cs",
		"Plugins/Chartboost/demo/CBTestScene.unity",
		"Plugins/Android/ChartboostSDK/",
		"Plugins/iOS/CBAnalytics.h",
		"Plugins/iOS/CBInPlay.h",
		"Plugins/iOS/Chartboost.h",
		"Plugins/iOS/ChartboostDelegate.h",
		"Plugins/iOS/Chartboost.h",
		"Plugins/iOS/CHBAd.h",
		"Plugins/iOS/CHBAdDelegate.h",
		"Plugins/iOS/CHBBanner.h",
		"Plugins/iOS/CHBDataUseConsent.h",
		"Plugins/iOS/CHBInterstitial.h",
		"Plugins/iOS/CHBRewarded.h",
		"Plugins/iOS/libChartboost.a"
	};

	public static void Clean()
	{
		foreach(string fileName in filesToRemove) {
			if(File.Exists(System.IO.Path.Combine(Application.dataPath, fileName))) {
				AssetDatabase.DeleteAsset(System.IO.Path.Combine("Assets", fileName));
				Debug.Log("Removed legacy Chartboost file: " + fileName);
			}
		}

		AssetDatabase.Refresh();
	}
}
