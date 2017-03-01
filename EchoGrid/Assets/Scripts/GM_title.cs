using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GM_title : MonoBehaviour {

	Vector2 touchOrigin = -Vector2.one;
	float touchTime = 0f;
	private float minSwipeDist = 100f;

	int cur_clip = 0;
	int orti_clip = 0;
	int cmd_cur_clip = 0;

	float time_interval = 2.0f;
	bool reset_audio = false;
	bool listenToCmd = false;
	public bool toMainflag = false;
	public Text titleText;
	bool doneTesting = false;
	eventHandler eh;

	// Use this for initialization
	void Start () {
		reset_audio = false;
		GameObject.Find ("GameMode").GetComponent <GameMode>().init ();
		eh = new eventHandler(InputModule.instance);
	}

	void play_audio(){
		if (!Utilities.isDeviceLandscape() && !listenToCmd) {
			if (SoundManager.instance.PlayVoice (Database.instance.TitleClips [cur_clip], reset_audio)) {
				reset_audio = false;
				cur_clip += 1;
				if (cur_clip >= Database.instance.TitleClips.Length)
					cur_clip = 0;
			}
		} else if (listenToCmd) {	
			if (SoundManager.instance.PlayVoice (Database.instance.TitleCmdlistClips[cmd_cur_clip], reset_audio)) {
				reset_audio = false;
				cmd_cur_clip += 1;
				if (cmd_cur_clip >= Database.instance.TitleCmdlistClips.Length)
					cmd_cur_clip = 0;
			}
		} else {//not landscape!
			if (SoundManager.instance.PlayVoice (Database.instance.oritClip[orti_clip], reset_audio)) {
				reset_audio = false;
				orti_clip += 1;
				if (orti_clip >= Database.instance.oritClip.Length)
					orti_clip = 0;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		/*
		if (!doneTesting){
			string str = Utilities.check_InternetConnection ();
			if (str.Length == 0) {//we're good to go
				doneTesting = true;
				titleText.text = Database.titleText_main;
			}else
				titleText.text = str;
		}
		*/

		play_audio ();

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		if(eh.isActivate()){
			InputEvent ie = eh.getEventData();
			switch(ie.keycode){
			case KeyCode.RightArrow:
				GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
				SceneManager.LoadScene("Main_pre");
				//toMainflag = true;
				//cur_clip = 0;
				SoundManager.instance.PlayVoice(Database.instance.TitletoMainClips[1], true);
				//SoundManager.instance.PlaySingle(swipeRight);
				break;
			case KeyCode.LeftArrow:
				GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
				SceneManager.LoadScene("Main");
				//SoundManager.instance.PlayVoice(to_tutorial, true);
				//SoundManager.instance.PlaySingle(swipeLeft);
				break;
			case KeyCode.UpArrow://Up
				SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
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
				break;
			case KeyCode.DownArrow://BACK
				SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
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
						SoundManager.instance.PlayVoice(Database.instance.TitletoMainClips[0], true);
						//SoundManager.instance.PlaySingle(swipeRight);
					} else {//LEFT
						GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
						SceneManager.LoadScene("Main");
						//SoundManager.instance.PlayVoice(to_tutorial, true);
					}
				} else if (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) {
					//If y is greater than zero, set vertical to 1, otherwise set it to -1
					if (y > 0) {//FRONT
						SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
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
						SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
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
