using UnityEngine;
using System.Collections;

public class GameMode : MonoBehaviour {
	public enum Game_Mode{
		TUTORIAL,
		MAIN
	}

	public static Game_Mode gamemode;

	public void init(){
		gamemode = Game_Mode.MAIN;
		DontDestroyOnLoad (transform.gameObject);
	}

	public Game_Mode get_mode(){
		return gamemode;
	}
}
