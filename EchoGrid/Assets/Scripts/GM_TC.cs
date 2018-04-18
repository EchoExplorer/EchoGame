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
   
    /// <summary>
    /// Loads the terms and conditions data.
    /// </summary>
    void Awake()
    {
        URL_opened = false;
        android_window_displayed = false;
        ad = GetComponent<AndroidDialogue>();

        string[] consentResult = Utilities.Loadfile("consentRecord");
        int[] intResult = new int[1];
        if ((consentResult[0] != null) && (consentResult != null))
        {
            intResult = Array.ConvertAll<string, int>(consentResult, int.Parse);
            // if (intResult[0] == 1)
            //     SceneManager.LoadScene("Title_screen");
        }

        doneTesting = false;
        Utilities.initEncrypt();
    }

    void Start()
    {
        eh = new eventHandler (InputModule.instance);

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
                {//we're good to go
                    doneTesting = true;
                    titleText.text = Database.tcText_main;
                }
                else
                    titleText.text = str;
            }
        }

#if UNITY_IOS || UNITY_ANDROID
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
        android_window_displayed = true;
        URL_opened = true;
        finished_reading = true;
#endif
        play_audio();

// Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
		if (eh.isActivate())
        {
			InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

            /*clips = new List<AudioClip>() { Database.soundEffectClips[7] };

            // If a tap was registered.
            if (ie.isTap == true)
            {
                // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
            }
            // If a swipe was registered.
            else if (ie.isSwipe == true)
            {
                // If the swipe was left.
                if (ie.isLeft == true)
                {
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                }
                // If the swipe was right.
                else if (ie.isRight == true)
                {
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                }
                // If the swipe was up.
                else if (ie.isUp == true)
                {
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                }
                // If the swipe was down.
                else if (ie.isDown == true)
                {
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                }
            }
            // If a rotation was registered.
            else if (ie.isRotate == true)
            {
                // If it was a left rotation.
                if (ie.isLeft == true)
                {
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                }
                // If it was a right rotation.
                else if (ie.isRight == true)
                {
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                }
            }
            // If a hold was registered.
            else if (ie.isHold == true)
            {
                // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
            }*/           
        }

        // debugPlayerInfo = "Read Terms and Conditions. Moving to main menu.";
        // DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.        
        // clips = new List<AudioClip>() { Database.soundEffectClips[7] };
        // SoundManager.instance.PlayClips(clips); // To notify that this scene loaded, but it immediately moves to the main menu.
        SceneManager.LoadScene("Title_Screen"); // Move to main menu.
#endif
        // Check if we are running on iOS/Android.
#if UNITY_IOS || UNITY_ANDROID
        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

            /*clips = new List<AudioClip>() { Database.soundEffectClips[7] };

            if (ie.isTap == true)
            {
                Utilities.writefile("consentRecord", "1");
                // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                Screen.orientation = ScreenOrientation.Landscape;
            }
            else if (ie.isHold == true)
            {
                Utilities.writefile("consentRecord", "1");
                // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                Screen.orientation = ScreenOrientation.Landscape;
            }
            else if (ie.isSwipe == true)
            {
                if (ie.isUp == true)
                {
                    Utilities.writefile("consentRecord", "1");
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
                else if (ie.isDown == true)
                {
                    Utilities.writefile("consentRecord", "1");
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
                else if (ie.isLeft == true)
                {
                    Utilities.writefile("consentRecord", "1");
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
                else if (ie.isRight == true)
                {
                    Utilities.writefile("consentRecord", "1");
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
            }
            else if (ie.isRotate == true)
            {
                if (ie.isLeft == true)
                {
                    Utilities.writefile("consentRecord", "1");
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
                else if (ie.isRight == true)
                {
                    Utilities.writefile("consentRecord", "1");
                    // Notify the player we are moving to the main menu with the sound, then go to the main menu.
                    SoundManager.instance.PlayClips(clips, 0, () => SceneManager.LoadScene("Title_Screen"), 1);
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    Screen.orientation = ScreenOrientation.Landscape;
                }
            }*/          
        }

        Utilities.writefile("consentRecord", "1");
        // debugPlayerInfo = "Read Terms and Conditions. Moving to main menu.";
        // DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
        // clips = new List<AudioClip>() { Database.soundEffectClips[7] };
        // SoundManager.instance.PlayClips(clips); // To notify that this scene loaded, but it immediately moves to the main menu.
        SceneManager.LoadScene("Title_Screen"); // Move to main menu.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Landscape;
#endif // End of mobile platform dependendent compilation section started above with #elif
    }
}
