using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GM_main_pre : MonoBehaviour {
	int cur_clip = 0;
	bool at_confirm = false;
	bool reset_audio = false;

	eventHandler eh;
	CDTimer TriggerStartNewGame = new CDTimer(1f, InputModule.instance);

	// Use this for initialization
	void Start () {
		GameObject.Find ("GameMode").GetComponent <GameMode>().init ();
		init();
	}

	void OnLevelWasLoaded(int index){
		init ();
	}

	void init(){
		Screen.orientation = ScreenOrientation.Landscape;
		at_confirm = false;
		reset_audio = false;
		eh = new eventHandler (InputModule.instance);
		CDTimer TriggerStartNewGame = new CDTimer(1f, InputModule.instance);
		TriggerStartNewGame.TakeDownTime ();
	}

	void play_audio(){
		if (!at_confirm) {
			if (SoundManager.instance.PlayVoice (Database.instance.MainPreGameClips[cur_clip], reset_audio)) {
				reset_audio = false;
				cur_clip += 1;
				if (cur_clip >= Database.instance.MainPreGameClips.Length)
					cur_clip = 0;
			}
		} else {
			if (SoundManager.instance.PlayVoice (Database.instance.MainPreConfirmClips[cur_clip], reset_audio)) {
				reset_audio = false;
				cur_clip += 1;
				if (cur_clip >= Database.instance.MainPreConfirmClips.Length)
					cur_clip = 0;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		play_audio ();

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		if(eh.isActivate()){
			InputEvent ie = eh.getEventData();
			switch(ie.keycode){
			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
			case KeyCode.RightArrow:
				if(!at_confirm){
					GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
					SoundManager.instance.PlayVoice(Database.instance.MainPreContinueGame, true);
					SceneManager.LoadScene("Main");
				}
				//SoundManager.instance.PlaySingle(swipeRight);
				break;
			case KeyCode.LeftArrow:
				if(at_confirm){
					GameMode.instance.gamemode = GameMode.Game_Mode.MAIN;
					SoundManager.instance.PlayVoice(Database.instance.MainPreNewGame, true);
					SceneManager.LoadScene("Main");
				}
				//SoundManager.instance.PlaySingle(swipeLeft);
				break;
			case KeyCode.E:
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
				break;
			default:
				break;
			}
		}
		/*
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
		*/

		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		if(eh.isActivate()){
			InputEvent ie = eh.getEventData();

			if( (ie.touchNum == 1)&&(!ie.isRotate) ){
				if(ie.isRight){
					if(!at_confirm){
						SoundManager.instance.PlaySingle(Database.instance.inputSFX);
						GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
						SceneManager.LoadScene("Main");
						SoundManager.instance.PlayVoice(Database.instance.MainPreContinueGame, true);						
					}
				} else if (ie.isLeft){
					if(!at_confirm){
						//nothing
					}else{//at_confirm
						at_confirm = false;
						SoundManager.instance.PlaySingle(Database.instance.inputSFX);
						GameMode.instance.gamemode = GameMode.Game_Mode.MAIN;
						Utilities.write_save(0);
						SceneManager.LoadScene("Main");
						SoundManager.instance.PlayVoice(Database.instance.MainPreNewGame, true);	
					}
				}
				
			}
			else if ( (ie.cumulativeTouchNum >= 2)&&(!ie.hasDir()) ){
				if (!at_confirm && TriggerStartNewGame.CDfinish()){
					cur_clip = 0;
					reset_audio = true;
					at_confirm = true;
					SoundManager.instance.PlaySingle(Database.instance.inputSFX);
					TriggerStartNewGame.reset();
				}else if (TriggerStartNewGame.CDfinish()){//at_confirm
					cur_clip = 0;
					reset_audio = true;
					at_confirm = false;
					SoundManager.instance.PlaySingle(Database.instance.inputSFX);
					TriggerStartNewGame.reset();
				}
			}
		}

		#endif //End of mobile platform dependendent compilation section started above with #elif	
	}
}
