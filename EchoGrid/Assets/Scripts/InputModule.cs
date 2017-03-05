using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct InputEvent{
	public bool isRight;
	public bool isLeft;
	public bool isUp;
	public bool isDown;
	public int touchNum;
	public int cumulativeTouchNum;//within a short period of time, how many time user touches
	public bool isRotate;
	public KeyCode keycode;
	public float elapsedTime;//how long the user has hold

	public void init(){
		isRight = false; isLeft = false; isUp = false; isDown = false;
		touchNum = 0; isRotate = false; keycode = KeyCode.None;
		elapsedTime = 0;
		cumulativeTouchNum = 0;
	}

	public bool hasDir(){
		return isRight || isLeft || isUp || isDown;
	}

	public bool hasEffectiveInput(){
		return isRight || isLeft || isUp || isDown || isRotate || (keycode != KeyCode.None) || (touchNum > 0) || (cumulativeTouchNum > 0);
	}
}

//Class to receive input
public class InputModule : MonoBehaviour {
	//consts
	const float TOUCH_TIME = 0.02f;
	float minSwipeDist = Screen.width*0.01f;
	const float multiTapCD = 0.4f;//make multitap easier, num of tap made within this time period
	const float rotateGestCD = 0.3f;

	//storage needed for recognizing different input
	bool hasrotated = false;
	bool swp_lock = false;//stop very fast input
	bool isSwipe = false;
	int TouchTapCount;
	int numTouchlastframe = 0;
 	float touchTime = 0f;
	float rotateGestStartTime;
	float multiTapStartTime;
	Vector2 swipeStartPlace = new Vector2();
	Vector2 firstSwipePos = new Vector2();
	Vector2 VecStart = new Vector2();
	Vector2 VecEnd = new Vector2();
	Vector2 touchOrigin = new Vector2();

	List<eventHandler> listeners = new List<eventHandler>();
	List<CDTimer> cdtiemrs = new List<CDTimer>();
	
	public static InputModule instance;
	void Awake ()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	}

	public void RegisterEventHandler(eventHandler eh){
		listeners.Add (eh);
	}

	public void RegisterCDTimer(CDTimer ct){
		cdtiemrs.Add (ct);
	}

	// Update is called once per frame
	void Update () {
		GetInput ();
	}

	void GetInput(){
		InputEvent ievent = new InputEvent ();
		ievent.init ();

		#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

		//Check if Input has registered more than zero touches
		int numTouches = Input.touchCount;
		ievent.touchNum = numTouches;

		//temp storage
		Touch myTouch;
		Vector2 touchEndpos = new Vector2();
		BoardManager.Direction swp_dir = BoardManager.Direction.OTHER;
		bool isRotation = false;

		//update all timers
		//update TouchTapCount part 1
		if( (Time.time - multiTapStartTime) >= multiTapCD ){
			multiTapStartTime = Time.time;
			TouchTapCount = 0;
		}

		//collect raw data from the device
		if (numTouches > 0) {
			//Store the first touch detected.
			myTouch = Input.touches[0];
			touchEndpos = myTouch.position;

			if((numTouches == 2) && numTouches != numTouchlastframe){
				VecStart = Input.touches[0].position - Input.touches[1].position;
			}
				
			if (myTouch.phase == TouchPhase.Began){
				hasrotated = false;
				swipeStartPlace = myTouch.position;
				//get the vector between 2 fingers
				if(numTouches == 2){
					VecStart = myTouch.position - Input.touches[1].position;
				}
				//If so, set touchOrigin to the position of that touch
				touchOrigin = myTouch.position;
				touchTime = Time.time;
				swp_lock = true;
				ResetCDTimers();
			} else if ((myTouch.phase == TouchPhase.Ended) && swp_lock){//deals with swipe and multiple taps
				//Set touchEnd to equal the position of this touch
				hasrotated = false;
				touchEndpos = myTouch.position;
				float x = touchEndpos.x - touchOrigin.x;
				float y = touchEndpos.y - touchOrigin.y;

				if (Mathf.Abs(x) > Mathf.Abs(y) && Mathf.Abs(x) >= minSwipeDist){//right & left
					if (x > 0){//right
						swp_dir = BoardManager.Direction.RIGHT;
						ievent.isRight = true;
					}else{//left
						swp_dir = BoardManager.Direction.LEFT;
						ievent.isLeft = true;
					}
				} else if (Mathf.Abs(y) > Mathf.Abs(x) && Mathf.Abs(y) >= minSwipeDist) {//up & down
					if (y > 0){//up/front
						swp_dir = BoardManager.Direction.FRONT;
						ievent.isUp = true;
					} else{//down/back
						swp_dir = BoardManager.Direction.BACK;
						ievent.isDown = true;					}
				}

				//update TouchTapCount part 2
				if( (Time.time - multiTapStartTime) < multiTapCD ){
					TouchTapCount += myTouch.tapCount;
					ievent.elapsedTime = Time.time - touchTime;
				}

				swp_lock = false;//flip the lock, until we find another TouchPhase.Began
			}else if( (numTouches == 2) && (!hasrotated) ){
				
				if(numTouches == 2){//detect a rotate
					VecEnd = Input.touches[0].position - Input.touches[1].position;
					Vector3 cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized);
					float crossPz = cross.z;
					if( (crossPz >= 0)&&(Mathf.Abs(crossPz) >= Screen.height*0.00015f ) ){//left
						isRotation = true;
						hasrotated = true;
						if(Time.time - rotateGestStartTime >= rotateGestCD){
							rotateGestStartTime = Time.time;
							swp_dir = BoardManager.Direction.LEFT;
							ievent.isLeft = true;
							ievent.elapsedTime = Time.time - touchTime;
						}
					} else if( (crossPz < 0)&&(Mathf.Abs(crossPz) >= Screen.height*0.00015f) ){//right
						isRotation = true;
						hasrotated = true;
						if(Time.time - rotateGestStartTime >= rotateGestCD){
							rotateGestStartTime = Time.time;
							swp_dir = BoardManager.Direction.RIGHT;
							ievent.isRight = true;
							ievent.elapsedTime = Time.time - touchTime;
						}
					}
					ievent.isRotate = true;
				}
			}
			numTouchlastframe = numTouches;
			ievent.elapsedTime = Time.time - touchTime;
		} else{
			numTouchlastframe = 0;
		}

		float touchx = touchEndpos.x - touchOrigin.x;
		float touchy = touchEndpos.y - touchOrigin.y;

		if (Mathf.Abs(touchx) >= minSwipeDist){//right & left
			isSwipe = true;
		} else if (Mathf.Abs(touchy) >= minSwipeDist) {//up & down
			isSwipe = true;
		} else
			isSwipe = false;
		
		ievent.cumulativeTouchNum = TouchTapCount;
		#elif UNITY_STANDALONE || UNITY_WEBPLAYER
			if (Input.GetKeyUp (KeyCode.RightArrow)) {
				ievent.keycode = KeyCode.RightArrow;
			} else if (Input.GetKeyUp (KeyCode.LeftArrow)) {
				ievent.keycode = KeyCode.LeftArrow;
			} else if (Input.GetKeyUp (KeyCode.UpArrow)) {
				ievent.keycode = KeyCode.UpArrow;
			} else if (Input.GetKeyUp (KeyCode.DownArrow)) {
				ievent.keycode = KeyCode.DownArrow;
			} else if (Input.GetKeyUp ("f")) {
				ievent.keycode = KeyCode.F;
			} else if (Input.GetKeyUp ("e")) {
				ievent.keycode = KeyCode.E;
			} else if (Input.GetKeyUp ("r")) {
				ievent.keycode = KeyCode.R;
			} else if (Input.GetKeyUp ("m")) {
				ievent.keycode = KeyCode.M;
			} else
				return;
		#endif
		//print(touchTime);
		//if no input, code should not reach here
		PassDataToListeners(ievent);
		NotifyLlisteners ();
	}

	void PassDataToListeners(InputEvent _ie){
		foreach (eventHandler eh in listeners) {
			if (eh != null)
				eh.SetData (_ie);
		}
	}

	void NotifyLlisteners(){
		foreach (eventHandler eh in listeners) {
			if (eh != null)
				eh.Activate ();
		}
	}

	void ResetCDTimers(){
		foreach (CDTimer ctimer in cdtiemrs) {
			if (ctimer != null)
				ctimer.resetLock();
		}
	}
}

public class eventHandler{
	bool hasNewEvent;
	InputEvent inputevetData;

	//so when you new() it's automatically registered
	public eventHandler(InputModule Iinstance){
		Iinstance.RegisterEventHandler(this);
	}

	public void Activate(){
		hasNewEvent = true;
	}

	//this quary clears flag
	//Note: currently this function always return true for first qury of each frame
	//This is due to the gestures needed for this game
	//inputEvent has a function called hasEffectiveInput that actually does what this function does
	public bool isActivate(){
		bool result = hasNewEvent;
		deActivate ();
		return result;
	}

	public void deActivate(){
		hasNewEvent = false;
	}

	public InputEvent getEventData(){
		return inputevetData;
	}

	public void SetData(InputEvent _ie){
		inputevetData = _ie;
	}
}

//not only CD, but also lock operation per touch
public class CDTimer{
	float startTime;
	float CD;
	bool isLock;

	public CDTimer(float _cd, InputModule Iinstance){
		CD = _cd;
		isLock = false;
		Iinstance.RegisterCDTimer (this);
	}

	public void TakeDownTime(){
		startTime = Time.time;
	}

	public bool CDfinish(){
		if((Time.time - startTime >= CD)&&!isLock){
			isLock = true;
			return true; 
		}

		return false;
	}

	public void reset(){
		startTime = Time.time;
	}

	public void resetLock(){
		isLock = false;
	}
}
