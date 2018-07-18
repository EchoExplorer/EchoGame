using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// A script to display the user agreement dialogues.
/// This is attached to the ``GameManager`` GameObject in the main_pre scene.
/// </summary>
public class GM_main_pre : MonoBehaviour
{
    bool at_confirm = false;

    enum SelectMode { NONE, CONTINUE, NEW, CONFIRM, BACK, SPECIFIC }

    eventHandler eh;

    public static int skippingTutorial = -1;

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    public static bool hasGoneThroughSetup = false;

    bool madeUnrecognizedGesture = false;

    List<AudioClip> clips;
    float[] balances;

    SelectMode selectMode;

    bool canRepeat = true;
    bool repeatPregameClip = false;
    static bool firstConfirm = true;

    bool repeatInterruptedClips = true;

    int levelToStart;
    public static int tempLevelToStart;
    static int tempTempLevel;
    public static bool level1TutorialFinished = false;
    public static bool level3TutorialFinished = false;

    bool pickedSpecificLevel = false;

    GameObject levelImage;
    Text levelText;

    // Use this for initialization
    void Start()
    {       
        string filename = "";
        string[] svdata_split;
        GameMode.Game_Mode current = GameMode.instance.get_mode();

        // choose save for tutorial and normal game
        if ((current == GameMode.Game_Mode.RESTART) || (current == GameMode.Game_Mode.CONTINUE))
        {
            // filename = Application.persistentDataPath + "echosaved";
            filename = System.IO.Path.Combine(Application.persistentDataPath, "echosaved");
        }
        // load specific save for tutorial
        else if ((current == GameMode.Game_Mode.TUTORIAL_RESTART) || (current == GameMode.Game_Mode.TUTORIAL))
        {
            // filename = Application.persistentDataPath + "echosaved_tutorial";
            filename = System.IO.Path.Combine(Application.persistentDataPath, "echosaved_tutorial");
        }

        if (System.IO.File.Exists(filename))
        {
            svdata_split = System.IO.File.ReadAllLines(filename);
            //read existing data
            levelToStart = Int32.Parse(svdata_split[0]);
            tempLevelToStart = Int32.Parse(svdata_split[0]);
            tempTempLevel = Int32.Parse(svdata_split[0]);
            level1TutorialFinished = BoardManager.StringToBool(svdata_split[1]);
            level3TutorialFinished = BoardManager.StringToBool(svdata_split[2]);

            if ((current == GameMode.Game_Mode.RESTART) || (current == GameMode.Game_Mode.CONTINUE))
            {
                if ((levelToStart < 12) || (tempLevelToStart < 12) || (tempTempLevel < 12))
                {
                    levelToStart = 12;
                    tempLevelToStart = 12;
                    tempTempLevel = 12;
                }
            }
            else if ((current == GameMode.Game_Mode.TUTORIAL_RESTART) || (current == GameMode.Game_Mode.TUTORIAL))
            {
                if ((levelToStart > 11) || (tempLevelToStart > 11) || (tempTempLevel > 11))
                {
                    levelToStart = 11;
                    tempLevelToStart = 11;
                    tempTempLevel = 11;
                }
            }
        }
        else
        {
            if ((current == GameMode.Game_Mode.RESTART) || (current == GameMode.Game_Mode.CONTINUE))
            {
                levelToStart = 12;
                tempLevelToStart = 12;
                tempTempLevel = 12;
            }
            else if ((current == GameMode.Game_Mode.TUTORIAL_RESTART) || (current == GameMode.Game_Mode.TUTORIAL))
            {
                levelToStart = 1;
                tempLevelToStart = 1;
                tempTempLevel = 1;
            }
        }

        selectMode = SelectMode.NONE;

        init();
    }

    void init()
    {
        skippingTutorial = -1;
        firstConfirm = false;

        levelImage = GameObject.Find("LevelImage").gameObject;
        levelText = levelImage.transform.Find("LevelText").gameObject.GetComponent<Text>();
        levelText.fontSize = 18;
        if ((GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL) || (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL_RESTART))
        {
            if (GM_title.isUsingTalkback == true)
            {
                levelText.text = "Tap and hold for two seconds with \n";
                levelText.text += "three fingers, then swipe left with \n";
                levelText.text += "them to start a new game and go \n";
                levelText.text += "through a gesture tutorial. \n \n";
                levelText.text += "Swipe right with them to continue from \n";
                levelText.text += "where you left off in the tutorial. \n \n";
                levelText.text += "Swipe down with them to \n";
                levelText.text += "go to the main menu.";
            }
            else if (GM_title.isUsingTalkback == false)
            {
                levelText.text = "Swipe left with two/three fingers to \n";
                levelText.text += "start a new game and go through \n";
                levelText.text += "the gesture tutorial. \n \n";
                levelText.text += "Swipe right with those fingers to continue \n";
                levelText.text += "from where you left off in the tutorial. \n \n";
                levelText.text += "Swipe down with those fingers to \n";
                levelText.text += "go to the main menu.";
            }
        }
        else if ((GameMode.instance.gamemode == GameMode.Game_Mode.CONTINUE) || (GameMode.instance.gamemode == GameMode.Game_Mode.RESTART))
        {
            if (GM_title.isUsingTalkback == true)
            {
                levelText.text = "Tap and hold for two seconds with \n";
                levelText.text += "three fingers, then swipe left with \n";
                levelText.text += "them to start a new game \n";
                levelText.text += "after the tutorial. \n \n";
                levelText.text += "Swipe right with them to continue from \n";
                levelText.text += "where you left off after the tutorial. \n \n";
                levelText.text += "Swipe down with them to \n";
                levelText.text += "go to the main menu.";
            }
            else if (GM_title.isUsingTalkback == false)
            {
                levelText.text = "Swipe left with two/three fingers to \n";
                levelText.text += "start a new game after the tutorial. \n \n";
                levelText.text += "Swipe right with those fingers to continue \n";
                levelText.text += "from where you left off after the tutorial. \n \n";
                levelText.text += "Swipe down with those fingers to \n";
                levelText.text += "go to the main menu.";
            }
        }

        selectMode = SelectMode.NONE;
        at_confirm = false;
        hasGoneThroughSetup = true;
        eh = new eventHandler(InputModule.instance);
    }

    /// <summary>
    /// Plays instruction clips to select game modes.
    /// </summary>
	void play_audio()
    {
        if (madeUnrecognizedGesture == false)
        {
            if (at_confirm == false)
            {
                if (selectMode != SelectMode.SPECIFIC)
                {
                    if (selectMode != SelectMode.CONTINUE)
                    {
                        if ((GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL) || (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL_RESTART))
                        {
                            if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                            {
                                if (skippingTutorial == 1)
                                {
                                    print("Found error with freeze.");
                                    if (repeatPregameClip == true)
                                    {
                                        print("Fixed error with freeze.");
                                        repeatPregameClip = false;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[18] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.  
                                    }
                                }

                                // If the player is using Talkback.
                                else if ((skippingTutorial != 1) && (GM_title.isUsingTalkback == true))
                                {
                                    canRepeat = false;
                                    if (repeatPregameClip == true)
                                    {
                                        repeatInterruptedClips = true;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[1], Database.preGameMenuClips[5], Database.levelStartClips[levelToStart], Database.preGameMenuClips[9], Database.preGameMenuClips[13] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                    }
                                    else if (repeatPregameClip == false)
                                    {
                                        repeatInterruptedClips = true;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[1], Database.preGameMenuClips[5], Database.levelStartClips[levelToStart], Database.preGameMenuClips[9], Database.preGameMenuClips[13] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                        repeatPregameClip = true;
                                    }
                                }
                                // If the player is not using Talkback.
                                else if ((skippingTutorial != 1) && (GM_title.isUsingTalkback == false))
                                {
                                    canRepeat = false;
                                    if (repeatPregameClip == true)
                                    {
                                        repeatInterruptedClips = true;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[0], Database.preGameMenuClips[4], Database.levelStartClips[levelToStart], Database.preGameMenuClips[8], Database.preGameMenuClips[12] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                    }
                                    else if (repeatPregameClip == false)
                                    {
                                        repeatInterruptedClips = true;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[0], Database.preGameMenuClips[4], Database.levelStartClips[levelToStart], Database.preGameMenuClips[8], Database.preGameMenuClips[12] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                        repeatPregameClip = true;
                                    }
                                }
                            }
                        }
                        else if ((GameMode.instance.gamemode == GameMode.Game_Mode.CONTINUE) || (GameMode.instance.gamemode == GameMode.Game_Mode.RESTART))
                        {
                            if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                            {
                                if (skippingTutorial == 1)
                                {
                                    if (repeatPregameClip == true)
                                    {
                                        repeatPregameClip = false;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[18] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.  
                                    }
                                }

                                // If the player is using Talkback.
                                else if ((skippingTutorial != 1) && (GM_title.isUsingTalkback == true))
                                {
                                    canRepeat = false;
                                    if (repeatPregameClip == true)
                                    {
                                        repeatInterruptedClips = true;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[3], Database.preGameMenuClips[7], Database.levelStartClips[levelToStart], Database.preGameMenuClips[11], Database.preGameMenuClips[13] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                    }
                                    else if (repeatPregameClip == false)
                                    {
                                        repeatInterruptedClips = true;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[3], Database.preGameMenuClips[7], Database.levelStartClips[levelToStart], Database.preGameMenuClips[11], Database.preGameMenuClips[13] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                        repeatPregameClip = true;
                                    }
                                }
                                // If the player is not using Talkback.
                                else if ((skippingTutorial != 1) && (GM_title.isUsingTalkback == false))
                                {
                                    canRepeat = false;
                                    if (repeatPregameClip == true)
                                    {
                                        repeatInterruptedClips = true;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[2], Database.preGameMenuClips[6], Database.levelStartClips[levelToStart], Database.preGameMenuClips[10], Database.preGameMenuClips[12] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                    }
                                    else if (repeatPregameClip == false)
                                    {
                                        repeatInterruptedClips = true;
                                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[2], Database.preGameMenuClips[6], Database.levelStartClips[levelToStart], Database.preGameMenuClips[10], Database.preGameMenuClips[12] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                        repeatPregameClip = true;
                                    }
                                }
                            }
                        }
                    }
                    if (selectMode == SelectMode.CONTINUE)
                    {
                        if (repeatPregameClip == true)
                        {
                            repeatPregameClip = false;                            
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[18] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.  
                        }
                    }
                }
                else if (selectMode == SelectMode.SPECIFIC)
                {
                    if ((GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL) || (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL_RESTART))
                    {
                        if (canRepeat == true)
                        {
                            // If the player is using Talkback.
                            if (GM_title.isUsingTalkback == true)
                            {
                                canRepeat = false;
                                if (repeatPregameClip == true)
                                {
                                    repeatInterruptedClips = true;
                                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[19], Database.levelStartClips[levelToStart], Database.preGameMenuClips[21], Database.preGameMenuClips[23], Database.preGameMenuClips[25], Database.preGameMenuClips[27] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                }
                                else if (repeatPregameClip == false)
                                {
                                    repeatInterruptedClips = true;
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[19], Database.levelStartClips[levelToStart], Database.preGameMenuClips[21], Database.preGameMenuClips[23], Database.preGameMenuClips[25], Database.preGameMenuClips[27] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                    repeatPregameClip = true;
                                }
                            }
                            // If the player is not using Talkback.
                            else if (GM_title.isUsingTalkback == false)
                            {
                                canRepeat = false;
                                if (repeatPregameClip == true)
                                {
                                    repeatInterruptedClips = true;
                                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[19], Database.levelStartClips[levelToStart], Database.preGameMenuClips[20], Database.preGameMenuClips[22], Database.preGameMenuClips[24], Database.preGameMenuClips[26] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                }
                                else if (repeatPregameClip == false)
                                {
                                    repeatInterruptedClips = true;
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[19], Database.levelStartClips[levelToStart], Database.preGameMenuClips[20], Database.preGameMenuClips[22], Database.preGameMenuClips[24], Database.preGameMenuClips[26] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                    repeatPregameClip = true;
                                }
                            }
                        }
                    }
                    else if ((GameMode.instance.gamemode == GameMode.Game_Mode.CONTINUE) || (GameMode.instance.gamemode == GameMode.Game_Mode.RESTART))
                    {
                        if (canRepeat == true)
                        {
                            // If the player is using Talkback.
                            if (GM_title.isUsingTalkback == true)
                            {
                                canRepeat = false;
                                if (repeatPregameClip == true)
                                {
                                    repeatInterruptedClips = true;
                                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[19], Database.levelStartClips[levelToStart], Database.preGameMenuClips[21], Database.preGameMenuClips[23], Database.preGameMenuClips[25], Database.preGameMenuClips[29] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                }
                                else if (repeatPregameClip == false)
                                {
                                    repeatInterruptedClips = true;
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[19], Database.levelStartClips[levelToStart], Database.preGameMenuClips[21], Database.preGameMenuClips[23], Database.preGameMenuClips[25], Database.preGameMenuClips[29] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                    repeatPregameClip = true;
                                }
                            }
                            // If the player is not using Talkback.
                            else if (GM_title.isUsingTalkback == false)
                            {
                                canRepeat = false;
                                if (repeatPregameClip == true)
                                {
                                    repeatInterruptedClips = true;
                                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[19], Database.levelStartClips[levelToStart], Database.preGameMenuClips[20], Database.preGameMenuClips[22], Database.preGameMenuClips[24], Database.preGameMenuClips[28] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                }
                                else if (repeatPregameClip == false)
                                {
                                    repeatInterruptedClips = true;
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[19], Database.levelStartClips[levelToStart], Database.preGameMenuClips[20], Database.preGameMenuClips[22], Database.preGameMenuClips[24], Database.preGameMenuClips[28] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                                    repeatPregameClip = true;
                                }
                            }
                        }
                    }
                }
            }
            else if (at_confirm == true)
            {
                if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (skippingTutorial == -1))
                {
                    // If the player is using Talkback.
                    if (GM_title.isUsingTalkback == true)
                    {
                        if (firstConfirm == false)
                        {
                            canRepeat = false;
                            repeatInterruptedClips = false;
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[14], Database.preGameMenuClips[16] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }
                        if (firstConfirm == true)
                        {
                            firstConfirm = false;
                            canRepeat = false;
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[14], Database.preGameMenuClips[16] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }                        
                    }
                    // If the player is not using Talkback.
                    else if (GM_title.isUsingTalkback == false)
                    {
                        if (firstConfirm == false)
                        {
                            canRepeat = false;
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.preGameMenuClips[14], Database.preGameMenuClips[15] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }
                        if (firstConfirm == true)
                        {
                            firstConfirm = false;
                            canRepeat = false;
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[14], Database.preGameMenuClips[15] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }                       
                    }
                }
            }
        }

        else if ((madeUnrecognizedGesture == true) && (SoundManager.instance.finishedClip == true) && (repeatInterruptedClips == true))
        {
            madeUnrecognizedGesture = false;

            if (SoundManager.clipsCurrentlyPlaying.Count >= 1)
            {
                int i = 0;
                print("Interrupted clips:");
                foreach (AudioClip clip in SoundManager.clipsCurrentlyPlaying)
                {
                    print("Clip " + i + ": " + SoundManager.clipsCurrentlyPlaying[i]);
                    i++;
                }

                List<AudioClip> currentClips = SoundManager.clipsCurrentlyPlaying;
                SoundManager.instance.PlayClips(currentClips, SoundManager.currentBalances, 0, SoundManager.currentCallback, SoundManager.currentCallbackIndex, SoundManager.currentVolumes, true);
                SoundManager.clipsCurrentlyPlaying.Clear();
            }
        }
    }

    /// <summary>
    /// Checks user input with raw touch data and transitions to the next scene according to the input.
    /// </summary>
    void Update()
    {
        play_audio();

        // Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        // Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

            // Do something based on this event info.
            // If a tap was registered.
            if ((ie.isTap == true) && (skippingTutorial == -1))
            {
                // We have swiped left to start a new game and confirmed that this is the action we want, so set mode to Confirm.
                if (at_confirm == true)
                {
                    canRepeat = true;
                    repeatInterruptedClips = false;
                    selectMode = SelectMode.CONFIRM; // We have tapped to confirm we want to start a new game, so set mode to Confirm.
                }

                else if (selectMode == SelectMode.SPECIFIC)
                {
                    selectMode = SelectMode.NONE;
                    madeUnrecognizedGesture = false;
                    at_confirm = false;
                    canRepeat = true;
                    repeatPregameClip = false;
                    repeatInterruptedClips = false;
                    tempLevelToStart = tempTempLevel;
                }
            }
            // If a swipe was registered.
            else if ((ie.isSwipe == true) && (skippingTutorial == -1))
            {
                // If the swipe was left.
                if (ie.isLeft == true)
                {
                    if (selectMode != SelectMode.SPECIFIC)
                    {
                        canRepeat = true;
                        repeatInterruptedClips = false;
                        selectMode = SelectMode.NEW; // If we have swiped left, set mode to New.
                    }
                }
                // If the swipe was right.
                if (ie.isRight == true)
                {
                    if (selectMode != SelectMode.SPECIFIC)
                    {
                        firstConfirm = false;
                        repeatInterruptedClips = false;
                        selectMode = SelectMode.CONTINUE; // If we have swiped right, set mode to Continue.
                    }
                }
                // If the swipe was up.
                if (ie.isUp == true)
                {
                    if (selectMode != SelectMode.SPECIFIC)
                    {
                        selectMode = SelectMode.SPECIFIC; // If we have swiped up, set mode to Specific.
                        canRepeat = true;
                        repeatInterruptedClips = false;
                        repeatPregameClip = false;
                    }
                    else if (selectMode == SelectMode.SPECIFIC)
                    {
                        if ((GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL) || (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL_RESTART))
                        {
                            if ((tempLevelToStart >= 1) && (tempLevelToStart < 11))
                            {
                                repeatInterruptedClips = false;
                                tempLevelToStart += 1;
                                debugPlayerInfo = "Increased level to start from. Would start at level " + tempLevelToStart.ToString();
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.levelStartClips[tempLevelToStart] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (tempLevelToStart == 11)
                            {
                                repeatInterruptedClips = false;
                                debugPlayerInfo = "At maximum level for tutorial levels. Cannot pick a higher level to play.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[32] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((GameMode.instance.gamemode == GameMode.Game_Mode.CONTINUE) || (GameMode.instance.gamemode == GameMode.Game_Mode.RESTART))
                        {
                            if ((tempLevelToStart >= 12) && (tempLevelToStart < 150))
                            {
                                repeatInterruptedClips = false;
                                tempLevelToStart += 1;
                                debugPlayerInfo = "Increased level to start from. Would start at level " + tempLevelToStart.ToString();
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.levelStartClips[tempLevelToStart] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (tempLevelToStart == 150)
                            {
                                repeatInterruptedClips = false;
                                debugPlayerInfo = "At maximum level for post-tutorial levels. Cannot pick a higher level to play.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[34] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                    }
                }
                // If the swipe was down.
                if (ie.isDown == true)
                {
                    if (selectMode != SelectMode.SPECIFIC)
                    {
                        repeatInterruptedClips = false;
                        canRepeat = true;
                        selectMode = SelectMode.BACK; // If we have swiped down, set mode to Back.  
                    }
                    else if (selectMode == SelectMode.SPECIFIC)
                    {
                        if ((GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL) || (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL_RESTART))
                        {
                            if ((tempLevelToStart > 1) && (tempLevelToStart < 12))
                            {
                                repeatInterruptedClips = false;
                                tempLevelToStart -= 1;
                                debugPlayerInfo = "Decreased level to start from. Would start at level " + tempLevelToStart.ToString();
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.levelStartClips[tempLevelToStart] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (tempLevelToStart == 1)
                            {
                                repeatInterruptedClips = false;
                                debugPlayerInfo = "At minimum level for tutorial levels. Cannot pick a lower level to play.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[31] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((GameMode.instance.gamemode == GameMode.Game_Mode.CONTINUE) || (GameMode.instance.gamemode == GameMode.Game_Mode.RESTART))
                        {
                            if ((tempLevelToStart > 12) && (tempLevelToStart < 151))
                            {
                                repeatInterruptedClips = false;
                                tempLevelToStart -= 1;
                                debugPlayerInfo = "Decreased level to start from. Would start at level " + tempLevelToStart.ToString();
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.levelStartClips[tempLevelToStart] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (tempLevelToStart == 12)
                            {
                                repeatInterruptedClips = false;
                                debugPlayerInfo = "At minimum level for post-tutorial levels. Cannot pick a lower level to play.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                    }
                }
            }
            // If a rotation was registered.
            else if ((ie.isRotate == true) && (skippingTutorial == -1))
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
            // If a hold was registered.
            else if ((ie.isHold == true) && (skippingTutorial == -1))
            {
                if (selectMode == SelectMode.SPECIFIC)
                {
                    repeatInterruptedClips = false;
                    pickedSpecificLevel = true;
                }
                else
                {
                    debugPlayerInfo = "Hold registered. This gesture does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }

            // If an unrecognized gesture was registered.
            else if ((ie.isUnrecognized == true) && (skippingTutorial == -1))
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
                else if (ie.isTapHorizontalVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on tap.";
                    SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isTapRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                    SoundManager.instance.PlayVoice(Database.errorClips[3], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe left.";
                    SoundManager.instance.PlayVoice(Database.errorClips[4], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe right.";
                    SoundManager.instance.PlayVoice(Database.errorClips[5], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe up.";
                    SoundManager.instance.PlayVoice(Database.errorClips[6], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe down.";
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeHorizontalVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on swipe.";
                    SoundManager.instance.PlayVoice(Database.errorClips[8], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else if (ie.isSwipeDirectionError == true)
                {
                    debugPlayerInfo = "Nothing happened because gesture was neither a horizontal or vertical swipe.";
                    SoundManager.instance.PlayVoice(Database.errorClips[9], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else if (ie.isBetweenTapSwipeError == true)
                {
                    debugPlayerInfo = "Nothing happened because gesture was in between tap and swipe.";
                    SoundManager.instance.PlayVoice(Database.errorClips[10], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else if (ie.isBetweenHoldSwipeError == true)
                {
                    debugPlayerInfo = "Nothing happened because gesture was in between hold and swipe.";
                    SoundManager.instance.PlayVoice(Database.errorClips[11], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe left.";
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe right.";
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe up.";
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe down.";
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isRotationAngleError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with angle on rotation.";
                    SoundManager.instance.PlayVoice(Database.errorClips[13], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[14], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[15], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldHorizontalVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[16], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
        }

#endif
        // Check if we are running on iOS/Android.
#if UNITY_IOS || UNITY_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

            // If a swipe was recognized.
            if ((ie.isSwipe == true) && (skippingTutorial == -1))
            {
                // If the swipe was left.
                if (ie.isLeft == true)
                {
                    if (selectMode != SelectMode.SPECIFIC)
                    {
                        canRepeat = true;
                        repeatInterruptedClips = false;
                        selectMode = SelectMode.NEW; // If we have swiped left, set mode to New.
                    }
                }
                // If the swipe was right.
                else if (ie.isRight == true)
                {
                    if (selectMode != SelectMode.SPECIFIC)
                    {
                        firstConfirm = false;
                        repeatInterruptedClips = false;
                        selectMode = SelectMode.CONTINUE; // If we have swiped right, set mode to Continue.
                    }
                }
                // If the swipe was up.
                else if (ie.isUp == true)
                {
                    if (selectMode != SelectMode.SPECIFIC)
                    {
                        selectMode = SelectMode.SPECIFIC; // If we have swiped up, set mode to Specific.
                        canRepeat = true;
                        repeatPregameClip = false;
                        repeatInterruptedClips = false;
                    }
                    else if (selectMode == SelectMode.SPECIFIC)
                    {
                        if ((GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL) || (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL_RESTART))
                        {
                            if ((tempLevelToStart >= 1) && (tempLevelToStart < 11))
                            {
                                repeatInterruptedClips = false;
                                tempLevelToStart += 1;
                                debugPlayerInfo = "Increased level to start from. Would start at level " + tempLevelToStart.ToString();
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.levelStartClips[tempLevelToStart] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (tempLevelToStart == 11)
                            {
                                repeatInterruptedClips = false;
                                debugPlayerInfo = "At maximum level for tutorial levels. Cannot pick a higher level to play.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[32] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((GameMode.instance.gamemode == GameMode.Game_Mode.CONTINUE) || (GameMode.instance.gamemode == GameMode.Game_Mode.RESTART))
                        {
                            if ((tempLevelToStart >= 12) && (tempLevelToStart < 150))
                            {
                                repeatInterruptedClips = false;
                                tempLevelToStart += 1;
                                debugPlayerInfo = "Increased level to start from. Would start at level " + tempLevelToStart.ToString();
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.levelStartClips[tempLevelToStart] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (tempLevelToStart == 150)
                            {
                                repeatInterruptedClips = false;
                                debugPlayerInfo = "At maximum level for post-tutorial levels. Cannot pick a higher level to play.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[34] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                    }
                }
                // If the swipe was down.
                else if (ie.isDown == true)
                {
                    if (selectMode != SelectMode.SPECIFIC)
                    {
                        canRepeat = true;
                        repeatInterruptedClips = false;
                        selectMode = SelectMode.BACK; // If we have swiped down, set mode to Back.  
                    }
                    else if (selectMode == SelectMode.SPECIFIC)
                    {
                        if ((GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL) || (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL_RESTART))
                        {
                            if ((tempLevelToStart > 1) && (tempLevelToStart < 12))
                            {
                                repeatInterruptedClips = false;
                                tempLevelToStart -= 1;
                                debugPlayerInfo = "Decreased level to start from. Would start at level " + tempLevelToStart.ToString();
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.levelStartClips[tempLevelToStart] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (tempLevelToStart == 1)
                            {
                                repeatInterruptedClips = false;
                                debugPlayerInfo = "At minimum level for tutorial levels. Cannot pick a lower level to play.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[31] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((GameMode.instance.gamemode == GameMode.Game_Mode.CONTINUE) || (GameMode.instance.gamemode == GameMode.Game_Mode.RESTART))
                        {
                            if ((tempLevelToStart > 12) && (tempLevelToStart < 151))
                            {
                                repeatInterruptedClips = false;
                                tempLevelToStart -= 1;
                                debugPlayerInfo = "Decreased level to start from. Would start at level " + tempLevelToStart.ToString();
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.levelStartClips[tempLevelToStart] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (tempLevelToStart == 12)
                            {
                                repeatInterruptedClips = false;
                                debugPlayerInfo = "At minimum level for post-tutorial levels. Cannot pick a lower level to play.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                    }
                }
            }

            // If a tap was registered and we are able to start a new game, set mode to Confirm.
            else if ((ie.isTap == true) && (skippingTutorial == -1))
            {
                // We have swiped left to start a new game and confirmed that this is the action we want, so set mode to Confirm.
                if (at_confirm == true)
                {
                    canRepeat = true;
                    repeatInterruptedClips = false;
                    selectMode = SelectMode.CONFIRM; // We have tapped to confirm we want to start a new game, so set mode to Confirm.
                }

                else if (selectMode == SelectMode.SPECIFIC)
                {
                    selectMode = SelectMode.NONE;
                    madeUnrecognizedGesture = false;
                    at_confirm = false;
                    canRepeat = true;
                    repeatPregameClip = false;
                    repeatInterruptedClips = false;
                    tempLevelToStart = tempTempLevel;
                }
            }

            // If a rotation was registered.
            else if ((ie.isRotate) == true && (skippingTutorial == -1))
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

            // If a hold was registered.
            else if ((ie.isHold == true) && (skippingTutorial == -1))
            {
                if (selectMode == SelectMode.SPECIFIC)
                {
                    repeatInterruptedClips = false;
                    pickedSpecificLevel = true;
                }
                else
                {
                    debugPlayerInfo = "Hold registered. This gesture does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }

            // If there was an unrecognized gesture made.
            else if ((ie.isUnrecognized == true) && (skippingTutorial == -1))
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
                else if (ie.isTapHorizontalVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on tap.";
                    SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isTapRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                    SoundManager.instance.PlayVoice(Database.errorClips[3], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe left.";
                    SoundManager.instance.PlayVoice(Database.errorClips[4], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe right.";
                    SoundManager.instance.PlayVoice(Database.errorClips[5], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe up.";
                    SoundManager.instance.PlayVoice(Database.errorClips[6], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe down.";
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeHorizontalVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on swipe.";
                    SoundManager.instance.PlayVoice(Database.errorClips[8], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else if (ie.isSwipeDirectionError == true)
                {
                    debugPlayerInfo = "Nothing happened because gesture was neither a horizontal or vertical swipe.";
                    SoundManager.instance.PlayVoice(Database.errorClips[9], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else if (ie.isBetweenTapSwipeError == true)
                {
                    debugPlayerInfo = "Nothing happened because gesture was in between tap and swipe.";
                    SoundManager.instance.PlayVoice(Database.errorClips[10], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                else if (ie.isBetweenHoldSwipeError == true)
                {
                    debugPlayerInfo = "Nothing happened because gesture was in between hold and swipe.";
                    SoundManager.instance.PlayVoice(Database.errorClips[11], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe left.";
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe right.";
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe up.";
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe down.";
                    SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isRotationAngleError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with angle on rotation.";
                    SoundManager.instance.PlayVoice(Database.errorClips[13], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[14], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[15], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldHorizontalVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[16], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                    SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
        }

#endif // End of mobile platform dependendent compilation section started above with #elif

        switch (selectMode)
        {
            // If mode is set to Continue, we have swiped right, so continue from where we left off.
            case SelectMode.CONTINUE:                
                debugPlayerInfo = "Swiped right. Continuing from where you left off.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                if ((firstConfirm == true) && (skippingTutorial == 1) && (repeatPregameClip == false) && (canRepeat == false) && (SoundManager.instance.finishedAllClips == true))
                {
                    selectMode = SelectMode.NONE;
                    SceneManager.LoadScene("Main");
                }
                if (firstConfirm == false)
                {
                    firstConfirm = true;
                    canRepeat = false;
                    repeatPregameClip = true;
                    skippingTutorial = 1;
                }               
                break;
            // If mode is set to New, we have confirmed and swiped left, so start a new game from either the tutorial or the first non-tutorial level.
            case SelectMode.NEW:                
                debugPlayerInfo = "Swiped left. Going to confirm we want to start a new game.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                
                at_confirm = true;
                canRepeat = true;
                firstConfirm = true;
                repeatPregameClip = false;
                selectMode = SelectMode.NONE;
                break;
            // If mode is set to Confirm, we have tapped to confirm we want to start a new game, so let the player swipe left to start.
            case SelectMode.CONFIRM:
                debugPlayerInfo = "Tap registered. Confirmed we want to start a new game.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                               
                if (GameMode.instance.gamemode == GameMode.Game_Mode.TUTORIAL)
                {
                    GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL_RESTART;
                }
                else if (GameMode.instance.gamemode == GameMode.Game_Mode.CONTINUE)
                {
                    GameMode.instance.gamemode = GameMode.Game_Mode.RESTART;
                }
                // Utilities.write_save(0); ???

                if ((firstConfirm == true) && (skippingTutorial == 0) && (SoundManager.instance.finishedAllClips == true))
                {
                    selectMode = SelectMode.NONE;
                    SceneManager.LoadScene("Main");
                }
                if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (skippingTutorial == -1) && (selectMode == SelectMode.CONFIRM))
                {
                    canRepeat = false;
                    firstConfirm = true;
                    skippingTutorial = 0;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[17] };
                    balances = new float[] { 0, 0, 0 };
                    SoundManager.instance.PlayClips(clips, balances, 0, null, 0, null); // Play the appropriate clips.
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
                    selectMode = SelectMode.NONE;
                    SceneManager.LoadScene("Title_Screen"); // Move back to the main menu.                   
                }
                break;
            // If the mode is set to Specific, load a specific that the user selects.
            case SelectMode.SPECIFIC:
                if ((pickedSpecificLevel == true) && (skippingTutorial == -1))
                {                                   
                    skippingTutorial = 2;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.preGameMenuClips[30], Database.soundEffectClips[0] };
                    balances = new float[] { 0, 0, 0, 0 };
                    SoundManager.instance.PlayClips(clips, balances, 0, null, 0, null); // Play the appropriate clips.
                }
                if ((pickedSpecificLevel == true) && (skippingTutorial == 2) && (SoundManager.instance.finishedAllClips == true))
                {
                    selectMode = SelectMode.NONE;
                    SceneManager.LoadScene("Main");
                }
                break;
            default:
                break;
        }
    }
}
