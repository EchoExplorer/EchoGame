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
using System.Linq;
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

    bool is_freezed; // is player not allowed to do anything?
    bool tapped; // did player tap to hear an echo at this position?
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

    // Usage data to keep track of
    public static bool want_exit = false;
    bool at_pause_menu = false; // indicating if the player activated pause menu
    bool localRecordWritten = false;
    // int score;
    eventHandler eh;

    string debugPlayerInfo; // String for debugging the effects of the player's actions (Tells you they rotated, swiped, etc.).

#if UNITY_ANDROID && !UNITY_EDITOR
    bool survey_activated = false;
    bool code_entered = false;
    bool URL_shown = false;
    bool survey_shown = false;
    string surveyCode = "";
#endif 

    List<AudioClip> clips;

    bool madeUnrecognizedGesture = false;

    bool finishedTappingInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to make a tap.
    bool finishedSwipingInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to make a swipe.
    bool finishedMenuInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to open or close the pause menu.
    bool finishedExitingInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to exit a level.
    static bool finishedCornerInstruction = false; // Make sure the player cannot make a gesture we we are explaining to tap at a corner.
    bool finishedTurningInstruction = false; // Make sure the player cannot make a gesture when we are explaining how to make a rotation.    

    bool finishedEcho = false;

    bool waitingForOpenMenuInstruction = false; // Make sure the player cannot make gestures while we are explaining what you can do in the pause menu in the gesture tutorial.
    bool wantLevelRestart = false; // Used to make sure the player has to tap once after swiping left in the pause menu to confirm they want to restart the level.
    bool wantMainMenu = false; // Used to make sure the player has to tap once after swiping right in the pause menu to confirm they want to go to the main menu.

    static bool endingLevel;

    bool hasMoved = false;
    float ratio;

    bool swipedUp;
    bool rotatedLeft;
    bool rotatedRight;

    static bool[,] canPlayClip = new bool[12, 7];
    static bool playedExitClip;
    static bool canGoToNextLevel;

    static bool hasTappedAtCorner = false;

    public static bool reachedExit = false;

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

    AudioClip attenuatedClick = Database.attenuatedClickHRTF;
    AudioClip echoFront = Database.hrtf_front;
    AudioClip echoLeft_Left = Database.hrtf_left_leftspeaker;
    AudioClip echoLeft_Right = Database.hrtf_left_rightspeaker;
    AudioClip echoRight_Left = Database.hrtf_right_leftspeaker;
    AudioClip echoRight_Right = Database.hrtf_right_rightspeaker;
    AudioClip echoLeftEnd_Left = Database.odeon_left_leftspeaker;
    AudioClip echoLeftEnd_Right = Database.odeon_left_rightspeaker;
    AudioClip echoRightEnd_Left = Database.odeon_right_leftspeaker;
    AudioClip echoRightEnd_Right = Database.odeon_right_rightspeaker;
    AudioClip echoLeftFront_Left = Database.hrtf_leftfront_leftspeaker;
    AudioClip echoLeftFront_Right = Database.hrtf_leftfront_rightspeaker;
    AudioClip echoRightFront_Left = Database.hrtf_rightfront_leftspeaker;
    AudioClip echoRightFront_Right = Database.hrtf_rightfront_rightspeaker;

    GvrAudioSource frontGAS = null, leftGAS = null, rightGAS = null, leftFrontGAS = null, rightFrontGAS = null, leftEndGAS = null, rightEndGAS = null;
    GvrAudioSource leftGAS_right = null, rightGAS_left = null, leftFrontGAS_rightfront = null, rightFrontGAS_leftfront = null, leftEndGAS_rightend = null, rightEndGAS_leftend = null;

    float blocksToFrontWall;
    int stepsize;

    bool canCheckForConsent = false;
    bool hasCheckedForConsent = false;
    public static bool hasFinishedConsentForm = false;

    bool canRepeat = true;

    public static bool hasStartedConsent = false;

    bool canPlayLevel = true;

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
        tapped = false;
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

    private IEnumerator DelayedEcho(GvrAudioSource gas, float delay)
    {
        yield return new WaitForSeconds(delay);
        gas.Play();
    }

    // A breakdown of short, medium and long distances
    string[] frontDistS = { "2.25", "3.75" };
    string[] frontDistM = { "5.25", "6.75" };
    string[] frontDistL = { "8.25", "9.75", "11.25", "12.75" };

    private String wallPrefix = "Wall_";
    private String sounderPrefix = "Sounder_";
    private GvrAudioSource MoveAndGetGAS(String sounderName, Vector3 pos)
    {
        GameObject go = GameObject.Find(sounderPrefix + sounderName);
        go.transform.position = pos;
        return go.GetComponent<GvrAudioSource>();
    }

    private String PositionToString(Vector3 pos)
    {
        return (int)pos.x + "_" + (int)pos.y;
    }

    /// <summary>
    /// A function that determines which echo file to play based on the surrounding environment.
    /// </summary>
    private void PlayEcho(GvrAudioSource leftGAS, GvrAudioSource rightGAS, GvrAudioSource leftGAS_right, GvrAudioSource rightGAS_left, GvrAudioSource leftEndGAS, GvrAudioSource rightEndGAS, GvrAudioSource leftEndGAS_rightend, GvrAudioSource rightEndGAS_leftend, GvrAudioSource frontGAS, int stepsize, float blocksToFrontWall, bool real = true)
    {
        if (!real) return;
        if ((GM_title.usingHRTFEchoes == true) && (GM_title.usingOdeonEchoes == false))
        {
            attenuatedClick = Database.attenuatedClickHRTF;          
        }
        else if ((GM_title.usingOdeonEchoes == true) && (GM_title.usingHRTFEchoes == false))
        {
            attenuatedClick = Database.attenuatedClickHRTF;
        }               

        float leftDelay = 0.0f;
        float left_rightDelay = 0.0f;
        float rightDelay = 0.0f;
        float right_leftDelay = 0.0f;
        float leftEndDelay = 0.0f;
        float leftEnd_rightEndDelay = 0.0f;
        float rightEndDelay = 0.0f;
        float rightEnd_leftEndDelay = 0.0f;
        float frontDelay = 0.0f;

        finishedEcho = false;

        float[] volumes = new float[2];
        if (canDoGestureTutorial == true)
        {
            clips = new List<AudioClip>() { attenuatedClick, Database.soundEffectClips[0] };
            volumes = new float[] { 1.0f, 0.0f };
        }
        else if (canDoGestureTutorial == false)
        {
            clips = new List<AudioClip>() { attenuatedClick };
            volumes = new float[] { 1.0f };
        }

        if ((canDoGestureTutorial == true) && (curLevel == 1))
        {
            SoundManager.instance.PlayClips(clips, null, 0, null, 0, volumes, true);
        }
        else if ((canDoGestureTutorial == false) || ((canDoGestureTutorial == true) && (curLevel == 3)))
        {
            SoundManager.instance.PlayClips(clips, null, 0, () =>
            {
                if (leftGAS != null)
                {
                    // print("Playing Left GAS.");
                    leftDelay = 1.5f / 340;
                    left_rightDelay = 1.5f / 340;
                    leftGAS.PlayDelayed(leftDelay);
                    leftGAS_right.PlayDelayed(left_rightDelay);
                }
                else if (leftGAS == null)
                {
                    // print("Left null.");
                }
                if (rightGAS != null)
                {
                    // print("Playing Right GAS.");
                    rightDelay = 1.5f / 340;
                    right_leftDelay = 1.5f / 340;
                    rightGAS.PlayDelayed(rightDelay);
                    rightGAS_left.PlayDelayed(right_leftDelay);
                }
                else if (rightGAS == null)
                {
                    // print("Right null.");
                }
                if (leftEndGAS != null)
                {
                    // print("Playing Left End GAS. Left end is " + stepsize + " blocks away.");
                    leftEndDelay = (1.5f * stepsize) / 340;
                    leftEnd_rightEndDelay = (1.5f * stepsize) / 340;
                    leftEndGAS.PlayDelayed(leftEndDelay);
                    leftEndGAS_rightend.PlayDelayed(leftEnd_rightEndDelay);
                }
                else if (leftEndGAS == null)
                {
                    // print("Left End null.");
                }
                if (rightEndGAS != null)
                {
                    // print("Playing Right End GAS. Right end is " + stepsize + " blocks away.");
                    rightEndDelay = (1.5f * stepsize) / 340;
                    rightEnd_leftEndDelay = (1.5f * stepsize) / 340;
                    rightEndGAS.PlayDelayed(rightEndDelay);
                    rightEndGAS_leftend.PlayDelayed(rightEnd_leftEndDelay);
                }
                else if (rightEndGAS == null)
                {
                    // print("Right End null.");
                }
                if (frontGAS != null)
                {
                    // print("Playing Front GAS.");
                    frontDelay = (1.5f * blocksToFrontWall + 0.75f) * 2 / 340;
                    frontGAS.PlayDelayed(frontDelay);
                }
                else if (frontGAS == null)
                {
                    // print("Front null.");
                }
            }, 0, volumes, true);   
        }

        float waitTime = 0.0f;
        float[] delayTimes = { leftDelay, left_rightDelay, rightDelay, right_leftDelay, leftEndDelay, leftEnd_rightEndDelay, rightEndDelay, rightEnd_leftEndDelay, frontDelay };

        StartCoroutine(PlayEchoClip(waitTime, delayTimes));
    }

    IEnumerator PlayEchoClip(float waitTime, float[] delayTimes = null)
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[8] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
                }
            }
            else if (curLevel >= 12)
            {
                clips = new List<AudioClip>() { Database.soundEffectClips[8] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
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
            if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false) && (finished_reading == false) && (hasFinishedConsentForm == false) && (hasStartedConsent == false))
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
                    finished_listening = false;
                    can_display_window = false;
                }
            }

            if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false) && (finished_reading == false) && (hasFinishedConsentForm == false) && (hasStartedConsent == true))
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
                    finished_listening = false;
                    can_display_window = false;
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

            if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == false) && (finished_listening == true))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
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

            if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == false) && (finished_listening == true))
            {
                if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
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

            if ((readingConsentForm == true) && (answeredQuestion1 == false) && (answeredQuestion2 == false) && (answeredQuestion3 == false))
            {
                if ((canRepeat == true) || (SoundManager.instance.finishedAllClips == true))
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

            if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true) && (hasFinishedConsentForm == false))
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

            if (((hearingConsentForm == true) || (readingConsentForm == true)) && ((answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true)) && ((question1 == false) || (question2 == false) || (question3 == false)) && (hasFinishedConsentForm == false))
            {
                if (question1 == false)
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[19] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                }
                else if (question2 == false)
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[20] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                    }
                }
                else if (question3 == false)
                {
                    if ((SoundManager.instance.finishedAllClips == true) || (canRepeat == true))
                    {
                        canRepeat = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.consentClips[21] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
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

        Vector2 playerPos = BoardManager.player_idx;
        Vector2 startPos = BoardManager.start_idx;
        Vector2 exitPos = BoardManager.exit_idx;

        if (hasFinishedConsentForm == true)
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
                            if ((canPlayClip[0, 1] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel1 == true) && (ratio <= 0.5f) && (ratio >= 0.332f))
                            {
                                canPlayClip[0, 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 1.
                            else if ((canPlayClip[0, 2] == false) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel1 == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
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
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                            }
                        }
                        // If we are in level 2.
                        else if (curLevel == 2)
                        {
                            // If the player has reached halfway.
                            if ((canPlayClip[1, 1] == true) && (ratio <= 0.5f) && (ratio >= 0.332f))
                            {
                                canPlayClip[1, 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 2.
                            else if ((canPlayClip[1, 2] == false) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing
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
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                            }
                        }
                        // If we are in level 3.
                        else if (curLevel == 3)
                        {
                            if ((canPlayClip[2, 4] == false) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 6) && (playerPos.y == 9))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                            }
                            // Play clip for when they approach the corner in level 3.
                            else if ((canPlayClip[2, 4] == true) && (canDoGestureTutorial == false) && (playerPos.x == 6) && (playerPos.y == 9))
                            {
                                canPlayClip[2, 4] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[12] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            else if ((canPlayClip[2, 5] == false) && (canPlayClip[2, 6] == false) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                            }
                            // Play clip for when the player reaches the corner in level 3.
                            else if ((canPlayClip[2, 5] == true) && (canPlayClip[2, 6] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                canPlayClip[2, 5] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[13], Database.mainGameClips[14] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                            }
                            // Allow the player to tap once to see what the echo at a right turn sounds like before starting the gesture tutorial.
                            else if ((canPlayClip[2, 5] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == false) && (playerPos.x == 9) && (playerPos.y == 9))
                            {
                                canPlayClip[2, 5] = false;
                            }
                            // If the player has reached halfway.
                            else if ((canPlayClip[2, 1] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (ratio <= 0.5f) && (ratio >= 0.4665f))
                            {
                                canPlayClip[2, 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 3.
                            else if ((canPlayClip[2, 2] == false) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[2, 3] == true) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                // Keep this check in, but do nothing.
                            }
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.

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
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                            }
                            // Play clip for when they approach the corner in level 5.
                            else if ((canPlayClip[4, 4] == true) && (playerPos.x == 4) && (playerPos.y == 9))
                            {
                                canPlayClip[4, 4] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[12] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the corner in level 5.
                            else if ((canPlayClip[4, 5] == true) && (playerPos.x == 1) && (playerPos.y == 9) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.LEFT))
                            {
                                canPlayClip[4, 5] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[20] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached halfway.
                            else if ((canPlayClip[4, 1] == true) && (ratio <= 0.5f) && (ratio >= 0.4465f))
                            {
                                canPlayClip[4, 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 5.
                            else if ((canPlayClip[4, 2] == false) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[4, 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                // Keep this check in, but do nothing.
                            }
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.

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
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                            }
                            else if ((canPlayClip[10, 4] == false) && (playerPos.x >= 2) && (playerPos.y <= 6))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[10, 1] = true;
                                }
                                if ((playerPos.x != startPos.x) && (playerPos.y != startPos.y) && (BoardManager.left_start_pt == true))
                                {
                                    canPlayClip[10, 3] = true;
                                }
                            }
                            // If the player has reached halfway and they have not gone into an arm of the T hallway or went back into the start hallway.
                            else if ((canPlayClip[10, 1] == false) && (ratio <= 0.5f) && (ratio >= 0.4665f) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.FRONT))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            // Play clips for when player hits the T intersection in level 11.
                            else if ((canPlayClip[10, 4] == true) && (playerPos.x == 5) && (playerPos.y == 6) && (GameManager.instance.boardScript.get_player_dir_world() == BoardManager.Direction.FRONT))
                            {
                                if (canPlayClip[10, 1] == true)
                                {
                                    canPlayClip[10, 1] = false;
                                }
                                canPlayClip[10, 4] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30], Database.soundEffectClips[0], Database.mainGameClips[27] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            else if ((canPlayClip[10, 4] == true) && (playerPos.x >= 2) && (playerPos.y <= 6))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.

                                if ((ratio > 0.5f) || (ratio < 0.4665f))
                                {
                                    canPlayClip[10, 1] = true;
                                }
                                if ((playerPos.x != startPos.x) && (playerPos.y != startPos.y) && (BoardManager.left_start_pt == true))
                                {
                                    canPlayClip[10, 3] = true;
                                }
                            }
                            // If the player has reached halfway and they have gone into an arm of the T hallway.
                            else if ((canPlayClip[10, 1] == false) && (ratio <= 0.5f) && (ratio >= 0.4665f) && (GameManager.instance.boardScript.get_player_dir_world() != BoardManager.Direction.FRONT))
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clips.
                            }
                            // If the player has reached the exit at level 11.
                            else if ((canPlayClip[10, 2] == false) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[10, 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                // Keep this check in, but do nothing.
                            }
                        }
                        // If we are in another tutorial level
                        else
                        {
                            // If the player has reached halfway.
                            if ((canPlayClip[(curLevel - 1), 1] == true) && (ratio <= 0.5f) && (ratio >= 0.4665f))
                            {
                                canPlayClip[(curLevel - 1), 1] = false;
                                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[30] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                            }
                            // If the player has reached the exit on another level.
                            else if ((canPlayClip[(curLevel - 1), 2] == false) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // If the player returns to their start position.
                            else if ((canPlayClip[(curLevel - 1), 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                            {
                                // Keep this check in, but do nothing.
                            }
                            // Otherwise play a normal swipe up.
                            else
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.

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
                            canPlayClip[10, 2] = true;
                            canPlayClip[10, 3] = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                        }
                        // If the player has reached the exit.
                        else if ((canPlayClip[11, 2] == true) && (playerPos.x == exitPos.x) && (playerPos.y == exitPos.y))
                        {
                            // Keep this check in, but do nothing.
                            canPlayClip[10, 1] = true;
                            canPlayClip[10, 3] = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                        }
                        // If the player returns to their start position.
                        else if ((canPlayClip[11, 3] == true) && (playerPos.x == startPos.x) && (playerPos.y == startPos.y) && (BoardManager.left_start_pt == true) && (get_player_dir("BACK") == BoardManager.startDir))
                        {
                            canPlayClip[11, 3] = false;
                            canPlayClip[10, 1] = true;
                            canPlayClip[10, 2] = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                        }
                        else
                        {
                            canPlayClip[10, 1] = true;
                            canPlayClip[10, 2] = true;
                            canPlayClip[10, 3] = true;
                            clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[5] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
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
                    clips = new List<AudioClip>() { Database.soundEffectClips[6] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
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
                SoundManager.instance.PlayClips(clips, null, 0, () => {
                    canCheckForConsent = true;
                }, 2, null, true); // Play the appropriate clips.
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

    void Update()
    {
        Vector3 echoDir = transform.right;
        int echoDirX = (int)Math.Round(echoDir.x);
        int echoDirY = (int)Math.Round(echoDir.y);
        int echoX = (int)transform.position.x;
        int echoY = (int)transform.position.y;

        GameObject frontWall, leftWall, rightWall, leftFrontWall, rightFrontWall, rightEndWall, leftEndWall;
        Vector3 frontWallPos, frontWallLeftPos, frontWallRightPos, leftWallPos, rightWallPos, leftFrontWallPos, rightFrontWallPos, rightEndWallPos, leftEndWallPos;

        leftWallPos = new Vector3((echoX - echoDirY), (echoY + echoDirX));
        rightWallPos = new Vector3((echoX + echoDirY), (echoY - echoDirX));
        leftFrontWallPos = new Vector3((echoX - echoDirY + echoDirX), (echoY + echoDirX + echoDirY));
        rightFrontWallPos = new Vector3((echoX + echoDirY + echoDirX), (echoY - echoDirX + echoDirY));
        leftEndWallPos = new Vector3();
        rightEndWallPos = new Vector3();

        leftWall = GameObject.Find(wallPrefix + PositionToString(leftWallPos));
        rightWall = GameObject.Find(wallPrefix + PositionToString(rightWallPos));
        leftFrontWall = GameObject.Find(wallPrefix + PositionToString(leftFrontWallPos));
        rightFrontWall = GameObject.Find(wallPrefix + PositionToString(rightFrontWallPos));

        leftEndWall = null;
        rightEndWall = null;
        stepsize = 1;
        if (leftWall == null)
        {
            stepsize = 1;
            while (leftEndWall == null)
            {
                leftEndWallPos = new Vector3((echoX - echoDirY * stepsize), (echoY + echoDirX * stepsize));
                leftEndWall = GameObject.Find(wallPrefix + PositionToString(leftEndWallPos));
                stepsize += 1;
            }
        }
        if (rightWall == null)
        {
            stepsize = 1;
            while (rightEndWall == null)
            {
                rightEndWallPos = new Vector3((echoX + (echoDirY * stepsize)), (echoY - (echoDirX * stepsize)));
                rightEndWall = GameObject.Find(wallPrefix + PositionToString(rightEndWallPos));
                stepsize += 1;
            }
        }
        int _x = echoX, _y = echoY;
        do
        {
            _x += echoDirX;
            _y += echoDirY;
            frontWallPos = new Vector3(_x, _y);
            frontWallLeftPos = new Vector3((_x - echoDirY), (_y + echoDirX));
            frontWallRightPos = new Vector3((_x + echoDirY), (_y - echoDirX));
            frontWall = GameObject.Find(wallPrefix + PositionToString(frontWallPos));
        }
        while (frontWall == null);

        blocksToFrontWall = Vector3.Distance(transform.position, frontWall.transform.position) - 1;
        if (blocksToFrontWall >= 0.0f)
        {
            // print("Blocks To Front Wall: " + blocksToFrontWall.ToString());
        }
        // Four-wall echoes preparation
        frontGAS = null;
        leftGAS = null;
        rightGAS = null;
        leftFrontGAS = null;
        rightFrontGAS = null;
        leftEndGAS = null;
        rightEndGAS = null;
        leftGAS_right = null;
        rightGAS_left = null;
        leftFrontGAS_rightfront = null;
        rightFrontGAS_leftfront = null;
        leftEndGAS_rightend = null;
        rightEndGAS_leftend = null;
        // Now 45db is inactive
        float horizontal_45db = -10.3f;

        float horizontaldb = 12f;
        float frontwalldb = 20f;
        float farenddb = -10;
        // float fourblockdb = -13.7f;
        // float frontwalldb = -5.7f;

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

        frontGAS = MoveAndGetGAS("Front", frontWallPos);
        frontGAS.gainDb = frontwalldb;
        frontGAS.clip = echoFront;
        if (leftWall != null)
        {
            leftGAS = MoveAndGetGAS("Left", leftWallPos);
            leftGAS.clip = echoLeft_Left;
            leftGAS.gainDb = horizontaldb;
            leftGAS_right = MoveAndGetGAS("Left_Right", rightWallPos);
            leftGAS_right.clip = echoLeft_Right;
            leftGAS_right.gainDb = horizontaldb;
        }
        else
        {
            leftEndGAS = MoveAndGetGAS("LeftEnd", leftEndWallPos);
            leftEndGAS.clip = echoLeftEnd_Left;
            leftEndGAS.gainDb = farenddb;
            Vector3 leftEnd_RightPos = new Vector3(echoX * 2 - (int)leftEndWallPos.x, echoY * 2 - (int)leftEndWallPos.y);
            leftEndGAS_rightend = MoveAndGetGAS("LeftEnd_Right", leftEnd_RightPos);
            leftEndGAS_rightend.clip = echoLeftEnd_Right;
            leftEndGAS_rightend.gainDb = farenddb;
        }

        if (rightWall != null)
        {
            rightGAS = MoveAndGetGAS("Right", rightWallPos);
            rightGAS.clip = echoRight_Right;
            rightGAS.gainDb = horizontaldb;
            rightGAS_left = MoveAndGetGAS("Right_Left", leftWallPos);
            rightGAS_left.clip = echoRight_Left;
            rightGAS_left.gainDb = horizontaldb;
        }
        else
        {
            rightEndGAS = MoveAndGetGAS("RightEnd", rightEndWallPos);
            rightEndGAS.clip = echoRightEnd_Right;
            rightEndGAS.gainDb = farenddb;
            Vector3 rightEnd_LeftPos = new Vector3(echoX * 2 - (int)rightEndWallPos.x, echoY * 2 - (int)rightEndWallPos.y);
            rightEndGAS_leftend = MoveAndGetGAS("RightEnd_Left", rightEnd_LeftPos);
            rightEndGAS_leftend.clip = echoRightEnd_Left;
            rightEndGAS_leftend.gainDb = farenddb;
        }

        Vector3 RealLeftFrontPos = new Vector3(echoX - (echoX - (int)leftWallPos.x) * (float)Math.Sqrt(2), echoY - (echoY - (int)leftWallPos.y) * (float)Math.Sqrt(2));
        Vector3 RealRightFrontPos = new Vector3(echoX - (echoX - (int)rightWallPos.x) * (float)Math.Sqrt(2), echoY - (echoY - (int)rightWallPos.y) * (float)Math.Sqrt(2));
        if (blocksToFrontWall > 0 && leftFrontWall != null && leftWall != null)
        {
            leftFrontGAS = MoveAndGetGAS("LeftFront", RealLeftFrontPos);
            leftFrontGAS.clip = echoLeftFront_Left;
            leftFrontGAS.gainDb = horizontal_45db;
            leftFrontGAS_rightfront = MoveAndGetGAS("LeftFront_Right", RealRightFrontPos);
            leftFrontGAS_rightfront.clip = echoLeftFront_Right;
            leftFrontGAS_rightfront.gainDb = horizontal_45db;
        }

        if (blocksToFrontWall > 0 && rightFrontWall != null && rightWall != null)
        {
            rightFrontGAS = MoveAndGetGAS("RightFront", RealRightFrontPos);
            rightFrontGAS.clip = echoRightFront_Right;
            rightFrontGAS.gainDb = horizontal_45db;
            rightFrontGAS_leftfront = MoveAndGetGAS("RightFront_Left", RealLeftFrontPos);
            rightFrontGAS_leftfront.clip = echoRightFront_Left;
            rightFrontGAS_leftfront.gainDb = horizontal_45db;
        }

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
        else if ((curLevel == 3) && (BoardManager.player_idx.x == 9) && (BoardManager.player_idx.y == 9) && (canDoGestureTutorial == false) && (BoardManager.finishedTutorialLevel3 == false))
        {
            print("Can do gesture tutorial level 3");
            canDoGestureTutorial = true;

            // Play the first level 3 gesture tutorial clip and reset necessary variables.
            debugPlayerInfo = "Playing tutorial level 3 clips.";
            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
            finishedCornerInstruction = false; // Reset if the player is going through this tutorial level again.
            finishedTurningInstruction = false; // Reset if the player is going through this tutorial level again.             
            level3_remaining_turns = 4; // Set the remaining rotations for the tutorial to 4.
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
                debugPlayerInfo = "Returned to start position. Turn around by rotating twice, then continue by moving forward.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[29] };
                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
            }
            else if ((curLevel >= 12) && (canPlayClip[11, 3] == true))
            {
                canPlayClip[11, 3] = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[29] };
                SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
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
                        clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[8] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                    }
                    else if (curLevel == 2)
                    {
                        canPlayClip[1, 2] = false;
                        debugPlayerInfo = "Playing found level 2 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[8] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                    }
                    else if (curLevel == 3)
                    {
                        canPlayClip[2, 2] = false;
                        debugPlayerInfo = "Playing found level 3 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[16] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                    }
                    else if (curLevel == 4)
                    {
                        canPlayClip[3, 2] = false;
                        debugPlayerInfo = "Playing found level 4 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[18] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                    }
                    else if (curLevel == 11)
                    {
                        canPlayClip[10, 2] = false;
                        debugPlayerInfo = "Playing found level 11 exit clip.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo);
                        clips = new List<AudioClip>() { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.mainGameClips[28] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
                    }
                    else
                    {
                        canPlayClip[(curLevel - 1), 2] = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[4] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall), 1, null, true); // Play the appropriate clip.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clip.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readProcedures == false) && (proceduresFlag == true) && (ad.noclicked() == true))
            {
                proceduresFlag = false;
                readConsent = false;
                consentFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRequirements == false) && (requirementsFlag == true) && (ad.noclicked() == true))
            {
                requirementsFlag = false;
                readConsent = true;
                readProcedures = false;
                proceduresFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readRisks == false) && (risksFlag == true) && (ad.noclicked() == true))
            {
                risksFlag = false;
                readProcedures = true;
                readRequirements = false;
                requirementsFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readBenefits == false) && (benefitsFlag == true) && (ad.noclicked() == true))
            {
                benefitsFlag = false;
                readRequirements = true;
                readRisks = false;
                risksFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readCompCost == false) && (compCostFlag == true) && (ad.noclicked() == true))
            {
                compCostFlag = false;
                readRisks = true;
                readBenefits = false;
                benefitsFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readConfidentiality == false) && (confidentialityFlag == true) && (ad.noclicked() == true))
            {
                confidentialityFlag = false;
                readBenefits = true;
                readCompCost = false;
                compCostFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readQuestionsContact == false) && (questionsContactFlag == true) && (ad.noclicked() == true))
            {
                questionsContactFlag = false;
                readCompCost = true;
                readConfidentiality = false;
                confidentialityFlag = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readEighteenPlus == false) && (eighteenPlusFlag == true) && (ad.noclicked() == true))
            {
                readEighteenPlus = true;
                answeredQuestion1 = true;
                question1 = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readUnderstand == false) && (understandFlag == true) && (ad.noclicked() == true))
            {
                readUnderstand = true;
                answeredQuestion2 = true;
                question2 = false;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // If they are using Talkback, play the correct instructions.
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
                can_display_window = false;
                finished_reading = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, () => {
                    canRepeat = true;
                }, 1, null, true);
                ad.clearflag();
            }

            if ((readingConsentForm == true) && (android_window_displayed == true) && (finished_reading == false) && (readParticipate == false) && (participateFlag == true) && (ad.noclicked() == true))
            {
                readParticipate = true;
                answeredQuestion3 = true;
                question3 = false;
                android_window_displayed = false;
                can_display_window = false;
                finished_reading = true;
                clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                SoundManager.instance.PlayClips(clips, null, 0, () => {
                    canRepeat = true;
                }, 1, null, true);
                ad.clearflag();
            }
        }
#endif

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

            if ((curLevel == 3) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false) && (BoardManager.player_idx.x == 9) && (BoardManager.player_idx.y == 9) && (finishedEcho == true))
            {
                hasTappedAtCorner = true;
                finishedEcho = false;
            }

            if ((curLevel == 3) && (finishedCornerInstruction == false) && (hasTappedAtCorner == false) && (BoardManager.player_idx.x == 9) && (BoardManager.player_idx.y == 9) && (finishedEcho == true))
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

            if ((curLevel == 3) && (hasTappedAtCorner == true) && (SoundManager.instance.finishedAllClips == true) && (level3_remaining_turns == 4))
            {
                if (GM_title.isUsingTalkback == true)
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[25], Database.soundEffectClips[0], Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[26], Database.tutorialClips[27] };
                    SoundManager.instance.PlayClips(clips, null, 0, () =>
                    {
                        debugPlayerInfo = "Finished turning instruction.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.             
                        finishedTurningInstruction = true; // We have finished the turning instruction, so the player can rotate.
                    }, 7, null, true); // If they are using Talkback, play the correct instructions.                
                }
                else if (GM_title.isUsingTalkback == false)
                {
                    clips = new List<AudioClip>() { Database.soundEffectClips[0], Database.tutorialClips[24], Database.soundEffectClips[0], Database.soundEffectClips[6], Database.soundEffectClips[0], Database.tutorialClips[26], Database.tutorialClips[27] };
                    SoundManager.instance.PlayClips(clips, null, 0, () =>
                    {
                        debugPlayerInfo = "Finished turning instruction.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.             
                        finishedTurningInstruction = true; // We have finished the turning instruction, so the player can rotate.
                    }, 7, null, true); // If they are not using Talkback, play the correct instructions.                
                }
            }
        }

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

        if ((hasFinishedConsentForm == true) && (SoundManager.instance.finishedAllClips == true))
        {
            canPlayLevel = true;
        }


        Vector3 dir = Vector3.zero;
        // Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        // Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction.
        if (eh.isActivate() && ((hasFinishedConsentForm == false) || ((hasFinishedConsentForm == true) && (canPlayLevel == true))))
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
                            clips = new List<AudioClip> { attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[5] };
                            float[] volumes = new float[] { 1.0f, 0.5f, 0.5f };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                finishedEcho = true;
                            }, 0, volumes, true); // This tap was correct. Please tap 2 more times.
                        }
                        else if (level1_remaining_taps == 1)
                        {
                            clips = new List<AudioClip> { attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[6] };
                            float[] volumes = new float[] { 1.0f, 0.5f, 0.5f };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                finishedEcho = true;
                            }, 0, volumes, true); // This tap was correct. Please tap 1 more time.                     
                        }
                        // If the player has finished the tapping section.
                        else if (level1_remaining_taps == 0)
                        {
                            debugPlayerInfo = "Finished tapping section for gesture tutorial.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip> { attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[7], Database.tutorialClips[9], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
                                float[] volumes = new float[] { 1.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    finishedEcho = true;
                                    haveTappedThreeTimes = true;
                                }, 1, volumes, true); // If they are not using Talkback, play the correct instructions.                  
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip> { attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[7], Database.tutorialClips[8], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
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

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                {
                    debugPlayerInfo = "Tapped at corner. Played echo.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                        
                    PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall);
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
                
                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
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

                if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                {
                    // If the player is not in the pause menu, play an echo.
                    if ((want_exit == false) && (loadingScene == false))
                    {
                        if ((finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                        {
                            // Do nothing.
                        }

                        else
                        {
                            debugPlayerInfo = "Tap registered. Played echo.";
                        }
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        GameManager.instance.boardScript.gamerecord += "E{"; // Record the echo.
                        PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall); // Play the echo.
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
                        Utilities.writefile("consentRecord", "0");
                        debugPlayerInfo = "Tap registered. Did not consent to having data collected. Can continue with level " + curLevel.ToString() + ".";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        hasFinishedConsentForm = true;
                        canPlayLevel = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[23], Database.levelStartClips[curLevel], Database.consentClips[24], Database.levelStartClips[curLevel] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            hasCheckedForConsent = true;
                            canRepeat = true;
                        }, 6, null, true);
                    }

                    else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
                    {
                        Utilities.writefile("consentRecord", "1");
                        debugPlayerInfo = "Tap registered. Consented to having data collected. Can continue with level " + curLevel.ToString() + ".";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                      
                        hasFinishedConsentForm = true;
                        canPlayLevel = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[22], Database.levelStartClips[curLevel], Database.consentClips[24], Database.levelStartClips[curLevel] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {
                            hasCheckedForConsent = true;                            
                            canRepeat = true;
                        }, 6, null, true);
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                        if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                        {
                            hearingConsentForm = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Reading consent form through audio instructions.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0], Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[4] };
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0], Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[3] };
                            }
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
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
                            finished_questions = true;
                            debugPlayerInfo = "Swipe left registered. Does not want to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
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
                        else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                        if ((readingConsentForm == false) && (hearingConsentForm == false) && (noConsent == false))
                        {
                            readingConsentForm = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Reading consent form manually.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
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
                            finished_reading = true;
                            finished_questions = true;
                            debugPlayerInfo = "Swipe right registered. Wants to participate.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.

                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
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
                        else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
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
                                SoundManager.instance.PlayClips(clips, null, 0, () => {
                                    haveSwipedThreeTimes = true;
                                }, 4, null, true); // If they are using Talkback, play the correct instructions.
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip> { Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[13], Database.tutorialClips[14] };
                                SoundManager.instance.PlayClips(clips, null, 0, () => {
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                // If the down arrow key has been pressed.d
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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

                            debugPlayerInfo = "Swipe down registered. Deciding not to give consent.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                           

                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => { 
                                canRepeat = true;                                
                                hasCheckedForConsent = true;
                            }, 1, null, true);
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
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

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
        if (eh.isActivate() && ((hasFinishedConsentForm == false) || ((hasFinishedConsentForm == true) && (canPlayLevel == true))))
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
                            clips = new List<AudioClip> { attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[5] };
                            float[] volumes = new float[] { 1.0f, 0.5f, 0.5f };
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                finishedEcho = true;
                            }, 0, volumes, true); // This tap was correct. Please tap 2 more times.
                        }
                        else if (level1_remaining_taps == 1)
                        {
                            clips = new List<AudioClip> { attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[6] };
                            float[] volumes = new float[] { 1.0f, 0.5f, 0.5f };
                            SoundManager.instance.PlayClips(clips, null, 0, () =>
                            {
                                finishedEcho = true;
                            }, 0, volumes, true); // This tap was correct. Please tap 1 more time.                     
                        }
                        // If the player has finished the tapping section.
                        else if (level1_remaining_taps == 0)
                        {
                            debugPlayerInfo = "Finished tapping section for gesture tutorial.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip> { attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[7], Database.tutorialClips[9], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
                                float[] volumes = new float[] { 1.0f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
                                SoundManager.instance.PlayClips(clips, null, 0, () =>
                                {
                                    finishedEcho = true;
                                    haveTappedThreeTimes = true;
                                }, 1, volumes, true); // If they are not using Talkback, play the correct instructions.                  
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip> { attenuatedClick, Database.soundEffectClips[0], Database.tutorialClips[7], Database.tutorialClips[8], Database.soundEffectClips[0], Database.soundEffectClips[4], Database.soundEffectClips[0], Database.tutorialClips[10] };
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

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                {
                    debugPlayerInfo = "Tapped at corner. Played echo.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                        
                    PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall);
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

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
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

                if ((hasFinishedConsentForm == true) && (canDoGestureTutorial == false))
                {
                    // If the player is not in the pause menu, play an echo.
                    if ((want_exit == false) && (loadingScene == false))
                    {
                        if ((finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                        {
                            // Do nothing.
                        }

                        else
                        {
                            debugPlayerInfo = "Tap registered. Played echo.";
                        }
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        GameManager.instance.boardScript.gamerecord += "E{"; // Record the echo.
                        PlayEcho(leftGAS, rightGAS, leftGAS_right, rightGAS_left, leftEndGAS, rightEndGAS, leftEndGAS_rightend, rightEndGAS_leftend, frontGAS, stepsize, blocksToFrontWall); // Play the echo.
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

                        }
                    }
                }

                if ((hasFinishedConsentForm == false) && (hasStartedConsent == true))
                {
                    if (noConsent == true)
                    {
                        Utilities.writefile("consentRecord", "0");
                        debugPlayerInfo = "Tap registered. Did not consent to having data collected. Can continue with level " + curLevel.ToString() + ".";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                        
                        hasFinishedConsentForm = true;
                        canPlayLevel = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[23], Database.levelStartClips[curLevel], Database.consentClips[24], Database.levelStartClips[curLevel] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {                            
                            hasCheckedForConsent = true;
                            canRepeat = true;
                        }, 6, null, true);
                    }

                    else if (((hearingConsentForm == true) || (readingConsentForm == true)) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (question1 == true) && (question2 == true) && (question3 == true))
                    {
                        Utilities.writefile("consentRecord", "1");
                        debugPlayerInfo = "Tap registered. Consented to having data collected. Can continue with level " + curLevel.ToString() + ".";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        hasFinishedConsentForm = true;
                        canPlayLevel = false;
                        clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[22], Database.levelStartClips[curLevel], Database.consentClips[24], Database.levelStartClips[curLevel] };
                        SoundManager.instance.PlayClips(clips, null, 0, () => {                            
                            hasCheckedForConsent = true;
                            canRepeat = true;
                        }, 6, null, true);
                    }

                    else
                    {
                        debugPlayerInfo = "Tap registered. Does nothing here.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                        if ((hearingConsentForm == false) && (readingConsentForm == false) && (noConsent == false))
                        {
                            hearingConsentForm = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe left registered. Reading consent form through audio instructions.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                            
                            if (GM_title.isUsingTalkback == true)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0], Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[4] };
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip>() { Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[2], Database.soundEffectClips[0], Database.soundEffectClips[7], Database.soundEffectClips[0], Database.consentClips[3] };
                            }
                            SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
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

                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
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
                        else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
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
                            noConsent = false;
                            hearingConsentForm = true;
                            readingConsentForm = false;
                            hasCheckedForConsent = true;
                            hasFinishedConsentForm = false;
                            finished_listening = false;
                            android_window_displayed = false;
                            can_display_window = false;
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                        if ((readingConsentForm == false) && (hearingConsentForm == false) && (noConsent == false))
                        {
                            readingConsentForm = true;
                            canRepeat = true;
                            debugPlayerInfo = "Swipe right registered. Reading consent form manually.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
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

                            clips = new List<AudioClip>() { Database.soundEffectClips[7] };
                            SoundManager.instance.PlayClips(clips, null, 0, () => {
                                canRepeat = true;
                                hasCheckedForConsent = true;
                            }, 1, null, true);
                        }
                        else if ((hearingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
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
                        else if ((readingConsentForm == true) && (answeredQuestion1 == true) && (answeredQuestion2 == true) && (answeredQuestion3 == true) && (finished_questions == true) && ((question1 == false) || (question2 == false) || (question3 == false)))
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                    {
                        debugPlayerInfo = "Please wait for the instructions to finish.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                    {
                        debugPlayerInfo = "Incorrect gesture made. You should tap.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
                    }

                    else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == true))
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

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                }

                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
                {
                    debugPlayerInfo = "Incorrect gesture made. You should tap.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.PlayVoice(Database.errorClips[18], true, 0.0f, 0.0f, 0.5f); // Play the appropriate clip.
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
                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedCornerInstruction == false))
                {
                    debugPlayerInfo = "Please wait for the instructions to finish.";
                }
                else if ((canDoGestureTutorial == true) && (curLevel == 3) && (finishedTurningInstruction == false) && (finishedCornerInstruction == true) && (hasTappedAtCorner == false))
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
