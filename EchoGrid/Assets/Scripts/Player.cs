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
public class Player : MovingObject
{
	public enum dist_type{
		WALL,
		SHORT,
		MID,
		LONG,
	}

	public static Player instance;
	public float restartLevelDelay = 3.0f;
	//Delay time in seconds to restart level.
	private Vector2 touchOrigin = -Vector2.one;

	private float touchTime = 0f;
	private float minSwipeDist = 100f;
	bool restarted = false;
	bool is_freezed;//is player not allowed to do anything?
	bool tapped;//did player tap to hear an echo at this position?
	bool reportSent;
	private int curLevel;

	//TODO(agotsis/wenyuw1) This needs to be integrated with the local database so these are not hardcoded

	public AudioClip wallHit;
	public AudioClip winSound;
	public AudioClip walking;
	AudioClip inputSFX;
	AudioClip menuOn, menuOff;
	AudioClip[] menuClips;

	//TODO(agotsis/wenyuw1) This volume of these sounds may need to go down
	public AudioClip swipeAhead;
	public AudioClip swipeRight;
	public AudioClip swipeLeft;

	int cur_clip = 0;
	int max_quit_clip = 2;
	bool reset_audio;

	//private SpriteRenderer spriteRenderer;

	// variables to implement data collection
	private int numCrashes;
	//Keep track of number of times user crashed into wall
	private int numSteps;
	//Keep track of number of steps taken per level
	private int exitAttempts;

	private String lastEcho = "";

	//Track locations of the player's crashes
	private string crashLocs;

	//Keep track of time taken for the game level
	private Stopwatch stopWatch;
	private DateTime startTime;
	private DateTime endTime;

	bool want_exit;
	bool swp_lock = false;//stop very fast input
	bool at_pause_menu = false;//indicating if the player activated pause menu
	static bool level_already_loaded = false;
	bool localRecordWritten = false;

	public Text debug_text;

	//Create a new instance of RSACryptoServiceProvider.
	private RSACryptoServiceProvider encrypter = new RSACryptoServiceProvider ();

	//public bool soundPlaying = false;

	private void initData ()
	{
		numCrashes = 0;
		numSteps = 0;
		crashLocs = "";
	}

	private void initEncrypt ()
	{
		string publicKeyString = "MIGdMA0GCSqGSIb3DQEBAQUAA4GLADCBhwKBgQC1hBlMytDpiLGqCNGfx+IvbRH9edqFcxJoL5CuEPOjr31u9PXTgtSuZhldKc9KpPR4j62M6+UxSs9abDd1/C0txQEB4Jxe/FPMOBmlvNHNHLw6htPx5JRHzN1cegi3W6Qd8YRMi3XfSx5tGx0NNLxuf+EDrE5NIVUdp0hpQ7yMFQIBAw==";
		byte[] publicKeyBytes = Convert.FromBase64String (publicKeyString);

		byte[] Exponent = {3};


		//Create a new instance of RSAParameters.
		RSAParameters RSAKeyInfo = new RSAParameters ();

		//Set RSAKeyInfo to the public key values.
		RSAKeyInfo.Modulus = publicKeyBytes;
		RSAKeyInfo.Exponent = Exponent;

		//Import key parameters into RSA.
		encrypter.ImportParameters (RSAKeyInfo);

		//UnityEngine.Debug.Log (encrypt ("This is a test String"));
	}

	private String encrypt (String encryptThis)
	{
		string b64EncryptThis = Base64Encode (encryptThis);

		//Encrypt the symmetric key and IV.
		byte[] encryptedString = encrypter.EncryptValue (Convert.FromBase64String (b64EncryptThis));

		//Add the encrypted test string to the form
		return Convert.ToBase64String (encryptedString);
	}

	void Awake(){

		level_already_loaded = false;

		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		enabled = true;
		DontDestroyOnLoad (gameObject);

	}

	protected override void Start ()
	{
		curLevel = GameManager.instance.level;
		//spriteRenderer = GetComponent<SpriteRenderer>();

		//Initialize data collection variables
		initData ();
		initEncrypt ();

		/*
		//TODO(agotsis/wenyuw1) Once the local database is integrated this hardcoding will go away.
		numEcho1 = 0;
		numEcho2 = 0;
		numEcho3 = 0;
		numEcho4 = 0;
		numEcho5 = 0;
		numEcho6 = 0;
		numEcho7 = 0;
		*/

		//Initialize list of crash locations
		crashLocs = "";

		//Adjust player scale
		Vector3 new_scale = transform.localScale;
		new_scale *= (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;
		transform.localScale = new_scale;

		//Start the time for the game level
		stopWatch = new Stopwatch ();
		stopWatch.Start ();
		startTime = System.DateTime.Now;

		want_exit = false;
		at_pause_menu = false;
		swp_lock = false;
		reset_audio = false;
		tapped = false;
		reportSent = false;

		//load audio
		inputSFX = Resources.Load ("fx/inputSFX") as AudioClip;
		menuOn = Resources.Load ("instructions/Menu opened") as AudioClip;
		menuOff = Resources.Load ("instructions/Menu closed") as AudioClip;
		menuClips = new AudioClip[5];
		menuClips [0] = Resources.Load ("instructions/Swipe left to restart the current level") as AudioClip;
		menuClips [1] = Resources.Load ("instructions/Swipe left to return to the tutorial, swipe right to return to the main menu, and swipe down to toggle the screen on and off") as AudioClip;
		menuClips [2] = Resources.Load ("instructions/Swipe up to hear a hint") as AudioClip;
		menuClips [3] = Resources.Load ("instructions/To close the menu, press and hold with two fingers") as AudioClip;
		menuClips [4] = Resources.Load ("instructions/2sec_silence") as AudioClip;

		//specify controls
		if(Utilities.OLD_ANDROID_SUPPORT){
			touch_simple = 1;
			touch_audio = 2;
			touch_exit = 1;
			touch_menu = 2;
			tap_simple = 1;
			tap_exit = 2;
			tap_menu = 2;
		} else{
			touch_simple = 1;
			touch_audio = 2;
			touch_exit = 2;
			touch_menu = 3;
			tap_simple = 1;
			tap_exit = 2;
			tap_menu = 1;
		}
		multiTapStartTime = 0.0f;
		echoTapTime = 0.0f;
		rotateGestStartTime = 0.0f;
		menuTapTime = 0.0f;
		echoPlayedThisTouch = false;
		menuUpdatedThisTouch = false;
		TouchTapCount = 0;
		level_already_loaded = false;

		base.Start ();
	}

	void OnLevelWasLoaded(int index){
		//if (!level_already_loaded) {
		//	level_already_loaded = true;
			//Initialize data collection variables
			initData ();
			//Initialize list of crash locations
			crashLocs = "";
			curLevel = GameManager.instance.level;
			stopWatch = new Stopwatch ();
			stopWatch.Start ();
			startTime = System.DateTime.Now;

			want_exit = false;
			at_pause_menu = false;
			swp_lock = false;
			reset_audio = false;
			tapped = false;
			reportSent = false;

			multiTapStartTime = 0.0f;
			echoTapTime = 0.0f;
			rotateGestStartTime = 0.0f;
			menuTapTime = 0.0f;
			echoPlayedThisTouch = false;
			menuUpdatedThisTouch = false;
			TouchTapCount = 0;
			localRecordWritten = false;

			base.Start ();
		//}
	}

	public enum DistRange{
		SHORT,
		MID,
		LONG,
	}

	private string _dist_type_to_string(dist_type type){
		switch (type) {
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

	string[] frontDistS = {"2.25", "3.75"};
	string[] frontDistM = {"5.25", "6.75"};
	string[] frontDistL = {"8.25", "9.75", "11.25", "12.75"};

	private void PlayEcho() {
		tapped = true;
		reportSent = true;
		BoardManager.echoDistData data =
			GameManager.instance.boardScript.getEchoDistData (transform.position, get_player_dir ("FRONT"), get_player_dir ("LEFT"));

		UnityEngine.Debug.Log (data.all_jun_to_string ());

		String prefix = "C00-21"; //change this prefix when you change the echo files
		if ( (GameManager.instance.level >= 26)&&(GameManager.instance.level < 41) )
			prefix = "C00-18";
		else if ( (GameManager.instance.level >= 41)&&(GameManager.instance.level < 56) )
			prefix = "C00-15";
		else if ( (GameManager.instance.level >= 56)&&(GameManager.instance.level < 71) )
			prefix = "C00-12";
		else if ( (GameManager.instance.level >= 71)&&(GameManager.instance.level < 86) )
			prefix = "C00-9";
		else if ( (GameManager.instance.level >= 86)&&(GameManager.instance.level < 101) )
			prefix = "C00-6";
		else if ( (GameManager.instance.level >= 101)&&(GameManager.instance.level < 116) )
			prefix = "C00-3";
		else if ( (GameManager.instance.level >= 116) )
			prefix = "C00-0";

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

		filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
			data.frontDist, front_type, _dist_type_to_string(b_dtype), "D",
			_dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);

		AudioClip echo = Resources.Load ("echoes/" + filename) as AudioClip;
		string front_typeC = front_type, back_typeC = back_type, left_typeC = left_type, right_typeC = right_type;
		if (echo == null) {
			UnityEngine.Debug.Log ("replace US with Deadend");
			switch (data.exitpos) {
			case 1://left
				left_typeC= "D";
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
			echo = Resources.Load ("echoes/" + filename) as AudioClip;
			UnityEngine.Debug.Log (filename);
		}
		lastEcho = filename;

		//special cases
		//try alternative front dist

		if (echo == null) {
			UnityEngine.Debug.Log ("Secondary search_alt_front_dist");
			string frontString = "";
			if (Mathf.Abs (data.frontDist - 3.75f) <= 0.0001f) {
				frontString = "2.25";

				filename = String.Format ("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
					frontString, front_type, _dist_type_to_string (b_dtype), "D",
					_dist_type_to_string (l_dtype), left_type, _dist_type_to_string (r_dtype), right_type);
				echo = Resources.Load ("echoes/" + filename) as AudioClip;
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
		if (echo == null) {
			UnityEngine.Debug.Log ("Secondary search_wall");
			string frontString = "";
			if(f_dtype == dist_type.WALL)
				frontString = "0.75";

			filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
				frontString, front_type, _dist_type_to_string(b_dtype), "D",
				_dist_type_to_string(l_dtype), left_type, _dist_type_to_string(r_dtype), right_type);
			echo = Resources.Load ("echoes/" + filename) as AudioClip;
			lastEcho = filename;
		}

		//other cases
		if (echo == null) {
			bool found = false;
			UnityEngine.Debug.Log ("Secondary search_other");
			string frontString = "";
			if (f_dtype == dist_type.SHORT) {
				for (int i = 0; i < frontDistS.Length; ++i) {
					filename = String.Format ("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
						frontDistS [i], front_type, _dist_type_to_string (b_dtype), "D",
						_dist_type_to_string (l_dtype), left_type, _dist_type_to_string (r_dtype), right_type);
					echo = Resources.Load ("echoes/" + filename) as AudioClip;
					if (echo != null)
						found = true;
					if (found)
						break;
				}
			} else if (f_dtype == dist_type.MID) {
				for (int i = 0; i < frontDistM.Length; ++i) {
					filename = String.Format ("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
						frontDistM [i], front_type, _dist_type_to_string (b_dtype), "D",
						_dist_type_to_string (l_dtype), left_type, _dist_type_to_string (r_dtype), right_type);
					echo = Resources.Load ("echoes/" + filename) as AudioClip;
					if (echo != null)
						found = true;
					if (found)
						break;
				}
			} else if (f_dtype == dist_type.LONG) {
				for (int i = 0; i < frontDistL.Length; ++i) {
					filename = String.Format ("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
						frontDistL [i], front_type, _dist_type_to_string (b_dtype), "D",
						_dist_type_to_string (l_dtype), left_type, _dist_type_to_string (r_dtype), right_type);
					echo = Resources.Load ("echoes/" + filename) as AudioClip;
					if (echo != null)
						found = true;
					if (found)
						break;
				}
			}
			lastEcho = filename;
		}

		if (echo == null) {
			bool found = false;
			UnityEngine.Debug.Log ("replacing everything with D");
			string frontString = "";
			if (f_dtype == dist_type.SHORT) {
				for (int i = 0; i < frontDistS.Length; ++i) {
					filename = String.Format ("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
						frontDistS [i], "D", _dist_type_to_string (b_dtype), "D",
						_dist_type_to_string (l_dtype), "D", _dist_type_to_string (r_dtype), "D");
					echo = Resources.Load ("echoes/" + filename) as AudioClip;
					if (echo != null)
						found = true;
					if (found)
						break;
				}
			} else if (f_dtype == dist_type.MID) {
				for (int i = 0; i < frontDistM.Length; ++i) {
					filename = String.Format ("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
						frontDistM [i], "D", _dist_type_to_string (b_dtype), "D",
						_dist_type_to_string (l_dtype), "D", _dist_type_to_string (r_dtype), "D");
					echo = Resources.Load ("echoes/" + filename) as AudioClip;
					if (echo != null)
						found = true;
					if (found)
						break;
				}
			} else if (f_dtype == dist_type.LONG) {
				for (int i = 0; i < frontDistL.Length; ++i) {
					filename = String.Format ("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
						frontDistL [i], "D", _dist_type_to_string (b_dtype), "D",
						_dist_type_to_string (l_dtype), "D", _dist_type_to_string (r_dtype), "D");
					echo = Resources.Load ("echoes/" + filename) as AudioClip;
					if (echo != null)
						found = true;
					if (found)
						break;
				}
			}
			lastEcho = filename;
		}

		//have to use the old files
		if (echo == null) {
			UnityEngine.Debug.Log (filename);
			UnityEngine.Debug.Log ("did not find accurate one, searching everything");
				//Old version
			//this is the full filename, if back is not D or Stairs, it will be "na"
				back_type = "D"; front_type = ""; left_type = ""; right_type = "";
			if( (data.bType != BoardManager.JunctionType.DEADEND)&&(data.exitpos != 4) )
				back_type = "na";
			if (data.exitpos != 1)
				left_type = "D";
			if (data.exitpos != 2)
				right_type = "D";
			if (data.exitpos != 3)
				front_type = data.jun_to_string (data.fType);

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

			//search for the most accurate one first
			filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix,
				data.frontDist, front_type, data.backDist, "D",
				data.leftDist, left_type, data.rightDist, right_type);
			echo = Resources.Load ("echoes/" + filename) as AudioClip;
			lastEcho = filename;

			DistRange fr, br, lr, rr;
			int fr_start, fr_end, br_start, br_end, lr_start, lr_end, rr_start, rr_end;
			if (echo == null) {
				if (data.front <= 3) {
					fr = DistRange.SHORT;
					fr_start = 0;
					fr_end = 3;
				} else if ((data.front > 3) && (data.front <= 5)) {
					fr = DistRange.MID;
					fr_start = 4;
					fr_end = 5;
				} else {
					fr = DistRange.LONG;
					fr_start = 6;
					fr_end = 10;
				}

				if (data.back <= 3) {
					br = DistRange.SHORT;
					br_start = 0;
					br_end = 3;
				} else if ((data.back > 3) && (data.back <= 5)) {
					br = DistRange.MID;
					br_start = 4;
					br_end = 5;
				} else {
					br = DistRange.LONG;
					br_start = 6;
					br_end = 10;
				}

				if (data.left <= 3) {
					lr = DistRange.SHORT;
					lr_start = 0;
					lr_end = 3;
				} else if ((data.left > 3) && (data.left <= 5)) {
					lr = DistRange.MID;
					lr_start = 4;
					lr_end = 5;
				} else {
					lr = DistRange.LONG;
					lr_start = 6;
					lr_end = 10;
				}

				if (data.right <= 3) {
					rr = DistRange.SHORT;
					rr_start = 0;
					rr_end = 3;
				} else if ((data.right > 3) && (data.right <= 5)) {
					rr = DistRange.MID;
					rr_start = 4;
					rr_end = 5;
				} else {
					rr = DistRange.LONG;
					rr_start = 6;
					rr_end = 10;
				}

				bool found = false;
				string[] back_str = new string[3]{"D", "na", "US"};
				string[] front_str = new string[2]{"", "D"};
				for (int i = fr_start; i <= fr_end; ++i) {
					for (int j = br_start; j <= br_end; ++j) {
						for (int k = lr_start; k <= lr_end; ++k) {
							for (int l = rr_start; l <= rr_end; ++l) {
								front_str [0] = front_type;
								for (int bsi = 0; bsi < back_str.Length; ++bsi) {
									for (int fsi = 0; fsi < front_str.Length; ++fsi) {
										//DistS = {"2.25", "3.75"};
										//DistM = {"5.25", "6.75"};
										//DistL = {"8.25", "9.75", "11.25", "12.75"};
										string tb="",tl="",tr="";
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
											0.75f + 1.5f*i, front_str[fsi], tb, back_str[bsi],
											tl, left_type, tr, right_type);

										echo = Resources.Load ("echoes/" + filename) as AudioClip;
										if (echo != null) {
											lastEcho = filename+"ERROR";
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

		if (echo == null) {
			UnityEngine.Debug.Log ("Echo not found");
			UnityEngine.Debug.Log (lastEcho);
		}
		else {
			SoundManager.instance.PlayEcho (echo);
			UnityEngine.Debug.Log (lastEcho);
		}
	}

	string post_act = "";
	string correct_post_act = "";

	private void reportOnEcho ()
	{

		string echoEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptEchoData.py";

		Vector2 idx_location = GameManager.instance.boardScript.get_idx_from_pos (transform.position);
		string location = "(" + idx_location.x.ToString () + "," + idx_location.y.ToString () + ")";
		correct_post_act = "";
		GameManager.instance.boardScript.sol = "";
		for(int i = 0; i < GameManager.instance.boardScript.searched_temp.Length; ++i)
			GameManager.instance.boardScript.searched_temp[i] = false;
		correct_post_act = GameManager.instance.boardScript.getHint (idx_location,"s");
		//if(GameManager.instance.boardScript.sol.Length >= 2)
		//	correct_post_act = GameManager.instance.boardScript.sol[GameManager.instance.boardScript.sol.Length-2].ToString();

		Vector3 forward = old_dir;
		Vector3 sol_dir = new Vector3 ();
		if (correct_post_act == "u")
			sol_dir = Vector3.up;
		else if (correct_post_act == "d")
			sol_dir = Vector3.down;
		else if (correct_post_act == "l")
			sol_dir = Vector3.left;
		else if (correct_post_act == "r")
			sol_dir = Vector3.right;

		if(correct_post_act != ""){
			if (forward == sol_dir)
				correct_post_act = "Forward";
			else if (forward == -sol_dir)
				correct_post_act = "Turn Around";
			else {
				Vector3 angle = Vector3.Cross (forward, sol_dir);
				if(angle.z > 0)
					correct_post_act = "Turn Left";
				else 
					correct_post_act = "Turn Right";
			}
		} else 
			correct_post_act = "Exit";


		WWWForm echoForm = new WWWForm ();
		echoForm.AddField ("userName", encrypt (SystemInfo.deviceUniqueIdentifier));
		echoForm.AddField ("currentLevel", encrypt (curLevel.ToString ()));
		echoForm.AddField ("trackCount", encrypt (GameManager.instance.boardScript.local_stats[curLevel].ToString()));
		echoForm.AddField ("echo", lastEcho); //fix
		echoForm.AddField ("echoLocation", encrypt (location));
		echoForm.AddField ("postEchoAction", encrypt (post_act));
		echoForm.AddField ("correctAction", encrypt (correct_post_act));
		echoForm.AddField ("dateTimeStamp", encrypt (System.DateTime.Now.ToString ()));

		UnityEngine.Debug.Log (System.Text.Encoding.ASCII.GetString (echoForm.data));

		WWW www = new WWW (echoEndpoint, echoForm);
		StartCoroutine (WaitForRequest (www));
	}

	void getHint (){

		Vector2 idx_location = GameManager.instance.boardScript.get_idx_from_pos (transform.position);
		correct_post_act = "";
		GameManager.instance.boardScript.sol = "";
		for(int i = 0; i < GameManager.instance.boardScript.searched_temp.Length; ++i)
			GameManager.instance.boardScript.searched_temp[i] = false;
		correct_post_act = GameManager.instance.boardScript.getHint (idx_location,"s");

		AudioClip clip;
		if (correct_post_act.Length <= 0) {
			clip = Resources.Load("instructions/You should exit") as AudioClip;
			SoundManager.instance.PlayVoice (clip, true);
			return;
		}
		Vector3 forward = old_dir;
		Vector3 sol_dir = new Vector3 ();
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
		else {
			Vector3 angle = Vector3.Cross (forward, sol_dir);
			if(angle.z > 0)
				clip = Resources.Load("instructions/You should turn left") as AudioClip;
			else 
				clip = Resources.Load("instructions/You should turn right") as AudioClip;
		}

		SoundManager.instance.PlayVoice (clip, true);
	}

	//due to the chaotic coord system
	//return the relative direction
	public Vector3 get_player_dir(string dir){
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
	void rotateplayer (Vector3 dir)
	{
		if (dir == get_player_dir ("FRONT"))
			return;
		else if (dir == get_player_dir ("BACK"))
			return;
		else if (dir == get_player_dir ("LEFT")) {
			transform.Rotate (new Vector3 (0, 0, 90));
			GameManager.instance.boardScript.gamerecord += "l";
		} else if (dir == get_player_dir ("RIGHT")) {
			transform.Rotate (new Vector3 (0, 0, -90));
			GameManager.instance.boardScript.gamerecord += "r";
		}
	}

	//used to be called from outside
	public void rotateplayer_no_update (BoardManager.Direction dir)
	{
		if (dir == BoardManager.Direction.FRONT)
			transform.Rotate (new Vector3 (0, 0, 90));
		else if (dir == BoardManager.Direction.BACK)
			transform.Rotate (new Vector3 (0, 0, -90));
		else if (dir == BoardManager.Direction.LEFT) {
			transform.Rotate (new Vector3 (0, 0, 180));
		} else if (dir == BoardManager.Direction.RIGHT) {
			transform.Rotate (new Vector3 (0, 0, 0));
		}
	}

	private void calculateMove (Vector3 dir)
	{
		old_dir = get_player_dir ("FRONT");

		if (dir.magnitude == 0)
			return;

		bool changedDir = false;
		//print (dir);

		if ((dir != get_player_dir ("FRONT")) && (dir != get_player_dir ("BACK"))) {
			changedDir = true;
			rotateplayer (dir);
			if (reportSent) {
				post_act = "Turn ";
				if( (dir - get_player_dir("LEFT")).magnitude <= 0.01f )
					post_act += "Left";
				else
					post_act += "Right";

				reportOnEcho ();
				reportSent = false;
			}
		}

		dir.Normalize ();

		if (!changedDir) {
			if (AttemptMove<Wall> ((int)dir.x, (int)dir.y)) {
				tapped = false;
				if (dir == get_player_dir ("FRONT")) {
					GameManager.instance.boardScript.gamerecord += "f";
				}if (dir == get_player_dir ("BACK"))
					GameManager.instance.boardScript.gamerecord += "b";
			}
		}
	}

	public bool tapped_at_this_block(){
		return tapped;
	}

	protected override bool AttemptMove <T> (int xDir, int yDir)
	{
		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		bool canMove = base.AttemptMove <T> (xDir, yDir);
		numSteps++;
		//If player could not move to that location, play the crash sound
		if (!canMove) {
			GameManager.instance.boardScript.gamerecord += "C";
			//if(!SoundManager.instance.isBusy())
			SoundManager.instance.playcrash (wallHit);
			//Increment the crash count
			numCrashes++;
			//Decrement the step count (as no successful step was made)
			numSteps--;
			reportOnCrash (); //send crash report

			//Add the crash location details
			string loc = transform.position.x.ToString () + "," + transform.position.y.ToString ();
			//TODO put those two lines back
			//string crashPos = getCrashDescription((int) transform.position.x, (int) transform.position.y);
			//loc = loc + "," + crashPos;
			if (crashLocs.Equals ("")) {
				crashLocs = loc;
			} else {
				crashLocs = crashLocs + ";" + loc;
			}

			if (reportSent) {
				post_act = "Crash";
				reportOnEcho ();
				reportSent = false;
			}
		}

		if (reportSent) {
			post_act = "Forward";
			reportOnEcho ();
			reportSent = false;
		}
		//Hit allows us to reference the result of the Linecast done in Move.
		//RaycastHit2D hit;

		//GameManager.instance.playersTurn = false;
		return canMove;
	}

	private void reportOnCrash ()
	{

		string crashEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptCrashData.py";

		Vector2 idx_pos = GameManager.instance.boardScript.get_idx_from_pos (transform.position);
		string location = "(" + idx_pos.x.ToString () + "," + idx_pos.y.ToString () + ")";

		WWWForm crashForm = new WWWForm ();
		crashForm.AddField ("userName", encrypt (SystemInfo.deviceUniqueIdentifier));
		crashForm.AddField ("currentLevel", encrypt (curLevel.ToString ()));
		crashForm.AddField ("trackCount", encrypt (GameManager.instance.boardScript.local_stats[curLevel].ToString()));
		crashForm.AddField ("crashNumber", encrypt (numCrashes.ToString ()));
		crashForm.AddField ("crashLocation", encrypt (location));
		crashForm.AddField ("dateTimeStamp", encrypt (System.DateTime.Now.ToString ()));

		UnityEngine.Debug.Log (System.Text.Encoding.ASCII.GetString (crashForm.data));

		WWW www = new WWW (crashEndpoint, crashForm);
		StartCoroutine (WaitForRequest (www));
	}

	private void attemptExitFromLevel ()
	{
		exitAttempts++;
		GameObject exitSign = GameObject.FindGameObjectWithTag ("Exit");
		Vector2 distFromExit = transform.position - exitSign.transform.position;
		if (Vector2.SqrMagnitude (distFromExit) < 0.25) {
			//Calculate time elapsed during the game level
			endLevel ();
		}

		if (reportSent) {
			post_act = "Exit";
			reportOnEcho ();
			reportSent = false;
		}
	}

	private void endLevel ()
	{
		stopWatch.Stop ();
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
		int score = 1500;
		if (timeElapsed > 15) {
			score = score - (((timeElapsed - 16) / 10) + 1) * 100;
		}
		if (numCrashes > 0) {
			score = score - numCrashes * 150;
		}
		//Check if the score went below 0
		if (score < 0) {
			score = 0;
		}
		//TODO
		//if numSteps > numOptimalSteps, then adjust score
		//Calculate optimal steps by getting start position and end position
		//and calculate the number of steps


		//TODO(agotsis) understand this. Reimplement.
		//Send the crash count data and level information to server
		string levelDataEndpoint = "http://echolock.andrew.cmu.edu/cgi-bin/acceptLevelData.py";
		int temp = GameManager.instance.boardScript.local_stats [curLevel];

		WWWForm levelCompleteForm = new WWWForm ();
		levelCompleteForm.AddField ("userName", encrypt (SystemInfo.deviceUniqueIdentifier));
		levelCompleteForm.AddField ("currentLevel", encrypt (curLevel.ToString ()));
		levelCompleteForm.AddField ("trackCount", encrypt (temp.ToString()));
		levelCompleteForm.AddField ("crashCount", encrypt (numCrashes.ToString ()));
		levelCompleteForm.AddField ("stepCount", encrypt (numSteps.ToString ()));
		levelCompleteForm.AddField ("startTime", encrypt (startTime.ToString ()));
		levelCompleteForm.AddField ("endTime", encrypt (endTime.ToString ()));
		levelCompleteForm.AddField ("timeElapsed", encrypt (accurateElapsed.ToString ("F3")));
		levelCompleteForm.AddField ("exitAttempts", encrypt (exitAttempts.ToString ()));
		levelCompleteForm.AddField ("asciiLevelRep", encrypt (GameManager.instance.boardScript.asciiLevelRep));
		levelCompleteForm.AddField ("levelRecord", (GameManager.instance.boardScript.gamerecord));

		UnityEngine.Debug.Log (System.Text.Encoding.ASCII.GetString (levelCompleteForm.data));

		//Send the name of the echo files used in this level and the counts
		//form.AddField("echoFileNames", getEchoNames());

		//Send the details of the crash locations
		//form.AddField("crashLocations", crashLocs);

		levelCompleteForm.AddField ("score", score);

		WWW www = new WWW (levelDataEndpoint, levelCompleteForm);
		StartCoroutine (WaitForRequest (www));

		//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
		restarted = true;
		Invoke ("Restart", restartLevelDelay);
		//Disable the player object since level is over.
		//enabled = true;

		GameManager.instance.level += 1;
		GameManager.instance.boardScript.write_save (GameManager.instance.level);
		GameManager.instance.playersTurn = false;
		SoundManager.instance.PlaySingle (winSound);
		//AudioSource.PlayClipAtPoint (winSound, transform.localPosition, 1.0f);

		//Reset extra data.
		resetData ();
	}

	private void resetData ()
	{
		numCrashes = 0;
		exitAttempts = 0;
	}

	public static string Base64Encode (string plainText)
	{
		var plainTextBytes = System.Text.Encoding.UTF8.GetBytes (plainText);
		return System.Convert.ToBase64String (plainTextBytes);
	}

	//	//Creates a comma delimited string containing all the echo file names used in the level
	//	//and the corresponding number of times the echo was played
	//	private string getEchoNames() {
	//		//TODO(agotsis/wenyuw1) Once the local database is integrated, this hardcoding will go away.
	//		string allNames = "";
	//		allNames = allNames + echo1m.name + ":" + numEcho1.ToString() + ",";
	//		allNames = allNames + echo2m.name + ":" + numEcho2.ToString() + ",";
	//		allNames = allNames + echo3m.name + ":" + numEcho3.ToString() + ",";
	//		allNames = allNames + echo4m.name + ":" + numEcho4.ToString() + ",";
	//		allNames = allNames + echo5m.name + ":" + numEcho5.ToString() + ",";
	//		allNames = allNames + echo6m.name + ":" + numEcho6.ToString() + ",";
	//		allNames = allNames + echo7m.name + ":" + numEcho7.ToString();
	//
	//		return allNames;
	//	}


	//Makes HTTP requests and waits for response and checks for errors
	IEnumerator WaitForRequest (WWW www)
	{
		yield return www;

		//Check for errors
		if (www.error == null) {
			JSONNode data = JSON.Parse (www.data);
			//Debug.Log("this is the parsed json data: " + data["testData"]);
			//Debug.Log(data["testData"]);
			UnityEngine.Debug.Log ("WWW.Ok! " + www.data);
		} else {
			UnityEngine.Debug.Log ("WWWError: " + www.error);
		}
	}

	void play_audio ()
	{
		if(at_pause_menu){
			if (SoundManager.instance.PlayVoice (menuClips[cur_clip])) {
				cur_clip += 1;
				if (cur_clip >= menuClips.Length)
					cur_clip = 0;
			}
		}
	}

	//control
	//"touch is how many finger on the screen"
	int touch_simple, touch_audio, touch_exit, touch_menu;
	//tap is how many times player tap the screen
	int tap_simple, tap_exit, tap_menu;
	int TouchTapCount;
	int numTouchlastframe = 0;
	const float multiTapCD = 0.4f;//make multitap easier
	const float echoCD = 0.1f;//shortest time between two PlayEcho() calls
	const float menuUpdateCD = 0.5f;//shortest time between turn on/off pause menu
	const float rotateGestCD = 0.3f;
	bool echoPlayedThisTouch;//echo will only play once duriing one touch, so if you hold your finger on the screen, echo will not repeat
	bool menuUpdatedThisTouch;
	bool isSwipe = false;
	public bool hasrotated = false;
	float echoTapTime;
	float rotateGestStartTime;
	float multiTapStartTime;
	float menuTapTime;
	Vector2 swipeStartPlace = new Vector2();
	Vector2 firstSwipePos = new Vector2();
	Vector2 VecStart = new Vector2();
	Vector2 VecEnd = new Vector2();
	List<Touch> touches;

	void Update ()
	{
		play_audio ();
		//UnityEngine.Debug.DrawLine (transform.position, transform.position+get_player_dir("FRONT"), Color.green);
		//UnityEngine.Debug.DrawLine (transform.position, transform.position+get_player_dir("LEFT"), Color.yellow);
		//If it's not the player's turn, exit the function.
		if (!GameManager.instance.playersTurn)
			return;

		if (!localRecordWritten) {
			//update stats
			GameManager.instance.boardScript.local_stats[curLevel] += 1;
			GameManager.instance.boardScript.write_local_stats ();
			localRecordWritten = true;
		}

		Vector3 dir = Vector3.zero;

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		if (Input.GetKeyUp (KeyCode.RightArrow)) {
			if(!want_exit){
				dir = -transform.up;
				SoundManager.instance.PlaySingle (swipeRight);
			}else{
				GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
				Destroy (GameObject.Find ("GameManager"));
				SceneManager.LoadScene ("Main");
			}
		} else if (Input.GetKeyUp (KeyCode.LeftArrow)) {
			if (!want_exit) {
				dir = get_player_dir ("LEFT");
				SoundManager.instance.PlaySingle (swipeLeft);
			} else {
				//SceneManager.UnloadScene("Main");
				Destroy (GameObject.Find ("GameManager"));
				SceneManager.LoadScene ("Title_Screen");
			}
		} else if (Input.GetKeyUp (KeyCode.UpArrow)) {
			dir = transform.right;
			SoundManager.instance.PlaySingle (swipeAhead);
		} else if (Input.GetKeyUp (KeyCode.DownArrow)) {
			dir = -transform.right;
			SoundManager.instance.PlaySingle (swipeAhead);
		}

		if (Input.GetKeyUp ("f")) {
			GameManager.instance.boardScript.gamerecord += "E{";
			PlayEcho ();
			GameManager.instance.boardScript.gamerecord += lastEcho;
			GameManager.instance.boardScript.gamerecord += "}";
		} else if (Input.GetKeyUp ("e")) {
			if (!want_exit) {
				GameManager.instance.boardScript.gamerecord += "X";
				attemptExitFromLevel ();
			} else
				want_exit = false;
		} else if (Input.GetKeyUp ("r")) {
			want_exit = true;
			reset_audio = true;
		}

		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

		float ECHO_TOUCH_TIME = 0.15f;
		float TOUCH_TIME = 0.02f;
		float MENU_TOUCH_TIME = 1.5f;
		minSwipeDist = Screen.width*0.01f;
		//Check if Input has registered more than zero touches
		int numTouches = Input.touchCount;

		Touch myTouch;
		Vector2 touchEndpos = new Vector2();
		BoardManager.Direction swp_dir = BoardManager.Direction.OTHER;
		bool isRotation = false;

		//update all timers
		//update TouchTapCount part 1
		if( (Time.time - multiTapStartTime) >= multiTapCD ){
			multiTapStartTime = Time.time;
			TouchTapCount = 0;
		}

		debug_text.text = "numTOuches: " + numTouches.ToString() + "\n"
						+ "PauseMenuOn: " + at_pause_menu.ToString() + "\n"
						+ "Tap Count: " + TouchTapCount.ToString() + "\n";

		//collect raw data from the device
		if (numTouches > 0) {

			//Store the first touch detected.
			myTouch = Input.touches[0];
			//if(touches.Contains(myTouch)){
			//}
			touchEndpos = myTouch.position;

			debug_text.text = "numTOuches: " + numTouches.ToString() + "\n"
							+ "PauseMenuOn: " + at_pause_menu.ToString() + "\n"
							+ "Tap Count: " + TouchTapCount.ToString() + "\n";

			if((numTouches == 2) && numTouches != numTouchlastframe){
				VecStart = Input.touches[0].position - Input.touches[1].position;
				menuUpdatedThisTouch = false;
			}

			//Check if the phase of that touch equals Began
			if (myTouch.phase == TouchPhase.Began){
				hasrotated = false;
				swipeStartPlace = myTouch.position;
				echoTapTime = Time.time;
				if(numTouches == 2){
					VecStart = Input.touches[0].position - Input.touches[1].position;
				}
				//If so, set touchOrigin to the position of that touch
				touchOrigin = myTouch.position;
				touchTime = Time.time;
				swp_lock = true;
				//update flags
				echoPlayedThisTouch = false;
				menuUpdatedThisTouch = false;
			} else if ((myTouch.phase == TouchPhase.Ended) && swp_lock){//deals with swipe and multiple taps
				//Set touchEnd to equal the position of this touch
				hasrotated = false;
				echoTapTime = Time.time;
				touchEndpos = myTouch.position;
				float x = touchEndpos.x - touchOrigin.x;
				float y = touchEndpos.y - touchOrigin.y;
				/*
				if(numTouches == 2){//detect a rotate
					isRotation = true;
					//Vector2 firstfingerPos = Input.touches[1].position;
					//VecStart = swipeStartPlace - firstfingerPos;
					VecEnd = Input.touches[0].position - Input.touches[1].position;
					//UnityEngine.Debug.DrawLine(new Vector3(firstfingerPos.x, firstfingerPos.y, 0f), new Vector3(swipeStartPlace.x, swipeStartPlace.y, 0f));
					//UnityEngine.Debug.DrawLine(new Vector3(firstfingerPos.x, firstfingerPos.y, 0f), new Vector3(touchEndpos.x, touchEndpos.y, 0f));
					Vector3 cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized);
					float crossPz = cross.z;
					if( (crossPz >= 0)&&(Mathf.Abs(crossPz) >= Screen.height*0.001f ) )//left
						swp_dir = BoardManager.Direction.LEFT;
					else if( (crossPz < 0)&&(Mathf.Abs(crossPz) >= Screen.width*0.0005f) )//right
						swp_dir = BoardManager.Direction.RIGHT;

					//if ( (firstfingerPos - firstSwipePos).magnitude >= minSwipeDist){//right & left
					//	print("gulululululululu");
					//	swp_dir = BoardManager.Direction.OTHER;
					//}

					//debug_text.text = "numTOuches: " + numTouches.ToString() + "\n";
					//debug_text.text += "VecStart: (" + VecStart.x + " ," + VecStart.y + ")\n";
					//debug_text.text += "VecEnd: (" + VecEnd.x + " ," + VecEnd.y + ")\n";
					//debug_text.text += "Cross: (" + cross.x + " ," + cross.y + " ," + cross.z + ")\n";
					//debug_text.text += x.ToString() + "\n";
					//debug_text.text += (minSwipeDist*0.05f).ToString() + "\n";
					//if(swp_dir == BoardManager.Direction.LEFT)
					//	debug_text.text += "LEFT";
					//else if(swp_dir == BoardManager.Direction.LEFT)
					//	debug_text.text += "RIGHT";
					//else
					//	debug_text.text += "OTHER";
					print(crossPz);
					print(VecEnd);
					print(VecStart);
					VecEnd = VecStart;//to "reset" the input
				}
				*/
				if (Mathf.Abs(x) > Mathf.Abs(y) && Mathf.Abs(x) >= minSwipeDist){//right & left
					if (x > 0)//right
						swp_dir = BoardManager.Direction.RIGHT;
					else//left
						swp_dir = BoardManager.Direction.LEFT;
				} else if (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) {//up & down
					if (y > 0)//up/front
						swp_dir = BoardManager.Direction.FRONT;
					else//down/back
						swp_dir = BoardManager.Direction.BACK;
					//Increment step count
					numSteps++;
				}

				//update TouchTapCount part 2
				if( (Time.time - multiTapStartTime) < multiTapCD ){
					TouchTapCount += myTouch.tapCount;
				}

				swp_lock = false;//flip the lock, until we find another TouchPhase.Began
			}else if( (numTouches == 2) && (!at_pause_menu) && (!hasrotated) ){
				if(numTouches == 2){//detect a rotate
					//enable this part for rotate turning control
					VecEnd = Input.touches[0].position - Input.touches[1].position;
					Vector3 cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized);
					float crossPz = cross.z;
					if( (crossPz >= 0)&&(Mathf.Abs(crossPz) >= Screen.height*0.00015f ) ){//left
						isRotation = true;
						hasrotated = true;
						if(Time.time - rotateGestStartTime >= rotateGestCD){
							rotateGestStartTime = Time.time;
							swp_dir = BoardManager.Direction.LEFT;
						}
					}else if( (crossPz < 0)&&(Mathf.Abs(crossPz) >= Screen.height*0.00015f) ){//right
						isRotation = true;
						hasrotated = true;
						if(Time.time - rotateGestStartTime >= rotateGestCD){
							rotateGestStartTime = Time.time;
							swp_dir = BoardManager.Direction.RIGHT;
						}
					}
					//enable this part for rotate turning control END
					//print(crossPz);
					//print(VecEnd);
					//print(VecStart);
					//VecEnd = VecStart;//to "reset" the input
				}
			}
			numTouchlastframe = numTouches;
		} else{
			numTouchlastframe = 0;
		}

		float touchx = touchEndpos.x - touchOrigin.x;
		float touchy = touchEndpos.y - touchOrigin.y;
		if (Mathf.Abs(touchx) >= minSwipeDist){//right & left
			isSwipe = true;
		} else if (Mathf.Abs(touchy) >= minSwipeDist) {//up & down
			isSwipe = true;
		} else
			isSwipe = false;

		//process the data
		if( (numTouches == touch_exit)&&(TouchTapCount >= tap_exit)&&(swp_dir == BoardManager.Direction.OTHER) ){//exit
			GameManager.instance.boardScript.gamerecord += "X";
			attemptExitFromLevel();
		}else if( (numTouches == touch_simple) ){//turn, get echo, etc.
			if(!at_pause_menu){
				if(swp_dir == BoardManager.Direction.FRONT){
					dir = get_player_dir("FRONT");
					SoundManager.instance.PlaySingle(swipeAhead);
					debug_text.text += "MOVE FORWARD";
				}
				//enable this part for swipe control turning
				/*
				else if(swp_dir == BoardManager.Direction.LEFT){
					dir = get_player_dir("LEFT");
					SoundManager.instance.PlaySingle(swipeLeft);
					debug_text.text += "TURN LEFT";
				}else if(swp_dir == BoardManager.Direction.RIGHT){
					dir = get_player_dir("RIGHT");
					SoundManager.instance.PlaySingle(swipeRight);
					debug_text.text += "TURN RIGHT";
				}*/
				//enable this part for swipe control turning END
				else if(swp_dir == BoardManager.Direction.OTHER){//play echo
					float x = touchEndpos.x - touchOrigin.x;
					float y = touchEndpos.y - touchOrigin.y;
					bool final_check_flag = true;
					if ((Mathf.Abs(x) > Mathf.Abs(y) && Mathf.Abs(x) >= minSwipeDist) || (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) || (numTouches>1) || (isRotation)){
						final_check_flag = false;
						isRotation = false;
					}
					
					if((Mathf.Abs(Time.time - touchTime) > ECHO_TOUCH_TIME)&&final_check_flag&&(!at_pause_menu)&&(!menuUpdatedThisTouch)&&(!isRotation)&&(!hasrotated)&&(!isSwipe)){
						//check echo timer
						if(Time.time - echoTapTime >= echoCD){
							echoTapTime = Time.time;
							if(!echoPlayedThisTouch){
								echoPlayedThisTouch = true;
								GameManager.instance.boardScript.gamerecord += "E{";
								PlayEcho();
								GameManager.instance.boardScript.gamerecord += lastEcho;
								GameManager.instance.boardScript.gamerecord += "}";
								debug_text.text += "PLAY ECHO";
							}
						}
					}
				}
			}
			else{//at the pause menu
				if(swp_dir == BoardManager.Direction.BACK){//turn on/of black screen
					if(GameManager.levelImageActive)
						GameManager.instance.HideLevelImage();
					else
						GameManager.instance.UnHideLevelImage();
					at_pause_menu = false;
					SoundManager.instance.PlayVoice(menuOff, true);//shoule have another set of sound effect
					debug_text.text += "BLACKEN SCREEN";
				}else if(swp_dir == BoardManager.Direction.LEFT){//restart level
					SoundManager.instance.playcrash(inputSFX);
					//GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
					SoundManager.instance.PlayVoice(menuOff, true);//shoule have another set of sound effect
					Destroy(GameObject.Find("GameManager"));
					SceneManager.LoadScene("Main");
				}else if(swp_dir == BoardManager.Direction.RIGHT){//quit to main menu
					SoundManager.instance.playcrash(inputSFX);
					Destroy(GameObject.Find("GameManager"));
					SceneManager.LoadScene("Title_Screen");
				}else if(swp_dir == BoardManager.Direction.FRONT){//repeat audio (duplicate)
					getHint ();
					debug_text.text += "GET HINT";
				}
			}
		}else if( (numTouches == touch_audio)&&(swp_dir != BoardManager.Direction.OTHER) ){//skip/repeat sudio
			if((!at_pause_menu) && (isRotation)){
				isRotation = false;
				if(swp_dir == BoardManager.Direction.LEFT){
					dir = get_player_dir("LEFT");
					SoundManager.instance.PlaySingle(swipeLeft);
					debug_text.text += "TURN LEFT";
				}else if(swp_dir == BoardManager.Direction.RIGHT){
					dir = get_player_dir("RIGHT");
					SoundManager.instance.PlaySingle(swipeRight);
					debug_text.text += "TURN RIGHT";
				}
			}
			else if((!at_pause_menu) && (!isRotation)){
				//if(swp_dir == BoardManager.Direction.LEFT){//repeat instruction
				//	GameManager.instance.boardScript.repeat_latest_instruction();
				//	debug_text.text += "REPEAT AUDIO";
				//}else if(swp_dir == BoardManager.Direction.RIGHT){//skip instruction
				//	GameManager.instance.boardScript.skip_instruction();
				//	debug_text.text += "SKIP AUDIO";
				//}
			}
		}else if( (numTouches == touch_menu)&&(Mathf.Abs(Time.time - touchTime) >= MENU_TOUCH_TIME)&&(!menuUpdatedThisTouch)&&(!isRotation)&&(!hasrotated) ){
			if((Time.time - menuTapTime >= menuUpdateCD)&&(!isRotation)){
				SoundManager.instance.playcrash(inputSFX);
				if(!at_pause_menu){//turn on/off pause menu
					at_pause_menu = true;
					SoundManager.instance.PlayVoice(menuOn, true);
				}else{
					at_pause_menu = false;
					SoundManager.instance.PlayVoice(menuOff, true);
				}

				debug_text.text += "UPDATE MENU";
				menuTapTime= Time.time;
				menuUpdatedThisTouch = true;
			}
		}

		#endif //End of mobile platform dependendent compilation section started above with #elif
		calculateMove (dir);
	}

	//Returns a description of the location of the crash (for analysis)
	//Currently, the ouput is from the following list of options
	//["End of the Corridor", "Intersection of 2 Corridors", "Start of the Corridor",
	//"Middle of the Corridor", "Towards End of the Corridor", "Towards Start of the Corridor"
	//"Crashed while on the Exit Sign"];
	//Currently not returning the Towards Start/End descriptions due to only having 7 discrete
	//movements in each x/y direction. May be relevant in the future.
	private string getCrashDescription (int xLoc, int yLoc)
	{
		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		List<Vector3> positions = new List<Vector3> ();

		//Go through all the walls
		foreach (var wall in walls) {
			positions.Add (new Vector3 (wall.transform.position.x, wall.transform.position.y, 0f));
		}

		float distXUp = 0;
		float distXDown = 0;
		float distYUp = 0;
		float distYDown = 0;
		float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;
		float threshhold = 0.01f;

		while (true) {
			distXUp = distXUp + 1 * scale;
			Vector3 currPos = new Vector3 (xLoc + distXUp, yLoc, 0f);
			for (int j = 0; j < positions.Count; ++j) {
				if ((positions [j] - currPos).magnitude <= threshhold) {
					break;
				}
			}
		}
		while (true) {
			distXDown = distXDown + 1 * scale;
			Vector3 currPos = new Vector3 (xLoc - distXDown, yLoc, 0f);
			for (int j = 0; j < positions.Count; ++j) {
				if ((positions [j] - currPos).magnitude <= threshhold) {
					break;
				}
			}
		}
		while (true) {
			distYUp = distYUp + 1 * scale;
			Vector3 currPos = new Vector3 (xLoc, yLoc + distYUp, 0f);
			for (int j = 0; j < positions.Count; ++j) {
				if ((positions [j] - currPos).magnitude <= threshhold) {
					break;
				}
			}
		}
		while (true) {
			distYDown = distYDown + 1 * scale;
			Vector3 currPos = new Vector3 (xLoc, yLoc - distYDown, 0f);
			for (int j = 0; j < positions.Count; ++j) {
				if ((positions [j] - currPos).magnitude <= threshhold) {
					break;
				}
			}
		}

		//positions.Contains (xLoc, yLoc);

		UnityEngine.Debug.Log ("Number of walls detected");
		UnityEngine.Debug.Log (walls.Length);

		UnityEngine.Debug.Log ("Current Position of Player");
		UnityEngine.Debug.Log (xLoc);
		UnityEngine.Debug.Log (yLoc);

		UnityEngine.Debug.Log ("Distances to walls in all directions");
		UnityEngine.Debug.Log (distXUp);
		UnityEngine.Debug.Log (distXDown);
		UnityEngine.Debug.Log (distYUp);
		UnityEngine.Debug.Log (distYDown);

		//All the crash location options
		//string[] locs = ["End of the Corridor", "Intersection of 2 Corridors", "Start of the Corridor", "Middle of the Corridor", "Towards End of the Corridor", "Towards Start of the Corridor"];

		//If Crash happened while on the Exit Sign
		GameObject exitSign = GameObject.FindGameObjectWithTag ("Exit");
		if ((xLoc == (int)exitSign.transform.position.x) & (yLoc == (int)exitSign.transform.position.y)) {
			return "Crashed while on the Exit Sign";
		}
		//TODO(agotsis/wenyuw1) This hardcoding needs to go away. Mainly here to test the database.
		//For the x direction
		if ((distXUp == 7) & (distXDown == 1) & (distYUp == 1) & (distYDown == 1)) {
			return "Start of the Corridor";
		}
		if ((distXUp == 4) & (distXDown == 4) & (distYUp == 1) & (distYDown == 1)) {
			return "Middle of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 7) & (distYUp == 1) & (distYDown == 1)) {
			return "End of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 8) & (distYUp == 8) & (distYDown == 1)) {
			return "Intersection of 2 Corridors";
		}
		//For the y direction
		if ((distXUp == 1) & (distXDown == 1) & (distYUp == 7) & (distYDown == 2)) {
			return "Start of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp == 4) & (distYDown == 5)) {
			return "Middle of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp == 5) & (distYDown == 4)) {
			return "Middle of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp == 1) & (distYDown == 8)) {
			return "End of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp > distYDown)) {
			return "Towards Start of the Corridor";
		}
		if ((distYUp == 1) & (distYDown == 1) & (distXUp > distXDown)) {
			return "Towards Start of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp < distYDown)) {
			return "Towards End of the Corridor";
		}
		if ((distYUp == 1) & (distYDown == 1) & (distXUp < distXDown)) {
			return "Towards End of the Corridor";
		}

		return "Error";
	}

	protected override void OnCantMove <T> (T component)
	{
		//Set hitWall to equal the component passed in as a parameter.
		Wall hitWall = component as Wall;
		//if(!SoundManager.instance.isBusy())
		SoundManager.instance.playcrash (wallHit);
	}

	protected override void OnMove ()
	{
	}

	private void OnTriggerEnter2D (Collider2D other)
	{
	}

	private void OnDisable ()
	{
		//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
		//int nextLevel = curLevel + 1;
		//GameManager.instance.level = nextLevel;
	}

	//Restart reloads the scene when called.
	private void Restart ()
	{
		//Load the last scene loaded, in this case Main, the only scene in the game.
		SceneManager.LoadScene("Main");
		restarted = false;
	}
}
