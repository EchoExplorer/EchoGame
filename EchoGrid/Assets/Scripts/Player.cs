using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Allows us to use UI.
using System.Collections.Generic;
using SimpleJSON;
using System.Security.Cryptography;
using System;
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
    bool want_exit;
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

    bool finishedTappingInstruction = false;
    bool finishedSwipingInstruction = false;
    bool finishedMenuInstruction = false;
    bool finishedTurningInstruction = false;

	public int level1_remaining_taps = -1; // Gesture tutorial level 1 remaining taps. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
	public int level1_remaining_ups = -1; // Gesture tutorial level 1 remaining swipes up. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
	public int level1_remaining_menus = -1; // Gesture tutorial level 1 remaining holds for pause menu. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
	public int level3_remaining_turns = -1; // Gesture tutorial level 3 remaining turns/rotations. Initially set to -1 as there are checks for if they are greater or equal to 0, and we don't want hints playing at the wrong time.
    enum InterceptAction {NONE, UP, DOWN, LEFT, RIGHT, TAP, DOUBLE_TAP, TRIPLE_TAP, MENU};

    string surveyCode = "";

    bool wantLevelRestart = false; // Used to make sure the player has to tap once after swiping left in the pause menu to confirm they want to restart the level.
    bool wantMainMenu = false; // Used to make sure the player has to tap once after swiping right in the pause menu to confirm they want to go to the main menu.

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

    // A breakdown of short, medium and long distances
    string[] frontDistS = { "2.25", "3.75" };
    string[] frontDistM = { "5.25", "6.75" };
    string[] frontDistL = { "8.25", "9.75", "11.25", "12.75" };

    /// <summary>
    /// A function that determines which echo file to play based on the surrounding environment.
    /// </summary>
	private void PlayEcho()
    {
        Vector3 dir = transform.right;
        int dir_x = (int)dir.x;
        int dir_y = (int)dir.y;
        int x = (int)transform.position.x;
        int y = (int)transform.position.y;
        //print("Position: " + transform.position);/////
        //print("Rotation: " + transform.rotation);/////
        //print("Forward: " + transform.forward);/////
        //print("Right: " + transform.right);/////
        //print("Up: " + transform.up);/////
        //print("Facing: " + dir_x + ", " + dir_y);
        
        GameObject frontWall, leftWall, rightWall, leftFrontWall, rightFrontWall, tempWall;
        leftWall = GameObject.Find("Wall_" + x + "_" + y);
        rightWall = GameObject.Find("Wall_" + x + "_" + y);
        // assume dir.x != 0
        leftWall = GameObject.Find("Wall_" + (x + dir_y) + "_" + (y + dir_x));
        rightWall = GameObject.Find("Wall_" + (x + -dir_y) + "_" + (y + -dir_x));
        leftFrontWall = GameObject.Find("Wall_" + (x + dir_y + dir_x) + "_" + (y + dir_x + dir_y));
        rightFrontWall = GameObject.Find("Wall_" + (x + -dir_y + dir_x) + "_" + (y + -dir_x + dir_y));
        if (dir.y != 0)
        {
            tempWall = leftWall;
            leftWall = rightWall;
            rightWall = tempWall;
            tempWall = leftFrontWall;
            leftFrontWall = rightFrontWall;
            rightFrontWall = tempWall;
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
        playerGAS.clip = Database.instance.attenuatedClick;
        // Front wall echo preparation
        GvrAudioSource frontGAS = frontWall.GetComponent<GvrAudioSource>();
		frontGAS.clip = Database.instance.attenuatedClickfront;
        float blocksToFrontWall = Vector3.Distance(transform.position, frontWall.transform.position) - 1;
        // Four-wall echoes preparation
        GvrAudioSource leftGAS = null, rightGAS = null, leftFrontGAS = null, rightFrontGAS = null;
		float fourblockdb = 5;
		float frontwalldb = 2;
        if (leftWall != null)
        {
            leftGAS = leftWall.GetComponent<GvrAudioSource>();
			leftGAS.clip = Database.instance.attenuatedaround;
			leftGAS.gainDb=fourblockdb;

        }
        if (rightWall != null)
        {
            rightGAS = rightWall.GetComponent<GvrAudioSource>();
			rightGAS.clip = Database.instance.attenuatedaround;
			rightGAS.gainDb = fourblockdb;
        }
		if (frontWall==null && leftFrontWall != null)
        {
            if (((int)blocksToFrontWall) != 0 || leftWall == null)
            {
                leftFrontGAS = leftFrontWall.GetComponent<GvrAudioSource>();
				leftFrontGAS.clip = Database.instance.attenuatedaround;
				leftFrontGAS.gainDb=fourblockdb;
            }
        }
		if (frontWall==null && rightFrontWall != null)
        {
            if (((int)blocksToFrontWall) != 0 || rightWall == null)
            {
                rightFrontGAS = rightFrontWall.GetComponent<GvrAudioSource>();
				rightFrontGAS.clip = Database.instance.attenuatedaround;
				rightFrontGAS.gainDb=fourblockdb;
            }
        }
        // Play all echoes
		// The SoundManager would be interrupted by GVR, Use GVR or Coroutine to avoid this. 
        //playerGAS.Play(); 
		SoundManager.instance.PlaySingle(Database.instance.attenuatedClick);
		if (leftGAS != null) {
			leftGAS.PlayDelayed (1.5f / 340);
			UnityEngine.Debug.Log ("Left wall played");
		}
		if (rightGAS != null){
            rightGAS.PlayDelayed(1.5f / 340);
			UnityEngine.Debug.Log ("Right wall played");
		}
		if (frontWall==null && leftFrontGAS != null){
            leftFrontGAS.PlayDelayed(2.12132f / 340);
			UnityEngine.Debug.Log ("LeftFront wall played");
		}
		if (frontWall==null && rightFrontGAS != null){
            rightFrontGAS.PlayDelayed(2.12132f / 340);
			UnityEngine.Debug.Log ("RightFront wall played");
		}
        frontGAS.PlayDelayed((1.5f * blocksToFrontWall + 0.75f) * 2 / 340);
        return;
        tapped = true;
        reportSent = true;
        BoardManager.echoDistData data = GameManager.instance.boardScript.getEchoDistData(transform.position, get_player_dir("FRONT"), get_player_dir("LEFT"));

        // Logging.Log(data.all_jun_to_string(), Logging.LogLevel.NORMAL);
        String prefix = "C00-21"; // change this prefix when you change the echo files
        if (GameManager.instance.level < 26)
        {
            prefix = "C00 - 21";
        }
        else if ((GameManager.instance.level >= 26) && (GameManager.instance.level < 41))
        {
            prefix = "19 dB/C00-19";
        }
        else if ((GameManager.instance.level >= 41) && (GameManager.instance.level < 56))
        {
            prefix = "17 dB/C00-17";
        }
        else // if ((GameManager.instance.level >= 56) && (GameManager.instance.level < 71))
        {
            prefix = "15 dB/C00-15";
        }
        /*
		else if ((GameManager.instance.level >= 71) && (GameManager.instance.level < 86))
        {
			prefix = "13 dB/C00-13";
		}
        else if ((GameManager.instance.level >= 86) && (GameManager.instance.level < 101))
		{
            prefix = "11 dB/C00-11";
		}
        else if ((GameManager.instance.level >= 101) && (GameManager.instance.level < 116))
		{
            prefix = "9 dB/C00-9";
		}
        else if (GameManager.instance.level >= 116)
		{
            prefix = "7 dB/C00-7";
        }
		*/

        String filename;
        float wallDist = 0.8f, shortDist = 3.8f, midDist = 6.8f, longDist = 12.8f;
        dist_type f_dtype, b_dtype, l_dtype, r_dtype;
        string front_type = data.jun_to_string(data.fType), back_type = "D", left_type = "D", right_type = "D";

        // catogrize the distance
        // front
        if (data.frontDist <= wallDist)
        {
            f_dtype = dist_type.WALL;
        }
        else if ((data.frontDist > wallDist) && (data.frontDist <= shortDist))
        {
            f_dtype = dist_type.SHORT;
        }
        else if ((data.frontDist > shortDist) && (data.frontDist <= midDist))
        {
            f_dtype = dist_type.MID;
        }
        else
        {
            f_dtype = dist_type.LONG;
        }
        // back
        if (data.backDist <= wallDist)
        {
            b_dtype = dist_type.WALL;
        }
        else if ((data.backDist > wallDist) && (data.backDist <= shortDist))
        {
            b_dtype = dist_type.SHORT;
        }
        else if ((data.backDist > shortDist) && (data.backDist <= midDist))
        {
            b_dtype = dist_type.MID;
        }
        else
        {
            b_dtype = dist_type.LONG;
        }
        // left
        if (data.leftDist <= wallDist)
        {
            l_dtype = dist_type.WALL;
        }
        else if ((data.leftDist > wallDist) && (data.leftDist <= shortDist))
        {
            l_dtype = dist_type.SHORT;
        }
        else if ((data.leftDist > shortDist) && (data.leftDist <= midDist))
        {
            l_dtype = dist_type.MID;
        }
        else
        {
            l_dtype = dist_type.LONG;
        }
        // right
        if (data.rightDist <= wallDist)
        {
            r_dtype = dist_type.WALL;
        }
        else if ((data.rightDist > wallDist) && (data.rightDist <= shortDist))
        {
            r_dtype = dist_type.SHORT;
        }
        else if ((data.rightDist > shortDist) && (data.rightDist <= midDist))
        {
            r_dtype = dist_type.MID;
        }
        else
        {
            r_dtype = dist_type.LONG;
        }

        // mark exist position as "US"
        /*
		switch (data.exitpos) 
        {
		    case 1: // left
			    left_type= "US";
    			break;
	    	case 2: // right
	    		right_type = "US";
	    		break;
	    	case 3: // front
		    	front_type = "US";
		    	break;
	    	case 4: // back
	    		back_type = "US";
	    		break;
	    	default:
		    	break;
		}
		*/

        filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, data.frontDist, front_type, _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);

        if (filename.Equals("C00-21_F-6.75-DS_B-s-D_L-w-D_R-w-D.wav"))
        {
            filename = "C00-21_F-2.25-DS_B-m-D_L-w-D_R-w-D.wav";
        }
        else if (filename.Equals("C00-21_F-6.75-DS_B-m-D_L-w-D_R-w-D.wav"))
        {
            filename = "C00-21_F-5.25-DS_B-l-D_L-w-D_R-w-D.wav";
        }

        AudioClip echo = Resources.Load("echoes/" + filename) as AudioClip;
        string front_typeC = front_type, back_typeC = back_type, left_typeC = left_type, right_typeC = right_type;
        if (echo == null)
        {
            // Logging.Log("replace US with Deadend", Logging.LogLevel.NORMAL);
            switch (data.exitpos)
            {
                case 1: // left
                    left_typeC = "D";
                    break;
                case 2: // right
                    right_typeC = "D";
                    break;
                case 3: // front
                    front_typeC = "D";
                    break;
                case 4: // back
                    back_typeC = "D";
                    break;
                default:
                    break;
            }
            filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, data.frontDist, front_typeC, _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), left_typeC, _dist_type_to_string(r_dtype), right_typeC);
            echo = Resources.Load("echoes/" + filename) as AudioClip;
        }
        lastEcho = filename;

        // special cases
        // try alternative front dist

        if (echo == null)
        {
            Logging.Log("Secondary search_alt_front_dist", Logging.LogLevel.ABNORMAL);
            string frontString = "";
            if (Mathf.Abs(data.frontDist - 3.75f) <= 0.0001f)
            {
                frontString = "2.25";

                filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, frontString, front_type, _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
                echo = Resources.Load("echoes/" + filename) as AudioClip;
                lastEcho = filename;
            }
            /*
			else if (f_dtype == dist_type.LONG)
            {
				for (int i = 0; i < frontDistL.Length; ++i) 
                {
					frontString = frontDistL [i];
					filename = String.Format ("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, frontString, front_type, _dist_type_to_string (b_dtype), "D", _dist_type_to_string (l_dtype), left_type, _dist_type_to_string (r_dtype), right_type);
					echo = Resources.Load ("echoes/" + filename) as AudioClip;
					lastEcho = filename;
					if (echo != null)
                    {
						break;
                    }
				}
			}
			*/
        }


        // try wall
        if (echo == null)
        {
            Logging.Log("Secondary search_wall", Logging.LogLevel.ABNORMAL);
            string frontString = "";
            if (f_dtype == dist_type.WALL)
            {
                frontString = "0.75";
            }

            filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, frontString, front_type, _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
            echo = Resources.Load("echoes/" + filename) as AudioClip;
            lastEcho = filename;
        }

        // other cases
        if (echo == null)
        {
            bool found = false;
            Logging.Log("Secondary search_other", Logging.LogLevel.ABNORMAL);
            string frontString = "";
            if (f_dtype == dist_type.SHORT)
            {
                for (int i = 0; i < frontDistS.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, frontDistS[i], front_type, _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                    {
                        found = true;
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
            else if (f_dtype == dist_type.MID)
            {
                for (int i = 0; i < frontDistM.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, frontDistM[i], front_type, _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                    {
                        found = true;
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
            else if (f_dtype == dist_type.LONG)
            {
                for (int i = 0; i < frontDistL.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, frontDistL[i], front_type, _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                    {
                        found = true;
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
            lastEcho = filename;
        }

        if (echo == null)
        {
            bool found = false;
            // Logging.Log("replacing everything with D", Logging.LogLevel.NORMAL);
            string frontString = "";
            if (f_dtype == dist_type.SHORT)
            {
                for (int i = 0; i < frontDistS.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, frontDistS[i], "D", _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), "D", _dist_type_to_string(r_dtype), "D");
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                    {
                        found = true;
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
            else if (f_dtype == dist_type.MID)
            {
                for (int i = 0; i < frontDistM.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, frontDistM[i], "D", _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), "D", _dist_type_to_string(r_dtype), "D");
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                    {
                        found = true;
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
            else if (f_dtype == dist_type.LONG)
            {
                for (int i = 0; i < frontDistL.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, frontDistL[i], "D", _dist_type_to_string(b_dtype), "D", _dist_type_to_string(l_dtype), "D", _dist_type_to_string(r_dtype), "D");
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                    {
                        found = true;
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
            lastEcho = filename;
        }

        // have to use the old files
        if (echo == null)
        {
            Logging.Log("did not find accurate one, searching everything", Logging.LogLevel.WARNING);
            // Old version
            // this is the full filename, if back is not D or Stairs, it will be "na"
            prefix = "C00-21";
            back_type = "D"; front_type = ""; left_type = ""; right_type = "";
            if ((data.bType != BoardManager.JunctionType.DEADEND) && (data.exitpos != 4))
            {
                back_type = "na";
            }
            if (data.exitpos != 1)
            {
                left_type = "D";
            }
            if (data.exitpos != 2)
            {
                right_type = "D";
            }
            if (data.exitpos != 3)
            {
                front_type = data.jun_to_string(data.fType);
            }
            /*
			switch (data.exitpos) 
            {
			    case 1: // left
				    left_type = "US";
				    break;
			    case 2: // right
			    	right_type = "US";
			    	break;
			    case 3: // front
			    	front_type = "US";
				    break;
			    case 4: // back
			    	back_type = "US";
			    	break;
			    default:
				    break;
			}
			*/
            // TODO this is only a dummy one, use the one above when ready
            switch (data.exitpos)
            {
                case 1: // left
                    left_type = "D";
                    break;
                case 2: // right
                    right_type = "D";
                    break;
                case 3: // front
                    front_type = "D";
                    break;
                case 4: // back
                    back_type = "D";
                    break;
                default:
                    break;
            }

            // search for the most accurate one first
            filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, data.frontDist, front_type, data.backDist, "D", data.leftDist, left_type, data.rightDist, right_type);
            echo = Resources.Load("echoes/" + filename) as AudioClip;
            lastEcho = filename;

            DistRange fr, br, lr, rr;
            int fr_start, fr_end, br_start, br_end, lr_start, lr_end, rr_start, rr_end;
            if (echo == null)
            {
                if (data.front <= 3)
                {
                    fr = DistRange.SHORT;
                    fr_start = 0;
                    fr_end = 3;
                }
                else if ((data.front > 3) && (data.front <= 5))
                {
                    fr = DistRange.MID;
                    fr_start = 4;
                    fr_end = 5;
                }
                else
                {
                    fr = DistRange.LONG;
                    fr_start = 6;
                    fr_end = 10;
                }

                if (data.back <= 3)
                {
                    br = DistRange.SHORT;
                    br_start = 0;
                    br_end = 3;
                }
                else if ((data.back > 3) && (data.back <= 5))
                {
                    br = DistRange.MID;
                    br_start = 4;
                    br_end = 5;
                }
                else
                {
                    br = DistRange.LONG;
                    br_start = 6;
                    br_end = 10;
                }

                if (data.left <= 3)
                {
                    lr = DistRange.SHORT;
                    lr_start = 0;
                    lr_end = 3;
                }
                else if ((data.left > 3) && (data.left <= 5))
                {
                    lr = DistRange.MID;
                    lr_start = 4;
                    lr_end = 5;
                }
                else
                {
                    lr = DistRange.LONG;
                    lr_start = 6;
                    lr_end = 10;
                }

                if (data.right <= 3)
                {
                    rr = DistRange.SHORT;
                    rr_start = 0;
                    rr_end = 3;
                }
                else if ((data.right > 3) && (data.right <= 5))
                {
                    rr = DistRange.MID;
                    rr_start = 4;
                    rr_end = 5;
                }
                else
                {
                    rr = DistRange.LONG;
                    rr_start = 6;
                    rr_end = 10;
                }

                bool found = false;
                string[] back_str = new string[3] { "D", "na", "US" };
                string[] front_str = new string[2] { "", "D" };
                for (int i = fr_start; i <= fr_end; ++i)
                {
                    for (int j = br_start; j <= br_end; ++j)
                    {
                        for (int k = lr_start; k <= lr_end; ++k)
                        {
                            for (int l = rr_start; l <= rr_end; ++l)
                            {
                                front_str[0] = front_type;
                                for (int bsi = 0; bsi < back_str.Length; ++bsi)
                                {
                                    for (int fsi = 0; fsi < front_str.Length; ++fsi)
                                    {
                                        // DistS = {"2.25", "3.75"};
                                        // DistM = {"5.25", "6.75"};
                                        // DistL = {"8.25", "9.75", "11.25", "12.75"};
                                        string tb = "", tl = "", tr = "";
                                        // back
                                        if (((0.75f + 1.5f * j) >= 2) && ((0.75f + 1.5f * j) <= 4))
                                        {
                                            tb = "s";
                                        }
                                        else if (((0.75f + 1.5f * j) >= 5) && ((0.75f + 1.5f * j) <= 7))
                                        {
                                            tb = "m";
                                        }
                                        else if ((0.75f + 1.5f * j) >= 8)
                                        {
                                            tb = "l";
                                        }
                                        else if ((0.75f + 1.5f * j) <= 1)
                                        {
                                            tb = "w";
                                        }

                                        // left
                                        if (((0.75f + 1.5f * k) >= 2) && ((0.75f + 1.5f * k) <= 4))
                                        {
                                            tl = "s";
                                        }
                                        else if (((0.75f + 1.5f * k) >= 5) && ((0.75f + 1.5f * k) <= 7))
                                        {
                                            tl = "m";
                                        }
                                        else if ((0.75f + 1.5f * k) >= 8)
                                        {
                                            tl = "l";
                                        }
                                        else if ((0.75f + 1.5f * k) <= 1)
                                        {
                                            tl = "w";
                                        }

                                        // right
                                        if (((0.75f + 1.5f * l) >= 2) && ((0.75f + 1.5f * l) <= 4))
                                        {
                                            tr = "s";
                                        }
                                        else if (((0.75f + 1.5f * l) >= 5) && ((0.75f + 1.5f * l) <= 7))
                                        {
                                            tr = "m";
                                        }
                                        else if ((0.75f + 1.5f * l) >= 8)
                                        {
                                            tr = "l";
                                        }
                                        else if ((0.75f + 1.5f * l) <= 1)
                                        {
                                            tr = "w";
                                        }

                                        filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, (0.75f + 1.5f * i), front_str[fsi], tb, back_str[bsi], tl, left_type, tr, right_type);

                                        echo = Resources.Load("echoes/" + filename) as AudioClip;
                                        if (echo != null)
                                        {
                                            lastEcho = filename + "ERROR";
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (found)
                                    {
                                        break;
                                    }
                                }
                                if (found)
                                {
                                    break;
                                }
                            }
                            if (found)
                            {
                                break;
                            }
                        }
                        if (found)
                        {
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
        }

        if (echo == null)
        {
            Logging.Log("Echo not found", Logging.LogLevel.CRITICAL);
            Logging.Log(lastEcho, Logging.LogLevel.CRITICAL);
        }
        else
        {
            // SoundManager.instance.PlayEcho(echo);
            Logging.Log(lastEcho, Logging.LogLevel.LOW_PRIORITY);
            // SoundManager.instance.PlayClips(new List<AudioClip> { Database.instance.TitletoMainClips[0], echo }, 0, () => PlayedEcho(), 2);
            SoundManager.instance.PlayEcho(echo);
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
        // correct_post_act = GameManager.instance.boardScript.getHint (idx_location,"s");
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
            clip = Database.instance.hintClips[3];
            SoundManager.instance.PlayVoice(clip, true);
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
            clip = Database.instance.hintClips[0];
        }
        else if (forward == -sol_dir)
        {
            clip = Resources.Load("instructions/You should turn around by turning in the same direction twice") as AudioClip;
        }
        else
        {
            Vector3 angle = Vector3.Cross(forward, sol_dir);
            if (angle.z > 0)
            {
                clip = Database.instance.hintClips[1];
            }
            else
            {
                clip = Database.instance.hintClips[2];
            }
        }

        SoundManager.instance.PlayVoice(clip, true);
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
            transform.Rotate(0, 0, 90);
            GameManager.instance.boardScript.gamerecord += "l";
        }
        else if (dir == get_player_dir("RIGHT"))
        {
            transform.Rotate(0, 0, -90);
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
        // print (dir);

        if ((dir != get_player_dir("FRONT")) && (dir != get_player_dir("BACK")))
        {
            if (!GameManager.instance.boardScript.turning_lock)
            {
                changedDir = true;
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
            float ratio = (float)remaining_steps / total_step;
            if ((!reachQuarter) && (ratio <= 0.75f) && (ratio > 0.5f))
            {
                reachQuarter = true;
                // if (GameManager.instance.boardScript.latest_clip != null)
                // {
                //	  GameManager.instance.boardScript.restore_audio = true;
                //	  GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
                // }
                // SoundManager.instance.PlayVoice (Resources.Load ("instructions/You are 25% of the way through this level") as AudioClip);
            }
            else if ((!reachHalf) && (ratio <= 0.5f) && (ratio > 0.25f))
            {
                reachHalf = true;
                if (GameManager.instance.boardScript.latest_clip != null)
                {
                    GameManager.instance.boardScript.restore_audio = true;
                    GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
                }
                SoundManager.instance.PlayVoice(Database.instance.mainGameClips[34]);
                // Logging.Log("50%", Logging.LogLevel.NORMAL);
            }
            else if ((!reach3Quarter) && (ratio <= 0.25f))
            {
                // reach3Quarter = true;
                // if (GameManager.instance.boardScript.latest_clip != null) 
                // {
                //     GameManager.instance.boardScript.restore_audio = true;
                //     GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
                // }
                // SoundManager.instance.PlayVoice (Resources.Load ("instructions/You are 75% of the way through this level") as AudioClip);
                // print ("75%");
            }
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
        // If player could not move to that location, play the crash sound
        if (!canMove)
        {
            GameManager.instance.boardScript.gamerecord += "C";
            // if(!SoundManager.instance.isBusy())
            SoundManager.instance.playcrash(Database.instance.mainGameClips[5]);
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
	private void attemptExitFromLevel()
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

        GameObject exitSign = GameObject.FindGameObjectWithTag("Exit");
        Vector2 distFromExit = transform.position - exitSign.transform.position;
        if (Vector2.SqrMagnitude(distFromExit) < 0.25)
        {
            // Calculate time elapsed during the game level
            endLevel();
        }
        else
        {
            SoundManager.instance.PlaySingle(Database.instance.mainGameClips[36]); // Tell the player that they are not currently at the exit.
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

        if ((numSteps - GameManager.instance.boardScript.mazeSolution.Length + 1) > 0)
        {
            score -= 100 * (numSteps - GameManager.instance.boardScript.mazeSolution.Length + 1);
        }
        // Check if the score went below 0
        if (score < 1000)
        {
            score = 1000;
        }
        // Logging.Log(numSteps, Logging.LogLevel.NORMAL);
        // Logging.Log(GameManager.instance.boardScript.mazeSolution.Length - 1, Logging.LogLevel.NORMAL);
        // Logging.Log(numCrashes, Logging.LogLevel.NORMAL);

        // TODO(agotsis) understand this. Reimplement.
        // Send the crash count data and level information to server
        string levelDataEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptLevelData.py";
        int temp = GameManager.instance.boardScript.local_stats[curLevel];

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

        WWW www = new WWW(levelDataEndpoint, levelCompleteForm);
        StartCoroutine(Utilities.WaitForRequest(www));

        // display score
#if UNITY_ANDROID
        /*
		ad.clearflag();
		ad.DisplayAndroidWindow (
			"Your Score is:" + score.ToString() + ".\n" + 
			"Crashed " + numCrashes.ToString() + " times.\n" +
			"Used " + numSteps.ToString() + " Steps.\n"+ 
			"Optimal number of steps is: " + (GameManager.instance.boardScript.mazeSolution.Length - 1).ToString() + "\n", 
			AndroidDialogue.DialogueType.YESONLY);
		*/
#endif

        // Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
        restarted = true;
        Invoke("Restart", restartLevelDelay);
        // Disable the player object since level is over.
        // enabled = true;

        GameManager.instance.level += 1;
        GameManager.instance.boardScript.write_save(GameManager.instance.level);
        GameManager.instance.playersTurn = false;
        SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[8]);
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
        if (at_pause_menu)
        {
            if (GM_title.isUsingTalkback == true)
            {
                clips = new List<AudioClip>() { Database.instance.pauseMenuClips[2], Database.instance.pauseMenuClips[4], Database.instance.pauseMenuClips[8] };
                SoundManager.instance.PlayClips(clips);
            }
            else if (GM_title.isUsingTalkback == false)
            {
                clips = new List<AudioClip>() { Database.instance.pauseMenuClips[1], Database.instance.pauseMenuClips[3], Database.instance.pauseMenuClips[7] };
                SoundManager.instance.PlayClips(clips);
            }            
        }
    }

    //control
    bool echoLock = false;
    List<Touch> touches;
    CDTimer TriggerechoTimer;
    CDTimer TriggermenuTimer;
    CDTimer TriggerrotateTimer;

    void Update()
    {
        play_audio();
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
            // Make sure all the clips have finished playing before allowing the player to swipe.
            if ((finishedSwipingInstruction == false) && (level1_remaining_taps == 0) && (SoundManager.instance.finishedAllClips == true))
            {
                debugPlayerInfo = "Finished swiping instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedSwipingInstruction = true; // We have finished the swiping instruction, so the player can swipe.
            }
            // Make sure all the clips have finished playing before allowing the player to open the pause menu.
            if ((finishedMenuInstruction == false) && (level1_remaining_ups == 0) && (SoundManager.instance.finishedAllClips == true))
            {
                debugPlayerInfo = "Finished menu instruction.";
                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                finishedMenuInstruction = true; // We have finished the pause menu instruction, so the player can hold.
            }
            // Make sure all the clips have finished playing before allowing the player to rotate.
            if ((finishedTurningInstruction == false) && (level3_remaining_turns == 4) && (SoundManager.instance.finishedAllClips == true))
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
        // Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction\
        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

			// Do something based on this event info.
            switch (ie.keycode)
            {
            	// If the right arrow key has been pressed.
                case KeyCode.RightArrow:
                	// If the player is not in the pause menu, rotate them 90 degrees to the right.
                    if (!want_exit)
                    {
                        debugPlayerInfo = "Rotated right. Turning player right 90 degrees.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        dir = -transform.up; // Rotate the player right 90 degrees.
                        SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[5]);
                    }
                    // If the player is in the pause menu, they have told us they want to go back to the main menu.
                    else
                    {
                        debugPlayerInfo = "Swiped right. We want to return to the main menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.                        
                        if (GM_title.isUsingTalkback == true)
                        {
                            SoundManager.instance.PlaySingle(Database.instance.pauseMenuClips[10]);
                        }
                        else if (GM_title.isUsingTalkback == false)
                        {
                            SoundManager.instance.PlaySingle(Database.instance.pauseMenuClips[9]);
                        }
                        wantMainMenu = true;                        
                    }
                    break;
                // If the left arrow key has been pressed.
                case KeyCode.LeftArrow:
                	// If the player is not in the pause menu, rotate them 90 degrees to the left.
                    if (!want_exit)
                    {
                        debugPlayerInfo = "Rotated left. Turning player left 90 degrees.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        dir = get_player_dir("LEFT"); // Rotate the player left 90 degrees.
                        SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[4]);
                    }
                    // If the player is in the pause menu, they have told us they want to restart the level.
                    else
                    {
                        debugPlayerInfo = "Swiped left. We want to restart the level.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        if (GM_title.isUsingTalkback == true)
                        {
                            SoundManager.instance.PlaySingle(Database.instance.pauseMenuClips[6]);
                        }
                        else if (GM_title.isUsingTalkback == false)
                        {
                            SoundManager.instance.PlaySingle(Database.instance.pauseMenuClips[5]);
                        }                        
                        wantLevelRestart = true;                       
                    }
                    break;
                // If the up arrow key has been pressed.
                case KeyCode.UpArrow:
                	// If the player is not in the pause menu, move them forward.
                    if (!want_exit)
                    {
                        debugPlayerInfo = "Swiped up. Moved player forward.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        dir = transform.right; // Move the player forward.
                        SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[3]);
                    } 
                    // If the player is in the pause menu, give them a hint.
                    else
                    {
                        debugPlayerInfo = "Swiped up. Gave player hint";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        getHint(); // Give the player a hint.
                    }
                    break;
                // If the down arrow key has been pressed.
                case KeyCode.DownArrow:
                	// If the player is not in the pause menu, attempt to exit the level.
                	if (!want_exit) 
                	{
                        debugPlayerInfo = "Swiped down. Attempting to exit level.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                        attemptExitFromLevel(); // Attempt to exit the level.
                    }
                    else
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
                    break;
                // If the 'f' key has been pressed.
                case KeyCode.F:
                	// If the player is not in the pause menu, play an echo.
                	if (!want_exit)
                	{
                        debugPlayerInfo = "Tap registered. Played echo.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        GameManager.instance.boardScript.gamerecord += "E{"; // Record the echo.
                    	PlayEcho(); // Play the echo.
                    	GameManager.instance.boardScript.gamerecord += lastEcho;
                    	GameManager.instance.boardScript.gamerecord += "}";
                	}
                    else
                    {
                        // If the player has told us they want to restart the level, then restart the level.
                        if (wantLevelRestart == true)
                        {
                            debugPlayerInfo = "Tap registered. Restarting current level.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                            SoundManager.instance.PlayVoice(Database.instance.pauseMenuClips[11], true); // should have another set of sound effect
                            Destroy(GameObject.Find("GameManager"));
                            SceneManager.LoadScene("Main"); // Restart the level.
                        }
                        // If the player has told us they want to return to the main menu, then return to the main menu.
                        else if (wantMainMenu == true)
                        {
                            debugPlayerInfo = "Tap registered. Moving to main menu.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            // SceneManager.UnloadScene("Main");
                            SoundManager.instance.PlayVoice(Database.instance.pauseMenuClips[11], true); // should have another set of sound effect
                            Destroy(GameObject.Find("GameManager"));
                            SceneManager.LoadScene("Title_Screen"); // Move to the main menu.                            
                        }
                    }
                    break;          
                // If the 'r' key has been pressed.
                case KeyCode.R:
                	// If the player is not in the pause menu, open the pause menu.
                	if (want_exit == false) 
                	{
                        debugPlayerInfo = "Hold registered. Opened pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        if (SoundManager.instance.voiceSource.isPlaying)
						{
							GameManager.instance.boardScript.restore_audio = true;
							GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
						}
						SoundManager.instance.PlayVoice(Database.instance.pauseMenuClips[0], true);
						want_exit = true; // Open the pause menu.
    	                reset_audio = false;
                	}
                	// If the player is in the pause menu, close the pause menu.
                	else if (want_exit == true)
                	{
                        debugPlayerInfo = "Hold registered. Closed pause menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.PlayVoice(Database.instance.pauseMenuClips[11], true);
						want_exit = false; // Close the pause menu.
            	        reset_audio = true;
                	}
                    break;
                // If the 'p' key has been pressed.
				case KeyCode.P:
                    debugPlayerInfo = "P key pressed. Moving to main menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    SoundManager.instance.playcrash(Database.instance.soundEffectClips[6]);
					//SceneManager.UnloadScene("Main");
					Destroy(GameObject.Find("GameManager"));
					SceneManager.LoadScene("Title_Screen"); // Move to the main menu.
					break;
                default:
                    break;
            }
		}
#endif
// Check if we are running on iOS or Android
#if UNITY_IOS || UNITY_ANDROID
		//pop up the survey at the end of tutorial
		Vector2 distFromExit = transform.position - GameManager.instance.boardScript.exitPos;
		if ((Vector2.SqrMagnitude(distFromExit) < 0.25f) && survey_activated)
        {
			if ((GameManager.instance.level == 11) && !survey_shown)
            {
				ad.clearflag();
				ad.DisplayAndroidWindow("Would you like to take \n a short survey about the game?");
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
				ad.DisplayAndroidWindow(codemsg, AndroidDialogue.DialogueType.YESONLY);
			}
            else if (!URL_shown && ad.yesclicked() && code_entered)
            {
				URL_shown = true;
				Application.OpenURL("https://echolock.andrew.cmu.edu/survey/"); // "http://echolock.andrew.cmu.edu/survey/?"
			}
            else if (URL_shown)
            {
				ad.clearflag();
				ad.DisplayAndroidWindow("Thank you for taking the survey!", AndroidDialogue.DialogueType.YESONLY);
				reportsurvey(surveyCode);
				survey_activated = false;
			}
		}

		//process input
		if (eh.isActivate())
		{
			InputEvent ie = eh.getEventData(); // Get input event data from InputModule.cs.

			// If a tap is registered.
			if (ie.isTap == true)
			{
				// If the player is not in the pause menu, play an echo if possible.
				if (at_pause_menu == false)
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
			// If a hold is registered.
			else if (ie.isHold == true)
			{
				flipEchoLock(true);
				// If the player is not in the pause menu, open the pause menu.
				if (at_pause_menu == false)
				{
                    debugPlayerInfo = "Hold registered. Opened pause menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    at_pause_menu = true; // The player is now in the pause menu.
					if (SoundManager.instance.voiceSource.isPlaying)
					{
						GameManager.instance.boardScript.restore_audio = true;
						GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
					}
					SoundManager.instance.PlayVoice(Database.instance.pauseMenuClips[0], true);
				}
				// If the player is in the pause menu, close the pause menu.
				else if (at_pause_menu == true)
				{
                    debugPlayerInfo = "Hold registered. Closed pause menu.";
                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                    at_pause_menu = false; // The player is no longer in the pause menu.
					SoundManager.instance.PlayVoice(Database.instance.pauseMenuClips[11], true);
				}
			}
			// If a swipe is registered.
			else if (ie.isSwipe == true)
			{
				flipEchoLock(true);
				// If the player is not in the pause menu.
				if (at_pause_menu == false)
				{
					// If the swipe was up, move the player forward.
					if (ie.isUp == true)
					{
                        debugPlayerInfo = "Swiped up. Moved player forward.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        dir = get_player_dir("FRONT"); // Move the player forward.
						SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[3]);
					}
                    // If the swipe was down, attempt to exit the level.
                    if (ie.isDown == true)
                    {
                        debugPlayerInfo = "Swiped down. Attempting to exit level.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        GameManager.instance.boardScript.gamerecord += "X"; // Record the attempt.
                        attemptExitFromLevel(); // Attempt to exit the level.
                    }
                } 
				// If the player is in the pause menu.
				else if (at_pause_menu == true)
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
					// If the swipe was left, restart the level.
					else if (ie.isLeft == true)
					{
                        debugPlayerInfo = "Swiped left. Restarting level.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.playcrash(Database.instance.soundEffectClips[6]);
						//GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
						SoundManager.instance.PlayVoice(Database.instance.pauseMenuClips[11], true);//shoule have another set of sound effect
						Destroy(GameObject.Find("GameManager"));
						//Destroy(this);
						SceneManager.LoadScene("Main"); // Restart the level.
					}
					// If the swipe was right, return to the main menu.
					else if (ie.isRight == true)
					{
                        debugPlayerInfo = "Swiped right. Moving to main menu.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        SoundManager.instance.playcrash(Database.instance.soundEffectClips[6]);
						Destroy(GameObject.Find("GameManager"));
						SceneManager.LoadScene("Title_Screen"); // Move to the main menu.
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
			// If a rotation was registered.
			else if (ie.isRotate == true)
			{
				flipEchoLock(true);
				// If the player is not in the pause menu.
				if (at_pause_menu == false)
				{
					// If the rotation was left, rotate the player left 90 degrees.
					if (ie.isLeft == true)
					{
                        debugPlayerInfo = "Rotated left. Turning player left 90 degrees.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        dir = get_player_dir("LEFT"); // Rotate the player left 90 degrees.
						if (!GameManager.instance.boardScript.turning_lock)
						{
							SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[4]);
						}
					}
					// If the rotation was right, rotate the player right 90 degrees.
					else if (ie.isRight == true)
					{
                        debugPlayerInfo = "Rotated right. Turning player right 90 degrees.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        dir = get_player_dir("RIGHT"); // Rotate the player right 90 degrees.
						if (!GameManager.instance.boardScript.turning_lock) 
						{
							SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[5]);
						}
					}
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
        SoundManager.instance.playcrash(Database.instance.soundEffectClips[7]);
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

    List<AudioClip> clips;
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
                if (GM_title.isUsingTalkback == true)
                {
                    clips = new List<AudioClip>() { Database.instance.tutorialClips[0], Database.instance.tutorialClips[2], Database.instance.soundEffectClips[0], Database.instance.tutorialClips[3], Database.instance.tutorialClips[4] };
                    SoundManager.instance.PlayClips(clips, 0, () => PlayEcho(), 2);
                }
                else if (GM_title.isUsingTalkback == false)
                {
                    clips = new List<AudioClip>() { Database.instance.tutorialClips[0], Database.instance.tutorialClips[1], Database.instance.soundEffectClips[0], Database.instance.tutorialClips[3], Database.instance.tutorialClips[4] };
                    SoundManager.instance.PlayClips(clips, 0, () => PlayEcho(), 2);
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
                finishedTurningInstruction = false; // Reset if the player is going through this tutorial level again.
                if (GM_title.isUsingTalkback == true)
                {
                    clips = new List<AudioClip>() { Database.instance.tutorialClips[22], Database.instance.tutorialClips[23], Database.instance.tutorialClips[24] };
                    SoundManager.instance.PlayClips(clips);
                }
                else if (GM_title.isUsingTalkback == false)
                {
                    clips = new List<AudioClip>() { Database.instance.tutorialClips[21], Database.instance.tutorialClips[23], Database.instance.tutorialClips[24] };
                    SoundManager.instance.PlayClips(clips);
                }             
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
		// Do something based on this event info.
        switch (ie.keycode)
        {
        	// If the right arrow key has been pressed, the action was a rotation right.
            case KeyCode.RightArrow:
                action = InterceptAction.RIGHT;
                break;
			// If the left arrow key has been pressed, the action was a rotation left.
            case KeyCode.LeftArrow:
                action = InterceptAction.LEFT;
                break;
			// If the up arrow key has been pressed, the action was a swipe up.
            case KeyCode.UpArrow:
                action = InterceptAction.UP;
                break;
			// If the down arrow key has been pressed, the action was a swipe down.
            case KeyCode.DownArrow:
                action = InterceptAction.DOWN;
                break;
			// If the 'f' key has been pressed, the action was a tap.
            case KeyCode.F:
                action = InterceptAction.TAP;
                break;
			// If the 'r' key has been pressed, the action was a hold.
            case KeyCode.R:
                action = InterceptAction.MENU;
                break;
			// If the 'p' key has been pressed, no action is taken.
            case KeyCode.P:
            	return true;
            default:
                break;
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
		} 
		// If a rotation was registered. 
		else if (ie.isRotate == true)
		{
			// If the rotation was left, set this as the action.
			if (ie.isLeft == true)
			{
				action = InterceptAction.LEFT;
			}
			// If the rotation was right, set this as the action.
			else if (ie.isRight == true)
			{
				action = InterceptAction.RIGHT;
			}
		}
#endif
        
        // Based on the action, play the appropriate sound.
        switch (action)
        {
            case InterceptAction.UP:
                if ((finishedSwipingInstruction == true) && (level1_remaining_ups > 0))
                {
                    SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[3]);
                }
                break;
            case InterceptAction.LEFT:
                if ((finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                {
                    SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[4]);
                }
                break;
            case InterceptAction.RIGHT:
                if ((finishedTurningInstruction == true) && (level3_remaining_turns > 0))
                {
                    SoundManager.instance.PlaySingle(Database.instance.soundEffectClips[5]);
                }
                break;
            case InterceptAction.TAP:
                if ((finishedTappingInstruction == true) && (level1_remaining_taps > 0))
                {
                    PlayEcho();
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
                            clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[5] };
                            SoundManager.instance.PlayClips(clips); // This tap was correct. Please tap X more times.
                        }
                        else if (level1_remaining_taps == 1)
                        {
                            clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[6] };
                            SoundManager.instance.PlayClips(clips); // This tap was correct. Please tap X more times.
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
                                clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[7], Database.instance.tutorialClips[9], Database.instance.soundEffectClips[0], Database.instance.soundEffectClips[3], Database.instance.tutorialClips[10] };
                                SoundManager.instance.PlayClips(clips);
                            }
                            else if (GM_title.isUsingTalkback == false)
                            {
                                clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[7], Database.instance.tutorialClips[8], Database.instance.soundEffectClips[0], Database.instance.soundEffectClips[3], Database.instance.tutorialClips[10] };
                                SoundManager.instance.PlayClips(clips);
                            }
                            // Make sure all the clips have finished playing before allowing the player to swipe.
                            if (SoundManager.instance.finishedAllClips == true)
                            {
                                finishedSwipingInstruction = true; // We have finished the swiping instruction, so the player can swipe.
                            }
                        }
                    }
                    // If the action was not a tap.
                    else if (((action == InterceptAction.MENU) || (action == InterceptAction.LEFT) || (action == InterceptAction.RIGHT) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (ie.isUnrecognized == true)) && (finishedTappingInstruction == true))
                    {
                        debugPlayerInfo = "Incorrect tap for gesture tutorial.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        // This tap was too long/short. Please tap again.
                        SoundManager.instance.PlayVoice(Database.instance.errorClips[0], true);
                    }
                    // If the tapping instruction has not finished yet.
                    else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFT) || (action == InterceptAction.RIGHT) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (ie.isUnrecognized == true)) && (finishedTappingInstruction == false))
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
                                clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[11] };
                                SoundManager.instance.PlayClips(clips); // This swipe was correct. Please swipe X more times.
                            }
                            else if (level1_remaining_ups == 1)
                            {
                                clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[12] };
                                SoundManager.instance.PlayClips(clips); // This swipe was correct. Please swipe X more times.
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
                                    clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[13], Database.instance.tutorialClips[15] };
                                    SoundManager.instance.PlayClips(clips);
                                }
                                else if (GM_title.isUsingTalkback == false)
                                {
                                    clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[13], Database.instance.tutorialClips[14] };
                                    SoundManager.instance.PlayClips(clips);
                                }
                                // Make sure all the clips have finished playing before allowing the player to open the pause menu.
                                if (SoundManager.instance.finishedAllClips == true)
                                {
                                    finishedMenuInstruction = true; // We have finished the pause menu instruction, so the player can open the pause menu.
                                }
                            }
                        }
                        // If the action was not a swipe up.
						else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFT) || (action == InterceptAction.RIGHT) || (action == InterceptAction.DOWN) || (ie.isUnrecognized == true)) && (finishedSwipingInstruction == true))
                        {
                            // This swipe's distance was too long/short. Please swipe again.
                            debugPlayerInfo = "Incorrect swipe up for gesture tutorial.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            SoundManager.instance.PlayVoice(Database.instance.errorClips[4], true);
                        }
                        // If the swiping instruction has not finished yet.
                        else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFT) || (action == InterceptAction.RIGHT) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (ie.isUnrecognized == true)) && (finishedSwipingInstruction == false))
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
                                    // You are now in the pause menu. To get a hint, swipe up. To restart the level, swipe left. To go to the main menu, swipe right. To close the pause menu, tap and hold with two fingers for 2 seconds. Please close the pause menu now.
                                    if (GM_title.isUsingTalkback == true)
                                    {                           
                                        SoundManager.instance.PlayVoice(Database.instance.tutorialClips[17], true);
                                    }
                                    else if (GM_title.isUsingTalkback == false)
                                    {                                       
                                        SoundManager.instance.PlayVoice(Database.instance.tutorialClips[16],true);
                                    }
                                }
                                // If the pause menu has not been closed.
                                else if (level1_remaining_menus == 1)
                                {
                                    debugPlayerInfo = "Hold registered. Closed pause menu. Finished menu section for gesture tutorial.";
                                    DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                    level1_remaining_menus--; // Decrease the number of holds left to do.
                                    // Congratulations! You have reached the exit. Once you believe you have reached the exit in a level, swiping down will move you to the next level and you will hear a congratulatory sound like this.
                                    // Finish SOUND
                                    // Now try to swipe down to move on to the next level.
                                    if (GM_title.isUsingTalkback == true)
                                    {
                                        clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[19], Database.instance.soundEffectClips[8], Database.instance.tutorialClips[20] };
                                        SoundManager.instance.PlayClips(clips);
                                    }
                                    else if (GM_title.isUsingTalkback == false)
                                    {
                                        clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[18], Database.instance.soundEffectClips[8], Database.instance.tutorialClips[20] };
                                        SoundManager.instance.PlayClips(clips);
                                    }
                                }
                            }
                            // If the action was not a hold.
							else if (((action == InterceptAction.TAP) || (action == InterceptAction.LEFT) || (action == InterceptAction.RIGHT) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (ie.isUnrecognized == true)) && (finishedMenuInstruction == true))
							{
                                debugPlayerInfo = "Incorrect hold for menu in gesture tutorial.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                SoundManager.instance.PlayVoice(Database.instance.errorClips[16], true);
                            }
                            // If the pause menu instruction has not finished yet.
                            else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFT) || (action == InterceptAction.RIGHT) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (ie.isUnrecognized == true)) && (finishedMenuInstruction == false))
                            {
                                debugPlayerInfo = "Please wait for the instructions to finish.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            }
                        }
                        // If the player has finished the hold section of the tutorial.
                        else if (level1_remaining_menus == 0)
                        {
							// If the action was a swipe down.
                            if (action == InterceptAction.DOWN)
                            {
                                debugPlayerInfo = "Swiped down. Starting tutorial level 1.";
                                DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                                // TODO: Replace the winSound with "Congratulations! You have completed the tutorial. Now we will move back to the game!"
                                clips = new List<AudioClip> { Database.instance.soundEffectClips[8] };
                                BoardManager.finishedTutorialLevel1 = true; // Make sure the player does not have to go through the tutorial again if they have gone through it once.
                                SoundManager.instance.PlayClips(clips, 0, () => quitInterception(), 1);
                            }
                            // If the action was not a swipe down.
							else if ((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFT) || (action == InterceptAction.RIGHT) || (action == InterceptAction.UP) || (ie.isUnrecognized == true))
							{
                                debugPlayerInfo = "Incorrect swipe down for gesture tutorial.";
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
                	// If the action was a left or right rotation.
                	if (((action == InterceptAction.LEFT) || (action == InterceptAction.RIGHT)) && (finishedTurningInstruction == true))
                	{
                        debugPlayerInfo = "Rotated for gesture tutorial. Turned player 90 degrees.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        level3_remaining_turns--; // Decrease the number of turns left to do.
                 		if (level3_remaining_turns == 3) 
                 		{
                            clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[25] };
                            SoundManager.instance.PlayClips(clips); // This rotation was correct. Please rotate X more times.
                 		}
                        else if (level3_remaining_turns == 2)
                        {
                            clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[26] };
                            SoundManager.instance.PlayClips(clips); // This rotation was correct. Please rotate X more times.
                        }
                        else if (level3_remaining_turns == 1)
                        {
                            clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[27] };
                            SoundManager.instance.PlayClips(clips); // This rotation was correct. Please rotate X more times.
                        }
                        // If the player has finished the rotation section of the tutorial.
                        else if (level3_remaining_turns == 0)
                        {
                            debugPlayerInfo = "Finished rotations for gesture tutorial. Completed gesture tutorial. Continuing with level 3.";
                            DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                            BoardManager.finishedTutorialLevel3 = true; // Make sure the player does not have to go through the tutorial again if they have gone through it once.
                            // Good job! now we will move back to the game. Try and get around the corner!
                            clips = new List<AudioClip> { Database.instance.soundEffectClips[0], Database.instance.tutorialClips[28] };
                            SoundManager.instance.PlayClips(clips, 0, () => quitInterception(), 2);
                        }
                    }
                    // If the action was not a right or left rotation.
					else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (ie.isUnrecognized == true)) && (finishedTurningInstruction == true))
                	{
                        debugPlayerInfo = "Incorrect rotation for gesture tutorial.";
                        DebugPlayer.instance.ChangeDebugPlayerText(debugPlayerInfo); // Update the debug textbox.
                        // This rotation was not correct/in the same direction. Please rotate again.
                        SoundManager.instance.PlayVoice(Database.instance.errorClips[14], true);
                	}
                    // If the turning instruction has not finished yet.
                    else if (((action == InterceptAction.TAP) || (action == InterceptAction.MENU) || (action == InterceptAction.LEFT) || (action == InterceptAction.RIGHT) || (action == InterceptAction.UP) || (action == InterceptAction.DOWN) || (ie.isUnrecognized == true)) && (finishedTurningInstruction == false))
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

    private void quitInterception()
    {
        intercepted = false;
    }
}
