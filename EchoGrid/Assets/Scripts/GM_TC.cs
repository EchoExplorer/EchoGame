using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class GM_TC : MonoBehaviour {

	AudioClip[] orit;
	int orti_clip = 0;
	AudioClip[] clips;
	int cur_clip = 0;
	bool isLandscape = true;
	bool finished_reading = false;
	bool URL_opened = false;
	AudioClip swipeAhead;
	//public Scrollbar sb;
	bool android_window_displayed = false;

	AndroidDialogue ad;
	string msg = "Please hold your phone horizontally for this game, \n " +
		         "and please read the online consent form; \n " +
		         "after finish, you can click back button to " +
		         "return to the game";

	string msgCode = "Your consent code is: \n";
	string consentCode = "";

	void Awake () {
		URL_opened = false;
		android_window_displayed = false;
		ad = GetComponent<AndroidDialogue> ();
		swipeAhead = Resources.Load("fx/swipe-ahead") as AudioClip;
		orit = new AudioClip[2];
		orit[0] = Resources.Load ("instructions/Please hold your phone horizontally for this game") as AudioClip;
		orit[1] = Resources.Load ("instructions/2sec_silence") as AudioClip;	
		clips = new AudioClip[2];
		//clips[0] = Resources.Load ("instructions/Swipe left to confirm") as AudioClip;
		clips[0] = Resources.Load ("instructions/2sec_silence") as AudioClip;
		clips[1] = Resources.Load ("instructions/2sec_silence") as AudioClip;

		string[] consentResult = Utilities.Loadfile ("consentRecord");
		int[] intResult = new int[1];
		if ((consentResult[0] != null)&&(consentResult != null)) {
			intResult = Array.ConvertAll<string, int>(consentResult, int.Parse);
			if(intResult[0] == 1)
				SceneManager.LoadScene("Title_screen");
		}

		Utilities.initEncrypt ();
	}

	private void reportConsent(string code) {
		string echoEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptConsent.py";

		WWWForm echoForm = new WWWForm ();
		echoForm.AddField ("userName", Utilities.encrypt (SystemInfo.deviceUniqueIdentifier));
		echoForm.AddField ("consentID", Utilities.encrypt (code));

		UnityEngine.Debug.Log (System.Text.Encoding.ASCII.GetString (echoForm.data));

		WWW www = new WWW (echoEndpoint, echoForm);
		StartCoroutine (Utilities.WaitForRequest (www));
	}


	bool reset_audio = false;
	void play_audio(){
		if (!isLandscape) {//not landscape!
			if (SoundManager.instance.PlayVoice (orit [orti_clip], reset_audio)) {
				reset_audio = false;
				orti_clip += 1;
				if (orti_clip >= orit.Length)
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

	Vector2 touchOrigin = -Vector2.one;
	float touchTime = 0f;
	private float minSwipeDist = 100f;
	// Update is called once per frame
	void Update () {
		if ( (!android_window_displayed) ) {
			android_window_displayed = true;
			finished_reading = false;
			ad.clearflag();
			ad.DisplayAndroidWindow (msg, AndroidDialogue.DialogueType.YESONLY);
		}

		if (!URL_opened && ad.yesclicked () && finished_reading) {
			//open URL
			URL_opened = true;
			Application.OpenURL ("http://echolock.andrew.cmu.edu/consent/");//"http://echolock.andrew.cmu.edu/consent/?"
			ad.clearflag ();
		} else if (!URL_opened && ad.yesclicked () && !finished_reading) {
			ad.clearflag ();
			if (SystemInfo.deviceUniqueIdentifier.Length <= 4)
				consentCode = SystemInfo.deviceUniqueIdentifier;
			else
				consentCode = SystemInfo.deviceUniqueIdentifier.Substring (0, 4);
			string codemsg = msgCode + consentCode + "\n please enter this in the consent form.";
			ad.DisplayAndroidWindow (codemsg, AndroidDialogue.DialogueType.YESONLY);
			finished_reading = true;
		} else if (URL_opened) {//report code from popup using reportConsent()
			reportConsent(consentCode);
			SceneManager.LoadScene("Title_Screen");
		}

		
		play_audio ();

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			//SoundManager.instance.PlaySingle(swipeRight);
		}else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			SceneManager.LoadScene("Title_Screen");
			SoundManager.instance.PlaySingle(swipeAhead);
		} else if (Input.GetKeyUp(KeyCode.UpArrow)){//Up
		} else if (Input.GetKeyUp(KeyCode.DownArrow)){//BACK
			//SoundManager.instance.PlaySingle(swipeAhead);
			//credit
		}

		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Screen.orientation = ScreenOrientation.Landscape;
		if ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft) || (Input.deviceOrientation == DeviceOrientation.LandscapeRight))
			isLandscape = true;
		else if (Input.deviceOrientation == DeviceOrientation.Portrait){
			isLandscape = false;
		}

		float TOUCH_TIME = 0.05f;

		//Check if Input has registered more than zero touches
		int numTouches = Input.touchCount;

		if (numTouches > 0) {
			//Store the first touch detected.
			Touch myTouch = Input.touches[0];

			//Check if the phase of that touch equals Began
			if (myTouch.phase == TouchPhase.Began){
			//If so, set touchOrigin to the position of that touch
				touchOrigin = myTouch.position;
				touchTime = Time.time;
			}

			//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
			else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
				//Set touchEnd to equal the position of this touch
				Vector2 touchEnd = myTouch.position;

				//Calculate the difference between the beginning and end of the touch on the x axis.
				float x = touchEnd.x - touchOrigin.x;

				//Calculate the difference between the beginning and end of the touch on the y axis.
				float y = touchEnd.y - touchOrigin.y;

				//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
				touchOrigin.x = -1;

				//Check if the difference along the x axis is greater than the difference along the y axis.
				if (Mathf.Abs(x) > Mathf.Abs(y) && Mathf.Abs(x) >= minSwipeDist)
				{
					//If x is greater than zero, set horizontal to 1, otherwise set it to -1
					if (x > 0) {//RIGHT
					//SoundManager.instance.PlaySingle(swipeRight);
					} else {//LEFT
						SceneManager.LoadScene("Title_Screen");
						//SoundManager.instance.PlaySingle(swipeAhead);
					}
				} else if (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) {
					//If y is greater than zero, set vertical to 1, otherwise set it to -1
					if (y > 0) {//FRONT
						//SoundManager.instance.PlaySingle(swipeAhead);
					} else {//BACK
						//SoundManager.instance.PlaySingle(swipeAhead);
					//credit
					}
				} else if (Mathf.Abs(Time.time - touchTime) > TOUCH_TIME) {
					if (numTouches == 2){
					//GameMode.gamemode = GameMode.Game_Mode.MAIN;
					//SceneManager.LoadScene("Main");
					}
					else{}
				}
			}
		}
		#endif //End of mobile platform dependendent compilation section started above with #elif	
	}
}
