using UnityEngine;
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

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

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
                	// If we have not confirmed we want to start a new game and have swiped right, set mode to Continue.
                    if (!at_confirm)
                    {	
                        selectMode = SelectMode.CONTINUE;
                    }
                    break;
                // If the left arrow key was pressed.
                case KeyCode.LeftArrow:
                	// If we have confirmed we want to start a new game and have swiped left, set mode to New.
                    if (at_confirm)
                    {
                        selectMode = SelectMode.NEW;
                    }
                    break;
                // If the 'f' key was pressed.
                case KeyCode.F:
            		// We have confirmed we want to start a new game, so set mode to Confirm.
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
					// If we have not confirmed we want to start a new game and have swiped right, set mode to Continue.
					if (!at_confirm)
					{
                        selectMode = SelectMode.CONTINUE;
					}
				} 
				// If the swipe was right.
				else if (ie.isLeft == true)
				{
					// If we have not confirmed we want to start a new game, do nothing.
					if (!at_confirm)
					{
						// nothing
					}
					// If we have confirmed we want to start a new game and have swiped left, set mode to New.
					else
					{ 
                        selectMode = SelectMode.NEW;
					}
				}
			}
			// If a tap was registered and we are able to start a new game, set mode to Continue.
			else if ((ie.isTap == true) && TriggerStartNewGame.CDfinish())
			{
                selectMode = SelectMode.CONFIRM;
			}
		}
#endif //End of mobile platform dependendent compilation section started above with #elif
        switch (selectMode)
        {
        	// If mode is set to Continue, we have swiped right, so continue from where we left off.
            case SelectMode.CONTINUE:
            	debugPlayerInfo = "Swiped right. Continuing from where you left off.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                SoundManager.instance.PlaySingle(Database.instance.inputSFX);
                SoundManager.instance.PlayVoice(Database.instance.MainPreContinueGame, true);
                SceneManager.LoadScene("Main"); 
                break;
            // If mode is set to New, we have confirmed and swiped left, so start a new game from either the tutorial or the first non-tutorial level.
            case SelectMode.NEW:
            	debugPlayerInfo = "Swiped left. Starting new game.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                SoundManager.instance.PlaySingle(Database.instance.inputSFX);
                if (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL)
                    GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL_RESTART;
                else
                    GameMode.instance.gamemode = GameMode.Game_Mode.RESTART;
                SoundManager.instance.PlayVoice(Database.instance.MainPreNewGame, true);
                // Utilities.write_save(0); ???
                SceneManager.LoadScene("Main");
                break;
            // If mode is set to Confirm, we have tapped to confirm we want to start a new game, so let the player swipe left to start.
			case SelectMode.CONFIRM:
				debugPlayerInfo = "Tap registered. Confirmed action.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                cur_clip = 0;
                reset_audio = true;
                at_confirm = !at_confirm;
                SoundManager.instance.PlaySingle(Database.instance.inputSFX);
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
