﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using System.Collections.Generic;
using SimpleJSON;
using System.Security.Cryptography;
using System;
using System.Text;
using System.Diagnostics;

//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
public class Player : MovingObject {
	
	public float restartLevelDelay = 3.0f;		//Delay time in seconds to restart level.
	private Animator animator;					//Used to store a reference to the Player's animator component.
	private Vector2 touchOrigin = -Vector2.one;
	
	private float touchTime = 0f;
	private float minSwipeDist = 100f;
	bool restarted = false;
	private int curLevel;

	//TODO(agotsis/wenyuw1) This needs to be integrated with the local database so these are not hardcoded 
	public AudioClip echo1m;
	public AudioClip echo2m;
	public AudioClip echo3m;
	public AudioClip echo4m;
	public AudioClip echo5m;
	public AudioClip echo6m;
	public AudioClip echo7m;

	public AudioClip wallHit;
	public AudioClip winSound;
	public AudioClip walking;

	//TODO(agotsis/wenyuw1) This volume of these sounds may need to go down 
    public AudioClip swipeAhead;
	public AudioClip swipeRight;
	public AudioClip swipeLeft;

	//private SpriteRenderer spriteRenderer; 
	
	private int numCrashes; //Keep track of number of times user crashed into wall
	private int numSteps;   //Keep track of number of steps taken per level

	/*TODO(agotsis/wenyuw1) Similar to the comment above: this needs to be integrated with the local 
	 * database so these are not hardcoded */
	//Track number of times each echo was played
	private int numEcho1;
	private int numEcho2;
	private int numEcho3;
	private int numEcho4;
	private int numEcho5;
	private int numEcho6;
	private int numEcho7;

	//Track locations of the player's crashes
	private string crashLocs;

	//Keep track of time taken for the game level
	private long startTime;
	private Stopwatch stopWatch;
	
	//public bool soundPlaying = false;
	protected override void Start () {
		//Get a component reference to the Player's animator component
		animator = GetComponent<Animator>();
		curLevel = GameManager.instance.level;
		//spriteRenderer = GetComponent<SpriteRenderer>();
		
		//Initialize data collection variables
		numCrashes = 0;
		numSteps = 0;

		//TODO(agotsis/wenyuw1) Once the local database is integrated this hardcoding will go away. 
		numEcho1 = 0;
		numEcho2 = 0;
		numEcho3 = 0;
		numEcho4 = 0;
		numEcho5 = 0;
		numEcho6 = 0;
		numEcho7 = 0;

		//Initialize list of crash locations
		crashLocs = "";

		//Adjust player scale
		Vector3 new_scale = transform.localScale;
		new_scale *= (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;
		transform.localScale = new_scale;

		//Start the time for the game level
		stopWatch = new Stopwatch();
		stopWatch.Start ();
		
		base.Start ();
	}

	private void PlayEcho() {

		BoardManager.echoDistData data = 
			GameManager.instance.boardScript.getEchoDistData(transform.position, get_player_dir("FRONT"), get_player_dir("LEFT"));

		String filename;
		filename = "Front: " + data.front.ToString () + "; Back: " + data.back.ToString () +
			"; Left: " + data.left.ToString () + "; Right: " + data.right.ToString ();

		UnityEngine.Debug.Log (filename);
		UnityEngine.Debug.Log (data.all_jun_to_string());

		//TODO HJKJHJK

		/*
		switch (dist) 
		{
		case 0:
		case 1:
			SoundManager.instance.PlaySingle(echo1m);
			numEcho1++;
			UnityEngine.Debug.Log ("1m");
			break;
		case 2:
			SoundManager.instance.PlaySingle(echo2m);
			numEcho2++;
			UnityEngine.Debug.Log ("2m");
			break;
		case 3:
			SoundManager.instance.PlaySingle(echo3m);
			numEcho3++;
			UnityEngine.Debug.Log ("3m");
			break;
		case 4:
			SoundManager.instance.PlaySingle(echo4m);
			numEcho4++;
			UnityEngine.Debug.Log ("4m");
			break;
		case 5:
			SoundManager.instance.PlaySingle(echo5m);
			numEcho5++;
			UnityEngine.Debug.Log ("5m");
			break;
		case 6:
			SoundManager.instance.PlaySingle(echo6m);
			numEcho6++;
			UnityEngine.Debug.Log ("6m");
			break;
		case 7:
			SoundManager.instance.PlaySingle(echo7m);
			numEcho7++;
			UnityEngine.Debug.Log ("7m");
			break;
		default:
			SoundManager.instance.PlaySingle(echo7m);
			numEcho7++;
			break;
			
		}*/
	}

	//due to the chaotic coord system
	//return the relative direction
	Vector3 get_player_dir(string dir){
		if (dir == "FRONT")
			return transform.right.normalized;
		else if (dir == "BACK")
			return -transform.right.normalized;
		else if (dir == "LEFT")
			return transform.up.normalized;
		else if (dir == "RIGHT")
			return -transform.up.normalized;

		UnityEngine.Debug.Log ("INVALID direction string");
		return Vector3.zero;
	}

	//please call this function to rotate player
	//use this with get_player_dir("SOMETHING")
	void rotateplayer(Vector3 dir){
		if (dir == get_player_dir ("FRONT"))
			return;
		else if (dir == get_player_dir ("BACK"))
			return;
		else if (dir == get_player_dir("LEFT"))
			transform.Rotate (new Vector3(0,0,90));
		else if(dir == get_player_dir("RIGHT"))
			transform.Rotate (new Vector3(0,0,-90));
	}

	private void calculateMove(Vector3 dir) {
		if (dir.magnitude == 0)
			return;

		bool changedDir = false;
		//print (dir);

		if ((dir != get_player_dir("FRONT")) && (dir != get_player_dir("BACK"))) {
			changedDir = true;
			rotateplayer (dir);
		}
		
		dir.Normalize();

		if(!changedDir)
			AttemptMove<Wall> ((int)dir.x, (int)dir.y);
	}

	protected override bool AttemptMove <T> (int xDir, int yDir)
	{	
		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		bool canMove = base.AttemptMove <T> (xDir, yDir);

		//If player could not move to that location, play the crash sound
		if (!canMove) {
			SoundManager.instance.PlaySingle(wallHit);
			//Increment the crash count
			numCrashes++;
			//Decrement the step count (as no successful step was made)
			numSteps--;

			//Add the crash location details
			string loc = transform.position.x.ToString() + "," + transform.position.y.ToString();
			//TODO put those two lines back
			//string crashPos = getCrashDescription((int) transform.position.x, (int) transform.position.y);
			//loc = loc + "," + crashPos;
			if (crashLocs.Equals("")) {
				crashLocs = loc;
			}
			else {
				crashLocs = crashLocs + ";" + loc;
			}
		}

		//Hit allows us to reference the result of the Linecast done in Move.
		//RaycastHit2D hit;

		//GameManager.instance.playersTurn = false;
		return canMove;
	}
	
	private void attemptExitFromLevel() {
		GameObject exitSign = GameObject.FindGameObjectWithTag("Exit");
		Vector2 distFromExit = transform.position - exitSign.transform.position;
		if (Vector2.SqrMagnitude(distFromExit) < 0.25) {
			//Calculate time elapsed during the game level
			stopWatch.Stop();
			int timeElapsed = unchecked((int)(stopWatch.ElapsedMilliseconds/1000));

			//Calculate the points for the game level
			//Score based on: time taken, num crashes, steps taken, trying(num echoes played on same spot)
			//Finish in less than 15 seconds => full score
			//For every 10 seconds after 15 seconds, lose 100 points
			//For every crash, lose 150 points
			//For every step taken over the optimal steps, lose 50 points
			//Max score currently is 1500 points
			int score = 1500;
			if (timeElapsed > 15) {
				score = score - (((timeElapsed - 16)/10) + 1)*100;
			}
			if (numCrashes > 0) {
				score = score - numCrashes*150;
			}
			//Check if the score went below 0
			if (score < 0) {
				score = 0;
			}
			//TODO
			//if numSteps > numOptimalSteps, then adjust score
			//Calculate optimal steps by getting start position and end position
			//and calculate the number of steps 



			//Send the crash count data and level information to server
			//string dataEndpoint = "http://cmuecholocation.herokuapp.com/storeGameLevelData";
			string dataEndpoint = "http://128.237.139.120:8000/storeGameLevelData";
			
			WWWForm form = new WWWForm();
			form.AddField("userName", SystemInfo.deviceUniqueIdentifier);
			form.AddField("crashCount", numCrashes);
			form.AddField("stepCount", numSteps);
			form.AddField("currentLevel", curLevel);
			//Send the name of the echo files used in this level and the counts
			form.AddField("echoFileNames", getEchoNames());

			//Send the details of the crash locations
			form.AddField("crashLocations", crashLocs);
			form.AddField("timeElapsed", timeElapsed);

			form.AddField("score", score);
			
			//Start of the encryption data
			try {
				string testToEncrypt = Base64Encode("This is a test string");
				//initialze the byte arrays to the public key information.
				string publicKeyString = "iqKXThQvzLKgG0FQXuznGk4nEyFlE9VGmFIzkQyX9n3giHXJoqln4pZASPH3XnJX7ZOxmXXGskjrAYXLD2BZ8eZFkEmNj0GTC9kbDZzcjd+3Lc6P32J7MjfD7dIyPH8IUB9ELtL2MZ36kZrLrf3c2q2pQIl4s5k0Ro2F2aXWB+s=";
				byte[] publicKeyBytes = Convert.FromBase64String(publicKeyString);
				
				byte[] Exponent = {17};
				
				//Create a new instance of RSACryptoServiceProvider.
				RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
				
				//Create a new instance of RSAParameters.
				RSAParameters RSAKeyInfo = new RSAParameters();
				
				//Set RSAKeyInfo to the public key values. 
				RSAKeyInfo.Modulus = publicKeyBytes;
				RSAKeyInfo.Exponent = Exponent;
				
				//Import key parameters into RSA.
				RSA.ImportParameters(RSAKeyInfo);
				
				//Create a new instance of the RijndaelManaged class.
				RijndaelManaged RM = new RijndaelManaged();
				
				//Encrypt the symmetric key and IV.
				byte[] encryptedTestString = RSA.EncryptValue(Convert.FromBase64String(testToEncrypt));
				
				//Add the encrypted test string to the form
				form.AddField("testEncrypt", Convert.ToBase64String(encryptedTestString));
				
			}
			catch(CryptographicException e)
			{
				Console.WriteLine(e.Message);
				form.AddField("testEncrypt", e.Message);
			}
			
			WWW www = new WWW(dataEndpoint, form);
			StartCoroutine(WaitForRequest(www));
			
			//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
			restarted = true;
			Invoke ("Restart", restartLevelDelay);
			//Disable the player object since level is over.
			enabled = false;
			AudioSource.PlayClipAtPoint(winSound, transform.localPosition, 0.3f);
			
			//Reset the crash count
			numCrashes = 0;
		}
	}
	
	public static string Base64Encode(string plainText) {
		var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
		return System.Convert.ToBase64String(plainTextBytes);
	}
	
	//Creates a comma delimited string containing all the echo file names used in the level
	//and the corresponding number of times the echo was played
	private string getEchoNames() {
		//TODO(agotsis/wenyuw1) Once the local database is integrated, this hardcoding will go away. 
		string allNames = "";
		allNames = allNames + echo1m.name + ":" + numEcho1.ToString() + ",";
		allNames = allNames + echo2m.name + ":" + numEcho2.ToString() + ",";
		allNames = allNames + echo3m.name + ":" + numEcho3.ToString() + ",";
		allNames = allNames + echo4m.name + ":" + numEcho4.ToString() + ",";
		allNames = allNames + echo5m.name + ":" + numEcho5.ToString() + ",";
		allNames = allNames + echo6m.name + ":" + numEcho6.ToString() + ",";
		allNames = allNames + echo7m.name + ":" + numEcho7.ToString();
		
		return allNames;
	}
	
	
	//Makes HTTP requests and waits for response and checks for errors
	IEnumerator WaitForRequest(WWW www) {
		yield return www;
		
		//Check for errors 
		if (www.error == null) {
			JSONNode data = JSON.Parse(www.data);
			//Debug.Log("this is the parsed json data: " + data["testData"]);
			//Debug.Log(data["testData"]);
			UnityEngine.Debug.Log ("WWW.Ok! " + www.data);
		} else {
			UnityEngine.Debug.Log ("WWWError: " + www.error);
		}
	}
	
	private void Update () {
		//UnityEngine.Debug.DrawLine (transform.position, transform.position+get_player_dir("FRONT"), Color.green);
		//UnityEngine.Debug.DrawLine (transform.position, transform.position+get_player_dir("LEFT"), Color.yellow);
		//If it's not the player's turn, exit the function.
		if(!GameManager.instance.playersTurn) return;

		Vector3 dir = Vector3.zero;

		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER
		
		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			dir = -transform.up;
			SoundManager.instance.PlaySingle(swipeRight);
		} else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			dir = transform.up;
			SoundManager.instance.PlaySingle(swipeLeft);
		} else if (Input.GetKeyUp(KeyCode.UpArrow)) {
			dir = transform.right;
			SoundManager.instance.PlaySingle(swipeAhead);
		} else if (Input.GetKeyUp(KeyCode.DownArrow)) {
			dir = -transform.right;
			SoundManager.instance.PlaySingle(swipeAhead);
		}
		
		if (Input.GetKeyUp("f"))
			PlayEcho();
		else if (Input.GetKeyUp("e"))
			attemptExitFromLevel();
		
		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

		float TOUCH_TIME = 0.05f;

		//Check if Input has registered more than zero touches
		int numTouches = Input.touchCount;
		
		if (numTouches > 0) {
			//Store the first touch detected.
			Touch myTouch = Input.touches[0];

			//Check if the phase of that touch equals Began
			if (myTouch.phase == TouchPhase.Began){
				//If so, set touchOrigin to the position of that touch
				touchOrigin = myTouch.position;
				touchTime = Time.time;
			}
			
			//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
			else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
				//Set touchEnd to equal the position of this touch
				Vector2 touchEnd = myTouch.position;
				
				//Calculate the difference between the beginning and end of the touch on the x axis.
				float x = touchEnd.x - touchOrigin.x;
				
				//Calculate the difference between the beginning and end of the touch on the y axis.
				float y = touchEnd.y - touchOrigin.y;
				
				//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
				touchOrigin.x = -1;
				
				//Check if the difference along the x axis is greater than the difference along the y axis.
				if (Mathf.Abs(x) > Mathf.Abs(y) && Mathf.Abs(x) >= minSwipeDist)
				{
					//If x is greater than zero, set horizontal to 1, otherwise set it to -1
					if (x > 0) {
						dir = get_player_dir("RIGHT");
						SoundManager.instance.PlaySingle(swipeRight);
					} else {
						dir = get_player_dir("LEFT");
						SoundManager.instance.PlaySingle(swipeLeft);
					}
				} else if (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) {
					//If y is greater than zero, set vertical to 1, otherwise set it to -1
					if (y > 0) {
						dir = get_player_dir("FRONT");
						SoundManager.instance.PlaySingle(swipeAhead);
					} else {
						dir = get_player_dir("BACK");
						SoundManager.instance.PlaySingle(swipeAhead);
					}
					//Increment step count
					numSteps++;
				} else if (Mathf.Abs(Time.time - touchTime) > TOUCH_TIME) {
					if (numTouches == 2)
						attemptExitFromLevel();
					else
						PlayEcho(echoDist());
				}
			}
		}
		#endif //End of mobile platform dependendent compilation section started above with #elif
		calculateMove(dir);
	}


	//no longer used
	private int echoDist() {

		//Get all the walls on the grid
		/*
		GameObject[] wallsArray = GameObject.FindGameObjectsWithTag("Wall");
		List <Vector3> wallPositions = new List<Vector3>();
		foreach (GameObject wall in wallsArray) {
			wallPositions.Add(wall.transform.position);
		}
		int dist;
		int minDistance = 7;
		float threshhold = 0.01f;
		float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

		for (int i = 1; i < Utilities.MAZE_SIZE; i++) {
			Vector3 tPos = transform.position + get_player_dir ("FRONT") * i*scale;
			for (int j = 0; j < wallPositions.Count; ++j) {
				if ((wallPositions [j] - tPos).magnitude <= threshhold) {
					dist = (int)((tPos - transform.position).magnitude/scale);
					minDistance = Mathf.Min (minDistance, dist);
				}
			}
		}
		return minDistance;
		*/
		return 0;
	}

	//Returns a description of the location of the crash (for analysis)
	//Currently, the ouput is from the following list of options
	//["End of the Corridor", "Intersection of 2 Corridors", "Start of the Corridor",
	//"Middle of the Corridor", "Towards End of the Corridor", "Towards Start of the Corridor"
	//"Crashed while on the Exit Sign"];
	//Currently not returning the Towards Start/End descriptions due to only having 7 discrete
	//movements in each x/y direction. May be relevant in the future.
	private string getCrashDescription(int xLoc, int yLoc) {
		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		List<Vector3> positions = new List<Vector3>();

		//Go through all the walls
		foreach (var wall in walls) {
			positions.Add(new Vector3(wall.transform.position.x, wall.transform.position.y, 0f));
		}

		float distXUp = 0;
		float distXDown = 0;
		float distYUp = 0;
		float distYDown = 0;
		float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;
		float threshhold = 0.01f;

		while (true) {
			distXUp = distXUp + 1*scale;
			Vector3 currPos = new Vector3(xLoc + distXUp, yLoc, 0f);
			for (int j = 0; j < positions.Count; ++j) {
				if ((positions [j] - currPos).magnitude <= threshhold) {
					break;
				}
			}
		}
		while (true) {
			distXDown = distXDown + 1*scale;
			Vector3 currPos = new Vector3(xLoc - distXDown, yLoc, 0f);
			for (int j = 0; j < positions.Count; ++j) {
				if ((positions [j] - currPos).magnitude <= threshhold) {
					break;
				}
			}
		}
		while (true) {
			distYUp = distYUp + 1*scale;
			Vector3 currPos = new Vector3(xLoc, yLoc + distYUp, 0f);
			for (int j = 0; j < positions.Count; ++j) {
				if ((positions [j] - currPos).magnitude <= threshhold) {
					break;
				}
			}
		}
		while (true) {
			distYDown = distYDown + 1*scale;
			Vector3 currPos = new Vector3(xLoc, yLoc - distYDown, 0f);
			for (int j = 0; j < positions.Count; ++j) {
				if ((positions [j] - currPos).magnitude <= threshhold) {
					break;
				}
			}
		}

		//positions.Contains (xLoc, yLoc);

		UnityEngine.Debug.Log ("Number of walls detected");
		UnityEngine.Debug.Log(walls.Length);

		UnityEngine.Debug.Log ("Current Position of Player");
		UnityEngine.Debug.Log (xLoc);
		UnityEngine.Debug.Log (yLoc);

		UnityEngine.Debug.Log ("Distances to walls in all directions");
		UnityEngine.Debug.Log (distXUp);
		UnityEngine.Debug.Log (distXDown);
		UnityEngine.Debug.Log (distYUp);
		UnityEngine.Debug.Log (distYDown);

		//All the crash location options
		//string[] locs = ["End of the Corridor", "Intersection of 2 Corridors", "Start of the Corridor", "Middle of the Corridor", "Towards End of the Corridor", "Towards Start of the Corridor"];

		//If Crash happened while on the Exit Sign
		GameObject exitSign = GameObject.FindGameObjectWithTag("Exit");
		if ((xLoc == (int)exitSign.transform.position.x) & (yLoc == (int)exitSign.transform.position.y)) {
			return "Crashed while on the Exit Sign";
		}
		//TODO(agotsis/wenyuw1) This hardcoding needs to go away. Mainly here to test the database.  
		//For the x direction
		if ((distXUp == 7) & (distXDown == 1) & (distYUp == 1) & (distYDown == 1)) {
			return "Start of the Corridor";
		}
		if ((distXUp == 4) & (distXDown == 4) & (distYUp == 1) & (distYDown == 1)) {
			return "Middle of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 7) & (distYUp == 1) & (distYDown == 1)) {
			return "End of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 8) & (distYUp == 8) & (distYDown == 1)) {
			return "Intersection of 2 Corridors";
		}
		//For the y direction
		if ((distXUp == 1) & (distXDown == 1) & (distYUp == 7) & (distYDown == 2)) {
			return "Start of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp == 4) & (distYDown == 5)) {
			return "Middle of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp == 5) & (distYDown == 4)) {
			return "Middle of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp == 1) & (distYDown == 8)) {
			return "End of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp > distYDown)) {
			return "Towards Start of the Corridor";
		}
		if ((distYUp == 1) & (distYDown == 1) & (distXUp > distXDown)) {
			return "Towards Start of the Corridor";
		}
		if ((distXUp == 1) & (distXDown == 1) & (distYUp < distYDown)) {
			return "Towards End of the Corridor";
		}
		if ((distYUp == 1) & (distYDown == 1) & (distXUp < distXDown)) {
			return "Towards End of the Corridor";
		}

		return "Error";
	}
	
	protected override void OnCantMove <T> (T component)
	{
		//Set hitWall to equal the component passed in as a parameter.
		Wall hitWall = component as Wall;
		SoundManager.instance.PlaySingle(wallHit);
	}
	
	protected override void OnMove () 
	{
	}
	
	private void OnTriggerEnter2D (Collider2D other)
	{
	}
	
	private void OnDisable ()
	{
		//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
		int nextLevel = curLevel + 1;
		GameManager.instance.level = nextLevel;
	}
	
	//Restart reloads the scene when called.
	private void Restart ()
	{
		//Load the last scene loaded, in this case Main, the only scene in the game.
		Application.LoadLevel (Application.loadedLevel);
		restarted = false;
	}
}

