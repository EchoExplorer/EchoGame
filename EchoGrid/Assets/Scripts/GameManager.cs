using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public BoardManager boardScript;

	public static GameManager instance = null;
	[HideInInspector] public bool playersTurn = true;

	private int level = 3;

	void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
		boardScript = GetComponent<BoardManager> ();
		InitGame ();
	}

	void InitGame() 
	{
		boardScript.SetupScene (level);
	}

	// Use this for initialization
	void Start () {
	
	}

	public void GameOver() 
	{
		enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
