using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    bool repeatSetupClip = false;

    public static bool isUsingTalkback = true; // Tells us if the player has told us that they are using Talkback or not.

    bool inOptionsMenu = false;
    public static bool switch_click_toggle = false; // If switch_click_toggle is false, then play the odeon click, which is option 1 in database.

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    enum Direction { NONE, UP, DOWN, LEFT, RIGHT }

    /// <summary>
    /// Sets up a reference to the GameMode module so it can set up its singleton.
    /// </summary>
    void Start()
    {
        titleText = GameObject.Find("ContactText").GetComponent<Text>();
        eh = new eventHandler(InputModule.instance);

#if UNITY_STANDALONE
        // The textbox for leveltext and contact text are larger when in standalone than when building on PC or mobile for Android, so it should be made smaller so we can see everything.
        GameObject mainTextbox; // Main menu textbox object.
        mainTextbox = GameObject.Find("Canvas").gameObject.transform.Find("LevelImage").gameObject; // Find the textbox.
        mainTextbox.transform.localScale -= new Vector3(0.3f, 0.3f, 0.3f); // Make the textbox smaller.
#endif
    }

    public static bool determined_talkback = false;
    bool plugin_earphone = false;
    bool environment_setup = false;
    bool orientation_correct = false;

    List<AudioClip> clips;
    float[] balances;

    bool madeUnrecognizedGesture = false;

    bool canRepeat = true;

    void play_audio()
    {
        if (madeUnrecognizedGesture == false)
        {
            // Check if the player's phone is oriented correctly.
            if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == false))
            {
                // If the phone is not in landscape mode.
                if (!Utilities.isDeviceLandscape())
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        // Tell them to orient their phone to the proper position.
                        canRepeat = false;
                        if (repeatSetupClip == true)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.settingsClips[0] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[0] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                            repeatSetupClip = true;
                        }
                        return;
                    }
                }
                // If the phone is in landscape mode.
                else
                {
                    canRepeat = true;
                    orientation_correct = true; // Their phone is oriented correctly.
                    repeatSetupClip = false;
                }
            }
            // If the player has oriented their phone correctly, ask them to tell us if they are using Talkback or not.
            if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
            {
                // If the last set of clips is finished playing.
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    if (repeatSetupClip == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.settingsClips[1], Database.settingsClips[2] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                    }
                    else if (repeatSetupClip == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[1], Database.settingsClips[2] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        repeatSetupClip = true;
                    }
                    return;
                }
            }
            // If the player has oriented the phone correctly and told us if they are using talkback, then ask them to put in headphones.
            if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == false))
            {
                // If the last set of clips is finished playing.
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    // If the player is using Talkback.
                    if (isUsingTalkback == true)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[3], Database.settingsClips[5], Database.settingsClips[7] };
                        List<AudioClip> clips2 = new List<AudioClip>() { Database.soundEffectClips[0], Database.settingsClips[3], Database.settingsClips[5], Database.settingsClips[7] };
                        if (repeatSetupClip == true)
                        {
                            balances = new float[] { 0, 0, 0, 1 };
                            SoundManager.instance.PlayClips(clips2, balances, 1, null, 0, 0.5f, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            balances = new float[] { 0, 0, 0, 0, 1 };
                            SoundManager.instance.PlayClips(clips, balances, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                            repeatSetupClip = true;
                        }
                    }
                    // If the player is not using Talkback.
                    else if (isUsingTalkback == false)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[4], Database.settingsClips[6] };
                        List<AudioClip> clips2 = new List<AudioClip>() { Database.soundEffectClips[0], Database.settingsClips[4], Database.settingsClips[6] };
                        if (repeatSetupClip == true)
                        {
                            balances = new float[] { 0, 0, 1 };
                            SoundManager.instance.PlayClips(clips2, balances, 1, null, 0, 0.5f, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            balances = new float[] { 0, 0, 0, 1 };
                            SoundManager.instance.PlayClips(clips, balances, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                            repeatSetupClip = true;
                        }
                    }
                    return;
                }
            }
            // If the player has oriented the phone correctly, told us if they are using talkback, and have put in headphones, then tell them their game environment is set up.
            if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == false))
            {
                // If the last set of clips has finished playing.
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    // If the player is using Talkback.
                    if (isUsingTalkback == true)
                    {
                        canRepeat = false;
                        if (repeatSetupClip == true)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.settingsClips[9] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.   
                        }
                        else if (repeatSetupClip == false)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[9] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.   
                            repeatSetupClip = true;
                        }
                    }
                    // If the player is not using Talkback.
                    else if (isUsingTalkback == false)
                    {
                        canRepeat = false;
                        if (repeatSetupClip == true)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.settingsClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.  
                        }
                        else if (repeatSetupClip == false)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.  
                            repeatSetupClip = true;
                        }
                    }
                    return;
                }
            }
            // If the player's game environment has been set up and they have not swiped up, tell them what they can do at this menu.
            if (((GM_main_pre.hasGoneThroughSetup == true) || (environment_setup == true)) && (listenToCmd == false))
            {
                // If the last set of clips has finished playing.
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    // If the player is using Talkback.
                    if (isUsingTalkback == true)
                    {
                        canRepeat = false;
                        if (repeatSetupClip == true)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[0], Database.mainMenuClips[2], Database.mainMenuClips[4], Database.mainMenuClips[6], Database.mainMenuClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[0], Database.mainMenuClips[2], Database.mainMenuClips[4], Database.mainMenuClips[6], Database.mainMenuClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                            repeatSetupClip = true;
                        }
                    }
                    // If the player is not using Talkback.
                    else if (isUsingTalkback == false)
                    {
                        canRepeat = false;
                        if (repeatSetupClip == true)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[0], Database.mainMenuClips[1], Database.mainMenuClips[3], Database.mainMenuClips[5], Database.mainMenuClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[0], Database.mainMenuClips[1], Database.mainMenuClips[3], Database.mainMenuClips[5], Database.mainMenuClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                            repeatSetupClip = true;
                        }
                    }
                    return;
                }
            }
            // If the player's game environment is set up and they swipe up, give them a list of commands.
            if (((GM_main_pre.hasGoneThroughSetup == true) || (environment_setup == true)) && (listenToCmd == true))
            {
                if (canRepeat == true)
                {
                    // If the player is using Talkback.
                    if (isUsingTalkback == true)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[15], Database.mainMenuClips[17], Database.mainMenuClips[19], Database.mainMenuClips[21], Database.mainMenuClips[23] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                    }
                    // If the player is not using Talkback.
                    else if (isUsingTalkback == false)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[14], Database.mainMenuClips[16], Database.mainMenuClips[18], Database.mainMenuClips[20], Database.mainMenuClips[22] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                    }
                }
                // If the list of command hints have finished playing, go back to the menu options.
                if (SoundManager.instance.finishedAllClips == true)
                {
                    listenToCmd = false;
                }
            }
        }

        else if ((madeUnrecognizedGesture == true) && (SoundManager.instance.finishedClip == true))
        {
            madeUnrecognizedGesture = false;

            int i = 0;
            print("Interrupted clips:");
            foreach (AudioClip clip in SoundManager.clipsCurrentlyPlaying)
            {
                print("Clip " + i + ": " + clip.name);
                i++;
            }
            SoundManager.instance.PlayClips(SoundManager.clipsCurrentlyPlaying, SoundManager.currentBalances, 0, SoundManager.currentCallback, SoundManager.currentCallbackIndex, 0.5f, true);
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

        if (GM_main_pre.hasGoneThroughSetup == true)
        {
            determined_talkback = true;
            plugin_earphone = true;
            environment_setup = true;
            orientation_correct = true;
            repeatSetupClip = false;
        }

        play_audio();

        Direction inputDirection = Direction.NONE;

        // Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        if (eh.isActivate() && doneTesting) // isActivate() has side effects so this order is required...
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs

            // Do something based on this event info.
            // If a tap was registered.
            if (ie.isTap == true)
            {
                // If the player has plugged in headphones and single tapped, let them perform actions for the main menu.
                if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == false))
                {
                    debugPlayerInfo = "Tap registered. Earphones in.";
                    plugin_earphone = true; // The player has plugged in earphones.
                    repeatSetupClip = false;
                    reset_audio = true;
                    canRepeat = true;
                }
                // If the player's game environment is set up properly, let them go to the main menu.
                else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == false))
                {
                    debugPlayerInfo = "Tap registered. Game environment set up.";
                    environment_setup = true;
                    repeatSetupClip = false;
                    reset_audio = true;
                    canRepeat = true;
                }
                //
                else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                {
                    debugPlayerInfo = "Tap registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else
                {
                    debugPlayerInfo = "Tap registered. Does nothing in this menu.";
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
            // If a swipe was registered.
            else if (ie.isSwipe == true)
            {
                // If the swipe was left.
                if (ie.isLeft == true)
                {
                    // If the player has not informed us if they are using Talkback or not.
                    if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                    {
                        debugPlayerInfo = "Swiped left. Player is using Talkback.";
                        isUsingTalkback = true; // The player has told us they are using Talkback.
                        determined_talkback = true; // Determined if the player is using Talkback or not.
                        repeatSetupClip = false;
                        canRepeat = true;
                    }
                    //
                    else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                    {
                        debugPlayerInfo = "Swiped left. Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the player's game environment is set up properly.
                    else if ((GM_main_pre.hasGoneThroughSetup == true) || ((orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == true)))
                    {
                        inputDirection = Direction.LEFT;
                    }
                }
                // If the swipe was right.
                else if (ie.isRight == true)
                {
                    // If the player has not informed us if they are using Talkback or not.
                    if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                    {
                        debugPlayerInfo = "Swiped right. Player is not using Talkback.";
                        isUsingTalkback = false; // The player has told us they are not using Talkback.
                        determined_talkback = true; // Determined if the player is using Talkback or not.
                        repeatSetupClip = false;
                        canRepeat = true;
                    }
                    //
                    else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                    {
                        debugPlayerInfo = "Swiped right. Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the player's game environment is set up properly.
                    else if ((GM_main_pre.hasGoneThroughSetup == true) || ((orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == true)))
                    {
                        inputDirection = Direction.RIGHT;
                    }
                }
                // If the swipe was up.
                else if (ie.isUp == true)
                {
                    //
                    if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    //
                    else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the player's game environment is set up properly.
                    else if ((GM_main_pre.hasGoneThroughSetup == true) || ((orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == true)))
                    {
                        inputDirection = Direction.UP;
                    }
                }
                // If the swipe was down.
                else if (ie.isDown == true)
                {
                    //
                    if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    //
                    else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the player's game environment is set up properly.
                    else if ((GM_main_pre.hasGoneThroughSetup == true) || ((orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == true)))
                    {
                        inputDirection = Direction.DOWN;
                    }
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
            // If a rotation was registered.
            else if (ie.isRotate == true)
            {
                if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                {
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    }
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                //
                else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                {
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. Incorrect gesture made. You should tap.";
                    }
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. Incorrect gesture made. You should tap.";
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else
                {
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. This gesture does nothing in this menu.";
                    }
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. This gesture does nothing in this menu.";
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            // If a hold was registered.
            else if (ie.isHold == true)
            {
                //
                if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                //
                else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should tap.";
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else
                {
                    debugPlayerInfo = "Hold registered. This gesture does nothing in this menu.";
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }

            // If there was an unrecognized gesture made.
            if (ie.isUnrecognized == true)
            {
                madeUnrecognizedGesture = true;

                // If this error was registered.
                if (ie.isTapHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                    SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.                    
                }
                // If this error was registered.
                else if (ie.isTapVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                    SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isTapRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                    SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe left.";
                    SoundManager.instance.PlayVoice(Database.errorClips[3], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe right.";
                    SoundManager.instance.PlayVoice(Database.errorClips[4], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe up.";
                    SoundManager.instance.PlayVoice(Database.errorClips[5], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe down.";
                    SoundManager.instance.PlayVoice(Database.errorClips[6], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe left.";
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe right.";
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe up.";
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe down.";
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isRotationAngleError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with angle on rotation.";
                    SoundManager.instance.PlayVoice(Database.errorClips[8], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[9], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[10], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[11], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
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
            if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
            {
                // If a swipe was registered.
                if (ie.isSwipe == true)
                {
                    // If the swipe was left, the user is using Talkback.
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Swiped left. Player is using Talkback.";
                        isUsingTalkback = true; // The player has told us they are using Talkback.
                        determined_talkback = true; // Determined if the player is using Talkback or not.
                        repeatSetupClip = false;
                        reset_audio = true;
                        canRepeat = true;
                    }
                    // If the swipe was right, the user is not using Talkback.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Swiped right. Player is not using Talkback.";
                        isUsingTalkback = false; // The player has told us they are not using Talkback.
                        determined_talkback = true; // Determined if the player is using Talkback or not.
                        repeatSetupClip = false;
                        reset_audio = true;
                        canRepeat = true;
                    }
                    // If the swipe was up, that was not the gesture asked for.
                    else if (ie.isUp == true)
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the swipe was down, that was not the gesture asked for.
                    else if (ie.isDown == true)
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.    
                }
                // If a tap is registered.
                else if (ie.isTap == true)
                {
                    debugPlayerInfo = "Tap registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.  
                    SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else if (ie.isHold == true)
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.     
                    SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else if (ie.isRotate == true)
                {
                    debugPlayerInfo = "Rotation registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.   
                    SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If an unrecognized gesture is made.
                else if (ie.isUnrecognized == true)
                {
                    madeUnrecognizedGesture = true;

                    // If this error was registered.
                    if (ie.isSwipeLeftHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe left.";
                        SoundManager.instance.PlayVoice(Database.errorClips[3], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeRightHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe right.";
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeLeftRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe left.";
                        SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeRightRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe right.";
                        SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            // If the player's game environment is set up properly.
            else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == false))
            {
                // If a tap was registered.
                if (ie.isTap == true)
                {
                    debugPlayerInfo = "Tap registered. Game environment set up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                    
                    environment_setup = true; // Player environment is now set up.
                    repeatSetupClip = false;
                    reset_audio = true;
                    canRepeat = true;
                }
                // If a hold is registered.
                else if (ie.isHold == true)
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.  
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If a swipe is registered.
                else if (ie.isSwipe == true)
                {
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Swiped left. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Swiped right. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    else if (ie.isUp == true)
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    else if (ie.isDown == true)
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should tap to register that you are ready to continue.";

                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If a rotation is registered.
                else if (ie.isRotate == true)
                {
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.       
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If an unrecognized gesture is made.
                else if (ie.isUnrecognized == true)
                {
                    madeUnrecognizedGesture = true;

                    // If this error was registered.
                    if (ie.isTapHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isTapVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isTapRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap to register that you are ready to continue.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            // If the player has not put in headphones.
            else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == false))
            {
                // If a tap was registered.
                if (ie.isTap == true)
                {
                    debugPlayerInfo = "Tap registered. Earphones in.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    plugin_earphone = true; // The player has put in headphones.
                    repeatSetupClip = false;
                    reset_audio = true;
                    canRepeat = true;
                }
                // If a hold is registered.
                else if (ie.isHold == true)
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.      
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If a swipe is registered.
                else if (ie.isSwipe == true)
                {
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Swiped left. Incorrect gesture made. You should tap to register that you have put in headphones.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Swiped right. Incorrect gesture made. You should tap to register that you have put in headphones.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isUp == true)
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should tap to register that you have put in headphones.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isDown == true)
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should tap to register that you have put in headphones.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.   
                }
                // If a rotation is registered.
                else if (ie.isRotate == true)
                {
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    }
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If an unrecognized gesture is made.
                else if (ie.isUnrecognized == true)
                {
                    madeUnrecognizedGesture = true;

                    // If this error was registered.
                    if (ie.isTapHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isTapVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isTapRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap to register that you have put in headphones.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
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
                // If a tap was registered.
                else if (ie.isTap == true)
                {
                    debugPlayerInfo = "Tap registered. This gesture does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                // If a hold was registered.
                else if (ie.isHold == true)
                {
                    debugPlayerInfo = "Hold registered. This gesture does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                // If a rotation was registered.
                else if (ie.isRotate == true)
                {
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. This gesture does nothing in this menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. This gesture does nothing in this menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
                // If there was an unrecognized gesture made.
                else if (ie.isUnrecognized == true)
                {
                    madeUnrecognizedGesture = true;

                    // If this error was registered.
                    if (ie.isTapHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isTapVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isTapRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeLeftHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe left.";
                        SoundManager.instance.PlayVoice(Database.errorClips[3], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeRightHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe right.";
                        SoundManager.instance.PlayVoice(Database.errorClips[4], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeUpVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe up.";
                        SoundManager.instance.PlayVoice(Database.errorClips[5], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeDownVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe down.";
                        SoundManager.instance.PlayVoice(Database.errorClips[6], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeLeftRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe left.";
                        SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeRightRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe right.";
                        SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeUpRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe up.";
                        SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isSwipeDownRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe down.";
                        SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isRotationAngleError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with angle on rotation.";
                        SoundManager.instance.PlayVoice(Database.errorClips[8], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isHoldHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[9], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isHoldVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[10], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If this error was registered.
                    else if (ie.isHoldRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[11], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
        }
#endif //End of mobile platform dependendent compilation section started above with #elif
        if ((determined_talkback == false) || (plugin_earphone == false) || (environment_setup == false))
        {
            return;
        }
        switch (inputDirection)
        {
            // If the player swiped right, move to the pregame menu to continue where you left off.
            case Direction.RIGHT:
                if (inOptionsMenu == true)
                {
                    switch_click_toggle = false;
                    debugPlayerInfo = "Swiped left. Switched to using Odeon echoes.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    clips = new List<AudioClip>() { Database.mainMenuClips[13] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
                else if (inOptionsMenu == false)
                {
                    debugPlayerInfo = "Swiped right. Moving to pregame menu to continue where you left off.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
                    SceneManager.LoadScene("Main_pre"); // Move to pregame menu.				
                    GM_main_pre.hasGoneThroughSetup = true; // Since the player has gotten to this point and has chosen to continue a game, they must have gone through the environment setup.
                }
                break;
            // If the player swiped left, move to the pregame menu to start the tutorial.
            case Direction.LEFT:
                if (inOptionsMenu == true)
                {
                    switch_click_toggle = true;
                    debugPlayerInfo = "Swiped left. Switched to using HRTF echoes.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    clips = new List<AudioClip>() { Database.mainMenuClips[12] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
                else if (inOptionsMenu == false)
                {
                    debugPlayerInfo = "Swiped left. Moving to pregame menu to start tutorial.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                    SceneManager.LoadScene("Main_pre"); // Move to pregame menu.				
                    GM_main_pre.hasGoneThroughSetup = true; // Since the player has gotten to this point and has chosen to start the tutorial, they must have gone through the environment setup.
                }
                break;
            // If the player swiped up, listen to the commands.
            case Direction.UP:
                if (inOptionsMenu == true)
                {
                    inOptionsMenu = false;
                    debugPlayerInfo = "Swiped up. Closing options menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    clips = new List<AudioClip>() { Database.mainMenuClips[11] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
                else if (inOptionsMenu == false)
                {
                    debugPlayerInfo = "Swiped up. Listening to commands.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    if (!listenToCmd)
                    {
                        listenToCmd = true;
                        reset_audio = false;
                        canRepeat = true;
                    }
                }
                break;
            // If the player swiped down, open/close the options menu.
            case Direction.DOWN:
                if (inOptionsMenu == true)
                {
                    debugPlayerInfo = "Swiped down. Moving to next option (currently only one option, so this doesn't do anything).";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                else if (inOptionsMenu == false)
                {
                    inOptionsMenu = true;
                    debugPlayerInfo = "Swiped down. Opening options menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    if (isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.mainMenuClips[10] };
                    }
                    else if (isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.mainMenuClips[9] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
                break;
            default:
                break;
        }
    }
}
