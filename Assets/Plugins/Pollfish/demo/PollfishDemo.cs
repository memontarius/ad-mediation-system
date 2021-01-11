using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class PollfishDemo : MonoBehaviour
{
	#if UNITY_ANDROID || UNITY_IPHONE

	private string apiKey;

	private Position pollfishPosition = Position.MIDDLE_RIGHT;

	private bool releaseMode = false;
	private bool offerwallMode = false;
	private bool rewardMode = false;
	private int indPadding = 10;
	private string requestUUID = "my_id";
	private bool ispaused = false;

	private string[] text = new string[] { "Show/Hide", "Rewarded Survey", "Offerwall"};

	private static int curGridInt;
	private static int oldGridInt=-1;

	private GUIStyle buttonStyle;
	private GUIStyle labelStyle;

	private int currentScreenOrientation = 0;

	void OnApplicationPause (bool pause)
	{

		Debug.Log ("PollfishDemo - OnApplicationPaused: " + pause);

		if (pause) {
			// we are in background
			ispaused = true;

		} else {
			// we are in foreground again.

			ispaused = false;			

		}
	}
		

	public void Update ()
	{		
		/* handling Android back event */	

		if (Input.GetKeyDown (KeyCode.Escape)) {

			Pollfish.ShouldQuit ();
		}
		
		if (!ispaused) { // resume
			
			Time.timeScale = 1;
				
		} else if (ispaused) { // pause
			
			Time.timeScale = 0;		
		}


		// init Pollfish on orientation change in case you support different screen orientations in your app

		if (currentScreenOrientation != (int) Screen.orientation) {

			currentScreenOrientation =(int)  Screen.orientation;

			initializePollfish ();
		}
	}

	public void OnEnable ()
	{
		Debug.Log ("PollfishDemo - onEnabled called");

		#if UNITY_ANDROID

		//apiKey = "ANDROID_API_KEY";

		apiKey ="1603299a-ec6a-464d-954c-538f3669571e";

		#elif UNITY_IPHONE

		//apiKey = "IOS_API_KEY";
		
		apiKey = "1603299a-ec6a-464d-954c-538f3669571e";
				
		#endif

		initializePollfish ();

	}
		

	void OnGUI ()
	{	

		buttonStyle = new GUIStyle(GUI.skin.button);
		buttonStyle.fontSize = 33;

		labelStyle = new GUIStyle ();

		labelStyle.fontSize = 18;
		labelStyle.normal.textColor = Color.black;
		labelStyle.alignment = TextAnchor.MiddleCenter;


		curGridInt = GUI.SelectionGrid(new Rect(10, Screen.height-200, Screen.width - 20, 60), curGridInt, text, 3,buttonStyle);

		// uncomment to view show or hide buttons

		if (curGridInt ==0) {

			// Show only on standard integration

			if (GUI.Button (new Rect (10, 150, 100, 60), "Show",buttonStyle)) {

				Debug.Log ("PollfishDemo: Show Pollfish Button pressed.");

				Pollfish.ShowPollfish ();
			}

			if (GUI.Button (new Rect (10, 230, 100, 60), "Hide",buttonStyle)) {

				Debug.Log ("PollfishDemo: Hide Pollfish Button pressed.");

				Pollfish.HidePollfish ();
			}
		}


		//rewarded mode (both Rewarded Survey & Offerwall approach)

		if ((!PollfishEventListener.isSurveyCompleted() && Pollfish.IsPollfishPresent() && (curGridInt== 1))|| (Pollfish.IsPollfishPresent() && (curGridInt== 2))) {

			if (GUI.Button (new Rect (10, 260, Screen.width - 20, 60), (curGridInt== 1)?"Complete survey to win coins":"Survey Offerwall",buttonStyle)) {

				Pollfish.ShowPollfish (); 
			}

			//rewarded mode after completion of survey

        } else if ((curGridInt == 1) && (PollfishEventListener.isSurveyRejected()))
        {

            labelStyle.fontSize = 32;
            labelStyle.normal.textColor = Color.black;
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(10, 260, Screen.width - 20, 100), "YOU REJECTED SURVEY", labelStyle);

        }else if((curGridInt == 1) && (PollfishEventListener.isSurveyCompleted())){

			labelStyle.fontSize = 32;
			labelStyle.normal.textColor = Color.black;
			labelStyle.alignment = TextAnchor.MiddleCenter;

			GUI.Label(new Rect (10, 260, Screen.width - 20, 100), "YOU WON COINS", labelStyle);
        }



		labelStyle.fontSize = 30;
		labelStyle.normal.textColor = Color.red;
		labelStyle.alignment = TextAnchor.MiddleCenter;

		// standard or rewarded mode of demo scene

		if(curGridInt==0){
			GUI.Label (new Rect (Screen.width / 2 - 200, 110, 400, 40), "Standard -Show/Hide", labelStyle);
		}else if(curGridInt==1){
			GUI.Label (new Rect (Screen.width / 2 - 200, 110, 400, 40), "Rewarded Survey", labelStyle);
		}else if(curGridInt==2){
			GUI.Label (new Rect (Screen.width / 2 - 200, 110, 400, 40), "Offerwall Mode", labelStyle);
		}

		if (GUI.changed)
		{
			if (oldGridInt != curGridInt)
			{
				Debug.Log ("User changed mode");

				oldGridInt = curGridInt;
			
				initializePollfish ();
			}
		}
	}



	void initializePollfish ()
	{	


		
		// Send user demographic attributes to shorten or skip demographic surveys

		Dictionary<string, string> dict = new Dictionary<string, string> ();
		//used in demographic surveys
		dict.Add ("gender", "1");
		dict.Add ("year_of_birth", "1974");
		dict.Add ("marital_status", "2");
		dict.Add ("parental", "3");
		dict.Add ("education", "1");
		dict.Add ("employment", "1");
		dict.Add ("career", "2");
		dict.Add ("race", "3");
		dict.Add ("income", "1");
		//general user attributes
		dict.Add ("email", "user_email@gmail.com");
		dict.Add ("google_id", "USER_GOOGLE");
		dict.Add ("linkedin_id", "USER_LINKEDIN");
		dict.Add ("twitter_id", "USER_TWITTER");
		dict.Add ("facebook_id", "USER_FB");
		dict.Add ("phone", "USER_PHONE");
		dict.Add ("name", "USER_NAME");
		dict.Add ("surname", "USER_SURNAME");
		

        rewardMode = (curGridInt == 0) ? false : true;
		offerwallMode = (curGridInt == 2) ? true : false;

		Debug.Log ("PollfishDemo||| - offerwallMode: " + offerwallMode);


        PollfishEventListener.resetStatus(); // using this for demo purposes

		Pollfish.PollfishParams pollfishParams = new Pollfish.PollfishParams();

		pollfishParams.OfferwallMode(offerwallMode);
		pollfishParams.IndicatorPadding(indPadding);
		pollfishParams.ReleaseMode(releaseMode);
		pollfishParams.RewardMode(rewardMode);
		pollfishParams.IndicatorPosition((int)pollfishPosition);
		pollfishParams.RequestUUID(requestUUID);
		pollfishParams.UserAttributes(dict);
		
		Pollfish.PollfishInitFunction(apiKey, pollfishParams);
	}

	#endif

}