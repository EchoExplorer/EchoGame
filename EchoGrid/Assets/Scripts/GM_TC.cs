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
	AudioClip swipeAhead;
	public Scrollbar sb;
	bool android_window_displayed = false;

	AndroidDialogue ad;
	string msg = "Show the online consent form";

	void Awake () {
		android_window_displayed = false;
		ad = GetComponent<AndroidDialogue> ();
		swipeAhead = Resources.Load("fx/swipe-ahead") as AudioClip;
		orit = new AudioClip[2];
		orit[0] = Resources.Load ("instructions/Please hold your phone horizontally for this game") as AudioClip;
		orit[1] = Resources.Load ("instructions/2sec_silence") as AudioClip;	
		clips = new AudioClip[3];
		clips[0] = Resources.Load ("instructions/Swipe left to confirm") as AudioClip;
		clips[1] = Resources.Load ("instructions/2sec_silence") as AudioClip;
		clips[2] = Resources.Load ("instructions/2sec_silence") as AudioClip;

		string[] consentResult = Utilities.Loadfile ("consentRecord");
		int[] intResult = new int[1];
		if ((consentResult[0] != null)&&(consentResult != null)) {
			intResult = Array.ConvertAll<string, int>(consentResult, int.Parse);
			if(intResult[0] == 1)
				SceneManager.LoadScene("Title_screen");
		}
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
			if (SoundManager.instance.PlayVoice (clips [cur_clip])) {
				cur_clip += 1;
				if (cur_clip >= orit.Length)
					cur_clip = 0;
			}
		}
	}

	Vector2 touchOrigin = -Vector2.one;
	float touchTime = 0f;
	private float minSwipeDist = 100f;
	// Update is called once per frame
	void Update () {
		if ( (sb.value <= 0.5f)&&(!android_window_displayed) ) {
			android_window_displayed = true;
			finished_reading = true;
			ad.DisplayAndroidWindow (msg);
		}

		if (ad.yesclicked ()) {
			//open URL
			//Application.OpenURL ("");
		} else if (ad.noclicked ()) {
			SceneManager.LoadScene("T&C");
		}
		
		play_audio ();

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			//SoundManager.instance.PlaySingle(swipeRight);
		}else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			SceneManager.LoadScene("Agreement");
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
						SceneManager.LoadScene("Agreement");
						SoundManager.instance.PlaySingle(swipeAhead);
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
