using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.

//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
public class Player : MovingObject {

	public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.
	private Animator animator;					//Used to store a reference to the Player's animator component.
	private Vector2 touchOrigin = -Vector2.one;

	private float touchTime = 0f;
	private float minSwipeDist = 100f;

	public AudioClip echo1m;
	public AudioClip echo2m;
	public AudioClip echo3m;
	public AudioClip echo4m;
	public AudioClip echo5m;
	public AudioClip echo6m;
	public AudioClip echo7m;
	public AudioClip wallHit;

	//public bool soundPlaying = false;
	protected override void Start ()
	{
		//Get a component reference to the Player's animator component
		animator = GetComponent<Animator>();

		//Call the Start function of the MovingObject base class.
		base.Start ();
	}

	private void PlayEcho(int xPos) {
		switch (xPos) 
		{
			
			case 0:
				SoundManager.instance.PlaySingle(echo7m);
				Debug.Log ("7m");
				break;
			case 1:
				SoundManager.instance.PlaySingle(echo6m);
				Debug.Log ("6m");
				break;
			case 2:
				SoundManager.instance.PlaySingle(echo5m);
				Debug.Log ("5m");
				break;
			case 3:
				SoundManager.instance.PlaySingle(echo4m);
				Debug.Log ("4m");
				break;
			case 4:
				SoundManager.instance.PlaySingle(echo3m);
				Debug.Log ("3m");
				break;
			case 5:
				SoundManager.instance.PlaySingle(echo2m);
				Debug.Log ("2m");
				break;
			case 6:
				SoundManager.instance.PlaySingle(echo1m);
				Debug.Log ("1m");
				break;
			default:
				SoundManager.instance.PlaySingle(echo1m);
				break;
			
		}
	}

	private void Update ()
	{
		//If it's not the player's turn, exit the function.
		if(!GameManager.instance.playersTurn) return;
		
		int horizontal = 0;  	//Used to store the horizontal move direction.
		int vertical = 0;		//Used to store the vertical move direction.
		
		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER
		
		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
		
		//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
		vertical = (int) (Input.GetAxisRaw ("Vertical"));
		
		//Check if moving horizontally, if so set vertical to zero.
		if(horizontal != 0)
		{
			vertical = 0;
		}

		if (Input.GetKeyDown("f")) {
			Debug.Log ("time down");
			Debug.Log (Time.time);
		}

		if (Input.GetKeyUp("f")) {
			
			Vector2 curPosition = transform.position;
			int xPos = (int) Mathf.Ceil(curPosition.x);
			PlayEcho(xPos);
			Debug.Log ("time UP");
			Debug.Log (Time.time);	
		}

		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		
		//Check if Input has registered more than zero touches
		if (Input.touchCount > 0)
		{
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
			else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
			{
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
					horizontal = x > 0 ? 1 : -1;
					Debug.Log ("X direction swipe");
				}
				else if (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) {
					//If y is greater than zero, set horizontal to 1, otherwise set it to -1
					vertical = y > 0 ? 1 : -1;
					Debug.Log ("Y direction swipe");
				}
				else if (Mathf.Abs(Time.time - touchTime) > 0.25) {
					Vector2 curPosition = transform.position;
					int xPos = (int) Mathf.Ceil(curPosition.x);
					PlayEcho(xPos);
				}

			}
		}
		
		#endif //End of mobile platform dependendent compilation section started above with #elif
		//Check if we have a non-zero value for horizontal or vertical

		if(horizontal != 0 || vertical != 0)
		{
			AttemptMove<Wall> (horizontal, vertical);
		}
	}


	protected override void AttemptMove <T> (int xDir, int yDir)
	{	
		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		base.AttemptMove <T> (xDir, yDir);
		
		//Hit allows us to reference the result of the Linecast done in Move.
		RaycastHit2D hit;

	}

	protected override void OnCantMove <T> (T component)
	{
		//Set hitWall to equal the component passed in as a parameter.
		Wall hitWall = component as Wall;
		Debug.Log ("Hit the wall");
		SoundManager.instance.PlaySingle(wallHit);
	}
		
	//Restart reloads the scene when called.
	private void Restart ()
	{
		//Load the last scene loaded, in this case Main, the only scene in the game.
		Application.LoadLevel (Application.loadedLevel);
	}
}


