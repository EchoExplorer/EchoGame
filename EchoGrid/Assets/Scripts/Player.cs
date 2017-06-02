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

//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
/// <summary>
/// A class representing the player object in the game.
///  It also is responsible for determining which echo sound to when the user
///  requests an echo, and for keeping track of and sending usage data
///  to a remote server when certain events occur in the game.
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

    //FIXME: these should not be public
    public static Player instance;
    //Delay time in seconds to restart level.
    public float restartLevelDelay = 3.0f;

    bool restarted = false;
    bool is_freezed;//is player not allowed to do anything?
    bool tapped;//did player tap to hear an echo at this position?
    bool reportSent;
    private int curLevel;

    int cur_clip = 0;
    int max_quit_clip = 2;
    bool reset_audio;

    //private SpriteRenderer spriteRenderer;

    // variables to implement data collection
    public int numCrashes;
    //Keep track of number of times user crashed into wall
    public int numSteps;
    //Keep track of number of steps taken per level
    private int exitAttempts;

    private String lastEcho = "";

    //Track locations of the player's crashes
    private string crashLocs;

    //Keep track of time taken for the game level
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
    bool at_pause_menu = false;//indicating if the player activated pause menu
    static bool level_already_loaded = false;
    bool localRecordWritten = false;
    //int score;
    eventHandler eh;

    string surveyCode = "";

    void Awake()
    {

        level_already_loaded = false;

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        enabled = true;
        surveyCode = "";
        DontDestroyOnLoad(gameObject);

    }

    private void init()
    {
        numCrashes = 0;
        numSteps = 0;
        crashLocs = "";
        Utilities.initEncrypt();
        //Initialize list of crash locations
        crashLocs = "";
        //Start the time for the game level
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

        touch_simple = 1;
        touch_audio = 2;
        touch_exit = 1;
        touch_menu = 2;
        tap_simple = 1;
        tap_exit = 2;
        tap_menu = 2;

        level_already_loaded = false;
        //score = 1000;
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
        //Adjust player scale
        Vector3 new_scale = transform.localScale;
        new_scale *= (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;
        transform.localScale = new_scale;
    }

    void OnLevelWasLoaded(int index)
    {
        //if (!level_already_loaded) {
        //	level_already_loaded = true;
        init();
        //}
    }

    private string _dist_type_to_string(dist_type type)
    {
        switch (type)
        {
            case dist_type.WALL:
                return "w";
                break;
            case dist_type.SHORT:
                return "s";
                break;
            case dist_type.MID:
                return "m";
                break;
            case dist_type.LONG:
                return "l";
                break;
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
        tapped = true;
        reportSent = true;
        BoardManager.echoDistData data =
            GameManager.instance.boardScript.getEchoDistData(transform.position, get_player_dir("FRONT"), get_player_dir("LEFT"));

        Logging.Log(data.all_jun_to_string(), Logging.LogLevel.NORMAL);
        String prefix = "C00-21"; //change this prefix when you change the echo files
        if (GameManager.instance.level < 26)
            prefix = "C00 - 21";
        else if ((GameManager.instance.level >= 26) && (GameManager.instance.level < 41))
            prefix = "19 dB/C00-19";
        else if ((GameManager.instance.level >= 41) && (GameManager.instance.level < 56))
            prefix = "17 dB/C00-17";
        else //if( (GameManager.instance.level >= 56)&&(GameManager.instance.level < 71) )
            prefix = "15 dB/C00-15";
        /*
		else if ( (GameManager.instance.level >= 71)&&(GameManager.instance.level < 86) )
			prefix = "13 dB/C00-13";
		else if ( (GameManager.instance.level >= 86)&&(GameManager.instance.level < 101) )
			prefix = "11 dB/C00-11";
		else if ( (GameManager.instance.level >= 101)&&(GameManager.instance.level < 116) )
			prefix = "9 dB/C00-9";
		else if ( (GameManager.instance.level >= 116) )
			prefix = "7 dB/C00-7";
			*/

        String filename;
        float wallDist = 0.8f, shortDist = 3.8f, midDist = 6.8f, longDist = 12.8f;
        dist_type f_dtype, b_dtype, l_dtype, r_dtype;
        string front_type = data.jun_to_string(data.fType), back_type = "D", left_type = "D", right_type = "D";

        //catogrize the distance
        //front
        if (data.frontDist <= wallDist)
            f_dtype = dist_type.WALL;
        else if ((data.frontDist > wallDist) && (data.frontDist <= shortDist))
            f_dtype = dist_type.SHORT;
        else if ((data.frontDist > shortDist) && (data.frontDist <= midDist))
            f_dtype = dist_type.MID;
        else
            f_dtype = dist_type.LONG;
        //back
        if (data.backDist <= wallDist)
            b_dtype = dist_type.WALL;
        else if ((data.backDist > wallDist) && (data.backDist <= shortDist))
            b_dtype = dist_type.SHORT;
        else if ((data.backDist > shortDist) && (data.backDist <= midDist))
            b_dtype = dist_type.MID;
        else
            b_dtype = dist_type.LONG;
        //left
        if (data.leftDist <= wallDist)
            l_dtype = dist_type.WALL;
        else if ((data.leftDist > wallDist) && (data.leftDist <= shortDist))
            l_dtype = dist_type.SHORT;
        else if ((data.leftDist > shortDist) && (data.leftDist <= midDist))
            l_dtype = dist_type.MID;
        else
            l_dtype = dist_type.LONG;
        //right
        if (data.rightDist <= wallDist)
            r_dtype = dist_type.WALL;
        else if ((data.rightDist > wallDist) && (data.rightDist <= shortDist))
            r_dtype = dist_type.SHORT;
        else if ((data.rightDist > shortDist) && (data.rightDist <= midDist))
            r_dtype = dist_type.MID;
        else
            r_dtype = dist_type.LONG;

        //mark exist position as "US"
        /*
		switch (data.exitpos) {
		case 1://left
			left_type= "US";
			break;
		case 2://right
			right_type = "US";
			break;
		case 3://front
			front_type = "US";
			break;
		case 4://back
			back_type = "US";
			break;
		default:
			break;
		}
		*/

        filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
            data.frontDist, front_type, _dist_type_to_string(b_dtype), "D",
            _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);

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
            Logging.Log("replace US with Deadend", Logging.LogLevel.NORMAL);
            switch (data.exitpos)
            {
                case 1://left
                    left_typeC = "D";
                    break;
                case 2://right
                    right_typeC = "D";
                    break;
                case 3://front
                    front_typeC = "D";
                    break;
                case 4://back
                    back_typeC = "D";
                    break;
                default:
                    break;
            }
            filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                data.frontDist, front_typeC, _dist_type_to_string(b_dtype), "D",
                _dist_type_to_string(l_dtype), left_typeC, _dist_type_to_string(r_dtype), right_typeC);
            echo = Resources.Load("echoes/" + filename) as AudioClip;
        }
        lastEcho = filename;

        //special cases
        //try alternative front dist

        if (echo == null)
        {
            Logging.Log("Secondary search_alt_front_dist", Logging.LogLevel.ABNORMAL);
            string frontString = "";
            if (Mathf.Abs(data.frontDist - 3.75f) <= 0.0001f)
            {
                frontString = "2.25";

                filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                    frontString, front_type, _dist_type_to_string(b_dtype), "D",
                    _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
                echo = Resources.Load("echoes/" + filename) as AudioClip;
                lastEcho = filename;
            }
            /*
			else if ( f_dtype == dist_type.LONG){
				for (int i = 0; i < frontDistL.Length; ++i) {
					frontString = frontDistL [i];
					filename = String.Format ("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
						frontString, front_type, _dist_type_to_string (b_dtype), "D",
						_dist_type_to_string (l_dtype), left_type, _dist_type_to_string (r_dtype), right_type);
					echo = Resources.Load ("echoes/" + filename) as AudioClip;
					lastEcho = filename;
					if (echo != null)
						break;
				}
			}
			*/
        }


        //try wall
        if (echo == null)
        {
            Logging.Log("Secondary search_wall", Logging.LogLevel.ABNORMAL);
            string frontString = "";
            if (f_dtype == dist_type.WALL)
                frontString = "0.75";

            filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                frontString, front_type, _dist_type_to_string(b_dtype), "D",
                _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
            echo = Resources.Load("echoes/" + filename) as AudioClip;
            lastEcho = filename;
        }

        //other cases
        if (echo == null)
        {
            bool found = false;
            Logging.Log("Secondary search_other", Logging.LogLevel.ABNORMAL);
            string frontString = "";
            if (f_dtype == dist_type.SHORT)
            {
                for (int i = 0; i < frontDistS.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                        frontDistS[i], front_type, _dist_type_to_string(b_dtype), "D",
                        _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                        found = true;
                    if (found)
                        break;
                }
            }
            else if (f_dtype == dist_type.MID)
            {
                for (int i = 0; i < frontDistM.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                        frontDistM[i], front_type, _dist_type_to_string(b_dtype), "D",
                        _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                        found = true;
                    if (found)
                        break;
                }
            }
            else if (f_dtype == dist_type.LONG)
            {
                for (int i = 0; i < frontDistL.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                        frontDistL[i], front_type, _dist_type_to_string(b_dtype), "D",
                        _dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                        found = true;
                    if (found)
                        break;
                }
            }
            lastEcho = filename;
        }

        if (echo == null)
        {
            bool found = false;
            Logging.Log("replacing everything with D", Logging.LogLevel.NORMAL);
            string frontString = "";
            if (f_dtype == dist_type.SHORT)
            {
                for (int i = 0; i < frontDistS.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                        frontDistS[i], "D", _dist_type_to_string(b_dtype), "D",
                        _dist_type_to_string(l_dtype), "D", _dist_type_to_string(r_dtype), "D");
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                        found = true;
                    if (found)
                        break;
                }
            }
            else if (f_dtype == dist_type.MID)
            {
                for (int i = 0; i < frontDistM.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                        frontDistM[i], "D", _dist_type_to_string(b_dtype), "D",
                        _dist_type_to_string(l_dtype), "D", _dist_type_to_string(r_dtype), "D");
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                        found = true;
                    if (found)
                        break;
                }
            }
            else if (f_dtype == dist_type.LONG)
            {
                for (int i = 0; i < frontDistL.Length; ++i)
                {
                    filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                        frontDistL[i], "D", _dist_type_to_string(b_dtype), "D",
                        _dist_type_to_string(l_dtype), "D", _dist_type_to_string(r_dtype), "D");
                    echo = Resources.Load("echoes/" + filename) as AudioClip;
                    if (echo != null)
                        found = true;
                    if (found)
                        break;
                }
            }
            lastEcho = filename;
        }

        //have to use the old files
        if (echo == null)
        {
            Logging.Log("did not find accurate one, searching everything", Logging.LogLevel.WARNING);
            //Old version
            //this is the full filename, if back is not D or Stairs, it will be "na"
            prefix = "C00-21";
            back_type = "D"; front_type = ""; left_type = ""; right_type = "";
            if ((data.bType != BoardManager.JunctionType.DEADEND) && (data.exitpos != 4))
                back_type = "na";
            if (data.exitpos != 1)
                left_type = "D";
            if (data.exitpos != 2)
                right_type = "D";
            if (data.exitpos != 3)
                front_type = data.jun_to_string(data.fType);
            /*
			switch (data.exitpos) {
			case 1://left
				left_type = "US";
				break;
			case 2://right
				right_type = "US";
				break;
			case 3://front
				front_type = "US";
				break;
			case 4://back
				back_type = "US";
				break;
			default:
				break;
			}
			*/
            //TODO this is only a dummy one, use the one above when ready
            switch (data.exitpos)
            {
                case 1://left
                    left_type = "D";
                    break;
                case 2://right
                    right_type = "D";
                    break;
                case 3://front
                    front_type = "D";
                    break;
                case 4://back
                    back_type = "D";
                    break;
                default:
                    break;
            }

            //search for the most accurate one first
            filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                data.frontDist, front_type, data.backDist, "D",
                data.leftDist, left_type, data.rightDist, right_type);
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
                                        //DistS = {"2.25", "3.75"};
                                        //DistM = {"5.25", "6.75"};
                                        //DistL = {"8.25", "9.75", "11.25", "12.75"};
                                        string tb = "", tl = "", tr = "";
                                        //back
                                        if ((0.75f + 1.5f * j) >= 2 && (0.75f + 1.5f * j) <= 4)
                                            tb = "s";
                                        else if ((0.75f + 1.5f * j) >= 5 && (0.75f + 1.5f * j) <= 7)
                                            tb = "m";
                                        else if ((0.75f + 1.5f * j) >= 8)
                                            tb = "l";
                                        else if ((0.75f + 1.5f * j) <= 1)
                                            tb = "w";

                                        //left
                                        if ((0.75f + 1.5f * k) >= 2 && (0.75f + 1.5f * k) <= 4)
                                            tl = "s";
                                        else if ((0.75f + 1.5f * k) >= 5 && (0.75f + 1.5f * k) <= 7)
                                            tl = "m";
                                        else if ((0.75f + 1.5f * k) >= 8)
                                            tl = "l";
                                        else if ((0.75f + 1.5f * k) <= 1)
                                            tl = "w";

                                        //right
                                        if ((0.75f + 1.5f * l) >= 2 && (0.75f + 1.5f * l) <= 4)
                                            tr = "s";
                                        else if ((0.75f + 1.5f * l) >= 5 && (0.75f + 1.5f * l) <= 7)
                                            tr = "m";
                                        else if ((0.75f + 1.5f * l) >= 8)
                                            tr = "l";
                                        else if ((0.75f + 1.5f * l) <= 1)
                                            tr = "w";

                                        filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
                                            0.75f + 1.5f * i, front_str[fsi], tb, back_str[bsi],
                                            tl, left_type, tr, right_type);

                                        echo = Resources.Load("echoes/" + filename) as AudioClip;
                                        if (echo != null)
                                        {
                                            lastEcho = filename + "ERROR";
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (found)
                                        break;
                                }
                                if (found)
                                    break;
                            }
                            if (found)
                                break;
                        }
                        if (found)
                            break;
                    }
                    if (found)
                        break;
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
            SoundManager.instance.PlayEcho(echo);
            Logging.Log(lastEcho, Logging.LogLevel.LOW_PRIORITY);
        }
    }

    string post_act = "";
    string correct_post_act = "";

    /// <summary>
    /// Reports data when an echo is requested.
    ///  The function is actually called during other actions after an echo was played.
    /// </summary>
	private void reportOnEcho()
    {

        string echoEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptEchoData.py";

        Vector2 idx_location = GameManager.instance.boardScript.get_idx_from_pos(transform.position);
        string location = "(" + idx_location.x.ToString() + "," + idx_location.y.ToString() + ")";
        correct_post_act = "";
        //manually setup, TODO: warp it into a function
        GameManager.instance.boardScript.sol = "";
        for (int i = 0; i < GameManager.instance.boardScript.searched_temp.Length; ++i)
            GameManager.instance.boardScript.searched_temp[i] = false;
        //correct_post_act = GameManager.instance.boardScript.getHint (idx_location,"s");
        GameManager.instance.boardScript.solveMazeMid(idx_location, "s");
        if (GameManager.instance.boardScript.sol.Length >= 2)
            correct_post_act = GameManager.instance.boardScript.sol[GameManager.instance.boardScript.sol.Length - 2].ToString();

        Vector3 forward = old_dir;
        Vector3 sol_dir = new Vector3();
        if (correct_post_act == "u")
            sol_dir = Vector3.up;
        else if (correct_post_act == "d")
            sol_dir = Vector3.down;
        else if (correct_post_act == "l")
            sol_dir = Vector3.left;
        else if (correct_post_act == "r")
            sol_dir = Vector3.right;

        if (correct_post_act != "")
        {
            if (forward == sol_dir)
                correct_post_act = "Forward";
            else if (forward == -sol_dir)
                correct_post_act = "Turn Around";
            else
            {
                Vector3 angle = Vector3.Cross(forward, sol_dir);
                if (angle.z > 0)
                    correct_post_act = "Turn Left";
                else
                    correct_post_act = "Turn Right";
            }
        }
        else
            correct_post_act = "Exit";


        WWWForm echoForm = new WWWForm();
        echoForm.AddField("userName", Utilities.encrypt(SystemInfo.deviceUniqueIdentifier));
        echoForm.AddField("currentLevel", Utilities.encrypt(curLevel.ToString()));
        echoForm.AddField("trackCount", Utilities.encrypt(GameManager.instance.boardScript.local_stats[curLevel].ToString()));
        echoForm.AddField("echo", lastEcho); //fix
        echoForm.AddField("echoLocation", Utilities.encrypt(location));
        echoForm.AddField("postEchoAction", Utilities.encrypt(post_act));
        echoForm.AddField("correctAction", Utilities.encrypt(correct_post_act));
        echoForm.AddField("dateTimeStamp", Utilities.encrypt(System.DateTime.Now.ToString()));

        Logging.Log(System.Text.Encoding.ASCII.GetString(echoForm.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www = new WWW(echoEndpoint, echoForm);
        StartCoroutine(Utilities.WaitForRequest(www));
    }

    void getHint()
    {

        Vector2 idx_location = GameManager.instance.boardScript.get_idx_from_pos(transform.position);
        correct_post_act = "";
        GameManager.instance.boardScript.sol = "";
        for (int i = 0; i < GameManager.instance.boardScript.searched_temp.Length; ++i)
            GameManager.instance.boardScript.searched_temp[i] = false;
        correct_post_act = GameManager.instance.boardScript.getHint(idx_location, "s");

        AudioClip clip;
        if (correct_post_act.Length <= 0)
        {
            clip = Resources.Load("instructions/You should exit") as AudioClip;
            SoundManager.instance.PlayVoice(clip, true);
            return;
        }
        Vector3 forward = old_dir;
        Vector3 sol_dir = new Vector3();
        if (correct_post_act == "u")
            sol_dir = Vector3.up;
        else if (correct_post_act == "d")
            sol_dir = Vector3.down;
        else if (correct_post_act == "l")
            sol_dir = Vector3.left;
        else if (correct_post_act == "r")
            sol_dir = Vector3.right;

        if (forward == sol_dir)
            clip = Resources.Load("instructions/You should move forward") as AudioClip;
        else if (forward == -sol_dir)
            clip = Resources.Load("instructions/You should turn around by turning in the same direction twice") as AudioClip;
        else
        {
            Vector3 angle = Vector3.Cross(forward, sol_dir);
            if (angle.z > 0)
                clip = Resources.Load("instructions/You should turn left") as AudioClip;
            else
                clip = Resources.Load("instructions/You should turn right") as AudioClip;
        }

        SoundManager.instance.PlayVoice(clip, true);
    }

    //due to the chaotic coord system
    //return the relative direction
    public Vector3 get_player_dir(string dir)
    {
        if (dir == "FRONT")
            return transform.right.normalized;
        else if (dir == "BACK")
            return -transform.right.normalized;
        else if (dir == "LEFT")
            return transform.up.normalized;
        else if (dir == "RIGHT")
            return -transform.up.normalized;

        Logging.Log("INVALID direction string", Logging.LogLevel.CRITICAL);
        return Vector3.zero;
    }
    //get the direction in world space
    /*
	Vector3 get_world_dir(string dir){
		if (dir == "FRONT")
			return transform.right.normalized;
		else if (dir == "BACK")
			return -transform.right.normalized;
		else if (dir == "LEFT")
			return transform.up.normalized;
		else if (dir == "RIGHT")
			return -transform.up.normalized;

		UnityEngine.Debug.Log ("INVALID direction string");
		return Vector3.zero;
	}
	*/

    //please call this function to rotate player
    //use this with get_player_dir("SOMETHING")
    Vector3 old_dir = new Vector3();
    /// <summary>
    /// Also a function to rotate the player. The specified direction is
    ///  an absolute direction to make the player face toward, and is only
    ///  valid if it corresponds to turning left or right.
    /// </summary>
	void rotateplayer(Vector3 dir)
    {
        if (dir == get_player_dir("FRONT"))
            return;
        else if (dir == get_player_dir("BACK"))
            return;
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
            transform.Rotate(new Vector3(0, 0, 90));
        else if (dir == BoardManager.Direction.BACK)
            transform.Rotate(new Vector3(0, 0, -90));
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
    ///  If it corresponds to moving forward or backward, the player moves.
    ///  Otherwise, the player will make a turn. This sends data online if the previous action
    ///  was to request an echo.
    /// </summary>
	private void calculateMove(Vector3 dir)
    {
        old_dir = get_player_dir("FRONT");

        if (dir.magnitude == 0)
            return;

        bool changedDir = false;
        //print (dir);

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
                        post_act += "Left";
                    else
                        post_act += "Right";

                    reportOnEcho();
                    reportSent = false;
                }
            }
            else
                return;
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
                    GameManager.instance.boardScript.gamerecord += "b";
            }
        }

        //Inform player about progress
        if (true)
        {
            GameManager.instance.boardScript.sol = "";
            for (int i = 0; i < GameManager.instance.boardScript.searched_temp.Length; ++i)
                GameManager.instance.boardScript.searched_temp[i] = false;

            Vector2 idx_location = GameManager.instance.boardScript.get_idx_from_pos(transform.position);
            GameManager.instance.boardScript.solveMazeMid(idx_location, "s");
            int remaining_steps = GameManager.instance.boardScript.sol.Length;
            if (remaining_steps >= 2)
                remaining_steps -= 2;
            int total_step = GameManager.instance.boardScript.mazeSolution.Length - 1;
            float ratio = (float)remaining_steps / total_step;
            if ((!reachQuarter) && (ratio <= 0.75f) && (ratio > 0.5f))
            {
                reachQuarter = true;
                //if (GameManager.instance.boardScript.latest_clip != null) {
                //	GameManager.instance.boardScript.restore_audio = true;
                //	GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
                //}
                //SoundManager.instance.PlayVoice (Resources.Load ("instructions/You are 25% of the way through this level") as AudioClip);
            }
            else if ((!reachHalf) && (ratio <= 0.5f) && (ratio > 0.25f))
            {
                reachHalf = true;
                if (GameManager.instance.boardScript.latest_clip != null)
                {
                    GameManager.instance.boardScript.restore_audio = true;
                    GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
                }
                SoundManager.instance.PlayVoice(Resources.Load("instructions/You are halfway there") as AudioClip);
                Logging.Log("50%", Logging.LogLevel.NORMAL);
            }
            else if ((!reach3Quarter) && (ratio <= 0.25f))
            {
                //reach3Quarter = true;
                //if (GameManager.instance.boardScript.latest_clip != null) {
                //	GameManager.instance.boardScript.restore_audio = true;
                //	GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
                //}
                //SoundManager.instance.PlayVoice (Resources.Load ("instructions/You are 75% of the way through this level") as AudioClip);
                //print ("75%");
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
        //Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
        bool canMove = base.AttemptMove<T>(xDir, yDir);
        numSteps += 1;
        //If player could not move to that location, play the crash sound
        if (!canMove)
        {
            GameManager.instance.boardScript.gamerecord += "C";
            //if(!SoundManager.instance.isBusy())
            SoundManager.instance.playcrash(Database.instance.wallHit);
            //Increment the crash count
            numCrashes++;
            //Decrement the step count (as no successful step was made)
            reportOnCrash(); //send crash report

            //Add the crash location details
            string loc = transform.position.x.ToString() + "," + transform.position.y.ToString();
            //TODO put those two lines back
            //string crashPos = getCrashDescription((int) transform.position.x, (int) transform.position.y);
            //loc = loc + "," + crashPos;
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
        //Hit allows us to reference the result of the Linecast done in Move.
        //RaycastHit2D hit;

        //GameManager.instance.playersTurn = false;
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

        Logging.Log(System.Text.Encoding.ASCII.GetString(crashForm.data), Logging.LogLevel.LOW_PRIORITY);

        WWW www = new WWW(crashEndpoint, crashForm);
        StartCoroutine(Utilities.WaitForRequest(www));
    }

    /// <summary>
    /// 
    /// </summary>
	private void attemptExitFromLevel()
    {
        //Increment step count
        //numSteps += 1;
        exitAttempts++;

        BoardManager.echoDistData data =
            GameManager.instance.boardScript.getEchoDistData(transform.position, get_player_dir("FRONT"), get_player_dir("LEFT"));

        float wallDist = 0.8f;
        //catogrize the distance
        //front
        if ((data.frontDist <= wallDist) && (data.leftDist <= wallDist) && (data.rightDist <= wallDist))
            Logging.Log("Not exit, in Wrong Dead end!", Logging.LogLevel.LOW_PRIORITY);

        GameObject exitSign = GameObject.FindGameObjectWithTag("Exit");
        Vector2 distFromExit = transform.position - exitSign.transform.position;
        if (Vector2.SqrMagnitude(distFromExit) < 0.25)
        {
            //Calculate time elapsed during the game level
            endLevel();
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

        //Calculate the points for the game level
        //Score based on: time taken, num crashes, steps taken, trying(num echoes played on same spot)
        //Finish in less than 15 seconds => full score
        //For every 10 seconds after 15 seconds, lose 100 points
        //For every crash, lose 150 points
        //For every step taken over the optimal steps, lose 50 points
        //Max score currently is 1500 points
        int score = 5000;
        /*
		if (timeElapsed > 15) {
			score = score - (((timeElapsed - 16) / 10) + 1) * 100;
		}
		*/
        //if numSteps > numOptimalSteps, then adjust score
        //Calculate optimal steps by getting start position and end position
        //and calculate the number of steps
        if (numCrashes > 0)
        {
            score = score - numCrashes * 300;
        }

        if (numSteps - GameManager.instance.boardScript.mazeSolution.Length + 1 > 0)
            score -= 100 * (numSteps - GameManager.instance.boardScript.mazeSolution.Length + 1);
        //Check if the score went below 0
        if (score < 1000)
        {
            score = 1000;
        }
        Logging.Log(numSteps, Logging.LogLevel.NORMAL);
        Logging.Log(GameManager.instance.boardScript.mazeSolution.Length - 1, Logging.LogLevel.NORMAL);
        Logging.Log(numCrashes, Logging.LogLevel.NORMAL);

        //TODO(agotsis) understand this. Reimplement.
        //Send the crash count data and level information to server
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

        Logging.Log(System.Text.Encoding.ASCII.GetString(levelCompleteForm.data), Logging.LogLevel.LOW_PRIORITY);

        //Send the name of the echo files used in this level and the counts
        //form.AddField("echoFileNames", getEchoNames());

        //Send the details of the crash locations
        //form.AddField("crashLocations", crashLocs);

        levelCompleteForm.AddField("score", score);

        WWW www = new WWW(levelDataEndpoint, levelCompleteForm);
        StartCoroutine(Utilities.WaitForRequest(www));

        //display score
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

        //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
        restarted = true;
        Invoke("Restart", restartLevelDelay);
        //Disable the player object since level is over.
        //enabled = true;

        GameManager.instance.level += 1;
        GameManager.instance.boardScript.write_save(GameManager.instance.level);
        GameManager.instance.playersTurn = false;
        SoundManager.instance.PlaySingle(Database.instance.winSound);
        //AudioSource.PlayClipAtPoint (winSound, transform.localPosition, 1.0f);

        //Reset extra data.
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
        //the code is the first digit of device id

        Logging.Log(System.Text.Encoding.ASCII.GetString(echoForm.data), Logging.LogLevel.LOW_PRIORITY);

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
            if (SoundManager.instance.PlayVoice(Database.instance.menuClips[cur_clip]))
            {
                cur_clip += 1;
                if (cur_clip >= Database.instance.menuClips.Length)
                    cur_clip = 0;
            }
        }
    }

    //control
    //"touch is how many finger on the screen"
    int touch_simple, touch_audio, touch_exit, touch_menu;
    //tap is how many times player tap the screen
    int tap_simple, tap_exit, tap_menu;
    bool echoLock = false;
    Vector2 swipeStartPlace = new Vector2();
    Vector2 firstSwipePos = new Vector2();
    Vector2 VecStart = new Vector2();
    Vector2 VecEnd = new Vector2();
    List<Touch> touches;
    CDTimer TriggerechoTimer;
    CDTimer TriggermenuTimer;
    CDTimer TriggerrotateTimer;

    void Update()
    {
        play_audio();
        //UnityEngine.Debug.DrawLine (transform.position, transform.position+get_player_dir("FRONT"), Color.green);
        //UnityEngine.Debug.DrawLine (transform.position, transform.position+get_player_dir("LEFT"), Color.yellow);
        //If it's not the player's turn, exit the function.
        if (!GameManager.instance.playersTurn)
            return;

        if (!localRecordWritten)
        {
            //update stats
            GameManager.instance.boardScript.local_stats[curLevel] += 1;
            GameManager.instance.boardScript.write_local_stats();
            localRecordWritten = true;
        }

        Vector3 dir = Vector3.zero;

        //Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction\
        if (eh.isActivate())
        {
            InputEvent ie = eh.getEventData();
            switch (ie.keycode)
            {
                case KeyCode.RightArrow:
                    if (!want_exit)
                    {
                        dir = -transform.up;
                        SoundManager.instance.PlaySingle(Database.instance.swipeRight);
                    }
                    else
                    {
                        GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                        Destroy(GameObject.Find("GameManager"));
                        SceneManager.LoadScene("Main");
                    }
                    break;
                case KeyCode.LeftArrow:
                    if (!want_exit)
                    {
                        dir = get_player_dir("LEFT");
                        SoundManager.instance.PlaySingle(Database.instance.swipeLeft);
                    }
                    else
                    {
                        //SceneManager.UnloadScene("Main");
                        Destroy(GameObject.Find("GameManager"));
                        SceneManager.LoadScene("Title_Screen");
                    }
                    break;
                case KeyCode.UpArrow:
                    dir = transform.right;
                    SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
                    break;
                case KeyCode.DownArrow:
                    dir = -transform.right;
                    SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
                    break;
                case KeyCode.F:
                    GameManager.instance.boardScript.gamerecord += "E{";
                    PlayEcho();
                    GameManager.instance.boardScript.gamerecord += lastEcho;
                    GameManager.instance.boardScript.gamerecord += "}";
                    break;
                case KeyCode.E:
                    if (!want_exit)
                    {
                        GameManager.instance.boardScript.gamerecord += "X";
                        attemptExitFromLevel();
                    }
                    else
                        want_exit = false;
                    break;
                case KeyCode.R:
                    want_exit = true;
                    reset_audio = true;
                    break;
                case KeyCode.M:
                    if (GameManager.levelImageActive)
                    {
                        GameManager.instance.HideLevelImage();
                        GameManager.instance.boardScript.gamerecord += "S_OFF";
                    }
                    else
                    {
                        GameManager.instance.UnHideLevelImage();
                        GameManager.instance.boardScript.gamerecord += "S_ON";
                    }
                    break;
                default:
                    break;
            }
        }
        //Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		//pop up the survey at the end of tutorial
		Vector2 distFromExit = transform.position - GameManager.instance.boardScript.exitPos;
		if ( (Vector2.SqrMagnitude (distFromExit) < 0.25f) && survey_activated ) {
			if( (GameManager.instance.level == 11)&&(!survey_shown) ){
				ad.clearflag();
				ad.DisplayAndroidWindow ("Would you like to take \n a short survey about the game?");
				survey_shown = true;
			}

			if(survey_shown && !URL_shown && ad.yesclicked() && !code_entered){

				//display a code, and submit it reportSurvey()
				// Please enter code (first six digits of UDID) on the survey page
				code_entered = true;

				if (SystemInfo.deviceUniqueIdentifier.Length <= 6)
					surveyCode = SystemInfo.deviceUniqueIdentifier;
				else
					surveyCode = SystemInfo.deviceUniqueIdentifier.Substring (0, 6);
				string codemsg = "Your survey code is: \n" + surveyCode + "\n please enter this in the survey.";
				ad.clearflag ();
				ad.DisplayAndroidWindow (codemsg, AndroidDialogue.DialogueType.YESONLY);
			}else if (!URL_shown && ad.yesclicked() && code_entered){
				URL_shown = true;
				Application.OpenURL("https://echolock.andrew.cmu.edu/survey/");//"http://echolock.andrew.cmu.edu/survey/?"
			}else if (URL_shown){
				ad.clearflag();
				ad.DisplayAndroidWindow("Thank you for taking the survey!", AndroidDialogue.DialogueType.YESONLY);
				reportsurvey(surveyCode);
				survey_activated = false;
			}
		}

		//process input
		if(eh.isActivate()){
			InputEvent ie = eh.getEventData();
			if ((ie.touchNum == touch_simple)&&(ie.hasDir())){//a swipe
				flipEchoLock(true);
				if(!at_pause_menu){
					if(ie.isUp){
						dir = get_player_dir("FRONT");
						SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
					}
				} else {//at pause menu
					if(ie.isDown){//turn on/of black screen
						if(GameManager.levelImageActive){
							GameManager.instance.HideLevelImage();
							GameManager.instance.boardScript.gamerecord += "S_OFF";
						}else{
							GameManager.instance.UnHideLevelImage();
							GameManager.instance.boardScript.gamerecord += "S_ON"; // dont forget to turn these two back on!
						}
						at_pause_menu = false;
						SoundManager.instance.PlayVoice(Database.instance.menuOff, true);//shoule have another set of sound effect
					}else if(ie.isLeft){//restart level
						SoundManager.instance.playcrash(Database.instance.inputSFX);
						//GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
						SoundManager.instance.PlayVoice(Database.instance.menuOff, true);//shoule have another set of sound effect
						Destroy(GameObject.Find("GameManager"));
						//Destroy(this);
						SceneManager.LoadScene("Main");
					}else if(ie.isRight){//return to main menu
						SoundManager.instance.playcrash(Database.instance.inputSFX);
						Destroy(GameObject.Find("GameManager"));
						SceneManager.LoadScene("Title_Screen");
					}else if(ie.isUp){//repeat audio (duplicate)
						//getHint ();
					}					
				}
			} else if ((ie.touchNum == 2) && (!ie.hasDir())){//turn on/off menu
				flipEchoLock(true);
				if(ie.elapsedTime >= Const.MENU_TOUCH_TIME) {
					if(TriggermenuTimer.CDfinish()) {//turn on/off pause menu
						if(!at_pause_menu){
							at_pause_menu = true;
							if(SoundManager.instance.voiceSource.isPlaying){
								GameManager.instance.boardScript.restore_audio = true;
								GameManager.instance.boardScript.latest_clip = SoundManager.instance.voiceSource.clip;
							}
							SoundManager.instance.PlayVoice(Database.instance.menuOn, true);
						}else{//menu already open, now close it
							at_pause_menu = false;
							SoundManager.instance.PlayVoice(Database.instance.menuOff, true);
						}
						TriggermenuTimer.reset();
					}
				}
			} else if ( (ie.isRotate)&&(ie.hasDir()) ){
				flipEchoLock(true);
				if(!at_pause_menu && TriggerrotateTimer.CDfinish()){
					if(ie.isLeft){
						dir = get_player_dir("LEFT");
						if (!GameManager.instance.boardScript.turning_lock)
							SoundManager.instance.PlaySingle(Database.instance.swipeLeft);
					}else if(ie.isRight){
						dir = get_player_dir("RIGHT");
						if (!GameManager.instance.boardScript.turning_lock)
							SoundManager.instance.PlaySingle(Database.instance.swipeRight);
					}
					TriggerrotateTimer.reset();
				}
			} else if( (ie.touchNum == touch_simple)&&(!ie.hasDir()) ){//a tap
				if(!at_pause_menu){
					if (ie.cumulativeTouchNum >= touch_exit){
						GameManager.instance.boardScript.gamerecord += "X";
						attemptExitFromLevel();
					} else if((ie.elapsedTime > Const.opsToEchoCD)&&(ie.elapsedTime < Const.opsToEchoCD+0.02f)&&TriggerechoTimer.CDfinish()&&(!echoLock)){
						GameManager.instance.boardScript.gamerecord += "E{";
						PlayEcho();
						GameManager.instance.boardScript.gamerecord += lastEcho;
						GameManager.instance.boardScript.gamerecord += "}";
						TriggerechoTimer.reset();
						flipEchoLock(true);
					}
				}
			}
			flipEchoLock(false);
		}
			

#endif //End of mobile platform dependendent compilation section started above with #elif
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

    //Returns a description of the location of the crash (for analysis)
    //Currently, the ouput is from the following list of options
    //["End of the Corridor", "Intersection of 2 Corridors", "Start of the Corridor",
    //"Middle of the Corridor", "Towards End of the Corridor", "Towards Start of the Corridor"
    //"Crashed while on the Exit Sign"];
    //Currently not returning the Towards Start/End descriptions due to only having 7 discrete
    //movements in each x/y direction. May be relevant in the future.
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

        //positions.Contains (xLoc, yLoc);

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

        //All the crash location options
        //string[] locs = ["End of the Corridor", "Intersection of 2 Corridors", "Start of the Corridor", "Middle of the Corridor", "Towards End of the Corridor", "Towards Start of the Corridor"];

        //If Crash happened while on the Exit Sign
        GameObject exitSign = GameObject.FindGameObjectWithTag("Exit");
        if ((xLoc == (int)exitSign.transform.position.x) & (yLoc == (int)exitSign.transform.position.y))
        {
            return "Crashed while on the Exit Sign";
        }
        //TODO(agotsis/wenyuw1) This hardcoding needs to go away. Mainly here to test the database.
        //For the x direction
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
        //Set hitWall to equal the component passed in as a parameter.
        Wall hitWall = component as Wall;
        //if(!SoundManager.instance.isBusy())
        SoundManager.instance.playcrash(Database.instance.wallHit);
    }

    private void OnDisable()
    {
        //When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
        //int nextLevel = curLevel + 1;
        //GameManager.instance.level = nextLevel;
    }

    //Restart reloads the scene when called.
    private void Restart()
    {
        //Load the last scene loaded, in this case Main, the only scene in the game.
        SceneManager.LoadScene("Main");
        restarted = false;
    }

    protected override void OnMove()
    {

    }
}
