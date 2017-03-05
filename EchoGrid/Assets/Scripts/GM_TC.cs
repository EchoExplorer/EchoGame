using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class GM_TC : MonoBehaviour {

	int orti_clip = 0;
	bool finished_reading = false;
	bool URL_opened = false;
	bool android_window_displayed = false;
	public Text titleText;
	bool doneTesting = false;

	AndroidDialogue ad;

	//eventHandler eh;

	void Awake () {
		URL_opened = false;
		android_window_displayed = false;
		ad = GetComponent<AndroidDialogue> ();

		string[] consentResult = Utilities.Loadfile ("consentRecord");
		int[] intResult = new int[1];
		if ((consentResult[0] != null)&&(consentResult != null)) {
			intResult = Array.ConvertAll<string, int>(consentResult, int.Parse);
			if(intResult[0] == 1)
				SceneManager.LoadScene("Title_screen");
		}

		doneTesting = false;
		Utilities.initEncrypt ();
	}

	void Start(){
		//eh = new eventHandler (InputModule.instance);
	}
		
	void OnLevelWasLoaded(int index){
		//eh = new eventHandler (InputModule.instance);
	}

	private void reportConsent(string code) {
		string echoEndpoint = "https://echolock.andrew.cmu.edu/cgi-bin/acceptConsent.py";

		WWWForm echoForm = new WWWForm ();
		echoForm.AddField ("userName", Utilities.encrypt (SystemInfo.deviceUniqueIdentifier));
		echoForm.AddField ("consentID", Utilities.encrypt (code));
		echoForm.AddField ("dateTimeStamp", Utilities.encrypt (System.DateTime.Now.ToString ()));

		UnityEngine.Debug.Log (System.Text.Encoding.ASCII.GetString (echoForm.data));

		WWW www = new WWW (echoEndpoint, echoForm);
		StartCoroutine (Utilities.WaitForRequest (www));
	}


	bool reset_audio = false;
	void play_audio(){
		if (!Utilities.isDeviceLandscape()) {//not landscape!
			if (SoundManager.instance.PlayVoice (Database.instance.oritClip [orti_clip], reset_audio)) {
				reset_audio = false;
				orti_clip += 1;
				if (orti_clip >= Database.instance.oritClip.Length)
					orti_clip = 0;
			}
		} else if (finished_reading) {
			//if (SoundManager.instance.PlayVoice (clips [cur_clip])) {
			//	cur_clip += 1;
			//	if (cur_clip >= orit.Length)
			//		cur_clip = 0;
			//}
		}
	}
		
	// Update is called once per frame
	void Update () {
		//MUST have internet connection
		if (Const.TEST_CONNECTION) {
			if (!doneTesting) {
				string str = Utilities.check_InternetConnection ();
				if (str.Length == 0) {//we're good to go
					doneTesting = true;
					titleText.text = Database.tcText_main;
				} else
					titleText.text = str;
			}
		}


		#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		if (!android_window_displayed) {
			android_window_displayed = true;
			finished_reading = false;
			ad.clearflag();
			ad.DisplayAndroidWindow (Database.tcmsg, AndroidDialogue.DialogueType.YESONLY);
		}

		if (!URL_opened && ad.yesclicked () && !finished_reading) {
			//open URL
			URL_opened = true;
			Application.OpenURL ("http://echolock.andrew.cmu.edu/consent/");//"http://echolock.andrew.cmu.edu/consent/?"
		} else if (URL_opened && !finished_reading) {//report code from popup using reportConsent()
			finished_reading = true;
			ad.clearflag ();
			ad.DisplayAndroidWindow ("Enter code provided from \n the consent form:", AndroidDialogue.DialogueType.INPUT);
		} else if (URL_opened && finished_reading && ad.yesclicked ()) {
			Utilities.writefile ("consentRecord","1");
			reportConsent(ad.getInputStr());
			ad.clearflag ();
			ad.DisplayAndroidWindow ("Thank you!", AndroidDialogue.DialogueType.YESONLY);
			SceneManager.LoadScene("Title_Screen");
		}
		#endif 
		
		play_audio ();

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		if(eh.isActivate()){
			InputEvent ie = eh.getEventData();
			switch(ie.keycode){
				//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
				case KeyCode.RightArrow:
					//SoundManager.instance.PlaySingle(swipeRight);
					break;
				case KeyCode.LeftArrow:
					SceneManager.LoadScene("Title_Screen");
					SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
					break;
				case KeyCode.UpArrow:
					break;
				case KeyCode.DownArrow://BACK
					//SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
					//credit
					break;
				default:
					break;
			}
		}

		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Screen.orientation = ScreenOrientation.Landscape;

		#endif //End of mobile platform dependendent compilation section started above with #elif	
	}
}
