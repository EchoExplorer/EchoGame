using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GM_title : MonoBehaviour {
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
		if (Const.TEST_CONNECTION) {
			if (!doneTesting) {
				string str = Utilities.check_InternetConnection ();
				if (str.Length == 0) {//we're good to go
					doneTesting = true;
					titleText.text = Database.titleText_main;
				} else
					titleText.text = str;
			}
		}

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

		if(eh.isActivate()){
			InputEvent ie = eh.getEventData();

			if( (ie.touchNum == 1)&&(!ie.isRotate) ){
				if (ie.isRight){
					GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
					SceneManager.LoadScene("Main_pre");
					SoundManager.instance.PlayVoice(Database.instance.TitletoMainClips[0], true);
				} else if (ie.isLeft){
					GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
					SceneManager.LoadScene("Main");
				} else if (ie.isUp){
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
				} else if (ie.isDown){
					SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
					//credit
				}
			}
		}
		#endif //End of mobile platform dependendent compilation section started above with #elif	
	}
}
