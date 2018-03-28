﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// A script to display the user agreement dialogues.
/// This is attached to the ``GameManager`` GameObject in the main_pre scene.
/// </summary>
public class GM_main_pre : MonoBehaviour
{
    int cur_clip = 0;
    bool at_confirm = false;
    bool reset_audio = false;

    enum SelectMode { NONE, CONTINUE, NEW, CONFIRM, BACK }

    eventHandler eh;
    CDTimer TriggerStartNewGame;

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    public static bool hasGoneThroughSetup = false;

    List<AudioClip> clips;

    bool canRepeat = true;

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
        hasGoneThroughSetup = true;
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
            if (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL)
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    if (GM_title.isUsingTalkback == true)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.instance.preGameMenuClips[1], Database.instance.preGameMenuClips[3], Database.instance.preGameMenuClips[7] };
                        SoundManager.instance.PlayClips(clips);
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.instance.preGameMenuClips[0], Database.instance.preGameMenuClips[2], Database.instance.preGameMenuClips[6] };
                        SoundManager.instance.PlayClips(clips);
                    }
                }
            }
            else
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    if (GM_title.isUsingTalkback == true)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.instance.preGameMenuClips[1], Database.instance.preGameMenuClips[5], Database.instance.preGameMenuClips[7] };
                        SoundManager.instance.PlayClips(clips);
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.instance.preGameMenuClips[0], Database.instance.preGameMenuClips[4], Database.instance.preGameMenuClips[6] };
                        SoundManager.instance.PlayClips(clips);
                    }
                }
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

// Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
		// Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
        if (eh.isActivate())
        {
			InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

			// Do something based on this event info.
            switch (ie.keycode)
            {
              	// If the right arrow key was pressed.
                case KeyCode.RightArrow:
                    selectMode = SelectMode.CONTINUE; // If we have swiped right, set mode to Continue.
                    break;
                // If the left arrow key was pressed.
                case KeyCode.LeftArrow:                	
                    selectMode = SelectMode.NEW; // If we have swiped left, set mode to New.
                    break;
                // If the down arrow key was pressed.
                case KeyCode.DownArrow:
                    selectMode = SelectMode.BACK; // If we want to go back to the main menu.
                    break;
                // If the 'f' key was pressed.
                case KeyCode.F:
                    // We have swiped left to start a new game and confirmed that this is the action we want, so set mode to Confirm.
                    if (at_confirm)
                    {
                        selectMode = SelectMode.CONFIRM;
                    }
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
#endif
        // Check if we are running on iOS/Android.
#if UNITY_IOS || UNITY_ANDROID
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		if (eh.isActivate())
		{
			InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

			// If a swipe was recognized.
			if (ie.isSwipe == true)
			{
				// If the swipe was right.
				if (ie.isRight == true)
				{                    
                    selectMode = SelectMode.CONTINUE; // If we have swiped right, set mode to Continue.
				} 
				// If the swipe was left.
				else if (ie.isLeft == true)
				{					
                    selectMode = SelectMode.NEW; // If we have swiped left, set mode to New.
				}
                // If the swipe was down.
                else if (ie.isDown == true)
                {
                    selectMode = SelectMode.BACK; // Let the player go back to the main menu.
                }
			}
			// If a tap was registered and we are able to start a new game, set mode to Confirm.
			else if ((ie.isTap == true) && TriggerStartNewGame.CDfinish())
			{
                if (at_confirm)
                {
                    selectMode = SelectMode.CONFIRM; // We have swiped left to start a new game and confirmed that this is the action we want, so set mode to Confirm.
                }
			}
		}
#endif //End of mobile platform dependendent compilation section started above with #elif
        switch (selectMode)
        {
        	// If mode is set to Continue, we have swiped right, so continue from where we left off.
            case SelectMode.CONTINUE:
                debugPlayerInfo = "Swiped right. Continuing from where you left off.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                clips = new List<AudioClip>() { Database.instance.soundEffectClips[6], Database.instance.preGameMenuClips[12] };
                SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Main"), 2);
                break;
            // If mode is set to New, we have confirmed and swiped left, so start a new game from either the tutorial or the first non-tutorial level.
            case SelectMode.NEW:
                debugPlayerInfo = "Swiped left. Going to confirm we want to start a new game.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                
                cur_clip = 0;
                reset_audio = true;
                at_confirm = true;
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    if (GM_title.isUsingTalkback == true)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.instance.soundEffectClips[6], Database.instance.preGameMenuClips[8], Database.instance.preGameMenuClips[10] };
                        SoundManager.instance.PlayClips(clips);
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.instance.soundEffectClips[6], Database.instance.preGameMenuClips[8], Database.instance.preGameMenuClips[9] };
                        SoundManager.instance.PlayClips(clips);
                    }
                }
                break;
            // If mode is set to Confirm, we have tapped to confirm we want to start a new game, so let the player swipe left to start.
			case SelectMode.CONFIRM:
                debugPlayerInfo = "Tap registered. Confirmed we want to start a new game.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                               
                if (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL)
                {
                    GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL_RESTART;
                }
                else
                {
                    GameMode.instance.gamemode = GameMode.Game_Mode.RESTART;
                }
                // Utilities.write_save(0); ???
                BoardManager.finishedTutorialLevel1 = false;
                BoardManager.finishedTutorialLevel3 = false;
                clips = new List<AudioClip>() { Database.instance.soundEffectClips[6], Database.instance.preGameMenuClips[11] };
                SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Main"), 2);
                break;
            // If mode is set to Back, go back to the main menu.
            case SelectMode.BACK:
                debugPlayerInfo = "Swiped down. Going back to main menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[6]);
                SceneManager.LoadScene("Title_Screen");

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
#endif
#if UNITY_IOS || UNITY_ANDROID
                TriggerStartNewGame.reset();
#endif
                break;
            default:
                break;
        }
    }
}
