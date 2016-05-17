using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using System.Collections.Generic;

//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
public class Player : MovingObject {

	public float restartLevelDelay = 3.0f;		//Delay time in seconds to restart level.
	private Animator animator;					//Used to store a reference to the Player's animator component.
	private Vector2 touchOrigin = -Vector2.one;

	private float touchTime = 0f;
	private float minSwipeDist = 100f;
	private bool restarted = false;
	private int curLevel;

	private const int right = 0;
	private const int left = 180;
	private const int up = 90;
	private const int down = 270;
	private int curDirection = right;
	private bool changedDir = false;
	private bool movingForward = true;
	
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
	public AudioClip swipeAhead;
	public AudioClip swipeRight;
	public AudioClip swipeLeft;

	public Sprite upSprite;
	public Sprite downSprite; 
	public Sprite leftSprite; 
	public Sprite rightSprite;
	private SpriteRenderer spriteRenderer; 

	//public bool soundPlaying = false;
	protected override void Start () {
		//Get a component reference to the Player's animator component
		animator = GetComponent<Animator>();
		curLevel = GameManager.instance.level;
		spriteRenderer = GetComponent<SpriteRenderer>();
		base.Start ();
	}

	private void PlayEcho(int dist) {
		switch (dist) 
		{
			case 0:
			case 1:
				SoundManager.instance.PlaySingle(echo1m);
				Debug.Log ("1m");
				break;
			case 2:
				SoundManager.instance.PlaySingle(echo2m);
				Debug.Log ("2m");
				break;
			case 3:
				SoundManager.instance.PlaySingle(echo3m);
				Debug.Log ("3m");
				break;
			case 4:
				SoundManager.instance.PlaySingle(echo4m);
				Debug.Log ("4m");
				break;
			case 5:
				SoundManager.instance.PlaySingle(echo5m);
				Debug.Log ("5m");
				break;
			case 6:
				SoundManager.instance.PlaySingle(echo6m);
				Debug.Log ("6m");
				break;
			case 7:
				SoundManager.instance.PlaySingle(echo7m);
				Debug.Log ("7m");
				break;
			default:
				SoundManager.instance.PlaySingle(echo7m);
				break;
			
		}
	}
		
	void printDir() {
		if (curDirection == left) {
			Debug.Log ("I left");
		} else if (curDirection == right) {
			Debug.Log ("I right");
		} else if (curDirection == up) {
			Debug.Log ("I up");
		} else if (curDirection == down) {
			Debug.Log ("I down");
		} else {
			Debug.Log ("No direction match");
		}
	}

	void ChangeSprite()
	{
		switch(curDirection) {
			case left:
				spriteRenderer.sprite = leftSprite;
				break;
			case right:
				spriteRenderer.sprite = rightSprite;
				break;
			case up:
				spriteRenderer.sprite = upSprite;
				break;
			case down:
				spriteRenderer.sprite = downSprite;
				break;
		}
	}
		
	private void calculateMove(int horizontal, int vertical) {
		if(horizontal != 0)
		{
			// when user presses the right/left arrow keys, the direction of the player is changed
			vertical = 0;
			if (horizontal > 0) {
				// a swipe to the right was made
				curDirection = (curDirection - 90);
				if (curDirection < 0) {
					curDirection = 360 + curDirection;
				}
			} else {
				// a swipe to the left was made, a left corresponds to a 90 counterclockwise turn
				curDirection = (curDirection + 90) % 360;
			}
			changedDir = true;
			ChangeSprite();
		} else if (vertical != 0) {
			// when the user presses the up-down arrow keys, the player moves in the direction the
			// player currently faces
			horizontal = 0;
			if (vertical < 0) {
				movingForward = false;
			} else {
				movingForward = true;
			}
			changedDir = false;
		}

		if((horizontal != 0 || vertical != 0) && !changedDir)
		{
			switch(curDirection) {
				case left:
					horizontal = -1;
					vertical = 0;
					break;
				case right:
					horizontal = 1;
					vertical = 0;
					break;
				case up:
					horizontal = 0;
					vertical = 1;
					break;
				case down:
					horizontal = 0;
					vertical = -1;
					break;
			}
			if (!movingForward) {
				horizontal *= -1;
				vertical *= -1;
			}
			AttemptMove<Wall> (horizontal, vertical);
		}
	}

	private void attemptExitFromLevel() {
		GameObject exitSign = GameObject.FindGameObjectWithTag("Exit");
		Vector2 distFromExit = transform.localPosition - exitSign.transform.localPosition;
		if (Vector2.SqrMagnitude(distFromExit) < 0.25) {
			//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
			restarted = true;
			Invoke ("Restart", restartLevelDelay);
			//Disable the player object since level is over.
			enabled = false;
			AudioSource.PlayClipAtPoint(winSound, transform.localPosition, 0.3f);
		}
	}

	private void Update () {
		//If it's not the player's turn, exit the function.
		if(!GameManager.instance.playersTurn) return;
		
		int horizontal = 0;
		int vertical = 0;
		
		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER
		
		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			horizontal = 1;
			SoundManager.instance.PlaySingle(swipeRight);
		} else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			horizontal = -1;
			SoundManager.instance.PlaySingle(swipeLeft);
		} else if (Input.GetKeyUp(KeyCode.UpArrow)) {
			vertical = 1;
			SoundManager.instance.PlaySingle(swipeAhead);
		} else if (Input.GetKeyUp(KeyCode.DownArrow)) {
			vertical = -1;
			SoundManager.instance.PlaySingle(swipeAhead);
		}

		if (Input.GetKeyUp("f")) {
			PlayEcho(echoDist());
		} else if (Input.GetKeyUp("e")) {
			attemptExitFromLevel();
		}
			
		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		
		//Check if Input has registered more than zero touches
		int numTouches = Input.touchCount;
		
		if (numTouches > 0) {
			//Store the first touch detected.
			Touch myTouch = Input.touches[0];
			
			
			//Check if the phase of that touch equals Began
			if (myTouch.phase == TouchPhase.Began)
			{
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
					vertical = 0;
					if (x > 0) {
						horizontal = 1;
						SoundManager.instance.PlaySingle(swipeRight);
					} else {
						horizontal = -1;
						SoundManager.instance.PlaySingle(swipeLeft);
					}
				} else if (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) {
					//If y is greater than zero, set vertical to 1, otherwise set it to -1
					horizontal = 0;
					if (y > 0) {
						vertical = 1;
						SoundManager.instance.PlaySingle(swipeAhead);
					} else {
						vertical = -1;
						SoundManager.instance.PlaySingle(swipeAhead);
					}
				} else if (Mathf.Abs(Time.time - touchTime) > 0.05) {
					if (numTouches == 2) {
						attemptExitFromLevel();
					} else {
						PlayEcho(echoDist());
					}
				}
			}
		}
		#endif //End of mobile platform dependendent compilation section started above with #elif
		calculateMove(horizontal, vertical);
	}

	private int echoDist() {
		//Get all the walls on the grid
		GameObject[] wallsArray = GameObject.FindGameObjectsWithTag("Wall");
		List <Vector3> wallPositions = new List<Vector3>();
		foreach (GameObject wall in wallsArray) {
			wallPositions.Add(wall.transform.localPosition);
		}
		int dist;
		int minDistance = 7;
		GameObject player = GameObject.Find("Player");
		int personX = (int) Mathf.Ceil(player.transform.localPosition.x);
		int personY = (int) Mathf.Ceil (player.transform.localPosition.y);
		switch (curDirection) {
			case right:
				Debug.Log ("echoDist right");
					//We're currrently facing right
				for (int i = 0; i < 8; i++) {
					int wallX = personX + i;
					Vector3 tPos = new Vector3 (wallX, personY, 0);
					if (wallPositions.Contains (tPos)) {
						dist = Mathf.Abs (wallX - personX);
						minDistance = Mathf.Min (minDistance, dist);
						;
					}
				}
				Debug.Log ("Echo_dist " + minDistance);
				return minDistance;
			case left:
				Debug.Log ("echoDist left");
				//We're currrently facing left
				for (int i = 1; i < 8; i++) {
					int wallX = personX - i;
					Vector3 tPos = new Vector3(wallX, personY, 0);
					if (wallPositions.Contains(tPos)) {
						dist = Mathf.Abs(wallX - personX);
						minDistance = Mathf.Min(minDistance, dist);
					}
				}
				Debug.Log ("Echo_dist " + minDistance);
				return minDistance;
			case up:
				Debug.Log ("echoDist up");
				//We're currrently facing up
				for (int i = 1; i < 8; i++) {
					int wallY = personY + i;
					Vector3 tPos = new Vector3(personX, wallY, 0);
					//Debug.Log("possible wall_Y " + wallY + " x pos " + personX);
					if (wallPositions.Contains(tPos)) {
						Debug.Log("wall_Y exists " + wallY);
						dist = Mathf.Abs(wallY - personY);
						Debug.Log("dist " + dist);
						minDistance = Mathf.Min(minDistance, dist);
					}
				}
				Debug.Log ("Echo_dist " + minDistance);
				return minDistance;
			case down:
				Debug.Log ("echoDist down");
				//We're currrently facing down
				for (int i = 1; i < 8; i++) {
					int wallY = personY - i;
					Vector3 tPos = new Vector3(personX, wallY, 0);
					if (wallPositions.Contains(tPos)) {
						dist = Mathf.Abs(wallY - personY);
						minDistance = Mathf.Min(minDistance, dist);

					}
				}
				Debug.Log ("Echo_dist " + minDistance);
				return minDistance;
			default:
				Debug.Log ("echoDist defualt");
				//default case, should never get here
				return 7;
		}
	}


	protected override bool AttemptMove <T> (int xDir, int yDir)
	{	
		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		bool canMove = base.AttemptMove <T> (xDir, yDir);
		
		//If player could not move to that location, play the crash sound
		if (!canMove) {
			SoundManager.instance.PlaySingle(wallHit);
		}
		
		//Hit allows us to reference the result of the Linecast done in Move.
		RaycastHit2D hit;
		
		//GameManager.instance.playersTurn = false;
		return canMove;
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


