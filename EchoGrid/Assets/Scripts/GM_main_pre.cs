using UnityEngine;
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
    bool at_confirm = false;
    bool reset_audio = false;

    enum SelectMode { NONE, CONTINUE, NEW, CONFIRM, BACK }

    eventHandler eh;
    CDTimer TriggerStartNewGame;

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    public static bool hasGoneThroughSetup = false;

    List<AudioClip> clips;

    bool canRepeat = true;
    static bool firstConfirm = true;

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
        if (at_confirm == false)
        {
            if (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL)
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    // If the player is using Talkback.
                    if (GM_title.isUsingTalkback == true)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[1], Database.preGameMenuClips[3], Database.preGameMenuClips[7] };
                        SoundManager.instance.PlayClips(clips); // Play the appropriate clips.
                    }
                    // If the player is not using Talkback.
                    else if (GM_title.isUsingTalkback == false)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[0], Database.preGameMenuClips[2], Database.preGameMenuClips[6] };
                        SoundManager.instance.PlayClips(clips); // Play the appropriate clips.
                    }
                }
            }
            else
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    // If the player is using Talkback.
                    if (GM_title.isUsingTalkback == true)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[1], Database.preGameMenuClips[5], Database.preGameMenuClips[7] };
                        SoundManager.instance.PlayClips(clips); // Play the appropriate clips.
                    }
                    // If the player is not using Talkback.
                    else if (GM_title.isUsingTalkback == false)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[0], Database.preGameMenuClips[4], Database.preGameMenuClips[6] };
                        SoundManager.instance.PlayClips(clips); // Play the appropriate clips.
                    }
                }
            }          
        }
        else if (at_confirm == true)
        {
            if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
            {
                // If the player is using Talkback.
                if (GM_title.isUsingTalkback == true)
                {
                    if (firstConfirm == true)
                    {
                        firstConfirm = false;
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.preGameMenuClips[8], Database.preGameMenuClips[10] };
                        SoundManager.instance.PlayClips(clips); // Play the appropriate clips.
                    }
                    else
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[8], Database.preGameMenuClips[10] };
                        SoundManager.instance.PlayClips(clips); // Play the appropriate clips.
                    }
                }
                // If the player is not using Talkback.
                else if (GM_title.isUsingTalkback == false)
                {
                    if (firstConfirm == true)
                    {
                        firstConfirm = false;
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.preGameMenuClips[8], Database.preGameMenuClips[9] };
                        SoundManager.instance.PlayClips(clips); // Play the appropriate clips.
                    }
                    else
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[8], Database.preGameMenuClips[9] };
                        SoundManager.instance.PlayClips(clips); // Play the appropriate clips.
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
            // If a tap was registered.
            if (ie.isTap == true)
            {
                // We have swiped left to start a new game and confirmed that this is the action we want, so set mode to Confirm.
                if (at_confirm == true)
                {
                    canRepeat = true;
                    selectMode = SelectMode.CONFIRM; // We have tapped to confirm we want to start a new game, so set mode to Confirm.
                }
            }
            // If a swipe was registered.
            else if (ie.isSwipe == true)
            {
                // If the swipe was left.
                if (ie.isLeft == true)
                {
                    canRepeat = true;
                    selectMode = SelectMode.NEW; // If we have swiped left, set mode to New.
                }
                // If the swipe was right.
                if (ie.isRight == true)
                {
                    canRepeat = true;
                    selectMode = SelectMode.CONTINUE; // If we have swiped right, set mode to Continue.
                }
                // If the swipe was up.
                if (ie.isUp == true)
                {
                    BoardManager.finishedTutorialLevel1 = true;
                    BoardManager.finishedTutorialLevel3 = true;
                    SceneManager.LoadScene("Main");                    
                    // debugPlayerInfo = "This gesture does nothing in this menu.";
                    // DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                // If the swipe was down.
                if (ie.isDown == true)
                {
                    canRepeat = true;
                    selectMode = SelectMode.BACK; // If we have swiped down, set mode to Back.   
                }
            }
            // If a rotation was registered.
            else if (ie.isRotate == true)
            {
                debugPlayerInfo = "This gesture does nothing in this menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
            // If a hold was registered.
            else if (ie.isHold == true)
            {
                debugPlayerInfo = "This gesture does nothing in this menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }         
            // If there was an unrecognized gesture made.
            else if (ie.isUnrecognized == true)
            {
                // If this error was registered.
                if (ie.isTapGapError == true)
                {
                    // If a clip is not playing, tell them about the error.
                    if ((SoundManager.instance.voiceSource.isPlaying == false) || (SoundManager.instance.clipSource.isPlaying == false))
                    {
                        SoundManager.instance.PlayVoice(Database.errorClips[0], true); // Play the appropriate clip.
                    }
                    debugPlayerInfo = "Nothing happened due to error with gap between tap and the most recent gesture.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftGapError == true)
                {
                    // If a clip is not playing, tell them about the error.
                    if ((SoundManager.instance.voiceSource.isPlaying == false) || (SoundManager.instance.clipSource.isPlaying == false))
                    {
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true); // Play the appropriate clip.
                    }
                    debugPlayerInfo = "Nothing happened due to error with gap between swipe left and the most recent gesture.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                // If this error was registered.
                else if (ie.isSwipeRightGapError == true)
                {
                    // If a clip is not playing, tell them about the error.
                    if ((SoundManager.instance.voiceSource.isPlaying == false) || (SoundManager.instance.clipSource.isPlaying == false))
                    {
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true); // Play the appropriate clip.
                    }
                    debugPlayerInfo = "Nothing happened due to error with gap between swipe right and the most recent gesture.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.   
                }
                // If this error was registered.
                else if (ie.isSwipeUpGapError == true)
                {
                    // If a clip is not playing, tell them about the error.
                    if ((SoundManager.instance.voiceSource.isPlaying == false) || (SoundManager.instance.clipSource.isPlaying == false))
                    {
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true); // Play the appropriate clip.
                    }
                    debugPlayerInfo = "Nothing happened due to error with gap between swipe up and the most recent gesture.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                // If this error was registered.
                else if (ie.isSwipeDownGapError == true)
                {
                    // If a clip is not playing, tell them about the error.
                    if ((SoundManager.instance.voiceSource.isPlaying == false) || (SoundManager.instance.clipSource.isPlaying == false))
                    {
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true); // Play the appropriate clip.
                    }
                    debugPlayerInfo = "Nothing happened due to error with gap between swipe down and the most recent gesture.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.              
                }
                // If this error was registered.
                else if (ie.isRotationGapError == true)
                {
                    // If a clip is not playing, tell them about the error.
                    if ((SoundManager.instance.voiceSource.isPlaying == false) || (SoundManager.instance.clipSource.isPlaying == false))
                    {
                        SoundManager.instance.PlayVoice(Database.errorClips[14], true); // Play the appropriate clip.
                    }
                    debugPlayerInfo = "Nothing happened due to error with gap between turn and the most recent gesture.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                 
                }
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
                    canRepeat = true;
                    selectMode = SelectMode.CONTINUE; // If we have swiped right, set mode to Continue.
				} 
				// If the swipe was left.
				else if (ie.isLeft == true)
				{
                    canRepeat = true;
                    selectMode = SelectMode.NEW; // If we have swiped left, set mode to New.
				}
                // If the swipe was down.
                else if (ie.isDown == true)
                {
                    canRepeat = true;
                    selectMode = SelectMode.BACK; // If we have swiped down, set mode to Back.
                }
                // If the swipe was up.
                else if (ie.isUp == true)
                {
                    BoardManager.finishedTutorialLevel1 = true;
                    BoardManager.finishedTutorialLevel3 = true;
                    SceneManager.LoadScene("Main");
                    // debugPlayerInfo = "This gesture does nothing in this menu.";
                    // DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
			}
			
            // If a tap was registered and we are able to start a new game, set mode to Confirm.
			else if ((ie.isTap == true) && TriggerStartNewGame.CDfinish())
			{
                if (at_confirm)
                {
                    canRepeat = true;
                    selectMode = SelectMode.CONFIRM; // We have tapped to confirm we want to start a new game, so set mode to Confirm.
                }
			}                     

            // If a hold or rotation was registered.
            else if ((ie.isHold == true) || (ie.isRotate == true))
            {
                debugPlayerInfo = "This gesture does nothing in this menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }

            // If there was an unrecognized gesture made.
            else if (ie.isUnrecognized == true)
            {
                // If a clip is not playing, tell them about the error.
                if ((SoundManager.instance.voiceSource.isPlaying == false) || (SoundManager.instance.clipSource.isPlaying == false))
                {
                    // If this error was registered.
                    if (ie.isTapGapError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with gap between a tap and the most recent gesture.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[0], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isTapHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[1], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isTapVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[2], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isTapRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[3], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeLeftGapError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with gap between swipe left and the most recent gesture.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true); // Play the appropriate clip.
                    }
                    else if (ie.isSwipeRightGapError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with gap between swipe right and the most recent gesture.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true); // Play the appropriate clip.
                    }
                    else if (ie.isSwipeUpGapError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with gap between swipe up and the most recent gesture.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true); // Play the appropriate clip.
                    }
                    else if (ie.isSwipeDownGapError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with gap between swipe down and the most recent gesture.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeLeftHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe left.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[10], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeRightHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeUpVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[7], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeDownVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[9], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeLeftRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe left.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[5], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeRightRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[5], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeUpRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[5], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeDownRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[5], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isRotationGapError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with gap between turn and the most recent gesture.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[14], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isRotationAngleError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with angle on rotation.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[15], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isHoldHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[17], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isHoldVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isHoldRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[16], true); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isLessThanThreeError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with less than three fingers on the screen.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[24], true); // Tell the player they had less than three fingers on the screen.
                    }
                    // If this error was registered.
                    else if (ie.isMoreThanThreeError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with more than three fingers on the screen.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[25], true); // Tell the player they had more than three fingers on the screen.
                    }
                }
            }           
        }        

#endif // End of mobile platform dependendent compilation section started above with #elif

        switch (selectMode)
        {
        	// If mode is set to Continue, we have swiped right, so continue from where we left off.
            case SelectMode.CONTINUE:
                debugPlayerInfo = "Swiped right. Continuing from where you left off.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    firstConfirm = true;
                    clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.preGameMenuClips[12] };
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Main"), 3); // Play the appropriate clips.
                }
                break;
            // If mode is set to New, we have confirmed and swiped left, so start a new game from either the tutorial or the first non-tutorial level.
            case SelectMode.NEW:
                debugPlayerInfo = "Swiped left. Going to confirm we want to start a new game.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                
                at_confirm = true;
                canRepeat = true;                
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
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {                   
                    canRepeat = false;
                    firstConfirm = true;
                    clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.preGameMenuClips[11] };
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Main"), 3); // Play the appropriate clips.                  
                }
                break;
            // If mode is set to Back, go back to the main menu.
            case SelectMode.BACK:
                debugPlayerInfo = "Swiped down. Going back to main menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.     
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    firstConfirm = true;
                    clips = new List<AudioClip>() { Database.soundEffectClips[6] };
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1); // Play the appropriate clip.
                }                

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
