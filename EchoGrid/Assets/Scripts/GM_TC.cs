using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// A script to display the user agreement dialogues.
/// This is attached to the ``GameManager`` GameObject in the T&C scene.
/// 
/// This file includes two checking item of user settings.
/// First is asking user to plugin earphones, tap to continue.
/// Second is asking user to hold the phone horizontaly once, tap to continue.
/// </summary>
public class GM_TC : MonoBehaviour
{

    int orti_clip = 0;
    bool finished_reading = false;
    bool URL_opened = false;
    bool android_window_displayed = false;
    public Text titleText;
    bool doneTesting = false;

    AndroidDialogue ad;

    eventHandler eh;

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    List<AudioClip> clips; // For sound clip that plays when we do a gesture in this scene.

    /*
    bool canRepeat = true;

    bool noConsent = false;
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
    bool eighteenPlusFlag = false;
    bool readEighteenPlus = false;
    bool understandFlag = false;
    bool readUnderstand = false;
    bool participateFlag = false;
    bool readParticipate = false;

    bool question1 = false;
    bool answeredQuestion1 = false;
    bool question2 = false;
    bool answeredQuestion2 = false;
    bool question3 = false;
    bool answeredQuestion3 = false;
    */

    /// <summary>
    /// Loads the terms and conditions data.
    /// </summary>
    void Awake()
    {
        URL_opened = false;
        android_window_displayed = false;
        ad = GetComponent<AndroidDialogue>();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Landscape;

        string[] consentResult = Utilities.Loadfile("consentRecord");
        int[] intResult = new int[1];
        if ((consentResult[0] != null) && (consentResult != null))
        {
            intResult = Array.ConvertAll<string, int>(consentResult, int.Parse);
            if (intResult[0] == 1)
            {
                // debugPlayerInfo = "Previously consented to having their data collected. Moving to main menu.";
                // DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                // SceneManager.LoadScene("Title_screen");
                // Screen.sleepTimeout = SleepTimeout.NeverSleep;
                // Screen.orientation = ScreenOrientation.Landscape;
            }
        }

        doneTesting = false;
        Utilities.initEncrypt();
    }

    void Start()
    {
        eh = new eventHandler(InputModule.instance);

#if UNITY_STANDALONE            
        // The textboxes in T&C are larger when in standalone than when building on PC or mobile for Android, so they should be made smaller and moved appropriately so we can see everything.
        GameObject tcTextbox; // T&C textbox object.
        tcTextbox = GameObject.Find("Canvas").gameObject.transform.Find("Text").gameObject; // Find the textbox.
        tcTextbox.transform.localScale -= new Vector3(0.3f, 0.3f, 0.3f); // Make the textbox smaller.

        GameObject inputTextbox; // Input textbox object.
        inputTextbox = GameObject.Find("Debug Canvas").gameObject.transform.Find("DebugInput").gameObject; // Find the textbox.
        inputTextbox.transform.localScale -= new Vector3(0.3f, 0.3f, 0.3f); // Make the textbox smaller.
        inputTextbox.transform.localPosition += new Vector3(-75.0f, 50.0f, 0.0f); // Move it to where it should be.

        GameObject playerTextbox; // Player textbox object.
        playerTextbox = GameObject.Find("Debug Canvas").gameObject.transform.Find("DebugPlayer").gameObject; // Find the textbox.
        playerTextbox.transform.localScale -= new Vector3(0.3f, 0.3f, 0.3f); // Make the textbox smaller.
        playerTextbox.transform.localPosition += new Vector3(-75.0f, 50.0f, 0.0f); // Move it to where it should be.

        GameObject touch0Textbox; // Touch0 textbox object.
        touch0Textbox = GameObject.Find("Debug Canvas").gameObject.transform.Find("DebugTouch0").gameObject; // Find the textbox.
        touch0Textbox.transform.localScale -= new Vector3(0.3f, 0.3f, 0.3f); // Make the textbox smaller.
        touch0Textbox.transform.localPosition -= new Vector3(0.0f, 30.0f, 0.0f); // Move it to where it should be.

        GameObject touch1Textbox; // Touch1 textbox object.
        touch1Textbox = GameObject.Find("Debug Canvas").gameObject.transform.Find("DebugTouch1").gameObject; // Find the textbox.
        touch1Textbox.transform.localScale -= new Vector3(0.3f, 0.3f, 0.3f); // Make the textbox smaller.
        touch1Textbox.transform.localPosition -= new Vector3(0.0f, 30.0f, 0.0f); // Move it to where it should be.

        GameObject touch2Textbox; // Touch2 textbox object.
        touch2Textbox = GameObject.Find("Debug Canvas").gameObject.transform.Find("DebugTouch2").gameObject; // Find the textbox.
        touch2Textbox.transform.localScale -= new Vector3(0.3f, 0.3f, 0.3f); // Make the textbox smaller.
        touch2Textbox.transform.localPosition -= new Vector3(0.0f, 30.0f, 0.0f); // Move it to where it should be.

        GameObject durationTextbox; // Touch duration textbox object.
        durationTextbox = GameObject.Find("Debug Canvas").gameObject.transform.Find("DebugTouchDuration").gameObject; // Find the textbox.
        durationTextbox.transform.localScale -= new Vector3(0.3f, 0.3f, 0.3f); // Make the textbox smaller.
        durationTextbox.transform.localPosition -= new Vector3(0.0f, 30.0f, 0.0f); // Move it to where it should be.
#endif
    }

    void OnLevelWasLoaded(int index)
    {
        //eh = new eventHandler (InputModule.instance);
    }

    /// <summary>
    /// Loads the online terms and consent page.
    /// </summary>
	private void reportConsent(string code)
    {
        string echoEndpoint = "https://echolock.andrew.cmu.edu/cgi-bin/acceptConsent.py";

        WWWForm echoForm = new WWWForm();
        echoForm.AddField("userName", Utilities.encrypt(SystemInfo.deviceUniqueIdentifier));
        echoForm.AddField("consentID", Utilities.encrypt(code));
        echoForm.AddField("dateTimeStamp", Utilities.encrypt(System.DateTime.Now.ToString()));

        Logging.Log(System.Text.Encoding.ASCII.GetString(echoForm.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www = new WWW(echoEndpoint, echoForm);
        StartCoroutine(Utilities.WaitForRequest(www));
    }


    //bool reset_audio = true;

    /// <summary>
    /// Plays voice instructions concerning the terms and conditions page.
    /// </summary>
    void play_audio()
    {
        /*
        if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false) && (finished_reading == false))
        {
            if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
            {
                canRepeat = false;
                clips = new List<AudioClip>() { Database.consentClips[0] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
            }
        }

        if ((hearingConsentForm == true) && (answeredQuestion1 == false))
        {
            if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
            {
                canRepeat = false;
                clips = new List<AudioClip>() { Database.consentClips[0], Database.consentClips[2] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
            }
        }

        if ((hearingConsentForm == true) && (answeredQuestion2 == false))
        {
            if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
            {
                canRepeat = false;
                clips = new List<AudioClip>() { Database.consentClips[0], Database.consentClips[3] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
            }
        }

        if ((hearingConsentForm == true) && (answeredQuestion3 == false))
        {
            if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
            {
                canRepeat = false;
                clips = new List<AudioClip>() { Database.consentClips[0], Database.consentClips[4] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
            }
        }

        if ((answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
        {
            if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
            {
                canRepeat = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[5] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
            }
        }

        if (noConsent == true)
        {
            if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
            {
                canRepeat = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[6] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
            }
        }

        if (((answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true)) && ((question1 == false) || (question2 == false) || (question3 == false)))
        {
            if (question1 == false)
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[9] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
            }
            else if (question2 == false)
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[10] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
            }
            else if (question3 == false)
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[11] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
            }
        }        
        */

        if (finished_reading)
        {
            //if (SoundManager.instance.PlayVoice (clips [cur_clip])) {
            //	cur_clip += 1;
            //	if (cur_clip >= orit.Length)
            //		cur_clip = 0;
            //}
        }
    }

    /// <summary>
    /// Checks for an internet connection, and opens the consent form if the connection is made.
    ///  The title screen is loaded once the form has been completed.
    /// </summary>
    void Update()
    {
        //MUST have internet connection
        if (Const.TEST_CONNECTION)
        {
            if (!doneTesting)
            {
                string str = Utilities.check_InternetConnection();
                if (str.Length == 0)
                { // we're good to go
                    doneTesting = true;
                    titleText.text = "Terms & Conditions\n\n" +
                        "Swipe left with two or three fingers to\n" +
                        "hear a consent form through audio.\n\n" +
                        "Swipe right with two or three fingers to\n" +
                        "read a consent form from physical text.\n\n" +
                        "If you do not wish to consent to having\n" +
                        "data from your game collected, swipe\n" +
                        "down with two or three fingers.";
                }
                else
                {
                    titleText.text = str;
                }
            }
        }        

#if (UNITY_IOS || UNITY_ANDROID) && (!UNITY_STANDALONE || !UNITY_WEBPLAYER)
        /*
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
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == false) && (questionsContactFlag == true) && (ad.noclicked() == true))
        {
            questionsContactFlag = false;
            readConfidentiality = false;
            confidentialityFlag = false;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == true) && (readEighteenPlus == false) && (eighteenPlusFlag == false))
        {
            eighteenPlusFlag = true;

            string title = "Age Limitation";
            string message = "I am age 18 or older.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readEighteenPlus == false) && (eighteenPlusFlag == true) && (ad.yesclicked() == true))
        {
            readEighteenPlus = true;
            answeredQuestion1 = true;
            question1 = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readEighteenPlus == false) && (eighteenPlusFlag == true) && (ad.noclicked() == true))
        {
            readEighteenPlus = true;
            answeredQuestion1 = true;
            question1 = false;
            ad.clearflag();                 
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readEighteenPlus == true) && (readUnderstand == false) && (understandFlag == false))
        {
            understandFlag = true;

            string title = "Read Information";
            string message = "I have read and understand the information above.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readUnderstand == false) && (understandFlag == true) && (ad.yesclicked() == true))
        {
            readUnderstand = true;
            answeredQuestion2 = true;
            question2 = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readUnderstand == false) && (understandFlag == true) && (ad.noclicked() == true))
        {
            readUnderstand = true;
            answeredQuestion2 = true;
            question2 = false;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readUnderstand == true) && (readParticipate == false) && (participateFlag == false))
        {
            participateFlag = true;

            string title = "Participation";
            string message = "I want to participate in this research and continue with the game and survey.";
            AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
            ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No");
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readParticipate == false) && (participateFlag == true) && (ad.yesclicked() == true))
        {
            readParticipate = true;
            answeredQuestion3 = true;
            question3 = true;
            android_window_displayed = false;
            finished_reading = true;
            ad.clearflag();
        }

        if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readParticipate == false) && (participateFlag == true) && (ad.noclicked() == true))
        {
            readParticipate = true;
            answeredQuestion3 = true;
            question3 = false;
            android_window_displayed = false;
            finished_reading = true;
            ad.clearflag();
        }
        */

        // Consent function temporarily disabled.
        /*
        if (!android_window_displayed)
        {
            android_window_displayed = true;
            finished_reading = false;
            ad.clearflag();
            ad.DisplayAndroidWindow(Database.tcmsg, AndroidDialogue.DialogueType.YESONLY);
        }

        if (!URL_opened && ad.yesclicked() && !finished_reading)
        {
            //open URL
            URL_opened = true;
            Application.OpenURL("http://echolock.andrew.cmu.edu/consent/");//"http://echolock.andrew.cmu.edu/consent/?"
        }
        else if (URL_opened && !finished_reading)
        {//report code from popup using reportConsent()
            finished_reading = true;
            ad.clearflag();
            ad.DisplayAndroidWindow("Enter code provided from \n the consent form:", AndroidDialogue.DialogueType.INPUT);
        }
        else if (URL_opened && finished_reading && ad.yesclicked())
        {
            Utilities.writefile("consentRecord", "1");
            reportConsent(ad.getInputStr());
            ad.clearflag();
            ad.DisplayAndroidWindow("Thank you!", AndroidDialogue.DialogueType.YESONLY);
            SceneManager.LoadScene("Title_Screen");
        }
        */
        // Assume consented
        // android_window_displayed = true;
        // URL_opened = true;
        // finished_reading = true;
#endif
        play_audio();

        // Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

            /*
            if (ie.isSwipe == true)
            {
                if (ie.isLeft == true)
                {
                    if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                    {
                        hearingConsentForm = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe left registered. Reading consent form through audio instructions.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                             
                        clips = new List<AudioClip>() { Database.consentClips[1], Database.consentClips[2] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == false))
                    {
                        question1 = false;
                        answeredQuestion1 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe left registered. Is not eighteen.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[3] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                    {
                        question2 = false;
                        answeredQuestion2 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe left registered. Did not understand information.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[4] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                    {
                        question3 = false;
                        answeredQuestion3 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe left registered. Does not want to participate.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                    if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                    {
                        answeredQuestion1 = false;
                        answeredQuestion2 = false;
                        answeredQuestion3 = false;
                        question1 = false;
                        question2 = false;
                        question3 = false;
                        canRepeat = true;
                    }
                    if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                    {
                        answeredQuestion1 = false;
                        answeredQuestion2 = false;
                        answeredQuestion3 = false;
                        question1 = false;
                        question2 = false;
                        question3 = false;
                        canRepeat = true;
                    }
                }
                else if (ie.isRight == true)
                {
                    if ((readingConsentForm == false) && (hearingConsentForm == false) && (noConsent == false))
                    {
                        readingConsentForm = true;
                        debugPlayerInfo = "Swipe right registered. Reading consent form manually.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.soundEffectClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        finished_reading = true;
                        answeredQuestion1 = true;
                        question1 = true;
                        answeredQuestion2 = true;
                        question2 = true;
                        answeredQuestion3 = true;
                        question3 = true;
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == false))
                    {
                        question1 = true;
                        answeredQuestion1 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe right registered. Is eighteen.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[3] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                    {
                        question2 = true;
                        answeredQuestion2 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe right registered. Understood information.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[4] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                    {
                        question3 = true;
                        answeredQuestion3 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe right registered. Wants to participate.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                       
                    }
                    if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                    {
                        finished_reading = false;
                        answeredQuestion1 = false;
                        answeredQuestion2 = false;
                        answeredQuestion3 = false;
                        question1 = false;
                        question2 = false;
                        question3 = false;
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
                        readEighteenPlus = false;
                        eighteenPlusFlag = false;
                        readUnderstand = false;
                        understandFlag = false;
                        readParticipate = false;
                        participateFlag = false;
                        canRepeat = true;
                    }
                    if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                    {
                        finished_reading = false;
                        answeredQuestion1 = false;
                        answeredQuestion2 = false;
                        answeredQuestion3 = false;
                        question1 = false;
                        question2 = false;
                        question3 = false;
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
                        readEighteenPlus = false;
                        eighteenPlusFlag = false;
                        readUnderstand = false;
                        understandFlag = false;
                        readParticipate = false;
                        participateFlag = false;
                        canRepeat = true;
                    }
                }
                else if (ie.isUp == true)
                {
                    debugPlayerInfo = "Swipe up registered. Does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                else if (ie.isDown == true)
                {
                    if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                    {
                        noConsent = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[6] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true))
                    {
                        hearingConsentForm = false;
                        readingConsentForm = false;
                        noConsent = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[6] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                }
            }

            else if (ie.isTap == true)
            {
                if (noConsent == true)
                {
                    Utilities.writefile("consentRecord", "0");
                    debugPlayerInfo = "Tap registered. Did not consent to having data collected. Moving to main menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                              
                    clips = new List<AudioClip>() { Database.consentClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, () => SceneManager.LoadScene("Title_Screen"), 1, 0.5f, true);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
                else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (finished_reading == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
                {
                    Utilities.writefile("consentRecord", "1");
                    debugPlayerInfo = "Tap registered. Consented to having data collected. Moving to main menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                         
                    clips = new List<AudioClip>() { Database.consentClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, () => SceneManager.LoadScene("Title_Screen"), 1, 0.5f, true);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
                else
                {
                    debugPlayerInfo = "Tap registered. Does nothing here.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            else if (ie.isHold == true)
            {
                debugPlayerInfo = "Hold registered. Does nothing in this menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
            else if (ie.isRotate == true)
            {
                if (ie.isLeft == true)
                {
                    debugPlayerInfo = "Left rotation registered. Does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                else if (ie.isRight == true)
                {
                    debugPlayerInfo = "Right rotation registered. Does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            */
        }
        SceneManager.LoadScene("Title_Screen");
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Landscape;
#endif
        // Check if we are running on iOS/Android.
#if UNITY_IOS || UNITY_ANDROID
        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.
            
            /*
            if (ie.isSwipe == true)
            {
                if (ie.isLeft == true)
                {
                    if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                    {
                        hearingConsentForm = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe left registered. Reading consent form through audio instructions.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                             
                        clips = new List<AudioClip>() { Database.consentClips[1], Database.consentClips[2] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == false))
                    {
                        question1 = false;
                        answeredQuestion1 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe left registered. Is not eighteen.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[3] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                    {                    
                        question2 = false;
                        answeredQuestion2 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe left registered. Did not understand information.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[4] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                    {
                        question3 = false;
                        answeredQuestion3 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe left registered. Does not want to participate.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                    if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                    {
                        answeredQuestion1 = false;
                        answeredQuestion2 = false;
                        answeredQuestion3 = false;
                        question1 = false;
                        question2 = false;
                        question3 = false;
                        canRepeat = true;
                    }
                    if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                    {               
                        answeredQuestion1 = false;
                        answeredQuestion2 = false;
                        answeredQuestion3 = false;
                        question1 = false;
                        question2 = false;
                        question3 = false;
                        canRepeat = true;
                    }
                }
                else if (ie.isRight == true)
                {
                    if ((readingConsentForm == false) && (hearingConsentForm == false) && (noConsent == false))
                    {
                        readingConsentForm = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe right registered. Reading consent form manually.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.soundEffectClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == false))
                    {
                        question1 = true;
                        answeredQuestion1 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe right registered. Is eighteen.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[3] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                    {
                        question2 = true;
                        answeredQuestion2 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe right registered. Understood information.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[4] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if ((hearingConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                    {
                        question3 = true;
                        answeredQuestion3 = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe right registered. Wants to participate.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                       
                    }
                    if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                    {
                        finished_reading = false;
                        answeredQuestion1 = false;
                        answeredQuestion2 = false;
                        answeredQuestion3 = false;
                        question1 = false;
                        question2 = false;
                        question3 = false;
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
                        readEighteenPlus = false;
                        eighteenPlusFlag = false;
                        readUnderstand = false;
                        understandFlag = false;
                        readParticipate = false;
                        participateFlag = false;
                        canRepeat = true;
                    }
                    if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                    {
                        finished_reading = false;
                        answeredQuestion1 = false;
                        answeredQuestion2 = false;
                        answeredQuestion3 = false;
                        question1 = false;
                        question2 = false;
                        question3 = false;
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
                        readEighteenPlus = false;
                        eighteenPlusFlag = false;
                        readUnderstand = false;
                        understandFlag = false;
                        readParticipate = false;
                        participateFlag = false;
                        canRepeat = true;
                    }
                }
                else if (ie.isUp == true)
                {
                    debugPlayerInfo = "Swipe up registered. Does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                else if (ie.isDown == true)
                {
                    if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                    {
                        noConsent = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[6] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true))
                    {
                        hearingConsentForm = false;
                        readingConsentForm = false;
                        noConsent = true;
                        canRepeat = true;
                        debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.consentClips[6] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                }
            }

            else if (ie.isTap == true)
            {
                if (noConsent == true)
                {
                    Utilities.writefile("consentRecord", "0");
                    debugPlayerInfo = "Tap registered. Did not consent to having data collected. Moving to main menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                            
                    clips = new List<AudioClip>() { Database.consentClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, () => SceneManager.LoadScene("Title_Screen"), 1, 0.5f, true);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
                else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
                {                    
                    Utilities.writefile("consentRecord", "1");
                    debugPlayerInfo = "Tap registered. Consented to having data collected. Moving to main menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                        
                    clips = new List<AudioClip>() { Database.consentClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, () => SceneManager.LoadScene("Title_Screen"), 1, 0.5f, true);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
                else
                {
                    debugPlayerInfo = "Tap registered. Does nothing here.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            else if (ie.isHold == true)
            {
                debugPlayerInfo = "Hold registered. Does nothing in this menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
            else if (ie.isRotate == true)
            {
                if (ie.isLeft == true)
                {
                    debugPlayerInfo = "Left rotation registered. Does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                else if (ie.isRight == true)
                {
                    debugPlayerInfo = "Right rotation registered. Does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            */
        }
#endif // End of mobile platform dependendent compilation section started above with #elif

        SceneManager.LoadScene("Title_Screen");
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Landscape;
    }
}
