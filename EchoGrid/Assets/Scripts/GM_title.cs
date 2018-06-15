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
    bool listenToCmd = false;
    public bool toMainflag = false;

    Text titleText;
    eventHandler eh;

    bool repeatSetupClip = false;

    bool repeatInterruptedClips = true;

    public static bool isUsingTalkback = true; // Tells us if the player has told us that they are using Talkback or not.

    bool inOptionsMenu = false;
    bool onEchoSetting = true;
    bool onConsentSetting = false;
    public static bool usingHRTFEchoes = true; // If this is true, use the HRTF echoes, which is option 1 in database.
    public static bool usingOdeonEchoes = false; // If this is true, use the Odeon echoes, which is option 2 in database.

    AndroidDialogue ad;
    bool yesPressed = false;
    bool noPressed = false;

    public void switchYes(string yes)
    {
        yesPressed = true;
    }
    bool android_window_displayed = false;
    bool can_display_window = false;
    bool finished_reading = false;

    bool hearingConsentForm = false;
    bool readingConsentForm = false;

    bool consentFlag = false;
    bool readConsent = false;
    bool proceduresFlag = false;
    bool readProcedures = false;
    bool requirementsFlag = false;
    bool readRequirements = false;
    bool risksFlag = false;
    bool readRisks = false;
    bool benefitsFlag = false;
    bool readBenefits = false;
    bool compCostFlag = false;
    bool readCompCost = false;
    bool confidentialityFlag = false;
    bool readConfidentiality = false;
    bool questionsContactFlag = false;
    bool readQuestionsContact = false;
    bool voluntaryFlag = false;
    bool readVoluntary = false;

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    enum Direction { NONE, UP, DOWN, LEFT, RIGHT }

    /// <summary>
    /// Sets up a reference to the GameMode module so it can set up its singleton.
    /// </summary>
    void Start()
    {
        titleText = GameObject.Find("ContactText").GetComponent<Text>();
        ad = GameObject.Find("GameManager").GetComponent<AndroidDialogue>();
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
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[0] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
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
                        repeatInterruptedClips = true;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.settingsClips[1], Database.settingsClips[2] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                    else if (repeatSetupClip == false)
                    {
                        repeatInterruptedClips = true;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[1], Database.settingsClips[2] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
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
                            repeatInterruptedClips = true;
                            balances = new float[] { 0, 0, 0, 1 };
                            SoundManager.instance.PlayClips(clips2, balances, 1, null, 0, null, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            repeatInterruptedClips = true;
                            balances = new float[] { 0, 0, 0, 0, 1 };
                            SoundManager.instance.PlayClips(clips, balances, 0, null, 0, null, true); // Play the appropriate clips.
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
                            repeatInterruptedClips = true;
                            balances = new float[] { 0, 0, 1 };
                            SoundManager.instance.PlayClips(clips2, balances, 1, null, 0, null, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            repeatInterruptedClips = true;
                            balances = new float[] { 0, 0, 0, 1 };
                            SoundManager.instance.PlayClips(clips, balances, 0, null, 0, null, true); // Play the appropriate clips.
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
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.settingsClips[9] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.   
                        }
                        else if (repeatSetupClip == false)
                        {
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[9] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.   
                            repeatSetupClip = true;
                        }
                    }
                    // If the player is not using Talkback.
                    else if (isUsingTalkback == false)
                    {
                        canRepeat = false;
                        if (repeatSetupClip == true)
                        {
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.settingsClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.  
                        }
                        else if (repeatSetupClip == false)
                        {
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.settingsClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.  
                            repeatSetupClip = true;
                        }
                    }
                    return;
                }
            }
            // If the player's game environment has been set up and they have not swiped up, tell them what they can do at this menu.
            if (((GM_main_pre.hasGoneThroughSetup == true) || (environment_setup == true)) && (listenToCmd == false) && (inOptionsMenu == false))
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
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[0], Database.mainMenuClips[2], Database.mainMenuClips[4], Database.mainMenuClips[6], Database.mainMenuClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[0], Database.mainMenuClips[2], Database.mainMenuClips[4], Database.mainMenuClips[6], Database.mainMenuClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                            repeatSetupClip = true;
                        }
                    }
                    // If the player is not using Talkback.
                    else if (isUsingTalkback == false)
                    {
                        canRepeat = false;
                        if (repeatSetupClip == true)
                        {
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[0], Database.mainMenuClips[1], Database.mainMenuClips[3], Database.mainMenuClips[5], Database.mainMenuClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }
                        else if (repeatSetupClip == false)
                        {
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[0], Database.mainMenuClips[1], Database.mainMenuClips[3], Database.mainMenuClips[5], Database.mainMenuClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                            repeatSetupClip = true;
                        }
                    }
                    return;
                }
            }
            // If the player's game environment is set up and they swipe up, give them a list of commands.
            if (((GM_main_pre.hasGoneThroughSetup == true) || (environment_setup == true)) && (listenToCmd == true) && (inOptionsMenu == false))
            {
                if (canRepeat == true)
                {
                    // If the player is using Talkback.
                    if (isUsingTalkback == true)
                    {
                        repeatInterruptedClips = true;
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[10], Database.mainMenuClips[12], Database.mainMenuClips[14], Database.mainMenuClips[16], Database.mainMenuClips[18] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                    // If the player is not using Talkback.
                    else if (isUsingTalkback == false)
                    {
                        repeatInterruptedClips = true;
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[9], Database.mainMenuClips[11], Database.mainMenuClips[13], Database.mainMenuClips[15], Database.mainMenuClips[17] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the list of command hints have finished playing, go back to the menu options.
                if (SoundManager.instance.finishedAllClips == true)
                {
                    listenToCmd = false;
                }
            }

            if (((GM_main_pre.hasGoneThroughSetup == true) || (environment_setup == true)) && (inOptionsMenu == true) && (android_window_displayed == false) && (hearingConsentForm == false) && (readingConsentForm == false))
            {
                if (SoundManager.instance.finishedAllClips == true)
                {
                    if (isUsingTalkback == true)
                    {
                        if (onEchoSetting == true)
                        {
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[21] };
                        }
                        else if (onConsentSetting == true)
                        {
                            repeatInterruptedClips = true;
                            if (finished_reading == true)
                            {
                                finished_reading = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[25] };

                            }
                            else if (finished_reading == false)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[25] };
                            }
                        }
                    }
                    else if (isUsingTalkback == false)
                    {
                        if (onEchoSetting == true)
                        {
                            repeatInterruptedClips = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[20] };
                        }
                        else if (onConsentSetting == true)
                        {
                            repeatInterruptedClips = true;
                            if (finished_reading == true)
                            {
                                finished_reading = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[24] };

                            }
                            else if (finished_reading == false)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[24] };
                            }
                        }
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
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
    /// Checks for an internet connection, and plays instructions.
    ///  Progresses to the main_pre scene for regular gameplay, or the main scene
    ///  for the tutorial by analyzing the touch data.
    /// </summary>
    void Update()
    {
        if (GM_main_pre.hasGoneThroughSetup == true)
        {
            determined_talkback = true;
            plugin_earphone = true;
            environment_setup = true;
            orientation_correct = true;
            repeatSetupClip = false;
        }

        play_audio();

        if ((readingConsentForm == true) && (android_window_displayed == false) && (can_display_window == true))
        {
            android_window_displayed = true;
            finished_reading = false;
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConsent == false) && (consentFlag == false))
        {
            consentFlag = true;

            string title = "Echolocation Consent";
            string message = "This game is part of a research study conducted by Laurie Heller and Pulkit Grover at Carnegie Mellon " +
                "University and is partially funded by Google. The purpose is to understand how people can use " +
                "sounds to figure out aspects of their physical environment. The game will use virtual sounds " +
                "and virtual walls to teach people how to use sound to virtually move around in the game.";

#if UNITY_IOS
            IOSNative.ShowOneG(title, message, "Next");
#endif
#if UNITY_ANDROID
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.YESONLY;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next");
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConsent == false) && (consentFlag == true) && (ad.yesclicked() == true || yesPressed == true))
        {
            readConsent = true;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            yesPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConsent == true) && (readProcedures == false) && (proceduresFlag == false))
        {
            proceduresFlag = true;

            string title = "Procedures";
            string message = "App users will install a free app on their phone named EchoAdventure. Launching the app for the " +
                "first time will direct users to a consent form. This consent process will only happen once. Users will " +
                "first go through a tutorial. Users will need to wear headphones in both ears. After a certain number of " +
                "levels have been played, an 18-question survey regarding the user experience and visual acuity will " +
                "appear. This survey will only happen once.";

#if UNITY_IOS
            IOSNative.ShowTwoG(title, message, "Next", "Back");
#endif
#if UNITY_ANDROID
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readProcedures == false) && (proceduresFlag == true) && (ad.yesclicked() == true || yesPressed == true))
        {
            readProcedures = true;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            yesPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readProcedures == false) && (proceduresFlag == true) && (ad.noclicked() == true || noPressed == true))
        {
            proceduresFlag = false;
            readConsent = false;
            consentFlag = false;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            noPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readProcedures == true) && (readRequirements == false) && (requirementsFlag == false))
        {
            requirementsFlag = true;

            string title = "Participant Requirements";
            string message = "You must be 18 or older and have normal hearing, because the game relies on detecting subtle differences " +
                "between sounds. You must have access to a smartphone.";

#if UNITY_IOS
            IOSNative.ShowTwoG(title, message, "Next", "Back");
#endif
#if UNITY_ANDROID
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRequirements == false) && (requirementsFlag == true) && (ad.yesclicked() == true || yesPressed == true))
        {
            readRequirements = true;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            yesPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRequirements == false) && (requirementsFlag == true) && (ad.noclicked() == true || noPressed == true))
        {
            requirementsFlag = false;
            readConsent = true;
            readProcedures = false;
            proceduresFlag = false;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            noPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRequirements == true) && (readRisks == false) && (risksFlag == false))
        {
            risksFlag = true;

            string title = "Risks";
            string message = "The risks associated with participation in this study are no greater than those ordinarily " +
                "encountered in daily life or other online activities.";

#if UNITY_IOS
            IOSNative.ShowTwoG(title, message, "Next", "Back");
#endif
#if UNITY_ANDROID
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRisks == false) && (risksFlag == true) && (ad.yesclicked() == true || yesPressed == true))
        {
            readRisks = true;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            yesPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRisks == false) && (risksFlag == true) && (ad.noclicked() == true || noPressed == true))
        {
            risksFlag = false;
            readProcedures = true;
            readRequirements = false;
            requirementsFlag = false;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            noPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRisks == true) && (readBenefits == false) && (benefitsFlag == false))
        {
            benefitsFlag = true;

            string title = "Benefits";
            string message = "There may be no personal benefit from your participation, but the knowledge received may be of value " +
                "to humanity.";

#if UNITY_IOS
            IOSNative.ShowTwoG(title, message, "Next", "Back");
#endif
#if UNITY_ANDROID
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readBenefits == false) && (benefitsFlag == true) && (ad.yesclicked() == true || yesPressed == true))
        {
            readBenefits = true;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            yesPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readBenefits == false) && (benefitsFlag == true) && (ad.noclicked() == true || noPressed == true))
        {
            benefitsFlag = false;
            readRequirements = true;
            readRisks = false;
            risksFlag = false;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            noPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readBenefits == true) && (readCompCost == false) && (compCostFlag == false))
        {
            compCostFlag = true;

            string title = "Compensation and Costs";
            string message = "There is no compensation or cost for participation in this study.";

#if UNITY_IOS
            IOSNative.ShowTwoG(title, message, "Next", "Back");
#endif
#if UNITY_ANDROID
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readCompCost == false) && (compCostFlag == true) && (ad.yesclicked() == true || yesPressed == true))
        {
            readCompCost = true;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            yesPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readCompCost == false) && (compCostFlag == true) && (ad.noclicked() == true || noPressed == true))
        {
            compCostFlag = false;
            readRisks = true;
            readBenefits = false;
            benefitsFlag = false;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            noPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readCompCost == true) && (readConfidentiality == false) && (confidentialityFlag == false))
        {
            confidentialityFlag = true;

            string title = "Confidentiality";
            string message = "Data captured for the research does not include any personally identifiable information about you. Your phone’s " +
                "device ID will be captured, which is customary for all apps that you install on a phone. You will indicate whether " +
                "or not you have a visual impairment, but that is not considered to be private. The moves you make while playing " +
                "the game will be captured and your app satisfaction survey responses will be captured.\n\n" +
                "By participating, you understand and agree that Carnegie Mellon may be required to disclose your consent form, " +
                "data and other personally identifiable information as required by law, regulation, subpoena or court order. " +
                "Otherwise, your confidentiality will be maintained in the following manner:\n\n" +
                "Your consent form will be stored electronically in a secure location and will not be disclosed to third parties. " +
                "Sharing of data with other researchers will only be done in such a manner that you will not be identified. " +
                "This research was sponsored by Google and the app survey data may be shared with them.\n\n" +
                "By participating, you understand that the data and information gathered during this study may be used by Carnegie " +
                "Mellon and published and/or disclosed by Carnegie Mellon to others outside of Carnegie Mellon. However, your name " +
                "and other direct personal identifiers will not be shared. Note that per regulation all research data must be kept " +
                "for a minimum of 3 years.";

#if UNITY_IOS
            IOSNative.ShowTwoG(title, message, "Next", "Back");
#endif
#if UNITY_ANDROID
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConfidentiality == false) && (confidentialityFlag == true) && (ad.yesclicked() == true || yesPressed == true))
        {
            readConfidentiality = true;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            yesPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConfidentiality == false) && (confidentialityFlag == true) && (ad.noclicked() == true || noPressed == true))
        {
            confidentialityFlag = false;
            readBenefits = true;
            readCompCost = false;
            compCostFlag = false;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            noPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConfidentiality == true) && (readQuestionsContact == false) && (questionsContactFlag == false))
        {
            questionsContactFlag = true;

            string title = "Right to Ask Questions and Contact Information";
            string message = "If you have any questions, please ask: Laurie Heller, Department of Psychology, " +
                "Carnegie Mellon University, Pittsburgh, PA, 15213, 412-268-8669, auditory@andrew.cmu.edu. " +
                "If you have questions later, or wish to withdraw your participation please contact the PI " +
                "by mail, phone, or e-mail using the contact information listed above.\n\n" +
                "If you have any questions pertaining to your rights as a research participant or to report " +
                "concerns, contact the Office of Research Integrity and Compliance at Carnegie Mellon " +
                "University: irb-review@andrew.cmu.edu. Phone: 412-268-1901 or 412-268-5460.";

#if UNITY_IOS
            IOSNative.ShowTwoG(title, message, "Next", "Back");
#endif
#if UNITY_ANDROID
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == false) && (questionsContactFlag == true) && (ad.yesclicked() == true || yesPressed == true))
        {
            readQuestionsContact = true;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            yesPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == false) && (questionsContactFlag == true) && (ad.noclicked() == true || noPressed == true))
        {
            questionsContactFlag = false;
            readCompCost = true;
            readConfidentiality = false;
            confidentialityFlag = false;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            noPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == true) && (readVoluntary == false) && (voluntaryFlag == false))
        {
            voluntaryFlag = true;

            string title = "Voluntary Participation";
            string message = "Your participation is voluntary. You may discontinue at any time.";

#if UNITY_IOS
            IOSNative.ShowTwoG(title, message, "Next", "Back");
#endif
#if UNITY_ANDROID
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readVoluntary == false) && (voluntaryFlag == true) && (ad.yesclicked() == true || yesPressed == true))
        {
            readVoluntary = true;
            android_window_displayed = false;
            can_display_window = false;
            finished_reading = true;
            hearingConsentForm = false;
            readingConsentForm = false;
#if UNITY_IOS
            yesPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readVoluntary == false) && (voluntaryFlag == true) && (ad.noclicked() == true || noPressed == true))
        {
            voluntaryFlag = false;
            readConfidentiality = true;
            readQuestionsContact = false;
            questionsContactFlag = false;
            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
            noPressed = false;
#endif
#if UNITY_ANDROID
            ad.clearflag();
#endif
        }

        Direction inputDirection = Direction.NONE;

        // Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        if (eh.isActivate()) // isActivate() has side effects so this order is required...
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
                    repeatInterruptedClips = false;
                    madeUnrecognizedGesture = false;
                    repeatSetupClip = false;
                    canRepeat = true;
                }
                // If the player's game environment is set up properly, let them go to the main menu.
                else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == false))
                {
                    debugPlayerInfo = "Tap registered. Game environment set up.";
                    environment_setup = true;
                    repeatInterruptedClips = false;
                    madeUnrecognizedGesture = false;
                    repeatSetupClip = false;
                    canRepeat = true;
                }
                // If the player has made a tap when they were asked to swipe left or right.
                else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                {
                    debugPlayerInfo = "Tap registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // Tap registered, but it doesn't do anything in this instance.
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
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
                        repeatSetupClip = false;
                        canRepeat = true;
                    }
                    // If the player has made a swipe left when they were asked to tap.
                    else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                    {
                        debugPlayerInfo = "Swiped left. Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
                        repeatSetupClip = false;
                        canRepeat = true;
                    }
                    // If the player has made a swipe right when they were asked to tap.
                    else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                    {
                        debugPlayerInfo = "Swiped right. Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                    // If the player has made a swipe up when they were asked to swipe left or right.
                    if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the player has made a swipe up when they were asked to tap.
                    else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the player's game environment is set up properly.
                    else if ((GM_main_pre.hasGoneThroughSetup == true) || ((orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == true)))
                    {
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
                        inputDirection = Direction.UP;
                    }
                }
                // If the swipe was down.
                else if (ie.isDown == true)
                {
                    // If the player has made a swipe down when they were asked to swipe left or right.
                    if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the player has made a swipe down when they were asked to tap.
                    else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the player's game environment is set up properly.
                    else if ((GM_main_pre.hasGoneThroughSetup == true) || ((orientation_correct == true) && (determined_talkback == true) && (plugin_earphone == true) && (environment_setup == true)))
                    {
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
                        inputDirection = Direction.DOWN;
                    }
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
            // If a rotation was registered.
            else if (ie.isRotate == true)
            {
                // If the player has made a rotation when they were asked to swipe left or right.
                if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                {
                    // If a left rotation was registered.
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    }
                    // If a right rotation was registered.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    }
                    SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If the player has made a rotation when they were asked to tap.
                else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                {
                    // If a left rotation was registered.
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. Incorrect gesture made. You should tap.";
                    }
                    // If a right rotation was registered.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. Incorrect gesture made. You should tap.";
                    }
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // Rotation registered, but it doesn't do anything in this instance.
                else
                {
                    // If a left rotation is registered.
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. This gesture does nothing in this menu.";
                    }
                    // If a right rotation is registered.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. This gesture does nothing in this menu.";
                    }
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
            // If a hold was registered.
            else if (ie.isHold == true)
            {
                // If the player has made a hold when they were asked to swipe left or right.
                if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == false))
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If the player has made a hold when they were asked to tap.
                else if ((GM_main_pre.hasGoneThroughSetup == false) && (orientation_correct == true) && (determined_talkback == true) && ((plugin_earphone == false) || (environment_setup == false)))
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should tap.";
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // Hold registered, but doesn't do anything in this instance.
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
        Screen.orientation = ScreenOrientation.Landscape;

        if (eh.isActivate())
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
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
                        repeatSetupClip = false;
                        canRepeat = true;
                    }
                    // If the swipe was right, the user is not using Talkback.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Swiped right. Player is not using Talkback.";
                        isUsingTalkback = false; // The player has told us they are not using Talkback.
                        determined_talkback = true; // Determined if the player is using Talkback or not.
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
                        repeatSetupClip = false;
                        canRepeat = true;
                    }
                    // If the swipe was up, that was not the gesture asked for.
                    else if (ie.isUp == true)
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    // If the swipe was down, that was not the gesture asked for.
                    else if (ie.isDown == true)
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.    
                }
                // If a tap is registered.
                else if (ie.isTap == true)
                {
                    debugPlayerInfo = "Tap registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.  
                    SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If a hold is registered.
                else if (ie.isHold == true)
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.     
                    SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If a rotation is registered.
                else if (ie.isRotate == true)
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
                    SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If an unrecognized gesture is made.
                else if (ie.isUnrecognized == true)
                {
                    madeUnrecognizedGesture = true;

                    // If this error was registered.
                    if (ie.isSwipeLeftHorizontalError == true)
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
                    // If another gesture error was registered that could have been a gesture other than a swipe left or right, tell the player that they should have swiped left or right.
                    else
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe left for Talkback or right for no Talkback.";
                        SoundManager.instance.PlayVoice(Database.errorClips[23], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                    repeatInterruptedClips = false;
                    madeUnrecognizedGesture = false;
                    repeatSetupClip = false;
                    canRepeat = true;
                }
                // If a hold is registered.
                else if (ie.isHold == true)
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.  
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If a swipe is registered.
                else if (ie.isSwipe == true)
                {
                    // If the swipe was left.
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Swiped left. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    // If the swipe was right.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Swiped right. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    // If the swipe was up.
                    else if (ie.isUp == true)
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    // If the swipe was down.
                    else if (ie.isDown == true)
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If a rotation is registered.
                else if (ie.isRotate == true)
                {
                    // If the rotation was left.
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    // If the rotation was right.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. Incorrect gesture made. You should tap to register that you are ready to continue.";
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.       
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                    else if (ie.isBetweenTapSwipeError == true)
                    {
                        debugPlayerInfo = "Nothing happened because gesture was in between tap and swipe.";
                        SoundManager.instance.PlayVoice(Database.errorClips[10], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap to register that you are ready to continue.";
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                    repeatInterruptedClips = false;
                    madeUnrecognizedGesture = false;
                    repeatSetupClip = false;
                    canRepeat = true;
                }
                // If a hold is registered.
                else if (ie.isHold == true)
                {
                    debugPlayerInfo = "Hold registered. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.  
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If a swipe is registered.
                else if (ie.isSwipe == true)
                {
                    // If the swipe was left.
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Swiped left. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    }
                    // If the swipe was right.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Swiped right. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    }
                    // If the swipe was up.
                    else if (ie.isUp == true)
                    {
                        debugPlayerInfo = "Swiped up. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    }
                    // If the swipe was down.
                    else if (ie.isDown == true)
                    {
                        debugPlayerInfo = "Swiped down. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.   
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If a rotation is registered.
                else if (ie.isRotate == true)
                {
                    // If a left rotation was registered.
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    }
                    // If a right rotation was registered.
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. Incorrect gesture made. You should tap to register that you have put in headphones.";
                    }
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                    else if (ie.isBetweenTapSwipeError == true)
                    {
                        debugPlayerInfo = "Nothing happened because gesture was in between tap and swipe.";
                        SoundManager.instance.PlayVoice(Database.errorClips[10], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap to register that you have put in headphones.";
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
                        inputDirection = Direction.RIGHT;
                    }
                    // If the swipe was left.
                    else if (ie.isLeft == true)
                    {
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
                        inputDirection = Direction.LEFT;
                    }
                    // If the swipe was up.
                    else if (ie.isUp == true)
                    {
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
                        inputDirection = Direction.UP;
                    }
                    // If the swipe was down.
                    else if (ie.isDown == true)
                    {
                        repeatInterruptedClips = false;
                        madeUnrecognizedGesture = false;
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
                    if (onEchoSetting == true)
                    {
                        usingOdeonEchoes = true;
                        usingHRTFEchoes = false;
                        debugPlayerInfo = "Swiped left. Switched to using Odeon echoes.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[23] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                    if (onConsentSetting == true)
                    {
                        readingConsentForm = true;
                        hearingConsentForm = false;
                        readConsent = false;
                        consentFlag = false;
                        readProcedures = false;
                        proceduresFlag = false;
                        readRequirements = false;
                        requirementsFlag = false;
                        readRisks = false;
                        risksFlag = false;
                        readBenefits = false;
                        benefitsFlag = false;
                        readCompCost = false;
                        compCostFlag = false;
                        readConfidentiality = false;
                        confidentialityFlag = false;
                        readQuestionsContact = false;
                        questionsContactFlag = false;
                        readVoluntary = false;
                        voluntaryFlag = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[10] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            can_display_window = true;
                        }, 3, null, true);
                    }
                }
                else if (inOptionsMenu == false)
                {
                    debugPlayerInfo = "Swiped right. Moving to pregame menu for post-tutorial levels.";
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
                    if (onEchoSetting == true)
                    {
                        usingHRTFEchoes = true;
                        usingOdeonEchoes = false;
                        debugPlayerInfo = "Swiped left. Switched to using HRTF echoes.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[22] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                    if (onConsentSetting == true)
                    {
                        hearingConsentForm = true;
                        readingConsentForm = false;
                        if (hearingConsentForm == true)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                hearingConsentForm = false;
                                readingConsentForm = false;
                            }, 3, null, true);
                        }
                    }
                }
                else if (inOptionsMenu == false)
                {
                    debugPlayerInfo = "Swiped left. Moving to pregame menu for tutorial levels.";
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
                    repeatSetupClip = false;
                    debugPlayerInfo = "Swiped up. Closed options menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    clips = new List<AudioClip>() { Database.mainMenuClips[26] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
                else if (inOptionsMenu == false)
                {
                    debugPlayerInfo = "Swiped up. Listening to commands.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    if (!listenToCmd)
                    {
                        listenToCmd = true;
                        canRepeat = true;
                    }
                }
                break;
            // If the player swiped down, open/close the options menu.
            case Direction.DOWN:
                if (inOptionsMenu == true)
                {
                    if (onEchoSetting == true)
                    {
                        onEchoSetting = false;
                        onConsentSetting = true;
                        debugPlayerInfo = "Swiped down. Moving to consent form settings.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        if (isUsingTalkback == true)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[25] };
                        }
                        else if (isUsingTalkback == false)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[24] };
                        }
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                    else if (onConsentSetting == true)
                    {
                        onConsentSetting = false;
                        onEchoSetting = true;
                        debugPlayerInfo = "Swiped down. Moving to echo settings.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        if (isUsingTalkback == true)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[21] };
                        }
                        else if (isUsingTalkback == false)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[20] };
                        }
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                }
                else if (inOptionsMenu == false)
                {
                    inOptionsMenu = true;
                    finished_reading = false;
                    debugPlayerInfo = "Swiped down. Opened options menu. On echo setting.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    if (isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[19], Database.soundEffectClips[0], Database.mainMenuClips[21] };
                    }
                    else if (isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[19], Database.soundEffectClips[0], Database.mainMenuClips[20] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
                break;
            default:
                break;
        }
    }
}
