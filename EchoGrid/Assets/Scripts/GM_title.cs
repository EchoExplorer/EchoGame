using UnityEngine;
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

	string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    enum Direction { NONE, UP, DOWN, LEFT, RIGHT }

    /// <summary>
    /// Sets up a reference to the GameMode module so it can set up its singleton.
    /// </summary>
    void Start()
    {
        eh = new eventHandler(InputModule.instance);
    }

    bool plugin_earphone = false;
    bool second_tap = false;
    bool orient_correction = false;
    bool clip0_reset = true;
    bool clip1_reset = true;
    bool clip2_reset = true;
    void play_audio()
    {
        if (!plugin_earphone)
        {
            if (SoundManager.instance.PlayVoice(Database.instance.settingClips[0], clip0_reset, 1))
            {
                clip0_reset = false;
            }
            return;
        }
        if (!orient_correction)
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
                orient_correction = true;
                if (SoundManager.instance.PlayVoice(Database.instance.settingClips[2], clip2_reset))
                {
                    clip2_reset = false;
                }
            }
        }
        if (!listenToCmd)
        {
            if (SoundManager.instance.PlayVoice(Database.instance.TitleClips[cur_clip], reset_audio))
            {
                reset_audio = false;
                cur_clip += 1;
                if (cur_clip >= Database.instance.TitleClips.Length)
                    cur_clip = 0;
            }
        }
        else
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
                    if (!plugin_earphone)
                    {
                    	debugPlayerInfo = "Single tapped. Earphones in.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        plugin_earphone = true;	// The player has plugged in earphones.
                    } 
                    // If the player's game environment is set up properly, let them go to the main menu.
                    else if (!second_tap)
                    {
                    	debugPlayerInfo = "Single tapped second time.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        second_tap = true;
                        reset_audio = true;
                    }
                    break;
                // If the right arrow key was pressed.
                case KeyCode.RightArrow:
					if ((plugin_earphone == true) && (second_tap == true))
					{
						inputDirection = Direction.RIGHT;
					}
                    break;
                // If the left arrow key was pressed.
                case KeyCode.LeftArrow:
					if ((plugin_earphone == true) && (second_tap == true))
					{
						inputDirection = Direction.LEFT;
					}
                    break;
                // If the up arrow key was pressed.
                case KeyCode.UpArrow:
					if ((plugin_earphone == true) && (second_tap == true))
					{
						inputDirection = Direction.UP;
					}
                    break;
                // If the down arrow key was pressed.
                case KeyCode.DownArrow:
					if ((plugin_earphone == true) && (second_tap == true))
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
            if (!plugin_earphone)
            {
                if (ie.isSingleTap == true)
                {
					debugPlayerInfo = "Single tapped. Earphones in.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    plugin_earphone = true;
                }
            }
            else if (!second_tap)
            {
                if (ie.isSingleTap == true)
                {
					debugPlayerInfo = "Single tapped second time.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    second_tap = true;
					reset_audio = true;
                }
            }
            //if ((plugin_earphone == true) && (second_tap == true)) // Placing if like this is NOT EQUAL to the original one (else)!
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
        if (!plugin_earphone || !second_tap) 
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
