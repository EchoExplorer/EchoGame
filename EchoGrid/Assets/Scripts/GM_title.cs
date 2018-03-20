﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// A script to display the user agreement dialogues.
/// This is attached to the ``GameManager`` GameObject in the Title scene.
/// </summary>
public class GM_title : MonoBehaviour
{
    int cur_clip = 0;
    int orti_clip = 0;
    int cmd_cur_clip = 0;

    float time_interval = 2.0f;
    bool reset_audio = false;
    bool listenToCmd = false;
    public bool toMainflag = false;
    Text titleText;
    bool doneTesting = false;
    eventHandler eh;

    public bool isUsingTalkback = false; // Tells us if the player has told us that they are using Talkback or not.

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    enum Direction { NONE, UP, DOWN, LEFT, RIGHT }

    /// <summary>
    /// Sets up a reference to the GameMode module so it can set up its singleton.
    /// </summary>
    void Start()
    {
        titleText = GameObject.Find("ContactText").GetComponent<Text>();
        eh = new eventHandler(InputModule.instance);
    }

    bool determined_talkback = false;
    bool plugin_earphone = false;
    bool environment_setup = false;
    bool orientation_correct = false;
    bool clip0_reset = true;
    bool clip1_reset = true;
    bool clip2_reset = true;

    void play_audio()
    {
        if ((determined_talkback == true) && !plugin_earphone)
        {
            if (SoundManager.instance.PlayVoice(Database.instance.settingClips[0], clip0_reset, 1))
            {
                clip0_reset = false;
            }
            return;
        }
        if ((determined_talkback == true) && (plugin_earphone == true) && !orientation_correct)
        {
            if (!Utilities.isDeviceLandscape())
            {//not landscape!
                if (SoundManager.instance.PlayVoice(Database.instance.settingClips[1], clip1_reset))
                {
                    clip1_reset = false;
                }
                return;

            }
            else
            {
                if (SoundManager.instance.PlayVoice(Database.instance.settingClips[2], clip2_reset))
                {
                    clip2_reset = false;
                }
                orientation_correct = true;
            }
        }
        if ((environment_setup == true) && !listenToCmd)
        {
            if (SoundManager.instance.PlayVoice(Database.instance.TitleClips[cur_clip], reset_audio))
            {
                reset_audio = false;
                cur_clip += 1;
                if (cur_clip >= Database.instance.TitleClips.Length)
                    cur_clip = 0;
            }
        }
        else if ((environment_setup == true) && listenToCmd)
        {
            if (SoundManager.instance.PlayVoice(Database.instance.TitleCmdlistClips[cmd_cur_clip], reset_audio))
            {
                reset_audio = false;
                cmd_cur_clip += 1;
                if (cmd_cur_clip >= Database.instance.TitleCmdlistClips.Length)
                    cmd_cur_clip = 0;
            }
        }
    }

    /// <summary>
    /// Checks for an internet connection, and plays instructions.
    ///  Progresses to the main_pre scene for regular gameplay, or the main scene
    ///  for the tutorial by analyzing the touch data.
    /// </summary>
    void Update()
    {
        if (Const.TEST_CONNECTION)
        {
            if (!doneTesting)
            {
                string str = Utilities.check_InternetConnection();
                if (str.Length == 0)
                {//we're good to go
                    doneTesting = true;
                    titleText.text = Database.titleText_main;
                }
                else
                    titleText.text = str;
            }
        }

        play_audio();

        Direction inputDirection = Direction.NONE;

// Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        if (eh.isActivate() && doneTesting) // isActivate() has side effects so this order is required...
        {
			InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs

			// Do something based on this event info.
            switch (ie.keycode)
            {
            	// If the 'f' key was pressed.
                case KeyCode.F:
                	// If the player has plugged in headphones and single tapped, let them perform actions for the main menu.
                    if ((determined_talkback == true) && !plugin_earphone)
                    {
                    	debugPlayerInfo = "Tap registered. Earphones in.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        plugin_earphone = true;	// The player has plugged in earphones.
                    } 
                    // If the player's game environment is set up properly, let them go to the main menu.
                    else if ((determined_talkback == true) && (plugin_earphone == true) && (orientation_correct == true) && !environment_setup)
                    {
                    	debugPlayerInfo = "Tap registered. Game environment set up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        environment_setup = true;
                        reset_audio = true;
                    }
                    break;             
                // If the right arrow key was pressed.
                case KeyCode.RightArrow:
                    // If the player has not informed us if they are using Talkback or not.
                    if (determined_talkback == false)
                    {
                        debugPlayerInfo = "Swiped right. Player is not using Talkback.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        isUsingTalkback = false; // The player has told us they are not using Talkback.
                        determined_talkback = true; // Determined if the player is using Talkback or not.
                    }
                    // If the player's game environment is set up properly.
                    if ((determined_talkback == true) && (plugin_earphone == true) && (orientation_correct == true) && (environment_setup == true))
					{
						inputDirection = Direction.RIGHT;
					}
                    break;
                // If the left arrow key was pressed.
                case KeyCode.LeftArrow:
                    // If the player has not informed us if they are using Talkback or not.
                    if (determined_talkback == false)
                    {
                        debugPlayerInfo = "Swiped left. Player is using Talkback.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        isUsingTalkback = true; // The player has told us they are using Talkback.
                        determined_talkback = true; // Determined if the player is using Talkback or not.
                    }
                    // If the player's game environment is set up properly.
                    if ((determined_talkback == true) && (plugin_earphone == true) && (orientation_correct == true) && (environment_setup == true))
                    {
						inputDirection = Direction.LEFT;
					}
                    break;
                // If the up arrow key was pressed.
                case KeyCode.UpArrow:
                    // If the player's game environment is set up properly.
                    if ((determined_talkback == true) && (plugin_earphone == true) && (orientation_correct == true) && (environment_setup == true))
                    {
						inputDirection = Direction.UP;
					}
                    break;
                // If the down arrow key was pressed.
                case KeyCode.DownArrow:
                    // If the player's game environment is set up properly.
                    if ((determined_talkback == true) && (plugin_earphone == true) && (orientation_correct == true) && (environment_setup == true))
                    {
						inputDirection = Direction.DOWN;
					}
                    break;
                default:
                    break;
            }
        }
#endif
// Check if we are running on iOS/Android.
#if UNITY_IOS || UNITY_ANDROID
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Screen.orientation = ScreenOrientation.Landscape;

		if (eh.isActivate() && doneTesting) 
		{  // isActivate() has side effects so this order is required...
			InputEvent ie = eh.getEventData();  // Get input event data from InputModule.cs

            // If the player has not informed us if they are using Talkback or not.
            if (determined_talkback == false)
            {
                // If a swipe was registered.
                if (ie.isSwipe == true)
                {
                    // If the swipe was left, the user is using Talkback.
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Swiped left. Player is using Talkback.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        isUsingTalkback = true; // The player has told us they are using Talkback.
                        determined_talkback = true; // Determined if the player is using Talkback or not.
                    }
                    // If the swipe was right, the user is not using Talkback.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Swiped right. Player is not using Talkback.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        isUsingTalkback = false; // The player has told us they are not using Talkback.
                        determined_talkback = true; // Determined if the player is using Talkback or not.
                    }
                }           
            }
            // If the player has not put in headphones.
            else if ((determined_talkback == true) && !plugin_earphone)
            {
                // If a tap was registered.
                if (ie.isTap == true)
                {
					debugPlayerInfo = "Tap registered. Earphones in.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    plugin_earphone = true; // The player has put in headphones.
                }
            }

            // If the player's game environment is set up properly.
            else if ((determined_talkback == true) && (plugin_earphone == true) && (orientation_correct == true) && !environment_setup)
            {
                // If a tap was registered.
                if (ie.isTap == true)
                {
					debugPlayerInfo = "Tap registered. Game environment set up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    environment_setup = true; // Player environment is now set up.
					reset_audio = true;
                }
            }         
            else
            {
            	// If a swipe was registered.
                if (ie.isSwipe == true)
                {
                	// If the swipe was right.
				    if (ie.isRight == true)
				    {
					    inputDirection = Direction.RIGHT;
				    }
				    // If the swipe was left.
				    else if (ie.isLeft == true)
				    {
					    inputDirection = Direction.LEFT;
				    }
				    // If the swipe was up.
				    else if (ie.isUp == true)
				    {
					    inputDirection = Direction.UP;
				    } 
				    // If the swipe was down.
				    else if (ie.isDown == true)
				    {
					    inputDirection = Direction.DOWN;
				    }
			    }
            }
		}
#endif //End of mobile platform dependendent compilation section started above with #elif
        if ((determined_talkback == false) || !plugin_earphone || !environment_setup) 
        {
        	return;
        }
        switch (inputDirection)
        {
        	// If the player swiped right, move to the pregame menu to continue where you left off.
            case Direction.RIGHT:
            	debugPlayerInfo = "Swiped right. Moving to pregame menu to continue where you left off.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
				SceneManager.LoadScene("Main_pre"); // Move to pregame menu.
				SoundManager.instance.PlayVoice(Database.instance.TitletoMainClips[0], true);
                break;
            // If the player swiped left, move to the pregame menu to start the tutorial.
            case Direction.LEFT:
            	debugPlayerInfo = "Swiped left. Moving to pregame menu to start tutorial.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                SceneManager.LoadScene("Main_pre"); // Move to pregame menu.
				SoundManager.instance.PlayVoice(Database.instance.TitletoMainClips[0], true);
				//SceneManager.LoadScene("Main");
                break;
            // If the player swiped up, listen to the commands.
            case Direction.UP:
            	debugPlayerInfo = "Swiped up. Listening to commands.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
				if (!listenToCmd)
				{
					listenToCmd = true;
					reset_audio = true;
					cur_clip = 0;
					cmd_cur_clip = 0;
				}
				else
				{
					listenToCmd = false;
					reset_audio = true;
					cur_clip = 0;
					cmd_cur_clip = 0;
				}
                break;
            // If the player swiped down, do nothing.
            case Direction.DOWN:
            	debugPlayerInfo = "Swiped down. Does nothing here.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
				//credit
                break;
            default:
                break;
        }
    }
}
