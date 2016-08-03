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
			//if not, set it to this.
			instance = this;
		//If instance already exists:
		else if (instance != this)
			//Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
			Destroy (gameObject);

		//Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
		DontDestroyOnLoad (gameObject);
	}

	public void init(){
		gamemode = Game_Mode.MAIN;
	}

	public Game_Mode get_mode(){
		return gamemode;
	}
}
