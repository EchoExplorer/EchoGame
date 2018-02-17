﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A script to display the user agreement dialogues.
/// This is attached to the ``GameManager`` GameObject in the main_pre scene.
/// </summary>
public class GM_main_pre : MonoBehaviour
{
    int cur_clip = 0;
    bool at_confirm = false;
    bool reset_audio = false;

    enum SelectMode { NONE, CONTINUE, NEW, CONFIRM }

    eventHandler eh;
    CDTimer TriggerStartNewGame;

    // Use this for initialization
    void Start()
    {
        init();
    }

    void OnLevelWasLoaded(int index)
    {
        init();
    }

    void init()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        at_confirm = false;
        reset_audio = false;
        eh = new eventHandler(InputModule.instance);
        TriggerStartNewGame = new CDTimer(1f, InputModule.instance);
        TriggerStartNewGame.TakeDownTime();
    }

    /// <summary>
    /// Plays instruction clips to select game modes.
    /// </summary>
	void play_audio()
    {
        if (!at_confirm)
        {
            if (SoundManager.instance.PlayVoice(Database.instance.MainPreGameClips[cur_clip], reset_audio))
            {
                reset_audio = false;
                cur_clip += 1;
                if (cur_clip >= Database.instance.MainPreGameClips.Length)
                    cur_clip = 0;
            }
        }
        else
        {
            if (SoundManager.instance.PlayVoice(Database.instance.MainPreConfirmClips[cur_clip], reset_audio))
            {
                reset_audio = false;
                cur_clip += 1;
                if (cur_clip >= Database.instance.MainPreConfirmClips.Length)
                    cur_clip = 0;
            }
        }
    }

    /// <summary>
    /// Checks user input with raw touch data and transitions to the next scene according to the input.
    /// </summary>
    void Update()
    {
        play_audio();

        SelectMode selectMode = SelectMode.NONE;

        //Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR

        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData();
            switch (ie.keycode)
            {
                //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
                case KeyCode.RightArrow:
                    if (!at_confirm)
                    {
                        selectMode = SelectMode.CONTINUE;
                    }
                    break;
                case KeyCode.LeftArrow:
                    if (at_confirm)
                    {
                        selectMode = SelectMode.NEW;
                    }
                    break;
                case KeyCode.E:
                    selectMode = SelectMode.CONFIRM;
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
				GameMode.instance.gamemode = GameMode.Game_Mode.RESTART;
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
                        selectMode = SelectMode.CONTINUE;
					}
				} else if (ie.isLeft){
					if(!at_confirm){
						//nothing
					}else{//at_confirm
                        selectMode = SelectMode.NEW;
					}
				}
			}
			else if ( (ie.cumulativeTouchNum >= 2)&&(!ie.hasDir()) && TriggerStartNewGame.CDfinish()){
                selectMode = SelectMode.CONFIRM;
			}
		}

#endif //End of mobile platform dependendent compilation section started above with #elif

        switch (selectMode)
        {
            case SelectMode.CONTINUE:
                SoundManager.instance.PlaySingle(Database.instance.inputSFX);
                SoundManager.instance.PlayVoice(Database.instance.MainPreContinueGame, true);
                SceneManager.LoadScene("Main");
                break;
            case SelectMode.NEW:
                SoundManager.instance.PlaySingle(Database.instance.inputSFX);
                if (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL)
                    GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL_RESTART;
                else
                    GameMode.instance.gamemode = GameMode.Game_Mode.RESTART;
                SoundManager.instance.PlayVoice(Database.instance.MainPreNewGame, true);
                // Utilities.write_save(0); ???
                SceneManager.LoadScene("Main");
                break;
            case SelectMode.CONFIRM:
                cur_clip = 0;
                reset_audio = true;
                at_confirm = !at_confirm;
                SoundManager.instance.PlaySingle(Database.instance.inputSFX);
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
                TriggerStartNewGame.reset();
#endif
                break;
            default:
                break;
        }
    }
}
