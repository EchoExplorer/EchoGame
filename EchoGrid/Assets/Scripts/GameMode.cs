using UnityEngine;
using System.Collections;

public class GameMode : MonoBehaviour {
	public enum Game_Mode{
		TUTORIAL,
		MAIN,
		CONTINUE
	}

	public static GameMode instance = null;		//Allows other scripts to call functions from SoundManager.			
	public Game_Mode gamemode;

	void Awake ()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	}

	public void init(){
		gamemode = Game_Mode.MAIN;
	}

	public Game_Mode get_mode(){
		return gamemode;
	}

	public void set_mode(Game_Mode gm){
		gamemode = gm;
	}
}
