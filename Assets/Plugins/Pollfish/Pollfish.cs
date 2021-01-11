using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public enum Position
{
	TOP_LEFT=0,
	BOTTOM_LEFT,
	TOP_RIGHT,
	BOTTOM_RIGHT,
	MIDDLE_LEFT,
	MIDDLE_RIGHT 
}

#if UNITY_ANDROID || UNITY_IPHONE

public class Pollfish
{
	#if UNITY_ANDROID 

	private static AndroidJavaClass mPollfishAssistantClass;

	static Pollfish()
	{
		if( Application.platform != RuntimePlatform.Android )
			return;
		
		Debug.Log ("Pollfish - Pollfish()");
	
		mPollfishAssistantClass = new AndroidJavaClass ("com.pollfish_unity.main.PollfishPlugin");
	}

	#elif UNITY_IPHONE

	#region API

	// Pollfish iOS Bridge functions

	[DllImport ("__Internal")]
	private static extern void PollfishInitWith(int position, int padding, string api_key, bool releaseMode, bool rewardMode, string request_uuid, string attrDict, bool offerwallMode);

	[DllImport ("__Internal")]
	private static extern void ShowPollfishFunction();

	[DllImport ("__Internal")]
	private static extern void HidePollfishFunction();

	[DllImport ("__Internal")]
	private static extern void SetEventObjectNamePollfish(string gameObjName);

	[DllImport ("__Internal")]
	private static extern bool IsPollfishPresentFunction();



	#endregion

	#endif


	public class PollfishParams
 	{
		public bool releaseMode { get; set; }
		public bool rewardMode { get; set; }
		public bool offerwallMode { get; set; }
		public int indicatorPadding { get; set; }
		public string requestUUID { get; set; }
		public int indicatorPosition { get; set; }
		public Dictionary<string, string> userAttributes { get; set; }

   		public PollfishParams()
    	{
			this.releaseMode = true;
			this.rewardMode = false;
			this.offerwallMode = false;
			this.indicatorPadding = 10;
			this.requestUUID = "";
			this.indicatorPosition = (int) Position.MIDDLE_LEFT;
			this.userAttributes=null;
    	}

    	public void OfferwallMode(bool offerwallMode)
    	{

     	   this.offerwallMode=offerwallMode;
   		}

     	public void IndicatorPadding(int indicatorPadding)
    	{
        	this.indicatorPadding=indicatorPadding;
    	}

     	public void ReleaseMode(bool releaseMode)
    	{
        	this.releaseMode=releaseMode;
    	}

     	public void RewardMode(bool rewardMode)
    	{
        	this.rewardMode=rewardMode;
    	}

     	public void IndicatorPosition(int indicatorPosition)
    	{
        	this.indicatorPosition=indicatorPosition;
    	}

    	public void RequestUUID(string requestUUID)
    	{
        	this.requestUUID=requestUUID;
    	}

    	public void UserAttributes(Dictionary<string, string> userAttributes)
    	{
       	 	this.userAttributes=userAttributes;
  		}
	}

	// Pollfish init with request uuid and user attributes
	//public static void PollfishInitFunction(int pollfishPosition, int indPadding, string apiKey, bool debugMode, bool rewardMode, string request_uuid,  Dictionary<string,string> attrDict){

	public static void PollfishInitFunction(string apiKey, PollfishParams pollfishParams){


		#if UNITY_ANDROID

		using(AndroidJavaObject obj_HashMap = new AndroidJavaObject("java.util.HashMap"))
		{
			// Call 'put' via the JNI instead of using helper classes to avoid:
			//  "JNI: Init'd AndroidJavaObject with null ptr!"
			IntPtr method_Put = AndroidJNIHelper.GetMethodID(obj_HashMap.GetRawClass(), "put",
				"(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");

			object[] args = new object[2];

		if(pollfishParams.userAttributes!=null){
			
			foreach(KeyValuePair<string, string> kvp in pollfishParams.userAttributes)
			{
				using(AndroidJavaObject k = new AndroidJavaObject("java.lang.String", kvp.Key))
				{
					using(AndroidJavaObject v = new AndroidJavaObject("java.lang.String", kvp.Value))
					{
						args[0] = k;
						args[1] = v;

						AndroidJNI.CallObjectMethod(obj_HashMap.GetRawObject(),
							method_Put, AndroidJNIHelper.CreateJNIArgArray(args));
					}
				}
			}
		}

			mPollfishAssistantClass.CallStatic("init",apiKey,pollfishParams.indicatorPosition, pollfishParams.indicatorPadding,pollfishParams.requestUUID,pollfishParams.releaseMode,pollfishParams.rewardMode, obj_HashMap, pollfishParams.offerwallMode); 
		
		}

		#elif UNITY_IPHONE

		string attributesString = "";

		if(pollfishParams.userAttributes!=null){

			foreach(KeyValuePair<string, string> kvp in pollfishParams.userAttributes)
			{
				attributesString += kvp.Key + "=" + kvp.Value + "\n";
			}
		
		}

		PollfishInitWith(pollfishParams.indicatorPosition,pollfishParams.indicatorPadding,apiKey,pollfishParams.releaseMode,pollfishParams.rewardMode,pollfishParams.requestUUID,attributesString,pollfishParams.offerwallMode);

		#endif
	}

	// Specify object to receive Unity messages
	public static void SetEventObjectPollfish(string gameObjNmae)
	{
		#if UNITY_ANDROID
		
		mPollfishAssistantClass.CallStatic("setEventObjectPollfish", gameObjNmae);
		
		#elif UNITY_IPHONE

		SetEventObjectNamePollfish(gameObjNmae);

		#endif
	}


	// Manually show Pollfish.
	public static bool IsPollfishPresent()
	{
		bool isPresent;

		#if UNITY_ANDROID

		isPresent = mPollfishAssistantClass.CallStatic<bool>("isPollfishPresent");

		#elif UNITY_IPHONE

		isPresent = IsPollfishPresentFunction ();

		#endif

		Debug.Log("Pollfish - isPresent = " + isPresent);

		return isPresent;
	}

	// Manually show Pollfish.
	public static void ShowPollfish()
	{
		#if UNITY_ANDROID

		mPollfishAssistantClass.CallStatic("show");

		#elif UNITY_IPHONE
				
		ShowPollfishFunction ();

		#endif
	}
	
	
	// Manually hide Pollfish.
	public static void HidePollfish()
	{
		#if UNITY_ANDROID
		
		mPollfishAssistantClass.CallStatic("hide");

		#elif UNITY_IPHONE

		HidePollfishFunction();

		#endif
	}


	// decide if you should quit your app on back button event (Pollfish is not open)
	public static void ShouldQuit()
	{	
		#if UNITY_ANDROID

		bool shouldQuit = mPollfishAssistantClass.CallStatic<bool>("shouldQuit");

		Debug.Log("Pollfish - shouldQuit = " + shouldQuit);

		if(shouldQuit){

			Application.Quit ();
		}

		#endif
	}


	#region Events

	public static event Action surveyOpenedEvent;
	public static event Action surveyClosedEvent;
	public static event Action <string> surveyReceivedEvent;
	public static event Action <string> surveyCompletedEvent;
	public static event Action surveyNotAvailableEvent;
	public static event Action userNotEligibleEvent;
    public static event Action userRejectedSurveyEvent;

	
	public void surveyCompleted(string surveyInfo)
	{
		if (surveyCompletedEvent != null)
			surveyCompletedEvent (surveyInfo);
	}

	public void surveyReceived(string surveyInfo)
	{
		
		if (surveyReceivedEvent != null)
			surveyReceivedEvent (surveyInfo);
	}
	
	public void surveyOpened()
	{
		if (surveyOpenedEvent != null)
			surveyOpenedEvent();
	}
	
	public void surveyClosed()
	{
		if (surveyClosedEvent != null)
			surveyClosedEvent();
	}

	public void surveyNotAvailable()
	{	
		if (surveyNotAvailableEvent != null)
			surveyNotAvailableEvent();
	}

	public void userNotEligible()
	{
		if (userNotEligibleEvent != null)
			userNotEligibleEvent();
	}

    public void userRejectedSurvey()
    {
        if (userRejectedSurveyEvent != null)
            userRejectedSurveyEvent();
    }

	#endregion

}

#endif