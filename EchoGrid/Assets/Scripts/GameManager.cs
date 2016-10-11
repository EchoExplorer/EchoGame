using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;		
using UnityEngine.UI;	
using System.Text;
using System.IO;

public class GameManager : MonoBehaviour {

	public BoardManager boardScript;

	public static GameManager instance = null;
	public bool level_already_loaded = false;
	public static bool levelImageActive = true;
	[HideInInspector] public bool playersTurn = true;

	public float levelStartDelay = 2f;	
	public int level = 0;
	public Text levelText;
	public GameObject levelImage;
	private bool doingSetup = true;
	public DbAccess db;

	void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
		
		DontDestroyOnLoad (gameObject);

		level_already_loaded = false;
		boardScript = GetComponent<BoardManager> ();
		LoadSaved ();
		/*
		if(GameMode.instance.get_mode () == GameMode.Game_Mode.MAIN)//MAIN
			level = 12;
		else if (GameMode.instance.get_mode () == GameMode.Game_Mode.TUTORIAL)//TUTORIAL
			level = 1;
		else if (GameMode.instance.get_mode () == GameMode.Game_Mode.CONTINUE)//CONTINUE
			LoadSaved();
		*/
	}

	bool LoadSaved(){
		string filename = "";
		string[] svdata_split;

		if(GameMode.instance.get_mode () != GameMode.Game_Mode.TUTORIAL)
			filename = Application.persistentDataPath + "echosaved";
		else//load specific save for tutorial
			filename = Application.persistentDataPath + "echosaved_tutorial";

		if (System.IO.File.Exists (filename)) {
			svdata_split = System.IO.File.ReadAllLines (filename);
		} else {
			if(GameMode.instance.get_mode () != GameMode.Game_Mode.TUTORIAL)
				level = 12;
			else//load specific save for tutorial
				level = 1;

			return false;
		}
			
		//read existing data
		foreach (string line in svdata_split) {
			int saved_level = Int32.Parse (line);
			if (saved_level == 0) {
				if (GameMode.instance.get_mode () != GameMode.Game_Mode.TUTORIAL)
					level = 12;
				else//load specific save for tutorial
					level = 1;
			} else {
				level = saved_level;
				if (GameMode.instance.get_mode () != GameMode.Game_Mode.TUTORIAL) {
					if ((level < 12) || (level > 150))
						level = 12;
				}else//load specific save for tutorial
					if ((level < 1) || (level > 11))
						level = 1;
			}
		}
		return true;
	}
	
	//Initializes the game for each level.
	//TODO(agotsis) Analyze database
	void InitGame(){
		#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			Screen.orientation = ScreenOrientation.Landscape;
		#endif

		//Setup database for the first time
		db = new DbAccess("data source=LocalDataBase.db");
		//db.CreateTable("PlayerInfo",new string[]{"id","name","high_score"}, new string[]{"integer","text","integer"});
		//db.CreateTable("AudioFiles",new string[]{"id","echo name","file_path", "game_level"}, new string[]{"integer","text","text", "integer"});
		//db.CloseSqlConnection();

		doingSetup = true;
		levelImage = UICanvas.instance.transform.FindChild ("LevelImage").gameObject; //GameObject.Find("LevelImage");
		levelText = levelImage.transform.FindChild("LevelText").gameObject.GetComponent<Text>();
		//Set the text of levelText to the string "Day" and append the current level number.;
		
		//Set levelImage to block player's view of the game board during setup.
		levelImage.SetActive(true);
		//levelImageActive = true;
		
		//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
		Invoke("StartGame", levelStartDelay);

		boardScript.max_total_level = boardScript.get_level_count ("GameData/levels");
		//Call the SetupScene function of the BoardManager script, pass it current level number.
		if (GameMode.instance.get_mode () == GameMode.Game_Mode.MAIN) {//MAIN
			boardScript.max_level = boardScript.get_level_count ("GameData/levels");
			boardScript.min_level = 12;
		} else if (GameMode.instance.get_mode () == GameMode.Game_Mode.TUTORIAL) {//TUTORIAL
			boardScript.max_level = 11;
			boardScript.min_level = 1;
		} else if (GameMode.instance.get_mode () == GameMode.Game_Mode.CONTINUE) {
			boardScript.max_level = boardScript.get_level_count ("GameData/levels");
			boardScript.min_level = 12;
		}
			
		levelText.text = "Loading level " + level.ToString();
		LoadSaved ();
		boardScript.SetupScene (level);

		checkEchoFiles ();
	}

	//Hides black image used between levels
	void StartGame(){
		//Disable the levelImage gameObject.
		if (!levelImageActive)
			HideLevelImage ();
		else
			UnHideLevelImage ();
		//Set doingSetup to false allowing player to move again.
		doingSetup = false;
		playersTurn = true;
		SoundManager.instance.PlayVoice ((AudioClip)Resources.Load("instructions/Level Start"), true);
	}

	public void UnHideLevelImage(){
		levelText.text = "level " + level.ToString () + "\n";
		levelText.text += "Game In Progress" + "\n";
		levelText.text += "Hold two fingers" + "\n";
		levelText.text += "to open menu";
		levelImage.SetActive(true);
		levelImageActive = true;
	}

	public void HideLevelImage(){
		//Disable the levelImage gameObject.
		levelImage.SetActive(false);
		levelImageActive = false;
	}

	//This is called each time a scene is loaded.
	void OnLevelWasLoaded(int index){
		//Call InitGame to initialize our level.
		//if (!level_already_loaded) {
			levelImage = UICanvas.instance.transform.FindChild("LevelImage").gameObject;
			levelText = levelImage.transform.FindChild ("LevelText").gameObject.GetComponent<Text> ();
			InitGame ();
			level_already_loaded = true;
		//}
	}

	public void checkEchoFiles(){
		String prefix = "C21-0"; //change this prefix when you change the echo files
		String filename, filename2, filename3;
		/*
		float step = 1.5f;
		float max_dist = 0.75f + 1.5f * 8;
		string[] types = new string[]{"D", "ER", "EL", "US",};
		for (int f_dist = 0.75f; f_dist < max_dist; f_dist += step) {
			for (int b_dist = 0.75f; b_dist < max_dist - f_dist; b_dist += step) {
				for (int l_dist = 0.75f; l_dist < max_dist; l_dist += step) {
					for (int r_dist = 0.75f; r_dist < max_dist - l_dist; r_dist += step) {
						
					}
				}
			}
		}

		filename = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, 
			data.frontDist, front_type, data.backDist, "D",
			data.leftDist, left_type, data.rightDist, right_type);
		filename2 = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, 
			data.frontDist, front_type, data.backDist, "na",
			data.leftDist, left_type, data.rightDist, right_type);
		filename3 = String.Format("{0}_F-{1:F2}-{2}_B-{3:F2}-{4}_L-{5:F2}-{6}_R-{7:F2}-{8}", prefix, 
			data.frontDist, front_type, data.backDist, "US",
			data.leftDist, left_type, data.rightDist, right_type);

		//try all three files
		AudioClip echo = Resources.Load ("echoes/" + filename) as AudioClip;
		lastEcho = filename;

		if (echo == null) {
			echo = Resources.Load ("echoes/" + filename2) as AudioClip;
			lastEcho = filename2;
		}
		if (echo == null) {
			echo = Resources.Load ("echoes/" + filename3) as AudioClip;
			lastEcho = filename3;
		}
		SoundManager.instance.PlayEcho (echo);

		UnityEngine.Debug.Log (lastEcho);
		UnityEngine.Debug.Log (data.all_jun_to_string ());
		if (echo == null)
			UnityEngine.Debug.Log ("Echo not found");
		*/
	}

	public void GameOver() {
		enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		#endif

		if (playersTurn || doingSetup)
			return;
	}
}
