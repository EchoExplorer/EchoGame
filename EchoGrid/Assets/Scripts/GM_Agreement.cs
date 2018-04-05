using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;
using System;

/// <summary>
/// A script to display the user agreement dialogues.
/// This is attached to the ``GameManager`` GameObject in the Agreement scene.
/// </summary>
public class GM_Agreement : MonoBehaviour
{
    AudioClip[] orit;
    int orti_clip = 0;
    AudioClip[] clips;
    int cur_clip = 0;
    bool isLandscape = true;
    //bool finished_reading = false;
    AudioClip swipeAhead;
    public GameObject AgreementTexts;
    int total_num_agreements = 7;
    int cur_text = 0;
    AndroidDialogue ad;

    string[] questions;

    eventHandler eh;

	string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    /// <summary>
    /// Loads all resources needed for the user agreement page.
    /// </summary>
    void Awake()
    {
    	eh = new eventHandler(InputModule.instance);

        ad = GetComponent<AndroidDialogue>();
        swipeAhead = Database.soundEffectClips[3];
        orit = new AudioClip[2];
        orit[0] = Resources.Load("instructions/Please hold your phone horizontally for this game") as AudioClip;
        orit[1] = Database.soundEffectClips[2];
        clips = new AudioClip[3];
        clips[0] = Resources.Load("instructions/Swipe left to confirm") as AudioClip;
        clips[1] = Database.soundEffectClips[2];
        clips[2] = Database.soundEffectClips[2];

        //total_num_agreements = AgreementTexts.transform.childCount;
        cur_text = 0;

        //load questions;
        questions = new string[7];
        questions[0] = "I am 18 or older.";
        questions[1] = "Do you have normal vision\n or vision that is corrected to normal?";
        questions[2] = "Do you have visual impairment\n that can't be corrected by glasses?";
        questions[3] = "Are you blind?";
        questions[4] = "I have normal hearing.";
        questions[5] = "I have read and understand \nall the information presented earlier.";
        questions[6] = "I want to participate this research \nand continue the game.\n";

        string[] consentResult = Utilities.Loadfile("consentRecord");
        int[] intResult = new int[1];
        if ((consentResult[0] != null) && (consentResult != null))
        {
            intResult = Array.ConvertAll<string, int>(consentResult, int.Parse);
            if (intResult[0] == 1)
                SceneManager.LoadScene("Title_screen");
        }
        gotoNextAgreement();
    }

    // Use this for initialization.
    void Start()
    {

    }

    bool reset_audio = false;
    /// <summary>
    /// Plays the instruction clips.
    /// </summary>
    void play_audio()
    {
        if (!isLandscape)
        {//not landscape!
            if (SoundManager.instance.PlayVoice(orit[orti_clip], reset_audio))
            {
                reset_audio = false;
                orti_clip += 1;
                if (orti_clip >= orit.Length)
                    orti_clip = 0;
            }
        }
        else
        {
            //if (SoundManager.instance.PlayVoice (clips [cur_clip])) {
            //	cur_clip += 1;
            //	if (cur_clip >= orit.Length)
            //		cur_clip = 0;
            //}
        }
    }

    /// <summary>
    /// Repeatedly polls to check if the dialogue buttons were pressed,
    ///  and transitions to the next instruction once the button is pressed.
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
            if (ie.isTap == true)
            {
                debugPlayerInfo = "This gesture does nothing in this menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
            // If a swipe was registered.
            if (ie.isSwipe == true)
            {
                // If the swipe was left.
                if (ie.isLeft == true)
                {
                    // Move to the next agreement.
                    debugPlayerInfo = "Swiped left. Moving to next agreement.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    gotoNextAgreement();
                    SoundManager.instance.PlayVoice(Database.soundEffectClips[3], true);
                }
                // If the swipe was right.
                else if (ie.isRight == true)
                {
                    debugPlayerInfo = "This gesture does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                // If the swipe was up.
                else if (ie.isUp == true)
                {
                    debugPlayerInfo = "This gesture does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                // If the swipe was down.
                else if (ie.isDown == true)
                {
                    debugPlayerInfo = "This gesture does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            // If a rotation was registered.
            else if (ie.isRotate == true)
            {
                // If it was a left rotation.
                if (ie.isLeft == true)
                {
                    debugPlayerInfo = "This gesture does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
                // If it was a right rotation.
                else if (ie.isRight == true)
                {
                    debugPlayerInfo = "This gesture does nothing in this menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            // If a hold was registered.
            else if (ie.isHold == true)
            {
                debugPlayerInfo = "This gesture does nothing in this menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }		
		}
#endif
// Check if we are running on iOS/Android.
#if UNITY_IOS || UNITY_ANDROID
        //AndroidDialogue
        if (ad.noclicked())
        {
            ad.clearflag();
            gotoNextAgreement();
        }
        else if (ad.yesclicked())
        {
            ad.clearflag();
            gotoNextAgreement();
        }

        //in-game control
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Landscape;
        if ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft) || (Input.deviceOrientation == DeviceOrientation.LandscapeRight))
            isLandscape = true;
        else if (Input.deviceOrientation == DeviceOrientation.Portrait)
        {
            isLandscape = false;
        }

        if (eh.isActivate()) 
        {
        	InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

        	// If a swipe was recognized.
        	if (ie.isSwipe == true)
        	{
        		// If the swipe was right.
        		if (ie.isRight == true)
        		{
					// Right
                    // SoundManager.instance.PlaySingle(swipeRight);
        		}
        		// If the swipe was left.
        		else if (ie.isLeft == true)
        		{
					// Move to the next agreement.
					debugPlayerInfo = "Swiped left. Moving to next agreement.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    gotoNextAgreement();
                    SoundManager.instance.PlayVoice(Database.soundEffectClips[3], true);
        		}
        		// If the swipe was up.
        		else if (ie.isUp == true)
        		{
					// Front
                    // SoundManager.instance.PlaySingle(swipeAhead);
        		}
        		// If the swipe was down.
        		else if (ie.isDown == true)
        		{
					// Back
                    // SoundManager.instance.PlaySingle(swipeAhead);
                    // credit
        		}
        	}
        	// If a tap was recognized.
        	else if (ie.isTap == true)
        	{
				// GameMode.gamemode = GameMode.Game_Mode.RESTART;
                // SceneManager.LoadScene("Main");
        	}
        }
#endif // End of mobile platform dependendent compilation section started above with #elif
    }

    /// <summary>
    /// Transitions across agreement dialogues and loads the next scene after the last dialogue.
    /// </summary>
    void gotoNextAgreement()
    {
        if (cur_text < total_num_agreements)
        {
            if ((cur_text == 5) || (cur_text == 6))
                ad.DisplayAndroidWindow(questions[cur_text], AndroidDialogue.DialogueType.YESONLY);
            else
                ad.DisplayAndroidWindow(questions[cur_text]);
        }

        if (cur_text >= total_num_agreements)
        {
            Utilities.writefile("consentRecord", "1");
            SceneManager.LoadScene("Title_screen");
        }
        else
        {
            //FIXME: Don't just comment things out like this, it gets confusing
            for (int i = 0; i < total_num_agreements; ++i)
            {
                //if (i == cur_text)
                //	AgreementTexts.transform.GetChild (i).gameObject.SetActive (true);
                //else
                //	AgreementTexts.transform.GetChild (i).gameObject.SetActive (false);
            }
            cur_text += 1;
        }
    }
}
