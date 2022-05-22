using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
//using UnityEditor.iOS.Xcode;

namespace ChartboostSDK {
	public class ChartboostPostProcess : MonoBehaviour {

		[PostProcessBuild(-10)]
		public static void OnPostProcessBuildCleanup(BuildTarget target, string path)
		{
			CBSettings.resetSettings();
		}

		[PostProcessBuild(5000)]
		public static void OnPostProcessBuild(BuildTarget target, string path)
		{
			#if UNITY_5 || UNITY_2017_1_OR_NEWER
			if(target == BuildTarget.iOS) {
				PostProcessBuild_iOS(path);
			}
			#else
			// UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
			if(target == BuildTarget.iOS && !EditorUserBuildSettings.appendProject) {
				PostProcessBuild_iOS(path);
			}
			#endif
			else if(target == BuildTarget.Android) {
				PostProcessBuild_Android(path);
			}
		}

		private static void PostProcessBuild_iOS(string path)
		{
			//ProcessXCodeProject(path);
		}

		private static void PostProcessBuild_Android(string path)
		{
			Debug.Log("PostProcessBuild_Android()");
			// Check for manifest issues
			if (!CBManifest.CheckAndFixManifest())
			{
				// If something is wrong with the Android Manifest, try to regenerate it to fix it for the next build.
				CBManifest.GenerateManifest();
			}
		}

		// private static void ProcessXCodeProject(string path)
		// {
		// 	//UnityEditor.XCodeEditorChartboost.XCProject project = new UnityEditor.XCodeEditorChartboost.XCProject(path);
		// 	PBXProject project = new PBXProject(path);
		// 	// Find and run through all projmods files to patch the project
		// 	string projModPath = System.IO.Path.Combine(Application.dataPath, "Chartboost/Editor");
		// 	var files = System.IO.Directory.GetFiles(projModPath, "*.projmods", System.IO.SearchOption.AllDirectories);
		// 	foreach (var file in files)
		// 	{
		// 		project.ApplyMod(file);
		// 	}
        //     project.AddOtherLDFlags("-ObjC");
		// 	project.Save();
		// }




	}
}
