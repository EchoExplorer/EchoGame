﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// A struct containing a large number of fields about user input data. 
/// </summary>
public struct InputEvent
{
    public bool isRight; // If a swipe or rotation registered was to the right.
    public bool isLeft; // If a swipe or rotation registered was to the left.
    public bool isUp; // If a swipe or rotation registered was up.
    public bool isDown; // If a swipe or rotation registered was down.
    public int touchNum; 
    public int cumulativeTouchNum; // within a short period of time, how many time user touches
	public bool isTap; // If a tap was registered.
	public bool isHold; // If a hold was registered.
	public bool isSwipe; // If a swipe was registered.
    public bool isRotate; // If a rotation was registered.
    public bool isUnrecognized; // If fingers were touching the screen but a gesture was not recognized (i.e. there was a hold, but it moved too much from start to end).
    public KeyCode keycode; // Key that is pressed by the player if they are using a keyboard.
 	public float elapsedTime; // how long the user has hold

    /// <summary>
    /// Initializes all the fields to default values.
    /// </summary>
	public void init()
    {
        isRight = false; isLeft = false; isUp = false; isDown = false;
		isTap = false; isHold = false; isSwipe = false; isRotate = false; isUnrecognized = false; keycode = KeyCode.None;
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
        return isRight || isLeft || isUp || isDown || isTap || isHold || isSwipe || isRotate || isUnrecognized || (keycode != KeyCode.None) || (touchNum > 0) || (cumulativeTouchNum > 0);
    }
}

/// <summary>
/// A class that maintains event handlers for input events and countdown timers.
/// </summary>
public class InputModule : MonoBehaviour
{
    //consts
    const float TOUCH_TIME = 0.02f;
    float maxHoldVerticalDist = Screen.width * 0.04f; // The maximum vertical distance the player can move their fingers to register a gesture as a hold.
    float maxHoldHorizontalDist = Screen.height * 0.05f; // The maximum horizontal distance the player can move their fingers to register a gesture as a hold.
    float minSwipeVerticalDist = Screen.width * 0.06f; // The minimum vertical distance the player needs to move their fingers to register a gesture as a swipe.
	float minSwipeHorizontalDist = Screen.height * 0.08f; // The minimum horizontal distance the player needs to move their fingers to register a gesture as a swipe.
    float maxRotateVerticalDist = Screen.width * 0.5f; // The maximum vertical distance the player can move their fingers to register a gesture as a rotation.
    float maxRotateHorizontalDist = Screen.height * 0.75f; // The maximum horizontal distance the player can move their fingers to register a gesture as a rotation.
    float tapCD = 1.0f; // How long the player has to finish a tap once it has been started.
	float swipeGestCD = 1.0f; // How long the player has to finish a swipe once it has been started. 
	float rotateGestCD = 1.0f; // How long the player has to finish a rotation once it has been started.

	int TouchTapCount; // Used to determine how many taps have been registered (single, double, triple) in a given time period.
	float touchTime = 0.0f;
	float tapGestStartTime = 0.0f; // When a tap starts.
    float swipeGestStartTime = 0.0f; // When a swipe starts.
    float rotateGestStartTime = 0.0f; // When a rotation starts.   

    List<eventHandler> listeners = new List<eventHandler>();
    List<CDTimer> cdtimers = new List<CDTimer>();

	string debugInputInfo; // String for debugging what inputs the game has registered from the player.
	string debugTouch0Info; // String for debugging what is happening with touch0.
    string debugTouch1Info; // String for debugging what is happening with touch1.
	string debugTouch2Info; // String for debugging what is happening with touch2.
	string debugTouchDurationInfo; // String for debugging the duration the player has been holding a gesture.

	Vector2[] touchStart = {new Vector2(), new Vector2(), new Vector2()}; // Array for the start positions of the three touches.
	Vector2[] touchEnd = {new Vector2(), new Vector2(), new Vector2()}; // Array for the end positions of the three touches.
	Vector2 vecStart1 = new Vector2(); // Vector for the start position of touch0.
	Vector2 vecEnd1 = new Vector2(); // Vector for the end position of touch0.
	Vector2 vecStart2 = new Vector2(); // Vector for the start position of touch1.
	Vector2 vecEnd2 = new Vector2(); // Vector for the end position of touch1.
	Vector2 VecStart = new Vector2(); // Vector for the difference between the start position vectors of touch0 and touch1.
	Vector2 VecEnd = new Vector2(); // Vector for the difference between the end position vectors of touch0 and touch1.

	float touchDuration = 0.0f; // How long the player has been holding on the screen for. Used to determine the difference between a hold and a tap/swipe/rotation.
	int tapRegister = 0; // Used to determine how many fingers have left the screen after initial touches have been made. Gestures are only recognized if this is equal to 3.
	bool[] hasRegistered = {false, false, false}; // For some reason TouchPhase.Began does not seem to be recognized. This fills a similar purpose, determining if the touch has been on the screen during a frame or not.
	bool stillHolding = false; // For if the user makes a hold longer than 1.0f. Because on each update call isHold is set back to false, we don't want to have it change back to true unless the user has let go and held for another 1.0f.

    int tapTimes = 0; // Number of times the player has made a single tap. Helpful for debugging if multiple single taps are made in a row.
    int holdTimes = 0; // Number of times the player has made a hold. Helpful for debugging if multiple holds are made in a row.
    int swipeLeftTimes = 0; // Number of times the player has made a swipe left. Helpful for debugging if multiple swipe lefts are made in a row.
	int swipeRightTimes = 0; // Number of times the player has made a swipe right. Helpful for debugging if multiple swipe rights are made in a row.
	int swipeUpTimes = 0; // Number of times the player has made a swipe up. Helpful for debugging if multiple swipe ups are made in a row.
	int swipeDownTimes = 0; // Number of times the player has made a swipe down. Helpful for debugging if multiple swipe downs are made in a row.
	int rotateLeftTimes = 0; // Number of times the player has made a left rotation. Helpful for debugging if multiple left rotations are made in a row.
	int rotateRightTimes = 0; // Number of times the player has made a right rotation. Helpful for debugging if multiple right rotations are made in a row.
	int unrecognizedTimes = 0; // Number of times the player has made an unrecognized gesture. Helpful for debugging if multiple unrecognized gestures are made in a row.

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

    // Use for initialization.
    void Start()
    {

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
		// If the right arrow key was pressed, set the keycode to the right arrow.
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            swipeRightTimes += 1; // Update the number of times the right arrow key has been pressed.
            debugInputInfo = "Right arrow key pressed " + swipeRightTimes + " times."; 
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
            ievent.keycode = KeyCode.RightArrow;
        }
		// If the left arrow key was pressed, set the keycode to the left arrow.
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            swipeLeftTimes += 1; // Update the number of times the left arrow key has been pressed.
            debugInputInfo = "Left arrow key pressed " + swipeLeftTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
            ievent.keycode = KeyCode.LeftArrow;
        }
		// If the up arrow key was pressed, set the keycode to the up arrow.
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            swipeUpTimes += 1; // Update the number of times the up arrow key has been pressed.
            debugInputInfo = "Up arrow key pressed " + swipeUpTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
            ievent.keycode = KeyCode.UpArrow;
        }
		// If the down arrow key was pressed, set the keycode to the down arrow.
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            swipeDownTimes += 1; // Update the number of times the down arrow key has been pressed.
            debugInputInfo = "Down arrow key pressed " + swipeDownTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
            ievent.keycode = KeyCode.DownArrow;
        }
		// If the 'f' key was pressed, set the keycode to 'f'.
        else if (Input.GetKeyUp("f"))
        {
            tapTimes += 1; // Update the number of times the 'f' key has been pressed.
            debugInputInfo = "F key pressed " + tapTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
            ievent.keycode = KeyCode.F;
        }
		// If the 'r' key was pressed, set the keycode to 'r'.
        else if (Input.GetKeyUp("r"))
        {
            holdTimes += 1; // Update the number of times the 'r' key has been pressed.
            debugInputInfo = "R key pressed " + holdTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
            ievent.keycode = KeyCode.R;
        }
        // If the 'p' key was pressed, set the keycode to 'p'.
		else if (Input.GetKeyUp("p"))
		{
			debugInputInfo = "P key pressed."; 
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
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

		// Go through each touch currently on the screen.
		foreach(Touch thisTouch in Input.touches) 
		{
			// If the touch is the first finger to touch the screen.
			if (thisTouch.fingerId == 0)
			{
				// If the touch has not been registered yet.
				if (hasRegistered[0] == false) 
				{
					touchStart[0] = thisTouch.position; // Set the start position of this touch.
					touchEnd[0] = thisTouch.position; // The end position of this touch should currently be the same as the start position.
					vecStart1 = thisTouch.position; // Set the start position vector of this touch.
					vecEnd1 = thisTouch.position; // The end position vector of this touch should currently be the same as the start position vector.
					tapRegister = 0; // A new series of touches is being registered, so set tapRegister back to 0.
					hasRegistered[0] = true; // The touch has now been registered.
				}
				// If the touch is currently stationary and has been registered.
				else if ((thisTouch.phase == TouchPhase.Stationary) && (hasRegistered[0] == true)) 
				{
					touchEnd[0] = thisTouch.position; // Update the end position of this touch. Not necessary as it should not have changed, but just for safety.
					vecEnd1 = thisTouch.position; // Update the end position vector of this touch. Not necessary as it should not have changed, but just for safety.
				}
				// If the touch has moved and has been registered.
				else if ((thisTouch.phase == TouchPhase.Moved) && (hasRegistered[0] == true)) 
				{
					touchEnd[0] = thisTouch.position; // Update the end position of this touch.
					vecEnd1 = thisTouch.position; // Update the end position vector of this touch.
				}
				// If the touch ended and has been registered.
				else if ((thisTouch.phase == TouchPhase.Ended) && (hasRegistered[0] == true)) 
				{
					tapRegister += 1; // Update the number of touches that have left the screen.
					touchEnd[0] = thisTouch.position; // Update the end position of this touch.
					vecEnd1 = thisTouch.position; // Update the end position vector of this touch.

					// If there is still time to register a tap.
					if ((Time.time - tapGestStartTime) < tapCD) 
					{
                        // If three fingers have left the screen and there has not been a tap in this time, increment TouchTapCount by three.
                        if ((tapRegister == 3) && (TouchTapCount < 3)) 
						{   
                            TouchTapCount += tapRegister;                            
                        }
						ievent.elapsedTime = Time.time - touchTime;
					}
				}
				// If the touch was canceled for some reason.
				else if (thisTouch.phase == TouchPhase.Canceled) 
				{
					debugTouch0Info = "touch0 canceled";
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                } 
				// Some weird error occured.
				else 
				{
					debugTouch0Info = "Cannot compute";
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                }
			}
			// If the touch is the second finger to touch the screen.
			else if (thisTouch.fingerId == 1) 
			{
				// If the touch has not been registered yet.
				if (hasRegistered[1] == false) 
				{
					touchStart[1] = thisTouch.position; // Set the start position of this touch.
					touchEnd[1] = thisTouch.position; // The end position of this touch should currently be the same as the start position.
					vecStart2 = thisTouch.position; // Set the start position vector of this touch.
					vecEnd2 = thisTouch.position; // The end position vector of this touch should currently be the same as the start position vector.
					hasRegistered[1] = true; // The touch has now been registered.
				}
				// If the touch is currently stationary and has been registered.
				else if ((thisTouch.phase == TouchPhase.Stationary) && (hasRegistered[1] == true)) 
				{
					touchEnd[1] = thisTouch.position; // Update the end position of this touch. Not necessary as it should not have changed, but just for safety.
					vecEnd2 = thisTouch.position; // Update the end position vector of this touch. Not necessary as it should not have changed, but just for safety.
				}
				// If the touch has moved and has been registered.
				else if ((thisTouch.phase == TouchPhase.Moved) && (hasRegistered[1] == true)) 
				{
					touchEnd[1] = thisTouch.position; // Update the end position of this touch.
					vecEnd2 = thisTouch.position; // Update the end position vector of this touch.
				}
				// If the touch ended and has been registered.
				else if ((thisTouch.phase == TouchPhase.Ended) && (hasRegistered[1] == true)) 
				{
					tapRegister += 1; // Update the number of touches that have left the screen.
					touchEnd[1] = thisTouch.position; // Update the end position of this touch.
					vecEnd2 = thisTouch.position; // Update the end position vector of this touch.

                    // If there is still time to register a tap.
                    if ((Time.time - tapGestStartTime) < tapCD)
                    {
                        // If three fingers have left the screen and there has not been a tap in this time, increment TouchTapCount by three.
                        if ((tapRegister == 3) && (TouchTapCount < 3))
                        {
                            TouchTapCount += tapRegister;
                        }
                        ievent.elapsedTime = Time.time - touchTime;
                    }
                }
				// If the touch was canceled for some reason.
				else if (thisTouch.phase == TouchPhase.Canceled) 
				{
					debugTouch1Info = "touch1 canceled"; 
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                }
				// Some weird error occured.
				else 
				{
					debugTouch1Info = "Cannot compute";
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                }
			}
			// If the touch is the third finger to touch the screen.
			else if (thisTouch.fingerId == 2) 
			{
                // If the touch has not been registered yet.
                if (hasRegistered[2] == false)
                {
                    touchStart[2] = thisTouch.position; // Set the start position of this touch.
                    touchEnd[2] = thisTouch.position; // The end position of this touch should currently be the same as the start position.         
                    hasRegistered[2] = true; // The touch has now been registered.

                    // If a tap can start now, set the start time to now and set the TouchTapCount back to 0.
                    if ((Time.time - tapGestStartTime) >= tapCD)
                    {
                        tapGestStartTime = Time.time;
                        TouchTapCount = 0;
                    }

					touchTime = Time.time; // Set the touch time to now.
					ResetCDTimers(); // Reset the CD timers.
				}
				// If the touch is currently stationary and has been registered.
				else if ((thisTouch.phase == TouchPhase.Stationary) && (hasRegistered[2] == true)) 
				{
					touchEnd[2] = thisTouch.position; // Update the end position of this touch. Not necessary as it should not have changed, but just for safety.
				}
				// If the touch has moved and has been registered.
				else if ((thisTouch.phase == TouchPhase.Moved) && (hasRegistered[2] == true)) 
				{
					touchEnd[2] = thisTouch.position; // Update the end position of this touch.					
				}
				// If the touch ended and has been registered.
				else if ((thisTouch.phase == TouchPhase.Ended) && (hasRegistered[2] == true)) 
				{
					tapRegister += 1; // Update the number of touches that have left the screen.
					touchEnd[2] = thisTouch.position; // Update the end position of this touch.

                    // If there is still time to register a tap.
                    if ((Time.time - tapGestStartTime) < tapCD)
                    {
                        // If three fingers have left the screen and there has not been a tap in this time, increment TouchTapCount by three.
                        if ((tapRegister == 3) && (TouchTapCount < 3))
                        {
                            TouchTapCount += tapRegister;
                        }
                        ievent.elapsedTime = Time.time - touchTime;
                    }
                }
				// If the touch was canceled for some reason.
				else if (thisTouch.phase == TouchPhase.Canceled) 
				{
					debugTouch2Info = "touch2 canceled";
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.
                }
				// Some weird error occured.
				else 
				{
                    debugTouch2Info = "Cannot compute";
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.
                }
            }

			ievent.elapsedTime = Time.time - touchTime;
		}

		// If there is at least one touch currently on the screen.
		if (Input.touchCount > 0) 
		{
			// If there are currently three touches on the screen.
			if (Input.touchCount == 3) 
			{
				touchDuration = touchDuration + Time.deltaTime; // Update the length of the touch.
				debugTouchDurationInfo = "Hold: " + touchDuration;
                DebugTouchDuration.instance.ChangeDebugTouchDurationText(debugTouchDurationInfo); // Update the debug textbox.

                // If the touch was long enough.
                if (touchDuration >= 1.0f) 
				{
                    stillHolding = true; // Make sure isHold does not get triggered again if they continue to hold for another 1.0f.
                }
			}	
		}

		// If there are currently no fingers on the screen, determine if a tap/swipe/rotation gesture was made.
		if (Input.touchCount == 0) 
		{
            if (tapRegister == 3)
            {
                // Get the distance between the start and end positions for each touch in x and y coordinates.
                float x0 = touchEnd[0].x - touchStart[0].x;
                float y0 = touchEnd[0].y - touchStart[0].y;
                float x1 = touchEnd[1].x - touchStart[1].x;
                float y1 = touchEnd[1].y - touchStart[1].y;
                float x2 = touchEnd[2].x - touchStart[2].x;
                float y2 = touchEnd[2].y - touchStart[2].y;

                debugTouch0Info = "Xdiff: " + x0 + "\n" + "Ydiff: " + y0;
                DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                debugTouch1Info = "Xdiff: " + x1 + "\n" + "Ydiff: " + y1;
                DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                debugTouch2Info = "Xdiff: " + x2 + "\n" + "Ydiff: " + y2;
                DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.

                VecStart = vecStart1 - vecStart2; // Get the vector between the start positions of touch0 and touch1.
                VecEnd = vecEnd1 - vecEnd2; // Get the vector between the end positions of touch0 and touch1.
                float angle = Vector2.Angle(VecStart, VecEnd);
                Vector3 cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized); // Get the cross product between the two vectors.
                float crossPz = cross.z; // Get the z-component of the cross product.

                // If the gesture could be a tap.
                if ((touchDuration < 1.0f) && (TouchTapCount == 3) && (Mathf.Abs(x0) < minSwipeHorizontalDist) && (Mathf.Abs(y0) < minSwipeVerticalDist) && (Mathf.Abs(x1) < minSwipeHorizontalDist) && (Mathf.Abs(y1) < minSwipeVerticalDist) && (Mathf.Abs(x2) < minSwipeHorizontalDist) && (Mathf.Abs(y2) < minSwipeVerticalDist) && (angle < 30))
                {
                    // If time passed since last tap is equal to/past the tap cooldown time and another gesture has not been registered already for this update. 
                    if (((Time.time - tapGestStartTime) >= tapCD) && (ievent.isTap == false) && (ievent.isHold == false) && (ievent.isSwipe == false) && (ievent.isRotate == false) && (ievent.isUnrecognized == false))
                    {
                        ievent.isTap = true; // A tap was registered.
                        tapTimes += 1; // Update the number of times a tap was made.
                        debugInputInfo = "Tapped " + tapTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        tapGestStartTime = Time.time; // Set the start time to now.
                        tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.
                    }
                }

                // If the gesture could be a swipe left or right.
                else if ((touchDuration < 1.0f) && (Mathf.Abs(x0) > Mathf.Abs(y0)) && (Mathf.Abs(x0) >= minSwipeHorizontalDist) && (Mathf.Abs(x1) > Mathf.Abs(y1)) && (Mathf.Abs(x1) >= minSwipeHorizontalDist) && (Mathf.Abs(x2) > Mathf.Abs(y2)) && (Mathf.Abs(x2) >= minSwipeHorizontalDist) && (angle <= 30))
                {
                    // If time passed since last swipe is equal to/past the swipe cooldown time and another gesture has not been registered already for this update. 
                    if (((Time.time - swipeGestStartTime) >= swipeGestCD) && (ievent.isTap == false) && (ievent.isHold == false) && (ievent.isSwipe == false) && (ievent.isRotate == false) && (ievent.isUnrecognized == false))
                    {
                        // Swipe right detected.
                        if ((x0 > 0.0f) && (x1 > 0.0f) && (x2 > 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isRight = true; // The swipe was right.
                            swipeRightTimes += 1; // Update the number of times a swipe right was made.
                            debugInputInfo = "Swiped right " + swipeRightTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                            swipeGestStartTime = Time.time; // Set the end time to now.
                            tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                            TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.
                        }
                        // Swipe left detected.
                        else if ((x0 < 0.0f) && (x1 < 0.0f) && (x2 < 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isLeft = true; // The swipe was left.
                            swipeLeftTimes += 1; // Update the number of times a swipe left was made.
                            debugInputInfo = "Swiped left " + swipeLeftTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                            swipeGestStartTime = Time.time; // Set the end time to now.
                            tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                            TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.
                        }
                    }
                }
                // If the gesture could be a swipe up or down.
                else if ((touchDuration < 1.0f) && (Mathf.Abs(y0) > Mathf.Abs(x0)) && (Mathf.Abs(y0) >= minSwipeVerticalDist) && (Mathf.Abs(y1) > Mathf.Abs(x1)) && (Mathf.Abs(y1) >= minSwipeVerticalDist) && (Mathf.Abs(y2) > Mathf.Abs(x2)) && (Mathf.Abs(y2) >= minSwipeVerticalDist) && (angle <= 30))
                {
                    // If time passed since last swipe is equal to/past the swipe cooldown time and another gesture has not been registered already for this update. 
                    if (((Time.time - swipeGestStartTime) >= swipeGestCD) && (ievent.isTap == false) && (ievent.isHold == false) && (ievent.isSwipe == false) && (ievent.isRotate == false) && (ievent.isUnrecognized == false))
                    {
                        // Swipe up detected.
                        if ((y0 > 0.0f) && (y1 > 0.0f) && (y2 > 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isUp = true; // The swipe was up.
                            swipeUpTimes += 1; // Update the number of times a swipe up was made.
                            debugInputInfo = "Swiped up " + swipeUpTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                            swipeGestStartTime = Time.time; // Set the end time to now.
                            tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                            TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.
                        }
                        // Swipe down detected.
                        else if ((y0 < 0.0f) && (y1 < 0.0f) && (y2 < 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isDown = true; // The swipe was down.
                            swipeDownTimes += 1; // Update the number of times a swipe down was made.
                            debugInputInfo = "Swiped down " + swipeDownTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                            swipeGestStartTime = Time.time; // Set the end time to now.
                            tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                            TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.
                        }
                    }
                }

                // If the gesture could be a left rotation.
                else if ((touchDuration < 1.0f) && (crossPz >= 0) && (angle >= 45))
                {
                    // If time passed since last rotation is equal to/past the rotation cooldown time and another gesture has not been registered already for this update. 
                    if (((Time.time - rotateGestStartTime) >= rotateGestCD) && (ievent.isTap == false) && (ievent.isHold == false) && (ievent.isSwipe == false) && (ievent.isRotate == false) && (ievent.isUnrecognized == false))
                    {
                        ievent.isRotate = true; // A rotation was detected.
                        ievent.isLeft = true; // The rotation was left.
                        rotateLeftTimes += 1; // Update the number of times a rotation left was made.
                        debugInputInfo = "Rotated left " + rotateLeftTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        rotateGestStartTime = Time.time; // Set the end time to now.
                        tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.
                    }
                }
                // If the gesture could be right rotation.
                else if ((touchDuration < 1.0f) && (crossPz < 0) && (angle >= 45))
                {
                    // If time passed since last rotation is equal to/past the rotation cooldown time and another gesture has not been registered already for this update. 
                    if (((Time.time - rotateGestStartTime) >= rotateGestCD) && (ievent.isTap == false) && (ievent.isHold == false) && (ievent.isSwipe == false) && (ievent.isRotate == false) && (ievent.isUnrecognized == false))
                    {
                        ievent.isRotate = true; // A rotation was detected.
                        ievent.isRight = true; // The rotation was right.
                        rotateRightTimes += 1; // Update the number of times a rotation right was made.
                        debugInputInfo = "Rotated right " + rotateRightTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        rotateGestStartTime = Time.time; // Set the end time to now.
                        tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.
                    }
                }

                // If the gesture could be a hold.
                else if ((touchDuration >= 1.0f) && (Mathf.Abs(x0) <= maxHoldHorizontalDist) && (Mathf.Abs(x1) <= maxHoldHorizontalDist) && (Mathf.Abs(x2) <= maxHoldHorizontalDist) && (Mathf.Abs(y0) <= maxHoldVerticalDist) && (Mathf.Abs(y1) <= maxHoldVerticalDist) && (Mathf.Abs(y2) <= maxHoldVerticalDist) && (angle <= 30))
                {
                    if ((stillHolding == true) && (ievent.isTap == false) && (ievent.isHold == false) && (ievent.isSwipe == false) && (ievent.isRotate == false) && (ievent.isUnrecognized == false))
                    {
                        ievent.isHold = true; // A hold has been registered.
                        holdTimes += 1; // Update the number of times a hold was made.
                        debugInputInfo = "Hold made " + holdTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                        stillHolding = false; // The player is no longer making a hold.
                        tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.
                    }
                }

                // If another gesture has not been registered already for this update.
                else if ((ievent.isTap == false) && (stillHolding == false) && (ievent.isSwipe == false) && (ievent.isRotate == false) && (ievent.isUnrecognized == false))
                {
                    ievent.isUnrecognized = true; // There was an unrecognized gesture.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture has been made.
                    debugInputInfo = "Unrecognized input has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.               
                    tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.                   
                }                                
            }

            // If there was an error with one of the fingers and it did not end correctly.
            if (tapRegister < 3)
            {
                // If another gesture has not been registered already for this update.
                if ((ievent.isTap == false) && (stillHolding == false) && (ievent.isSwipe == false) && (ievent.isRotate == false) && (ievent.isUnrecognized == false))
                {
                    ievent.isUnrecognized = true; // There was an unrecognized gesture.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture has been made.
                    debugInputInfo = "Unrecognized input has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.               
                    tapRegister = 0; // Reset the tapRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    TouchTapCount = 0; // Reset the TouchTapCount just to make sure that more than one tap is not recognized.                   
                }
            }

            touchDuration = 0.0f; // Reset duration of touch duration to 0, as nothing is touch the screen.          
            stillHolding = false; // Let the player register another hold.

            hasRegistered[0] = false; // Touch0 is no longer on the screen. Make sure it is not registered for the next time it touches so that its start position can be obtained.
			hasRegistered[1] = false; // Touch1 is no longer on the screen. Make sure it is not registered for the next time it touches so that its start position can be obtained.
			hasRegistered[2] = false; // Touch2 is no longer on the screen. Make sure it is not registered for the next time it touches so that its start position can be obtained.
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
