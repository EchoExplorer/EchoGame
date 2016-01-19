using UnityEngine;
using System.Collections;
using System.Collections.Generic;		
using UnityEngine.UI;					

public class GameManager : MonoBehaviour {

	public BoardManager boardScript;

	public static GameManager instance = null;
	[HideInInspector] public bool playersTurn = true;

	public float levelStartDelay = 2f;	
	public int level = 0;
	public Text levelText;
	public GameObject levelImage;
	private bool doingSetup = true;

	void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
		DontDestroyOnLoad (gameObject);
		boardScript = GetComponent<BoardManager> ();
		InitGame ();
	}
	
	//Initializes the game for each level.
	void InitGame()
	{
		Screen.orientation = ScreenOrientation.Landscape;
		doingSetup = true;
		levelImage = GameObject.Find("LevelImage");
		levelText = GameObject.Find("LevelText").GetComponent<Text>();
		//Set the text of levelText to the string "Day" and append the current level number.;
		levelText.text = "Loading level";
		
		//Set levelImage to active blocking player's view of the game board during setup.
		levelImage.SetActive(true);
		
		//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
		Invoke("HideLevelImage", levelStartDelay);
		
		//Call the SetupScene function of the BoardManager script, pass it current level number.
		boardScript.SetupScene(level);
		
	}

	//Hides black image used between levels
	void HideLevelImage()
	{
		//Disable the levelImage gameObject.
		levelImage.SetActive(false);
		
		//Set doingSetup to false allowing player to move again.
		doingSetup = false;
	}

	//This is called each time a scene is loaded.
	void OnLevelWasLoaded(int index)
	{

		//Call InitGame to initialize our level.
		InitGame();
	}


	public void GameOver() 
	{
		enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (playersTurn || doingSetup) {
			return;
		}
	
	}
}
