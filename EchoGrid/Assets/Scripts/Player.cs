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
using System.Diagnostics;
using UnityEngine.SceneManagement;

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

    bool restarted = false;
    bool is_freezed; // is player not allowed to do anything?
    bool tapped; // did player tap to hear an echo at this position?
    bool reportSent;
    private int curLevel;

    int cur_clip = 0;
    int max_quit_clip = 2;
    bool reset_audio;
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

    // Usage data to keep track of
    public static bool want_exit = false;
    bool survey_shown = false;
    bool URL_shown = false;
    bool code_entered = false;
    bool survey_activated = true;
    bool at_pause_menu = false; // indicating if the player activated pause menu
    bool localRecordWritten = false;
    // int score;
    eventHandler eh;

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

    public bool intercepted = false;

    List<AudioClip> clips;

    bool madeUnrecognizedGesture = false;

    bool finishedTappingInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to make a tap.
    bool finishedSwipingInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to make a swipe.
    bool finishedMenuInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to open or close the pause menu.
    bool finishedExitingInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to exit a level.
    public static bool finishedCornerInstruction = false; // Make sure the player cannot make a gesture we we are explaining to tap at a corner.
    bool finishedTurningInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to make a rotation.

    bool waitingForOpenMenuInstruction = false; // Make sure the player cannot make gestures while we are explaining what you can do in the pause menu in the gesture tutorial.
    bool wantLevelRestart = false; // Used to make sure the player has to tap once after swiping left in the pause menu to confirm they want to restart the level.
    bool wantMainMenu = false; // Used to make sure the player has to tap once after swiping right in the pause menu to confirm they want to go to the main menu.

    public static bool endingLevel;

    bool hasMoved = false;
    float ratio;

    bool swipedUp;
    bool rotatedLeft;
    bool rotatedRight;

    public static bool[,] canPlayClip = new bool[12, 7];
    public static bool playedExitClip;
    public static bool canGoToNextLevel;

    public static bool hasTappedAtCorner = false;

    public static bool reachedExit = false;

    bool restartLevel = false;
    bool goBackToMain = false;

    public static bool changingLevel = false;

    public static bool loadingScene = true;

    bool haveTappedThreeTimes = false;
    bool haveSwipedThreeTimes = false;

    public int level1_remaining_taps = -1; // Gesture tutorial level 1 remaining taps. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
    public int level1_remaining_ups = -1; // Gesture tutorial level 1 remaining swipes up. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
    public int level1_remaining_menus = -1; // Gesture tutorial level 1 remaining holds for pause menu. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
    public int level3_remaining_turns = -1; // Gesture tutorial level 3 remaining turns/rotations. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
    enum InterceptAction { NONE, UP, DOWN, LEFTSWIPE, RIGHTSWIPE, LEFTROTATE, RIGHTROTATE, TAP, MENU };
    string prefix = "C00";
    string surveyCode = "";

    AudioClip attenuatedClick = Database.attenuatedClick;
    AudioClip echofront = Database.hrtf_front;
	AudioClip echoleft_leftspeaker = Database.hrtf_left_leftspeaker;
	AudioClip echoleft_rightspeaker = Database.hrtf_left_rightspeaker;
    AudioClip echoright = Database.hrtf_right;
    AudioClip echoleftfront = Database.hrtf_leftfront;
    AudioClip echorightfront = Database.hrtf_rightfront;

    bool canCheckForConsent = false;
    bool hasCheckedForConsent = false;
    public static bool hasFinishedConsentForm = false;

    bool canRepeat = true;

    public static bool hasStartedConsent = false;

    bool finished_reading = false;
    bool finished_listening = false;
    bool finished_questions = false;
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
	GameObject dummyobject;

    bool canSendGameData = false;
    // bool switch_click_toggle = false; // If switch_click_toggle is false, then play the odeon click, which is option 1 in database.

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
        surveyCode = "";
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
        reset_audio = false;
        tapped = false;
        reportSent = false;
        survey_shown = false;
        URL_shown = false;
        code_entered = false;
        survey_activated = true;
        echoLock = false;
        ad = GetComponent<AndroidDialogue>();

        endingLevel = false;

        swipedUp = false;
        rotatedLeft = false;
        rotatedRight = false;
        playedExitClip = false;
        canGoToNextLevel = false;

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
        TriggerechoTimer = new CDTimer(Const.echoCD, InputModule.instance);
        TriggermenuTimer = new CDTimer(Const.menuUpdateCD, InputModule.instance);
        TriggerrotateTimer = new CDTimer(Const.rotateGestCD, InputModule.instance);
        TriggerechoTimer.TakeDownTime();
        TriggermenuTimer.TakeDownTime();
        TriggerrotateTimer.TakeDownTime();
        base.Start();
    }

    protected override void Start()
    {
        curLevel = GameManager.instance.level;
        init();
        // Adjust player scale
        Vector3 new_scale = transform.localScale;
        new_scale *= (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;
        transform.localScale = new_scale;
    }

    void OnLevelWasLoaded(int index)
    {
        // Since the gameObject is not destroyed automatically, the instance should be checked before calling this method.
        if (this != instance)
        {
            return;
        }
        init();
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

    private IEnumerator DelayedEcho(GvrAudioSource gas, float delay)
    {
        yield return new WaitForSeconds(delay);
        gas.Play();
    }

    // A breakdown of short, medium and long distances
    string[] frontDistS = { "2.25", "3.75" };
    string[] frontDistM = { "5.25", "6.75" };
    string[] frontDistL = { "8.25", "9.75", "11.25", "12.75" };
    /// <summary>
    /// A function that determines which echo file to play based on the surrounding environment.
    /// </summary>
    private void PlayEcho(bool real = true)
    {
        if (GM_title.switch_click_toggle == true)
        {
            attenuatedClick = Database.attenuatedClick;
            echofront = Database.hrtf_front;
			echoleft_leftspeaker = Database.hrtf_left_leftspeaker;
            echoright = Database.hrtf_right;
            echoleftfront = Database.hrtf_leftfront;
            echorightfront = Database.hrtf_rightfront;
        }

        if (GM_title.switch_click_toggle == false)
        {
            attenuatedClick = Database.attenuatedClick;
            echofront = Database.hrtf_front;
			echoleft_leftspeaker = Database.hrtf_left_leftspeaker;
            echoright = Database.hrtf_right;
            echoleftfront = Database.hrtf_leftfront;
            echorightfront = Database.hrtf_rightfront;

        }

        Vector3 dir = transform.right;
        int dir_x = (int)Math.Round(dir.x);
        int dir_y = (int)Math.Round(dir.y);
        int x = (int)transform.position.x;
        int y = (int)transform.position.y;
        //print("Position: " + transform.position);/////
        //print("Rotation: " + transform.rotation);/////
        //print("Forward: " + transform.forward);/////
        //print("Right: " + transform.right);/////
        //print("Up: " + transform.up);/////
        //print("Facing: " + dir_x + ", " + dir_y);

        GameObject frontWall, leftWall, rightWall, leftFrontWall, rightFrontWall, tempWall, rightEndWall, leftEndWall, leftTwoFrontWall, rightTwoFrontWall;
		GameObject dummyleft=null;
		//leftWall = GameObject.Find("Wall_" + x + "_" + y);
       
		//rightWall = GameObject.Find("Wall_" + x + "_" + y);
        // assume dir.x != 0
        leftWall = GameObject.Find("Wall_" + (x + dir_y) + "_" + (y + dir_x));
        rightWall = GameObject.Find("Wall_" + (x + -dir_y) + "_" + (y + -dir_x));
        leftFrontWall = GameObject.Find("Wall_" + (x + dir_y + dir_x) + "_" + (y + dir_x + dir_y));
        rightFrontWall = GameObject.Find("Wall_" + (x + -dir_y + dir_x) + "_" + (y + -dir_x + dir_y));

        leftEndWall = null;
        rightEndWall = null;
        leftTwoFrontWall = GameObject.Find("Wall_" + (x + 2 * dir_y + dir_x) + "_" + (y + 2 * dir_x + dir_y));
        rightTwoFrontWall = GameObject.Find("Wall_" + (x + (-dir_y * 2) + dir_x) + "_" + (y + (-dir_x * 2) + dir_y));
        int stepsize = 1;
        if (leftWall == null)
        {
            stepsize = 1;
            while (leftEndWall == null)
            {
                leftEndWall = GameObject.Find("Wall_" + (x + dir_y * stepsize) + "_" + (y + dir_x * stepsize));
                stepsize += 1;
            }
        }
        if (rightWall == null)
        {
            stepsize = 1;
            while (rightEndWall == null)
            {
                rightEndWall = GameObject.Find("Wall_" + (x + (-dir_y * stepsize)) + "_" + (y + (-dir_x * stepsize)));
                stepsize += 1;
            }
        }

        if (dir.y != 0)
        {
            tempWall = leftWall;
            leftWall = rightWall;
            rightWall = tempWall;
            tempWall = leftFrontWall;
            leftFrontWall = rightFrontWall;
            rightFrontWall = tempWall;
            tempWall = leftEndWall;
            leftEndWall = rightEndWall;
            rightEndWall = tempWall;
            tempWall = leftTwoFrontWall;
            leftTwoFrontWall = rightTwoFrontWall;
            rightTwoFrontWall = tempWall;
        }
        do
        {
            x += dir_x;
            y += dir_y;
            frontWall = GameObject.Find("Wall_" + x + "_" + y);
        }
        while (frontWall == null);
        // Player echo preparation
        GvrAudioSource playerGAS = this.GetComponent<GvrAudioSource>();
        playerGAS.clip = attenuatedClick;
        // Front wall echo preparation
        GvrAudioSource frontGAS = frontWall.GetComponent<GvrAudioSource>();
        frontGAS.clip = echofront;
        float blocksToFrontWall = Vector3.Distance(transform.position, frontWall.transform.position) - 1;
        // Four-wall echoes preparation
        GvrAudioSource leftGAS = null, rightGAS = null, leftFrontGAS = null, rightFrontGAS = null, leftEndGAS = null, rightEndGAS = null, leftTwoFrontGAS = null, rightTwoFrontGAS = null;
		GvrAudioSource leftGAS_right = null, rightGAS_left = null, leftFrontGAS_rightfront = null, rightfrontGAS_leftfront = null, leftEndGAS_rightend = null, rightEndGAS_leftend = null;
        float horizontal_45db = -5.3f;
        float horizontaldb = 5.3f;
        float frontwalldb = 10.3f;
        float farenddb = -5.3f;
        //float fourblockdb =-13.7f;
        //float frontwalldb = -5.7f;

        if (leftWall != null)
		{	
			if (dummyleft == null) {
				dummyleft = GameObject.Instantiate (dummyobject, new Vector3 ((x + -dir_y), (y + -dir_x), 0), Quaternion.identity);
			} else {
				dummyleft.transform.position = new Vector3 ((x + -dir_y), (y + -dir_x), 0);
			}

            leftGAS = leftWall.GetComponent<GvrAudioSource>();
			leftGAS.clip = echoleft_leftspeaker;
            leftGAS.gainDb = horizontaldb;
			leftGAS_right=dummyleft.GetComponent<GvrAudioSource>();
			leftGAS_right.clip=echoleft_rightspeaker;
			leftGAS_right.gainDb = horizontaldb;
			leftGAS_right.PlayDelayed(1.5f / 340);

        }
        else
        {
            //Left end wall if at left corner
            leftEndGAS = leftEndWall.GetComponent<GvrAudioSource>();
			leftEndGAS.clip = echoleft_leftspeaker;
            leftEndGAS.gainDb = farenddb;
        }
        if (rightWall != null)
        {
            rightGAS = rightWall.GetComponent<GvrAudioSource>();
            rightGAS.clip = echoright;
            rightGAS.gainDb = horizontaldb;
        }
        else
        {
            //Right end wall if at right corner
            rightEndGAS = rightEndWall.GetComponent<GvrAudioSource>();
            rightEndGAS.clip = echoright;
            rightEndGAS.gainDb = farenddb;
        }

        if (blocksToFrontWall > 0 && leftFrontWall != null && leftWall != null)
        {

            leftFrontGAS = leftFrontWall.GetComponent<GvrAudioSource>();
            leftFrontGAS.clip = echoleftfront;
            leftFrontGAS.gainDb = horizontal_45db;

        }

        if (blocksToFrontWall == 0 && leftTwoFrontWall != null && leftWall == null)
        {
            //Right two and front one block
            leftTwoFrontGAS = leftTwoFrontWall.GetComponent<GvrAudioSource>();
            leftTwoFrontGAS.clip = echoleftfront;
            leftTwoFrontGAS.gainDb = horizontal_45db;
        }

        if (blocksToFrontWall > 0 && rightFrontWall != null && rightWall != null)
        {

            rightFrontGAS = rightFrontWall.GetComponent<GvrAudioSource>();
            rightFrontGAS.clip = echorightfront;
            rightFrontGAS.gainDb = horizontal_45db;

        }


        if (blocksToFrontWall == 0 && rightTwoFrontWall != null && rightWall == null)
        {
            //Right two and front one block
            rightTwoFrontGAS = rightTwoFrontWall.GetComponent<GvrAudioSource>();
            rightTwoFrontGAS.clip = echorightfront;
            rightTwoFrontGAS.gainDb = horizontal_45db;
        }


        // Play all echoes
        // The SoundManager would be interrupted by GVR, Use GVR or Coroutine to avoid this.
        //playerGAS.Play();
        if (!real)
        {
            if (frontGAS != null) frontGAS.DummyInit();
            if (leftGAS != null) leftGAS.DummyInit();
            if (rightGAS != null) rightGAS.DummyInit();
            if (leftFrontGAS != null) leftFrontGAS.DummyInit();
            if (rightFrontGAS != null) rightFrontGAS.DummyInit();
            if (leftEndGAS != null) leftEndGAS.DummyInit();
            if (rightEndGAS != null) rightEndGAS.DummyInit();
            if (leftTwoFrontGAS != null) leftTwoFrontGAS.DummyInit();
            if (rightTwoFrontGAS != null) rightTwoFrontGAS.DummyInit();
            return;
        }
		/*
        SoundManager.instance.PlaySingle(attenuatedClick);
		*/

        if (leftGAS != null)
        {
            //leftGAS.PlayDelayed(1.5f / 340);
			leftGAS_right.PlayDelayed(1.5f / 340);
            UnityEngine.Debug.Log("left palyed!");
        }
        /*
        if (rightGAS != null)
        {
            rightGAS.PlayDelayed(1.5f / 340);
            UnityEngine.Debug.Log("right palyed!");
        }
        if (blocksToFrontWall > 0 && leftFrontGAS != null)
        {
            leftFrontGAS.PlayDelayed(2.12132f / 340);
            UnityEngine.Debug.Log("leftfront palyed!");
        }
        if (blocksToFrontWall > 0 && rightFrontGAS != null)
        {
            rightFrontGAS.PlayDelayed(2.12132f / 340);
            UnityEngine.Debug.Log("rightfront palyed!");
        }

        if (blocksToFrontWall == 0 && leftEndGAS != null)
        {
            leftEndGAS.PlayDelayed((1.5f * stepsize) / 340);
            UnityEngine.Debug.Log("Left End is " + stepsize + " blocks away!");
        }
        if (blocksToFrontWall == 0 && rightEndGAS != null)
        {
            rightEndGAS.PlayDelayed((1.5f * stepsize) / 340);
            UnityEngine.Debug.Log("Right End is " + stepsize + " blocks away!");
        }

        /*
		if (leftTwoFrontGAS != null)
		{
			leftTwoFrontGAS.PlayDelayed(3.1f / 340);
			UnityEngine.Debug.Log ("Left Front End is played");
		}
		if (rightTwoFrontGAS != null)
		{
			rightTwoFrontGAS.PlayDelayed(3.1f / 340);
			UnityEngine.Debug.Log ("Right Front End is played");
		}*/
		/*
        if (frontGAS != null)
        {
            frontGAS.gainDb = frontwalldb;
            frontGAS.PlayDelayed((1.5f * blocksToFrontWall + 0.75f) * 2 / 340);
            UnityEngine.Debug.Log("frontwall palyed!");
        }
		*/

        return;
        tapped = true;
        reportSent = true;
        BoardManager.echoDistData data = GameManager.instance.boardScript.getEchoDistData(transform.position, get_player_dir("FRONT"), get_player_dir("LEFT"));

        // Logging.Log(data.all_jun_to_string(), Logging.LogLevel.NORMAL);
        if ((GameManager.instance.level >= 17) && (GameManager.instance.level < 22))
        {
            horizontaldb = 1.3f;
            frontwalldb = 9.3f;
        }
        else if ((GameManager.instance.level >= 22) && (GameManager.instance.level < 27))
        {
            horizontaldb = 0.3f;
            frontwalldb = 8.3f;
        }
        else if ((GameManager.instance.level >= 27) && (GameManager.instance.level < 32))
        {
            horizontaldb = -0.7f;
            frontwalldb = 7.3f;
        }
        else if ((GameManager.instance.level >= 32) && (GameManager.instance.level < 37))
        {
            horizontaldb = -1.7f;
            frontwalldb = 6.3f;
        }
        else if ((GameManager.instance.level >= 37) && (GameManager.instance.level < 42))
        {
            horizontaldb = -2.7f;
            frontwalldb = 5.3f;
        }
        else if ((GameManager.instance.level >= 42) && (GameManager.instance.level < 47))
        {
            horizontaldb = -3.7f;
            frontwalldb = 4.3f;
        }
        else if ((GameManager.instance.level >= 47) && (GameManager.instance.level < 52))
        {
            horizontaldb = -4.7f;
            frontwalldb = 3.3f;
        }
        else if ((GameManager.instance.level >= 52) && (GameManager.instance.level < 57))
        {
            horizontaldb = -5.7f;
            frontwalldb = 2.3f;
        }
        else if ((GameManager.instance.level >= 57) && (GameManager.instance.level < 62))
        {
            horizontaldb = -6.7f;
            frontwalldb = 1.3f;
        }
        else if ((GameManager.instance.level >= 62) && (GameManager.instance.level < 67))
        {
            horizontaldb = -7.7f;
            frontwalldb = 0.3f;
        }
        else if ((GameManager.instance.level >= 67) && (GameManager.instance.level < 72))
        {
            horizontaldb = -8.7f;
            frontwalldb = -0.7f;
        }
        else if ((GameManager.instance.level >= 72) && (GameManager.instance.level < 77))
        {
            horizontaldb = -9.7f;
            frontwalldb = -1.7f;
        }
        else if ((GameManager.instance.level >= 77) && (GameManager.instance.level < 82))
        {
            horizontaldb = -10.7f;
            frontwalldb = -2.7f;
        }
        else if ((GameManager.instance.level >= 82) && (GameManager.instance.level < 87))
        {
            horizontaldb = -11.7f;
            frontwalldb = -3.7f;
        }
        else if ((GameManager.instance.level >= 87) && (GameManager.instance.level < 92))
        {
            horizontaldb = -12.7f;
            frontwalldb = -4.7f;
        }
        else if ((GameManager.instance.level >= 92))
        {
            horizontaldb = -13.7f;
            frontwalldb = -5.7f;
        }
    }

    string post_act = "";
    string correct_post_act = "";

    /// <summary>
    /// Reports data when an echo is requested.
    /// The function is actually called during other actions after an echo was played.
    /// </summary>
	private void reportOnEcho()
    {
        string echoEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptEchoData.py";

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
        echoForm.AddField("echo", lastEcho); // fix
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
            clip = Database.hintClips[0];
        }
        else if (forward == -sol_dir)
        {
            clip = Database.hintClips[4];
        }
        else
        {
            Vector3 angle = Vector3.Cross(forward, sol_dir);
            if (angle.z > 0)
            {
                clip = Database.hintClips[1];
            }
            else
            {
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
    // get the direction in world space
    /*
	Vector3 get_world_dir(string dir)
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

		UnityEngine.Debug.Log ("INVALID direction string");
		return Vector3.zero;
	}
	*/

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

    bool reachHalf = false;
    bool reachQuarter = false;
    bool reach3Quarter = false;

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
                if (reportSent)
                {
                    post_act = "Turn ";
                    if ((dir - get_player_dir("LEFT")).magnitude <= 0.01f)
                    {
                        post_act += "Left";
                    }
                    else
                    {
                        post_act += "Right";
                    }

                    reportOnEcho();
                    reportSent = false;
                }
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
                tapped = false;
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

            //if ((!reachQuarter) && (ratio <= 0.75f) && (ratio > 0.67f))
            //{
            //    reachQuarter = true;
            // if (GameManager.instance.boardScript.latest_clip != null)
            // {
            //	  GameManager.instance.boardScript.restore_audio = true;
            //	  GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
            // }
            // SoundManager.instance.PlayVoice (Resources.Load ("instructions/You are 25% of the way through this level") as AudioClip);
            //}

            //else if ((!reach3Quarter) && (ratio < 0.5f))
            //{
            // reach3Quarter = true;
            // if (GameManager.instance.boardScript.latest_clip != null)
            // {
            //     GameManager.instance.boardScript.restore_audio = true;
            //     GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
            // }
            // SoundManager.instance.PlayVoice (Resources.Load ("instructions/You are 75% of the way through this level") as AudioClip);
            // print ("75%");
            //}
        }
    }

    /// <summary>
    /// Determines whether the previous action was a tap (as a request for an echo).
    /// </summary>
	public bool tapped_at_this_block()
    {
        return tapped;
    }

    protected override bool AttemptMove<T>(int xDir, int yDir)
    {
        // Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
        bool canMove = base.AttemptMove<T>(xDir, yDir);
        numSteps += 1;
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[37] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If they swipe up here and they are facing the wall, tell them they have crashed into the wall and that they have to rotate left to progress further in the level.
                else if ((curLevel == 5) && (playerPos.x == 1) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.LEFT))
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[38] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                else if ((canPlayClip[(curLevel - 1), 2] == false) && (BoardManager.reachedExit == true))
                {
                    // If we are on top of the exit and still in the tutorial.
                    clips = new List<AudioClip>() { Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[36] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                else
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                }
            }
            else if (curLevel >= 12)
            {
                clips = new List<AudioClip>() { Database.soundEffectClips[8] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
            }
            // SoundManager.instance.playcrash(Database.soundEffectClips[8]);
            // Increment the crash count
            numCrashes++;
            // Decrement the step count (as no successful step was made)
            reportOnCrash(); // send crash report

            // Add the crash location details
            string loc = transform.position.x.ToString() + "," + transform.position.y.ToString();
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
        string crashEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptCrashData.py";

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
        // Increment step count
        // numSteps += 1;
        exitAttempts++;

        BoardManager.echoDistData data = GameManager.instance.boardScript.getEchoDistData(transform.position, get_player_dir("FRONT"), get_player_dir("LEFT"));

        float wallDist = 0.8f;
        // catogrize the distance
        // front
        // if ((data.frontDist <= wallDist) && (data.leftDist <= wallDist) && (data.rightDist <= wallDist))
        // {
        //     Logging.Log("Not exit, in Wrong Dead end!", Logging.LogLevel.LOW_PRIORITY);
        // }

        if (BoardManager.reachedExit == true)
        {
            reachedExit = true;
            // Calculate time elapsed during the game level
            endLevel();
        }
        else
        {
            SoundManager.instance.PlayVoice(Database.mainGameClips[35], true, 0.0f, 0.0f, 0.5f); // Tell the player that they are not currently at the exit.
        }

        if (reportSent)
        {
            post_act = "Exit";
            reportOnEcho();
            reportSent = false;
        }
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
        string levelDataEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptLevelData.py";
        int temp = GameManager.instance.boardScript.local_stats[curLevel];

        // Check if we are connected to the internet.
        CheckInternetConnection(temp, accurateElapsed, score, levelDataEndpoint);       

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
        restarted = true;
        Invoke("Restart", restartLevelDelay);
        // Disable the player object since level is over.
        // enabled = true;

        GameManager.instance.level += 1;
        GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
        GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
        GameMode.write_save_mode(GameManager.instance.level, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
        GameManager.instance.playersTurn = false;

        // AudioSource.PlayClipAtPoint (winSound, transform.localPosition, 1.0f);

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
    /// Unused function related to sending survey data.
    /// </summary>
	private void reportsurvey(string code)
    {
        string echoEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptSurvey.py";

        WWWForm echoForm = new WWWForm();
        echoForm.AddField("userName", Utilities.encrypt(SystemInfo.deviceUniqueIdentifier));
        echoForm.AddField("surveyID", Utilities.encrypt(code));
        echoForm.AddField("dateTimeStamp", Utilities.encrypt(System.DateTime.Now.ToString()));
        // the code is the first digit of device id

        // Logging.Log(System.Text.Encoding.ASCII.GetString(echoForm.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www = new WWW(echoEndpoint, echoForm);
        StartCoroutine(Utilities.WaitForRequest(www));
    }

    /// <summary>
    /// Plays an instruction voice related to the menu.
    /// </summary>
	void play_audio()
    {
        if ((madeUnrecognizedGesture == true) && (SoundManager.instance.finishedClip == true))
        {
            madeUnrecognizedGesture = false;
            int i = 0;
            bool canPlayInterruptedClip = true;
            print("Interrupted clips:");
            foreach (AudioClip clip in SoundManager.clipsCurrentlyPlaying)
            {
                print("Clip " + i + ": " + clip.name);
                if ((i == 0) && ((clip.name == "swipe-ahead") || (clip.name == "swipe-left") || (clip.name == "swipe-right") || (clip.name == "level_start") || (clip.name == "pause_menu_closed")))
                {
                    canPlayInterruptedClip = false;
                }
                i++;
            }
            if (canPlayInterruptedClip == true)
            {
                SoundManager.instance.PlayClips(SoundManager.clipsCurrentlyPlaying, SoundManager.currentBalances, 0, SoundManager.currentCallback, SoundManager.currentCallbackIndex, 0.5f, true);
            }
        }

        if ((((curLevel == 1) && (finishedExitingInstruction == true) && (BoardManager.finishedTutorialLevel1 == true)) || (curLevel == 12)) && (hasFinishedConsentForm == false))
        {
            if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false) && (finished_reading == false) && (hasFinishedConsentForm == false) && (hasStartedConsent == false))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.consentClips[0] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    hasStartedConsent = true;
                    hearingConsentForm = false;
                    readingConsentForm = false;
                    noConsent = false;
                    finished_listening = false;
                }
            }

            if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false) && (finished_reading == false) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.consentClips[0] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    hearingConsentForm = false;
                    readingConsentForm = false;
                    noConsent = false;
                    finished_listening = false;
                }
            }

            if ((hearingConsentForm == true) && (answeredQuestion1 == false) && (finished_listening == false))
            {
                if (SoundManager.instance.finishedAllClips == true)
                {
                    finished_listening = true;
                    debugPlayerInfo = "Finished listening to consent form and consent question 1.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }

            else if ((hearingConsentForm == true) && (answeredQuestion1 == false) && (finished_listening == true))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[2] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
            }

            if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false) && (finished_listening == true))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[3] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
            }

            if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false) && (finished_listening == true))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[4] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                }
            }

            if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                {
                    canRepeat = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[5] };
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
        }

        Vector2 playerPos = BoardManager.player_idx;
        Vector2 startPos = BoardManager.start_idx;
        Vector2 exitPos = BoardManager.exit_idx;

        if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
        {
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
                            if ((canPlayClip[0, 1] == true) && (intercepted == false) && (BoardManager.finishedTutorialLevel1 == true) && (ratio <= 0.5f) && (ratio >= 0.332f))
                            {
                                canPlayClip[0, 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                                                                                                      // Logging.Log("50%", Logging.LogLevel.NORMAL);
                            }
                            // If the player has reached the exit at level 1.
                            else if ((canPlayClip[0, 2] == true) && (intercepted == false) && (BoardManager.finishedTutorialLevel1 == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[0, 3] == true) && (intercepted == false) && (BoardManager.finishedTutorialLevel1 == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {

                            }
                            // Otherwise play a normal swipe up.
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                            }
                        }
                        // If we are in level 2.
                        else if (curLevel == 2)
                        {
                            // If the player has reached halfway.
                            if ((canPlayClip[1, 1] == true) && (ratio <= 0.5f) && (ratio >= 0.332f))
                            {
                                canPlayClip[1, 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                                                                                                      // Logging.Log("50%", Logging.LogLevel.NORMAL);
                            }
                            // If the player has reached the exit at level 2.
                            else if ((canPlayClip[1, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[1, 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {

                            }
                            // Otherwise play a normal swipe up.
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                            }
                        }
                        // If we are in level 3.
                        else if (curLevel == 3)
                        {
                            if ((canPlayClip[2, 4] == false) && (intercepted == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 6) && (playerPos.y == 9))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                            }
                            // Play clip for when they approach the corner in level 3.
                            else if ((canPlayClip[2, 4] == true) && (intercepted == false) && (playerPos.x == 6) && (playerPos.y == 9))
                            {
                                canPlayClip[2, 4] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[14] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                            }
                            else if ((canPlayClip[2, 5] == false) && (canPlayClip[2, 6] == false) && (intercepted == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                            }
                            // Play clip for when the player reaches the corner in level 3.
                            else if ((canPlayClip[2, 5] == true) && (canPlayClip[2, 6] == true) && (intercepted == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                canPlayClip[2, 5] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[15], Database.mainGameClips[16] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                            }
                            // Allow the player to tap once to see what the echo at a right turn sounds like before starting the gesture tutorial.
                            else if ((canPlayClip[2, 5] == true) && (intercepted == false) && (BoardManager.finishedTutorialLevel3 == false) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                canPlayClip[2, 5] = false;
                            }
                            // If the player has reached halfway.
                            else if ((canPlayClip[2, 1] == true) && (intercepted == false) && (BoardManager.finishedTutorialLevel3 == true) && (ratio <= 0.5f) && (ratio >= 0.4665f))
                            {
                                canPlayClip[2, 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                                                                                                      // Logging.Log("50%", Logging.LogLevel.NORMAL);
                            }
                            // If the player has reached the exit at level 3.
                            else if ((canPlayClip[2, 2] == true) && (intercepted == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[2, 3] == true) && (intercepted == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {

                            }
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[2, 1] = true;
                                }
                                if ((playerPos.x != startPos.x) && (playerPos.y != startPos.y) && (BoardManager.left_start_pt == true))
                                {
                                    canPlayClip[2, 3] = true;
                                }
                            }
                        }
                        // If we are in level 5.
                        else if (curLevel == 5)
                        {
                            if ((canPlayClip[4, 4] == false) && (playerPos.x <= 4) && (playerPos.x > 2) && (playerPos.y == 9))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                            }
                            // Play clip for when they approach the corner in level 5.
                            else if ((canPlayClip[4, 4] == true) && (playerPos.x == 4) && (playerPos.y == 9))
                            {
                                canPlayClip[4, 4] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[14] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                            }
                            // If the player has reached the corner in level 5.
                            else if ((canPlayClip[4, 5] == true) && (playerPos.x == 1) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.LEFT))
                            {
                                canPlayClip[4, 5] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[21] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                            }
                            // If the player has reached halfway.
                            else if ((canPlayClip[4, 1] == true) && (ratio <= 0.5f) && (ratio >= 0.4465f))
                            {
                                canPlayClip[4, 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                                                                                                      // Logging.Log("50%", Logging.LogLevel.NORMAL);
                            }
                            // If the player has reached the exit at level 5.
                            else if ((canPlayClip[4, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[4, 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {

                            }
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[4, 1] = true;
                                }
                                if ((playerPos.x != startPos.x) && (playerPos.y != startPos.y) && (BoardManager.left_start_pt == true))
                                {
                                    canPlayClip[4, 3] = true;
                                }
                            }
                        }
                        // If we are in level 11.
                        else if (curLevel == 11)
                        {
                            if ((canPlayClip[10, 4] == false) && (playerPos.x == 5) && (playerPos.y == 6) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.FRONT))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                            }
                            else if ((canPlayClip[10, 4] == false) && (playerPos.x >= 2) && (playerPos.y <= 6))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[10, 1] = true;
                                }
                                if ((playerPos.x != startPos.x) && (playerPos.y != startPos.y) && (BoardManager.left_start_pt == true))
                                {
                                    canPlayClip[10, 3] = true;
                                }
                            }
                            // If the player has reached halfway.
                            else if ((canPlayClip[10, 1] == false) && (ratio <= 0.5f) && (ratio >= 0.4665f) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.FRONT))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                                                                                                      // Logging.Log("50%", Logging.LogLevel.NORMAL);
                            }
                            // Play clips for when player hits the T intersection in level 11.
                            else if ((canPlayClip[10, 4] == true) && (playerPos.x == 5) && (playerPos.y == 6) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.FRONT))
                            {
                                if (canPlayClip[10, 1] == true)
                                {
                                    canPlayClip[10, 1] = false;
                                }
                                canPlayClip[10, 4] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[33], Database.soundEffectClips[0], Database.mainGameClips[28] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                            }
                            else if ((canPlayClip[10, 4] == true) && (playerPos.x >= 2) && (playerPos.y <= 6))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[10, 1] = true;
                                }
                                if ((playerPos.x != startPos.x) && (playerPos.y != startPos.y) && (BoardManager.left_start_pt == true))
                                {
                                    canPlayClip[10, 3] = true;
                                }
                            }
                            // If the player has reached halfway.
                            else if ((canPlayClip[10, 1] == false) && (ratio <= 0.5f) && (ratio >= 0.4665f) && (GameManager.instance.boardScript.get_player_dir_world() != BoardManager.Direction.FRONT))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                                                                                                      // Logging.Log("50%", Logging.LogLevel.NORMAL);
                            }
                            // If the player has reached the exit at level 11.
                            else if ((canPlayClip[10, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[10, 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {

                            }
                        }
                        // If we are in another tutorial level
                        else
                        {
                            // If the player has reached halfway.
                            if ((canPlayClip[(curLevel - 1), 1] == true) && (ratio <= 0.5f) && (ratio >= 0.4665f))
                            {
                                canPlayClip[(curLevel - 1), 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[33] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                                                                                                      // Logging.Log("50%", Logging.LogLevel.NORMAL);
                            }
                            // If the player has reached the exit on another level.
                            else if ((canPlayClip[(curLevel - 1), 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[(curLevel - 1), 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {

                            }
                            // Otherwise play a normal swipe up.
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[(curLevel - 1), 1] = true;
                                }
                                if ((playerPos.x != startPos.x) && (playerPos.y != startPos.y) && (BoardManager.left_start_pt == true))
                                {
                                    canPlayClip[(curLevel - 1), 3] = true;
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
                            // Logging.Log("50%", Logging.LogLevel.NORMAL);
                            canPlayClip[10, 2] = true;
                            canPlayClip[10, 3] = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                        }
                        // If the player has reached the exit.
                        else if ((canPlayClip[11, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                        {
                            // Keep this check in, but do nothing.
                            canPlayClip[10, 1] = true;
                            canPlayClip[10, 3] = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                        }
                        // If the player returns to their start position.
                        else if ((canPlayClip[11, 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                        {
                            canPlayClip[11, 3] = false;
                            canPlayClip[10, 1] = true;
                            canPlayClip[10, 2] = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                        }
                        else
                        {
                            canPlayClip[10, 1] = true;
                            canPlayClip[10, 2] = true;
                            canPlayClip[10, 3] = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                        }
                        canPlayClip[10, 4] = true;
                        canPlayClip[10, 5] = true;
                        canPlayClip[10, 6] = true;
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
                        clips = new List<AudioClip>() { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.mainGameClips[17], Database.soundEffectClips[0], Database.mainGameClips[18] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        rotatedLeft = false;
                    }
                }

                // Play clips for when the player has rotated around the corner in level 5 to move towards the exit.
                else if ((curLevel == 5) && (canPlayClip[4, 6] == true) && (playerPos.x == 1) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.BACK))
                {
                    if (hasMoved == true)
                    {
                        canPlayClip[4, 6] = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.mainGameClips[17], Database.soundEffectClips[0], Database.mainGameClips[18] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        rotatedLeft = false;
                    }
                }

                else
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[5] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    rotatedLeft = false;
                }

                if (BoardManager.gotBackToStart == false)
                {
                    debugPlayerInfo = "Rotated left. Rotated player left 90 degrees. XPos = " + BoardManager.player_idx.x.ToString() + ", YPos = " + BoardManager.player_idx.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }

            if (rotatedRight == true)
            {
                Vector2 level3Corner = new Vector2(9, 9);
                Vector2 level5Corner = new Vector2(1, 9);

                // Play clips for when the player has finished the gesture tutorial at level 3 and has rotated around the corner to move towards the exit.
                if ((curLevel == 3) && (canPlayClip[2, 6] == true) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 9) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.BACK))
                {
                    if (hasMoved == true)
                    {
                        clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.mainGameClips[17], Database.soundEffectClips[0], Database.mainGameClips[18] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
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
                        clips = new List<AudioClip>() { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.mainGameClips[17], Database.soundEffectClips[0], Database.mainGameClips[18] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        rotatedRight = false;
                    }
                }

                else
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[6] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    rotatedRight = false;
                }

                if (BoardManager.gotBackToStart == false)
                {
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
                if ((canPlayClip[0, 0] == true) && (intercepted == false) && (BoardManager.finishedTutorialLevel1 == true))
                {
                    if (((SoundManager.instance.finishedAllClips == true) || (canRepeat == true)) && (canCheckForConsent == false) && (hasCheckedForConsent == false))
                    {
                        canRepeat = false;
                        debugPlayerInfo = "Playing level 1 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.levelStartClips[1], Database.levelStartClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.

                        canCheckForConsent = true;
                    }
                    else if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true) && (hasCheckedForConsent == true))
                    {
                        canPlayClip[0, 0] = false;
                        hasCheckedForConsent = false;

                        debugPlayerInfo = "Playing level 1 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);

                        clips = new List<AudioClip>() { Database.soundEffectClips[2], Database.mainGameClips[0], Database.soundEffectClips[0], Database.mainGameClips[1], Database.soundEffectClips[0], Database.mainGameClips[2], Database.soundEffectClips[0], Database.mainGameClips[3], Database.soundEffectClips[0], Database.soundEffectClips[8], Database.soundEffectClips[0], Database.mainGameClips[4], Database.soundEffectClips[0], Database.mainGameClips[5], Database.soundEffectClips[0], Database.mainGameClips[6], Database.soundEffectClips[0], Database.mainGameClips[7], Database.soundEffectClips[0], Database.mainGameClips[8] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                    }
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[0, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                    canPlayClip[1, 0] = false;
                    debugPlayerInfo = "Playing level 2 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[2], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[10], Database.soundEffectClips[0], Database.mainGameClips[7], Database.soundEffectClips[0], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[1, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                if ((canPlayClip[2, 0] == true) && (intercepted == false))
                {
                    canPlayClip[2, 0] = false;
                    debugPlayerInfo = "Playing level 3 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[3], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[12], Database.soundEffectClips[0], Database.mainGameClips[13], Database.soundEffectClips[0], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[2, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                    canPlayClip[3, 0] = false;
                    debugPlayerInfo = "Playing level 4 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[4], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[19], Database.soundEffectClips[0], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[3, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                    canPlayClip[4, 0] = false;
                    debugPlayerInfo = "Playing level 5 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[5], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[20], Database.soundEffectClips[0], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[4, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                    canPlayClip[5, 0] = false;
                    debugPlayerInfo = "Playing level 6 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[6], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[5, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                    canPlayClip[6, 0] = false;
                    debugPlayerInfo = "Playing level 7 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[7], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[23], Database.soundEffectClips[0], Database.mainGameClips[24], Database.soundEffectClips[0], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[6, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                    canPlayClip[7, 0] = false;
                    debugPlayerInfo = "Playing level 8 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[8], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[7, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                    canPlayClip[8, 0] = false;
                    debugPlayerInfo = "Playing level 9 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[9], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[8, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                    canPlayClip[9, 0] = false;
                    debugPlayerInfo = "Playing level 10 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[10], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[9, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
                    canPlayClip[10, 0] = false;
                    debugPlayerInfo = "Playing level 11 beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                    clips = new List<AudioClip>() { Database.levelStartClips[11], Database.levelStartClips[0], Database.soundEffectClips[2], Database.mainGameClips[25], Database.soundEffectClips[0], Database.mainGameClips[26], Database.soundEffectClips[0], Database.mainGameClips[27], Database.soundEffectClips[0], Database.mainGameClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                }
                // If the player is not at the exit and swiped down.
                if ((canPlayClip[10, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
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
            // Play level 11 beginning clips.
            if (canPlayClip[11, 0] == true)
            {
                canPlayClip[11, 0] = false;

                debugPlayerInfo = "Playing level " + curLevel + " beginning clips. XPos = " + playerPos.x.ToString() + ", YPos = " + playerPos.y.ToString();
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                clips = new List<AudioClip>() { Database.levelStartClips[curLevel], Database.levelStartClips[0] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.

                if (curLevel == 12)
                {
                    canCheckForConsent = true;
                }
            }
            if ((canPlayClip[11, 0] == false) && (hasCheckedForConsent == true))
            {
                hasCheckedForConsent = false;
            }
            // If the player is not at the exit and swiped down.
            if ((canPlayClip[11, 2] == true) && (endingLevel == true) && ((SoundManager.instance.finishedAllClips == true) || ((playerPos.x == startPos.x) && (playerPos.y == startPos.y))))
            {
                endingLevel = false;
                debugPlayerInfo = "Swiped down. Attempting to exit level.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                attemptExitFromLevel(); // Attempt to exit the level.
            }
        }
    }

    //control
    bool echoLock = false;
    CDTimer TriggerechoTimer;
    CDTimer TriggermenuTimer;
    CDTimer TriggerrotateTimer;

    void Update()
    {
        PlayEcho(false);
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
                if (get_player_dir("BACK") == BoardManager.startDir)
                {
                    debugPlayerInfo = "Returned to start position. Turn around, then continue by moving forward.";
                }
                else if (get_player_dir("FRONT") == BoardManager.startDir)
                {
                    debugPlayerInfo = "Returned to start position. Continue by moving forward.";
                }
                else if (get_player_dir("LEFT") == BoardManager.startDir)
                {
                    debugPlayerInfo = "Returned to start position. Turn left, then continue by moving forward.";
                }
                else if (get_player_dir("RIGHT") == BoardManager.startDir)
                {
                    debugPlayerInfo = "Returned to start position. Turn right, then continue by moving forward.";
                }
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
            }
            else if ((curLevel >= 12) && (canPlayClip[11, 3] == true))
            {
                canPlayClip[11, 3] = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
            }
        }

        else if (BoardManager.gotBackToStart == false)
        {
            if (curLevel <= 11)
            {
                canPlayClip[(curLevel - 1), 3] = true;
            }
            else if (curLevel >= 12)
            {
                canPlayClip[11, 3] = true;
            }
        }

        if (BoardManager.reachedExit == true)
        {
            // If the player has reached the exit and not swiped down, play the appropriate exit clip.
            if ((endingLevel == false) && (playedExitClip == false))
            {
                if ((curLevel <= 11) && (canPlayClip[(curLevel - 1), 2] == true))
                {
                    if (curLevel == 1)
                    {
                        canPlayClip[0, 2] = false;
                        debugPlayerInfo = "Playing found level 1 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[9] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                    }
                    else if (curLevel == 2)
                    {
                        canPlayClip[1, 2] = false;
                        debugPlayerInfo = "Playing found level 2 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[9] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                    }
                    else if (curLevel == 3)
                    {
                        canPlayClip[2, 2] = false;
                        debugPlayerInfo = "Playing found level 3 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[11] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                    }
                    else if (curLevel == 11)
                    {
                        canPlayClip[10, 2] = false;
                        debugPlayerInfo = "Playing found level 11 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[29] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                    }
                    else
                    {
                        canPlayClip[(curLevel - 1), 2] = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                    }
                }
                else if ((curLevel >= 12) && (canPlayClip[11, 2] == true))
                {
                    canPlayClip[11, 2] = false;
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
            }

            // If the exit level sound has finished playing.
            if ((playedExitClip == true) && (canGoToNextLevel == false))
            {
                debugPlayerInfo = "Exiting level clip has finished.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                canGoToNextLevel = true;
            }

            // If the player is at the exit and the exit level sound has played.
            if ((playedExitClip == true) && (canGoToNextLevel == true) && (SoundManager.instance.finishedAllClips == true))
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

        if ((curLevel == 1) || (curLevel == 12))
        {
            if (curLevel == 1)
            {
                if (canCheckForConsent == true)
                {
                    canCheckForConsent = false;

                    string filename = Application.persistentDataPath + "consentRecord";
                    string[] svdata_split;

                    if (System.IO.File.Exists(filename))
                    {
                        svdata_split = System.IO.File.ReadAllLines(filename);
                        int hasConsented = Int32.Parse(svdata_split[0]);

                        if (hasConsented == 1)
                        {
                            hasFinishedConsentForm = true;
                        }
                        else if (hasConsented == 0)
                        {
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
            }
            else if ((curLevel == 12) && (canPlayClip[11, 0] == false) && (SoundManager.instance.finishedAllClips == true))
            {
                if (canCheckForConsent == true)
                {
                    canCheckForConsent = false;

                    string filename = Application.persistentDataPath + "consentRecord";
                    string[] svdata_split;

                    if (System.IO.File.Exists(filename))
                    {
                        svdata_split = System.IO.File.ReadAllLines(filename);
                        int hasConsented = Int32.Parse(svdata_split[0]);

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
            }
        }

        play_audio();

#if (UNITY_IOS || UNITY_ANDROID) && (!UNITY_STANDALONE || !UNITY_WEBPLAYER)
        if ((curLevel == 12) && (hasFinishedConsentForm == false))
        {
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readProcedures == false) && (proceduresFlag == true) && (ad.noclicked() == true))
            {
                proceduresFlag = false;
                readConsent = false;
                consentFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRequirements == false) && (requirementsFlag == true) && (ad.noclicked() == true))
            {
                requirementsFlag = false;
                readProcedures = false;
                proceduresFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRisks == false) && (risksFlag == true) && (ad.noclicked() == true))
            {
                risksFlag = false;
                readRequirements = false;
                requirementsFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readBenefits == false) && (benefitsFlag == true) && (ad.noclicked() == true))
            {
                benefitsFlag = false;
                readRisks = false;
                risksFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readCompCost == false) && (readCompCost == true) && (ad.noclicked() == true))
            {
                compCostFlag = false;
                readBenefits = false;
                benefitsFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConfidentiality == false) && (confidentialityFlag == true) && (ad.noclicked() == true))
            {
                confidentialityFlag = false;
                readCompCost = false;
                compCostFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == false) && (questionsContactFlag == true) && (ad.noclicked() == true))
            {
                questionsContactFlag = false;
                readConfidentiality = false;
                confidentialityFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readEighteenPlus == false) && (eighteenPlusFlag == true) && (ad.noclicked() == true))
            {
                readEighteenPlus = true;
                answeredQuestion1 = true;
                question1 = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readUnderstand == false) && (understandFlag == true) && (ad.noclicked() == true))
            {
                readUnderstand = true;
                answeredQuestion2 = true;
                question2 = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
        }
#endif

        // UnityEngine.Debug.DrawLine (transform.position, transform.position+get_player_dir("FRONT"), Color.green);
        // UnityEngine.Debug.DrawLine (transform.position, transform.position+get_player_dir("LEFT"), Color.yellow);
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

        if (intercepted && eh.isActivate())
        {
            // Make sure all the clips have finished playing before allowing the player to tap.
            if ((finishedTappingInstruction == false) && (level1_remaining_taps == 3) && (SoundManager.instance.finishedAllClips == true))
            {
                debugPlayerInfo = "Finished tapping instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedTappingInstruction = true; // We have finished the tapping instruction, so the player can tap.
            }
            if ((finishedSwipingInstruction == false) && (level1_remaining_taps == 0) && (level1_remaining_ups == 3) && (SoundManager.instance.finishedAllClips == true) && (haveTappedThreeTimes == true))
            {
                debugPlayerInfo = "Finished swiping instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedSwipingInstruction = true; // We have finished the swiping instruction, so the player can swipe up.
            }
            // Make sure all the clips have finished playing before allowing the player to swipe.
            if ((finishedSwipingInstruction == false) && (level1_remaining_taps == 0) && (level1_remaining_ups == 3) && (SoundManager.instance.finishedAllClips == true) && (haveTappedThreeTimes == false))
            {
                haveTappedThreeTimes = true;
            }
            // Make sure all the clips have finished playing before allowing the player to open the pause menu.
            if ((finishedMenuInstruction == false) && (level1_remaining_ups == 0) && (level1_remaining_menus == 2) && (SoundManager.instance.finishedAllClips == true) && (haveSwipedThreeTimes == true))
            {
                debugPlayerInfo = "Finished open pause menu instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedMenuInstruction = true; // We have finished the first pause menu instruction, so the player can open the pause menu.
            }
            // Make sure all the clips have finished playing before allowing the player to hold.
            if ((finishedMenuInstruction == false) && (level1_remaining_ups == 0) && (level1_remaining_menus == 2) && (SoundManager.instance.finishedAllClips == true) && (haveSwipedThreeTimes == false))
            {
                haveSwipedThreeTimes = true;
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

            if ((finishedExitingInstruction == true) && (BoardManager.finishedTutorialLevel1 == true) && (hasStartedConsent == false) && (hasFinishedConsentForm == false) && (SoundManager.instance.finishedAllClips == true))
            {
                if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false) && (finished_reading == false))
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.consentClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        hasStartedConsent = true;
                        hearingConsentForm = false;
                        readingConsentForm = false;
                        noConsent = false;
                        finished_listening = false;
                    }
                }
            }

            else if ((finishedExitingInstruction == true) && (BoardManager.finishedTutorialLevel1 == true) && (hasStartedConsent == true))
            {
#if (UNITY_IOS || UNITY_ANDROID) && (!UNITY_STANDALONE || !UNITY_WEBPLAYER)
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readProcedures == false) && (proceduresFlag == true) && (ad.noclicked() == true))
                {
                    proceduresFlag = false;
                    readConsent = false;
                    consentFlag = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRequirements == false) && (requirementsFlag == true) && (ad.noclicked() == true))
                {
                    requirementsFlag = false;
                    readProcedures = false;
                    proceduresFlag = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRisks == false) && (risksFlag == true) && (ad.noclicked() == true))
                {
                    risksFlag = false;
                    readRequirements = false;
                    requirementsFlag = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readBenefits == false) && (benefitsFlag == true) && (ad.noclicked() == true))
                {
                    benefitsFlag = false;
                    readRisks = false;
                    risksFlag = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readCompCost == false) && (readCompCost == true) && (ad.noclicked() == true))
                {
                    compCostFlag = false;
                    readBenefits = false;
                    benefitsFlag = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConfidentiality == false) && (confidentialityFlag == true) && (ad.noclicked() == true))
                {
                    confidentialityFlag = false;
                    readCompCost = false;
                    compCostFlag = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == false) && (questionsContactFlag == true) && (ad.noclicked() == true))
                {
                    questionsContactFlag = false;
                    readConfidentiality = false;
                    confidentialityFlag = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readEighteenPlus == false) && (eighteenPlusFlag == true) && (ad.noclicked() == true))
                {
                    readEighteenPlus = true;
                    answeredQuestion1 = true;
                    question1 = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readUnderstand == false) && (understandFlag == true) && (ad.noclicked() == true))
                {
                    readUnderstand = true;
                    answeredQuestion2 = true;
                    question2 = false;
                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
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
                    canRepeat = true;
                    ad.clearflag();
                }

                if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readParticipate == false) && (participateFlag == true) && (ad.noclicked() == true))
                {
                    readParticipate = true;
                    answeredQuestion3 = true;
                    question3 = false;
                    android_window_displayed = false;
                    finished_reading = true;
                    canRepeat = true;
                    ad.clearflag();
                }
#endif
                if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false) && (finished_reading == false))
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.consentClips[0] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        hearingConsentForm = false;
                        readingConsentForm = false;
                        noConsent = false;
                        finished_listening = false;
                    }
                }

                if ((hearingConsentForm == true) && (answeredQuestion1 == false) && (finished_listening == false))
                {
                    if (SoundManager.instance.finishedAllClips == true)
                    {
                        finished_listening = true;
                        debugPlayerInfo = "Finished listening to consent form and consent question 1.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }

                else if ((hearingConsentForm == true) && (answeredQuestion1 == false) && (finished_listening == true))
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[2] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                }

                if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false) && (finished_listening == true))
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[3] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                }

                if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false) && (finished_listening == true))
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[4] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                }

                if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[5] };
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
            }

            if ((intercepted == true) && (finishedCornerInstruction == false) && (level3_remaining_turns == 4) && (SoundManager.instance.finishedAllClips == true))
            {
                debugPlayerInfo = "Finished corner instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedCornerInstruction = true; // We have finished the corner instruction, so the player can tap.
            }
            if ((hasTappedAtCorner == true) && (finishedTurningInstruction == false) && (level3_remaining_turns == 4) && (SoundManager.instance.finishedAllClips == true))
            {
                debugPlayerInfo = "Finished turning instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedTurningInstruction = true; // We have finished the turning instruction, so the player can rotate.
            }

            if (!InterceptMission(eh.getEventData()))
            {
                return;
            }
        }

        Vector3 dir = Vector3.zero;
        // Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        // Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction.
        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

            Vector2 playerPos = BoardManager.player_idx;

            // Do something based on this event info.
            // If a tap was registered.
            if (ie.isTap == true)
            {
                if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                {
                    // If the player is not in the pause menu, play an echo.
                    if ((want_exit == false) && (loadingScene == false))
                    {
                        if ((finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                        {

                        }
                        else
                        {
                            debugPlayerInfo = "Tap registered. Played echo.";
                        }
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        GameManager.instance.boardScript.gamerecord += "E{"; // Record the echo.
                        PlayEcho(); // Play the echo.
                        GameManager.instance.boardScript.gamerecord += lastEcho;
                        GameManager.instance.boardScript.gamerecord += "}";
                    }
                    else if ((want_exit == true) && (loadingScene == false))
                    {
                        // If the player has told us they want to restart the level, then restart the level.
                        if (wantLevelRestart == true)
                        {
                            debugPlayerInfo = "Tap registered. Restarting current level.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                            clips = new List<AudioClip>() { Database.pauseMenuClips[11] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // should have another set of sound effect
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
                                                                                         // SceneManager.UnloadScene("Main");
                            clips = new List<AudioClip>() { Database.pauseMenuClips[11] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // should have another set of sound effect
                            GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                            GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                            GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
                            goBackToMain = true;
                        }
                    }
                }
                else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
                {
                    if (noConsent == true)
                    {
                        Utilities.writefile("consentRecord", "0");
                        debugPlayerInfo = "Tap registered. Did not consent to having data collected. Can continue with level 12.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        hasFinishedConsentForm = true;
                        canRepeat = true;
                        clips = new List<AudioClip>() { Database.consentClips[8] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
                    {
                        Utilities.writefile("consentRecord", "1");
                        debugPlayerInfo = "Tap registered. Consented to having data collected. Can continue with level 12.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        hasFinishedConsentForm = true;
                        canRepeat = true;
                        clips = new List<AudioClip>() { Database.consentClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    else
                    {
                        debugPlayerInfo = "Tap registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
            }
            // If a swipe was registered.
            else if (ie.isSwipe == true)
            {
                // If the left arrow key has been pressed.
                if (ie.isLeft == true)
                {
                    if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                    {
                        // If the player is in the pause menu, they have told us they want to restart the level.
                        if ((want_exit == true) && (loadingScene == false))
                        {
                            debugPlayerInfo = "Swiped left. We want to restart the level.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[6] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[5] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                            }
                            wantLevelRestart = true;
                        }
                    }
                    else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
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
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                        {
                            question1 = false;
                            answeredQuestion1 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Is not eighteen.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                        {
                            question2 = false;
                            answeredQuestion2 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Did not understand information.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                        {
                            question3 = false;
                            answeredQuestion3 = true;
                            canRepeat = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe left registered. Does not want to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                        {
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
                            answeredQuestion1 = false;
                            answeredQuestion2 = false;
                            answeredQuestion3 = false;
                            question1 = false;
                            question2 = false;
                            question3 = false;
                            canRepeat = true;
                        }
                        else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                        {
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
                            answeredQuestion1 = false;
                            answeredQuestion2 = false;
                            answeredQuestion3 = false;
                            question1 = false;
                            question2 = false;
                            question3 = false;
                            canRepeat = true;
                        }
                        else if (noConsent == true)
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = false;
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
                            answeredQuestion1 = false;
                            answeredQuestion2 = false;
                            answeredQuestion3 = false;
                            question1 = false;
                            question2 = false;
                            question3 = false;
                            canRepeat = true;
                        }
                    }
                }
                // If the right arrow key has been pressed.
                else if (ie.isRight == true)
                {
                    if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                    {
                        // If the player is in the pause menu, they have told us they want to go back to the main menu.
                        if ((want_exit == true) && (loadingScene == false))
                        {
                            debugPlayerInfo = "Swiped right. We want to return to the main menu.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[10] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[9] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                            }
                            wantMainMenu = true;
                        }
                    }
                    else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        if ((readingConsentForm == false) && (hearingConsentForm == false) && (noConsent == false))
                        {
                            readingConsentForm = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Reading consent form manually.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                            finished_reading = true;
                            answeredQuestion1 = true;
                            question1 = true;
                            answeredQuestion2 = true;
                            question2 = true;
                            answeredQuestion3 = true;
                            question3 = true;
                            finished_questions = true;
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                        {
                            question1 = true;
                            answeredQuestion1 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Is eighteen.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                        {
                            question2 = true;
                            answeredQuestion2 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Understood information.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                        {
                            question3 = true;
                            answeredQuestion3 = true;
                            canRepeat = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe right registered. Wants to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[5] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                        {
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
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
                        else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                        {
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
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
                        else if (noConsent == true)
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = false;
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
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
                }
                // If the up arrow key has been pressed.
                else if (ie.isUp == true)
                {
                    if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                    {
                        // If the player is not in the pause menu, move them forward.
                        if ((want_exit == false) && (loadingScene == false))
                        {
                            if ((intercepted == false) && (BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x == 9) && (playerPos.y == 9))
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
                            debugPlayerInfo = "Swiped up. Gave player hint";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            getHint(); // Give the player a hint.
                        }
                    }
                    else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        debugPlayerInfo = "Swipe up registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
                // If the down arrow key has been pressed.
                else if (ie.isDown == true)
                {
                    if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
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
                    else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                        {
                            noConsent = true;
                            canRepeat = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.consentClips[6] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        }
                        else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true))
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = true;
                            canRepeat = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.consentClips[6] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
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
                    if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                    {
                        // If the player is not in the pause menu, rotate them 90 degrees to the left.
                        if ((want_exit == false) && (loadingScene == false))
                        {
                            if ((intercepted == false) && (BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x <= 9) && (playerPos.y == 9))
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
                    else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        debugPlayerInfo = "Left rotation registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
                // If the right arrow key has been pressed.
                else if (ie.isRight == true)
                {
                    if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                    {
                        // If the player is not in the pause menu, rotate them 90 degrees to the right.
                        if ((want_exit == false) && (loadingScene == false))
                        {
                            if ((BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x <= 9) && (playerPos.y == 9))
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
                    else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
                    {
                        debugPlayerInfo = "Right rotation registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
            }
            // If a hold was registered.
            else if (ie.isHold == true)
            {
                if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                {
                    // If the player is not in the pause menu, open the pause menu.
                    if ((want_exit == false) && (loadingScene == false))
                    {
                        debugPlayerInfo = "Hold registered. Opened pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        if (SoundManager.instance.voiceSource.isPlaying)
                        {
                            GameManager.instance.boardScript.restore_audio = true;
                            GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
                        }
                        // If the player is using Talkback.
                        if (GM_title.isUsingTalkback == true)
                        {
                            clips = new List<AudioClip>() { Database.pauseMenuClips[0], Database.pauseMenuClips[2], Database.pauseMenuClips[4], Database.pauseMenuClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        }
                        // If the player is not using Talkback.
                        else if (GM_title.isUsingTalkback == false)
                        {
                            clips = new List<AudioClip>() { Database.pauseMenuClips[0], Database.pauseMenuClips[1], Database.pauseMenuClips[3], Database.pauseMenuClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        }
                        want_exit = true; // Open the pause menu.
                        reset_audio = false;
                    }
                    // If the player is in the pause menu, close the pause menu.
                    else if ((want_exit == true) && (loadingScene == false))
                    {
                        debugPlayerInfo = "Hold registered. Closed pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        clips = new List<AudioClip>() { Database.pauseMenuClips[11] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        want_exit = false; // Close the pause menu.
                        reset_audio = true;
                    }
                }
                else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                // SceneManager.UnloadScene("Main");
                loadingScene = true;
                Destroy(GameObject.Find("GameManager"));
                SceneManager.LoadScene("Title_Screen"); // Move to the main menu.
            }

            // If there was an unrecognized gesture made.
            if ((ie.isUnrecognized == true) && (loadingScene == false))
            {
                madeUnrecognizedGesture = true;

                // If this error was registered.
                if (ie.isTapHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isTapVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isTapRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe left.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[3], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe right.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[4], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[5], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe down.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[6], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe left.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe right.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe down.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isRotationAngleError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with angle on rotation.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[8], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[9], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[10], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[11], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
            }
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    // pop up the survey at the end of tutorial
        if ((BoardManager.reachedExit == true) && survey_activated)
        {
			if ((GameManager.instance.level == 11) && !survey_shown)
            {
				ad.clearflag();
				ad.DisplayAndroidWindow("Survey", "Would you like to take \n a short survey about the game?");
				survey_shown = true;
			}

			if (survey_shown && !URL_shown && ad.yesclicked() && !code_entered)
            {
				// display a code, and submit it reportSurvey()
				// Please enter code (first six digits of UDID) on the survey page
				code_entered = true;

                if (SystemInfo.deviceUniqueIdentifier.Length <= 6)
                {
                    surveyCode = SystemInfo.deviceUniqueIdentifier;
                }
                else
                {
                    surveyCode = SystemInfo.deviceUniqueIdentifier.Substring(0, 6);
                }
                string codemsg = "Your survey code is: \n" + surveyCode + "\n please enter this in the survey.";
				ad.clearflag();
				ad.DisplayAndroidWindow("Survey Code", codemsg, AndroidDialogue.DialogueType.YESONLY);
			}
            else if (!URL_shown && ad.yesclicked() && code_entered)
            {
				URL_shown = true;
				Application.OpenURL("https://echolock.andrew.cmu.edu/survey/"); // "http://echolock.andrew.cmu.edu/survey/?"
			}
            else if (URL_shown)
            {
				ad.clearflag();
				ad.DisplayAndroidWindow("Thank You", "Thank you for taking the survey!", AndroidDialogue.DialogueType.YESONLY);
				reportsurvey(surveyCode);
				survey_activated = false;
			}
		}
#endif

        // Check if we are running on iOS or Android
#if UNITY_IOS || UNITY_ANDROID
        // process input
        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

            Vector2 playerPos = BoardManager.player_idx;

            // If a tap is registered.
            if (ie.isTap == true)
            {
                if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                {
                    // If the player is not in the pause menu, play an echo if possible.
                    if ((at_pause_menu == false) && (loadingScene == false))
                    {
                        if ((finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                        {

                        }
                        else
                        {
                            if (!echoLock)
                            {
                                debugPlayerInfo = "Tap registered. Played echo.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                GameManager.instance.boardScript.gamerecord += "E{"; // Record the echo.
                                PlayEcho(); // Play the echo.
                                GameManager.instance.boardScript.gamerecord += lastEcho;
                                GameManager.instance.boardScript.gamerecord += "}";
                                flipEchoLock(true);
                            }
                        }
                    }
                    // If the player is at the pause menu.
                    else if ((at_pause_menu == true) && (loadingScene == false))
                    {
                        // If the player has told us they want to restart the level, then restart the level.
                        if (wantLevelRestart == true)
                        {
                            debugPlayerInfo = "Tap registered. Restarting current level.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                            clips = new List<AudioClip>() { Database.pauseMenuClips[11] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // should have another set of sound effect
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
                                                                                         // SceneManager.UnloadScene("Main");
                            clips = new List<AudioClip>() { Database.pauseMenuClips[11] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // should have another set of sound effect
                            GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                            GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                            GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
                            goBackToMain = true;
                        }
                    }
                }
                else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
                {
                    if (noConsent == true)
                    {
                        Utilities.writefile("consentRecord", "0");
                        debugPlayerInfo = "Tap registered. Did not consent to having data collected. Can continue with level 12.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        hasFinishedConsentForm = true;
                        canRepeat = true;
                        clips = new List<AudioClip>() { Database.consentClips[8] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
                    {
                        Utilities.writefile("consentRecord", "1");
                        debugPlayerInfo = "Tap registered. Consented to having data collected. Can continue with level 12.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        hasFinishedConsentForm = true;
                        canRepeat = true;
                        clips = new List<AudioClip>() { Database.consentClips[7] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                    else
                    {
                        debugPlayerInfo = "Tap registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
            }
            // If a hold is registered.
            else if (ie.isHold == true)
            {
                if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                {
                    flipEchoLock(true);
                    // If the player is not in the pause menu, open the pause menu.
                    if ((at_pause_menu == false) && (loadingScene == false))
                    {
                        debugPlayerInfo = "Hold registered. Opened pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        at_pause_menu = true; // The player is now in the pause menu.
                        if (SoundManager.instance.voiceSource.isPlaying)
                        {
                            GameManager.instance.boardScript.restore_audio = true;
                            GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
                        }
                        // If the player is using Talkback.
                        if (GM_title.isUsingTalkback == true)
                        {
                            clips = new List<AudioClip>() { Database.pauseMenuClips[0], Database.pauseMenuClips[2], Database.pauseMenuClips[4], Database.pauseMenuClips[8] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        }
                        // If the player is not using Talkback.
                        else if (GM_title.isUsingTalkback == false)
                        {
                            clips = new List<AudioClip>() { Database.pauseMenuClips[0], Database.pauseMenuClips[1], Database.pauseMenuClips[3], Database.pauseMenuClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clips.
                        }
                    }
                    // If the player is in the pause menu, close the pause menu.
                    else if ((at_pause_menu == true) && (loadingScene == false))
                    {
                        debugPlayerInfo = "Hold registered. Closed pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        at_pause_menu = false; // The player is no longer in the pause menu.
                        clips = new List<AudioClip>() { Database.pauseMenuClips[11] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                    }
                }
                else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
                {
                    debugPlayerInfo = "Hold registered. Does nothing here.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }
            }
            // If a swipe is registered.
            else if (ie.isSwipe == true)
            {
                if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                {
                    flipEchoLock(true);
                    // If the player is not in the pause menu.
                    if ((at_pause_menu == false) && (loadingScene == false))
                    {
                        // If the swipe was up, move the player forward.
                        if (ie.isUp == true)
                        {
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
                        // If the swipe was down.
                        if (ie.isDown == true)
                        {
                            endingLevel = true;
                        }
                    }
                    // If the player is in the pause menu.
                    else if ((at_pause_menu == true) && (loadingScene == false))
                    {
                        // If the swipe was down, turn on/off the visual map for debugging.
                        if (ie.isDown == true)
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
                        // If the swipe was left, the player has told us they want to restart the level.
                        else if (ie.isLeft == true)
                        {
                            debugPlayerInfo = "Swiped left. We want to restart the level.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[6] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[5] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                            }
                            wantLevelRestart = true;
                        }
                        // If the swipe was right, the player has told us they want to go back to the main menu.
                        else if (ie.isRight == true)
                        {
                            debugPlayerInfo = "Swiped right. We want to return to the main menu.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[10] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.pauseMenuClips[9] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                            }
                            wantMainMenu = true;
                        }
                        // If the swipe was up, give the player a hint.
                        else if (ie.isUp == true)
                        {
                            debugPlayerInfo = "Swiped up. Gave player hint.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            getHint(); // Give the player a hint.
                        }
                    }
                }
                else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
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
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                        {
                            question1 = false;
                            answeredQuestion1 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Is not eighteen.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                        {
                            question2 = false;
                            answeredQuestion2 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Did not understand information.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                        {
                            question3 = false;
                            answeredQuestion3 = true;
                            canRepeat = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe left registered. Does not want to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                        {
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
                            answeredQuestion1 = false;
                            answeredQuestion2 = false;
                            answeredQuestion3 = false;
                            question1 = false;
                            question2 = false;
                            question3 = false;
                            canRepeat = true;
                        }
                        else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                        {
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
                            answeredQuestion1 = false;
                            answeredQuestion2 = false;
                            answeredQuestion3 = false;
                            question1 = false;
                            question2 = false;
                            question3 = false;
                            canRepeat = true;
                        }
                        else if (noConsent == true)
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = false;
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
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
                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                        {
                            question1 = true;
                            answeredQuestion1 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Is eighteen.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                        {
                            question2 = true;
                            answeredQuestion2 = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Understood information.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                        {
                            question3 = true;
                            answeredQuestion3 = true;
                            canRepeat = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe right registered. Wants to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[5] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                        {
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
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
                        else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                        {
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
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
                        else if (noConsent == true)
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = false;
                            finished_listening = false;
                            finished_reading = false;
                            finished_questions = false;
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
                        debugPlayerInfo = "Swipe up registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                    else if (ie.isDown == true)
                    {
                        if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                        {
                            noConsent = true;
                            canRepeat = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.consentClips[6] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        }
                        else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true))
                        {
                            hearingConsentForm = false;
                            readingConsentForm = false;
                            noConsent = true;
                            canRepeat = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            clips = new List<AudioClip>() { Database.consentClips[6] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                        }
                    }
                }
            }
            // If a rotation was registered.
            else if (ie.isRotate == true)
            {
                if ((curLevel != 12) || ((curLevel == 12) && (hasFinishedConsentForm == true)))
                {
                    flipEchoLock(true);
                    // If the player is not in the pause menu.
                    if ((at_pause_menu == false) && (loadingScene == false))
                    {
                        // If the rotation was left, rotate the player left 90 degrees.
                        if (ie.isLeft == true)
                        {
                            if ((BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x <= 9) && (playerPos.y == 9))
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
                        // If the rotation was right, rotate the player right 90 degrees.
                        else if (ie.isRight == true)
                        {
                            if ((BoardManager.finishedTutorialLevel3 == false) && (curLevel == 3) && (playerPos.x <= 9) && (playerPos.y == 9))
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
                }
                else if ((curLevel == 12) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
                {
                    if (ie.isLeft == true)
                    {
                        debugPlayerInfo = "Left rotation registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                    else if (ie.isRight == true)
                    {
                        debugPlayerInfo = "Right rotation registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
            }

            // If there was an unrecognized gesture made.
            if ((ie.isUnrecognized == true) && (loadingScene == false))
            {
                madeUnrecognizedGesture = true;

                // If this error was registered.
                if (ie.isTapHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isTapVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isTapRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe left.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[3], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on swipe right.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[4], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[5], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe down.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[6], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeLeftRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe left.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeRightRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe right.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeUpRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe up.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isSwipeDownRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe down.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isRotationAngleError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with angle on rotation.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[8], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldHorizontalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[9], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldVerticalError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[10], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
                // If this error was registered.
                else if (ie.isHoldRotationError == true)
                {
                    debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[11], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                }
            }

            flipEchoLock(false);
        }
#endif // End of mobile platform dependendent compilation section started above with #elif
        calculateMove(dir);
    }

    private float lockStartTime;
    private void flipEchoLock(bool flg)
    {
        if (flg)
        {
            echoLock = flg;
            lockStartTime = Time.time;
        }
        else if (Time.time - lockStartTime > Const.opsToEchoCD)
        {
            echoLock = flg;
            lockStartTime = Time.time;
        }
    }

    // Returns a description of the location of the crash (for analysis)
    // Currently, the ouput is from the following list of options
    // ["End of the Corridor", "Intersection of 2 Corridors", "Start of the Corridor",
    // "Middle of the Corridor", "Towards End of the Corridor", "Towards Start of the Corridor"
    // "Crashed while on the Exit Sign"];
    // Currently not returning the Towards Start/End descriptions due to only having 7 discrete
    // movements in each x/y direction. May be relevant in the future.
    /// <summary>
    /// Unused function.
    /// </summary>
    private string getCrashDescription(int xLoc, int yLoc)
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        List<Vector3> positions = new List<Vector3>();

        //Go through all the walls
        foreach (var wall in walls)
        {
            positions.Add(new Vector3(wall.transform.position.x, wall.transform.position.y, 0f));
        }

        float distXUp = 0;
        float distXDown = 0;
        float distYUp = 0;
        float distYDown = 0;
        float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;
        float threshhold = 0.01f;

        while (true)
        {
            distXUp = distXUp + 1 * scale;
            Vector3 currPos = new Vector3(xLoc + distXUp, yLoc, 0f);
            for (int j = 0; j < positions.Count; ++j)
            {
                if ((positions[j] - currPos).magnitude <= threshhold)
                {
                    break;
                }
            }
        }
        while (true)
        {
            distXDown = distXDown + 1 * scale;
            Vector3 currPos = new Vector3(xLoc - distXDown, yLoc, 0f);
            for (int j = 0; j < positions.Count; ++j)
            {
                if ((positions[j] - currPos).magnitude <= threshhold)
                {
                    break;
                }
            }
        }
        while (true)
        {
            distYUp = distYUp + 1 * scale;
            Vector3 currPos = new Vector3(xLoc, yLoc + distYUp, 0f);
            for (int j = 0; j < positions.Count; ++j)
            {
                if ((positions[j] - currPos).magnitude <= threshhold)
                {
                    break;
                }
            }
        }
        while (true)
        {
            distYDown = distYDown + 1 * scale;
            Vector3 currPos = new Vector3(xLoc, yLoc - distYDown, 0f);
            for (int j = 0; j < positions.Count; ++j)
            {
                if ((positions[j] - currPos).magnitude <= threshhold)
                {
                    break;
                }
            }
        }

        // positions.Contains (xLoc, yLoc);

        UnityEngine.Debug.Log("Number of walls detected");
        UnityEngine.Debug.Log(walls.Length);

        UnityEngine.Debug.Log("Current Position of Player");
        UnityEngine.Debug.Log(xLoc);
        UnityEngine.Debug.Log(yLoc);

        UnityEngine.Debug.Log("Distances to walls in all directions");
        UnityEngine.Debug.Log(distXUp);
        UnityEngine.Debug.Log(distXDown);
        UnityEngine.Debug.Log(distYUp);
        UnityEngine.Debug.Log(distYDown);

        // All the crash location options
        // string[] locs = ["End of the Corridor", "Intersection of 2 Corridors", "Start of the Corridor", "Middle of the Corridor", "Towards End of the Corridor", "Towards Start of the Corridor"];

        // If Crash happened while on the Exit Sign
        GameObject exitSign = GameObject.FindGameObjectWithTag("Exit");
        if ((xLoc == (int)exitSign.transform.position.x) & (yLoc == (int)exitSign.transform.position.y))
        {
            return "Crashed while on the Exit Sign";
        }
        // TODO(agotsis/wenyuw1) This hardcoding needs to go away. Mainly here to test the database.
        // For the x direction
        if ((distXUp == 7) & (distXDown == 1) & (distYUp == 1) & (distYDown == 1))
        {
            return "Start of the Corridor";
        }
        if ((distXUp == 4) & (distXDown == 4) & (distYUp == 1) & (distYDown == 1))
        {
            return "Middle of the Corridor";
        }
        if ((distXUp == 1) & (distXDown == 7) & (distYUp == 1) & (distYDown == 1))
        {
            return "End of the Corridor";
        }
        if ((distXUp == 1) & (distXDown == 8) & (distYUp == 8) & (distYDown == 1))
        {
            return "Intersection of 2 Corridors";
        }
        //For the y direction
        if ((distXUp == 1) & (distXDown == 1) & (distYUp == 7) & (distYDown == 2))
        {
            return "Start of the Corridor";
        }
        if ((distXUp == 1) & (distXDown == 1) & (distYUp == 4) & (distYDown == 5))
        {
            return "Middle of the Corridor";
        }
        if ((distXUp == 1) & (distXDown == 1) & (distYUp == 5) & (distYDown == 4))
        {
            return "Middle of the Corridor";
        }
        if ((distXUp == 1) & (distXDown == 1) & (distYUp == 1) & (distYDown == 8))
        {
            return "End of the Corridor";
        }
        if ((distXUp == 1) & (distXDown == 1) & (distYUp > distYDown))
        {
            return "Towards Start of the Corridor";
        }
        if ((distYUp == 1) & (distYDown == 1) & (distXUp > distXDown))
        {
            return "Towards Start of the Corridor";
        }
        if ((distXUp == 1) & (distXDown == 1) & (distYUp < distYDown))
        {
            return "Towards End of the Corridor";
        }
        if ((distYUp == 1) & (distYDown == 1) & (distXUp < distXDown))
        {
            return "Towards End of the Corridor";
        }

        return "Error";
    }

    protected override void OnCantMove<T>(T component)
    {
        // Set hitWall to equal the component passed in as a parameter.
        Wall hitWall = component as Wall;
        // if (!SoundManager.instance.isBusy())
        //SoundManager.instance.PlayVoice(Database.soundEffectClips[8], true, 0.0f, 0.0f, 0.5f);
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
        // Load the last scene loaded, in this case Main, the only scene in the game.
        SceneManager.LoadScene("Main");
        restarted = false;
    }

    protected override void OnMove()
    {

    }

    public void Intercept(int level)
    {
        // If the current level is not the level we are trying to intercept.
        if (curLevel != level)
        {
            return;
        }
        intercepted = true; // Otherwise we are intercepting a level to give hints/a tutorial.
        switch (level)
        {
            case 1:
                /*
                This tutorial will go over gestures used in the game. When making gestures, please try to keep them in the middle of the screen.
                The first gesture is tapping, which generates a click and echo based on the player's location in the game.
                PlayEcho()
                This sound will change according to your surroundings.
                Please tap the screen 3 times, holding down for about half a second, and pausing about a second between taps.
                */
                debugPlayerInfo = "Playing tutorial level 1 clips.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedTappingInstruction = false; // Reset if the player is going through this tutorial level again.
                finishedSwipingInstruction = false; // Reset if the player is going through this tutorial level again.
                finishedMenuInstruction = false; // Reset if the player is going through this tutorial level again.
                finishedExitingInstruction = false; // Reset if the player is going through this tutorial level again.
                if (GM_title.isUsingTalkback == true)
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.tutorialClips[0], Database.tutorialClips[2], Database.soundEffectClips[1], Database.tutorialClips[3], Database.tutorialClips[4] };
                    SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(), 3, 0.5f); // If they are using Talkback, play the correct instructions.
                }
                else if (GM_title.isUsingTalkback == false)
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.tutorialClips[0], Database.tutorialClips[1], Database.soundEffectClips[1], Database.tutorialClips[3], Database.tutorialClips[4] };
                    SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(), 3, 0.5f); // If they are not using Talkback, play the correct instructions.
                }
                level1_remaining_taps = 3; // Set the remaining taps for the tutorial to 3.
                level1_remaining_ups = 3; // Set the remaining swipes up for the tutorial to 3.
                level1_remaining_menus = 2; // Set the remaining holds for the tutorial to 2.
                break;
            case 3:
                /*
                This is level 3. In this level there is a right corner you must navigate to reach the end. Notice how the echo changes as you approach corners.
                You have reached the corner! As you may have heard the, echo changed at this point. In order to proceed, you will need to rotate yourself to move without hitting a wall. To rotate yourself, tap and hold with two fingers and rotate them clockwise to turn right or counter-clockwise to turn left. After rotating in a direction, you will hear a sound like this in the ear corresponding to the direction you rotated.
                Tapping to hear an echo after rotating could be helpful in determining where you need to go and or if you should keep turning.
                Please rotate to the right or left 4 times, pausing about a second between rotations. This will bring you back to the same position you started at.
                */
                debugPlayerInfo = "Playing tutorial level 3 clips.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedCornerInstruction = false; // Reset if the player is going through this tutorial level again.
                finishedTurningInstruction = false; // Reset if the player is going through this tutorial level again.
                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[21] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // Play the appropriate clip.
                level3_remaining_turns = 4; // Set the remaining rotations for the tutorial to 4.
                break;
            default:
                break;
        }
    }

    public bool InterceptMission(InputEvent ie)
    {
        InterceptAction action = InterceptAction.NONE;
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        // If a tap was registered, set this as the action.
        if (ie.isTap == true)
        {
            action = InterceptAction.TAP;
        }
        // If a swipe was registered.
        else if (ie.isSwipe == true)
        {
            // If a swipe left was registered, set that as the action.
            if (ie.isLeft == true)
            {
                action = InterceptAction.LEFTSWIPE;
            }
            // If a swipe right was registered, set that as the action.
            else if (ie.isRight == true)
            {
                action = InterceptAction.RIGHTSWIPE;
            }
            // If a swipe right was registered, set that as the action.
            else if (ie.isUp == true)
            {
                action = InterceptAction.UP;
            }
            // If a swipe right was registered, set that as the action.
            else if (ie.isDown == true)
            {
                action = InterceptAction.DOWN;
            }
        }
        // If a rotation was registered.
        else if (ie.isRotate == true)
        {
            // If a left rotation was registered, set that as the action.
            if (ie.isLeft == true)
            {
                action = InterceptAction.LEFTROTATE;
            }
            // If a right rotation was registered, set that as the action.
            else if (ie.isRight == true)
            {
                action = InterceptAction.RIGHTROTATE;
            }
        }
        // If a hold was registered, set that as the action.
        else if (ie.isHold == true)
        {
            action = InterceptAction.MENU;
        }
#endif
#if UNITY_IOS || UNITY_ANDROID
        // If a tap was registered, set this as the action.
        if (ie.isTap == true)
        {
            action = InterceptAction.TAP;
        }
        // If a hold was registered, set this as the action.
        else if (ie.isHold == true)
        {
            action = InterceptAction.MENU;
        }
        // If a swipe was registered.
        else if (ie.isSwipe == true)
        {
            // If the swipe was up, set this as the action.
            if (ie.isUp == true)
            {
                action = InterceptAction.UP;
            }
            // If the swipe was down, set this as the action.
            else if (ie.isDown == true)
            {
                action = InterceptAction.DOWN;
            }
            else if (ie.isLeft == true)
            {
                action = InterceptAction.LEFTSWIPE;
            }
            else if (ie.isRight == true)
            {
                action = InterceptAction.RIGHTSWIPE;
            }
        }
        // If a rotation was registered.
        else if (ie.isRotate == true)
        {
            // If the rotation was left, set this as the action.
            if (ie.isLeft == true)
            {
                action = InterceptAction.LEFTROTATE;
            }
            // If the rotation was right, set this as the action.
            else if (ie.isRight == true)
            {
                action = InterceptAction.RIGHTROTATE;
            }
        }
#endif

        // Based on the action, play the appropriate sound.
        switch (action)
        {
            case InterceptAction.TAP:
                if ((finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                {
                    PlayEcho(); // If the tapping instructions have played and the player has more taps left to do, play an echo.
                }
                break;
            default:
                break;
        }

        // Based on the current level we are on, do something with the actions.
        switch (curLevel)
        {
            // If we are on level 1.
            case 1:
                // If the player has not finished the tapping part of the tutorial.
                if (level1_remaining_taps > 0)
                {
                    // If the action was a tap.
                    if ((action == InterceptAction.TAP) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Tapped for gesture tutorial. Played echo.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level1_remaining_taps--; // Decrease the amount of taps left to do.
                        if (level1_remaining_taps == 2)
                        {
                            clips = new List<AudioClip> { Database.soundEffectClips[1], Database.tutorialClips[5] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // This tap was correct. Please tap X more times.
                        }
                        else if (level1_remaining_taps == 1)
                        {
                            clips = new List<AudioClip> { Database.soundEffectClips[1], Database.tutorialClips[6] };
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // This tap was correct. Please tap X more times.
                        }
                        // If the player has finished the tapping section.
                        else if (level1_remaining_taps == 0)
                        {
                            // Good job! now we will move on to swiping.
                            // Swiping upward with three fingers moves you forward in the game and generates a sound like this.
                            // SoundManager.instance.PlaySingle(Database.instance.swipeAhead)
                            // Please swipe upward 3 times, pausing about a second between swipes.

                            debugPlayerInfo = "Finished tapping section for gesture tutorial.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[1], Database.tutorialClips[7], Database.tutorialClips[9], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[1], Database.tutorialClips[7], Database.tutorialClips[8], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are not using Talkback, play the correct instructions.
                            }
                        }
                    }
                    // If the action was not a tap.
                    else if (((action == InterceptAction.MENU) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (finishedTappingInstruction == true))
                    {
                        // If this error was registered.
                        if ((ie.isUnrecognized == true) && (ie.isTapHorizontalError == true))
                        {
                            debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                        // If this error was registered.
                        else if ((ie.isUnrecognized == true) && (ie.isTapVerticalError == true))
                        {
                            debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                        // If this error was registered.
                        else if ((ie.isUnrecognized == true) && (ie.isTapRotationError == true))
                        {
                            debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                        // If the gesture was not a tap.
                        else
                        {
                            debugPlayerInfo = "Incorrect gesture made. You should tap.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                    }
                    // If the tapping instruction has not finished yet.
                    else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (finishedTappingInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
                // If the player has finished the tapping part of the tutorial.
                else if (level1_remaining_taps == 0)
                {
                    // If the player has not finished the swiping part of the tutorial.
                    if (level1_remaining_ups > 0)
                    {
                        // If the action was a swipe up.
                        if ((action == InterceptAction.UP) && (finishedSwipingInstruction == true))
                        {
                            debugPlayerInfo = "Swiped up for gesture tutorial.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            level1_remaining_ups--; // Decrease the number of swipes up left to do.
                            if (level1_remaining_ups == 2)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[11] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // This swipe was correct. Please swipe X more times.
                            }
                            else if (level1_remaining_ups == 1)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[12] };
                                SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // This swipe was correct. Please swipe X more times.
                            }
                            // If the player has finished the swiping section of the tutorial.
                            else if (level1_remaining_ups == 0)
                            {
                                // TODO: Replace "now we will move back to the game" with "now we will show you how to use the pause menu"
                                // Good job! now we will move back to the game!
                                // Tapping the screen with two fingers and holding for 2 seconds opens the pause menu.
                                debugPlayerInfo = "Finished swiping section for gesture tutorial.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                if (GM_title.isUsingTalkback == true)
                                {
                                    clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[13], Database.tutorialClips[15] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[13], Database.tutorialClips[14] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are not using Talkback, play the correct instructions.
                                }
                            }
                        }
                        // If the action was not a swipe up.
                        else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (finishedSwipingInstruction == true))
                        {
                            // If this error was registered.
                            if ((ie.isUnrecognized == true) && (ie.isSwipeUpVerticalError == true))
                            {
                                debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe up.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                SoundManager.instance.PlayVoice(Database.errorClips[5], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                            }
                            // If this error was registered.
                            else if ((ie.isUnrecognized == true) && (ie.isSwipeUpRotationError == true))
                            {
                                debugPlayerInfo = "Nothing happened due to error with rotation on swipe up.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                            }
                            // If the gesture was not a swipe up.
                            else
                            {
                                debugPlayerInfo = "Incorrect gesture made. You should swipe up.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                SoundManager.instance.PlayVoice(Database.errorClips[13], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                            }
                        }
                        // If the swiping instruction has not finished yet.
                        else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (finishedSwipingInstruction == false))
                        {
                            debugPlayerInfo = "Please wait for the instructions to finish.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        }
                    }
                    // If the player has finished the swiping section of the tutorial.
                    else if (level1_remaining_ups == 0)
                    {
                        // If the player has not finished the menu section of the tutorial.
                        if (level1_remaining_menus > 0)
                        {
                            // If the action was a hold.
                            if ((action == InterceptAction.MENU) && (finishedMenuInstruction == true))
                            {
                                // If the pause menu has not been opened.
                                if (level1_remaining_menus == 2)
                                {
                                    debugPlayerInfo = "Hold registered. Opened pause menu for gesture tutorial.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    level1_remaining_menus--; // Decrease the number of holds left to do.
                                    finishedMenuInstruction = false; // The player should not be able to make a gesture while we are explaining what you can do in the pause menu.
                                    waitingForOpenMenuInstruction = true;  // The player should not be able to make a gesture while we are explaining what you can do in the pause menu.
                                    // You are now in the pause menu. To get a hint, swipe up. To restart the level, swipe left. To go to the main menu, swipe right. To close the pause menu, tap and hold with two fingers for 2 seconds. Please close the pause menu now.
                                    if (GM_title.isUsingTalkback == true)
                                    {
                                        clips = new List<AudioClip> { Database.tutorialClips[17] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                                    }
                                    else if (GM_title.isUsingTalkback == false)
                                    {
                                        clips = new List<AudioClip> { Database.tutorialClips[16] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                                    }
                                }
                                // If the pause menu has not been closed.
                                else if ((level1_remaining_menus == 1) && (waitingForOpenMenuInstruction == false))
                                {
                                    debugPlayerInfo = "Hold registered. Closed pause menu. Finished menu section for gesture tutorial.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    level1_remaining_menus--; // Decrease the number of holds left to do.
                                    // Congratulations! You have reached the exit. Once you believe you have reached the exit in a level, swiping down will move you to the next level and you will hear a congratulatory sound like this.
                                    // Finish SOUND
                                    // Now try to swipe down to move on to the next level.
                                    if (GM_title.isUsingTalkback == true)
                                    {
                                        clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[19], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[20] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                                    }
                                    else if (GM_title.isUsingTalkback == false)
                                    {
                                        clips = new List<AudioClip> { Database.soundEffectClips[0], Database.tutorialClips[18], Database.soundEffectClips[9], Database.soundEffectClips[0], Database.tutorialClips[20] };
                                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // If they are not using Talkback, play the correct instructions.
                                    }
                                }
                            }
                            // If the action was not a hold.
                            else if (((action == InterceptAction.TAP) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (finishedMenuInstruction == true))
                            {
                                // If this error was registered.
                                if ((ie.isUnrecognized == true) && (ie.isHoldHorizontalError == true))
                                {
                                    debugPlayerInfo = "Nothing happened due to error with horizontal distance on hold.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    SoundManager.instance.PlayVoice(Database.errorClips[9], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                                }
                                // If this error was registered.
                                else if ((ie.isUnrecognized == true) && (ie.isHoldVerticalError == true))
                                {
                                    debugPlayerInfo = "Nothing happened due to error with vertical distance on hold.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    SoundManager.instance.PlayVoice(Database.errorClips[10], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                                }
                                // If this error was registered.
                                else if ((ie.isUnrecognized == true) && (ie.isHoldRotationError == true))
                                {
                                    debugPlayerInfo = "Nothing happened due to error with rotation on hold.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    SoundManager.instance.PlayVoice(Database.errorClips[11], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                                }
                                // If the gesture was not a hold.
                                else
                                {
                                    debugPlayerInfo = "Incorrect gesture made. You should make a hold.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    SoundManager.instance.PlayVoice(Database.errorClips[14], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                                }
                            }
                            // If the pause menu instruction has not finished yet.
                            else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && ((finishedMenuInstruction == false) || (waitingForOpenMenuInstruction == true)))
                            {
                                debugPlayerInfo = "Please wait for the instructions to finish.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                        }
                        // If the player has finished the hold section of the tutorial.
                        else if (level1_remaining_menus == 0)
                        {
                            // If the action was a swipe down.
                            if ((action == InterceptAction.DOWN) && (finishedExitingInstruction == true) && (BoardManager.finishedTutorialLevel1 == false))
                            {                               
                                // TODO: Replace the winSound with "Congratulations! You have completed the tutorial. Now we will move back to the game!"                      
                                canRepeat = true;
                                BoardManager.finishedTutorialLevel1 = true; // Make sure the player does not have to go through the tutorial again if they have gone through it once.
                                GameMode.finishedLevel1Tutorial = BoardManager.finishedTutorialLevel1;
                                GameMode.finishedLevel3Tutorial = BoardManager.finishedTutorialLevel3;
                                GameMode.write_save_mode(curLevel, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);
                                if (hasFinishedConsentForm == false)
                                {
                                    debugPlayerInfo = "Swiped down correctly. Moving to consent instructions.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                                    
                                    clips = new List<AudioClip> { Database.soundEffectClips[9] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);                                    
                                }
                                else if (hasFinishedConsentForm == true)
                                {
                                    debugPlayerInfo = "Swiped down correctly. Moving to level 1.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    clips = new List<AudioClip> { Database.soundEffectClips[9] };
                                    SoundManager.instance.PlayClips(clips, null, 0, () => quitInterception(), 1, 0.5f, true);
                                }                                
                            }
                            // If the action was not a swipe down.
                            else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.UP) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (finishedExitingInstruction == true) && (BoardManager.finishedTutorialLevel1 == false))
                            {
                                // If this error was registered.
                                if ((ie.isUnrecognized == true) && (ie.isSwipeDownVerticalError == true))
                                {
                                    debugPlayerInfo = "Nothing happened due to error with vertical distance on swipe down.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    SoundManager.instance.PlayVoice(Database.errorClips[6], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                                }
                                // If this error was registered.
                                else if ((ie.isUnrecognized == true) && (ie.isSwipeDownRotationError == true))
                                {
                                    debugPlayerInfo = "Nothing happened due to error with rotation on swipe down.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    SoundManager.instance.PlayVoice(Database.errorClips[7], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                                }
                                // If the gesture was not a swipe down.
                                else
                                {
                                    debugPlayerInfo = "Incorrect gesture made. You should swipe down.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    SoundManager.instance.PlayVoice(Database.errorClips[15], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                                }
                            }
                            // If the exit instruction has not finished yet.
                            else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (finishedExitingInstruction == false))
                            {
                                debugPlayerInfo = "Please wait for the instructions to finish.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }

                            if ((action == InterceptAction.LEFTSWIPE) && (hasStartedConsent == true))
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
                                else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                                {
                                    question1 = false;
                                    answeredQuestion1 = true;
                                    canRepeat = true;
                                    debugPlayerInfo = "Swipe left registered. Is not eighteen.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                }
                                else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                                {
                                    question2 = false;
                                    answeredQuestion2 = true;
                                    canRepeat = true;
                                    debugPlayerInfo = "Swipe left registered. Did not understand information.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                }
                                else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                                {
                                    question3 = false;
                                    answeredQuestion3 = true;
                                    canRepeat = true;
                                    finished_questions = true;
                                    debugPlayerInfo = "Swipe left registered. Does not want to participate.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                }
                                else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                                {
                                    finished_listening = false;
                                    finished_reading = false;
                                    finished_questions = false;
                                    answeredQuestion1 = false;
                                    answeredQuestion2 = false;
                                    answeredQuestion3 = false;
                                    question1 = false;
                                    question2 = false;
                                    question3 = false;
                                    canRepeat = true;
                                }
                                else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                                {
                                    finished_listening = false;
                                    finished_reading = false;
                                    finished_questions = false;
                                    answeredQuestion1 = false;
                                    answeredQuestion2 = false;
                                    answeredQuestion3 = false;
                                    question1 = false;
                                    question2 = false;
                                    question3 = false;
                                    canRepeat = true;
                                }
                                else if (noConsent == true)
                                {
                                    hearingConsentForm = false;
                                    readingConsentForm = false;
                                    noConsent = false;
                                    finished_listening = false;
                                    finished_reading = false;
                                    finished_questions = false;
                                    answeredQuestion1 = false;
                                    answeredQuestion2 = false;
                                    answeredQuestion3 = false;
                                    question1 = false;
                                    question2 = false;
                                    question3 = false;
                                    canRepeat = true;
                                }
                            }
                            else if ((action == InterceptAction.RIGHTSWIPE) && (hasStartedConsent == true))
                            {
                                if ((readingConsentForm == false) && (hearingConsentForm == false) && (noConsent == false))
                                {
                                    readingConsentForm = true;
                                    canRepeat = true;
                                    debugPlayerInfo = "Swipe right registered. Reading consent form manually.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
                                    finished_reading = true;
                                    answeredQuestion1 = true;
                                    question1 = true;
                                    answeredQuestion2 = true;
                                    question2 = true;
                                    answeredQuestion3 = true;
                                    question3 = true;
                                    finished_questions = true;
#endif
                                }
                                else if ((hearingConsentForm == true) && (answeredQuestion1 == false))
                                {
                                    question1 = true;
                                    answeredQuestion1 = true;
                                    canRepeat = true;
                                    debugPlayerInfo = "Swipe right registered. Is eighteen.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                }
                                else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false))
                                {
                                    question2 = true;
                                    answeredQuestion2 = true;
                                    canRepeat = true;
                                    debugPlayerInfo = "Swipe right registered. Understood information.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                }
                                else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false))
                                {
                                    question3 = true;
                                    answeredQuestion3 = true;
                                    canRepeat = true;
                                    finished_questions = true;
                                    debugPlayerInfo = "Swipe right registered. Wants to participate.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[5] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                                }
                                else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                                {
                                    finished_listening = false;
                                    finished_reading = false;
                                    finished_questions = false;
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
                                else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
                                {
                                    finished_listening = false;
                                    finished_reading = false;
                                    finished_questions = false;
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
                                else if (noConsent == true)
                                {
                                    hearingConsentForm = false;
                                    readingConsentForm = false;
                                    noConsent = false;
                                    finished_listening = false;                                  
                                    finished_reading = false;
                                    finished_questions = false;
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
                            else if ((action == InterceptAction.UP) && (hasStartedConsent == true))
                            {
                                debugPlayerInfo = "Swipe up registered. Does nothing in this menu.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                            else if ((action == InterceptAction.DOWN) && (hasStartedConsent == true))
                            {
                                if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                                {
                                    noConsent = true;
                                    canRepeat = true;
                                    finished_questions = true;
                                    debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    clips = new List<AudioClip>() { Database.consentClips[6] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                                }
                                else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true))
                                {
                                    hearingConsentForm = false;
                                    readingConsentForm = false;
                                    noConsent = true;
                                    canRepeat = true;
                                    finished_questions = true;
                                    debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    clips = new List<AudioClip>() { Database.consentClips[6] };
                                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true);
                                }
                            }
                            else if ((action == InterceptAction.TAP) && (hasStartedConsent == true) && (hasFinishedConsentForm == false))
                            {
                                if (noConsent == true)
                                {
                                    Utilities.writefile("consentRecord", "0");
                                    debugPlayerInfo = "Tap registered. Did not consent to having data collected. Moving to level 1.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    hasFinishedConsentForm = true;
                                    canRepeat = true;
                                    clips = new List<AudioClip>() { Database.consentClips[8] };
                                    SoundManager.instance.PlayClips(clips, null, 0, () => quitInterception(), 1, 0.5f, true);
                                }
                                else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
                                {
                                    Utilities.writefile("consentRecord", "1");
                                    debugPlayerInfo = "Tap registered. Consented to having data collected. Moving to level 1.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    hasFinishedConsentForm = true;
                                    canRepeat = true;
                                    clips = new List<AudioClip>() { Database.consentClips[7] };
                                    SoundManager.instance.PlayClips(clips, null, 0, () => quitInterception(), 1, 0.5f, true);
                                }
                                else
                                {
                                    debugPlayerInfo = "Tap registered. Does nothing here.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                }
                            }
                            else if ((action == InterceptAction.MENU) && (hasStartedConsent == true))
                            {
                                debugPlayerInfo = "Hold registered. Does nothing here.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                            else if ((action == InterceptAction.LEFTROTATE) && (hasStartedConsent == true))
                            {
                                debugPlayerInfo = "Left rotation registered. Does nothing here.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                            else if ((action == InterceptAction.RIGHTROTATE) && (hasStartedConsent == true))
                            {
                                debugPlayerInfo = "Right rotation registered. Does nothing here.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                        }
                    }
                }
                break;
            // If we are on level 3.
            case 3:
                // If the player has not finished the rotation section of the tutorial.
                if (level3_remaining_turns > 0)
                {
                    // If the player has not tapped at the corner yet and the gesture was a tap.
                    if ((action == InterceptAction.TAP) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false) && (finishedTurningInstruction == false))
                    {
                        debugPlayerInfo = "Tapped at corner. Played echo.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        hasTappedAtCorner = true;
                        if (GM_title.isUsingTalkback == true)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[1], Database.tutorialClips[23], Database.soundEffectClips[0], Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[24], Database.tutorialClips[25] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(), 0, 0.5f, true); // If they are using Talkback, play the correct instructions.
                        }
                        else if (GM_title.isUsingTalkback == false)
                        {
                            clips = new List<AudioClip>() { Database.soundEffectClips[1], Database.tutorialClips[22], Database.soundEffectClips[0], Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[24], Database.tutorialClips[25] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(), 0, 0.5f, true); // If they are not using Talkback, play the correct instructions.
                        }
                    }
                    // If the player has not tapped at the corner yet and the gesture was not a tap.
                    else if (((action == InterceptAction.MENU) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false) && (finishedTurningInstruction == false))
                    {
                        // If this error was registered.
                        if ((ie.isUnrecognized == true) && (ie.isTapHorizontalError == true))
                        {
                            debugPlayerInfo = "Nothing happened due to error with horizontal distance on tap.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[0], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                        // If this error was registered.
                        else if ((ie.isUnrecognized == true) && (ie.isTapVerticalError == true))
                        {
                            debugPlayerInfo = "Nothing happened due to error with vertical distance on tap.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[1], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                        // If this error was registered.
                        else if ((ie.isUnrecognized == true) && (ie.isTapRotationError == true))
                        {
                            debugPlayerInfo = "Nothing happened due to error with rotation on tap.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[2], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                        // If a tap was not registered.
                        else
                        {
                            debugPlayerInfo = "Incorrect gesture made. You should tap.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[12], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                    }
                    // If the tap at corner instruction has not finished yet.
                    else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (finishedCornerInstruction == false) && (finishedTurningInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                    // If the action was a left or right rotation.
                    else if (((action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE)) && (hasTappedAtCorner == true) && (finishedTurningInstruction == true))
                    {
                        debugPlayerInfo = "Rotated for gesture tutorial. Turned player 90 degrees.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level3_remaining_turns--; // Decrease the number of turns left to do.
                        if (level3_remaining_turns == 3)
                        {
                            if (action == InterceptAction.LEFTROTATE)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.tutorialClips[26] };
                            }
                            else if (action == InterceptAction.RIGHTROTATE)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[26] };
                            }
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // This rotation was correct. Please rotate X more times.
                        }
                        else if (level3_remaining_turns == 2)
                        {
                            if (action == InterceptAction.LEFTROTATE)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.tutorialClips[27] };
                            }
                            else if (action == InterceptAction.RIGHTROTATE)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[27] };
                            }
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // This rotation was correct. Please rotate X more times.
                        }
                        else if (level3_remaining_turns == 1)
                        {
                            if (action == InterceptAction.LEFTROTATE)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.tutorialClips[28] };
                            }
                            else if (action == InterceptAction.RIGHTROTATE)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[28] };
                            }
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, 0.5f, true); // This rotation was correct. Please rotate X more times.
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
                            // Good job! now we will move back to the game. Try and get around the corner!
                            if (action == InterceptAction.LEFTROTATE)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[5], Database.soundEffectClips[0], Database.tutorialClips[29] };
                            }
                            else if (action == InterceptAction.RIGHTROTATE)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[29] };
                            }
                            SoundManager.instance.PlayClips(clips, null, 0, () => quitInterception(), 3, 0.5f);
                        }
                    }
                    // If the action was not a right or left rotation.
                    else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (ie.isUnrecognized == true)) && (hasTappedAtCorner == true) && (finishedTurningInstruction == true))
                    {
                        // If this error was registered.
                        if ((ie.isUnrecognized == true) && (ie.isRotationAngleError == true))
                        {
                            debugPlayerInfo = "Nothing happened due to error with angle of rotation.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[8], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                        // If the gesture was not a rotation
                        else
                        {
                            debugPlayerInfo = "Incorrect gesture made. You should make a left or right rotation.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.errorClips[16], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                        }
                    }
                    // If the turning instruction has not finished yet.
                    else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFTSWIPE) || (action == InterceptAction.RIGHTSWIPE) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (action == InterceptAction.LEFTROTATE) || (action == InterceptAction.RIGHTROTATE) || (ie.isUnrecognized == true)) && (hasTappedAtCorner == true) && (finishedTurningInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }
                }
                break;
            default:
                break;
        }
        return false;
    }

    /// <summary>
    /// If the player is in a gesture tutorial, leave the tutorial so that they can continue with the level.
    /// </summary>
    private void quitInterception()
    {
        intercepted = false;
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

        string filename = Application.persistentDataPath + "consentRecord";
        string[] svdata_split;

        if (System.IO.File.Exists(filename))
        {
            svdata_split = System.IO.File.ReadAllLines(filename);
            int hasConsented = Int32.Parse(svdata_split[0]);

            if (hasConsented == 1)
            {
                print("Has consented to sending their data to the server for research.");

                // If an error occcurs while loading the page, or if the player is not connected to the internet via WiFi or cell service.
                if (www.error != null)
                {
                    print("Text: " + www.text);
                    print("Error: " + www.error);
                    print("Could not connect to the Internet.");

                    string storedFilepath = Application.persistentDataPath + "storeddata" + curLevel;

                    int directoryEnd = 0;
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
                    directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab") + 10;
#endif
#if UNITY_IOS || UNITY_ANDROID
                    directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab.Echoadventure") + 24;
#endif

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
                    levelData[5] = Utilities.encrypt(startTime.ToString());
                    levelData[6] = Utilities.encrypt(endTime.ToString());
                    levelData[7] = Utilities.encrypt(accurateElapsed.ToString("F3"));
                    levelData[8] = Utilities.encrypt(exitAttempts.ToString());
                    levelData[9] = Utilities.encrypt(GameManager.instance.boardScript.asciiLevelRep);
                    levelData[10] = GameManager.instance.boardScript.gamerecord;
                    levelData[11] = score.ToString();

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
                        levelCompleteForm.AddField("startTime", Utilities.encrypt(startTime.ToString()));
                        levelCompleteForm.AddField("endTime", Utilities.encrypt(endTime.ToString()));
                        levelCompleteForm.AddField("timeElapsed", Utilities.encrypt(accurateElapsed.ToString("F3")));
                        levelCompleteForm.AddField("exitAttempts", Utilities.encrypt(exitAttempts.ToString()));
                        levelCompleteForm.AddField("asciiLevelRep", Utilities.encrypt(GameManager.instance.boardScript.asciiLevelRep));
                        levelCompleteForm.AddField("levelRecord", (GameManager.instance.boardScript.gamerecord));

                        // Logging.Log(System.Text.Encoding.ASCII.GetString(levelCompleteForm.data), Logging.LogLevel.LOW_PRIORITY);

                        // Send the name of the echo files used in this level and the counts
                        // form.AddField("echoFileNames", getEchoNames());

                        // Send the details of the crash locations
                        // form.AddField("crashLocations", crashLocs);

                        levelCompleteForm.AddField("score", score);

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

                        string storedFilepath = Application.persistentDataPath + "storeddata" + curLevel;

                        int directoryEnd = 0;
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
                        directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab") + 10;
#endif
#if UNITY_IOS || UNITY_ANDROID
                        directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab.Echoadventure") + 24;
#endif

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
                        levelData[5] = Utilities.encrypt(startTime.ToString());
                        levelData[6] = Utilities.encrypt(endTime.ToString());
                        levelData[7] = Utilities.encrypt(accurateElapsed.ToString("F3"));
                        levelData[8] = Utilities.encrypt(exitAttempts.ToString());
                        levelData[9] = Utilities.encrypt(GameManager.instance.boardScript.asciiLevelRep);
                        levelData[10] = GameManager.instance.boardScript.gamerecord;
                        levelData[11] = score.ToString();

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
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab") + 10;
#endif
#if UNITY_IOS || UNITY_ANDROID 
        directoryEnd = Application.persistentDataPath.IndexOf("AuditoryLab.Echoadventure") + 24;
#endif 

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
                    levelCompleteForm.AddField("startTime", svdata_split[5]);
                    levelCompleteForm.AddField("endTime", svdata_split[6]);
                    levelCompleteForm.AddField("timeElapsed", svdata_split[7]);
                    levelCompleteForm.AddField("exitAttempts", svdata_split[8]);
                    levelCompleteForm.AddField("asciiLevelRep", svdata_split[9]);
                    levelCompleteForm.AddField("levelRecord", svdata_split[10]);

                    // Logging.Log(System.Text.Encoding.ASCII.GetString(levelCompleteForm.data), Logging.LogLevel.LOW_PRIORITY);

                    // Send the name of the echo files used in this level and the counts
                    // form.AddField("echoFileNames", getEchoNames());

                    // Send the details of the crash locations
                    // form.AddField("crashLocations", crashLocs);

                    levelCompleteForm.AddField("score", Int32.Parse(svdata_split[11]));

                    WWW www2 = new WWW(levelDataEndpoint, levelCompleteForm);                    

                    print("Sending stored data to the server.");
                    StartCoroutine(Utilities.WaitForRequest(www2, file.Name, file.FullName));
                }
            }
        }
    }
}
