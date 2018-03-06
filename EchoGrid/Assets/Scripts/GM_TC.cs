using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

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
            if (intResult[0] == 1)
                SceneManager.LoadScene("Title_screen");
        }

        doneTesting = false;
        Utilities.initEncrypt();
    }

    void Start()
    {
        eh = new eventHandler (InputModule.instance);
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


#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
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

        //Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        SceneManager.LoadScene("Title_Screen");
        SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData();
        }

        //Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        Utilities.writefile("consentRecord", "1");
        SceneManager.LoadScene("Title_Screen");
        SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Landscape;
         if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData();
            
        }



#endif //End of mobile platform dependendent compilation section started above with #elif
    }
}
