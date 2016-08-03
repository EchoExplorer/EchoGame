using UnityEngine;
using System.Collections;

public class UICanvas : MonoBehaviour {

	public static UICanvas instance; 

	// Use this for initialization
	void Awake(){
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	}

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnLevelWasLoaded(int index){
		//Call InitGame to initialize our level.
		//transform.FindChild("LevelImage").gameObject.SetActive(true);
	}
}
