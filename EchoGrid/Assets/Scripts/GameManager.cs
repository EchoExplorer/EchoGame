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
	}

	bool LoadSaved(){
		string filename = "";
		string[] svdata_split;

		//choose save for tutorial and normal game
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
		int saved_level = Int32.Parse (svdata_split[0]);
		GameMode.Game_Mode gm = GameMode.instance.get_mode ();
		//assign level from file
		level = saved_level;
		switch (gm) {
		case GameMode.Game_Mode.MAIN:
			level = 12;
			break;
		case GameMode.Game_Mode.TUTORIAL:
			if ((level == 0) || (level < 1) || (level > 11))
				level = 1;
			break;
		case GameMode.Game_Mode.CONTINUE:
			if ((level == 0) || (level < 12) || (level > 150))
				level = 12;
			break;
		default:
			level = 12;
			return false;
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
		db = new DbAccess("Data Source=LocalDataBase.db");
		//db.CreateTable("PlayerInfo",new string[]{"id","name","high_score"}, new string[]{"integer","text","integer"});
		//db.CreateTable("AudioFiles",new string[]{"id","echo name","file_path", "game_level"}, new string[]{"integer","text","text", "integer"});
		//db.CloseSqlConnection();

		doingSetup = true;
		levelImage = UICanvas.instance.transform.FindChild ("LevelImage").gameObject; //GameObject.Find("LevelImage");
		levelText = levelImage.transform.FindChild("LevelText").gameObject.GetComponent<Text>();
		//Set the text of levelText to the string "Day" and append the current level number.;
		
		//Set levelImage to block player's view of the game board during setup.
		levelImage.SetActive(true);
		
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
