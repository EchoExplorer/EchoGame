using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// A struct containing a large number of fields about user input data. 
/// </summary>
public struct InputEvent
{
    public bool isRight;
    public bool isLeft;
    public bool isUp;
    public bool isDown;
    public int touchNum;
    public int cumulativeTouchNum;//within a short period of time, how many time user touches
	public bool isSingleTap;
	public bool isDoubleTap;
	public bool isTripleTap;
	public bool isHold;
	public bool isSwipe;
    public bool isRotate;
    public bool isUnrecognized;
    public KeyCode keycode;
 	public float elapsedTime;//how long the user has hold

    /// <summary>
    /// Initializes all the fields to default values.
    /// </summary>
	public void init()
    {
        isRight = false; isLeft = false; isUp = false; isDown = false;
		isSingleTap = false; isDoubleTap = false; isTripleTap = false; 
		isHold = false; isSwipe = false; isRotate = false; isUnrecognized = false; keycode = KeyCode.None;
        elapsedTime = 0.0f;
		touchNum = 0; 
        cumulativeTouchNum = 0;
    }

    /// <summary>
    /// Determines whether any directional input was received.
    /// </summary>
	public bool hasDir()
    {
        return isRight || isLeft || isUp || isDown;
    }

    /// <summary>
    /// Determines if any meaningful input was received.
    /// </summary>
    /// <returns></returns>
	public bool hasEffectiveInput()
    {
        return isRight || isLeft || isUp || isDown || isSingleTap || isDoubleTap || isTripleTap || isHold || isSwipe || isRotate || isUnrecognized || (keycode != KeyCode.None) || (touchNum > 0) || (cumulativeTouchNum > 0);
    }
}

/// <summary>
/// A class that maintains event handlers for input events and countdown timers.
/// </summary>
public class InputModule : MonoBehaviour
{
    //consts
    const float TOUCH_TIME = 0.02f;
    float minSwipeVerticalDist = Screen.width * 0.07f;
	float minSwipeHorizontalDist = Screen.height * 0.10f;
	float minHoldVerticalDist = Screen.width * 0.04f;
	float minHoldHorizontalDist = Screen.height * 0.05f;
    const float multiTapCD = 0.4f;//make multitap easier, num of tap made within this time period
	const float swipeGestCD = 0.3f;
    const float rotateGestCD = 0.3f;

    int TouchTapCount;
    float touchTime = 0.0f;
	float multiTapStartTime = 0.0f;
	float swipeGestStartTime = 0.0f;
    float rotateGestStartTime = 0.0f;

    List<eventHandler> listeners = new List<eventHandler>();
    List<CDTimer> cdtimers = new List<CDTimer>();

	public Text debugInputInfo;
	public Text debugPlayerInfo;
	public Text touch0Info;
	public Text touch1Info;
	public Text touch2Info;
	public Text touchDurationInfo;

	Vector2[] touchStart = {new Vector2(), new Vector2(), new Vector2()};
	Vector2[] touchEnd = {new Vector2(), new Vector2(), new Vector2()};
	Vector2 vecStart1 = new Vector2();
	Vector2 vecEnd1 = new Vector2();
	Vector2 vecStart2 = new Vector2();
	Vector2 vecEnd2 = new Vector2();
	Vector2 VecStart = new Vector2();
	Vector2 VecEnd = new Vector2();

	float touchDuration = 0.0f;
	int tapRegister = 0;
	bool[] hasRegistered = {false, false, false};

	int singleTapTimes = 0;
	int doubleTapTimes = 0;
	int tripleTapTimes = 0;
	int swipeLeftTimes = 0;
	int swipeRightTimes = 0;
	int swipeUpTimes = 0;
	int swipeDownTimes = 0;
	int rotateLeftTimes = 0;
	int rotateRightTimes = 0;

    public static InputModule instance;
    void Awake()
    {
		if (instance == null) 
		{
			instance = this;
		} 
		else if (instance != this) 
		{
			Destroy(gameObject);
		}

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Registers an event handler to all input events.
    /// </summary>
    public void RegisterEventHandler(eventHandler eh)
    {
        listeners.Add(eh);
    }

    /// <summary>
    /// Registers a new countdown timer.
    /// </summary>
    public void RegisterCDTimer(CDTimer ct)
    {
        cdtimers.Add(ct);
    }

    /// <summary>
    /// Checks for new input data every frame.
    /// </summary>
    void Update()
    {
		debugInputInfo = GameObject.FindGameObjectWithTag("DebugInput").GetComponent<Text>();
		debugPlayerInfo = GameObject.FindGameObjectWithTag("DebugPlayer").GetComponent<Text>();
		touch0Info = GameObject.FindGameObjectWithTag("Touch0").GetComponent<Text>();
		touch1Info = GameObject.FindGameObjectWithTag("Touch1").GetComponent<Text>();
		touch2Info = GameObject.FindGameObjectWithTag("Touch2").GetComponent<Text>();
		touchDurationInfo = GameObject.FindGameObjectWithTag("TouchDuration").GetComponent<Text>();

        GetInput();
    }

    /// <summary>
    /// Checks each key individually to determine if there is input data.
    /// </summary>
    void GetInput()
    {
        InputEvent ievent = new InputEvent();
        ievent.init();
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
        	debugInputInfo.text = "Right arrow key pressed.";
            ievent.keycode = KeyCode.RightArrow;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
			debugInputInfo.text = "Left arrow key pressed.";
            ievent.keycode = KeyCode.LeftArrow;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
			debugInputInfo.text = "Up arrow key pressed.";
            ievent.keycode = KeyCode.UpArrow;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
			debugInputInfo.text = "Down arrow key pressed.";
            ievent.keycode = KeyCode.DownArrow;
        }
        else if (Input.GetKeyUp("f"))
        {
			debugInputInfo.text = "F key pressed.";
            ievent.keycode = KeyCode.F;
        }
        else if (Input.GetKeyUp("e"))
        {
			debugInputInfo.text = "E key pressed.";
            ievent.keycode = KeyCode.E;
        }
        else if (Input.GetKeyUp("g"))
        {
			debugInputInfo.text = "G key pressed.";
            ievent.keycode = KeyCode.G;
        }
        else if (Input.GetKeyUp("r"))
        {
			debugInputInfo.text = "R key pressed.";
            ievent.keycode = KeyCode.R;
        }
        else if (Input.GetKeyUp("m"))
        {
			debugInputInfo.text = "M key pressed.";
            ievent.keycode = KeyCode.M;
        }
		else if (Input.GetKeyUp("p"))
		{
			debugInputInfo.text = "P key pressed.";
			ievent.keycode = KeyCode.P;
		}
        else
		{
            return;
		}
#endif
#if UNITY_IOS || UNITY_ANDROID
		//Check if Input has registered more than zero touches
		int numTouches = Input.touchCount;
		ievent.touchNum = numTouches;

		foreach(Touch thisTouch in Input.touches) 
		{
			if (thisTouch.fingerId == 0)
			{
				if (hasRegistered[0] == false) 
				{
					touchStart[0] = thisTouch.position;
					touchEnd[0] = thisTouch.position;
					vecStart1 = thisTouch.position;
					vecEnd1 = thisTouch.position;
					touch0Info.text = "Start X: " + touchStart[0].x + "\n" + "Start Y: " + touchStart[0].y + "\n" + "End X: " + touchEnd[0].x + "\n" + "End Y: " + touchEnd[0].y; 
					tapRegister = 0;
					hasRegistered[0] = true;
				}
				else if ((thisTouch.phase == TouchPhase.Stationary) && (hasRegistered[0] == true)) 
				{
					touchEnd[0] = thisTouch.position;
					vecEnd1 = thisTouch.position;
					touch0Info.text = "Start X: " + touchStart[0].x + "\n" + "Start Y: " + touchStart[0].y + "\n" + "End X: " + touchEnd[0].x + "\n" + "End Y: " + touchEnd[0].y; 
				}
				else if ((thisTouch.phase == TouchPhase.Moved) && (hasRegistered[0] == true)) 
				{
					touchEnd[0] = thisTouch.position;
					vecEnd1 = thisTouch.position;
					touch0Info.text = "Start X: " + touchStart[0].x + "\n" + "Start Y: " + touchStart[0].y + "\n" + "End X: " + touchEnd[0].x + "\n" + "End Y: " + touchEnd[0].y; 
				}
				else if ((thisTouch.phase == TouchPhase.Ended) && (hasRegistered[0] == true)) 
				{
					tapRegister += 1;
					touchEnd[0] = thisTouch.position;
					vecEnd1 = thisTouch.position;
					touch0Info.text = "Start X: " + touchStart[0].x + "\n" + "Start Y: " + touchStart[0].y + "\n" + "End X: " + touchEnd[0].x + "\n" + "End Y: " + touchEnd[0].y; 

					// update TouchTapCount part 2
					if ((Time.time - multiTapStartTime) < multiTapCD) 
					{
						if ((tapRegister == 3) && (TouchTapCount <= 6)) 
						{
							TouchTapCount += tapRegister;
						}
						ievent.elapsedTime = Time.time - touchTime;
					}
				}
				else if (thisTouch.phase == TouchPhase.Canceled) 
				{
					touch0Info.text = "touch0 canceled";
				}
				else 
				{
					touch0Info.text = "Cannot compute";
				}
			}
			else if (thisTouch.fingerId == 1) 
			{
				if (hasRegistered[1] == false) 
				{
					touchStart[1] = thisTouch.position;
					touchEnd[1] = thisTouch.position;
					vecStart2 = thisTouch.position;
					vecEnd2 = thisTouch.position;
					touch1Info.text = "Start X: " + touchStart[1].x + "\n" + "Start Y: " + touchStart[1].y + "\n" + "End X: " + touchEnd[1].x + "\n" + "End Y: " + touchEnd[1].y;
					hasRegistered[1] = true;
				}
				else if ((thisTouch.phase == TouchPhase.Stationary) && (hasRegistered[1] == true)) 
				{
					touchEnd[1] = thisTouch.position;
					vecEnd2 = thisTouch.position;
					touch1Info.text = "Start X: " + touchStart[1].x + "\n" + "Start Y: " + touchStart[1].y + "\n" + "End X: " + touchEnd[1].x + "\n" + "End Y: " + touchEnd[1].y;
				}
				else if ((thisTouch.phase == TouchPhase.Moved) && (hasRegistered[1] == true)) 
				{
					touchEnd[1] = thisTouch.position;
					vecEnd2 = thisTouch.position;
					touch1Info.text = "Start X: " + touchStart[1].x + "\n" + "Start Y: " + touchStart[1].y + "\n" + "End X: " + touchEnd[1].x + "\n" + "End Y: " + touchEnd[1].y;
				}
				else if ((thisTouch.phase == TouchPhase.Ended) && (hasRegistered[1] == true)) 
				{
					tapRegister += 1;
					touchEnd[1] = thisTouch.position;
					vecEnd2 = thisTouch.position;
					touch1Info.text = "Start X: " + touchStart[1].x + "\n" + "Start Y: " + touchStart[1].y + "\n" + "End X: " + touchEnd[1].x + "\n" + "End Y: " + touchEnd[1].y;

					// update TouchTapCount part 2
					if ((Time.time - multiTapStartTime) < multiTapCD) 
					{
						if ((tapRegister == 3) && (TouchTapCount <= 6)) 
						{
							TouchTapCount += tapRegister;	
						}
						ievent.elapsedTime = Time.time - touchTime;
					}
				}
				else if (thisTouch.phase == TouchPhase.Canceled) 
				{
					touch1Info.text = "touch1 canceled";
				}
				else 
				{
					touch1Info.text = "Cannot compute";
				}
			}
			else if (thisTouch.fingerId == 2) 
			{
				if (hasRegistered[2] == false) 
				{
					touchStart[2] = thisTouch.position;
					touchEnd[2] = thisTouch.position;
					touch2Info.text = "Start X: " + touchStart[2].x + "\n" + "Start Y: " + touchStart[2].y + "\n" + "End X: " + touchEnd[2].x + "\n" + "End Y: " + touchEnd[2].y;
					hasRegistered[2] = true;

					// update TouchTapCount part 1
					if ((Time.time - multiTapStartTime) >= multiTapCD)
					{
						multiTapStartTime = Time.time;
						TouchTapCount = 0;
					}

					touchTime = Time.time;
					ResetCDTimers();
				}
				else if ((thisTouch.phase == TouchPhase.Stationary) && (hasRegistered[2] == true)) 
				{
					touchEnd[2] = thisTouch.position;
					touch2Info.text = "Start X: " + touchStart[2].x + "\n" + "Start Y: " + touchStart[2].y + "\n" + "End X: " + touchEnd[2].x + "\n" + "End Y: " + touchEnd[2].y;
				}
				else if ((thisTouch.phase == TouchPhase.Moved) && (hasRegistered[2] == true)) 
				{
					touchEnd[2] = thisTouch.position;
					touch2Info.text = "Start X: " + touchStart[2].x + "\n" + "Start Y: " + touchStart[2].y + "\n" + "End X: " + touchEnd[2].x + "\n" + "End Y: " + touchEnd[2].y;
				}
				else if ((thisTouch.phase == TouchPhase.Ended) && (hasRegistered[2] == true)) 
				{
					tapRegister += 1;
					touchEnd[2] = thisTouch.position;
					touch2Info.text = "Start X: " + touchStart[2].x + "\n" + "Start Y: " + touchStart[2].y + "\n" + "End X: " + touchEnd[2].x + "\n" + "End Y: " + touchEnd[2].y;

					// update TouchTapCount part 2
					if ((Time.time - multiTapStartTime) < multiTapCD) 
					{
						if ((tapRegister == 3) && (TouchTapCount <= 6)) 
						{
							TouchTapCount += tapRegister;	
						}
						ievent.elapsedTime = Time.time - touchTime;
					}
				}
				else if (thisTouch.phase == TouchPhase.Canceled) 
				{
					touch2Info.text = "touch2 canceled";
				}
				else 
				{
					touch2Info.text = "Cannot compute";
				}
			}

			ievent.elapsedTime = Time.time - touchTime;
		}

		if (Input.touchCount > 0) 
		{
			if (Input.touchCount == 3) 
			{
				touchDuration = touchDuration + Time.deltaTime;
				touchDurationInfo.text = "Duration = " + touchDuration;

				if (touchDuration >= 1.0f) 
				{
					float x0 = touchEnd[0].x - touchStart[0].x;
					float y0 = touchEnd[0].y - touchStart[0].y;
					float x1 = touchEnd[1].x - touchStart[1].x;
					float y1 = touchEnd[1].y - touchStart[1].y;
					float x2 = touchEnd[2].x - touchStart[2].x;
					float y2 = touchEnd[2].y - touchStart[2].y;

					if ((Mathf.Abs(x0) <= minHoldHorizontalDist) && (Mathf.Abs(x1) <= minHoldHorizontalDist) && (Mathf.Abs(x2) <= minHoldHorizontalDist) && (Mathf.Abs(y0) <= minHoldVerticalDist) && (Mathf.Abs(y1) <= minHoldVerticalDist) && (Mathf.Abs(y2) <= minHoldVerticalDist))
					{
						ievent.isHold = true;
						debugInputInfo.text = "Hold registered";	
					}
				}
			}	
		}

		if (Input.touchCount == 0) 
		{
			// Check for single/double/triple taps.
			if ((tapRegister == 3) && (touchDuration < 1.0f) && (ievent.isSwipe == false) && (ievent.isRotate == false)) 
			{
				if ((Time.time - multiTapStartTime) >= multiTapCD)
				{
					if ((TouchTapCount == 3) && (ievent.isDoubleTap == false) && (ievent.isTripleTap == false)) 
					{
						ievent.isSingleTap = true;
						singleTapTimes += 1;
						debugInputInfo.text = "Single tapped " + singleTapTimes + " times";
						multiTapStartTime = Time.time;
						tapRegister = 0;
						TouchTapCount = 0;
					}	
					if ((TouchTapCount == 6) && (ievent.isSingleTap == false) && (ievent.isTripleTap == false))
					{
						ievent.isDoubleTap = true;
						doubleTapTimes += 1;
						debugInputInfo.text = "Double tapped " + doubleTapTimes + " times";
						multiTapStartTime = Time.time;
						tapRegister = 0;
						TouchTapCount = 0;
					}
					if ((TouchTapCount == 9) && (ievent.isSingleTap == false) && (ievent.isDoubleTap == false))
					{
						ievent.isTripleTap = true;
						tripleTapTimes += 1;
						debugInputInfo.text = "Triple tapped " + tripleTapTimes + " times";
						multiTapStartTime = Time.time;
						tapRegister = 0;
						TouchTapCount = 0;
					}
				}
			}

			// Check for swipe.
			if ((tapRegister == 3) && (touchDuration < 1.0f) && (ievent.isSingleTap == false) && (ievent.isDoubleTap == false) && (ievent.isTripleTap == false) && (ievent.isRotate == false)) 
			{
				// If time passed since last swipe is equal to/past the swipe cooldown time.
				if ((Time.time - swipeGestStartTime) >= swipeGestCD) 
				{
					float x0 = touchEnd[0].x - touchStart[0].x;
					float y0 = touchEnd[0].y - touchStart[0].y;
					float x1 = touchEnd[1].x - touchStart[1].x;
					float y1 = touchEnd[1].y - touchStart[1].y;
					float x2 = touchEnd[2].x - touchStart[2].x;
					float y2 = touchEnd[2].y - touchStart[2].y;

					// Left or right swipe detected
					if ((Mathf.Abs(x0) > Mathf.Abs(y0)) && (Mathf.Abs(x0) >= minSwipeHorizontalDist) && (Mathf.Abs(x1) > Mathf.Abs(y1)) && (Mathf.Abs(x1) >= minSwipeHorizontalDist) && (Mathf.Abs(x2) > Mathf.Abs(y2)) && (Mathf.Abs(x2) >= minSwipeHorizontalDist))
					{
						// Right swipe detected
						if ((x0 > 0.0f) && (x1 > 0.0f) && (x2 > 0.0f))
						{
							ievent.isSwipe = true;
							ievent.isRight = true;
							swipeRightTimes += 1;
							debugInputInfo.text = "Swiped right " + swipeRightTimes + " times";
							swipeGestStartTime = Time.time;
							tapRegister = 0;
						}
						// Left swipe detected
						else if ((x0 < 0.0f) && (x1 < 0.0f) && (x2 < 0.0f))
						{
							ievent.isSwipe = true;
							ievent.isLeft = true;
							swipeLeftTimes += 1;
							debugInputInfo.text = "Swiped left " + swipeLeftTimes + " times";
							swipeGestStartTime = Time.time;
							tapRegister = 0;
						}
					}
					// Up or down swipe detected
					else if ((Mathf.Abs(y0) > Mathf.Abs(x0)) && (Mathf.Abs(y0) >= minSwipeVerticalDist) && (Mathf.Abs(y1) > Mathf.Abs(x1)) && (Mathf.Abs(y1) >= minSwipeVerticalDist) && (Mathf.Abs(y2) > Mathf.Abs(x2)) && (Mathf.Abs(y2) >= minSwipeVerticalDist))
					{
						// Up swipe detected
						if ((y0 > 0.0f) && (y1 > 0.0f) && (y2 > 0.0f))
						{
							ievent.isSwipe = true;
							ievent.isUp = true;
							swipeUpTimes += 1;
							debugInputInfo.text = "Swiped up " + swipeUpTimes + " times";
							swipeGestStartTime = Time.time;
							tapRegister = 0;
						}
						// Down swipe detected
						else if ((y0 < 0.0f) && (y1 < 0.0f) && (y2 < 0.0f))
						{
							ievent.isSwipe = true;
							ievent.isDown = true;
							swipeDownTimes += 1;
							debugInputInfo.text = "Swiped down " + swipeDownTimes + " times";
							swipeGestStartTime = Time.time;
							tapRegister = 0;
						}
					}	
				}
			}

			// Check for rotation.
			if ((tapRegister == 3) && (touchDuration < 1.0f) && (ievent.isSingleTap == false) && (ievent.isDoubleTap == false) && (ievent.isTripleTap == false) && (ievent.isSwipe == false))
			{
				// If time passed since last rotation is equal to/past the rotation cooldown time.
				if ((Time.time - rotateGestStartTime) >= rotateGestCD)
				{
					VecStart = vecStart1 - vecStart2;
					VecEnd = vecEnd1 - vecEnd2;
					Vector3 cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized);
					float crossPz = cross.z;

					// Left rotation detected.
					if ((crossPz >= 0) && (Mathf.Abs(crossPz) >= Screen.height*0.00015f))
					{
						ievent.isRotate = true;
						ievent.isLeft = true;
						rotateLeftTimes += 1;
						debugInputInfo.text = "Rotated left " + rotateLeftTimes + " times";
						rotateGestStartTime = Time.time;
						tapRegister = 0;
					}
					// Right rotation detected.
					else if ((crossPz < 0) && (Mathf.Abs(crossPz) >= Screen.height*0.00015f)) 
					{
						ievent.isRotate = true;
						ievent.isRight = true;
						rotateRightTimes += 1;
						debugInputInfo.text = "Rotated right " + rotateRightTimes + " times";
						rotateGestStartTime = Time.time;
						tapRegister = 0;
					}
				}
			}

			if ((tapRegister == 3) && (touchDuration > 0.0f) && (ievent.isSingleTap == false) && (ievent.isDoubleTap == false) && (ievent.isTripleTap == false) && (ievent.isSwipe == false) && (ievent.isRotate == false))
			{
				if (touchDuration >= 1.0f) {
					float x0 = touchEnd[0].x - touchStart[0].x;
					float y0 = touchEnd[0].y - touchStart[0].y;
					float x1 = touchEnd[1].x - touchStart[1].x;
					float y1 = touchEnd[1].y - touchStart[1].y;
					float x2 = touchEnd[2].x - touchStart[2].x;
					float y2 = touchEnd[2].y - touchStart[2].y;

					if ((Mathf.Abs(x0) > minHoldHorizontalDist) || (Mathf.Abs(x1) > minHoldHorizontalDist) || (Mathf.Abs(x2) > minHoldHorizontalDist) || (Mathf.Abs(y0) > minHoldVerticalDist) || (Mathf.Abs(y1) > minHoldVerticalDist) || (Mathf.Abs(y2) > minHoldVerticalDist))
					{
						ievent.isUnrecognized = true;
						debugInputInfo.text = "Unrecognized input";
						tapRegister = 0;
					}
				}
			}

			touchDuration = 0.0f;

			hasRegistered[0] = false;
			hasRegistered[1] = false;
			hasRegistered[2] = false;	
		}

		ievent.cumulativeTouchNum = TouchTapCount;
#endif
        //print(touchTime);
        //if no input, code should not reach here
        PassDataToListeners(ievent);
        NotifyLlisteners();
    }

    /// <summary>
    /// Sets an internal variable in all event handlers to notify them of the nature of an input event.
    /// </summary>
    void PassDataToListeners(InputEvent _ie)
    {
        foreach (eventHandler eh in listeners)
        {
			if (eh != null) 
			{
				eh.SetData(_ie);
			}
        }
    }

    /// <summary>
    /// Notifies all event handlers of the arrival of input data.
    /// </summary>
    void NotifyLlisteners()
    {
        foreach (eventHandler eh in listeners)
        {
			if (eh != null) 
			{
				eh.Activate();
			}
        }
    }

    /// <summary>
    /// Reinitializes all countdown timers under the control of this module.
    /// </summary>
    void ResetCDTimers()
    {
        foreach (CDTimer ctimer in cdtimers)
        {
			if (ctimer != null) 
			{
				ctimer.resetLock();
			}
        }
    }
}

/// <summary>
/// A class for handling input events with user specified actions.
/// </summary>
public class eventHandler
{
    bool hasNewEvent;
    InputEvent inputevetData;

    /// <summary>
    /// Constructs the event handler and also registers it to the given input module.
    /// </summary>
    /// <param name="Iinstance"></param>
    public eventHandler(InputModule Iinstance)
    {
        Iinstance.RegisterEventHandler(this);
    }

    /// <summary>
    /// Sets a flag to signal the fact that a new input event has been received.
    /// </summary>
    public void Activate()
    {
        hasNewEvent = true;
    }

    //this quary clears flag
    //Note: currently this function always return true for first qury of each frame
    //This is due to the gestures needed for this game
    //inputEvent has a function called hasEffectiveInput that actually does what this function does
    /// <summary>
    /// Determines whether a new input event has been received. As a quirk, the
    ///  the function returns true for the first query in a frame.
    /// </summary>
    public bool isActivate()
    {
        bool result = hasNewEvent;
        deActivate();
        return result;
    }

    /// <summary>
    /// Sets up the handler to be ready for a new input event.
    /// </summary>
    public void deActivate()
    {
        hasNewEvent = false;
    }

    /// <summary>
    /// A function to determine the nature of an input event.
    /// </summary>
    public InputEvent getEventData()
    {
        return inputevetData;
    }

    /// <summary>
    /// Sets the nature of the input event.
    /// </summary>
    public void SetData(InputEvent _ie)
    {
        inputevetData = _ie;
    }
}

//not only CD, but also lock operation per touch
/// <summary>
/// A count down timer which can be used to lock certain operations.
/// </summary>
public class CDTimer
{
    float startTime;
    float CD;
    bool isLock;

    public CDTimer(float _cd, InputModule Iinstance)
    {
        CD = _cd;
        isLock = false;
        Iinstance.RegisterCDTimer(this);
    }

    /// <summary>
    /// Sets the timer's start time to the current time.
    /// </summary>
    public void TakeDownTime()
    {
        startTime = Time.time;
    }

    /// <summary>
    /// Halts the timer and sets a lock if the time elapsed since activation
    ///  is longer than the countdown value.
    /// </summary>
    public bool CDfinish()
    {
        if ((Time.time - startTime >= CD) && !isLock)
        {
            isLock = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the timer's start time to the current time.
    /// </summary>
    public void reset()
    {
        //FIXME: This is code duplication.
        startTime = Time.time;
    }

    /// <summary>
    /// Undoes the lock state of the timer.
    /// </summary>
    public void resetLock()
    {
        isLock = false;
    }
}
