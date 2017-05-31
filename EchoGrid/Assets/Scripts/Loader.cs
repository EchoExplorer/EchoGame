using UnityEngine;
using System.Collections;

/// <summary>
/// A simple class to load the game and sound managers.
/// </summary>
public class Loader : MonoBehaviour {

	public GameObject gameManager;
	public GameObject soundManager;	

	// Use this for initialization
	void Awake () {
		if (GameManager.instance == null)
			Instantiate (gameManager);
	
		if (SoundManager.instance == null)
			
			//Instantiate SoundManager prefab
			Instantiate (soundManager);
	}

}
