using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GM_title : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			SceneManager.LoadScene("Main");
			//SoundManager.instance.PlaySingle(swipeRight);
		} else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			SceneManager.LoadScene("Main");
			//SoundManager.instance.PlaySingle(swipeLeft);
		} else if (Input.GetKeyUp("f")) {
			SceneManager.LoadScene("Main");
			//SoundManager.instance.PlaySingle(swipeAhead);
		}

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
	}
}
