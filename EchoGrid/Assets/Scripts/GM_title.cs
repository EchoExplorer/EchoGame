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
    eventHandler eh;

    bool repeatSetupClip = false;

    public static bool isUsingTalkback = true; // Tells us if the player has told us that they are using Talkback or not.

    bool inOptionsMenu = false;
    bool onEchoSetting = true;
    bool onConsentSetting = false;
    public static bool switch_click_toggle = false; // If switch_click_toggle is false, then play the odeon click, which is option 1 in database.

    AndroidDialogue ad;
    bool android_window_displayed = false;
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
            if (((GM_main_pre.hasGoneThroughSetup == true) || (environment_setup == true)) && (listenToCmd == true) && (inOptionsMenu == false))
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
            if (((GM_main_pre.hasGoneThroughSetup == true) || (environment_setup == true)) && (inOptionsMenu == true) && (android_window_displayed == false))
            {
                if (SoundManager.instance.finishedAllClips == true)
                {
                    if (isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[10] };
                    }
                    else if (isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainMenuClips[9] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
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
        if (GM_main_pre.hasGoneThroughSetup == true)
        {
            determined_talkback = true;
            plugin_earphone = true;
            environment_setup = true;
            orientation_correct = true;
            repeatSetupClip = false;
        }

        play_audio();

        if ((readingConsentForm == true) && (android_window_displayed == false))
        {
            android_window_displayed = true;
            finished_reading = false;
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConsent == false) && (consentFlag == false))
        {
            consentFlag = true;

            string title = "Echolocation Consent";
            string message = "This game is part of a research study conducted by Laurie Heller and Pulkit Grover at Carnegie Mellon " +
                "University and is partially funded by Google. The purpose of the research is to understand how " +
                "people can use sounds (such as echoes) to figure out aspects of their physical environment, such " +
                "as whether or not a wall is nearby. The game will use virtual sounds and virtual walls to teach " +
                "people how to use sound to virtually move around in the game. This current release of the app is " +
                "designed to provide user feedback on the app itself.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.YESONLY;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConsent == false) && (consentFlag == true) && (ad.yesclicked() == true))
        {
            readConsent = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConsent == true) && (readProcedures == false) && (proceduresFlag == false))
        {
            proceduresFlag = true;

            string title = "Procedures";
            string message = "App users will install a free app on their phone named EchoGrid. Launching the app for the first " +
                "time will direct users to a consent form. If the user taps the screen to indicate that they are " +
                "providing informed consent to participate in the research supported by this app, they will be able " +
                "to begin playing the game.Users will first go through a tutorial that will provide spoken " +
                "instructions regarding the gestures needed to play the game, such as swiping or tapping on the " +
                "phone’s screen. Users will need to put on headphones correctly because the game’s sounds will differ " +
                "between the two ears. Users will play the game for as long as they want to. The game will increase in " +
                "difficulty as the levels increase. After a certain number of levels have been played, a survey regarding " +
                "the user experience will appear. The user will be asked to answer up to 18 questions regarding their " +
                "experience with the app and whether or not they have normal vision. This survey will only happen once.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readProcedures == false) && (proceduresFlag == true) && (ad.yesclicked() == true))
        {
            readProcedures = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readProcedures == false) && (proceduresFlag == true) && (ad.noclicked() == true))
        {
            proceduresFlag = false;
            readConsent = false;
            consentFlag = false;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readProcedures == true) && (readRequirements == false) && (requirementsFlag == false))
        {
            requirementsFlag = true;

            string title = "Participant Requirements";
            string message = "Participation in this study is limited to individuals age 18 and older. Participants with or without vision " +
                "may play this game. Participants need to have normal hearing because the game relies on detecting subtle " +
                "differences between sounds. Participants must have access to an Android smartphone to play this game.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRequirements == false) && (requirementsFlag == true) && (ad.yesclicked() == true))
        {
            readRequirements = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRequirements == false) && (requirementsFlag == true) && (ad.noclicked() == true))
        {
            requirementsFlag = false;
            readProcedures = false;
            proceduresFlag = false;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRequirements == true) && (readRisks == false) && (risksFlag == false))
        {
            risksFlag = true;

            string title = "Risks";
            string message = "The risks and discomfort associated with participation in this study are no greater than those " +
                "ordinarily encountered in daily life or during other online activities. Participants will not provide " +
                "confidential personal information or financial information.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRisks == false) && (risksFlag == true) && (ad.yesclicked() == true))
        {
            readRisks = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRisks == false) && (risksFlag == true) && (ad.noclicked() == true))
        {
            risksFlag = false;
            readRequirements = false;
            requirementsFlag = false;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRisks == true) && (readBenefits == false) && (benefitsFlag == false))
        {
            benefitsFlag = true;

            string title = "Benefits";
            string message = "There may be no personal benefit from your participation in the study but the knowledge received may be " +
                "of value to humanity. In theory, it is possible that you could become better at discriminating echoes in the real world " +
                "by playing this game, but the likelihood of this possibility is not known.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readBenefits == false) && (benefitsFlag == true) && (ad.yesclicked() == true))
        {
            readBenefits = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readBenefits == false) && (benefitsFlag == true) && (ad.noclicked() == true))
        {
            benefitsFlag = false;
            readRisks = false;
            risksFlag = false;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readBenefits == true) && (readCompCost == false) && (compCostFlag == false))
        {
            compCostFlag = true;

            string title = "Compensation and Costs";
            string message = "There is no compensation for participation in this study. There will be no cost to you if you " +
                "participate in this study.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readCompCost == false) && (compCostFlag == true) && (ad.yesclicked() == true))
        {
            readCompCost = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readCompCost == false) && (readCompCost == true) && (ad.noclicked() == true))
        {
            compCostFlag = false;
            readBenefits = false;
            benefitsFlag = false;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readCompCost == true) && (readConfidentiality == false) && (confidentialityFlag == false))
        {
            confidentialityFlag = true;

            string title = "Confidentiality";
            string message = "The data captured for the research does not include any personally identifiable information about you. " +
                "Your phone’s device ID will be captured, which is customary for all apps that you install on a phone. " +
                "You will indicate whether or not you have a visual impairment, but that is not considered to be private " +
                "health information. The moves you make while playing the game will be captured and your app satisfaction " +
                "survey responses will be captured.\n\n" +
                "By participating in this research, you understand and agree that Carnegie Mellon may be required to " +
                "disclose your consent form, data and other personally identifiable information as required by law, regulation, " +
                "subpoena or court order. Otherwise, your confidentiality will be maintained in the following manner:\n\n" +
                "Your data and consent form will be kept separate. Your response to the consent form will be stored electronically " +
                "in a secure location on Carnegie Mellon property and will not be disclosed to third parties. Sharing of data with " +
                "other researchers will only be done in such a manner that you will not be identified. This research was sponsored " +
                "by Google and the app survey data may be shared with them as part of the development process. By participating, you " +
                "understand and agree that the data and information gathered during this study may be used by Carnegie Mellon and " +
                "published and/or disclosed by Carnegie Mellon to others outside of Carnegie Mellon. However, your name, address, " +
                "contact information and other direct personal identifiers will not be gathered. Note that per regulation all research " +
                "data must be kept for a minimum of 3 years.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConfidentiality == false) && (confidentialityFlag == true) && (ad.yesclicked() == true))
        {
            readConfidentiality = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConfidentiality == false) && (confidentialityFlag == true) && (ad.noclicked() == true))
        {
            confidentialityFlag = false;
            readCompCost = false;
            compCostFlag = false;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConfidentiality == true) && (readQuestionsContact == false) && (questionsContactFlag == false))
        {
            questionsContactFlag = true;

            string title = "Right to Ask Questions and Contact Information";
            string message = "If you have any questions about this study, you should feel free to ask them by contacting the " +
                "Principal Investigator now at: Laurie Heller, Department of Psychology, Carnegie Mellon University, " +
                "Pittsburgh, PA, 15213, 412-268-8669, auditory@andrew.cmu.edu.\n\n" +
                "If you have questions later, desire additional information, or wish to withdraw your participation " +
                "please contact the Principal Investigator by mail, phone or e-mail in accordance with the contact " +
                "information listed above.\n\n" +
                "If you have questions pertaining to your rights as a research participant, or to report concerns to " +
                "this study, you should contact the Office of Research Integrity and Compliance at Carnegie Mellon " +
                "University.\n" +
                "Email: irb-review@andrew.cmu.edu.\n" +
                "Phone: 412-268-1901 or 412-268-5460.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == false) && (questionsContactFlag == true) && (ad.yesclicked() == true))
        {
            readQuestionsContact = true;
            android_window_displayed = false;
            finished_reading = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == false) && (questionsContactFlag == true) && (ad.noclicked() == true))
        {
            questionsContactFlag = false;
            readConfidentiality = false;
            confidentialityFlag = false;
            ad.clearflag();
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
                    if (onEchoSetting == true)
                    {
                        switch_click_toggle = false;
                        debugPlayerInfo = "Swiped left. Switched to using Odeon echoes.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[13] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if (onConsentSetting == true)
                    {
                        readingConsentForm = true;
                        hearingConsentForm = false;                          
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
                        switch_click_toggle = true;
                        debugPlayerInfo = "Swiped left. Switched to using HRTF echoes.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[12] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if (onConsentSetting == true)
                    {
                        hearingConsentForm = true;
                        readingConsentForm = false;
                        if (hearingConsentForm == true)
                        {
                            clips = new List<AudioClip>() { Database.consentClips[1] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
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
                    if (onEchoSetting == true)
                    {
                        onEchoSetting = false;
                        onConsentSetting = true;
                        debugPlayerInfo = "Swiped down. Moving to consent form settings.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                    else if (onConsentSetting == true)
                    {
                        onConsentSetting = false;
                        onEchoSetting = true;
                        debugPlayerInfo = "Swiped down. Moving to echo settings.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
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
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[10] };
                    }
                    else if (isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.mainMenuClips[9] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
                break;
            default:
                break;
        }
    }
}
