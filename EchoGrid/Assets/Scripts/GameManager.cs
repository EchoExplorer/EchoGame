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
	public static bool level_already_loaded = false;
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

		if(GameMode.instance.get_mode () == GameMode.Game_Mode.MAIN)//MAIN
			level = 12;
		else if (GameMode.instance.get_mode () == GameMode.Game_Mode.TUTORIAL)//TUTORIAL
			level = 1;
		else if (GameMode.instance.get_mode () == GameMode.Game_Mode.CONTINUE)//CONTINUE
			LoadSaved();
	}

	bool LoadSaved(){
		string filename = Application.persistentDataPath + "echosaved";

		string[] svdata_split;
		if (System.IO.File.Exists (filename)) {
			svdata_split = System.IO.File.ReadAllLines (filename);
		} else {
			level = 12;
			return false;
		}
			

		foreach (string line in svdata_split) {
			int saved_level = Int32.Parse (line);
			if (saved_level == 0)
				level = 12;
			else
				level = saved_level;
		}

		return true;
	}
	
	//Initializes the game for each level.
	//TODO(agotsis) Analyze database
	void InitGame(){
		Screen.orientation = ScreenOrientation.Landscape;

		//Setup database for the first time
		db = new DbAccess("data source=LocalDataBase.db");
		//db.CreateTable("PlayerInfo",new string[]{"id","name","high_score"}, new string[]{"integer","text","integer"});
		//db.CreateTable("AudioFiles",new string[]{"id","echo name","file_path", "game_level"}, new string[]{"integer","text","text", "integer"});
		//db.CloseSqlConnection();

		doingSetup = true;
		levelImage = GameObject.Find("LevelImage");
		levelText = GameObject.Find("LevelText").GetComponent<Text>();
		//Set the text of levelText to the string "Day" and append the current level number.;
		levelText.text = "Loading level";
		
		//Set levelImage to block player's view of the game board during setup.
		levelImage.SetActive(true);
		
		//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
		Invoke("HideLevelImage", levelStartDelay);

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

		boardScript.SetupScene (level);
	}

	//Hides black image used between levels
	void HideLevelImage(){
		//Disable the levelImage gameObject.
		levelImage.SetActive(false);
		
		//Set doingSetup to false allowing player to move again.
		doingSetup = false;
	}

	//This is called each time a scene is loaded.
	void OnLevelWasLoaded(int index){
		//Call InitGame to initialize our level.
		if (!level_already_loaded) {
			InitGame ();
			level_already_loaded = true;
		}
	}


	public void GameOver() {
		enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (playersTurn || doingSetup)
			return;
	}
}
