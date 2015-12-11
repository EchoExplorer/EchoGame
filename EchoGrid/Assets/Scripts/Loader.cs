using UnityEngine;
using System.Collections;

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
