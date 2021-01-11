using UnityEngine;
using System.Collections;

public class PollfishEventListener : MonoBehaviour
{
	#if UNITY_ANDROID || UNITY_IPHONE

	private bool ispaused = false ; // pause and resume when Pollfish panel opens

	// debug area
	/*private GUIStyle labelStyle;
	private static string labelText= "Debug area.."; */

	private static bool surveyOnDevice = false; // true if survey was received on device
	private static bool surveyCompl = false; //true if survey is completed
    private static bool surveyRejected = false; //true if survey got rejected

	/*void OnGUI() {

		labelStyle = new GUIStyle ();

		labelStyle.fontSize = 18;
		labelStyle.normal.textColor = Color.black;
		labelStyle.alignment = TextAnchor.MiddleCenter;

		GUI.Label(new Rect(Screen.width / 2 -200, 200, 400, 40), labelText, labelStyle);
	}*/

	void Awake()
	{	
		// Tell plugin which gameobject to send messages too

		Pollfish.SetEventObjectPollfish(this.gameObject.name);
	
		Debug.Log("UnitySendMessage to object with name: " + this.gameObject.name);

		//Makes the object target not be destroyed automatically when loading a new scene 

		DontDestroyOnLoad( this );
	}


	public void OnRectTransformDimensionsChange()
	{
		Debug.Log ("PollfishDemo - OnRectTransformDimensionsChange called");

		/* handling orintation change event */

	}

	public void Update () {
	
			if (!ispaused) { // resume

				Time.timeScale = 1;

			} else if (ispaused) { // pause

				Time.timeScale = 0;		
			}
	}

	/* registering to listen for Pollfish events */

	public void OnEnable()
	{
		Pollfish.surveyCompletedEvent += surveyCompleted ;
		Pollfish.surveyOpenedEvent += surveyOpened;
		Pollfish.surveyClosedEvent += surveyClosed;
		Pollfish.surveyReceivedEvent += surveyReceived;
		Pollfish.surveyNotAvailableEvent += surveyNotAvailable;
		Pollfish.userNotEligibleEvent += userNotEligible;
        Pollfish.userRejectedSurveyEvent += userRejectedSurvey;
	}

	/* unregister from Pollfish events */

	public void OnDisable()
	{
		Pollfish.surveyCompletedEvent -= surveyCompleted;
		Pollfish.surveyOpenedEvent -= surveyOpened;
		Pollfish.surveyClosedEvent -= surveyClosed;
		Pollfish.surveyReceivedEvent -=surveyReceived;
		Pollfish.surveyNotAvailableEvent -= surveyNotAvailable;
		Pollfish.userNotEligibleEvent -= userNotEligible;
        Pollfish.userRejectedSurveyEvent -= userRejectedSurvey;
	}

	public void surveyCompleted(string surveyInfo)
	{
		string[] surveyCharacteristics = surveyInfo.Split(',');
		
		if (surveyCharacteristics.Length >= 6) {
			
            Debug.Log ("PollfishEventListener: Survey was completed - SurveyInfo with CPA: " + surveyCharacteristics[0] + " and IR: " + surveyCharacteristics[1] + " and LOI: " + surveyCharacteristics[2] + " and SurveyClass: " + surveyCharacteristics[3]+ " and RewardName: " + surveyCharacteristics[4] + " and RewardValue: " + surveyCharacteristics[5]);
			
            //labelText = "Survey was completed - SurveyInfo with CPA: " + surveyCharacteristics[0] + " and IR: " + surveyCharacteristics[1] + " and LOI: " + surveyCharacteristics[2] + " \nand SurveyClass: " + surveyCharacteristics[3]+ " and RewardName: " + surveyCharacteristics[4] + " and RewardValue: " + surveyCharacteristics[5];
		
		}else{

			//labelText = "Survey was completed - SurveyInfo with CPA: " + surveyCharacteristics[0] + " and IR: " + surveyCharacteristics[1] + " and LOI: " + surveyCharacteristics[2] + " \nand SurveyClass: " + surveyCharacteristics[3]+ " and RewardName: " + surveyCharacteristics[4] + " and RewardValue: " + surveyCharacteristics[5];

			Debug.Log ("PollfishEventListener: Survey Offerwall received");
		}

		surveyCompl = true;
		surveyOnDevice = false;
        surveyRejected= false;
	}
	
    public void surveyReceived(string surveyInfo)
	{
		string[] surveyCharacteristics = surveyInfo.Split(',');

		if (surveyCharacteristics.Length >= 6) {

            Debug.Log ("PollfishEventListener: Survey was received - : Survey was completed - SurveyInfo with CPA: " + surveyCharacteristics[0] + " and IR: " + surveyCharacteristics[1] + " and LOI: " + surveyCharacteristics[2] + " and SurveyClass: " + surveyCharacteristics[3] + " and RewardName: " + surveyCharacteristics[4] + " and RewardValue: " + surveyCharacteristics[5]);
		
            //labelText = "Survey was received - SurveyInfo with CPA: " + surveyCharacteristics[0] + " and IR: " + surveyCharacteristics[1] + " and LOI: " + surveyCharacteristics[2] + " \nand SurveyClass: " + surveyCharacteristics[3]+ " and RewardName: " + surveyCharacteristics[4] + " and RewardValue: " + surveyCharacteristics[5];
		
		}else{

			//labelText = "PollfishEventListener: Survey Offerwall received";

			Debug.Log ("PollfishEventListener: Survey Offerwall received");
		}

		surveyOnDevice = true;
		surveyCompl = false;
        surveyRejected = false;
	}

	public void surveyOpened()
	{
		Debug.Log("PollfishEventListener: Survey was opened");

		//labelText = "Survey was opened";

		ispaused = true; // pause scene 
	}
	
	public void surveyClosed()
	{
		Debug.Log("PollfishEventListener: Survey was closed");

		//labelText = "Survey was closed";

		ispaused = false; // resume scene 
	}

	public void surveyNotAvailable()
	{
		Debug.Log("PollfishEventListener: Survey not available");
		
		//labelText = "Survey not available";

		surveyOnDevice = false;
		surveyCompl = false;	
        surveyRejected = false;
	}

	public void userNotEligible()
	{
		Debug.Log("PollfishEventListener: User not eligible");
		
		//labelText = "User not eligible";

		surveyOnDevice = false;
		surveyCompl = false;	
        surveyRejected = false;
	}

    public void userRejectedSurvey()
    {
        Debug.Log("PollfishEventListener: User rejected survey");

        //labelText = "User rejected survey";

        surveyOnDevice = false;
        surveyCompl = false;
        surveyRejected = true;
    }

	public static bool isSurveyCompleted()
	{
		return surveyCompl;
	}	

	public static bool isSurveyReceived()
	{
		return surveyOnDevice;
	}


    public static bool isSurveyRejected()
    {
        return surveyRejected;
    }

	public static void resetStatus()
	{
		surveyOnDevice = false;
		surveyCompl = false;	
        surveyRejected = false;

        //labelText = "";
	}	

	#endif
}