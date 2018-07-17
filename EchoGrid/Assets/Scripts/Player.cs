using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Allows us to use UI.
using System.Collections.Generic;
using SimpleJSON;
using System.Security.Cryptography;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

// Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
/// <summary>
/// A class representing the player object in the game.
/// It also is responsible for determining which echo sound to when the user
/// requests an echo, and for keeping track of and sending usage data
/// to a remote server when certain events occur in the game. It takes the
/// role of the GM_xyz objects for the Main scene too.
/// </summary>
public class Player : MovingObject
{
    public enum dist_type
    {
        WALL,
        SHORT,
        MID,
        LONG,
    }

    public enum DistRange
    {
        SHORT,
        MID,
        LONG,
    }

    // FIXME: these should not be public
    public static Player instance;
    // Delay time in seconds to restart level.
    public float restartLevelDelay = 3.0f;

    bool is_freezed; // is player not allowed to do anything?  
    bool reportSent;
    public int curLevel;          

    // private SpriteRenderer spriteRenderer;

    // variables to implement data collection
    public int numCrashes;
    // Keep track of number of times user crashed into wall
    public int numSteps;
    // Keep track of number of steps taken per level
    private int exitAttempts;

    private String lastEcho = "";

    // Track locations of the player's crashes
    private string crashLocs;

    // Keep track of time taken for the game level
    private Stopwatch stopWatch;
    private DateTime startTime;
    private DateTime endTime;
    AndroidDialogue ad;

    bool yesPressed = false;
    bool noPressed = false;
    bool naPressed = false;

    public void switchYes(string yes)
    {
        yesPressed = true;
    }

    public void switchNo(string no)
    {
        noPressed = true;
    }

    public void switchNA(string na)
    {
        naPressed = true;
    }

    private int echoNum = 0;

    // Usage data to keep track of
    public static bool want_exit = false;
    bool at_pause_menu = false; // indicating if the player activated pause menu
    bool localRecordWritten = false;
    // int score;
    eventHandler eh;

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    bool survey_activated = false;
    bool survey_shown = false;
    string consentCode = "";
    string surveyCode = "";

    List<AudioClip> clips;
    public AudioMixerGroup mixerGroup;

    bool madeUnrecognizedGesture = false;

    bool finishedTappingInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to make a tap.
    bool finishedSwipingInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to make a swipe.
    bool finishedMenuInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to open or close the pause menu.
    bool finishedExitingInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to exit a level.
    static bool finishedCornerInstruction = false; // Make sure the player cannot make a gesture we we are explaining to tap at a corner.
    bool hasStartedTurningInstruction = false;
    bool finishedTurningInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to make a rotation.    

    bool finishedEcho = false;

    bool waitingForOpenMenuInstruction = false; // Make sure the player cannot make gestures while we are explaining what you can do in the pause menu in the gesture tutorial.
    bool wantLevelRestart = false; // Used to make sure the player has to tap once after swiping left in the pause menu to confirm they want to restart the level.
    bool wantMainMenu = false; // Used to make sure the player has to tap once after swiping right in the pause menu to confirm they want to go to the main menu.

    static bool endingLevel;

    bool hasMoved = false;
    float ratio;

    bool tapped; 
    bool swipedUp;
    bool rotatedLeft;
    bool rotatedRight;

    static bool[,] canPlayClip = new bool[12, 7];
    bool canPlayHalfwayClip = false;
    bool canPlayApproachClip = false;
    bool canPlayTurnClip = false;
    bool canPlayForkClip = false;
    static bool playedExitClip;
    static bool canGoToNextLevel;

    static bool hasTappedAtCorner = false;

    public static bool reachedExit = false;

    bool backAtStart = false;
    bool playEchoOnce = false;

    bool restartLevel = false;
    bool goBackToMain = false;

    public static bool changingLevel = false;

    public static bool loadingScene = true;

    bool canDoGestureTutorial = false;

    bool haveTappedThreeTimes = false;
    bool haveSwipedThreeTimes = false;

    int level1_remaining_taps = -1; // Gesture tutorial level 1 remaining taps. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
    int level1_remaining_swipe_ups = -1; // Gesture tutorial level 1 remaining swipes up. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
    int level1_remaining_menus = -1; // Gesture tutorial level 1 remaining holds for pause menu. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
    int level3_remaining_turns = -1; // Gesture tutorial level 3 remaining turns/rotations. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.    

    AudioClip attenuatedClick = Database.attenuatedClick;
    AudioClip echofront = Database.hrtf_front;
	AudioClip echoleft = Database.hrtf_left_leftspeaker;
    AudioClip echoleft_right = Database.hrtf_left_rightspeaker;
    AudioClip echoright = Database.hrtf_right_rightspeaker;
    AudioClip echoright_left = Database.hrtf_right_leftspeaker;
    AudioClip echoleftfront = Database.hrtf_leftfront_leftspeaker;
    AudioClip echoleftfront_right = Database.hrtf_leftfront_rightspeaker;
    AudioClip echorightfront = Database.hrtf_rightfront_rightspeaker;
    AudioClip echorightfront_left = Database.hrtf_rightfront_leftspeaker;
    AudioClip echoleftend = Database.odeon_left_leftspeaker;
    AudioClip echoleftend_right = Database.odeon_left_rightspeaker;
    AudioClip echorightend = Database.odeon_right_rightspeaker;
    AudioClip echorightend_left = Database.odeon_right_leftspeaker;

    bool canCheckForConsent = false;
    bool hasCheckedForConsent = false;
    public static bool hasFinishedConsentForm = false;

    bool canRepeat = true;

    public static bool hasStartedConsent = false;

    bool canPlayLevel = false;
    bool canMakeGestures = false;

    bool finished_reading = false;
    bool finished_listening = false;
    bool finished_questions = false;
    bool can_display_window = false;
    bool android_window_displayed = false;

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
    bool voluntaryFlag = false;
    bool readVoluntary = false;
    bool eighteenPlusFlag = false;
    bool readEighteenPlus = false;
    bool understandFlag = false;
    bool readUnderstand = false;
    bool participateFlag = false;
    bool readParticipate = false;

    string question1 = "";
    bool answeredQuestion1 = false;
    string question2 = "";
    bool answeredQuestion2 = false;
    string question3 = "";
    bool answeredQuestion3 = false;

    bool finished_reading_survey = false;
    bool finished_listening_survey = false;
    bool finished_questions_survey = false;
    bool can_display_window_survey = false;
    bool can_submit_survey = false;

    bool noSurvey = false;
    bool readingSurvey = false;
    bool hearingSurvey = false;

    bool readControls = false;
    bool controlsFlag = false;
    bool readEasy = false;
    bool easyFlag = false;
    bool readEchoNavigate = false;
    bool echoNavigateFlag = false;
    bool readEnjoy = false;
    bool enjoyFlag = false;
    bool readFrustrating = false;
    bool frustratingFlag = false;
    bool readHearingImpaired = false;
    bool hearingImpairedFlag = false;
    bool readHints = false;
    bool hintsFlag = false;
    bool readInstructions = false;
    bool instructionsFlag = false;
    bool readLook = false;
    bool lookFlag = false;
    bool readLost = false;
    bool lostFlag = false;
    bool readPlayMore = false;
    bool playMoreFlag = false;
    bool readTutorial = false;
    bool tutorialFlag = false;
    bool readTutorialHelp = false;
    bool tutorialHelpFlag = false;
    bool readUnderstandEcho = false;
    bool understandEchoFlag = false;
    bool readVisuallyImpaired = false;
    bool visuallyImpairedFlag = false;

    string surveyQuestion1 = "";
    bool answeredSurveyQuestion1 = false;
    string surveyQuestion2 = "";
    bool answeredSurveyQuestion2 = false;
    string surveyQuestion3 = "";
    bool answeredSurveyQuestion3 = false;
    string surveyQuestion4 = "";
    bool answeredSurveyQuestion4 = false;
    string surveyQuestion5 = "";
    bool answeredSurveyQuestion5 = false;
    string surveyQuestion6 = "";
    bool answeredSurveyQuestion6 = false;
    string surveyQuestion7 = "";
    bool answeredSurveyQuestion7 = false;
    string surveyQuestion8 = "";
    bool answeredSurveyQuestion8 = false;
    string surveyQuestion9 = "";
    bool answeredSurveyQuestion9 = false;
    string surveyQuestion10 = "";
    bool answeredSurveyQuestion10 = false;
    string surveyQuestion11 = "";
    bool answeredSurveyQuestion11 = false;
    string surveyQuestion12 = "";
    bool answeredSurveyQuestion12 = false;
    string surveyQuestion13 = "";
    bool answeredSurveyQuestion13 = false;
    string surveyQuestion14 = "";
    bool answeredSurveyQuestion14 = false;
    string surveyQuestion15 = "";
    bool answeredSurveyQuestion15 = false;
   
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        enabled = true;
        // DontDestroyOnLoad(gameObject); // I don't understand why this is here, please check for bugs.
    }

    private void init()
    {
        numCrashes = 0;
        numSteps = 0;
        crashLocs = "";
        Utilities.initEncrypt();
        // Initialize list of crash locations
        crashLocs = "";
        // Start the time for the game level
        stopWatch = new Stopwatch();
        stopWatch.Start();
        startTime = System.DateTime.Now;

        want_exit = false;
        at_pause_menu = false;
        reportSent = false;
        ad = GetComponent<AndroidDialogue>();

        endingLevel = false;

        swipedUp = false;
        rotatedLeft = false;
        rotatedRight = false;
        playedExitClip = false;
        canGoToNextLevel = false;

        canCheckForConsent = false;

        wantLevelRestart = false;
        wantMainMenu = false;

        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                canPlayClip[i, j] = true;
            }
        }

        // score = 1000;
        eh = new eventHandler(InputModule.instance);
        base.Start();
    }

    protected override void Start()
    {
        curLevel = GameManager.instance.level;
        echoNum = 0;
        init();
        // Adjust player scale
        Vector3 new_scale = transform.localScale;
        new_scale *= (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;
        transform.localScale = new_scale;
    }

    private string _dist_type_to_string(dist_type type)
    {
        switch (type)
        {
            case dist_type.WALL:
                return "w";
            case dist_type.SHORT:
                return "s";
            case dist_type.MID:
                return "m";
            case dist_type.LONG:
                return "l";
            default:
                break;
        }

        return "na";
    }

    private IEnumerator DelayedPlayEcho(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayEcho();
    }

    /// <summary>
    /// A function that determines which echo file to play based on the surrounding environment.
    /// </summary>
    private void PlayEcho(bool real = true)
    {
        attenuatedClick = Database.attenuatedClick;
        echofront = Database.hrtf_front;
        echoleft = Database.hrtf_left_leftspeaker;
        echoleft_right = Database.hrtf_left_rightspeaker;
        echoright = Database.hrtf_right_rightspeaker;
        echoright_left = Database.hrtf_right_leftspeaker;
        echoleftfront = Database.hrtf_leftfront_leftspeaker;
        echoleftfront_right = Database.hrtf_leftfront_rightspeaker;
        echorightfront = Database.hrtf_rightfront_rightspeaker;
        echorightfront_left = Database.hrtf_rightfront_leftspeaker;
        echoleftend = Database.odeon_left_leftspeaker;
        echoleftend_right = Database.odeon_left_rightspeaker;
        echorightend = Database.odeon_right_rightspeaker;
        echorightend_left = Database.odeon_right_leftspeaker;

        Vector3 dir = transform.right;
        int dir_x = (int)Math.Round(dir.x);
        int dir_y = (int)Math.Round(dir.y);
        int x = (int)BoardManager.player_idx.x;
        int y = (int)BoardManager.player_idx.y;

        GameObject frontWall = null, leftWall = null, rightWall = null, leftFrontWall = null, rightFrontWall = null, leftEndWall = null, rightEndWall = null;

        if ((dir_x > 0) && (dir_y == 0))
        {
            leftWall = GameObject.Find("Wall_" + x + "_" + (y + 1));
            rightWall = GameObject.Find("Wall_" + x + "_" + (y - 1));
            leftFrontWall = GameObject.Find("Wall_" + (x + 1) + "_" + (y + 1));
            rightFrontWall = GameObject.Find("Wall_" + (x + 1) + "_" + (y - 1));
        }

        if ((dir_x < 0) && (dir_y == 0))
        {
            leftWall = GameObject.Find("Wall_" + x + "_" + (y - 1));
            rightWall = GameObject.Find("Wall_" + x + "_" + (y + 1));
            leftFrontWall = GameObject.Find("Wall_" + (x - 1) + "_" + (y - 1));
            rightFrontWall = GameObject.Find("Wall_" + (x - 1) + "_" + (y + 1));
        }

        if ((dir_x == 0) && (dir_y > 0))
        {
            leftWall = GameObject.Find("Wall_" + (x - 1) + "_" + y);
            rightWall = GameObject.Find("Wall_" + (x + 1) + "_" + y);
            leftFrontWall = GameObject.Find("Wall_" + (x - 1) + "_" + (y + 1));
            rightFrontWall = GameObject.Find("Wall_" + (x + 1) + "_" + (y + 1));
        }

        if ((dir_x == 0) && (dir_y < 0))
        {
            leftWall = GameObject.Find("Wall_" + (x + 1) + "_" + y);
            rightWall = GameObject.Find("Wall_" + (x - 1) + "_" + y);
            leftFrontWall = GameObject.Find("Wall_" + (x + 1) + "_" + (y - 1));
            rightFrontWall = GameObject.Find("Wall_" + (x - 1) + "_" + (y - 1));
        }

        float stepsToFrontWall = 0.0f;
        float stepsToLeftEnd = 0.0f;
        float stepsToRightEnd = 0.0f;
        if (leftWall == null)
        {
            while (leftEndWall == null)
            {
                if ((dir_x > 0) && (dir_y == 0))
                {
                    leftEndWall = GameObject.Find("Wall_" + x + "_" + (y + (stepsToLeftEnd + 1.0f)));
                }

                if ((dir_x < 0) && (dir_y == 0))
                {
                    leftEndWall = GameObject.Find("Wall_" + x + "_" + (y - (stepsToLeftEnd + 1.0f)));
                }

                if ((dir_x == 0) && (dir_y > 0))
                {
                    leftEndWall = GameObject.Find("Wall_" + (x - (stepsToLeftEnd + 1.0f)) + "_" + y);
                }

                if ((dir_x == 0) && (dir_y < 0))
                {
                    leftEndWall = GameObject.Find("Wall_" + (x + (stepsToLeftEnd + 1.0f)) + "_" + y);
                }
                stepsToLeftEnd += 1.0f;
            }
            stepsToLeftEnd -= 1.0f;
        }
        if (rightWall == null)
        {
            while (rightEndWall == null)
            {
                if ((dir_x > 0) && (dir_y == 0))
                {
                    rightEndWall = GameObject.Find("Wall_" + x + "_" + (y - ((int)stepsToRightEnd + 1)));
                }

                if ((dir_x < 0) && (dir_y == 0))
                {
                    rightEndWall = GameObject.Find("Wall_" + x + "_" + (y + ((int)stepsToRightEnd + 1)));
                }

                if ((dir_x == 0) && (dir_y > 0))
                {
                    rightEndWall = GameObject.Find("Wall_" + (x + ((int)stepsToRightEnd + 1)) + "_" + y);
                }

                if ((dir_x == 0) && (dir_y < 0))
                {
                    rightEndWall = GameObject.Find("Wall_" + (x - ((int)stepsToRightEnd + 1)) + "_" + y);
                }
                stepsToRightEnd += 1;
            }
            stepsToRightEnd -= 1;
        }

        do
        {
            if ((dir_x > 0) && (dir_y == 0))
            {
                frontWall = GameObject.Find("Wall_" + (x + ((int)stepsToFrontWall + 1)) + "_" + y);
            }

            if ((dir_x < 0) && (dir_y == 0))
            {
                frontWall = GameObject.Find("Wall_" + (x - ((int)stepsToFrontWall + 1)) + "_" + y);
            }

            if ((dir_x == 0) && (dir_y > 0))
            {
                frontWall = GameObject.Find("Wall_" + x + "_" + (y + ((int)stepsToFrontWall + 1)));
            }

            if ((dir_x == 0) && (dir_y < 0))
            {           
                frontWall = GameObject.Find("Wall_" + x + "_" + (y - ((int)stepsToFrontWall + 1)));
            }
            stepsToFrontWall += 1;
        }
        while (frontWall == null);
        stepsToFrontWall -= 1;

        if (real == true)
        {
            if (leftWall != null)
            {
                // print("Left Wall = " + leftWall.name);
            }
            if (rightWall != null)
            {
                // print("Right Wall = " + rightWall.name);
            }
            if (leftFrontWall != null)
            {
                // print("Left Front Wall = " + leftFrontWall.name);
            }
            if (rightFrontWall != null)
            {
                // print("Right Front Wall = " + rightFrontWall.name);
            }
            if (leftEndWall != null)
            {
                // print("Left End Wall = " + leftEndWall.name);
                // print("Steps to Left End = " + stepsToLeftEnd.ToString());
            }
            if (rightEndWall != null)
            {
                // print("Right End Wall = " + rightEndWall.name);
                // print("Steps to Right End = " + stepsToRightEnd.ToString());
            }
            if (frontWall != null)
            {
                // print("Front Wall = " + frontWall.name);
                // print("Steps to Front Wall = " + stepsToFrontWall.ToString());
            }                                   
        }       

        AudioSource[] frontAudios = null, leftAudios = null, left_rightAudios = null, rightAudios = null, right_leftAudios = null, leftFrontAudios = null, leftFront_rightAudios = null, rightFrontAudios = null, rightFront_leftAudios = null, leftEndAudios = null, leftEnd_rightAudios = null, rightEndAudios = null, rightEnd_leftAudios = null;
        AudioSource frontAudioSource = null, leftAudioSource = null, left_rightAudioSource = null, rightAudioSource = null, right_leftAudioSource = null, leftFrontAudioSource = null, leftFront_rightAudioSource = null, rightFrontAudioSource = null, rightFront_leftAudioSource = null, leftEndAudioSource = null, leftEnd_rightAudioSource = null, rightEndAudioSource = null, rightEnd_leftAudioSource = null;

        frontAudios = frontWall.GetComponents<AudioSource>();

        if (frontAudios.Length == 3)
        { 
            frontAudioSource = frontAudios[0];
            frontAudioSource.clip = echofront;
            frontAudioSource.volume = getEchoVolume(stepsToFrontWall, "front");
            frontAudioSource.panStereo = 0.0f;
        }

        if (leftWall != null)
        {
            leftAudios = leftWall.GetComponents<AudioSource>();
            left_rightAudios = leftWall.GetComponents<AudioSource>();

            if ((leftAudios.Length == 3) && (left_rightAudios.Length == 3))
            {
                leftAudioSource = leftAudios[1];
                leftAudioSource.clip = echoleft;
                leftAudioSource.volume = getEchoVolume(0.0f, "left");
                leftAudioSource.panStereo = -1.0f;

                left_rightAudioSource = left_rightAudios[2];
                left_rightAudioSource.clip = echoleft_right;
                left_rightAudioSource.volume = getEchoVolume(0.0f, "left_right");
                left_rightAudioSource.panStereo = 1.0f;
            }
        }
        if (leftFrontWall != null)
        {
            leftFrontAudios = leftFrontWall.GetComponents<AudioSource>();
            leftFront_rightAudios = leftFrontWall.GetComponents<AudioSource>();  
            
            if ((leftFrontAudios.Length == 3) && (leftFront_rightAudios.Length == 3))
            {
                leftFrontAudioSource = leftFrontAudios[1];
                leftFrontAudioSource.clip = echoleftfront;
                leftFrontAudioSource.volume = getEchoVolume(Mathf.Sqrt(2), "left_front");
                leftFrontAudioSource.panStereo = -1.0f;

                leftFront_rightAudioSource = leftFront_rightAudios[2];
                leftFront_rightAudioSource.clip = echoleftfront_right;
                leftFront_rightAudioSource.volume = getEchoVolume(Mathf.Sqrt(2), "left_front_right");
                leftFront_rightAudioSource.panStereo = 1.0f;
            }
        }
        if ((leftEndWall != null) && (leftWall == null))
        {
            leftEndAudios = leftEndWall.GetComponents<AudioSource>();
            leftEnd_rightAudios = leftEndWall.GetComponents<AudioSource>();
            
            if ((leftEndAudios.Length == 3) && (leftEnd_rightAudios.Length == 3))
            {
                leftEndAudioSource = leftEndAudios[1];
                leftEndAudioSource.clip = echoleftend;
                leftEndAudioSource.volume = getEchoVolume(stepsToLeftEnd, "left_end");
                leftEndAudioSource.panStereo = -1.0f;

                leftEnd_rightAudioSource = leftEnd_rightAudios[2];
                leftEnd_rightAudioSource.clip = echoleftend_right;
                leftEnd_rightAudioSource.volume = getEchoVolume(stepsToLeftEnd, "left_end_right");
                leftEnd_rightAudioSource.panStereo = 1.0f;
            }
        }
        if (rightWall != null)
        {
            rightAudios = rightWall.GetComponents<AudioSource>();
            right_leftAudios = rightWall.GetComponents<AudioSource>();

            if ((rightAudios.Length == 3) && (right_leftAudios.Length == 3))
            {
                rightAudioSource = rightAudios[1];
                rightAudioSource.clip = echoright;
                rightAudioSource.volume = getEchoVolume(0.0f, "right");
                rightAudioSource.panStereo = 1.0f;

                right_leftAudioSource = right_leftAudios[2];
                right_leftAudioSource.clip = echoright_left;
                right_leftAudioSource.volume = getEchoVolume(0.0f, "right_left");
                right_leftAudioSource.panStereo = -1.0f;
            }
        }
        if (rightFrontWall != null)
        {
            rightFrontAudios = rightFrontWall.GetComponents<AudioSource>();
            rightFront_leftAudios = rightFrontWall.GetComponents<AudioSource>();

            if ((rightFrontAudios.Length == 3) && (rightFront_leftAudios.Length == 3))
            {
                rightFrontAudioSource = rightFrontAudios[1];
                rightFrontAudioSource.clip = echorightfront;
                rightFrontAudioSource.volume = getEchoVolume(Mathf.Sqrt(2), "right_front");
                rightFrontAudioSource.panStereo = 1.0f;

                rightFront_leftAudioSource = rightFront_leftAudios[2];
                rightFront_leftAudioSource.clip = echorightfront_left;
                rightFront_leftAudioSource.volume = getEchoVolume(Mathf.Sqrt(2), "right_front_left");
                rightFront_leftAudioSource.panStereo = -1.0f;
            }
        }
        if ((rightEndWall != null) && (rightWall == null))
        {
            rightEndAudios = rightEndWall.GetComponents<AudioSource>();
            rightEnd_leftAudios = rightEndWall.GetComponents<AudioSource>();

            if ((rightEndAudios.Length == 3) && (rightEnd_leftAudios.Length == 3))
            {
                rightEndAudioSource = rightEndAudios[1];
                rightEndAudioSource.clip = echorightend;
                rightEndAudioSource.volume = getEchoVolume(stepsToRightEnd, "right_end");
                rightEndAudioSource.panStereo = 1.0f;

                rightEnd_leftAudioSource = rightEnd_leftAudios[2];
                rightEnd_leftAudioSource.clip = echorightend_left;
                rightEnd_leftAudioSource.volume = getEchoVolume(stepsToRightEnd, "right_end_left");
                rightEnd_leftAudioSource.panStereo = -1.0f;
            }
        }
    
        if (real == false)
        {
            return;
        }

        float leftDelay = 0.0f;
        float rightDelay = 0.0f;
        float leftFrontDelay = 0.0f;
        float rightFrontDelay = 0.0f;
        float leftEndDelay = 0.0f;
        float rightEndDelay = 0.0f;
        float frontDelay = 0.0f;

        SoundManager.instance.PlaySingle(attenuatedClick);

        if ((leftAudioSource != null) && (left_rightAudioSource != null))
        {
            leftDelay = 1.5f / 340;
            leftAudioSource.PlayDelayed(leftDelay);
            left_rightAudioSource.PlayDelayed(leftDelay);
            // print("Left played! Delay = " + leftDelay.ToString());
        }
        if ((rightAudioSource != null) && (right_leftAudioSource != null))
        {
            rightDelay = 1.5f / 340;
            rightAudioSource.PlayDelayed(rightDelay);
            right_leftAudioSource.PlayDelayed(rightDelay);
            // print("Right played! Delay = " + rightDelay.ToString());
        }
        if ((leftFrontAudioSource != null) && (leftFront_rightAudioSource != null))
        {
            leftFrontDelay = 2.12132f / 340;
            leftFrontAudioSource.PlayDelayed(leftFrontDelay);
            leftFront_rightAudioSource.PlayDelayed(leftFrontDelay);
            // print("Left front played! Delay = " + leftFrontDelay.ToString());
        }
        if ((rightFrontAudioSource != null) && (rightFront_leftAudioSource != null))
        {
            rightFrontDelay = 2.12132f / 340;
            rightFrontAudioSource.PlayDelayed(rightFrontDelay);
            rightFront_leftAudioSource.PlayDelayed(rightFrontDelay);
            // print("Right front played! Delay = " + rightFrontDelay.ToString());
        }
        if ((leftEndAudioSource != null) && (leftEnd_rightAudioSource != null))
        {
            leftEndDelay = (1.5f * stepsToLeftEnd + 0.75f) * 2 / 340;
            leftEndAudioSource.PlayDelayed(leftEndDelay);
            leftEnd_rightAudioSource.PlayDelayed(leftEndDelay);
            // print("Left End is " + stepsToLeftEnd + " steps away! Delay = " + leftEndDelay.ToString());
        }
        if ((rightEndAudioSource != null) && (rightEnd_leftAudioSource != null))
        {
            rightEndDelay = (1.5f * stepsToRightEnd + 0.75f) * 2 / 340;
            rightEndAudioSource.PlayDelayed(rightEndDelay);
            rightEnd_leftAudioSource.PlayDelayed(rightEndDelay);
            // print("Right End is " + stepsToRightEnd + " steps away! Delay = " + rightEndDelay.ToString());
        }

        frontDelay = (1.5f * stepsToFrontWall + 0.75f) * 2 / 340;
        frontAudioSource.PlayDelayed(frontDelay);
        // print("Front Wall is " + stepsToFrontWall + " steps away! Delay = " + frontDelay.ToString());

        float waitTime = 0.0f;
        float[] delayTimes = { leftDelay, rightDelay, leftFrontDelay, rightFrontDelay, leftEndDelay, rightEndDelay, frontDelay };

        StartCoroutine(FinishedEchoClip(waitTime, delayTimes));

        return;
    }   

    float getEchoVolume(float stepsAway, string echoType)
    {
        if (stepsAway == 0)
        {
            if (echoType == "front")
            {
                if (curLevel < 17)
                {
                    return 1.0000f;
                }        
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.8300f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.7000f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.5800f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.4940f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.4250f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.3600f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.3050f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.2670f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.2300f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.2000f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.1800f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.1560f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.1380f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.1200f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.1060f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0950f;
                }                
            }
            if ((echoType == "left") || (echoType == "left_right") || (echoType == "right") || (echoType == "right_left"))
            {
                if (curLevel < 17)
                {
                    return 0.5000f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.4320f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.3690f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.3123f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.2635f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.2330f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.2040f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.1776f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.1584f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.1391f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.1223f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.1064f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0936f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0836f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0750f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0665f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0587f;
                }
            }
        }
        if (stepsAway == Mathf.Sqrt(2))
        {
            if ((echoType == "left_front") || (echoType == "left_front_right") || (echoType == "right_front") || (echoType == "right_front_left"))
            {
                if (curLevel < 17)
                {
                    return 0.3310f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.2894f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.2490f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.2150f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.1885f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.1655f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.1460f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.1255f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.1115f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0957f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0854f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0757f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0681f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0605f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0535f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0483f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0432f;
                }
            }
        }
        if (stepsAway == 1)
        {
            if (echoType == "front")
            {
                if (curLevel < 17)
                {
                    return 0.4940f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.4280f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.3650f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.3088f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.2600f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.2300f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.2010f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.1751f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.1564f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.1371f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.1208f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.1049f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0921f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0826f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0740f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0655f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0582f;
                }
            }
            if ((echoType == "left_end") || (echoType == "left_end_right") || (echoType == "right_end") || (echoType == "right_end_left"))
            {
                if (curLevel < 17)
                {
                    return 0.2810f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.2480f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.2190f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.1920f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.1700f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.1535f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.1380f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.1245f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.1110f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0970f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0880f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0760f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0680f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0610f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0543f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0485f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0435f;
                }
            }
        }
        if (stepsAway == 2)
        {
            if (echoType == "front")
            {
                if (curLevel < 17)
                {
                    return 0.3370f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.2949f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.2540f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.2200f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.1930f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.1700f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.1500f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.1295f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.1150f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0992f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0884f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0787f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0706f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0630f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0555f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0503f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0447f;
                }
            }
            if ((echoType == "left_end") || (echoType == "left_end_right") || (echoType == "right_end") || (echoType == "right_end_left"))
            {
                if (curLevel < 17)
                {
                    return 0.2160f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1920f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.1700f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.1505f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.1345f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.1210f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.1080f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0950f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0850f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0760f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0665f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0600f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0530f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0470f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0425f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0370f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0330f;
                }
            }
        }
        if (stepsAway == 3)
        {
            if (echoType == "front")
            {
                if (curLevel < 17)
                {
                    return 0.2700f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.2363f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.2040f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.1760f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.1525f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.1340f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.1170f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.1007f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0909f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0805f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0730f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0636f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0561f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0512f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0457f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0410f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0357f;
                }
            }
            if ((echoType == "left_end") || (echoType == "left_end_right") || (echoType == "right_end") || (echoType == "right_end_left"))
            {
                if (curLevel < 17)
                {
                    return 0.1730f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1535f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.1375f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.1230f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.1095f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.0970f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0865f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0795f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0665f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0610f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0545f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0485f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0430f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0385f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0340f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0305f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0275f;
                }
            }
        }
        if (stepsAway == 4)
        {
            if (echoType == "front")
            {
                if (curLevel < 17)
                {
                    return 0.2250f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1920f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.1688f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.1470f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.1280f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.1140f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.1010f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0888f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0795f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0712f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0615f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0547f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0481f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0432f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0384f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0345f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0312f;
                }
            }
            if ((echoType == "left_end") || (echoType == "left_end_right") || (echoType == "right_end") || (echoType == "right_end_left"))
            {
                if (curLevel < 17)
                {
                    return 0.1480f;                    
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1310f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.1180f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.1050f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.0930f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.0835f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0740f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0655f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0595f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0520f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0475f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0425f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0377f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0335f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0296f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0268f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0238f;
                }
            }
        }
        if (stepsAway == 5)
        {
            if (echoType == "front")
            {
                if (curLevel < 17)
                {
                    return 0.1970f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1724f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.1490f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.1320f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.1140f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.1010f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0910f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0801f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0692f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0613f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0555f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0490f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0438f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0390f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0351f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0305f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0273f;
                }
            }
            if ((echoType == "left_end") || (echoType == "left_end_right") || (echoType == "right_end") || (echoType == "right_end_left"))
            {
                if (curLevel < 17)
                {
                    return 0.1330f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1200f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.1075f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.0940f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.0835f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.0760f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0655f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0595f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0520f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0470f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0430f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0385f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0345f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0300f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0265f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0235f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0210f;
                }
            }
        }
        if (stepsAway == 6)
        {
            if (echoType == "front")
            {
                if (curLevel < 17)
                {
                    return 0.1750f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1520f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.1320f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.1160f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.1020f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.0910f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0790f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0723f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0618f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0561f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0490f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0445f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0385f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0354f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0313f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0280f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0245f;
                }
            }
            if ((echoType == "left_end") || (echoType == "left_end_right") || (echoType == "right_end") || (echoType == "right_end_left"))
            {
                if (curLevel < 17)
                {
                    return 0.1185f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1060f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.0930f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.0835f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.0745f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.0665f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0590f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0535f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0470f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0420f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0375f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0335f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0300f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0265f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0237f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0210f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0191f;
                }
            }
        }
        if (stepsAway == 7)
        {
            if (echoType == "front")
            {
                if (curLevel < 17)
                {
                    return 0.1560f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1350f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.1190f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.1040f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.0935f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.0830f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0720f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0647f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0578f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0510f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0447f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0399f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0350f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0310f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0280f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0245f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0220f;
                }
            }
            if ((echoType == "left_end") || (echoType == "left_end_right") || (echoType == "right_end") || (echoType == "right_end_left"))
            {
                if (curLevel < 17)
                {
                    return 0.1095f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.0970f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.0865f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.0795f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.0665f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.0610f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0545f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0485f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0430f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0385f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0345f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0310f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0275f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0245f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0218f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0192f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0172f;
                }
            }
        }
        if (stepsAway == 8)
        {
            if (echoType == "front")
            {
                if (curLevel < 17)
                {
                    return 0.1400f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.1225f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.1085f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.0935f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.0870f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.0770f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0650f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0584f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0528f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0470f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0426f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0367f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0322f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0292f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0258f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0230f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0203f;
                }
            }
            if ((echoType == "left_end") || (echoType == "left_end_right") || (echoType == "right_end") || (echoType == "right_end_left"))
            {
                if (curLevel < 17)
                {
                    return 0.1000f;
                }
                else if ((curLevel >= 17) && (curLevel <= 21))
                {
                    return 0.0890f;
                }
                else if ((curLevel >= 22) && (curLevel <= 26))
                {
                    return 0.0800f;
                }
                else if ((curLevel >= 27) && (curLevel <= 31))
                {
                    return 0.0710f;
                }
                else if ((curLevel >= 32) && (curLevel <= 36))
                {
                    return 0.0630f;
                }
                else if ((curLevel >= 37) && (curLevel <= 41))
                {
                    return 0.0565f;
                }
                else if ((curLevel >= 42) && (curLevel <= 46))
                {
                    return 0.0500f;
                }
                else if ((curLevel >= 47) && (curLevel <= 51))
                {
                    return 0.0445f;
                }
                else if ((curLevel >= 52) && (curLevel <= 56))
                {
                    return 0.0395f;
                }
                else if ((curLevel >= 57) && (curLevel <= 61))
                {
                    return 0.0355f;
                }
                else if ((curLevel >= 62) && (curLevel <= 66))
                {
                    return 0.0315f;
                }
                else if ((curLevel >= 67) && (curLevel <= 71))
                {
                    return 0.0280f;
                }
                else if ((curLevel >= 72) && (curLevel <= 76))
                {
                    return 0.0250f;
                }
                else if ((curLevel >= 77) && (curLevel <= 81))
                {
                    return 0.0225f;
                }
                else if ((curLevel >= 82) && (curLevel <= 86))
                {
                    return 0.0199f;
                }
                else if ((curLevel >= 87) && (curLevel <= 91))
                {
                    return 0.0178f;
                }
                else if (curLevel >= 92)
                {
                    return 0.0155f;
                }
            }
        }

        return 0.0f;
    }

    IEnumerator FinishedEchoClip(float waitTime, float[] delayTimes = null)
    {
        waitTime = delayTimes.Max();
        print("Wait Time: " + waitTime.ToString());

        yield return new WaitForSeconds(waitTime);

        finishedEcho = true;
        print("Finished echo.");
    }

    string post_act = "";
    string correct_post_act = "";

    /// <summary>
    /// Reports data when an echo is requested.
    /// The function is actually called during other actions after an echo was played.
    /// </summary>
	private void reportOnEcho()
    {
        if (PlayerPrefs.GetInt("TotalTime", 0) > 180000)
        {
            return;
        }
        string echoEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/echo.py";

        Vector2 idx_location = GameManager.instance.boardScript.get_idx_from_pos(transform.position);
        string location = "(" + idx_location.x.ToString() + "," + idx_location.y.ToString() + ")";
        correct_post_act = "";
        // manually setup, TODO: warp it into a function
        GameManager.instance.boardScript.sol = "";
        for (int i = 0; i < GameManager.instance.boardScript.searched_temp.Length; ++i)
        {
            GameManager.instance.boardScript.searched_temp[i] = false;
        }
        // correct_post_act = boardScript.getHint (idx_location,"s");
        GameManager.instance.boardScript.solveMazeMid(idx_location, "s");
        if (GameManager.instance.boardScript.sol.Length >= 2)
        {
            correct_post_act = GameManager.instance.boardScript.sol[GameManager.instance.boardScript.sol.Length - 2].ToString();
        }

        Vector3 forward = old_dir;
        Vector3 sol_dir = new Vector3();
        if (correct_post_act == "u")
        {
            sol_dir = Vector3.up;
        }
        else if (correct_post_act == "d")
        {
            sol_dir = Vector3.down;
        }
        else if (correct_post_act == "l")
        {
            sol_dir = Vector3.left;
        }
        else if (correct_post_act == "r")
        {
            sol_dir = Vector3.right;
        }

        if (correct_post_act != "")
        {
            if (forward == sol_dir)
            {
                correct_post_act = "Forward";
            }
            else if (forward == -sol_dir)
            {
                correct_post_act = "Turn Around";
            }
            else
            {
                Vector3 angle = Vector3.Cross(forward, sol_dir);
                if (angle.z > 0)
                {
                    correct_post_act = "Turn Left";
                }
                else
                {
                    correct_post_act = "Turn Right";
                }
            }
        }
        else
        {
            correct_post_act = "Exit";
        }

        WWWForm echoForm = new WWWForm();
        echoForm.AddField("userName", Utilities.encrypt(SystemInfo.deviceUniqueIdentifier));
        echoForm.AddField("currentLevel", Utilities.encrypt(curLevel.ToString()));
        echoForm.AddField("trackCount", Utilities.encrypt(GameManager.instance.boardScript.local_stats[curLevel].ToString()));
        echoForm.AddField("echoLocation", Utilities.encrypt(location));
        echoForm.AddField("postEchoAction", Utilities.encrypt(post_act));
        echoForm.AddField("correctAction", Utilities.encrypt(correct_post_act));
        echoForm.AddField("dateTimeStamp", Utilities.encrypt(System.DateTime.Now.ToString()));

        // Logging.Log(System.Text.Encoding.ASCII.GetString(echoForm.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www = new WWW(echoEndpoint, echoForm);
        StartCoroutine(Utilities.WaitForRequest(www));
    }

    void getHint()
    {
        Vector2 idx_location = GameManager.instance.boardScript.get_idx_from_pos(transform.position);
        correct_post_act = "";
        GameManager.instance.boardScript.sol = "";
        for (int i = 0; i < GameManager.instance.boardScript.searched_temp.Length; ++i)
        {
            GameManager.instance.boardScript.searched_temp[i] = false;
        }
        correct_post_act = GameManager.instance.boardScript.getHint(idx_location, "s");

        AudioClip clip;
        if (correct_post_act.Length <= 0)
        {
            // If the player should exit.
            clip = Database.hintClips[3];
            SoundManager.instance.PlayVoice(clip, true, 0.0f, 0.0f, 0.5f);
            return;
        }
        Vector3 forward = old_dir;
        Vector3 sol_dir = new Vector3();
        if (correct_post_act == "u")
        {
            sol_dir = Vector3.up;
        }
        else if (correct_post_act == "d")
        {
            sol_dir = Vector3.down;
        }
        else if (correct_post_act == "l")
        {
            sol_dir = Vector3.left;
        }
        else if (correct_post_act == "r")
        {
            sol_dir = Vector3.right;
        }

        if (forward == sol_dir)
        {
            // If the player should move forward.
            clip = Database.hintClips[0];
        }
        else if (forward == -sol_dir)
        {
            // If the player should turn around.
            clip = Database.hintClips[4];
        }
        else
        {
            Vector3 angle = Vector3.Cross(forward, sol_dir);
            if (angle.z > 0)
            {
                // If the player should turn left.
                clip = Database.hintClips[1];
            }
            else
            {
                // If the player should turn right.
                clip = Database.hintClips[2];
            }
        }

        SoundManager.instance.PlayVoice(clip, true, 0.0f, 0.0f, 0.5f);
    }

    // For legacy compatibility reasons, the transform direction does not match
    // the direction of the player sprite. This conversion method is needed
    // to find the direction of the player. FIXME.
    public Vector3 get_player_dir(string dir)
    {
        if (dir == "FRONT")
        {
            return transform.right.normalized;
        }
        else if (dir == "BACK")
        {
            return -transform.right.normalized;
        }
        else if (dir == "LEFT")
        {
            return transform.up.normalized;
        }
        else if (dir == "RIGHT")
        {
            return -transform.up.normalized;
        }

        Logging.Log("INVALID direction string", Logging.LogLevel.CRITICAL);
        return Vector3.zero;
    }

    // please call this function to rotate player
    // use this with get_player_dir("SOMETHING")
    Vector3 old_dir = new Vector3();
    /// <summary>
    /// Also a function to rotate the player. The specified direction is
    /// an absolute direction to make the player face toward, and is only
    /// valid if it corresponds to turning left or right.
    /// </summary>
	void rotateplayer(Vector3 dir)
    {
        if (dir == get_player_dir("FRONT"))
        {
            return;
        }
        else if (dir == get_player_dir("BACK"))
        {
            return;
        }
        else if (dir == get_player_dir("LEFT"))
        {
            transform.Rotate(new Vector3(0, 0, 90));
            GameManager.instance.boardScript.gamerecord += "l";
        }
        else if (dir == get_player_dir("RIGHT"))
        {
            transform.Rotate(new Vector3(0, 0, -90));
            GameManager.instance.boardScript.gamerecord += "r";
        }
    }

    //used to be called from outside
    /// <summary>
    /// Rotates the player according to the specified direction.
    /// The direction to specify assumes the player is facing to the right.
    /// </summary>
    public void rotateplayer_no_update(BoardManager.Direction dir)
    {
        if (dir == BoardManager.Direction.FRONT)
        {
            transform.Rotate(new Vector3(0, 0, 90));
        }
        else if (dir == BoardManager.Direction.BACK)
        {
            transform.Rotate(new Vector3(0, 0, -90));
        }
        else if (dir == BoardManager.Direction.LEFT)
        {
            transform.Rotate(new Vector3(0, 0, 180));
        }
        else if (dir == BoardManager.Direction.RIGHT)
        {
            transform.Rotate(new Vector3(0, 0, 0));
        }
    }

    /// <summary>
    /// A function to determine how the player should react to the given directional command.
    /// If it corresponds to moving forward or backward, the player moves.
    /// Otherwise, the player will make a turn. This sends data online if the previous action
    /// was to request an echo.
    /// </summary>
	private void calculateMove(Vector3 dir)
    {
        old_dir = get_player_dir("FRONT");

        if (dir.magnitude == 0)
        {
            return;
        }

        bool changedDir = false;
        hasMoved = false;
        // print (dir);

        if ((dir != get_player_dir("FRONT")) && (dir != get_player_dir("BACK")))
        {
            if (!GameManager.instance.boardScript.turning_lock)
            {
                changedDir = true;
                hasMoved = true;
                rotateplayer(dir);
                post_act = "Turn ";
                if (old_dir == get_player_dir("LEFT"))
                {
                    post_act += "Right";
                }
                else if (old_dir == get_player_dir("RIGHT"))
                {
                    post_act += "Left";
                }

                reportOnEcho();
                reportSent = false;

            }
            else
            {
                return;
            }
        }

        dir.Normalize();

        if (!changedDir)
        {
            if (AttemptMove<Wall>((int)dir.x, (int)dir.y))
            {
                if (dir == get_player_dir("FRONT"))
                {
                    GameManager.instance.boardScript.gamerecord += "f";
                }
                if (dir == get_player_dir("BACK"))
                {
                    GameManager.instance.boardScript.gamerecord += "b";
                }
            }
        }

        // Inform player about progress
        if (true)
        {
            GameManager.instance.boardScript.sol = "";
            for (int i = 0; i < GameManager.instance.boardScript.searched_temp.Length; ++i)
            {
                GameManager.instance.boardScript.searched_temp[i] = false;
            }

            Vector2 idx_location = GameManager.instance.boardScript.get_idx_from_pos(transform.position);
            GameManager.instance.boardScript.solveMazeMid(idx_location, "s");
            int remaining_steps = GameManager.instance.boardScript.sol.Length;
            if (remaining_steps >= 2)
            {
                remaining_steps -= 2;
            }
            int total_step = GameManager.instance.boardScript.mazeSolution.Length - 1;
            ratio = ((float)remaining_steps) / total_step;
        }
    }

    protected override bool AttemptMove<T>(int xDir, int yDir)
    {
        // Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
        bool canMove = base.AttemptMove<T>(xDir, yDir);
        numSteps += 1;
        reportSent = true;
        if (canMove == true)
        {
            hasMoved = true;
        }

        // If player could not move to that location, play the crash sound
        if (!canMove)
        {
            hasMoved = false;

            GameManager.instance.boardScript.gamerecord += "C";
            // if(!SoundManager.instance.isBusy())

            Vector2 playerPos = BoardManager.player_idx;

            // If the player is at the exit and moves forward.
            if (curLevel <= 11)
            {
                // If they swipe up here and they are facing the wall, tell them they have crashed into the wall and that they have to rotate right to progress further in the level.
                if ((curLevel == 3) && (playerPos.x == 9) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.RIGHT))
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[33] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                }
                // If they swipe up here and they are facing the wall, tell them they have crashed into the wall and that they have to rotate left to progress further in the level.
                else if ((curLevel == 5) && (playerPos.x == 1) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.LEFT))
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[34] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                }
                // If the player is on top of the exit and still in the tutorial.
                else if ((canPlayClip[(curLevel - 1), 2] == false) && (BoardManager.reachedExit == true))
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[32] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                }
                else
                {
                    if (BoardManager.gotBackToStart == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[8] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
                    }
                    else if (BoardManager.gotBackToStart == true)
                    {
                        if (BoardManager.startDir == get_player_dir("FRONT"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[18] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        if (BoardManager.startDir == get_player_dir("LEFT"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[35] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        if (BoardManager.startDir == get_player_dir("RIGHT"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[36] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        if (BoardManager.startDir == get_player_dir("BACK"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[29] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                    }                    
                }
            }
            else if (curLevel >= 12)
            {
                if (BoardManager.gotBackToStart == false)
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
                }
                else if (BoardManager.gotBackToStart == true)
                {
                    if (BoardManager.startDir == get_player_dir("FRONT"))
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[18] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                    if (BoardManager.startDir == get_player_dir("LEFT"))
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[35] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                    if (BoardManager.startDir == get_player_dir("RIGHT"))
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[36] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                    if (BoardManager.startDir == get_player_dir("BACK"))
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[29] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                }
            }
            numCrashes++; // Increment the crash count
            // Decrement the step count (as no successful step was made)
            reportOnCrash(); // send crash report

            string loc = transform.position.x.ToString() + "," + transform.position.y.ToString(); // Add the crash location details
            // TODO put those two lines back
            // string crashPos = getCrashDescription((int) transform.position.x, (int) transform.position.y);
            // loc = loc + "," + crashPos;
            if (crashLocs.Equals(""))
            {
                crashLocs = loc;
            }
            else
            {
                crashLocs = crashLocs + ";" + loc;
            }
            if (reportSent)
            {
                post_act = "Crash";
                reportOnEcho();
                reportSent = false;
            }

        }

        if (reportSent)
        {
            post_act = "Forward";
            reportOnEcho();
            reportSent = false;
        }
        // Hit allows us to reference the result of the Linecast done in Move.
        // RaycastHit2D hit;

        // GameManager.instance.playersTurn = false;
        return canMove;
    }

    /// <summary>
    /// Sends data over the internet when the player crashes into a wall.
    /// </summary>
	private void reportOnCrash()
    {
        if (PlayerPrefs.GetInt("TotalTime", 0) > 180000)
        {
            return;
        }
        string crashEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/crash.py";

        Vector2 idx_pos = GameManager.instance.boardScript.get_idx_from_pos(transform.position);
        string location = "(" + idx_pos.x.ToString() + "," + idx_pos.y.ToString() + ")";

        WWWForm crashForm = new WWWForm();
        crashForm.AddField("userName", Utilities.encrypt(SystemInfo.deviceUniqueIdentifier));
        crashForm.AddField("currentLevel", Utilities.encrypt(curLevel.ToString()));
        crashForm.AddField("trackCount", Utilities.encrypt(GameManager.instance.boardScript.local_stats[curLevel].ToString()));
        crashForm.AddField("crashNumber", Utilities.encrypt(numCrashes.ToString()));
        crashForm.AddField("crashLocation", Utilities.encrypt(location));
        crashForm.AddField("dateTimeStamp", Utilities.encrypt(System.DateTime.Now.ToString()));

        // Logging.Log(System.Text.Encoding.ASCII.GetString(crashForm.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www = new WWW(crashEndpoint, crashForm);
        StartCoroutine(Utilities.WaitForRequest(www));
    }

    /// <summary>
    ///
    /// </summary>
	public void attemptExitFromLevel()
    {
        exitAttempts++;

        if (BoardManager.reachedExit == true)
        {
            reachedExit = true;
            endLevel(); // Calculate time elapsed during the game level
        }
        else
        {
            SoundManager.instance.PlayVoice(Database.mainGameClips[31], true, 0.0f, 0.0f, 0.5f); // Tell the player that they are not currently at the exit.
        }
        post_act = "Exit";
        reportOnEcho();
        reportSent = false;
    }

    /// <summary>
    /// A function that is called when finishing the level through the exit.
    /// </summary>
	private void endLevel()
    {
        stopWatch.Stop();
        endTime = System.DateTime.Now;

        float accurateElapsed = stopWatch.ElapsedMilliseconds / 1000;
        int timeElapsed = unchecked((int)(accurateElapsed));

        // Calculate the points for the game level
        // Score based on: time taken, num crashes, steps taken, trying(num echoes played on same spot)
        // Finish in less than 15 seconds => full score
        // For every 10 seconds after 15 seconds, lose 100 points
        // For every crash, lose 150 points
        // For every step taken over the optimal steps, lose 50 points
        // Max score currently is 1500 points
        int score = 5000;
        /*
		if (timeElapsed > 15)
        {
			score = score - (((timeElapsed - 16) / 10) + 1) * 100;
		}
		*/
        // if numSteps > numOptimalSteps, then adjust score
        // Calculate optimal steps by getting start position and end position
        // and calculate the number of steps
        if (numCrashes > 0)
        {
            score = score - numCrashes * 300;
        }

        if (((numSteps - GameManager.instance.boardScript.mazeSolution.Length) + 1) > 0)
        {
            score -= 100 * ((numSteps - GameManager.instance.boardScript.mazeSolution.Length) + 1);
        }
        // Check if the score went below 0
        if (score < 1000)
        {
            score = 1000;
        }
        // Logging.Log(numSteps, Logging.LogLevel.NORMAL);
        // Logging.Log(boardScript.mazeSolution.Length - 1, Logging.LogLevel.NORMAL);
        // Logging.Log(numCrashes, Logging.LogLevel.NORMAL);

        // TODO(agotsis) understand this. Reimplement.       
        string levelDataEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/level.py";
        int temp = GameManager.instance.boardScript.local_stats[curLevel];

        int timePlayed = PlayerPrefs.GetInt("TotalTime", 0);

        if (timePlayed < 180000)
        {
            // Check if we are connected to the internet.
            CheckInternetConnection(temp, accurateElapsed, score, levelDataEndpoint);
            timePlayed = timePlayed + timeElapsed;
            PlayerPrefs.SetInt("TotalTime", timePlayed);
        }

        // display score
#if UNITY_ANDROID
        /*
		ad.clearflag();
		ad.DisplayAndroidWindow (
			"Your Score is:" + score.ToString() + ".\n" +
			"Crashed " + numCrashes.ToString() + " times.\n" +
			"Used " + numSteps.ToString() + " Steps.\n"+
			"Optimal number of steps is: " + (boardScript.mazeSolution.Length - 1).ToString() + "\n",
			AndroidDialogue.DialogueType.YESONLY);
		*/
#endif

        // Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
        Invoke("Restart", restartLevelDelay);
        // Disable the player object since level is over.
        // enabled = true;

        GameManager.instance.level += 1;
        GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
        GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
        GameMode.write_save_mode(GameManager.instance.level, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
        GameManager.instance.playersTurn = false;

        // Reset extra data.
        resetData();
    }

    /// <summary>
    /// Resets a subset of fields stored in the instance.
    /// </summary>
	private void resetData()
    {
        numCrashes = 0;
        exitAttempts = 0;
    }

    /// <summary>
    /// Loads the online terms and consent page.
    /// </summary>
    private void reportConsent(string code)
    {
        string echoEndpoint = "https://echolock.andrew.cmu.edu/cgi-bin/consent1.py";

        WWWForm echoForm = new WWWForm();
        echoForm.AddField("userName", Utilities.encrypt(SystemInfo.deviceUniqueIdentifier));
        echoForm.AddField("consentID", Utilities.encrypt(code));
        echoForm.AddField("dateTimeStamp", Utilities.encrypt(System.DateTime.Now.ToString()));

        // Logging.Log(System.Text.Encoding.ASCII.GetString(echoForm.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www = new WWW(echoEndpoint, echoForm);
        StartCoroutine(Utilities.WaitForRequest(www));

        string echoEndpoint2 = "https://echolock.andrew.cmu.edu/cgi-bin/consent2.py";

        WWWForm echoForm2 = new WWWForm();
        echoForm.AddField("userName", Utilities.encrypt(SystemInfo.deviceUniqueIdentifier));
        echoForm2.AddField("agecheck", Utilities.encrypt(question1));
        echoForm2.AddField("understandcheck", Utilities.encrypt(question2));
        echoForm2.AddField("researchcheck", Utilities.encrypt(question3));     

        // Logging.Log(System.Text.Encoding.ASCII.GetString(echoForm2.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www2 = new WWW(echoEndpoint2, echoForm2);
        StartCoroutine(Utilities.WaitForRequest(www2));
    }

    /// <summary>
    /// Sends survey data to the server.
    /// </summary>
	private void reportsurvey(string code)
    {
        string echoEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/survey1.py";

        WWWForm echoForm = new WWWForm();        
        echoForm.AddField("surveyID", Utilities.encrypt(code));
        echoForm.AddField("enjoy", Utilities.encrypt(surveyQuestion1));
        echoForm.AddField("playmore", Utilities.encrypt(surveyQuestion2));
        echoForm.AddField("easy", Utilities.encrypt(surveyQuestion3));
        echoForm.AddField("lost", Utilities.encrypt(surveyQuestion4));
        echoForm.AddField("understandecho", Utilities.encrypt(surveyQuestion5));
        echoForm.AddField("frustrating", Utilities.encrypt(surveyQuestion6));
        echoForm.AddField("tutorial", Utilities.encrypt(surveyQuestion7));
        echoForm.AddField("tutorialhelp", Utilities.encrypt(surveyQuestion8));
        echoForm.AddField("hints", Utilities.encrypt(surveyQuestion9));
        echoForm.AddField("instructions", Utilities.encrypt(surveyQuestion10));
        echoForm.AddField("controls", Utilities.encrypt(surveyQuestion11));
        echoForm.AddField("look", Utilities.encrypt(surveyQuestion12));
        echoForm.AddField("echonavigate", Utilities.encrypt(surveyQuestion13));                       
        echoForm.AddField("visuallyimpaired", Utilities.encrypt(surveyQuestion14));
        echoForm.AddField("hearingimpaired", Utilities.encrypt(surveyQuestion15));
        // the code is the first digit of device id

        // Logging.Log(System.Text.Encoding.ASCII.GetString(echoForm.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www = new WWW(echoEndpoint, echoForm);
        StartCoroutine(Utilities.WaitForRequest(www));

        string echoEndpoint2 = "http://echolock.andrew.cmu.edu/cgi-bin/survey2.py";

        WWWForm echoForm2 = new WWWForm();
        echoForm2.AddField("userName", Utilities.encrypt(SystemInfo.deviceUniqueIdentifier));
        echoForm2.AddField("surveyID", Utilities.encrypt(code));
        echoForm2.AddField("dateTimeStamp", Utilities.encrypt(System.DateTime.Now.ToString()));
        // the code is the first digit of device id

        // Logging.Log(System.Text.Encoding.ASCII.GetString(echoForm2.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www2 = new WWW(echoEndpoint2, echoForm2);
        StartCoroutine(Utilities.WaitForRequest(www2));
    }

    /// <summary>
    /// Plays an instruction voice related to the menu.
    /// </summary>
	void play_audio()
    {
        if ((madeUnrecognizedGesture == true) && (SoundManager.instance.finishedClip == true))
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

        if (((curLevel == 1) && (finishedExitingInstruction == true) && (BoardManager.finishedTutorialLevel1 == true)) || ((hasCheckedForConsent == true) && (hasFinishedConsentForm == false)))
        {
            if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false) && (finished_reading == false) && (finished_listening == false) && (hasFinishedConsentForm == false) && (hasStartedConsent == false))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    if (GM_title.isUsingTalkback == true)
                    {
                        if ((curLevel == 1) && (finishedExitingInstruction == true) && (BoardManager.finishedTutorialLevel1 == true))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[9], Database.consentClips[1] };
                        }
                        else
                        {
                            clips = new List<AudioClip>() { Database.consentClips[1] };
                        }
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        if ((curLevel == 1) && (finishedExitingInstruction == true) && (BoardManager.finishedTutorialLevel1 == true))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[9], Database.consentClips[0] };
                        }
                        else
                        {
                            clips = new List<AudioClip>() { Database.consentClips[0] };
                        }
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    hasStartedConsent = true;
                    hearingConsentForm = false;
                    readingConsentForm = false;
                    noConsent = false;                    
                    can_display_window = false;
                }
            }

            if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false) && (finished_reading == false) && (finished_listening == false) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    if (GM_title.isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.consentClips[1] };
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.consentClips[0] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    hearingConsentForm = false;
                    readingConsentForm = false;
                    noConsent = false;
                    can_display_window = false;
                }
            }

            if ((hearingConsentForm == true) && (answeredQuestion1 == false) && (finished_listening == false))
            {
                if (SoundManager.instance.finishedAllClips == true)
                {
                    finished_listening = true;
                    canRepeat = true;
                    debugPlayerInfo = "Finished listening to consent form audio.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }

            if ((hearingConsentForm == true) && (answeredQuestion1 == false) && (finished_listening == true))
            {
                if (canRepeat == true)
                {
                    canRepeat = false;
                    if (GM_title.isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[4] };
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[3] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
            }

            
            if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == "yes") && (question2 == "yes") && (question3 == "yes") && (hasFinishedConsentForm == false))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    if (GM_title.isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[13], Database.levelStartClips[curLevel], Database.consentClips[14] };
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[11], Database.levelStartClips[curLevel], Database.consentClips[12] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
            }

            else if (((hearingConsentForm == true) || (readingConsentForm == true)) && ((answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true)) && ((question1 == "no") || (question2 == "no") || (question3 == "no")) && (hasFinishedConsentForm == false))
            {
                if (question1 == "no")
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[19] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                }
                else if (question2 == "no")
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[20] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                }
                else if (question3 == "no")
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[21] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                }
            }

            if ((readingConsentForm == true) && (answeredQuestion1 == false) && (answeredQuestion2 == false) && (answeredQuestion3 == false))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    if (can_display_window == false)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[9] };
                        SoundManager.instance.PlayClips(clips, null, 0, () =>
                        {
                            can_display_window = true;
                        }, 3, null, true);
                    }
                }
            }

            if ((noConsent == true) && (hasFinishedConsentForm == false))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    if (GM_title.isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[17], Database.levelStartClips[curLevel], Database.consentClips[18] };
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[15], Database.levelStartClips[curLevel], Database.consentClips[16] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
            }
        }

        if ((curLevel == 11) && (survey_activated == true) && (survey_shown == true))
        {
            if ((hearingSurvey == false) && (readingSurvey == false) && (noSurvey == false))
            {
                if (GM_title.isUsingTalkback == true)
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.surveyClips[1] };
                }
                else if (GM_title.isUsingTalkback == false)
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.surveyClips[0] };
                }
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.                
            }

            if ((readingSurvey == true) && (answeredSurveyQuestion1 == false))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    if (can_display_window_survey == false)
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[2] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            can_display_window_survey = true;
                        }, 3, null, true); // Play the appropriate clip.
                    }
                }                
            }

            if ((hearingSurvey == true) && (answeredSurveyQuestion1 == false))
            {
                if (canRepeat == true)
                {
                    canRepeat = false;
                    if (GM_title.isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[4] };
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[3] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
            }

            if (((hearingSurvey == true) || (readingSurvey == true)) && (finished_questions_survey == true) && (can_submit_survey == false))
            {
                if (canRepeat == true)
                {
                    canRepeat = false;
                    if (GM_title.isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[35] };
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[34] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
            }

            if (((hearingSurvey == true) || (readingSurvey == true)) && (finished_questions_survey == true) && (can_submit_survey == true))
            {
                if (canRepeat == true)
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[38] };                
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
            }

            if ((noSurvey == true) && (can_submit_survey == false))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    if (GM_title.isUsingTalkback == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[37] };
                    }
                    else if (GM_title.isUsingTalkback == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[36] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                } 
            }

            if ((noSurvey == true) && (can_submit_survey == true))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[39] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
            }
        }

        Vector2 playerPos = BoardManager.player_idx;
        Vector2 startPos = BoardManager.start_idx;
        Vector2 exitPos = BoardManager.exit_idx;

        if (hasFinishedConsentForm == true)
        {            
            if ((BoardManager.gotBackToStart == true) && (tapped == true))            
            {
                tapped = false;
                finishedEcho = false;
                StartCoroutine(DelayedPlayEcho(0.25f));                             
            }

            if ((BoardManager.gotBackToStart == true) && (tapped == false) && (swipedUp == false) && (finishedEcho == true))
            {
                finishedEcho = false;
                if (BoardManager.startDir == get_player_dir("FRONT"))
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[18] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
                if (BoardManager.startDir == get_player_dir("LEFT"))
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[35] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
                if (BoardManager.startDir == get_player_dir("RIGHT"))
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[36] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
                if (BoardManager.startDir == get_player_dir("BACK"))
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[29] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }
            }

            if ((BoardManager.gotBackToStart == false) && (BoardManager.reachedExit == false) && (tapped == true))
            {
                tapped = false;
            }

            if (swipedUp == true)
            {
                if (curLevel <= 11)
                {
                    if (hasMoved == true)
                    {
                        // If we are in level 1.
                        if (curLevel == 1)
                        {
                            // If the player has reached halfway.
                            if ((canPlayClip[0, 1] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel1 == true) && (ratio <= 0.5f) && (ratio >= 0.332f))
                            {
                                canPlayClip[0, 1] = false;
                                canPlayHalfwayClip = true;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 1.
                            else if ((canPlayClip[0, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                canPlayClip[0, 2] = false;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[0, 3] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel1 == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // Otherwise play a normal swipe up.
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                        }
                        // If we are in level 2.
                        else if (curLevel == 2)
                        {
                            // If the player has reached halfway.
                            if ((canPlayClip[1, 1] == true) && (ratio <= 0.5f) && (ratio >= 0.332f))
                            {
                                canPlayClip[1, 1] = false;
                                canPlayHalfwayClip = true;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 2.
                            else if ((canPlayClip[1, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                canPlayClip[1, 2] = false;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[1, 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // Otherwise play a normal swipe up.
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                        }
                        // If we are in level 3.
                        else if (curLevel == 3)
                        {
                            if ((canPlayClip[2, 4] == false) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 6) && (playerPos.y == 9))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // Play clip for when they approach the corner in level 3.
                            else if ((canPlayClip[2, 4] == true) && (canDoGestureTutorial == false) && (playerPos.x == 6) && (playerPos.y == 9))
                            {
                                canPlayClip[2, 4] = false;
                                finishedEcho = false;
                                canPlayApproachClip = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            else if ((canPlayClip[2, 5] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == false) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                finishedEcho = false;
                                canPlayClip[2, 5] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // Play clip for when the player reaches the corner in level 3.
                            else if ((canPlayClip[2, 5] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                canPlayClip[2, 5] = false;
                                finishedEcho = false;
                                canPlayTurnClip = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player has reached halfway.
                            else if ((canPlayClip[2, 1] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (ratio <= 0.5f) && (ratio >= 0.4665f))
                            {
                                canPlayClip[2, 1] = false;
                                canPlayHalfwayClip = true;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 3.
                            else if ((canPlayClip[2, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                canPlayClip[2, 2] = false;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player returns to their start position.
                            else if (((canPlayClip[2, 3] == true) || (canPlayClip[2, 3] == false)) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                backAtStart = true;
                                finishedEcho = false;
                            }
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[2, 1] = true;
                                }
                                if ((playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                                {
                                    canPlayClip[2, 2] = true;
                                }
                            }
                        }
                        // If we are in level 5.
                        else if (curLevel == 5)
                        {
                            if ((canPlayClip[4, 4] == false) && (playerPos.x <= 4) && (playerPos.x > 2) && (playerPos.y == 9))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // Play clip for when they approach the corner in level 5.
                            else if ((canPlayClip[4, 4] == true) && (playerPos.x == 4) && (playerPos.y == 9))
                            {
                                canPlayClip[4, 4] = false;
                                finishedEcho = false;
                                canPlayApproachClip = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player has reached the corner in level 5.
                            else if ((canPlayClip[4, 5] == true) && (playerPos.x == 1) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.LEFT))
                            {
                                canPlayClip[4, 5] = false;
                                finishedEcho = false;
                                canPlayTurnClip = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player has reached halfway.
                            else if ((canPlayClip[4, 1] == true) && (ratio <= 0.5f) && (ratio >= 0.4465f))
                            {
                                canPlayClip[4, 1] = false;                          
                                canPlayHalfwayClip = true;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 5.
                            else if ((canPlayClip[4, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                canPlayClip[4, 2] = false;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player returns to their start position.
                            else if (((canPlayClip[4, 3] == true) || (canPlayClip[4, 3] == false)) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                backAtStart = true;
                                finishedEcho = false;
                            }
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[4, 1] = true;
                                }
                                if ((playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                                {
                                    canPlayClip[4, 2] = true;
                                }
                            }
                        }
                        // If we are in level 11.
                        else if (curLevel == 11)
                        {
                            // Play clips for when player hits the T intersection in level 11.
                            if ((canPlayClip[10, 4] == true) && (playerPos.x == 5) && (playerPos.y == 6) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.FRONT))
                            {                                
                                canPlayClip[10, 4] = false;
                                canPlayClip[10, 1] = false;
                                canPlayForkClip = true;
                                canPlayHalfwayClip = true;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player has reached halfway and they have not gone into an arm of the T hallway or went back into the start hallway.
                            else if ((canPlayClip[10, 1] == false) && (ratio <= 0.5f) && (ratio >= 0.4665f) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.FRONT))
                            {
                                canPlayHalfwayClip = true;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached halfway and they have not gone into an arm of the T hallway or went back into the start hallway.
                            else if ((canPlayClip[10, 1] == false) && (ratio <= 0.5f) && (ratio >= 0.4665f) && (GameManager.instance.boardScript.get_player_dir_world() != BoardManager.Direction.FRONT))
                            {
                                canPlayHalfwayClip = true;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 11.
                            else if ((canPlayClip[10, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                canPlayClip[10, 2] = false;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player returns to their start position.
                            else if (((canPlayClip[10, 3] == true) || (canPlayClip[10, 3] == false)) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                backAtStart = true;
                                finishedEcho = false;
                            }
                            // Otherwise play a normal swipe up.
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[10, 1] = true;
                                }
                                if ((playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                                {
                                    canPlayClip[10, 2] = true;
                                }
                            }
                        }
                        // If we are in another tutorial level
                        else
                        {
                            // If the player has reached halfway.
                            if ((canPlayClip[(curLevel - 1), 1] == true) && (ratio <= 0.5f) && (ratio >= 0.4665f))
                            {
                                canPlayClip[(curLevel - 1), 1] = false;
                                canPlayHalfwayClip = true;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at another level.
                            else if ((canPlayClip[(curLevel - 1), 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                canPlayClip[(curLevel - 1), 2] = false;
                                finishedEcho = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player returns to their start position.
                            else if (((canPlayClip[(curLevel - 1), 3] == true) || (canPlayClip[(curLevel - 1), 3] == false)) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                backAtStart = true;
                                finishedEcho = false;
                            }
                            // Otherwise play a normal swipe up.
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[(curLevel - 1), 1] = true;
                                }
                                if ((playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                                {
                                    canPlayClip[(curLevel - 1), 2] = true;
                                }
                            }
                        }
                    }
                }
                else if (curLevel >= 12)
                {
                    if (hasMoved == true)
                    {
                        // If the player has reached halfway.
                        if ((canPlayClip[11, 1] == true) && (ratio <= 0.5f) && (ratio >= 0.4665f))
                        {
                            canPlayClip[11, 1] = false;
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                        }
                        // If the player has reached the exit.
                        else if ((canPlayClip[11, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                        {
                            canPlayClip[11, 2] = false;                          
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                        }
                        // If the player returns to their start position.
                        else if ((playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                        {                           
                            backAtStart = true;
                            finishedEcho = false;
                        }
                        else
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => StartCoroutine(DelayedPlayEcho(0.25f)), 1, null, true); // Play the appropriate clip.
                        }
                    }
                }
                swipedUp = false;

                if (BoardManager.gotBackToStart == false)
                {
                    debugPlayerInfo = "Swiped up. Moved player forward. XPos = " + BoardManager.player_idx.x.ToString() + ", YPos = " + BoardManager.player_idx.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }

            if (rotatedLeft == true)
            {
                // Play clips for when the player has finished the gesture tutorial at level 3 and has rotated around the corner to move towards the exit.
                if ((curLevel == 3) && (canPlayClip[2, 6] == true) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 9) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.BACK))
                {
                    if (hasMoved == true)
                    {
                        canPlayClip[2, 6] = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.mainGameClips[15] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        rotatedLeft = false;
                    }
                }

                // Play clips for when the player has rotated around the corner in level 5 to move towards the exit.
                else if ((curLevel == 5) && (canPlayClip[4, 6] == true) && (playerPos.x == 1) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.BACK))
                {
                    if (hasMoved == true)
                    {
                        canPlayClip[4, 6] = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.mainGameClips[15] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        rotatedLeft = false;
                    }
                }

                else
                {
                    if (BoardManager.gotBackToStart == true)
                    {
                        if (BoardManager.startDir == get_player_dir("FRONT"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.mainGameClips[18] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        if (BoardManager.startDir == get_player_dir("LEFT"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.mainGameClips[35] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        if (BoardManager.startDir == get_player_dir("RIGHT"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.mainGameClips[36] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        if (BoardManager.startDir == get_player_dir("BACK"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.mainGameClips[29] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }                       
                    }
                    else if (BoardManager.gotBackToStart == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[5] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }                   
                    rotatedLeft = false;
                    debugPlayerInfo = "Rotated left. Rotated player left 90 degrees. XPos = " + BoardManager.player_idx.x.ToString() + ", YPos = " + BoardManager.player_idx.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }

            if (rotatedRight == true)
            {
                // Play clips for when the player has finished the gesture tutorial at level 3 and has rotated around the corner to move towards the exit.
                if ((curLevel == 3) && (canPlayClip[2, 6] == true) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 9) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.BACK))
                {
                    if (hasMoved == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.mainGameClips[15] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        canPlayClip[2, 6] = false;
                        rotatedRight = false;
                    }
                }

                // Play clips for when the player has rotated around the corner in level 5 to move towards the exit.
                else if ((curLevel == 5) && (canPlayClip[4, 6] == true) && (playerPos.x == 1) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.BACK))
                {
                    if (hasMoved == true)
                    {
                        canPlayClip[4, 6] = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.mainGameClips[15] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        rotatedRight = false;
                    }
                }

                else if ((curLevel != 3) || ((curLevel == 3) && (canDoGestureTutorial == false)))
                {
                    if (BoardManager.gotBackToStart == true)
                    {
                        if (BoardManager.startDir == get_player_dir("FRONT"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.mainGameClips[18] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        if (BoardManager.startDir == get_player_dir("LEFT"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.mainGameClips[35] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        if (BoardManager.startDir == get_player_dir("RIGHT"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.mainGameClips[36] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        if (BoardManager.startDir == get_player_dir("BACK"))
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.mainGameClips[29] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                    }
                    else if (BoardManager.gotBackToStart == false)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[6] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }

                    rotatedRight = false;
                    debugPlayerInfo = "Rotated right. Rotated player right 90 degrees. XPos = " + BoardManager.player_idx.x.ToString() + ", YPos = " + BoardManager.player_idx.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }            
        }

        if (curLevel == 1)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 1 beginning clips.
                if ((canPlayClip[0, 0] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel1 == true))
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 1 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[1], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[0, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 1 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[0], Database.soundEffectClips[0], Database.mainGameClips[1], Database.soundEffectClips[0], Database.mainGameClips[2], Database.soundEffectClips[0], Database.mainGameClips[3], Database.soundEffectClips[0], Database.mainGameClips[4], Database.soundEffectClips[0], Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[5], Database.soundEffectClips[0], Database.mainGameClips[6], Database.soundEffectClips[0], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 2)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 2 beginning clips.
                if (canPlayClip[1, 0] == true)
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 2 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[2], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[1, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 2 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[9], Database.soundEffectClips[0], Database.mainGameClips[3], Database.soundEffectClips[0], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 3)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 3 beginning clips.
                if ((canPlayClip[2, 0] == true) && (canDoGestureTutorial == false))
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 3 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[3], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[2, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 3 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[10], Database.soundEffectClips[0], Database.mainGameClips[11], Database.soundEffectClips[0], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 4)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 4 beginning clips.
                if (canPlayClip[3, 0] == true)
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 4 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[4], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[3, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 4 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[17], Database.soundEffectClips[0], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 5)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 5 beginning clips.
                if (canPlayClip[4, 0] == true)
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 5 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[5], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[4, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 5 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[19], Database.soundEffectClips[0], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 6)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 6 beginning clips.
                if (canPlayClip[5, 0] == true)
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 6 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[6], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[5, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 6 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[21], Database.soundEffectClips[0], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 7)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 7 beginning clips.
                if (canPlayClip[6, 0] == true)
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 7 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[7], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[6, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 7 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[22], Database.soundEffectClips[0], Database.mainGameClips[23], Database.soundEffectClips[0], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 8)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 8 beginning clips.
                if (canPlayClip[7, 0] == true)
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 8 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[8], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[7, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 8 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 9)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 9 beginning clips.
                if (canPlayClip[8, 0] == true)
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 9 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[9], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[8, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 9 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 10)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 10 beginning clips.
                if (canPlayClip[9, 0] == true)
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 10 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[10], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[9, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 10 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel == 11)
        {
            if ((at_pause_menu == false) || (want_exit == false))
            {
                // Play level 11 beginning clips.
                if (canPlayClip[10, 0] == true)
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 11 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[11], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            canCheckForConsent = true;
                        }, 2, null, true); // Play the appropriate clips.
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[10, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 11 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[24], Database.soundEffectClips[0], Database.mainGameClips[25], Database.soundEffectClips[0], Database.mainGameClips[26], Database.soundEffectClips[0], Database.mainGameClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
                {
                    endingLevel = false;
                    debugPlayerInfo = "Swiped down. Attempting to exit level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }
        }
        else if (curLevel >= 12)
        {
            // Play level 12+ beginning clips.
            if (canPlayClip[11, 0] == true)
            {
                if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                {
                    canRepeat = false;
                    debugPlayerInfo = "Playing level " + curLevel + " beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[curLevel], Database.levelStartClips[0] };
                    SoundManager.instance.PlayClips(clips, null, 0, () => {
                        canCheckForConsent = true;
                    }, 2, null, true); // Play the appropriate clips.
                }
                else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                {
                    canPlayClip[11, 0] = false;
                    hasCheckedForConsent = false;
                }
            }
            // If the player is not at the exit and swiped down.
            if ((endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
            {
                endingLevel = false;
                debugPlayerInfo = "Swiped down. Attempting to exit level.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                attemptExitFromLevel(); // Attempt to exit the level.
            }
        }
    }

    void Update()
    {
        if ((curLevel == 1) && (BoardManager.player_idx.x == 1) && (BoardManager.player_idx.y == 1) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel1 == false))
        {
            print("Can do gesture tutorial level 1");
            canDoGestureTutorial = true;

            // Play the beginning level 1 gesture tutorial clips and reset necessary variables.
            debugPlayerInfo = "Playing tutorial level 1 clips.";
            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            finishedTappingInstruction = false; // Reset if the player is going through this tutorial level again.
            finishedSwipingInstruction = false; // Reset if the player is going through this tutorial level again.
            finishedMenuInstruction = false; // Reset if the player is going through this tutorial level again.
            finishedExitingInstruction = false; // Reset if the player is going through this tutorial level again.
            if (GM_title.isUsingTalkback == true)
            {
                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.tutorialClips[0], Database.tutorialClips[2], Database.soundEffectClips[0], attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[3], Database.tutorialClips[4] };
            }
            else if (GM_title.isUsingTalkback == false)
            {
                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.tutorialClips[0], Database.tutorialClips[1], Database.soundEffectClips[0], attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[3], Database.tutorialClips[4] };
            }
            float[] volumes = new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 1.0f, 0.5f, 0.5f, 0.5f };
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, volumes); // If they are not using Talkback, play the correct instructions.
            level1_remaining_taps = 3; // Set the remaining taps for the tutorial to 3.
            level1_remaining_swipe_ups = 3; // Set the remaining swipes up for the tutorial to 3.
            level1_remaining_menus = 2; // Set the remaining holds for the tutorial to 2.
        }       

        if ((restartLevel == true) && (SoundManager.instance.finishedAllClips == true))
        {
            if (curLevel <= 11)
            {
                canPlayClip[(curLevel - 1), 1] = true;
                canPlayClip[(curLevel - 1), 2] = true;
                canPlayClip[(curLevel - 1), 3] = true;
                canPlayClip[(curLevel - 1), 4] = true;
                canPlayClip[(curLevel - 1), 5] = true;
                canPlayClip[(curLevel - 1), 6] = true;
            }
            else if (curLevel >= 12)
            {
                canPlayClip[11, 1] = true;
                canPlayClip[11, 2] = true;
                canPlayClip[11, 3] = true;
                canPlayClip[11, 4] = true;
                canPlayClip[11, 5] = true;
                canPlayClip[11, 6] = true;
            }
            restartLevel = false;
            changingLevel = true;
            loadingScene = true;
            SceneManager.LoadScene("Main"); // Restart the level.
        }

        if ((goBackToMain == true) && (SoundManager.instance.finishedAllClips == true))
        {
            goBackToMain = false;
            changingLevel = false;
            loadingScene = true;
            Destroy(GameObject.Find("GameManager"));
            SceneManager.LoadScene("Title_Screen"); // Move to the main menu.
        }

        if (BoardManager.gotBackToStart == true)
        {
            if ((curLevel <= 11) && (canPlayClip[(curLevel - 1), 3] == true))
            {
                canPlayClip[(curLevel - 1), 3] = false;
                playEchoOnce = false;
                debugPlayerInfo = "Returned to start position. Turn around by rotating twice, then continue by moving forward.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.     
            }
            else if ((curLevel >= 12) && (canPlayClip[11, 3] == true))
            {
                canPlayClip[11, 3] = false;
                playEchoOnce = false;
                debugPlayerInfo = "Returned to start position. Turn around by rotating twice, then continue by moving forward.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.            
            }

            if ((curLevel <= 11) && (canPlayClip[(curLevel - 1), 3] == false) && (playEchoOnce == false) && (backAtStart == true) && (finishedEcho == false) && (SoundManager.instance.finishedAllClips == true))
            {
                playEchoOnce = true;
                StartCoroutine(DelayedPlayEcho(0.25f));
            }
            else if ((curLevel >= 12) && (canPlayClip[11, 3] == false) && (playEchoOnce == false) && (backAtStart == true) && (finishedEcho == false) && (SoundManager.instance.finishedAllClips == true))
            {
                playEchoOnce = true;
                StartCoroutine(DelayedPlayEcho(0.25f));
            }

            if ((curLevel <= 11) && (canPlayClip[(curLevel - 1), 3] == false) && (backAtStart == true) && (finishedEcho == true))
            {
                backAtStart = false;
                debugPlayerInfo = "Playing back to start clip.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[29] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
            }
            else if ((curLevel >= 12) && (canPlayClip[11, 3] == false) && (backAtStart == true) && (finishedEcho == true))
            {
                backAtStart = false;
                debugPlayerInfo = "Playing back to start clip.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[29] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
            }
        }

        else if (BoardManager.gotBackToStart == false)
        {
            if (curLevel <= 11)
            {
                canPlayClip[(curLevel - 1), 3] = true;
                backAtStart = false;
            }
            else if (curLevel >= 12)
            {
                canPlayClip[11, 3] = true;
                backAtStart = false;
            }
        }

        if (canPlayHalfwayClip == true)
        {
            if ((ratio <= 0.5f) && (ratio >= 0.332f))
            {
                if ((curLevel == 1) && (canPlayClip[0, 1] == false) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel1 == true) && (finishedEcho == true))
                {
                    finishedEcho = false;
                    canPlayHalfwayClip = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[30] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                }

                else if ((curLevel == 2) && (canPlayClip[1, 1] == false) && (finishedEcho == true))
                {
                    finishedEcho = false;
                    canPlayHalfwayClip = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[30] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                }
            }


            if ((ratio <= 0.5f) && (ratio >= 0.4665f))
            {
                if ((curLevel == 3) && (canPlayClip[2, 1] == false) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (finishedEcho == true))
                {
                    finishedEcho = false;
                    canPlayHalfwayClip = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[30] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                }

                if ((curLevel == 11) && (canPlayClip[10, 1] == false) && (finishedEcho == true) && (canPlayForkClip == false))
                {
                    finishedEcho = false;
                    canPlayHalfwayClip = false;
                    if ((GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.FRONT))
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[30], Database.soundEffectClips[0], Database.mainGameClips[27] };
                    }
                    else if ((GameManager.instance.boardScript.get_player_dir_world() != BoardManager.Direction.FRONT))
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[30] };
                    }
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                }

                else if ((curLevel <= 11) && (canPlayClip[(curLevel - 1), 1] == false) && (finishedEcho == true))
                {
                    finishedEcho = false;
                    canPlayHalfwayClip = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[30] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                }
            }
        }

        if (canPlayApproachClip == true)
        {
            if ((curLevel == 3) && (canPlayClip[2, 4] == false) && (canDoGestureTutorial == false) && (BoardManager.player_idx.x == 6) && (BoardManager.player_idx.y == 9) && (finishedEcho == true))
            {
                finishedEcho = false;
                canPlayApproachClip = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[12] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
            }

            if ((curLevel == 5) && (canPlayClip[4, 4] == false) && (BoardManager.player_idx.x == 4) && (BoardManager.player_idx.y == 9) && (finishedEcho == true))
            {
                finishedEcho = false;
                canPlayApproachClip = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[12] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
            }
        }

        if (canPlayTurnClip == true)
        {
            if ((curLevel == 3) && (canPlayClip[2, 5] == false) && (canDoGestureTutorial == false) && (BoardManager.player_idx.x == 9) && (BoardManager.player_idx.y == 9) && (finishedEcho == true))
            {
                finishedEcho = false;
                canPlayTurnClip = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[13], Database.mainGameClips[14] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.                
            }

            if ((curLevel == 5) && (canPlayClip[4, 5] == false) && (BoardManager.player_idx.x == 1) && (BoardManager.player_idx.y == 9) && (finishedEcho == true))
            {
                finishedEcho = false;
                canPlayTurnClip = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[20] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.          
            }
        }

        if (canPlayForkClip == true)
        {
            if ((curLevel == 11) && (canPlayClip[10, 4] == false) && (BoardManager.player_idx.x == 5) && (BoardManager.player_idx.y == 6) && (finishedEcho == true))
            {
                finishedEcho = false;
                canPlayForkClip = false;
                canPlayHalfwayClip = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[30], Database.soundEffectClips[0], Database.mainGameClips[27] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
            }
        }

        if (BoardManager.reachedExit == true)
        {
            // If the player has reached the exit and not swiped down, play the appropriate exit clip.
            if ((endingLevel == false) && (playedExitClip == false))
            {
                if ((curLevel <= 11) && (canPlayClip[(curLevel - 1), 2] == false) && (tapped == true) && (finishedEcho == true))
                {
                    tapped = false;
                    finishedEcho = false;
                }
                if ((curLevel <= 11) && (canPlayClip[(curLevel - 1), 2] == false) && (tapped == false) && (finishedEcho == true))
                {
                    if (curLevel == 1)
                    {
                        finishedEcho = false;
                        debugPlayerInfo = "Playing found level 1 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[8] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null);
                    }
                    else if (curLevel == 2)
                    {
                        finishedEcho = false;
                        debugPlayerInfo = "Playing found level 2 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[8] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null);
                    }
                    else if (curLevel == 3)
                    {
                        finishedEcho = false;
                        debugPlayerInfo = "Playing found level 3 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[16] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null);
                    }
                    else if (curLevel == 4)
                    {
                        finishedEcho = false;
                        debugPlayerInfo = "Playing found level 4 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[18] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null);
                    }
                    else if (curLevel == 11)
                    {
                        finishedEcho = false;
                        debugPlayerInfo = "Playing found level 11 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.mainGameClips[28] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null);
                    }
                    else
                    {
                        finishedEcho = false;
                    }
                }                
            }

            // If the player is at the exit and has swiped down.
            if ((endingLevel == true))
            {
                loadingScene = true;
                endingLevel = false;
                playedExitClip = true;
                debugPlayerInfo = "Playing exiting level clip.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                clips = new List<AudioClip>() { Database.soundEffectClips[9] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
            }

            // If the exit level sound has finished playing.
            if ((playedExitClip == true) && (canGoToNextLevel == false))
            {
                debugPlayerInfo = "Exiting level clip has finished.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                canGoToNextLevel = true;

                if ((curLevel == 11) && (survey_activated == false) && (survey_shown == false))
                {
                    survey_activated = true;
                }
            }

            // If the player is at the exit and the exit level sound has played.
            if ((playedExitClip == true) && (canGoToNextLevel == true) && (SoundManager.instance.finishedAllClips == true))
            {
                // pop up the survey at the end of tutorial        
                if ((GameManager.instance.level == 11) && (survey_activated == true))
                {                    
                    if (survey_shown == false)
                    {
                        print("At survey");
                        survey_shown = true;
                        debugPlayerInfo = "Showing survey.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                        if (SystemInfo.deviceUniqueIdentifier.Length <= 6)
                        {
                            surveyCode = SystemInfo.deviceUniqueIdentifier;
                        }
                        else if (SystemInfo.deviceUniqueIdentifier.Length > 6)
                        {
                            surveyCode = SystemInfo.deviceUniqueIdentifier.Substring(0, 6);
                        }
                    }       
                }

                if (survey_activated == false)
                {
                    playedExitClip = false;
                    canGoToNextLevel = false;
                    debugPlayerInfo = "Swiped down. Exiting level.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                    changingLevel = true;
                    attemptExitFromLevel(); // Attempt to exit the level.
                }
            }

            // pop up the survey at the end of tutorial        
            if ((GameManager.instance.level == 11) && (survey_activated == true))
            {
                if ((survey_shown == true) && (noConsent == true))
                {
                    survey_activated = false;
                    survey_shown = false;

                    print("Does not want to do survey.");
                    debugPlayerInfo = "Does not want to do survey.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }               
                if ((survey_shown == true) && (finished_questions_survey == true))
                {
                    print("Reporting survey.");
                    debugPlayerInfo = "Reporting survey.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                    reportsurvey(surveyCode);
                    survey_activated = false;
                    survey_shown = false;
                }                    
            }
        }

        else if (BoardManager.reachedExit == false)
        {
            if (curLevel <= 11)
            {
                canPlayClip[(curLevel - 1), 2] = true;
            }
            else if (curLevel >= 12)
            {
                canPlayClip[11, 2] = true;
            }
        }

        if (canCheckForConsent == true)
        {
            canCheckForConsent = false;

            // PlayerPrefs.SetInt("Consented", -1);
            int hasConsented = PlayerPrefs.GetInt("Consented", -1);

            if (hasConsented != -1)
            {
                if (hasConsented == 1)
                {
                    debugPlayerInfo = "Previously consented to having their data collected. Can continue with level " + curLevel.ToString() + ".";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    hasFinishedConsentForm = true;
                }
                else if (hasConsented == 0)
                {
                    debugPlayerInfo = "Previously not consented to having their data collected. Can continue with level " + curLevel.ToString() + ".";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    hasFinishedConsentForm = true;
                }
            }
            else
            {
                debugPlayerInfo = "Has not gone through consent form.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                hasFinishedConsentForm = false;
                canRepeat = true;
            }

            hasCheckedForConsent = true;
        }

        play_audio();

#if (UNITY_IOS || UNITY_ANDROID) && (!UNITY_STANDALONE || !UNITY_WEBPLAYER)
        if (((curLevel == 1) && (finishedExitingInstruction == true) && (BoardManager.finishedTutorialLevel1 == true) && (hasFinishedConsentForm == false)) || ((hasCheckedForConsent == true) && (hasFinishedConsentForm == false)))
        {
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
                IOSNative.ShowOne(title, message, "Next");   
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
                    "appear. This survey will only happen once. Users will play the game for as long as they want to but " +
                    "data collection will end after 50 hours of game play.";

#if UNITY_IOS
                IOSNative.ShowTwo(title, message, "Next", "Back");
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
                IOSNative.ShowTwo(title, message, "Next", "Back");
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
                IOSNative.ShowTwo(title, message, "Next", "Back");
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
                IOSNative.ShowTwo(title, message, "Next", "Back");
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
                IOSNative.ShowTwo(title, message, "Next", "Back");
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
                IOSNative.ShowTwo(title, message, "Next", "Back");
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
                IOSNative.ShowTwo(title, message, "Next", "Back");
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
                IOSNative.ShowTwo(title, message, "Next", "Back");
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readVoluntary == false) && (voluntaryFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readVoluntary = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readVoluntary == true) && (readEighteenPlus == false) && (eighteenPlusFlag == false))
            {
                eighteenPlusFlag = true;

                string title = "Age Limitation";
                string message = "I am 18 or older.";

#if UNITY_IOS
                IOSNative.ShowTwo(title, message, "Yes", "No");
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No");
#endif
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readEighteenPlus == false) && (eighteenPlusFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readEighteenPlus = true;
                answeredQuestion1 = true;
                question1 = "yes";
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readEighteenPlus == false) && (eighteenPlusFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readEighteenPlus = true;
                answeredQuestion1 = true;
                question1 = "no";
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readEighteenPlus == true) && (readUnderstand == false) && (understandFlag == false))
            {
                understandFlag = true;

                string title = "Read Information";
                string message = "I have read and understand the information above.";

#if UNITY_IOS
                IOSNative.ShowTwo(title, message, "Yes", "No");
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No");
#endif
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readUnderstand == false) && (understandFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readUnderstand = true;
                answeredQuestion2 = true;
                question2 = "yes";
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readUnderstand == false) && (understandFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readUnderstand = true;
                answeredQuestion2 = true;
                question2 = "no";
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readUnderstand == true) && (readParticipate == false) && (participateFlag == false))
            {
                participateFlag = true;

                string title = "Participation";
                string message = "I want to participate in this research and continue with the game and survey.";

#if UNITY_IOS
                IOSNative.ShowTwo(title, message, "Yes", "No");
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.NORMAL;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Next", "Back");
#endif
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readParticipate == false) && (participateFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readParticipate = true;
                answeredQuestion3 = true;
                question3 = "yes";
                android_window_displayed = false;
                can_display_window = false;
                finished_reading = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, () => {
                    canRepeat = true;
                    if (SystemInfo.deviceUniqueIdentifier.Length <= 6)
                    {
                        consentCode = SystemInfo.deviceUniqueIdentifier;
                    }
                    else if (SystemInfo.deviceUniqueIdentifier.Length > 6)
                    {
                        consentCode = SystemInfo.deviceUniqueIdentifier.Substring(0, 6);
                    }
                    reportConsent(consentCode);                   
                }, 1, null, true);
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readParticipate == false) && (participateFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readParticipate = true;
                answeredQuestion3 = true;
                question3 = "no";
                android_window_displayed = false;
                can_display_window = false;
                finished_reading = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, () => {
                    canRepeat = true;
                }, 1, null, true);
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }
        }

        if ((curLevel == 11) && (survey_activated == true))
        {
            if ((readingSurvey == true) && (android_window_displayed == false) && (can_display_window_survey == true))
            {
                android_window_displayed = true;
                finished_reading_survey = false;
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEnjoy == false) && (enjoyFlag == false))
            {
                enjoyFlag = true;

                string title = "Question 1";
                string message = "Are you enjoying the game?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType);
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEnjoy == false) && (enjoyFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readEnjoy = true;
                surveyQuestion1 = "yes";
                answeredSurveyQuestion1 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEnjoy == false) && (enjoyFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readEnjoy = true;
                surveyQuestion1 = "no";
                answeredSurveyQuestion1 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEnjoy == false) && (enjoyFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readEnjoy = true;
                surveyQuestion1 = "na";
                answeredSurveyQuestion1 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEnjoy == true) && (readPlayMore == false) && (playMoreFlag == false))
            {
                playMoreFlag = true;

                string title = "Question 2";
                string message = "Do you plan to play this game more?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readPlayMore == false) && (playMoreFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readPlayMore = true;
                surveyQuestion2 = "yes";
                answeredSurveyQuestion2 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readPlayMore == false) && (playMoreFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readPlayMore = true;
                surveyQuestion2 = "no";
                answeredSurveyQuestion2 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readPlayMore == false) && (playMoreFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readPlayMore = true;
                surveyQuestion2 = "na";
                answeredSurveyQuestion2 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readPlayMore == true) && (readEasy == false) && (easyFlag == false))
            {
                easyFlag = true;

                string title = "Question 3";
                string message = "Did you find this game easy?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEasy == false) && (easyFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readEasy = true;
                surveyQuestion3 = "yes";
                answeredSurveyQuestion3 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEasy == false) && (easyFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readEasy = true;
                surveyQuestion3 = "no";
                answeredSurveyQuestion3 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEasy == false) && (easyFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readEasy = true;
                surveyQuestion3 = "na";
                answeredSurveyQuestion3 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEasy == true) && (readLost == false) && (lostFlag == false))
            {
                lostFlag = true;

                string title = "Question 4";
                string message = "Did you get lost often?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readLost == false) && (lostFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readLost = true;
                surveyQuestion4 = "yes";
                answeredSurveyQuestion4 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readLost == false) && (lostFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readLost = true;
                surveyQuestion4 = "no";
                answeredSurveyQuestion4 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readLost == false) && (lostFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readLost = true;
                surveyQuestion4 = "na";
                answeredSurveyQuestion4 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readLost == true) && (readUnderstandEcho == false) && (understandEchoFlag == false))
            {
                understandEchoFlag = true;

                string title = "Question 5";
                string message = "Did you understand what the echoes were telling you about the maze?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readUnderstandEcho == false) && (understandEchoFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readUnderstandEcho = true;
                surveyQuestion5 = "yes";
                answeredSurveyQuestion5 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readUnderstandEcho == false) && (understandEchoFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readUnderstandEcho = true;
                surveyQuestion5 = "no";
                answeredSurveyQuestion5 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readUnderstandEcho == false) && (understandEchoFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readUnderstandEcho = true;
                surveyQuestion5 = "na";
                answeredSurveyQuestion5 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readUnderstandEcho == true) && (readFrustrating == false) && (frustratingFlag == false))
            {
                frustratingFlag = true;

                string title = "Question 6";
                string message = "Did you find this game frustrating?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readFrustrating == false) && (frustratingFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readFrustrating = true;
                surveyQuestion6 = "yes";
                answeredSurveyQuestion6 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readFrustrating == false) && (frustratingFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readFrustrating = true;
                surveyQuestion6 = "no";
                answeredSurveyQuestion6 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readFrustrating == false) && (frustratingFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readFrustrating = true;
                surveyQuestion6 = "na";
                answeredSurveyQuestion6 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readFrustrating == true) && (readTutorial == false) && (tutorialFlag == false))
            {
                tutorialFlag = true;

                string title = "Question 7";
                string message = "Did you start with the tutorial?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readTutorial == false) && (tutorialFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readTutorial = true;
                surveyQuestion7 = "yes";
                answeredSurveyQuestion7 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readTutorial == false) && (tutorialFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readTutorial = true;
                surveyQuestion7 = "no";
                answeredSurveyQuestion7 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readTutorial == false) && (tutorialFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readTutorial = true;
                surveyQuestion7 = "na";
                answeredSurveyQuestion7 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readTutorial == true) && (readTutorialHelp == false) && (tutorialHelpFlag == false))
            {
                tutorialHelpFlag = true;

                string title = "Question 8";
                string message = "Was the tutorial helpful?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readTutorialHelp == false) && (tutorialHelpFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readTutorialHelp = true;
                surveyQuestion8 = "yes";
                answeredSurveyQuestion8 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readTutorialHelp == false) && (tutorialHelpFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readTutorialHelp = true;
                surveyQuestion8 = "no";
                answeredSurveyQuestion8 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readTutorialHelp == false) && (tutorialHelpFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readTutorialHelp = true;
                surveyQuestion8 = "na";
                answeredSurveyQuestion8 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readTutorialHelp == true) && (readHints == false) && (hintsFlag == false))
            {
                hintsFlag = true;

                string title = "Question 9";
                string message = "Was the hint menu helpful?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readHints == false) && (hintsFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readHints = true;
                surveyQuestion9 = "yes";
                answeredSurveyQuestion9 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readHints == false) && (hintsFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readHints = true;
                surveyQuestion9 = "no";
                answeredSurveyQuestion9 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readHints == false) && (hintsFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readHints = true;
                surveyQuestion9 = "na";
                answeredSurveyQuestion9 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readHints == true) && (readInstructions == false) && (instructionsFlag == false))
            {
                instructionsFlag = true;

                string title = "Question 10";
                string message = "Were the voice instructions helpful?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readInstructions == false) && (instructionsFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readInstructions = true;
                surveyQuestion10 = "yes";
                answeredSurveyQuestion10 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readInstructions == false) && (instructionsFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readInstructions = true;
                surveyQuestion10 = "no";
                answeredSurveyQuestion10 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readInstructions == false) && (instructionsFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readInstructions = true;
                surveyQuestion10 = "na";
                answeredSurveyQuestion10 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readInstructions == true) && (readControls == false) && (controlsFlag == false))
            {
                controlsFlag = true;

                string title = "Question 11";
                string message = "Were the control gestures easy to learn and use?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readControls == false) && (controlsFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readControls = true;
                surveyQuestion11 = "yes";
                answeredSurveyQuestion11 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readControls == false) && (controlsFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readControls = true;
                surveyQuestion11 = "no";
                answeredSurveyQuestion11 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readControls == false) && (controlsFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readControls = true;
                surveyQuestion11 = "na";
                answeredSurveyQuestion11 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readControls == true) && (readLook == false) && (lookFlag == false))
            {
                lookFlag = true;

                string title = "Question 12";
                string message = "Did you ever look at the maze to figure out where to go?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readLook == false) && (lookFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readLook = true;
                surveyQuestion12 = "yes";
                answeredSurveyQuestion12 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readLook == false) && (lookFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readLook = true;
                surveyQuestion12 = "no";
                answeredSurveyQuestion12 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readLook == false) && (lookFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readLook = true;
                surveyQuestion12 = "na";
                answeredSurveyQuestion12 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readLook == true) && (readEchoNavigate == false) && (echoNavigateFlag == false))
            {
                echoNavigateFlag = true;

                string title = "Question 13";
                string message = "Have you ever used echoes to navigate before, either in a game or real life?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEchoNavigate == false) && (echoNavigateFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readEchoNavigate = true;
                surveyQuestion13 = "yes";
                answeredSurveyQuestion13 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEchoNavigate == false) && (echoNavigateFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readEchoNavigate = true;
                surveyQuestion13 = "no";
                answeredSurveyQuestion13 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEchoNavigate == false) && (echoNavigateFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readEchoNavigate = true;
                surveyQuestion13 = "na";
                answeredSurveyQuestion13 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readEchoNavigate == true) && (readVisuallyImpaired == false) && (visuallyImpairedFlag == false))
            {
                visuallyImpairedFlag = true;

                string title = "Question 14";
                string message = "Do you have a visual impairment that is not fully corrected by glasses?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readVisuallyImpaired == false) && (visuallyImpairedFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readVisuallyImpaired = true;
                surveyQuestion14 = "yes";
                answeredSurveyQuestion14 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readVisuallyImpaired == false) && (visuallyImpairedFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readVisuallyImpaired = true;
                surveyQuestion14 = "no";
                answeredSurveyQuestion14 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readVisuallyImpaired == false) && (visuallyImpairedFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readVisuallyImpaired = true;
                surveyQuestion14 = "na";
                answeredSurveyQuestion14 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readVisuallyImpaired == true) && (readHearingImpaired == false) && (hearingImpairedFlag == false))
            {
                hearingImpairedFlag = true;

                string title = "Question 15";
                string message = "Do you have a hearing impairment?";

#if UNITY_IOS
                IOSNative.ShowThree(title, message, "Yes", "No", "N/A");   
#endif
#if UNITY_ANDROID
                AndroidDialogue.DialogueType dialogueType = AndroidDialogue.DialogueType.SURVEY;
                ad.DisplayAndroidWindow(title, message, dialogueType, "Yes", "No", "N/A");
#endif
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readHearingImpaired == false) && (hearingImpairedFlag == true) && (ad.yesclicked() == true || yesPressed == true))
            {
                readHearingImpaired = true;
                surveyQuestion15 = "yes";
                answeredSurveyQuestion15 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, () => {
                    canRepeat = true;
                }, 1, null, true);
#if UNITY_IOS
                yesPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
                finished_reading_survey = true;
                finished_questions_survey = true;
                android_window_displayed = false;
                can_display_window_survey = false;
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readHearingImpaired == false) && (hearingImpairedFlag == true) && (ad.noclicked() == true || noPressed == true))
            {
                readHearingImpaired = true;
                surveyQuestion15 = "no";
                answeredSurveyQuestion15 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, () => {
                    canRepeat = true;
                }, 1, null, true);
#if UNITY_IOS
                noPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
                finished_reading_survey = true;
                finished_questions_survey = true;
                android_window_displayed = false;
                can_display_window_survey = false;
            }

            if ((readingSurvey == true) && (android_window_displayed == true) && (finished_reading_survey == false) && (readHearingImpaired == false) && (hearingImpairedFlag == true) && (ad.naclicked() == true || naPressed == true))
            {
                readHearingImpaired = true;
                surveyQuestion15 = "na";
                answeredSurveyQuestion15 = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[33] };
                SoundManager.instance.PlayClips(clips, null, 0, () => {
                    canRepeat = true;
                }, 1, null, true);
#if UNITY_IOS
                naPressed = false;
#endif
#if UNITY_ANDROID
                ad.clearflag();
#endif
                finished_reading_survey = true;
                finished_questions_survey = true;
                android_window_displayed = false;
                can_display_window_survey = false;
            }
        }
#endif

        if ((curLevel == 3) && (BoardManager.finishedTutorialLevel3 == false) && (canDoGestureTutorial == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false) && (BoardManager.player_idx.x == 9) && (BoardManager.player_idx.y == 9) && (finishedEcho == true))
        {
            hasTappedAtCorner = true;
            finishedEcho = false;

            print("Can do gesture tutorial level 3");
            canDoGestureTutorial = true;

            // Play the first level 3 gesture tutorial clip and reset necessary variables.
            debugPlayerInfo = "Playing tutorial level 3 clips.";
            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            hasStartedTurningInstruction = false;
            finishedTurningInstruction = false; // Reset if the player is going through this tutorial level again.             
            level3_remaining_turns = 4; // Set the remaining rotations for the tutorial to 4.
        }

        if ((curLevel == 3) && (BoardManager.finishedTutorialLevel3 == false) && (canDoGestureTutorial == false) && (finishedCornerInstruction == false) && (hasTappedAtCorner == false) && (BoardManager.player_idx.x == 9) && (BoardManager.player_idx.y == 9) && (finishedEcho == true))
        {
            finishedEcho = false;

            if (GM_title.isUsingTalkback == true)
            {
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[23] };
            }
            else if (GM_title.isUsingTalkback == false)
            {
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[22] };
            }
            SoundManager.instance.PlayClips(clips, null, 0, () =>
            {
                debugPlayerInfo = "Finished corner instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedCornerInstruction = true; // We have finished the corner instruction, so the player can tap.             
            }, 2, null, true); // Play the appropriate clip.                        
        }


        if ((curLevel == 3) && (BoardManager.finishedTutorialLevel3 == false) && (hasTappedAtCorner == true) && (hasStartedTurningInstruction == false) && (finishedTurningInstruction == false) && (level3_remaining_turns == 4))
        {
            if (GM_title.isUsingTalkback == true)
            {
                print("Starting turning instruction.");
                hasStartedTurningInstruction = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[25], Database.soundEffectClips[0], Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[26], Database.tutorialClips[27] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.                
            }
            else if (GM_title.isUsingTalkback == false)
            {
                print("Starting turning instruction.");
                hasStartedTurningInstruction = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[24], Database.soundEffectClips[0], Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[26], Database.tutorialClips[27] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are not using Talkback, play the correct instructions.                
            }
        }

        if ((curLevel == 3) && (BoardManager.finishedTutorialLevel3 == false) && (hasTappedAtCorner == true) && (hasStartedTurningInstruction == true) && (finishedTurningInstruction == false) && (level3_remaining_turns == 4) && (SoundManager.instance.finishedAllClips == true))
        {
            print("Finished turning instruction.");
            debugPlayerInfo = "Finished turning instruction.";
            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.             
            hasStartedTurningInstruction = false;
            finishedTurningInstruction = true; // We have finished the turning instruction, so the player can rotate.
        }

        if (canDoGestureTutorial == true)
        {
            // Make sure all the clips have finished playing before allowing the player to tap.
            if ((finishedTappingInstruction == false) && (level1_remaining_taps == 3) && (SoundManager.instance.finishedAllClips == true))
            {
                debugPlayerInfo = "Finished tapping instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedTappingInstruction = true; // We have finished the tapping instruction, so the player can tap.
            }
            if ((finishedSwipingInstruction == false) && (level1_remaining_taps == 0) && (level1_remaining_swipe_ups == 3) && (SoundManager.instance.finishedAllClips == true) && (haveTappedThreeTimes == true))
            {
                debugPlayerInfo = "Finished swiping instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedSwipingInstruction = true; // We have finished the swiping instruction, so the player can swipe up.
            }
            // Make sure all the clips have finished playing before allowing the player to swipe.
            if ((finishedSwipingInstruction == false) && (level1_remaining_taps == 0) && (level1_remaining_swipe_ups == 3) && (finishedEcho == true) && (haveTappedThreeTimes == false))
            {
                finishedEcho = false;
            }
            // Make sure all the clips have finished playing before allowing the player to open the pause menu.
            if ((finishedMenuInstruction == false) && (level1_remaining_swipe_ups == 0) && (level1_remaining_menus == 2) && (SoundManager.instance.finishedAllClips == true) && (haveSwipedThreeTimes == true))
            {
                debugPlayerInfo = "Finished open pause menu instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedMenuInstruction = true; // We have finished the first pause menu instruction, so the player can open the pause menu.
            }
            // Make sure the clip has finished playing before allowing the player to close the pause menu.
            if ((finishedMenuInstruction == false) && (level1_remaining_menus == 1) && (waitingForOpenMenuInstruction == true) && (SoundManager.instance.voiceSource.isPlaying == false))
            {
                // We have finished the second pause menu instruction, so the player can close the pause menu.
                debugPlayerInfo = "Finished close pause menu instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                waitingForOpenMenuInstruction = false;
                finishedMenuInstruction = true;
            }
            // Make sure all the clips have finished playing before allowing the player to swipe down.
            if ((finishedExitingInstruction == false) && (level1_remaining_menus == 0) && (SoundManager.instance.finishedAllClips == true))
            {
                debugPlayerInfo = "Finished exiting instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedExitingInstruction = true; // We have finished the exiting instruction, so the player can swipe down.
            }                       
        }

        // If it's not the player's turn, exit the function.
        if (!GameManager.instance.playersTurn)
        {
            return;
        }

        if (!localRecordWritten)
        {
            //update stats
            GameManager.instance.boardScript.local_stats[curLevel] += 1;
            GameManager.instance.boardScript.write_local_stats();
            localRecordWritten = true;
        }

        if (((hasFinishedConsentForm == true) && (hasStartedConsent == true) && (SoundManager.instance.finishedAllClips == true)) || ((hasFinishedConsentForm == true) && (hasStartedConsent == false)))
        {
            canPlayLevel = true;
        }

        if (canPlayLevel == true)
        {
            canMakeGestures = true;
        }

        Vector3 dir = Vector3.zero;
        // Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        // Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction.
        if (eh.isActivate() && ((hasFinishedConsentForm == false) || ((hasFinishedConsentForm == true) && (canMakeGestures == true))))
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

            Vector2 playerPos = BoardManager.player_idx;

            // Do something based on this event info.
            // If a tap was registered.
            if (ie.isTap == true)
            {
                // If the player has not finished the tapping part of the tutorial.
                if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true))
                {
                    if (level1_remaining_taps > 0)
                    {
                        debugPlayerInfo = "Tapped for gesture tutorial. Played echo.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level1_remaining_taps--; // Decrease the amount of taps left to do.           

                        if (level1_remaining_taps == 2)
                        {
                            clips = new List<AudioClip> { Database.attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[5] };
                            float[] volumes = new float[] { 1.0f, 0.5f, 0.5f };
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                finishedEcho = true;
                            }, 1, volumes, true); // This tap was correct. Please tap 2 more times.
                        }
                        else if (level1_remaining_taps == 1)
                        {
                            clips = new List<AudioClip> { Database.attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[6] };
                            float[] volumes = new float[] { 1.0f, 0.5f, 0.5f };
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                finishedEcho = true;
                            }, 1, volumes, true); // This tap was correct. Please tap 2 more times.              
                        }
                        // If the player has finished the tapping section.
                        else if (level1_remaining_taps == 0)
                        {
                            debugPlayerInfo = "Finished tapping section for gesture tutorial.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip> { Database.attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[7], Database.tutorialClips[9], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
                                float[] volumes = new float[] { 1.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    finishedEcho = true;
                                    haveTappedThreeTimes = true;
                                }, 1, volumes, true); // If they are not using Talkback, play the correct instructions.                  
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip> { Database.attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[7], Database.tutorialClips[8], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
                                float[] volumes = new float[] { 1.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    finishedEcho = true;
                                    haveTappedThreeTimes = true;
                                }, 1, volumes, true); // If they are not using Talkback, play the correct instructions.
                            }
                        }
                    }
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                {
                    // If the player is not in the pause menu, play an echo.
                    if ((want_exit == false) && (loadingScene == false))
                    {
                        if ((curLevel == 3) && (finishedCornerInstruction == false) && (playerPos.x == 9) && (playerPos.y == 9))
                        {
                            debugPlayerInfo = "Please wait for the instructions to finish.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            finishedEcho = false;

                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[23] };
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[22] };
                            }
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                debugPlayerInfo = "Finished corner instruction.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                finishedCornerInstruction = true; // We have finished the corner instruction, so the player can tap.             
                            }, 2, null, true); // Play the appropriate clip.               
                        }
                        else
                        {
                            if ((finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                            {
                                debugPlayerInfo = "Tapped at corner. Played echo.";
                            }
                            else
                            {
                                debugPlayerInfo = "Tap registered. Played echo.";
                            }
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            GameManager.instance.boardScript.gamerecord += "E{"; // Record the echo.                                         
                            if (BoardManager.gotBackToStart == false)
                            {
                                StartCoroutine(DelayedPlayEcho(0.25f)); // Play the echo.
                            }
                            GameManager.instance.boardScript.gamerecord += lastEcho;
                            GameManager.instance.boardScript.gamerecord += "}";
                            echoNum += 1;
                            tapped = true;
                        }
                    }
                    else if ((want_exit == true) && (loadingScene == false))
                    {
                        // If the player has told us they want to restart the level, then restart the level.
                        if (wantLevelRestart == true)
                        {
                            debugPlayerInfo = "Tap registered. Restarting current level.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                            clips = new List<AudioClip>() { Database.pauseMenuClips[13] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // should have another set of sound effect
                            GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                            GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                            GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
                            restartLevel = true;
                        }

                        // If the player has told us they want to return to the main menu, then return to the main menu.
                        else if (wantMainMenu == true)
                        {
                            debugPlayerInfo = "Tap registered. Moving to main menu.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.pauseMenuClips[13] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // should have another set of sound effect
                            GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                            GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                            GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
                            goBackToMain = true;
                        }
                    }
                }

                if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                {
                    if (noConsent == true)
                    {
                        PlayerPrefs.SetInt("Consented", 0);
                        debugPlayerInfo = "Tap registered. Did not consent to having data collected. Can continue with level " + curLevel.ToString() + ".";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        hasFinishedConsentForm = true;
                        canPlayLevel = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[23], Database.levelStartClips[curLevel], Database.consentClips[24], Database.levelStartClips[curLevel] };
                        SoundManager.instance.PlayClips(clips, null, 0, () =>
                        {
                            hasCheckedForConsent = true;
                            hasStartedConsent = false;
                            canRepeat = true;
                            if (curLevel == 1)
                            {
                                BoardManager.finishedTutorialLevel1 = true;
                                canDoGestureTutorial = false;
                            }
                            canPlayLevel = true;
                        }, 6, null, true);
                    }

                    else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == "yes") && (question2 == "yes") && (question3 == "yes"))
                    {
                        PlayerPrefs.SetInt("Consented", 1);
                        debugPlayerInfo = "Tap registered. Consented to having data collected. Can continue with level " + curLevel.ToString() + ".";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                      
                        hasFinishedConsentForm = true;
                        canPlayLevel = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[22], Database.levelStartClips[curLevel], Database.consentClips[24], Database.levelStartClips[curLevel] };
                        SoundManager.instance.PlayClips(clips, null, 0, () =>
                        {
                            hasCheckedForConsent = true;
                            hasStartedConsent = false;
                            canRepeat = true;
                            if (curLevel == 1)
                            {
                                BoardManager.finishedTutorialLevel1 = true;
                                canDoGestureTutorial = false;
                            }
                            canPlayLevel = true;
                        }, 6, null, true);
                    }

                    else
                    {
                        debugPlayerInfo = "Tap registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }

                if ((GameManager.instance.level == 11) && (survey_activated == true) && (survey_shown == true))
                {
                    if ((hearingSurvey == true) && (finished_questions_survey == true))
                    {
                        hearingSurvey = false;
                        survey_activated = false;                        
                        reportsurvey(surveyCode);
                    }

                    if ((readingSurvey == true) && (finished_questions_survey == true))
                    {
                        readingSurvey = false;
                        survey_activated = false;
                        reportsurvey(surveyCode);
                    }

                    if (noConsent == true)
                    {
                        noConsent = false;
                        survey_activated = false;
                        reportsurvey(surveyCode);
                    }
                }
            }

            // If a swipe was registered.
            else if (ie.isSwipe == true)
            {
                // If the left arrow key has been pressed.
                if (ie.isLeft == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is in the pause menu, they have told us they want to restart the level.
                        if ((want_exit == true) && (loadingScene == false))
                        {
                            debugPlayerInfo = "Swiped left. We want to restart the level.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[6] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[5] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            wantLevelRestart = true;
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        if ((noConsent == true) || (((readingConsentForm == true) || (hearingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == "no") || (question2 == "no") || (question3 == "no"))))
                        {
                            hearingConsentForm = true;
                            readingConsentForm = false;
                            noConsent = false;
                            hasCheckedForConsent = true;
                            hasFinishedConsentForm = false;
                            finished_listening = false;
                            finished_reading = false;
                            android_window_displayed = false;
                            can_display_window = false;
                            finished_questions = false;
                            answeredQuestion1 = false;
                            answeredQuestion2 = false;
                            answeredQuestion3 = false;
                            question1 = "";
                            question2 = "";
                            question3 = "";
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
                            readEighteenPlus = false;
                            eighteenPlusFlag = false;
                            readUnderstand = false;
                            understandFlag = false;
                            readParticipate = false;
                            participateFlag = false;
                            canRepeat = true;

                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                        {
                            question3 = "no";
                            answeredQuestion3 = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe left registered. Does not want to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                        {
                            question2 = "no";
                            answeredQuestion2 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Did not understand information.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            if ((answeredQuestion3 == false) && (finished_listening == true))
                            {
                                canRepeat = false;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                        {
                            question1 = "no";
                            answeredQuestion1 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Is not eighteen.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            if ((answeredQuestion2 == false) && (finished_listening == true))
                            {
                                canRepeat = false;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                        {
                            hearingConsentForm = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Hearing consent form through audio instructions.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                    }

                    if ((GameManager.instance.level == 11) && (survey_activated == true) && (survey_shown == true))
                    {
                        if (hearingSurvey == true)
                        {
                            if ((answeredSurveyQuestion15 == false) && (answeredSurveyQuestion14 == true) && (surveyQuestion14 != ""))
                            {
                                surveyQuestion15 = "no";
                                answeredSurveyQuestion15 = true;
                                finished_listening_survey = true;
                                finished_questions_survey = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    canRepeat = true;
                                }, 0, null, true);
                            }
                            if ((answeredSurveyQuestion14 == false) && (answeredSurveyQuestion13 == true) && (surveyQuestion13 != ""))
                            {
                                surveyQuestion14 = "no";
                                answeredSurveyQuestion14 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[32] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[31] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion13 == false) && (answeredSurveyQuestion12 == true) && (surveyQuestion12 != ""))
                            {
                                surveyQuestion13 = "no";
                                answeredSurveyQuestion13 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[30] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[29] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion12 == false) && (answeredSurveyQuestion11 == true) && (surveyQuestion11 != ""))
                            {
                                surveyQuestion12 = "no";
                                answeredSurveyQuestion12 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[28] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[27] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion11 == false) && (answeredSurveyQuestion10 == true) && (surveyQuestion10 != ""))
                            {
                                surveyQuestion11 = "no";
                                answeredSurveyQuestion11 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[26] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[25] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion10 == false) && (answeredSurveyQuestion9 == true) && (surveyQuestion9 != ""))
                            {
                                surveyQuestion10 = "no";
                                answeredSurveyQuestion10 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[24] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[23] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion9 == false) && (answeredSurveyQuestion8 == true) && (surveyQuestion8 != ""))
                            {
                                surveyQuestion9 = "no";
                                answeredSurveyQuestion9 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[22] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[21] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion8 == false) && (answeredSurveyQuestion7 == true) && (surveyQuestion7 != ""))
                            {
                                surveyQuestion8 = "no";
                                answeredSurveyQuestion8 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[20] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[19] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion7 == false) && (answeredSurveyQuestion6 == true) && (surveyQuestion6 != ""))
                            {
                                surveyQuestion7 = "no";
                                answeredSurveyQuestion7 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[18] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[17] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion6 == false) && (answeredSurveyQuestion5 == true) && (surveyQuestion5 != ""))
                            {
                                surveyQuestion6 = "no";
                                answeredSurveyQuestion6 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[16] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[15] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion5 == false) && (answeredSurveyQuestion4 == true) && (surveyQuestion4 != ""))
                            {
                                surveyQuestion5 = "no";
                                answeredSurveyQuestion5 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[14] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[13] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion4 == false) && (answeredSurveyQuestion3 == true) && (surveyQuestion3 != ""))
                            {
                                surveyQuestion4 = "no";
                                answeredSurveyQuestion4 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[12] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[11] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion3 == false) && (answeredSurveyQuestion2 == true) && (surveyQuestion2 != ""))
                            {
                                surveyQuestion3 = "no";
                                answeredSurveyQuestion3 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[10] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[9] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion2 == false) && (answeredSurveyQuestion1 == true) && (surveyQuestion1 != ""))
                            {
                                surveyQuestion2 = "no";
                                answeredSurveyQuestion2 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if (answeredSurveyQuestion1 == false)
                            {
                                surveyQuestion1 = "no";
                                answeredSurveyQuestion1 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }

                        if ((noConsent == true) || ((readingSurvey == true) && (finished_reading_survey == true)) || ((hearingSurvey == true) && (finished_listening_survey == true)) || ((hearingSurvey == false) && (readingSurvey == false) && (noSurvey == false)))
                        {
                            noConsent = false;
                            readingSurvey = false;
                            hearingSurvey = true;
                            android_window_displayed = false;
                            finished_listening_survey = false;
                            finished_reading_survey = false;
                            finished_questions_survey = false;
                            surveyQuestion1 = "";
                            answeredSurveyQuestion1 = false;
                            surveyQuestion2 = "";
                            answeredSurveyQuestion2 = false;
                            surveyQuestion3 = "";
                            answeredSurveyQuestion3 = false;
                            surveyQuestion4 = "";
                            answeredSurveyQuestion4 = false;
                            surveyQuestion5 = "";
                            answeredSurveyQuestion5 = false;
                            surveyQuestion6 = "";
                            answeredSurveyQuestion6 = false;
                            surveyQuestion7 = "";
                            answeredSurveyQuestion7 = false;
                            surveyQuestion8 = "";
                            answeredSurveyQuestion8 = false;
                            surveyQuestion9 = "";
                            answeredSurveyQuestion9 = false;
                            surveyQuestion10 = "";
                            answeredSurveyQuestion10 = false;
                            surveyQuestion11 = "";
                            answeredSurveyQuestion11 = false;
                            surveyQuestion12 = "";
                            answeredSurveyQuestion12 = false;
                            surveyQuestion13 = "";
                            answeredSurveyQuestion13 = false;
                            surveyQuestion14 = "";
                            answeredSurveyQuestion14 = false;
                            surveyQuestion15 = "";
                            answeredSurveyQuestion15 = false;
                            readEnjoy = false;
                            enjoyFlag = false;
                            readPlayMore = false;
                            playMoreFlag = false;
                            readEasy = false;
                            easyFlag = false;
                            readLost = false;
                            lostFlag = false;
                            readUnderstandEcho = false;
                            understandEchoFlag = false;
                            readFrustrating = false;
                            frustratingFlag = false;
                            readTutorial = false;
                            tutorialFlag = false;
                            readTutorialHelp = false;
                            tutorialHelpFlag = false;
                            readHints = false;
                            hintsFlag = false;
                            readInstructions = false;
                            instructionsFlag = false;
                            readControls = false;
                            controlsFlag = false;
                            readLook = false;
                            lookFlag = false;
                            readEchoNavigate = false;
                            echoNavigateFlag = false;
                            readVisuallyImpaired = false;
                            visuallyImpairedFlag = false;
                            readHearingImpaired = false;
                            hearingImpairedFlag = false;
                            canRepeat = true;
                        }
                    }
                }
                // If the right arrow key has been pressed.
                else if (ie.isRight == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is in the pause menu, they have told us they want to go back to the main menu.
                        if ((want_exit == true) && (loadingScene == false))
                        {
                            debugPlayerInfo = "Swiped right. We want to return to the main menu.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[10] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[9] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            wantMainMenu = true;
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        if ((noConsent == true) || (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == "no") || (question2 == "no") || (question3 == "no"))))
                        {
                            hearingConsentForm = false;
                            readingConsentForm = true;
                            noConsent = false;
                            hasCheckedForConsent = true;
                            hasFinishedConsentForm = false;
                            android_window_displayed = false;
                            can_display_window = false;
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
                            answeredQuestion1 = false;
                            answeredQuestion2 = false;
                            answeredQuestion3 = false;
                            question1 = "";
                            question2 = "";
                            question3 = "";
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
                            readEighteenPlus = false;
                            eighteenPlusFlag = false;
                            readUnderstand = false;
                            understandFlag = false;
                            readParticipate = false;
                            participateFlag = false;
                            canRepeat = true;

                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                        {
                            question3 = "yes";
                            answeredQuestion3 = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe right registered. Wants to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                                if (SystemInfo.deviceUniqueIdentifier.Length <= 6)
                                {
                                    consentCode = SystemInfo.deviceUniqueIdentifier;
                                }
                                else if (SystemInfo.deviceUniqueIdentifier.Length > 6)
                                {
                                    consentCode = SystemInfo.deviceUniqueIdentifier.Substring(0, 6);
                                }
                                reportConsent(consentCode);
                            }, 1, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                        {
                            question2 = "yes";
                            answeredQuestion2 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Understood information.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            if ((answeredQuestion3 == false) && (finished_listening == true))
                            {
                                canRepeat = false;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                        {
                            question1 = "yes";
                            answeredQuestion1 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Is eighteen.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            if ((answeredQuestion2 == false) && (finished_listening == true))
                            {
                                canRepeat = false;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((readingConsentForm == false) && (hearingConsentForm == false) && (noConsent == false))
                        {
                            readingConsentForm = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Reading consent form manually.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            finished_reading = true;
                            answeredQuestion1 = true;
                            question1 = "yes";
                            answeredQuestion2 = true;
                            question2 = "yes";
                            answeredQuestion3 = true;
                            question3 = "yes";
                            finished_questions = true;

                            if (SystemInfo.deviceUniqueIdentifier.Length <= 6)
                            {
                                consentCode = SystemInfo.deviceUniqueIdentifier;
                            }
                            else if (SystemInfo.deviceUniqueIdentifier.Length > 6)
                            {
                                consentCode = SystemInfo.deviceUniqueIdentifier.Substring(0, 6);
                            }
                            reportConsent(consentCode);
                        }
                    }

                    if ((GameManager.instance.level == 11) && (survey_activated == true) && (survey_shown == true))
                    {
                        if (hearingSurvey == true)
                        {
                            if ((answeredSurveyQuestion15 == false) && (answeredSurveyQuestion14 == true) && (surveyQuestion14 != ""))
                            {
                                surveyQuestion15 = "yes";
                                answeredSurveyQuestion15 = true;
                                finished_listening_survey = true;
                                finished_questions_survey = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    canRepeat = true;
                                }, 0, null, true);
                            }
                            if ((answeredSurveyQuestion14 == false) && (answeredSurveyQuestion13 == true) && (surveyQuestion13 != ""))
                            {
                                surveyQuestion14 = "yes";
                                answeredSurveyQuestion14 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[32] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[31] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion13 == false) && (answeredSurveyQuestion12 == true) && (surveyQuestion12 != ""))
                            {
                                surveyQuestion13 = "yes";
                                answeredSurveyQuestion13 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[30] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[29] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion12 == false) && (answeredSurveyQuestion11 == true) && (surveyQuestion11 != ""))
                            {
                                surveyQuestion12 = "yes";
                                answeredSurveyQuestion12 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[28] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[27] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion11 == false) && (answeredSurveyQuestion10 == true) && (surveyQuestion10 != ""))
                            {
                                surveyQuestion11 = "yes";
                                answeredSurveyQuestion11 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[26] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[25] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion10 == false) && (answeredSurveyQuestion9 == true) && (surveyQuestion9 != ""))
                            {
                                surveyQuestion10 = "yes";
                                answeredSurveyQuestion10 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[24] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[23] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion9 == false) && (answeredSurveyQuestion8 == true) && (surveyQuestion8 != ""))
                            {
                                surveyQuestion9 = "yes";
                                answeredSurveyQuestion9 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[22] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[21] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion8 == false) && (answeredSurveyQuestion7 == true) && (surveyQuestion7 != ""))
                            {
                                surveyQuestion8 = "yes";
                                answeredSurveyQuestion8 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[20] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[19] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion7 == false) && (answeredSurveyQuestion6 == true) && (surveyQuestion6 != ""))
                            {
                                surveyQuestion7 = "yes";
                                answeredSurveyQuestion7 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[18] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[17] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion6 == false) && (answeredSurveyQuestion5 == true) && (surveyQuestion5 != ""))
                            {
                                surveyQuestion6 = "yes";
                                answeredSurveyQuestion6 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[16] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[15] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion5 == false) && (answeredSurveyQuestion4 == true) && (surveyQuestion4 != ""))
                            {
                                surveyQuestion5 = "yes";
                                answeredSurveyQuestion5 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[14] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[13] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion4 == false) && (answeredSurveyQuestion3 == true) && (surveyQuestion3 != ""))
                            {
                                surveyQuestion4 = "yes";
                                answeredSurveyQuestion4 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[12] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[11] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion3 == false) && (answeredSurveyQuestion2 == true) && (surveyQuestion2 != ""))
                            {
                                surveyQuestion3 = "yes";
                                answeredSurveyQuestion3 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[10] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[9] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion2 == false) && (answeredSurveyQuestion1 == true) && (surveyQuestion1 != ""))
                            {
                                surveyQuestion2 = "yes";
                                answeredSurveyQuestion2 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if (answeredSurveyQuestion1 == false)
                            {
                                surveyQuestion1 = "yes";
                                answeredSurveyQuestion1 = true;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }

                        if ((noConsent == true) || ((readingSurvey == true) && (finished_reading_survey == true)) || ((hearingSurvey == true) && (finished_listening_survey == true)) || ((hearingSurvey == false) && (readingSurvey == false) && (noSurvey == false)))
                        {
                            noConsent = false;
                            readingSurvey = true;
                            hearingSurvey = false;
                            android_window_displayed = false;
                            finished_listening_survey = false;
                            finished_reading_survey = true;
                            finished_questions_survey = true;
                            surveyQuestion1 = "na";
                            answeredSurveyQuestion1 = true;
                            surveyQuestion2 = "na";
                            answeredSurveyQuestion2 = true;
                            surveyQuestion3 = "na";
                            answeredSurveyQuestion3 = true;
                            surveyQuestion4 = "na";
                            answeredSurveyQuestion4 = true;
                            surveyQuestion5 = "na";
                            answeredSurveyQuestion5 = true;
                            surveyQuestion6 = "na";
                            answeredSurveyQuestion6 = true;
                            surveyQuestion7 = "na";
                            answeredSurveyQuestion7 = true;
                            surveyQuestion8 = "na";
                            answeredSurveyQuestion8 = true;
                            surveyQuestion9 = "na";
                            answeredSurveyQuestion9 = true;
                            surveyQuestion10 = "na";
                            answeredSurveyQuestion10 = true;
                            surveyQuestion11 = "na";
                            answeredSurveyQuestion11 = true;
                            surveyQuestion12 = "na";
                            answeredSurveyQuestion12 = true;
                            surveyQuestion13 = "na";
                            answeredSurveyQuestion13 = true;
                            surveyQuestion14 = "na";
                            answeredSurveyQuestion14 = true;
                            surveyQuestion15 = "na";
                            answeredSurveyQuestion15 = true;
                            readEnjoy = true;
                            enjoyFlag = true;
                            readPlayMore = true;
                            playMoreFlag = true;
                            readEasy = true;
                            easyFlag = true;
                            readLost = true;
                            lostFlag = true;
                            readUnderstandEcho = true;
                            understandEchoFlag = true;
                            readFrustrating = true;
                            frustratingFlag = true;
                            readTutorial = true;
                            tutorialFlag = true;
                            readTutorialHelp = true;
                            tutorialHelpFlag = true;
                            readHints = true;
                            hintsFlag = true;
                            readInstructions = true;
                            instructionsFlag = true;
                            readControls = true;
                            controlsFlag = true;
                            readLook = true;
                            lookFlag = true;
                            readEchoNavigate = true;
                            echoNavigateFlag = true;
                            readVisuallyImpaired = true;
                            visuallyImpairedFlag = true;
                            readHearingImpaired = true;
                            hearingImpairedFlag = true;
                            canRepeat = true;
                        }
                    }
                }
                // If the up arrow key has been pressed.
                else if (ie.isUp == true)
                {
                    // If the player has not finished the swiping part of the tutorial.
                    if ((canDoGestureTutorial == true) && (level1_remaining_swipe_ups > 0) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Swiped up for gesture tutorial.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level1_remaining_swipe_ups--; // Decrease the number of swipes up left to do.
                        if (level1_remaining_swipe_ups == 2)
                        {
                            clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[11] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This swipe was correct. Please swipe X more times.
                        }
                        else if (level1_remaining_swipe_ups == 1)
                        {
                            clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[12] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This swipe was correct. Please swipe X more times.
                        }
                        else if (level1_remaining_swipe_ups == 0)
                        {
                            debugPlayerInfo = "Finished swiping section for gesture tutorial.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[13], Database.tutorialClips[15] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    haveSwipedThreeTimes = true;
                                }, 4, null, true); // If they are using Talkback, play the correct instructions.
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[13], Database.tutorialClips[14] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    haveSwipedThreeTimes = true;
                                }, 0, null, true); // If they are not using Talkback, play the correct instructions.
                            }
                        }
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is not in the pause menu, move them forward.
                        if ((want_exit == false) && (loadingScene == false))
                        {
                            if ((canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                debugPlayerInfo = "Cannot swipe up at this time.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                            else
                            {
                                dir = transform.right; // Move the player forward.
                                swipedUp = true;
                            }
                        }
                        // If the player is in the pause menu, give them a hint.
                        else if ((want_exit == true) && (loadingScene == false))
                        {
                            debugPlayerInfo = "Swiped up. Gave player hint.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            getHint(); // Give the player a hint.
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        debugPlayerInfo = "Swipe up registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
                // If the down arrow key has been pressed.
                else if (ie.isDown == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (BoardManager.finishedTutorialLevel1 == false) && (hasStartedConsent == false))
                    {
                        // TODO: Replace the winSound with "Congratulations! You have completed the tutorial. Now we will move back to the game!"                                                      
                        BoardManager.finishedTutorialLevel1 = true; // Make sure the player does not have to go through the tutorial again if they have gone through it once.
                        GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                        GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                        GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
                        if (hasFinishedConsentForm == false)
                        {
                            finishedExitingInstruction = true;
                            hasStartedConsent = false;
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            can_display_window = false;
                            noConsent = false;
                            finished_reading = false;
                            canRepeat = true;
                            debugPlayerInfo = "Swiped down correctly. Moving to consent instructions.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                                                                                                     
                        }
                        else if (hasFinishedConsentForm == true)
                        {
                            debugPlayerInfo = "Swiped down correctly. Moving to level 1.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            canDoGestureTutorial = false;
                            clips = new List<AudioClip> { Database.soundEffectClips[9] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => quitTutorial(), 1, null, true);
                        }
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        if ((want_exit == false) && (loadingScene == false))
                        {
                            endingLevel = true;
                        }
                        else if ((want_exit == true) && (loadingScene == false))
                        {
                            // If the visual map for debugging is on, turn it off.
                            if (GameManager.levelImageActive)
                            {
                                debugPlayerInfo = "Swiped down. Turning off visual map for debugging.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                GameManager.instance.HideLevelImage(); // Turn off the map.
                                GameManager.instance.boardScript.gamerecord += "S_OFF"; // Record the switch off.
                            }
                            // If the visual map for debugging is off, turn it on.
                            else
                            {
                                debugPlayerInfo = "Swiped down. Turning on visual map for debugging.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                GameManager.instance.UnHideLevelImage(); // Turn on the map.
                                GameManager.instance.boardScript.gamerecord += "S_ON"; // Record the switch.
                            }
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = true;
                            android_window_displayed = false;
                            can_display_window = false;

                            debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                        else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true))
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = true;
                            android_window_displayed = false;
                            can_display_window = false;

                            debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                           

                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                    }

                    if ((GameManager.instance.level == 11) && (survey_activated == true) && (survey_shown == true))
                    {
                        if (finished_questions_survey == true)
                        {
							readingSurvey = false;
                            hearingSurvey = false;
                            noConsent = true;
                        }

                        if (hearingSurvey == true)
                        {
                            if ((answeredSurveyQuestion15 == false) && (answeredSurveyQuestion14 == true) && (surveyQuestion14 != ""))
                            {
                                surveyQuestion15 = "na";
                                answeredSurveyQuestion15 = true;
                                finished_listening = true;
                                finished_questions_survey = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    canRepeat = true;
                                }, 0, null, true);
                            }
                            if ((answeredSurveyQuestion14 == false) && (answeredSurveyQuestion13 == true) && (surveyQuestion13 != ""))
                            {
                                surveyQuestion14 = "na";
                                answeredSurveyQuestion14 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[32] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[31] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion13 == false) && (answeredSurveyQuestion12 == true) && (surveyQuestion12 != ""))
                            {
                                surveyQuestion13 = "na";
                                answeredSurveyQuestion13 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[30] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[29] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion12 == false) && (answeredSurveyQuestion11 == true) && (surveyQuestion11 != ""))
                            {
                                surveyQuestion12 = "na";
                                answeredSurveyQuestion12 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[28] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[27] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion11 == false) && (answeredSurveyQuestion10 == true) && (surveyQuestion10 != ""))
                            {
                                surveyQuestion11 = "na";
                                answeredSurveyQuestion11 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[26] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[25] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion10 == false) && (answeredSurveyQuestion9 == true) && (surveyQuestion9 != ""))
                            {
                                surveyQuestion10 = "na";
                                answeredSurveyQuestion10 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[24] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[23] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion9 == false) && (answeredSurveyQuestion8 == true) && (surveyQuestion8 != ""))
                            {
                                surveyQuestion9 = "na";
                                answeredSurveyQuestion9 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[22] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[21] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion8 == false) && (answeredSurveyQuestion7 == true) && (surveyQuestion7 != ""))
                            {
                                surveyQuestion8 = "na";
                                answeredSurveyQuestion8 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[20] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[19] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion7 == false) && (answeredSurveyQuestion6 == true) && (surveyQuestion6 != ""))
                            {
                                surveyQuestion7 = "na";
                                answeredSurveyQuestion7 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[18] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[17] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion6 == false) && (answeredSurveyQuestion5 == true) && (surveyQuestion5 != ""))
                            {
                                surveyQuestion6 = "na";
                                answeredSurveyQuestion6 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[16] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[15] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion5 == false) && (answeredSurveyQuestion4 == true) && (surveyQuestion4 != ""))
                            {
                                surveyQuestion5 = "na";
                                answeredSurveyQuestion5 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[14] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[13] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion4 == false) && (answeredSurveyQuestion3 == true) && (surveyQuestion3 != ""))
                            {
                                surveyQuestion4 = "na";
                                answeredSurveyQuestion4 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[12] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[11] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion3 == false) && (answeredSurveyQuestion2 == true) && (surveyQuestion2 != ""))
                            {
                                surveyQuestion3 = "na";
                                answeredSurveyQuestion3 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[10] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[9] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion2 == false) && (answeredSurveyQuestion1 == true) && (surveyQuestion1 != ""))
                            {
                                surveyQuestion2 = "na";
                                answeredSurveyQuestion2 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if (answeredSurveyQuestion1 == false)
                            {
                                surveyQuestion1 = "na";
                                answeredSurveyQuestion1 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }

                        if ((hearingSurvey == false) && (readingSurvey == false) && (noSurvey == false))
                        {
                            noSurvey = true;
                        }
                    }
                }
            }
            // If a rotation was registered.
            else if (ie.isRotate == true)
            {
                // If the left arrow key has been pressed.
                if (ie.isLeft == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is not in the pause menu, rotate them 90 degrees to the left.
                        if ((want_exit == false) && (loadingScene == false))
                        {
                            if ((canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x < 9) && (playerPos.y == 9))
                            {
                                debugPlayerInfo = "Cannot rotate at this time.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                            else
                            {
                                dir = get_player_dir("LEFT"); // Rotate the player left 90 degrees.
                                if (!GameManager.instance.boardScript.turning_lock)
                                {
                                    rotatedLeft = true;
                                }
                            }
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        debugPlayerInfo = "Left rotation registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
                // If the right arrow key has been pressed.
                else if (ie.isRight == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Rotated right for gesture tutorial. Turned player 90 degrees to the right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level3_remaining_turns--; // Decrease the number of right turns left to do.
                        dir = get_player_dir("RIGHT"); // Rotate the player right 90 degrees.
                        if (!GameManager.instance.boardScript.turning_lock)
                        {
                            if (level3_remaining_turns == 3)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[28] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This rotation was correct. Please rotate 3 more times.
                            }
                            else if (level3_remaining_turns == 2)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[29] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This rotation was correct. Please rotate 2 more times.                               
                            }
                            else if (level3_remaining_turns == 1)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[30] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This rotation was correct. Please rotate 1 more times.                               
                            }
                            // If the player has finished the rotation section of the tutorial.
                            else if (level3_remaining_turns == 0)
                            {
                                debugPlayerInfo = "Finished rotations for gesture tutorial. Completed gesture tutorial. Continuing with level 3.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                BoardManager.finishedTutorialLevel3 = true; // Make sure the player does not have to go through the tutorial again if they have gone through it once.
                                GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                                GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                                GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);

                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[31] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => quitTutorial(), 3, null);
                            }
                        }
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is not in the pause menu, rotate them 90 degrees to the right.
                        if ((want_exit == false) && (loadingScene == false))
                        {
                            if ((BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x < 9) && (playerPos.y == 9))
                            {
                                debugPlayerInfo = "Cannot rotate at this time.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                            else
                            {
                                dir = -transform.up; // Rotate the player right 90 degrees.
                                if (!GameManager.instance.boardScript.turning_lock)
                                {
                                    rotatedRight = true;
                                }
                            }
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        debugPlayerInfo = "Right rotation registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
            }
            // If a hold was registered.
            else if (ie.isHold == true)
            {
                if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                {
                    // If the pause menu has not been opened.
                    if (level1_remaining_menus == 2)
                    {
                        debugPlayerInfo = "Hold registered. Opened pause menu for gesture tutorial.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level1_remaining_menus--; // Decrease the number of holds left to do.
                        finishedMenuInstruction = false; // The player should not be able to make a gesture while we are explaining what you can do in the pause menu.
                        waitingForOpenMenuInstruction = true;  // The player should not be able to make a gesture while we are explaining what you can do in the pause menu.

                        if (GM_title.isUsingTalkback == true)
                        {
                            clips = new List<AudioClip> { Database.tutorialClips[17] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                        }
                        else if (GM_title.isUsingTalkback == false)
                        {
                            clips = new List<AudioClip> { Database.tutorialClips[16] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                        }
                    }
                    // If the pause menu has not been closed.
                    else if ((level1_remaining_menus == 1) && (waitingForOpenMenuInstruction == false))
                    {
                        debugPlayerInfo = "Hold registered. Closed pause menu. Finished menu section for gesture tutorial.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level1_remaining_menus--; // Decrease the number of holds left to do.

                        if (GM_title.isUsingTalkback == true)
                        {
                            if (hasFinishedConsentForm == true)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[19], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[20] };
                            }
                            else if (hasFinishedConsentForm == false)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[19], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[21] };
                            }
                        }
                        else if (GM_title.isUsingTalkback == false)
                        {
                            if (hasFinishedConsentForm == true)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[18], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[20] };
                            }
                            else if (hasFinishedConsentForm == false)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[18], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[21] };
                            }
                        }
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                    }
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (finishedSwipingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                {
                    // If the player is not in the pause menu, open the pause menu.
                    if ((want_exit == false) && (loadingScene == false))
                    {
                        debugPlayerInfo = "Hold registered. Opened pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        // If the player is using Talkback.
                        if (GM_title.isUsingTalkback == true)
                        {
                            clips = new List<AudioClip>() { Database.pauseMenuClips[0], Database.pauseMenuClips[2], Database.pauseMenuClips[4], Database.pauseMenuClips[8], Database.pauseMenuClips[12] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }
                        // If the player is not using Talkback.
                        else if (GM_title.isUsingTalkback == false)
                        {
                            clips = new List<AudioClip>() { Database.pauseMenuClips[0], Database.pauseMenuClips[1], Database.pauseMenuClips[3], Database.pauseMenuClips[7], Database.pauseMenuClips[11] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }
                        want_exit = true; // Open the pause menu.
                    }
                    // If the player is in the pause menu, close the pause menu.
                    else if ((want_exit == true) && (loadingScene == false))
                    {
                        debugPlayerInfo = "Hold registered. Closed pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.pauseMenuClips[13] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        want_exit = false; // Close the pause menu.
                    }
                }

                if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                {
                    debugPlayerInfo = "Hold registered. Does not do anything here.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            // If the 'p' key was pressed.
            else if ((ie.isMain == true) && (loadingScene == false))
            {
                debugPlayerInfo = "P key pressed. Moving to main menu.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                loadingScene = true;
                Destroy(GameObject.Find("GameManager"));
                SceneManager.LoadScene("Title_Screen"); // Move to the main menu.
            }
        }
#endif

        // Check if we are running on iOS or Android
#if UNITY_IOS || UNITY_ANDROID
        // process input
        if (eh.isActivate() && ((hasFinishedConsentForm == false) || ((hasFinishedConsentForm == true) && (canMakeGestures == true))))
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

            Vector2 playerPos = BoardManager.player_idx;

            // If a tap is registered.
            if (ie.isTap == true)
            {
                // If the player has not finished the tapping part of the tutorial.
                if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true))
                {
                    if (level1_remaining_taps > 0)
                    {
                        debugPlayerInfo = "Tapped for gesture tutorial. Played echo.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level1_remaining_taps--; // Decrease the amount of taps left to do.           

                        if (level1_remaining_taps == 2)
                        {
                            clips = new List<AudioClip> { Database.attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[5] };
                            float[] volumes = new float[] { 1.0f, 0.5f, 0.5f };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                finishedEcho = true;
                            }, 1, volumes, true); // This tap was correct. Please tap 2 more times.
                        }
                        else if (level1_remaining_taps == 1)
                        {
                            clips = new List<AudioClip> { Database.attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[6] };
                            float[] volumes = new float[] { 1.0f, 0.5f, 0.5f };
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                finishedEcho = true;
                            }, 1, volumes, true); // This tap was correct. Please tap 1 more time.                     
                        }
                        // If the player has finished the tapping section.
                        else if (level1_remaining_taps == 0)
                        {
                            debugPlayerInfo = "Finished tapping section for gesture tutorial.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip> { Database.attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[7], Database.tutorialClips[9], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
                                float[] volumes = new float[] { 1.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    finishedEcho = true;
                                    haveTappedThreeTimes = true;
                                }, 1, volumes, true); // If they are not using Talkback, play the correct instructions.                  
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip> { Database.attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[7], Database.tutorialClips[8], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
                                float[] volumes = new float[] { 1.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    finishedEcho = true;
                                    haveTappedThreeTimes = true;
                                }, 1, volumes, true); // If they are not using Talkback, play the correct instructions.
                            }
                        }
                    }
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }               

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                {
                    // If the player is not in the pause menu, play an echo.
                    if ((at_pause_menu == false) && (loadingScene == false))
                    {
                        if ((curLevel == 3) && (finishedCornerInstruction == false) && (playerPos.x == 9) && (playerPos.y == 9))
                        {
                            debugPlayerInfo = "Please wait for the instructions to finish.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            finishedEcho = false;

                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[23] };
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[22] };
                            }
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                debugPlayerInfo = "Finished corner instruction.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                finishedCornerInstruction = true; // We have finished the corner instruction, so the player can tap.             
                            }, 2, null, true); // Play the appropriate clip.               
                        }
                        else
                        {
                            if ((finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                            {
                                debugPlayerInfo = "Tapped at corner. Played echo.";
                            }
                            else
                            {
                                debugPlayerInfo = "Tap registered. Played echo.";
                            }
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            GameManager.instance.boardScript.gamerecord += "E{"; // Record the echo.                                         
                            if (BoardManager.gotBackToStart == false)
                            {
                                StartCoroutine(DelayedPlayEcho(0.25f)); // Play the echo.
                            }
                            GameManager.instance.boardScript.gamerecord += lastEcho;
                            GameManager.instance.boardScript.gamerecord += "}";
                            echoNum += 1;
                            tapped = true;
                        }
                    }
                    else if ((at_pause_menu == true) && (loadingScene == false))
                    {
                        // If the player has told us they want to restart the level, then restart the level.
                        if (wantLevelRestart == true)
                        {
                            debugPlayerInfo = "Tap registered. Restarting current level.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                            clips = new List<AudioClip>() { Database.pauseMenuClips[13] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // should have another set of sound effect
                            GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                            GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                            GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
                            restartLevel = true;
                        }

                        // If the player has told us they want to return to the main menu, then return to the main menu.
                        else if (wantMainMenu == true)
                        {
                            debugPlayerInfo = "Tap registered. Moving to main menu.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.pauseMenuClips[13] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // should have another set of sound effect
                            GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                            GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                            GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
                            goBackToMain = true;
                        }
                    }
                }

                if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                {
                    if (noConsent == true)
                    {
                        PlayerPrefs.SetInt("Consented", 0);
                        debugPlayerInfo = "Tap registered. Did not consent to having data collected. Can continue with level " + curLevel.ToString() + ".";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                        
                        hasFinishedConsentForm = true;
                        canPlayLevel = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[23], Database.levelStartClips[curLevel], Database.consentClips[24], Database.levelStartClips[curLevel] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            hasCheckedForConsent = true;
                            hasStartedConsent = false;
                            canRepeat = true;
                            if (curLevel == 1)
                            {
                                BoardManager.finishedTutorialLevel1 = true;
                                canDoGestureTutorial = false;
                            }
                            canPlayLevel = true;
                        }, 6, null, true);
                    }

                    else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == "yes") && (question2 == "yes") && (question3 == "yes"))
                    {
                        PlayerPrefs.SetInt("Consented", 1);
                        debugPlayerInfo = "Tap registered. Consented to having data collected. Can continue with level " + curLevel.ToString() + ".";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        hasFinishedConsentForm = true;
                        canPlayLevel = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[22], Database.levelStartClips[curLevel], Database.consentClips[24], Database.levelStartClips[curLevel] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            hasCheckedForConsent = true;
                            hasStartedConsent = false;
                            canRepeat = true;
                            if (curLevel == 1)
                            {
                                BoardManager.finishedTutorialLevel1 = true;
                                canDoGestureTutorial = false;
                            }
                            canPlayLevel = true;
                        }, 6, null, true);
                    }

                    else
                    {
                        debugPlayerInfo = "Tap registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }

                if ((GameManager.instance.level == 11) && (survey_activated == true) && (survey_shown == true))
                {
                    if ((hearingSurvey == true) && (finished_questions_survey == true))
                    {
                        hearingSurvey = false;
                        survey_activated = false;
                        reportsurvey(surveyCode);
                    }
                    if ((readingSurvey == true) && (finished_questions_survey == true))
                    {
                        readingSurvey = false;
                        survey_activated = false;
                        reportsurvey(surveyCode);
                    }
                    if (noConsent == true)
                    {
                        noConsent = false;
                        survey_activated = false;
                        reportsurvey(surveyCode);
                    }
                }
            }
            // If a swipe is registered.
            else if (ie.isSwipe == true)
            {
                if (ie.isLeft == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is not in the pause menu.
                        if ((at_pause_menu == false) && (loadingScene == false))
                        {
                            // Do nothing.
                            debugPlayerInfo = "Swiped left. Does nothing here.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        // If the player is in the pause menu.
                        else if ((at_pause_menu == true) && (loadingScene == false))
                        {
                            // If the swipe was left, the player has told us they want to restart the level.
                            debugPlayerInfo = "Swiped left. We want to restart the level.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[6] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[5] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            wantLevelRestart = true;
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {        
                        if ((noConsent == true) || (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == "no") || (question2 == "no") || (question3 == "no"))))
                        {
                            hearingConsentForm = true;
                            readingConsentForm = false;
                            noConsent = false;
                            hasCheckedForConsent = true;
                            hasFinishedConsentForm = false;
                            finished_listening = false;
                            finished_reading = false;
                            android_window_displayed = false;
                            can_display_window = false;
                            finished_questions = false;
                            answeredQuestion1 = false;
                            answeredQuestion2 = false;
                            answeredQuestion3 = false;
                            question1 = "";
                            question2 = "";
                            question3 = "";
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
                            readEighteenPlus = false;
                            eighteenPlusFlag = false;
                            readUnderstand = false;
                            understandFlag = false;
                            readParticipate = false;
                            participateFlag = false;
                            canRepeat = true;

                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                        {
                            question3 = "no";
                            answeredQuestion3 = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe left registered. Does not want to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                        {
                            question2 = "no";
                            answeredQuestion2 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Did not understand information.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if ((answeredQuestion3 == false) && (finished_listening == true))
                            {
                                canRepeat = false;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                        {
                            question1 = "no";
                            answeredQuestion1 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Is not eighteen.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if ((answeredQuestion2 == false) && (finished_listening == true))
                            {
                                canRepeat = false;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                        {
                            hearingConsentForm = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Hearing consent form through audio instructions.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                            
                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }                                            
                    }


                    if ((GameManager.instance.level == 11) && (survey_activated == true) && (survey_shown == true))
                    {
                        if (hearingSurvey == true)
                        {                                                                                                                  
                            if ((answeredSurveyQuestion15 == false) && (answeredSurveyQuestion14 == true) && (surveyQuestion14 != ""))
                            {
                                surveyQuestion15 = "no";                                       
                                answeredSurveyQuestion15 = true;
                                finished_listening_survey = true;
                                finished_questions_survey = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    canRepeat = true;
                                }, 0, null, true);
                            }
                            if ((answeredSurveyQuestion14 == false) && (answeredSurveyQuestion13 == true) && (surveyQuestion13 != ""))
                            {
                                surveyQuestion14 = "no";
                                answeredSurveyQuestion14 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[32] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[31] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion13 == false) && (answeredSurveyQuestion12 == true) && (surveyQuestion12 != ""))
                            {
                                surveyQuestion13 = "no";
                                answeredSurveyQuestion13 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[30] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[29] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion12 == false) && (answeredSurveyQuestion11 == true) && (surveyQuestion11 != ""))
                            {
                                surveyQuestion12 = "no";
                                answeredSurveyQuestion12 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[28] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[27] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion11 == false) && (answeredSurveyQuestion10 == true) && (surveyQuestion10 != ""))
                            {
                                surveyQuestion11 = "no";
                                answeredSurveyQuestion11 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[26] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[25] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion10 == false) && (answeredSurveyQuestion9 == true) && (surveyQuestion9 != ""))
                            {
                                surveyQuestion10 = "no";
                                answeredSurveyQuestion10 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[24] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[23] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion9 == false) && (answeredSurveyQuestion8 == true) && (surveyQuestion8 != ""))
                            {
                                surveyQuestion9 = "no";
                                answeredSurveyQuestion9 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[22] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[21] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion8 == false) && (answeredSurveyQuestion7 == true) && (surveyQuestion7 != ""))
                            {
                                surveyQuestion8 = "no";
                                answeredSurveyQuestion8 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[20] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[19] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion7 == false) && (answeredSurveyQuestion6 == true) && (surveyQuestion6 != ""))
                            {
                                surveyQuestion7 = "no";
                                answeredSurveyQuestion7 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[18] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[17] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion6 == false) && (answeredSurveyQuestion5 == true) && (surveyQuestion5 != ""))
                            {
                                surveyQuestion6 = "no";
                                answeredSurveyQuestion6 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[16] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[15] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion5 == false) && (answeredSurveyQuestion4 == true) && (surveyQuestion4 != ""))
                            {
                                surveyQuestion5 = "no";
                                answeredSurveyQuestion5 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[14] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[13] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion4 == false) && (answeredSurveyQuestion3 == true) && (surveyQuestion3 != ""))
                            {
                                surveyQuestion4 = "no";
                                answeredSurveyQuestion4 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[12] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[11] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion3 == false) && (answeredSurveyQuestion2 == true) && (surveyQuestion2 != ""))
                            {
                                surveyQuestion3 = "no";
                                answeredSurveyQuestion3 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[10] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[9] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion2 == false) && (answeredSurveyQuestion1 == true) && (surveyQuestion1 != ""))
                            {
                                surveyQuestion2 = "no";
                                answeredSurveyQuestion2 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if (answeredSurveyQuestion1 == false)
                            {
                                surveyQuestion1 = "no";
                                answeredSurveyQuestion1 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }

                        if ((noConsent == true) || ((readingSurvey == true) && (finished_reading_survey == true)) || ((hearingSurvey == true) && (finished_listening_survey == true)) || ((hearingSurvey == false) && (readingSurvey == false) && (noSurvey == false)))
                        {
							noConsent = false;
							readingSurvey = false;
							hearingSurvey = true;
                            android_window_displayed = false;
                            finished_listening_survey = false;
                            finished_reading_survey = false;
                            finished_questions_survey = false;
                            surveyQuestion1 = "";
                            answeredSurveyQuestion1 = false;
                            surveyQuestion2 = "";
                            answeredSurveyQuestion2 = false;
                            surveyQuestion3 = "";
                            answeredSurveyQuestion3 = false;
                            surveyQuestion4 = "";
                            answeredSurveyQuestion4 = false;
                            surveyQuestion5 = "";
                            answeredSurveyQuestion5 = false;
                            surveyQuestion6 = "";
                            answeredSurveyQuestion6 = false;
                            surveyQuestion7 = "";
                            answeredSurveyQuestion7 = false;
                            surveyQuestion8 = "";
                            answeredSurveyQuestion8 = false;
                            surveyQuestion9 = "";
                            answeredSurveyQuestion9 = false;
                            surveyQuestion10 = "";
                            answeredSurveyQuestion10 = false;
                            surveyQuestion11 = "";
                            answeredSurveyQuestion11 = false;
                            surveyQuestion12 = "";
                            answeredSurveyQuestion12 = false;
                            surveyQuestion13 = "";
                            answeredSurveyQuestion13 = false;
                            surveyQuestion14 = "";
                            answeredSurveyQuestion14 = false;
                            surveyQuestion15 = "";
                            answeredSurveyQuestion15 = false;
                            readEnjoy = false;
                            enjoyFlag = false;
                            readPlayMore = false;
                            playMoreFlag = false;
                            readEasy = false;
                            easyFlag = false;
                            readLost = false;
                            lostFlag = false;
                            readUnderstandEcho = false;
                            understandEchoFlag = false;
                            readFrustrating = false;
                            frustratingFlag = false;
                            readTutorial = false;
                            tutorialFlag = false;
                            readTutorialHelp = false;
                            tutorialHelpFlag = false;
                            readHints = false;
                            hintsFlag = false;
                            readInstructions = false;
                            instructionsFlag = false;
                            readControls = false;
                            controlsFlag = false;
                            readLook = false;
                            lookFlag = false;
                            readEchoNavigate = false;
                            echoNavigateFlag = false;
                            readVisuallyImpaired = false;
                            visuallyImpairedFlag = false;
                            readHearingImpaired = false;
                            hearingImpairedFlag = false;
                        }
                    }
                }

                else if (ie.isRight == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is not in the pause menu.
                        if ((at_pause_menu == false) && (loadingScene == false))
                        {
                            // Do nothing.
                            debugPlayerInfo = "Swiped right. Does nothing here.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        // If the player is in the pause menu.
                        else if ((at_pause_menu == true) && (loadingScene == false))
                        {
                            // If the swipe was right, the player has told us they want to go back to the main menu.                            
                            debugPlayerInfo = "Swiped right. We want to return to the main menu.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[10] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[9] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            wantMainMenu = true;
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {        
                        if ((noConsent == true) || (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == "no") || (question2 == "no") || (question3 == "no"))))
                        {
                            hearingConsentForm = false;
                            readingConsentForm = true;
                            noConsent = false;
                            hasCheckedForConsent = true;
                            hasFinishedConsentForm = false;
                            finished_listening = false;
                            finished_reading = false;
                            android_window_displayed = false;
                            can_display_window = false;
                            finished_questions = false;
                            answeredQuestion1 = false;
                            answeredQuestion2 = false;
                            answeredQuestion3 = false;
                            question1 = "";
                            question2 = "";
                            question3 = "";
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
                            readEighteenPlus = false;
                            eighteenPlusFlag = false;
                            readUnderstand = false;
                            understandFlag = false;
                            readParticipate = false;
                            participateFlag = false;
                            canRepeat = true;

                            clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                        {
                            question3 = "yes";
                            answeredQuestion3 = true;                            
                            finished_questions = true;
                            debugPlayerInfo = "Swipe right registered. Wants to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                                if (SystemInfo.deviceUniqueIdentifier.Length <= 6)
                                {
                                    consentCode = SystemInfo.deviceUniqueIdentifier;
                                }
                                else if (SystemInfo.deviceUniqueIdentifier.Length > 6)
                                {
                                    consentCode = SystemInfo.deviceUniqueIdentifier.Substring(0, 6);
                                }
                                reportConsent(SystemInfo.deviceUniqueIdentifier);
                            }, 1, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                        {
                            question2 = "yes";
                            answeredQuestion2 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Understood information.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if ((answeredQuestion3 == false) && (finished_listening == true))
                            {
                                canRepeat = false;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                        {
                            question1 = "yes";
                            answeredQuestion1 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Is eighteen.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if ((answeredQuestion2 == false) && (finished_listening == true))
                            {
                                canRepeat = false;
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }
                        else if ((readingConsentForm == false) && (hearingConsentForm == false) && (noConsent == false))
                        {
                            readingConsentForm = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Reading consent form manually.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }                                              
                    }

                    if ((GameManager.instance.level == 11) && (survey_activated == true) && (survey_shown == true))
                    {
                        if (hearingSurvey == true)
                        {
                            if ((answeredSurveyQuestion15 == false) && (answeredSurveyQuestion14 == true) && (surveyQuestion14 != ""))
                            {
                                surveyQuestion15 = "yes";
                                answeredSurveyQuestion15 = true;
                                finished_listening_survey = true;
                                finished_questions_survey = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    canRepeat = true;
                                }, 0, null, true);
                            }
                            if ((answeredSurveyQuestion14 == false) && (answeredSurveyQuestion13 == true) && (surveyQuestion13 != ""))
                            {
                                surveyQuestion14 = "yes";
                                answeredSurveyQuestion14 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[32] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[31] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion13 == false) && (answeredSurveyQuestion12 == true) && (surveyQuestion12 != ""))
                            {
                                surveyQuestion13 = "yes";
                                answeredSurveyQuestion13 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[30] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[29] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion12 == false) && (answeredSurveyQuestion11 == true) && (surveyQuestion11 != ""))
                            {
                                surveyQuestion12 = "yes";
                                answeredSurveyQuestion12 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[28] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[27] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion11 == false) && (answeredSurveyQuestion10 == true) && (surveyQuestion10 != ""))
                            {
                                surveyQuestion11 = "yes";
                                answeredSurveyQuestion11 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[26] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[25] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion10 == false) && (answeredSurveyQuestion9 == true) && (surveyQuestion9 != ""))
                            {
                                surveyQuestion10 = "yes";
                                answeredSurveyQuestion10 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[24] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[23] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion9 == false) && (answeredSurveyQuestion8 == true) && (surveyQuestion8 != ""))
                            {
                                surveyQuestion9 = "yes";
                                answeredSurveyQuestion9 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[22] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[21] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion8 == false) && (answeredSurveyQuestion7 == true) && (surveyQuestion7 != ""))
                            {
                                surveyQuestion8 = "yes";
                                answeredSurveyQuestion8 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[20] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[19] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion7 == false) && (answeredSurveyQuestion6 == true) && (surveyQuestion6 != ""))
                            {
                                surveyQuestion7 = "yes";
                                answeredSurveyQuestion7 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[18] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[17] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion6 == false) && (answeredSurveyQuestion5 == true) && (surveyQuestion5 != ""))
                            {
                                surveyQuestion6 = "yes";
                                answeredSurveyQuestion6 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[16] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[15] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion5 == false) && (answeredSurveyQuestion4 == true) && (surveyQuestion4 != ""))
                            {
                                surveyQuestion5 = "yes";
                                answeredSurveyQuestion5 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[14] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[13] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion4 == false) && (answeredSurveyQuestion3 == true) && (surveyQuestion3 != ""))
                            {
                                surveyQuestion4 = "yes";
                                answeredSurveyQuestion4 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[12] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[11] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion3 == false) && (answeredSurveyQuestion2 == true) && (surveyQuestion2 != ""))
                            {
                                surveyQuestion3 = "yes";
                                answeredSurveyQuestion3 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[10] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[9] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion2 == false) && (answeredSurveyQuestion1 == true) && (surveyQuestion1 != ""))
                            {
                                surveyQuestion2 = "yes";
                                answeredSurveyQuestion2 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if (answeredSurveyQuestion1 == false)
                            {
                                surveyQuestion1 = "yes";
                                answeredSurveyQuestion1 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }

                        if ((noConsent == true) || ((readingSurvey == true) && (finished_reading_survey == true)) || ((hearingSurvey == true) && (finished_listening_survey == true)) || ((hearingSurvey == false) && (readingSurvey == false) && (noSurvey == false)))
                        {
                            noConsent = false;
							hearingSurvey = false;
                            readingSurvey = true;
                            android_window_displayed = false;
                            if ((noConsent == true) || (readingSurvey == true) || (hearingSurvey == true))
                            {
                                can_display_window_survey = true;
                            }
                            finished_reading_survey = false;
                            finished_listening_survey = false;
                            finished_questions_survey = false;
                            surveyQuestion1 = "";
                            answeredSurveyQuestion1 = false;
                            surveyQuestion2 = "";
                            answeredSurveyQuestion2 = false;
                            surveyQuestion3 = "";
                            answeredSurveyQuestion3 = false;
                            surveyQuestion4 = "";
                            answeredSurveyQuestion4 = false;
                            surveyQuestion5 = "";
                            answeredSurveyQuestion5 = false;
                            surveyQuestion6 = "";
                            answeredSurveyQuestion6 = false;
                            surveyQuestion7 = "";
                            answeredSurveyQuestion7 = false;
                            surveyQuestion8 = "";
                            answeredSurveyQuestion8 = false;
                            surveyQuestion9 = "";
                            answeredSurveyQuestion9 = false;
                            surveyQuestion10 = "";
                            answeredSurveyQuestion10 = false;
                            surveyQuestion11 = "";
                            answeredSurveyQuestion11 = false;
                            surveyQuestion12 = "";
                            answeredSurveyQuestion12 = false;
                            surveyQuestion13 = "";
                            answeredSurveyQuestion13 = false;
                            surveyQuestion14 = "";
                            answeredSurveyQuestion14 = false;
                            surveyQuestion15 = "";
                            answeredSurveyQuestion15 = false;
                            readEnjoy = false;
                            enjoyFlag = false;
                            readPlayMore = false;
                            playMoreFlag = false;
                            readEasy = false;
                            easyFlag = false;
                            readLost = false;
                            lostFlag = false;
                            readUnderstandEcho = false;
                            understandEchoFlag = false;
                            readFrustrating = false;
                            frustratingFlag = false;
                            readTutorial = false;
                            tutorialFlag = false;
                            readTutorialHelp = false;
                            tutorialHelpFlag = false;
                            readHints = false;
                            hintsFlag = false;
                            readInstructions = false;
                            instructionsFlag = false;
                            readControls = false;
                            controlsFlag = false;
                            readLook = false;
                            lookFlag = false;
                            readEchoNavigate = false;
                            echoNavigateFlag = false;
                            readVisuallyImpaired = false;
                            visuallyImpairedFlag = false;
                            readHearingImpaired = false;
                            hearingImpairedFlag = false;
                        }                     
                    }
                }

                else if (ie.isUp == true)
                {
                    // If the player has not finished the swiping part of the tutorial.
                    if ((canDoGestureTutorial == true) && (level1_remaining_swipe_ups > 0) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Swiped up for gesture tutorial.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level1_remaining_swipe_ups--; // Decrease the number of swipes up left to do.
                        if (level1_remaining_swipe_ups == 2)
                        {
                            clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[11] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This swipe was correct. Please swipe X more times.
                        }
                        else if (level1_remaining_swipe_ups == 1)
                        {
                            clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[12] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This swipe was correct. Please swipe X more times.
                        }
                        else if (level1_remaining_swipe_ups == 0)
                        {
                            debugPlayerInfo = "Finished swiping section for gesture tutorial.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[13], Database.tutorialClips[15] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    haveSwipedThreeTimes = true;
                                }, 4, null, true); // If they are using Talkback, play the correct instructions.
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[13], Database.tutorialClips[14] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    haveSwipedThreeTimes = true;
                                }, 0, null, true); // If they are not using Talkback, play the correct instructions.
                            }
                        }
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is not in the pause menu.
                        if ((at_pause_menu == false) && (loadingScene == false))
                        {
                            // If the swipe was up, move the player forward if they are not in a gesture tutorial
                            if ((BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                debugPlayerInfo = "Cannot swipe up at this time.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                            else
                            {
                                dir = get_player_dir("FRONT"); // Move the player forward.
                                swipedUp = true;
                            }
                        }
                        // If the player is in the pause menu.
                        else if ((at_pause_menu == true) && (loadingScene == false))
                        {
                            // If the swipe was up, give the player a hint.
                            debugPlayerInfo = "Swiped up. Gave player hint.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            getHint(); // Give the player a hint.                          
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        debugPlayerInfo = "Swipe up registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                    
                    }
                }

                else if (ie.isDown == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (BoardManager.finishedTutorialLevel1 == false) && (hasStartedConsent == false))
                    {
                        // TODO: Replace the winSound with "Congratulations! You have completed the tutorial. Now we will move back to the game!"                                                      
                        BoardManager.finishedTutorialLevel1 = true; // Make sure the player does not have to go through the tutorial again if they have gone through it once.
                        GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                        GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                        GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
                        if (hasFinishedConsentForm == false)
                        {
                            finishedExitingInstruction = true;
                            hasStartedConsent = false;
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            can_display_window = false;
                            noConsent = false;
                            finished_reading = false;
                            canRepeat = true;
                            debugPlayerInfo = "Swiped down correctly. Moving to consent instructions.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                                                                                                     
                        }
                        else if (hasFinishedConsentForm == true)
                        {
                            debugPlayerInfo = "Swiped down correctly. Moving to level 1.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            canDoGestureTutorial = false;
                            clips = new List<AudioClip> { Database.soundEffectClips[9] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => quitTutorial(), 1, null, true);
                        }
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is not in the pause menu.
                        if ((at_pause_menu == false) && (loadingScene == false))
                        {
                            endingLevel = true; // If the swipe was down, attempt to exit the level.
                        }
                        // If the player is in the pause menu.
                        else if ((at_pause_menu == true) && (loadingScene == false))
                        {
                            // If the visual map for debugging is on, turn it off.
                            if (GameManager.levelImageActive)
                            {
                                debugPlayerInfo = "Swiped down. Turning off visual map for debugging.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                GameManager.instance.HideLevelImage(); // Turn off the map.
                                GameManager.instance.boardScript.gamerecord += "S_OFF"; // Record the switch off.
                            }
                            // If the visual map for debugging is off, turn it on.
                            else
                            {
                                debugPlayerInfo = "Swiped down. Turning on visual map for debugging.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                GameManager.instance.UnHideLevelImage(); // Turn on the map.
                                GameManager.instance.boardScript.gamerecord += "S_ON"; // Record the switch.
                            }
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = true;
                            android_window_displayed = false;
                            can_display_window = false;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                        else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true))
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = true;
                            android_window_displayed = false;
                            can_display_window = false;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                    }

                    if ((GameManager.instance.level == 11) && (survey_activated == true) && (survey_shown == true))
                    {
                        if (finished_questions_survey == true)
                        {
							readingSurvey = false;
                            hearingSurvey = false;
                            noConsent = true;
                        }

                        if (hearingSurvey == true)
                        {
                            if ((answeredSurveyQuestion15 == false) && (answeredSurveyQuestion14 == true) && (surveyQuestion14 != ""))
                            {
                                surveyQuestion15 = "na";
                                answeredSurveyQuestion15 = true;
                                finished_listening_survey = true;
                                finished_questions_survey = true;
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.surveyClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    canRepeat = true;
                                }, 0, null, true);
                            }
                            if ((answeredSurveyQuestion14 == false) && (answeredSurveyQuestion13 == true) && (surveyQuestion13 != ""))
                            {
                                surveyQuestion14 = "na";
                                answeredSurveyQuestion14 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[32] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[31] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion13 == false) && (answeredSurveyQuestion12 == true) && (surveyQuestion12 != ""))
                            {
                                surveyQuestion13 = "na";
                                answeredSurveyQuestion13 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[30] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[29] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion12 == false) && (answeredSurveyQuestion11 == true) && (surveyQuestion11 != ""))
                            {
                                surveyQuestion12 = "na";
                                answeredSurveyQuestion12 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[28] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[27] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion11 == false) && (answeredSurveyQuestion10 == true) && (surveyQuestion10 != ""))
                            {
                                surveyQuestion11 = "na";
                                answeredSurveyQuestion11 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[26] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[25] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion10 == false) && (answeredSurveyQuestion9 == true) && (surveyQuestion9 != ""))
                            {
                                surveyQuestion10 = "na";
                                answeredSurveyQuestion10 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[24] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[23] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion9 == false) && (answeredSurveyQuestion8 == true) && (surveyQuestion8 != ""))
                            {
                                surveyQuestion9 = "na";
                                answeredSurveyQuestion9 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[22] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[21] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion8 == false) && (answeredSurveyQuestion7 == true) && (surveyQuestion7 != ""))
                            {
                                surveyQuestion8 = "na";
                                answeredSurveyQuestion8 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[20] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[19] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion7 == false) && (answeredSurveyQuestion6 == true) && (surveyQuestion6 != ""))
                            {
                                surveyQuestion7 = "na";
                                answeredSurveyQuestion7 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[18] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[17] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion6 == false) && (answeredSurveyQuestion5 == true) && (surveyQuestion5 != ""))
                            {
                                surveyQuestion6 = "na";
                                answeredSurveyQuestion6 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[16] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[15] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion5 == false) && (answeredSurveyQuestion4 == true) && (surveyQuestion4 != ""))
                            {
                                surveyQuestion5 = "na";
                                answeredSurveyQuestion5 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[14] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[13] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion4 == false) && (answeredSurveyQuestion3 == true) && (surveyQuestion3 != ""))
                            {
                                surveyQuestion4 = "na";
                                answeredSurveyQuestion4 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[12] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[11] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion3 == false) && (answeredSurveyQuestion2 == true) && (surveyQuestion2 != ""))
                            {
                                surveyQuestion3 = "na";
                                answeredSurveyQuestion3 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[10] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[9] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if ((answeredSurveyQuestion2 == false) && (answeredSurveyQuestion1 == true) && (surveyQuestion1 != ""))
                            {
                                surveyQuestion2 = "na";
                                answeredSurveyQuestion2 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[8] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[7] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                            if (answeredSurveyQuestion1 == false)
                            {
                                surveyQuestion1 = "na";
                                answeredSurveyQuestion1 = true;
								if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[6] };
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip>() { Database.surveyClips[5] };
                                }
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                            }
                        }

                        if ((hearingSurvey == false) && (readingSurvey == false) && (noSurvey == false))
                        {
                            noSurvey = true;
                        }
                    }
                }
            }
            // If a rotation was registered.
            else if (ie.isRotate == true)
            {
                if (ie.isLeft == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is not in the pause menu.
                        if ((at_pause_menu == false) && (loadingScene == false))
                        {
                            // If the rotation was left, rotate the player left 90 degrees if they are not in a gesture tutorial.
                            if ((BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x < 9) && (playerPos.y == 9))
                            {
                                debugPlayerInfo = "Cannot rotate at this time.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                            else
                            {
                                dir = get_player_dir("LEFT"); // Rotate the player left 90 degrees.
                                if (!GameManager.instance.boardScript.turning_lock)
                                {
                                    rotatedLeft = true;
                                }
                            }
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        debugPlayerInfo = "Left rotation registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                    
                    }
                }

                else if (ie.isRight == true)
                {
                    if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                    {
                        debugPlayerInfo = "Rotated right for gesture tutorial. Turned player 90 degrees to the right.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level3_remaining_turns--; // Decrease the number of right turns left to do.
                        dir = get_player_dir("RIGHT"); // Rotate the player right 90 degrees.
                        if (!GameManager.instance.boardScript.turning_lock)
                        {
                            if (level3_remaining_turns == 3)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[28] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This rotation was correct. Please rotate 3 more times.
                            }
                            else if (level3_remaining_turns == 2)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[29] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This rotation was correct. Please rotate 2 more times.                               
                            }
                            else if (level3_remaining_turns == 1)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[30] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // This rotation was correct. Please rotate 1 more times.                               
                            }
                            // If the player has finished the rotation section of the tutorial.
                            else if (level3_remaining_turns == 0)
                            {
                                debugPlayerInfo = "Finished rotations for gesture tutorial. Completed gesture tutorial. Continuing with level 3.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                BoardManager.finishedTutorialLevel3 = true; // Make sure the player does not have to go through the tutorial again if they have gone through it once.
                                GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                                GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                                GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);

                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[31] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => quitTutorial(), 3, null);
                            }
                        }
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }                                       

                    if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                    {
                        // If the player is not in the pause menu.
                        if ((at_pause_menu == false) && (loadingScene == false))
                        {
                            // If the rotation was right, rotate the player right 90 degrees if they are not in a gesture tutorial.
                            if ((BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x < 9) && (playerPos.y == 9))
                            {
                                debugPlayerInfo = "Cannot rotate at this time.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                            else
                            {
                                dir = get_player_dir("RIGHT"); // Rotate the player right 90 degrees.
                                if (!GameManager.instance.boardScript.turning_lock)
                                {
                                    rotatedRight = true;
                                }
                            }
                        }
                    }

                    if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        debugPlayerInfo = "Right rotation registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
            }
            // If a hold is registered.
            else if (ie.isHold == true)
            {
                if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                {
                    // If the pause menu has not been opened.
                    if (level1_remaining_menus == 2)
                    {
                        debugPlayerInfo = "Hold registered. Opened pause menu for gesture tutorial.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level1_remaining_menus--; // Decrease the number of holds left to do.
                        finishedMenuInstruction = false; // The player should not be able to make a gesture while we are explaining what you can do in the pause menu.
                        waitingForOpenMenuInstruction = true;  // The player should not be able to make a gesture while we are explaining what you can do in the pause menu.

                        if (GM_title.isUsingTalkback == true)
                        {
                            clips = new List<AudioClip> { Database.tutorialClips[17] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                        }
                        else if (GM_title.isUsingTalkback == false)
                        {
                            clips = new List<AudioClip> { Database.tutorialClips[16] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                        }
                    }
                    // If the pause menu has not been closed.
                    else if ((level1_remaining_menus == 1) && (waitingForOpenMenuInstruction == false))
                    {
                        debugPlayerInfo = "Hold registered. Closed pause menu. Finished menu section for gesture tutorial.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level1_remaining_menus--; // Decrease the number of holds left to do.

                        if (GM_title.isUsingTalkback == true)
                        {
                            if (hasFinishedConsentForm == true)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[19], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[20] };
                            }
                            else if (hasFinishedConsentForm == false)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[19], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[21] };
                            }
                        }
                        else if (GM_title.isUsingTalkback == false)
                        {
                            if (hasFinishedConsentForm == true)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[18], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[20] };
                            }
                            else if (hasFinishedConsentForm == false)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[18], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[21] };
                            }
                        }
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                    }
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (finishedSwipingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }

                if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                {
                    // If the player is not in the pause menu, open the pause menu.
                    if ((at_pause_menu == false) && (loadingScene == false))
                    {
                        debugPlayerInfo = "Hold registered. Opened pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        at_pause_menu = true; // The player is now in the pause menu.
                                              // If the player is using Talkback.
                        if (GM_title.isUsingTalkback == true)
                        {
                            clips = new List<AudioClip>() { Database.pauseMenuClips[0], Database.pauseMenuClips[2], Database.pauseMenuClips[4], Database.pauseMenuClips[8], Database.pauseMenuClips[12] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }
                        // If the player is not using Talkback.
                        else if (GM_title.isUsingTalkback == false)
                        {
                            clips = new List<AudioClip>() { Database.pauseMenuClips[0], Database.pauseMenuClips[1], Database.pauseMenuClips[3], Database.pauseMenuClips[7], Database.pauseMenuClips[11] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                        }
                    }
                    // If the player is in the pause menu, close the pause menu.
                    else if ((at_pause_menu == true) && (loadingScene == false))
                    {
                        debugPlayerInfo = "Hold registered. Closed pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        at_pause_menu = false; // The player is no longer in the pause menu.
                        clips = new List<AudioClip>() { Database.pauseMenuClips[13] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                }

                if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                {
                    debugPlayerInfo = "Hold registered. Does nothing here.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }

            // If there was an unrecognized gesture made.
            if ((ie.isUnrecognized == true) && (loadingScene == false))
            {
                madeUnrecognizedGesture = true;

                if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                {
                    if ((ie.isSwipeLeftHorizontalError == true) || (ie.isSwipeRightHorizontalError == true) || (ie.isSwipeUpVerticalError == true) || (ie.isSwipeDownVerticalError == true) || (ie.isSwipeHorizontalVerticalError == true) || (ie.isSwipeLeftRotationError == true) || (ie.isSwipeRightRotationError == true) || (ie.isSwipeUpRotationError == true) || (ie.isSwipeDownRotationError == true) || (ie.isSwipeDirectionError == true) || (ie.isRotationAngleError == true) || (ie.isHoldHorizontalError == true) || (ie.isHoldVerticalError == true) || (ie.isHoldHorizontalVerticalError == true) || (ie.isHoldRotationError == true) || (ie.isBetweenHoldSwipeError == true))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isTapHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.                    
                    }
                    else if (ie.isTapVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.               
                    }
                    else if (ie.isTapHorizontalVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.                 
                    }
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
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == false) && (finishedTappingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedSwipingInstruction == true) && (level1_remaining_swipe_ups > 0))
                {
                    if ((ie.isTapHorizontalError == true) || (ie.isTapVerticalError == true) || (ie.isTapHorizontalVerticalError == true) || (ie.isTapRotationError == true) || (ie.isSwipeLeftHorizontalError == true) || (ie.isSwipeRightHorizontalError == true) || (ie.isSwipeDownVerticalError == true) || (ie.isSwipeLeftRotationError == true) || (ie.isSwipeRightRotationError == true) || (ie.isSwipeDownRotationError == true) || (ie.isRotationAngleError == true) || (ie.isHoldHorizontalError == true) || (ie.isHoldVerticalError == true) || (ie.isHoldHorizontalVerticalError == true) || (ie.isHoldRotationError == true))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                        SoundManager.instance.PlayVoice(Database.errorClips[19], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isSwipeUpVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe up.";
                        SoundManager.instance.PlayVoice(Database.errorClips[6], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
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
                    else if (ie.isSwipeUpRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe up.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == false) && (waitingForOpenMenuInstruction == false) && (finishedSwipingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (finishedSwipingInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedMenuInstruction == true) && (level1_remaining_menus > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should hold.";

                    if ((ie.isTapHorizontalError == true) || (ie.isTapVerticalError == true) || (ie.isTapHorizontalVerticalError == true) || (ie.isTapRotationError == true) || (ie.isBetweenTapSwipeError == true) || (ie.isSwipeLeftHorizontalError == true) || (ie.isSwipeRightHorizontalError == true) || (ie.isSwipeUpVerticalError == true) || (ie.isSwipeDownVerticalError == true) || (ie.isSwipeHorizontalVerticalError == true) || (ie.isSwipeLeftRotationError == true) || (ie.isSwipeRightRotationError == true) || (ie.isSwipeUpRotationError == true) || (ie.isSwipeDownRotationError == true) || (ie.isSwipeDirectionError == true) || (ie.isRotationAngleError == true))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isBetweenHoldSwipeError == true)
                    {
                        debugPlayerInfo = "Nothing happened because gesture was in between hold and swipe.";
                        SoundManager.instance.PlayVoice(Database.errorClips[11], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isHoldHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[14], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isHoldVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[15], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isHoldHorizontalVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[16], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isHoldRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (waitingForOpenMenuInstruction == true) && (level1_remaining_menus > 0))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should hold.";

                    if ((ie.isTapHorizontalError == true) || (ie.isTapVerticalError == true) || (ie.isTapHorizontalVerticalError == true) || (ie.isTapRotationError == true) || (ie.isBetweenTapSwipeError == true) || (ie.isSwipeLeftHorizontalError == true) || (ie.isSwipeRightHorizontalError == true) || (ie.isSwipeUpVerticalError == true) || (ie.isSwipeDownVerticalError == true) || (ie.isSwipeHorizontalVerticalError == true) || (ie.isSwipeLeftRotationError == true) || (ie.isSwipeRightRotationError == true) || (ie.isSwipeUpRotationError == true) || (ie.isSwipeDownRotationError == true) || (ie.isSwipeDirectionError == true) || (ie.isRotationAngleError == true))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[20], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isBetweenHoldSwipeError == true)
                    {
                        debugPlayerInfo = "Nothing happened because gesture was in between hold and swipe.";
                        SoundManager.instance.PlayVoice(Database.errorClips[11], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isHoldHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[14], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isHoldVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[15], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isHoldHorizontalVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[16], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isHoldRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                        SoundManager.instance.PlayVoice(Database.errorClips[17], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == false) && (finishedMenuInstruction == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 1) && (finishedExitingInstruction == true) && (level1_remaining_menus == 0) && (hasStartedConsent == false))
                {
                    if ((ie.isTapHorizontalError == true) || (ie.isTapVerticalError == true) || (ie.isTapHorizontalVerticalError == true) || (ie.isTapRotationError == true) || (ie.isSwipeLeftHorizontalError == true) || (ie.isSwipeRightHorizontalError == true) || (ie.isSwipeUpVerticalError == true) || (ie.isSwipeLeftRotationError == true) || (ie.isSwipeRightRotationError == true) || (ie.isSwipeUpRotationError == true) || (ie.isRotationAngleError == true) || (ie.isHoldHorizontalError == true) || (ie.isHoldVerticalError == true) || (ie.isHoldHorizontalVerticalError == true) || (ie.isHoldRotationError == true))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                        SoundManager.instance.PlayVoice(Database.errorClips[21], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isSwipeDownVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe down.";
                        SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
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
                    else if (ie.isSwipeDownRotationError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with rotation on swipe down.";
                        SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                {
                    if ((ie.isTapHorizontalError == true) || (ie.isTapVerticalError == true) || (ie.isTapHorizontalVerticalError == true) || (ie.isTapRotationError == true) || (ie.isBetweenTapSwipeError == true) || (ie.isSwipeLeftHorizontalError == true) || (ie.isSwipeRightHorizontalError == true) || (ie.isSwipeUpVerticalError == true) || (ie.isSwipeDownVerticalError == true) || (ie.isSwipeHorizontalVerticalError == true) || (ie.isSwipeLeftRotationError == true) || (ie.isSwipeRightRotationError == true) || (ie.isSwipeUpRotationError == true) || (ie.isSwipeDownRotationError == true) || (ie.isSwipeDirectionError == true) || (ie.isHoldHorizontalError == true) || (ie.isHoldVerticalError == true) || (ie.isHoldHorizontalVerticalError == true) || (ie.isHoldRotationError == true) || (ie.isBetweenHoldSwipeError == true))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should rotate right.";
                        SoundManager.instance.PlayVoice(Database.errorClips[22], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isRotationAngleError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with angle on rotation.";
                        SoundManager.instance.PlayVoice(Database.errorClips[13], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                }
                else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedCornerInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                }
                else if ((canDoGestureTutorial == false) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                {
                    if ((ie.isSwipeLeftHorizontalError == true) || (ie.isSwipeRightHorizontalError == true) || (ie.isSwipeUpVerticalError == true) || (ie.isSwipeDownVerticalError == true) || (ie.isSwipeHorizontalVerticalError == true) || (ie.isSwipeLeftRotationError == true) || (ie.isSwipeRightRotationError == true) || (ie.isSwipeUpRotationError == true) || (ie.isSwipeDownRotationError == true) || (ie.isSwipeDirectionError == true) || (ie.isRotationAngleError == true) || (ie.isHoldHorizontalError == true) || (ie.isHoldVerticalError == true) || (ie.isHoldHorizontalVerticalError == true) || (ie.isHoldRotationError == true) || (ie.isBetweenHoldSwipeError == true))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }
                    else if (ie.isTapHorizontalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.                    
                    }
                    else if (ie.isTapVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.               
                    }
                    else if (ie.isTapHorizontalVerticalError == true)
                    {
                        debugPlayerInfo = "Nothing happened due to error with horizontal and vertical distance on tap.";
                        SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.                 
                    }
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
                }                

                else
                {
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
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            }
        }
#endif // End of mobile platform dependendent compilation section started above with #elif
                    calculateMove(dir);
    }

    protected override void OnCantMove<T>(T component)
    {
        // Set hitWall to equal the component passed in as a parameter.
        Wall hitWall = component as Wall;
        // if (!SoundManager.instance.isBusy())
        // { 
        //     SoundManager.instance.PlayVoice(Database.soundEffectClips[8], true, 0.0f, 0.0f, 0.5f);
        // }
    }

    private void OnDisable()
    {
        // When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
        // int nextLevel = curLevel + 1;
        // GameManager.instance.level = nextLevel;
    }

    // Restart reloads the scene when called.
    private void Restart()
    {
        SceneManager.LoadScene("Main"); // Load the last scene loaded, in this case Main, the only scene in the game.
    }

    protected override void OnMove()
    {

    }

    /// <summary>
    /// If the player is in a gesture tutorial, leave the tutorial so that they can continue with the level.
    /// </summary>
    private void quitTutorial()
    {
        canDoGestureTutorial = false;
    }

    /// <summary>
    /// Check if the player is connected to the internet.
    /// </summary>
    /// <param name="temp"></param>
    /// <param name="accurateElapsed"></param>
    /// <param name="score"></param>
    /// <param name="levelDataEndpoint"></param>   
    void CheckInternetConnection(int temp, float accurateElapsed, int score, string levelDataEndpoint)
    {
        // Start the coroutine that checks their connectivity.
        StartCoroutine(CheckConnectivity(temp, accurateElapsed, score, levelDataEndpoint));
    }

    /// <summary>
    /// Check if the player is connected to the internet via WiFi or cell service. 
    /// If the player is connected to the internet and would not be redirected to another page if they tried to go to a site (i.e. an airport or hotel WiFi login screen), 
    /// then send their game data for the level to the server. 
    /// If they are not connected to the internet, if there is an error connecting, or they would be redirected, store their data for the level in a file and send it next time they are connected.
    /// </summary>
    /// <param name="temp"></param>
    /// <param name="accurateElapsed"></param>
    /// <param name="score"></param>
    /// <param name="levelDataEndpoint"></param>
    /// <returns></returns>
    IEnumerator CheckConnectivity(int temp, float accurateElapsed, int score, string levelDataEndpoint)
    {
        // We are going to attempt to look at this site, which just has the text 'Microsoft NCSI' on it. 
        // If the text retrieved later in the function is equal to this, then we know the player is connected to the internet properly and can send their data to the server.
        WWW www = new WWW("http://www.msftncsi.com/ncsi.txt");

        // Wait until the page is finished downloading.
        while (www.isDone == false)
        {
            yield return false;
        }

        int hasConsented = PlayerPrefs.GetInt("Consented", -1);
        if (hasConsented != -1)
        {
            if (hasConsented == 1)
            {
                print("Has consented to sending their data to the server for research.");

                // If an error occcurs while loading the page, or if the player is not connected to the internet via WiFi or cell service.
                if (www.error != null)
                {
                    print("Text: " + www.text);
                    print("Error: " + www.error);
                    print("Could not connect to the Internet.");

                    // string storedFilepath = Application.persistentDataPath + "storeddata" + curLevel;
                    string storedFilepath = Path.Combine(Application.persistentDataPath, (Application.productName + "storeddata" + curLevel.ToString()));

                    int directoryEnd = 0;
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
                    directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab") + 10;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBPLAYER
                    directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab.Echoadventure") + 24;
#endif
#if UNITY_IOS && !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBPLAYER
                    directoryEnd = Application.persistentDataPath.IndexOf("Documents") + 8;
#endif
                    print("Directory End: " + directoryEnd.ToString());
                    string searchPath = Application.persistentDataPath.Substring(0, (directoryEnd + 1));
                    print("Search Path: " + searchPath);
                    string searchPattern = Application.productName + "storeddata";
                    print("Search Pattern: " + searchPattern);

                    DirectoryInfo directory = new DirectoryInfo(searchPath);
                    FileInfo[] files = directory.GetFiles();
                    string tempFilepath = storedFilepath;
                    string tempTempFilepath = tempFilepath;
                    int sameFilepathIndex = 2;
                    string storedFilename = "";

                    foreach (FileInfo file in files)
                    {
                        if (file.Name.Contains("storeddata") == true)
                        {
                            print("File: " + file.Name);

                            if (file.Name.Equals(tempFilepath) == true)
                            {
                                tempTempFilepath = storedFilepath + "_" + sameFilepathIndex.ToString();
                                tempFilepath = tempTempFilepath;
                                storedFilename = file.Name;
                                sameFilepathIndex += 1;
                                print("New Filepath: " + tempTempFilepath);
                                print("Next Index: " + sameFilepathIndex.ToString());
                            }
                        }
                    }

                    storedFilepath = tempFilepath;
                    print("Final Path: " + storedFilepath);

                    string[] levelData = new string[12];

                    levelData[0] = Utilities.encrypt(SystemInfo.deviceUniqueIdentifier);
                    levelData[1] = Utilities.encrypt(curLevel.ToString());
                    levelData[2] = Utilities.encrypt(temp.ToString());
                    levelData[3] = Utilities.encrypt(numCrashes.ToString());
                    levelData[4] = Utilities.encrypt(numSteps.ToString());
                    levelData[5] = Utilities.encrypt(accurateElapsed.ToString("F3"));
                    levelData[6] = Utilities.encrypt(echoNum.ToString());
                    levelData[7] = Utilities.encrypt(startTime.ToString());
                    levelData[8] = Utilities.encrypt(endTime.ToString());
                    levelData[9] = Utilities.encrypt(exitAttempts.ToString());
                    levelData[10] = Utilities.encrypt(GameManager.instance.boardScript.asciiLevelRep);
                    levelData[11] = GameManager.instance.boardScript.gamerecord;

                    System.IO.File.WriteAllLines(storedFilepath, levelData);
                    print("Stored data in local file to send later.");

                    debugPlayerInfo = "Error connecting to the Internet. Storing data to file: " + storedFilename;
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.            
                }
                // If no errors occur in loading the page.
                else
                {
                    print("Text: " + www.text);

                    // If the player can go to the msftncsi.com site without being redirected.
                    if (www.text == "Microsoft NCSI")
                    {
                        debugPlayerInfo = "Connected to the Internet.";

                        // Since the player is connected to the internet, send any data from levels where they were not connected to the internet to the server.
                        SendStoredData(levelDataEndpoint);

                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        print("Connected to the Internet.");

                        // Send the crash count data and level information to server
                        WWWForm levelCompleteForm = new WWWForm();
                        levelCompleteForm.AddField("userName", Utilities.encrypt(SystemInfo.deviceUniqueIdentifier));
                        levelCompleteForm.AddField("currentLevel", Utilities.encrypt(curLevel.ToString()));
                        levelCompleteForm.AddField("trackCount", Utilities.encrypt(temp.ToString()));
                        levelCompleteForm.AddField("crashCount", Utilities.encrypt(numCrashes.ToString()));
                        levelCompleteForm.AddField("stepCount", Utilities.encrypt(numSteps.ToString()));
                        levelCompleteForm.AddField("timeElapsed", Utilities.encrypt(accurateElapsed.ToString("F3")));
                        levelCompleteForm.AddField("totalEchoes", Utilities.encrypt(echoNum.ToString()));
                        levelCompleteForm.AddField("startTime", Utilities.encrypt(startTime.ToString()));
                        levelCompleteForm.AddField("endTime", Utilities.encrypt(endTime.ToString()));                        
                        levelCompleteForm.AddField("exitAttempts", Utilities.encrypt(exitAttempts.ToString()));
                        levelCompleteForm.AddField("asciiLevelRep", Utilities.encrypt(GameManager.instance.boardScript.asciiLevelRep));
                        levelCompleteForm.AddField("levelRecord", (GameManager.instance.boardScript.gamerecord));                        

                        // Logging.Log(System.Text.Encoding.ASCII.GetString(levelCompleteForm.data), Logging.LogLevel.LOW_PRIORITY);

                        // Send the name of the echo files used in this level and the counts
                        // form.AddField("echoFileNames", getEchoNames());

                        // Send the details of the crash locations
                        // form.AddField("crashLocations", crashLocs);

                        // levelCompleteForm.AddField("score", score);

                        WWW www2 = new WWW(levelDataEndpoint, levelCompleteForm);

                        print("Sending the data to the server.");
                        StartCoroutine(Utilities.WaitForRequest(www2));

                    }
                    // If the player could connect to the internet, but would be directed to another page.
                    else
                    {
                        debugPlayerInfo = "Connected to the internet, but redirected to another page.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        print("Connected to the Internet, but got redirected to another page.");

                        // string storedFilepath = Application.persistentDataPath + "storeddata" + curLevel;
                        string storedFilepath = Path.Combine(Application.persistentDataPath, (Application.productName + "storeddata" + curLevel.ToString()));

                        int directoryEnd = 0;
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
                        directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab") + 10;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBPLAYER
                        directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab.Echoadventure") + 24;
#endif
#if UNITY_IOS && !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBPLAYER
                        directoryEnd = Application.persistentDataPath.IndexOf("Documents") + 8;
#endif
                        print("Directory End: " + directoryEnd.ToString());
                        string searchPath = Application.persistentDataPath.Substring(0, (directoryEnd + 1));
                        print("Search Path: " + searchPath);
                        string searchPattern = Application.productName + "storeddata";
                        print("Search Pattern: " + searchPattern);

                        DirectoryInfo directory = new DirectoryInfo(searchPath);
                        FileInfo[] files = directory.GetFiles();
                        string tempFilepath = storedFilepath;
                        string tempTempFilepath = tempFilepath;
                        int sameFilepathIndex = 2;
                        string storedFilename = "";

                        foreach (FileInfo file in files)
                        {
                            if (file.Name.Contains("storeddata") == true)
                            {
                                print("File: " + file.Name);

                                if (file.Name.Equals(tempFilepath) == true)
                                {
                                    tempTempFilepath = storedFilepath + "_" + sameFilepathIndex.ToString();
                                    tempFilepath = tempTempFilepath;
                                    storedFilename = file.Name;
                                    sameFilepathIndex += 1;
                                    print("New Filepath: " + tempTempFilepath);
                                    print("Next Index: " + sameFilepathIndex.ToString());
                                }
                            }
                        }

                        storedFilepath = tempFilepath;
                        print("Final Path: " + storedFilepath);

                        string[] levelData = new string[12];

                        levelData[0] = Utilities.encrypt(SystemInfo.deviceUniqueIdentifier);
                        levelData[1] = Utilities.encrypt(curLevel.ToString());
                        levelData[2] = Utilities.encrypt(temp.ToString());
                        levelData[3] = Utilities.encrypt(numCrashes.ToString());
                        levelData[4] = Utilities.encrypt(numSteps.ToString());
                        levelData[5] = Utilities.encrypt(accurateElapsed.ToString("F3"));
                        levelData[6] = Utilities.encrypt(echoNum.ToString());
                        levelData[7] = Utilities.encrypt(startTime.ToString());
                        levelData[8] = Utilities.encrypt(endTime.ToString());
                        levelData[9] = Utilities.encrypt(exitAttempts.ToString());
                        levelData[10] = Utilities.encrypt(GameManager.instance.boardScript.asciiLevelRep);
                        levelData[11] = GameManager.instance.boardScript.gamerecord;

                        System.IO.File.WriteAllLines(storedFilepath, levelData);
                        print("Stored data in local file to send later.");

                        debugPlayerInfo = "Error connecting to the Internet. Storing data to file: " + storedFilename;
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.    
                    }
                }
            }
            else if (hasConsented == 0)
            {
                print("Has not consented to sending their data to the server for research.");
            }
        }
    }

    /// <summary>
    /// Sends the stored data from levels that were completed.
    /// </summary>
    /// <param name="temp"></param>
    /// <param name="accurateElapsed"></param>
    /// <param name="score"></param>
    /// <param name="levelDataEndpoint"></param>
    void SendStoredData(string levelDataEndpoint)
    {
        int directoryEnd = 0;
#if (UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR)
        directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab") + 10;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBPLAYER
        directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab.Echoadventure") + 24;
#endif
#if UNITY_IOS && !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBPLAYER
        directoryEnd = Application.persistentDataPath.IndexOf("Documents") + 8;
#endif
        print("Directory End: " + directoryEnd.ToString());
        string searchPath = Application.persistentDataPath.Substring(0, (directoryEnd + 1));
        print("Search Path: " + searchPath);
        string searchPattern = Application.productName + "storeddata";
        print("Search Pattern: " + searchPattern);

        DirectoryInfo directory = new DirectoryInfo(searchPath);
        FileInfo[] files = directory.GetFiles();

        foreach (FileInfo file in files)
        {
            if (file.Name.Contains("storeddata") == true)
            {
                print("Stored Data File Found: " + file.Name);

                // If the file exists.
                if (System.IO.File.Exists(file.FullName))
                {
                    debugPlayerInfo += "Stored: " + file.Name + ", ";

                    // Read the lines of the file.
                    print("Reading lines of stored data file.");
                    string[] svdata_split = System.IO.File.ReadAllLines(file.FullName);

                    // Send the crash count data and level information to server
                    WWWForm levelCompleteForm = new WWWForm();
                    levelCompleteForm.AddField("userName", svdata_split[0]);
                    levelCompleteForm.AddField("currentLevel", svdata_split[1]);
                    levelCompleteForm.AddField("trackCount", svdata_split[2]);
                    levelCompleteForm.AddField("crashCount", svdata_split[3]);
                    levelCompleteForm.AddField("stepCount", svdata_split[4]);
                    levelCompleteForm.AddField("timeElapsed", svdata_split[5]);
                    levelCompleteForm.AddField("totalEchoes", svdata_split[6]);
                    levelCompleteForm.AddField("startTime", svdata_split[7]);
                    levelCompleteForm.AddField("endTime", svdata_split[8]);
                    levelCompleteForm.AddField("exitAttempts", svdata_split[9]);
                    levelCompleteForm.AddField("asciiLevelRep", svdata_split[10]);
                    levelCompleteForm.AddField("levelRecord", svdata_split[11]);

                    // Logging.Log(System.Text.Encoding.ASCII.GetString(levelCompleteForm.data), Logging.LogLevel.LOW_PRIORITY);

                    // Send the name of the echo files used in this level and the counts
                    // form.AddField("echoFileNames", getEchoNames());

                    // Send the details of the crash locations
                    // form.AddField("crashLocations", crashLocs);

                    // levelCompleteForm.AddField("score", Int32.Parse(svdata_split[11]));

                    WWW www2 = new WWW(levelDataEndpoint, levelCompleteForm);

                    print("Sending stored data to the server.");
                    StartCoroutine(Utilities.WaitForRequest(www2, file.Name, file.FullName));
                }
            }
        }
    }
}
