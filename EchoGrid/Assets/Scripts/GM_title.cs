using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GM_title : MonoBehaviour {

	Vector2 touchOrigin = -Vector2.one;
	float touchTime = 0f;
	private float minSwipeDist = 100f;
	AudioClip swipeAhead;
	AudioClip swipeRight;
	AudioClip swipeLeft;
	//AudioClip to_tutorial;
	AudioClip[] to_main;
	AudioClip[] orit;
	AudioClip[] clips;
	AudioClip[] cmdlist;
	int cur_clip = 0;
	int orti_clip = 0;
	int cmd_cur_clip = 0;
	int total_clip = 7;
	float time_interval = 2.0f;
	bool isLandscape = true;
	bool reset_audio = false;
	bool listenToCmd = false;
	public bool toMainflag = false;

	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.Landscape;
		reset_audio = false;
		orit = new AudioClip[2];
		orit[0] = Resources.Load ("instructions/Please hold your phone horizontally for this game") as AudioClip;
		orit[1] = Resources.Load ("instructions/2sec_silence") as AudioClip;
		if ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft) || (Input.deviceOrientation == DeviceOrientation.LandscapeRight))
			isLandscape = true;
		else if (Input.deviceOrientation == DeviceOrientation.Portrait)
			isLandscape = false;
		
		GameObject.Find ("GameMode").GetComponent <GameMode>().init ();
		//load instruction clips
		clips = new AudioClip[total_clip];
		clips[0] = Resources.Load ("instructions/Welcome to Echo Adventure") as AudioClip;
		clips[1] = Resources.Load ("instructions/Swipe right to go to the new game menu") as AudioClip;
		clips[2] = Resources.Load ("instructions/Swipe left to go to the tutorial") as AudioClip;
		clips[3] = Resources.Load ("instructions/Swipe up to hear a list of commands") as AudioClip;
		clips[4] = Resources.Load ("instructions/2sec_silence") as AudioClip;
		clips[5] = Resources.Load ("instructions/2sec_silence") as AudioClip;
		clips[6] = Resources.Load ("instructions/1sec_silence") as AudioClip;

		swipeAhead = Resources.Load("fx/swipe-ahead") as AudioClip;
		swipeRight = Resources.Load("fx/swipe-right") as AudioClip;
		swipeLeft = Resources.Load("fx/swipe-left") as AudioClip;
		//to_tutorial = Resources.Load ("instructions/Welcome to the tutorial") as AudioClip;
		to_main = new AudioClip[4];
		to_main[0] = Resources.Load ("instructions/0_5sec_silence") as AudioClip;
		to_main[1] = Resources.Load ("instructions/Swipe right to continue from last time") as AudioClip;
		to_main[2] = Resources.Load ("instructions/Double tap to start a new game, then, swipe left to confirm, or double tap to cancel") as AudioClip;
		to_main[3] = Resources.Load ("instructions/0_5sec_silence") as AudioClip;

		cmdlist = new AudioClip[9];
		cmdlist[0] = Resources.Load ("instructions/Tap and hold to hear an echo") as AudioClip;
		cmdlist[1] = Resources.Load ("instructions/To open the pause menu, press two fingers on the screen and hold") as AudioClip;
		cmdlist[2] = Resources.Load ("instructions/Swipe up to move forward") as AudioClip;
		cmdlist[3] = Resources.Load ("instructions/Rotate two fingers counterclockwise to turn left") as AudioClip;
		cmdlist[4] = Resources.Load ("instructions/Rotate two fingers clockwise to turn right") as AudioClip;
		cmdlist[5] = Resources.Load ("instructions/Double tap to attempt to exit") as AudioClip;
		cmdlist[6] = Resources.Load ("instructions/0_5sec_silence") as AudioClip;
		cmdlist[7] = Resources.Load ("instructions/turn around by turning in the same direction twice") as AudioClip;
		cmdlist[8] = Resources.Load ("instructions/2sec_silence") as AudioClip;
	}

	void play_audio(){
		if (isLandscape && !listenToCmd) {
			if (SoundManager.instance.PlayVoice (clips [cur_clip], reset_audio)) {
				reset_audio = false;
				cur_clip += 1;
				if (cur_clip >= total_clip)
					cur_clip = 0;
			}
		} else if (listenToCmd) {	
			if (SoundManager.instance.PlayVoice (cmdlist [cmd_cur_clip], reset_audio)) {
				reset_audio = false;
				cmd_cur_clip += 1;
				if (cmd_cur_clip >= cmdlist.Length)
					cmd_cur_clip = 0;
			}
		} else {//not landscape!
			if (SoundManager.instance.PlayVoice (orit [orti_clip], reset_audio)) {
				reset_audio = false;
				orti_clip += 1;
				if (orti_clip >= orit.Length)
					orti_clip = 0;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		play_audio ();

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
			SceneManager.LoadScene("Main_pre");
			//toMainflag = true;
			//cur_clip = 0;
			SoundManager.instance.PlayVoice(to_main[1], true);
			//SoundManager.instance.PlaySingle(swipeRight);
		} else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
			SceneManager.LoadScene("Main");
			//SoundManager.instance.PlayVoice(to_tutorial, true);
			//SoundManager.instance.PlaySingle(swipeLeft);
		} else if (Input.GetKeyUp("f")) {
			//SceneManager.LoadScene("Main");
			//SoundManager.instance.PlaySingle(swipeAhead);
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
						GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
						SceneManager.LoadScene("Main_pre");
						SoundManager.instance.PlayVoice(to_main[0], true);
						//SoundManager.instance.PlaySingle(swipeRight);
					} else {//LEFT
						GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
						SceneManager.LoadScene("Main");
						//SoundManager.instance.PlayVoice(to_tutorial, true);
					}
				} else if (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) {
					//If y is greater than zero, set vertical to 1, otherwise set it to -1
					if (y > 0) {//FRONT
						SoundManager.instance.PlaySingle(swipeAhead);
						if(!listenToCmd){
							listenToCmd = true;
							reset_audio = true;
							cur_clip = 0;
							cmd_cur_clip = 0;
						}else{
							listenToCmd = false;
							reset_audio = true;
							cur_clip = 0;
							cmd_cur_clip = 0;
						}
					} else {//BACK
						//SoundManager.instance.PlaySingle(swipeAhead);
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
