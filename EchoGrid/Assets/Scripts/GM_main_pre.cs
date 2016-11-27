using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GM_main_pre : MonoBehaviour {
	Vector2 touchOrigin = -Vector2.one;
	float touchTime = 0f;
	private float minSwipeDist = 100f;
	AudioClip swipeAhead;
	AudioClip swipeRight;
	AudioClip swipeLeft;
	AudioClip[] clips;
	AudioClip[] confirm_list;
	AudioClip continue_game, new_game;
	int cur_clip = 0;
	int total_clip = 4;
	int total_confirm_clip = 3;
	float time_interval = 2.0f;
	bool at_confirm = false;
	bool reset_audio = false;

	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.Landscape;

		GameObject.Find ("GameMode").GetComponent <GameMode>().init ();
		//load instruction clips
		clips = new AudioClip[total_clip];
		clips[0] = Resources.Load ("instructions/To continue from where you left off, swipe right ") as AudioClip;
		clips[1] = Resources.Load ("instructions/0_5sec_silence") as AudioClip;
		clips[2] = Resources.Load ("instructions/Double tap to start a new game, then, swipe left to confirm, or double tap to cancel") as AudioClip;
		clips[3] = Resources.Load ("instructions/2sec_silence") as AudioClip;
		confirm_list = new AudioClip[total_confirm_clip];
		confirm_list [0] = Resources.Load ("instructions/Are you sure you want to start a new game, this will overwrite existing saves") as AudioClip;
		confirm_list [1] = Resources.Load ("instructions/Swipe left to confirm or double tap to cancel") as AudioClip;
		confirm_list [2] = Resources.Load ("instructions/2sec_silence") as AudioClip;
		swipeAhead = Resources.Load("fx/swipe-ahead") as AudioClip;
		swipeRight = Resources.Load("fx/swipe-right") as AudioClip;
		swipeLeft = Resources.Load("fx/swipe-left") as AudioClip;
		continue_game = Resources.Load ("instructions/Loaded saved game") as AudioClip;
		new_game = Resources.Load ("instructions/New game started") as AudioClip;
		at_confirm = false;
		reset_audio = false;
		init_input ();
	}

	void init_input(){
		TouchTapCount = 0;
		menuTapTime = 0f;
		multiTapStartTime = 0f;
		swp_lock = false;
		inputSFX = Resources.Load ("fx/inputSFX") as AudioClip;
	}

	void play_audio(){
		if (!at_confirm) {
			if (SoundManager.instance.PlayVoice (clips [cur_clip], reset_audio)) {
				reset_audio = false;
				cur_clip += 1;
				if (cur_clip >= total_clip)
					cur_clip = 0;
			}
		} else {
			if (SoundManager.instance.PlayVoice (confirm_list [cur_clip], reset_audio)) {
				reset_audio = false;
				cur_clip += 1;
				if (cur_clip >= total_confirm_clip)
					cur_clip = 0;
			}
		}
	}

	float multiTapStartTime;
	int TouchTapCount;
	const float multiTapCD = 0.4f;//make multitap easier
	const float menuUpdateCD = 0.5f;//shortest time between turn on/off pause menu
	bool swp_lock = false;//stop very fast input
	AudioClip inputSFX;
	public Text debug_text;
	float menuTapTime;

	// Update is called once per frame
	void Update () {
		play_audio ();

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			if(!at_confirm){
				GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
				SoundManager.instance.PlayVoice(continue_game, true);
				SceneManager.LoadScene("Main");
			}
			//SoundManager.instance.PlaySingle(swipeRight);
		} else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			if(at_confirm){
				GameMode.instance.gamemode = GameMode.Game_Mode.MAIN;
				SoundManager.instance.PlayVoice(new_game, true);
				SceneManager.LoadScene("Main");
			}
			//SoundManager.instance.PlaySingle(swipeLeft);
		} else if (Input.GetKeyUp("f")) {
			//SceneManager.LoadScene("Main");
			//SoundManager.instance.PlaySingle(swipeAhead);
		} else if (Input.GetKeyUp("e")) {
			if(!at_confirm){
				at_confirm = true;
				cur_clip = 0;
				reset_audio = true;
			}
			else{
				at_confirm = false;
				cur_clip = 0;
				reset_audio = true;
			}
			//SoundManager.instance.PlaySingle(swipeAhead);
		}

		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		float ECHO_TOUCH_TIME = 0.2f;
		float TOUCH_TIME = 0.02f;
		float MENU_TOUCH_TIME = 1.5f;
		//Check if Input has registered more than zero touches
		int numTouches = Input.touchCount;

		Touch myTouch;
		Vector2 touchEndpos;
		BoardManager.Direction swp_dir = BoardManager.Direction.OTHER;

		//update all timers
		//update TouchTapCount part 1
		if( (Time.time - multiTapStartTime) >= multiTapCD ){
			multiTapStartTime = Time.time;
			TouchTapCount = 0;
		}

		//collect raw data from the device
		if (numTouches > 0) {
			//Store the first touch detected.
			myTouch = Input.touches[0];

			//Check if the phase of that touch equals Began
			if (myTouch.phase == TouchPhase.Began){
				//If so, set touchOrigin to the position of that touch
				touchOrigin = myTouch.position;
				touchTime = Time.time;
				swp_lock = true;
			} else if ((myTouch.phase == TouchPhase.Ended) && swp_lock){//deals with swipe and multiple taps
				//Set touchEnd to equal the position of this touch
				touchEndpos = myTouch.position;
				float x = touchEndpos.x - touchOrigin.x;
				float y = touchEndpos.y - touchOrigin.y;		

				if (Mathf.Abs(x) > Mathf.Abs(y) && Mathf.Abs(x) >= minSwipeDist){//right & left
					if (x > 0)//right
						swp_dir = BoardManager.Direction.RIGHT;
					else//left
						swp_dir = BoardManager.Direction.LEFT;
				} else if (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) {//up & down
					if (y > 0)//up/front
						swp_dir = BoardManager.Direction.FRONT;
					else//down/back
						swp_dir = BoardManager.Direction.BACK;
				}

				//update TouchTapCount part 2
				if( (Time.time - multiTapStartTime) < multiTapCD ){
					TouchTapCount += myTouch.tapCount;
				}

				swp_lock = false;//flip the lock, until we find another TouchPhase.Began
			}
		}

		debug_text.text = "numTouches:" + numTouches.ToString() + "\n"
						+ "TouchTapCount: " + TouchTapCount.ToString() + "\n"
						+ "at_confirm: " + at_confirm.ToString() + "\n";

		//process the data
		if(Time.time - menuTapTime >= menuUpdateCD){
			menuTapTime= Time.time;
			if( (!at_confirm)&&(TouchTapCount >= 2)&&(swp_dir == BoardManager.Direction.OTHER) ){
				cur_clip = 0;
				reset_audio = true;
				at_confirm = true;
				SoundManager.instance.PlaySingle(inputSFX);
				TouchTapCount = 0;
			}else if( (at_confirm)&&(TouchTapCount >= 2)&&(swp_dir == BoardManager.Direction.OTHER) ){
				cur_clip = 0;
				reset_audio = true;
				at_confirm = false;
				SoundManager.instance.PlaySingle(inputSFX);
				TouchTapCount = 0;
			}
		}

		if(numTouches > 0){//turn, get echo, etc.
			if(!at_confirm){
				if(swp_dir == BoardManager.Direction.RIGHT){
					SoundManager.instance.PlaySingle(inputSFX);
					GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
					SceneManager.LoadScene("Main");
					SoundManager.instance.PlayVoice(continue_game, true);
				}
			}else{//at_confirm = true
				if(swp_dir == BoardManager.Direction.LEFT){
					at_confirm = false;
					SoundManager.instance.PlaySingle(inputSFX);
					GameMode.instance.gamemode = GameMode.Game_Mode.MAIN;
					SceneManager.LoadScene("Main");
					SoundManager.instance.PlayVoice(new_game, true);					
				}
			}
		}
		#endif //End of mobile platform dependendent compilation section started above with #elif	
	}
}
