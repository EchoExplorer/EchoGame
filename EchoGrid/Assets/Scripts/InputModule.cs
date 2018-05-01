using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public bool isTap; // If a tap was registered.
    public bool isHold; // If a hold was registered.
    public bool isSwipe; // If a swipe was registered.
    public bool isRotate; // If a rotation was registered.
    public bool isMain; // For when the 'p' key is pressed, which sends you straight to the main menu.
    public bool isUnrecognized; // If fingers were touching the screen but a gesture was not recognized (i.e. there was a hold, but it moved too much from start to end).
    public bool isTapHorizontalError; // If the player moved too much horizontally during the tap.
    public bool isTapVerticalError; // If the player moved too much vertically during the tap.
    public bool isTapRotationError; // If the player rotated too much during the tap.
    public bool isSwipeLeftHorizontalError; // If the player did not move enough horizontally during the swipe left.
    public bool isSwipeRightHorizontalError; // If the player did not move enough horizontally during the swipe right.
    public bool isSwipeUpVerticalError; // If the player did not move enough vertically during the swipe up.
    public bool isSwipeDownVerticalError; // If the player did not move enough vertically during the swipe down.
    public bool isSwipeLeftRotationError; // If the player rotated too much during the swipe left.
    public bool isSwipeRightRotationError; // If the player rotated too much during the swipe right.
    public bool isSwipeUpRotationError; // If the player rotated too much during the swipe up.
    public bool isSwipeDownRotationError; // If the player rotated too much during the swipe down.
    public bool isRotationAngleError; // If the player did not rotate far enough during the rotation.
    public bool isHoldRotationError; // If the player rotated too much during the hold.
    public bool isHoldHorizontalError; // If the player moved too much horizontally during the hold.
    public bool isHoldVerticalError; // If the player moved too much vertically during the hold.
    public bool isLessThanTwoError; // If the player had only one finger on the screen and is not using Talkback.
    public bool isMoreThanTwoError; // If the player had more than two fingers on the screen and is not using Talkback.
    public bool isLessThanThreeError; // If the player had only one or two fingers on the screen and is using Talkback.
    public bool isMoreThanThreeError; // If the player had more than three fingers on the screen and is using Talkback.
    public KeyCode keycode; // Key that is pressed by the player if they are using a keyboard.
    public float elapsedTime; // how long the user has hold

    /// <summary>
    /// Initializes all the fields to default values.
    /// </summary>
	public void init()
    {
        isRight = false; isLeft = false; isUp = false; isDown = false;
        isTap = false; isHold = false; isSwipe = false; isRotate = false; isMain = false; isUnrecognized = false; keycode = KeyCode.None;
        isTapHorizontalError = false; isTapVerticalError = false; isTapRotationError = false;
        isSwipeLeftHorizontalError = false; isSwipeLeftRotationError = false; isSwipeRightHorizontalError = false; isSwipeRightRotationError = false;
        isSwipeUpVerticalError = false; isSwipeUpRotationError = false; isSwipeDownVerticalError = false; isSwipeDownRotationError = false;
        isRotationAngleError = false; isHoldHorizontalError = false; isHoldVerticalError = false; isHoldRotationError = false;
        elapsedTime = 0.0f;
        touchNum = 0;
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
        return isRight || isLeft || isUp || isDown || isTap || isHold || isSwipe || isRotate || isUnrecognized || (keycode != KeyCode.None) || (touchNum > 0);
    }
}

/// <summary>
/// A class that maintains event handlers for input events and countdown timers.
/// </summary>
public class InputModule : MonoBehaviour
{
    //consts
    float maxTapHorizontalDist = Screen.width * 0.06f;  // The maximum horizontal distance the player can move their fingers to register a gesture as a tap.
    float maxTapVerticalDist = Screen.height * 0.07f; // The maximum vertical distance the player can move their fingers to register a gesture as a tap.
    float maxHoldHorizontalDist = Screen.width * 0.06f; // The maximum horizontal distance the player can move their fingers to register a gesture as a hold.
    float maxHoldVerticalDist = Screen.height * 0.07f; // The maximum vertical distance the player can move their fingers to register a gesture as a hold.   
    float minSwipeHorizontalDist = Screen.width * 0.10f; // The minimum horizontal distance the player needs to move their fingers to register a gesture as a swipe.
    float minSwipeVerticalDist = Screen.height * 0.12f; // The minimum vertical distance the player needs to move their fingers to register a gesture as a swipe.
    float minRotateHorizontalDist = Screen.width * 0.06f; // The maximum horizontal distance the player needs to move their fingers to register a gesture as a rotation.
    float minRotateVerticalDist = Screen.height * 0.15f; // The minimum vertical distance the player needs to move their fingers to register a gesture as a rotation.

    List<eventHandler> listeners = new List<eventHandler>();
    List<CDTimer> cdtimers = new List<CDTimer>();

    string debugInputInfo; // String for debugging what inputs the game has registered from the player.
    string debugTouch0Info; // String for debugging what is happening with touch0.
    string debugTouch1Info; // String for debugging what is happening with touch1.
    string debugTouch2Info; // String for debugging what is happening with touch2.
    string debugTouchDurationInfo; // String for debugging the duration the player has been holding a gesture.

    Vector2[] touchStart = { new Vector2(), new Vector2(), new Vector2() }; // Array for the start positions of the three touches.
    Vector2[] touchEnd = { new Vector2(), new Vector2(), new Vector2() }; // Array for the end positions of the three touches.
    Vector2 vecStart1 = new Vector2(); // Vector for the start position of touch0.
    Vector2 vecEnd1 = new Vector2(); // Vector for the end position of touch0.
    Vector2 vecStart2 = new Vector2(); // Vector for the start position of touch1.
    Vector2 vecEnd2 = new Vector2(); // Vector for the end position of touch1.
    Vector2 VecStart = new Vector2(); // Vector for the difference between the start position vectors of touch0 and touch1.
    Vector2 VecEnd = new Vector2(); // Vector for the difference between the end position vectors of touch0 and touch1.
    float x0;
    float x1;
    float x2;
    float y0;
    float y1;
    float y2;
    float angle;
    Vector3 cross;
    float crossPz;

    float gestureStartTime = 0.0f;
    float gestureStopTime = 0.0f;
    float touchDuration = 0.0f; // How long the player has been holding on the screen for. Used to determine the difference between a hold and a tap/swipe/rotation.
    int touchRegister = 0; // Used to determine how many fingers have left the screen after initial touches have been made. Gestures are only recognized if this is equal to 3.
    bool[] hasRegistered = { false, false, false }; // For some reason TouchPhase.Began does not seem to be recognized. This fills a similar purpose, determining if the touch has been on the screen during a frame or not.
    bool stillHolding = false; // For if the user makes a hold longer than 1.0f. Because on each update call isHold is set back to false, we don't want to have it change back to true unless the user has let go and held for another 1.0f.

    public bool clipWasPaused = false; // For when we input a gesture and it has to pause a currently playing instruction.
    public AudioClip pausedClip; // The clip that was paused.
    public float pausedTime; // Tells us when to resume the paused clip.

    int tapTimes = 0; // Number of times the player has made a single tap. Helpful for debugging if multiple single taps are made in a row.
    int holdTimes = 0; // Number of times the player has made a hold. Helpful for debugging if multiple holds are made in a row.
    int swipeLeftTimes = 0; // Number of times the player has made a swipe left. Helpful for debugging if multiple swipe lefts are made in a row.
    int swipeRightTimes = 0; // Number of times the player has made a swipe right. Helpful for debugging if multiple swipe rights are made in a row.
    int swipeUpTimes = 0; // Number of times the player has made a swipe up. Helpful for debugging if multiple swipe ups are made in a row.
    int swipeDownTimes = 0; // Number of times the player has made a swipe down. Helpful for debugging if multiple swipe downs are made in a row.
    int rotateLeftTimes = 0; // Number of times the player has made a left rotation. Helpful for debugging if multiple left rotations are made in a row.
    int rotateRightTimes = 0; // Number of times the player has made a right rotation. Helpful for debugging if multiple right rotations are made in a row.
    int mainTimes = 0; // Number of times the player has gone to the main menu directly by pressing the 'p' key. Helpful for debugging if the key is pressed multiple times in a row.
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
        Scene activeScene = SceneManager.GetActiveScene(); // Gets the active scene.

        if (Input.GetKeyUp(KeyCode.RightArrow) == true)
        {
            // For swipe rights.
            if (activeScene.name.Equals("Agreement") || activeScene.name.Equals("Main_pre") || activeScene.name.Equals("T&C") || activeScene.name.Equals("Title_Screen") || ((activeScene.name.Equals("Main") && (Player.want_exit == true))))
            {
                ievent.isSwipe = true; // A swipe was registered.
                ievent.isRight = true; // Register a right rotation.
                swipeRightTimes += 1; // Update the number of times the right arrow key has been pressed by swiping right.
                int totalRightTimes = swipeRightTimes + rotateRightTimes; // Get the total number of times the right arrow key has been pressed.
                debugInputInfo = "Right arrow key pressed " + totalRightTimes + " times.";
            }
            // For right rotations.
            else if (activeScene.name.Equals("Main") && Player.want_exit == false)
            {
                ievent.isRotate = true; // A rotation was registered.
                ievent.isRight = true; // Register a right rotation.
                rotateRightTimes += 1; // Update the number of times the right arrow key has been pressed by rotating right.
                int totalRightTimes = swipeRightTimes + rotateRightTimes; // Get the total number of times the right arrow key has been pressed.
                debugInputInfo = "Right arrow key pressed " + totalRightTimes + " times.";
            }
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) == true)
        {
            // For swipe lefts.
            if (activeScene.name.Equals("Agreement") || activeScene.name.Equals("Main_pre") || activeScene.name.Equals("T&C") || activeScene.name.Equals("Title_Screen") || ((activeScene.name.Equals("Main") && (Player.want_exit == true))))
            {
                ievent.isSwipe = true; // A swipe was registered.
                ievent.isLeft = true; // Register a left rotation.
                swipeLeftTimes += 1; // Update the number of times the left arrow key has been pressed by swiping left.
                int totalLeftTimes = swipeLeftTimes + rotateLeftTimes; // Get the total number of times the left arrow key has been pressed.
                debugInputInfo = "Left arrow key pressed " + totalLeftTimes + " times.";
            }
            // For left rotations.
            else if (activeScene.name.Equals("Main") && Player.want_exit == false)
            {
                ievent.isRotate = true; // A rotation was registered.
                ievent.isLeft = true; // Register a left rotation.
                rotateLeftTimes += 1; // Update the number of times the left arrow key has been pressed by rotating left.
                int totalLeftTimes = swipeLeftTimes + rotateLeftTimes; // Get the total number of times the left arrow key has been pressed.
                debugInputInfo = "Left arrow key pressed " + totalLeftTimes + " times.";
            }
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow) == true)
        {
            ievent.isSwipe = true; // A swipe was registered.
            ievent.isUp = true; // Register a swipe up.
            swipeUpTimes += 1; // Update the number of times the up arrow key has been pressed.
            debugInputInfo = "Up arrow key pressed " + swipeUpTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) == true)
        {
            ievent.isSwipe = true; // A swipe was registered.
            ievent.isDown = true; // Register a swipe down.
            swipeDownTimes += 1; // Update the number of times the down arrow key has been pressed.
            debugInputInfo = "Down arrow key pressed " + swipeDownTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                
        }
        else if (Input.GetKeyUp(KeyCode.F) == true)
        {
            ievent.isTap = true; // A tap was registered.
            tapTimes += 1; // Update the number of times the 'f' key has been pressed.
            debugInputInfo = "F key pressed " + tapTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                
        }
        else if (Input.GetKeyUp(KeyCode.R) == true)
        {
            ievent.isHold = true; // A hold was registered.
            holdTimes += 1; // Update the number of times the 'r' key has been pressed.
            debugInputInfo = "R key pressed " + holdTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                              
        }
        // If the 'p' key was pressed.
        else if (Input.GetKeyUp(KeyCode.P) == true)
        {
            ievent.isMain = true; // The 'p' key was registered.
            mainTimes += 1; // Update the number of times the 'p' key has been pressed.
            debugInputInfo = "P key pressed " + mainTimes + " times.";
            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.            
        }
#endif
#if UNITY_IOS || UNITY_ANDROID
        //Check if Input has registered more than zero touches
        int numTouches = Input.touchCount;
		ievent.touchNum = numTouches;
       
        // Go through each touch currently on the screen.
        foreach (Touch thisTouch in Input.touches) 
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
					touchRegister = 0; // A new series of touches is being registered, so set touchRegister back to 0.
                    hasRegistered[0] = true; // The touch has now been registered.
				}
				// If the touch is currently stationary and has been registered.
				else if ((thisTouch.phase == TouchPhase.Stationary) && (hasRegistered[0] == true)) 
				{
					touchEnd[0] = thisTouch.position; // Update the end position of this touch. Not necessary as it should not have changed, but just for safety.
					vecEnd1 = thisTouch.position; // Update the end position vector of this touch. Not necessary as it should not have changed, but just for safety.
				}				
				// If the touch ended and has been registered.
				else if ((thisTouch.phase == TouchPhase.Ended) && (hasRegistered[0] == true)) 
				{
					touchRegister += 1; // Update the number of touches that have left the screen.
					touchEnd[0] = thisTouch.position; // Update the end position of this touch.
					vecEnd1 = thisTouch.position; // Update the end position vector of this touch.
                }
				// If the touch was canceled for some reason.
				else if (thisTouch.phase == TouchPhase.Canceled) 
				{
					debugTouch0Info = "touch0 canceled";
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                } 
				// Some weird error occured.
				else if (thisTouch.phase != TouchPhase.Moved)
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

                    if ((GM_title.determined_talkback == false) || (GM_title.isUsingTalkback == false))
                    {
                        gestureStartTime = Time.time; // Set the start time of this gesture to now.
                        ResetCDTimers(); // Reset the CD timers.
                    }
                }
				// If the touch is currently stationary and has been registered.
				else if ((thisTouch.phase == TouchPhase.Stationary) && (hasRegistered[1] == true)) 
				{
					touchEnd[1] = thisTouch.position; // Update the end position of this touch. Not necessary as it should not have changed, but just for safety.
					vecEnd2 = thisTouch.position; // Update the end position vector of this touch. Not necessary as it should not have changed, but just for safety.
				}				
				// If the touch ended and has been registered.
				else if ((thisTouch.phase == TouchPhase.Ended) && (hasRegistered[1] == true)) 
				{
                    touchRegister += 1; // Update the number of touches that have left the screen.
					touchEnd[1] = thisTouch.position; // Update the end position of this touch.
					vecEnd2 = thisTouch.position; // Update the end position vector of this touch.
                }
				// If the touch was canceled for some reason.
				else if (thisTouch.phase == TouchPhase.Canceled) 
				{
					debugTouch1Info = "touch1 canceled"; 
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                }
				// Some weird error occured.
				else if (thisTouch.phase != TouchPhase.Moved)
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

                    if ((GM_title.determined_talkback == true) && (GM_title.isUsingTalkback == true))
                    {
                        gestureStartTime = Time.time; // Set the start time of this gesture to now.
                        ResetCDTimers(); // Reset the CD timers.
                    }                    
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
                    touchRegister += 1; // Update the number of touches that have left the screen.
					touchEnd[2] = thisTouch.position; // Update the end position of this touch.
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
		}

        // If there is at least one touch currently on the screen.
        if (Input.touchCount > 0)
        {
            // IF the player has not told us if they are using Talkback or not, accept two and three finger inputs.
            if (((Input.touchCount == 2) || (Input.touchCount == 3)) && (GM_title.determined_talkback == false))
            {
                touchDuration = touchDuration + Time.deltaTime; // Update the length of the touch.

                debugTouch0Info = "XStart: " + touchStart[0].x.ToString() + "\nYStart: " + touchStart[0].y.ToString() + "\nXEnd: " + touchEnd[0].x.ToString() + "\nYEnd: " + touchEnd[0].y.ToString();
                DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                debugTouch1Info = "XStart: " + touchStart[1].x.ToString() + "\nYStart: " + touchStart[1].y.ToString() + "\nXEnd: " + touchEnd[1].x.ToString() + "\nYEnd: " + touchEnd[1].y.ToString();
                DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                if (Input.touchCount == 3)
                {
                    debugTouch2Info = "XStart: " + touchStart[2].x.ToString() + "\nYStart: " + touchStart[2].y.ToString() + "\nXEnd: " + touchEnd[2].x.ToString() + "\nYEnd: " + touchEnd[2].y.ToString();
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.
                }                
                
                x0 = touchEnd[0].x - touchStart[0].x;
                y0 = touchEnd[0].y - touchStart[0].y;
                x1 = touchEnd[1].x - touchStart[1].x;
                y1 = touchEnd[1].y - touchStart[1].y;
                if (Input.touchCount == 3)
                {
                    x2 = touchEnd[2].x - touchStart[2].x;
                    y2 = touchEnd[2].y - touchStart[2].y;
                }

                Touch touch0 = Input.touches[0];
                Touch touch1 = Input.touches[1];
                // If the touch has moved and has been registered.
                if (((touch0.phase == TouchPhase.Moved) && (hasRegistered[0] == true)) || ((touch1.phase == TouchPhase.Moved) && (hasRegistered[1] == true)))
                {
                    if (touch0.phase == TouchPhase.Moved)
                    {
                        touchEnd[0] = touch0.position; // Update the end position of this touch.
                        vecEnd1 = touch0.position; // Update the end position vector of this touch.
                    }
                    if (touch1.phase == TouchPhase.Moved)
                    {
                        touchEnd[1] = touch1.position; // Update the end position of this touch.
                        vecEnd2 = touch1.position; // Update the end position vector of this touch.
                    }

                    float currentAngle = Angle(touch0.position, touch1.position);
                    float previousAngle = Angle((touch0.position - touch0.deltaPosition), (touch1.position - touch1.deltaPosition));
                    angle = Mathf.DeltaAngle(currentAngle, previousAngle);
                }
                
                VecStart = vecStart1 - vecStart2; // Get the vector between the start positions of touch0 and touch1.
                VecEnd = vecEnd1 - vecEnd2; // Get the vector between the end positions of touch0 and touch1.
                cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized); // Get the cross product between the two vectors.
                crossPz = cross.z; // Get the z-component of the cross product.   

                debugTouchDurationInfo = "Hold: " + touchDuration + "\nAngle: " + angle.ToString() + "\nCrossPz: " + crossPz;
                DebugTouchDuration.instance.ChangeDebugTouchDurationText(debugTouchDurationInfo); // Update the debug textbox.

                // If the touch was long enough.
                if (touchDuration >= 1.0f)
                {
                    stillHolding = true; // Make sure isHold does not get triggered again if they continue to hold for another 1.0f.
                }
            }
            // If the player has told us they are not using Talkback and there are currently two or three fingers on the screen.
            if (((Input.touchCount == 2) || (Input.touchCount == 3)) && (GM_title.determined_talkback == true) && (GM_title.isUsingTalkback == false))
            {
                touchDuration = touchDuration + Time.deltaTime; // Update the length of the touch.

                debugTouch0Info = "XStart: " + touchStart[0].x.ToString() + "\nYStart: " + touchStart[0].y.ToString() + "\nXEnd: " + touchEnd[0].x.ToString() + "\nYEnd: " + touchEnd[0].y.ToString();
                DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                debugTouch1Info = "XStart: " + touchStart[1].x.ToString() + "\nYStart: " + touchStart[1].y.ToString() + "\nXEnd: " + touchEnd[1].x.ToString() + "\nYEnd: " + touchEnd[1].y.ToString();
                DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                if (Input.touchCount == 3)
                {
                    debugTouch2Info = "XStart: " + touchStart[2].x.ToString() + "\nYStart: " + touchStart[2].y.ToString() + "\nXEnd: " + touchEnd[2].x.ToString() + "\nYEnd: " + touchEnd[2].y.ToString();
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.
                }

                x0 = touchEnd[0].x - touchStart[0].x;
                y0 = touchEnd[0].y - touchStart[0].y;
                x1 = touchEnd[1].x - touchStart[1].x;
                y1 = touchEnd[1].y - touchStart[1].y;
                if (Input.touchCount == 3)
                {
                    x2 = touchEnd[2].x - touchStart[2].x;
                    y2 = touchEnd[2].y - touchStart[2].y;
                }

                Touch touch0 = Input.touches[0];
                Touch touch1 = Input.touches[1];
                // If the touch has moved and has been registered.
                if (((touch0.phase == TouchPhase.Moved) && (hasRegistered[0] == true)) || ((touch1.phase == TouchPhase.Moved) && (hasRegistered[1] == true)))
                {
                    if (touch0.phase == TouchPhase.Moved)
                    {
                        touchEnd[0] = touch0.position; // Update the end position of this touch.
                        vecEnd1 = touch0.position; // Update the end position vector of this touch.
                    }
                    if (touch1.phase == TouchPhase.Moved)
                    {
                        touchEnd[1] = touch1.position; // Update the end position of this touch.
                        vecEnd2 = touch1.position; // Update the end position vector of this touch.
                    }

                    float currentAngle = Angle(touch0.position, touch1.position);
                    float previousAngle = Angle((touch0.position - touch0.deltaPosition), (touch1.position - touch1.deltaPosition));
                    angle = Mathf.DeltaAngle(currentAngle, previousAngle);
                }

                VecStart = vecStart1 - vecStart2; // Get the vector between the start positions of touch0 and touch1.
                VecEnd = vecEnd1 - vecEnd2; // Get the vector between the end positions of touch0 and touch1.
                cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized); // Get the cross product between the two vectors.
                crossPz = cross.z; // Get the z-component of the cross product.   

                debugTouchDurationInfo = "Hold: " + touchDuration + "\nAngle: " + angle.ToString() + "\nCrossPz: " + crossPz;
                DebugTouchDuration.instance.ChangeDebugTouchDurationText(debugTouchDurationInfo); // Update the debug textbox.

                // If the touch was long enough.
                if (touchDuration >= 1.0f)
                {
                    stillHolding = true; // Make sure isHold does not get triggered again if they continue to hold for another 1.0f.
                }
            }
            // If the player has told us they are using Talkback and there are currently three fingers on the screen.
            if ((Input.touchCount == 3) && (GM_title.determined_talkback == true) && (GM_title.isUsingTalkback == true))
            {
                touchDuration = touchDuration + Time.deltaTime; // Update the length of the touch.

                debugTouch0Info = "XStart: " + touchStart[0].x.ToString() + "\nYStart: " + touchStart[0].y.ToString() + "\nXEnd: " + touchEnd[0].x.ToString() + "\nYEnd: " + touchEnd[0].y.ToString();
                DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                debugTouch1Info = "XStart: " + touchStart[1].x.ToString() + "\nYStart: " + touchStart[1].y.ToString() + "\nXEnd: " + touchEnd[1].x.ToString() + "\nYEnd: " + touchEnd[1].y.ToString();
                DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                debugTouch2Info = "XStart: " + touchStart[2].x.ToString() + "\nYStart: " + touchStart[2].y.ToString() + "\nXEnd: " + touchEnd[2].x.ToString() + "\nYEnd: " + touchEnd[2].y.ToString();
                DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.

                x0 = touchEnd[0].x - touchStart[0].x;
                y0 = touchEnd[0].y - touchStart[0].y;
                x1 = touchEnd[1].x - touchStart[1].x;
                y1 = touchEnd[1].y - touchStart[1].y;
                x2 = touchEnd[2].x - touchStart[2].x;
                y2 = touchEnd[2].y - touchStart[2].y;

                Touch touch0 = Input.touches[0];
                Touch touch1 = Input.touches[1];
                // If the touch has moved and has been registered.
                if (((touch0.phase == TouchPhase.Moved) && (hasRegistered[0] == true)) || ((touch1.phase == TouchPhase.Moved) && (hasRegistered[1] == true)))
                {
                    if (touch0.phase == TouchPhase.Moved)
                    {
                        touchEnd[0] = touch0.position; // Update the end position of this touch.
                        vecEnd1 = touch0.position; // Update the end position vector of this touch.
                    }
                    if (touch1.phase == TouchPhase.Moved)
                    {
                        touchEnd[1] = touch1.position; // Update the end position of this touch.
                        vecEnd2 = touch1.position; // Update the end position vector of this touch.
                    }
                   
                    float currentAngle = Angle(touch0.position, touch1.position);
                    float previousAngle = Angle((touch0.position - touch0.deltaPosition), (touch1.position - touch1.deltaPosition));
                    angle = Mathf.DeltaAngle(currentAngle, previousAngle);
                }

                VecStart = vecStart1 - vecStart2; // Get the vector between the start positions of touch0 and touch1.
                VecEnd = vecEnd1 - vecEnd2; // Get the vector between the end positions of touch0 and touch1.
                cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized); // Get the cross product between the two vectors.
                crossPz = cross.z; // Get the z-component of the cross product.   

                debugTouchDurationInfo = "Hold: " + touchDuration + "\nAngle: " + angle.ToString() + "\nCrossPz: " + crossPz;
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
            bool canMakeGesture = false;
            bool wentOffscreen = false;
            gestureStopTime = Time.time; // Set the stop time of this gesture to now.

            if (touchRegister == 2)
            {                
                if (GM_title.determined_talkback == false)
                {
                    canMakeGesture = true;
                }
                else if ((GM_title.determined_talkback == true) && (GM_title.isUsingTalkback == false))
                {
                    canMakeGesture = true;
                }

                if (canMakeGesture == true)
                {
                    // If a finger went off the screen.
                    if ((touchEnd[0].x >= (Screen.width - 10.0f)) || (touchEnd[1].x >= (Screen.width - 10.0f)) || (touchEnd[0].x <= 8.0f) || (touchEnd[1].x <= 8.0f) || (touchEnd[0].y >= (Screen.height - 10.0f)) || (touchEnd[1].y >= (Screen.width - 10.0f)) || (touchEnd[0].y <= 4.0f) || (touchEnd[1].y <= 4.0f))
                    {
                        wentOffscreen = true;
                    }

                    // Get the distance between the start and end positions for each touch in x and y coordinates.
                    // float x0 = touchEnd[0].x - touchStart[0].x;
                    // float y0 = touchEnd[0].y - touchStart[0].y;
                    // float x1 = touchEnd[1].x - touchStart[1].x;
                    // float y1 = touchEnd[1].y - touchStart[1].y;

                    // debugTouch0Info = "XStart: " + touchStart[0].x.ToString() + "\nYStart: " + touchStart[0].y.ToString() + "\nXEnd: " + touchEnd[0].x.ToString() +"\nYEnd: " + touchEnd[0].y.ToString();
                    // DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                    // debugTouch1Info = "XStart: " + touchStart[1].x.ToString() + "\nYStart: " + touchStart[1].y.ToString() + "\nXEnd: " + touchEnd[1].x.ToString() + "\nYEnd: " + touchEnd[1].y.ToString();
                    // DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.

                    // VecStart = vecStart1 - vecStart2; // Get the vector between the start positions of touch0 and touch1.
                    // VecEnd = vecEnd1 - vecEnd2; // Get the vector between the end positions of touch0 and touch1.
                    // float angle = Vector2.Angle(VecStart, VecEnd);
                    // Vector3 cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized); // Get the cross product between the two vectors.
                    // float crossPz = cross.z; // Get the z-component of the cross product.          

                    float time = gestureStopTime - gestureStartTime;
                    // print("X0: " + Mathf.Abs(x0).ToString() + ", X1: " + Mathf.Abs(x1).ToString() + ", Y0: " + Mathf.Abs(y0).ToString() + ", Y1: " + Mathf.Abs(y1).ToString());
                    // print("VS1: " + vecStart1.ToString() + ", VS2: " + vecStart2.ToString() + ", VS: " + VecStart.ToString() + ", VE1: " + vecEnd1.ToString() + ", VE2: " + vecEnd2.ToString() + ", VE: " + VecEnd.ToString());
                    // print("Time: " + time.ToString() + ", Angle: " + angle + ", CrossPz: " + crossPz.ToString() + ", Cross: " + cross.ToString());
                    // print("MTH: " + maxTapHorizontalDist.ToString() + ", MTV: " + maxTapVerticalDist.ToString() + ", MSH: " + minSwipeHorizontalDist.ToString() + ", MSV: " + minSwipeVerticalDist.ToString() + ", MRH: " + minRotateHorizontalDist.ToString() + ", MRV: " + minRotateVerticalDist.ToString() + ", MHH: " + maxHoldHorizontalDist.ToString() + ", MHV: " + maxHoldVerticalDist.ToString());
                    
                    // If a finger went off the screen.
                    if (wentOffscreen == true)
                    {
                        unrecognizedTimes += 1;
                        debugInputInfo = "At least one finger went off the screen. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                    }

                    // If the gesture was a tap.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) <= maxTapHorizontalDist) && (Mathf.Abs(y0) <= maxTapVerticalDist) && (Mathf.Abs(x1) <= maxTapHorizontalDist) && (Mathf.Abs(y1) <= maxTapVerticalDist) && (angle <= 40))
                    {
                        ievent.isTap = true; // A tap was registered.
                        tapTimes += 1; // Update the number of times a tap was made.
                        debugInputInfo = "Tapped " + tapTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                  
                    }

                    // If the gesture was a left or right swipe.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) > Mathf.Abs(y0)) && (Mathf.Abs(x0) >= minSwipeHorizontalDist) && (Mathf.Abs(x1) > Mathf.Abs(y1)) && (Mathf.Abs(x1) >= minSwipeHorizontalDist) && (angle <= 40))
                    {
                        // Swipe left detected.
                        if ((x0 < 0.0f) && (x1 < 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isLeft = true; // The swipe was left.
                            swipeLeftTimes += 1; // Update the number of times a swipe left was made.
                            debugInputInfo = "Swiped left " + swipeLeftTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                      
                        }
                        // Swipe right detected.
                        else if ((x0 > 0.0f) && (x1 > 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isRight = true; // The swipe was right.
                            swipeRightTimes += 1; // Update the number of times a swipe right was made.
                            debugInputInfo = "Swiped right " + swipeRightTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        }
                    }

                    // If the gesture was an up or down swipe.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(y0) > Mathf.Abs(x0)) && (Mathf.Abs(y0) >= minSwipeVerticalDist) && (Mathf.Abs(y1) > Mathf.Abs(x1)) && (Mathf.Abs(y1) >= minSwipeVerticalDist) && (angle <= 40))
                    {
                        // Swipe up detected.
                        if ((y0 > 0.0f) && (y1 > 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isUp = true; // The swipe was right.
                            swipeUpTimes += 1; // Update the number of times a swipe up was made.
                            debugInputInfo = "Swiped up " + swipeUpTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                           
                        }
                        // Swipe down detected.
                        else if ((y0 < 0.0f) && (y1 < 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isDown = true; // The swipe was down.
                            swipeDownTimes += 1; // Update the number of times a swipe down was made.
                            debugInputInfo = "Swiped down " + swipeDownTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                        }
                    }

                    // If the gesture was a left turn.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (crossPz >= 0) && ((Mathf.Abs(x0) >= minRotateHorizontalDist) || (Mathf.Abs(x1) >= minRotateHorizontalDist)) && ((Mathf.Abs(y0) >= minRotateVerticalDist) || (Mathf.Abs(y1) >= minRotateVerticalDist)) && (angle >= 45))
                    {
                        ievent.isRotate = true; // A rotation was registered.
                        ievent.isLeft = true; // The rotation was left.
                        rotateLeftTimes += 1; // Update the number of times a left rotation was made.
                        debugInputInfo = "Rotated left " + rotateLeftTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                      
                    }

                    // If the gesture was a right turn.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (crossPz < 0) && ((Mathf.Abs(x0) >= minRotateHorizontalDist) || (Mathf.Abs(x1) >= minRotateHorizontalDist)) && ((Mathf.Abs(y0) >= minRotateVerticalDist) || (Mathf.Abs(y1) >= minRotateVerticalDist)) && (angle >= 45))
                    {
                        ievent.isRotate = true; // A rotation was registered.
                        ievent.isRight = true; // The rotation was right.
                        rotateRightTimes += 1; // Update the number of times a right rotation was made.
                        debugInputInfo = "Rotated right " + rotateRightTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                       
                    }

                    // If the gesture was a hold.
                    else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && (Mathf.Abs(x0) <= maxHoldHorizontalDist) && (Mathf.Abs(x1) <= maxHoldHorizontalDist) && (Mathf.Abs(y0) <= maxHoldVerticalDist) && (Mathf.Abs(y1) <= maxHoldVerticalDist) && (angle <= 40))
                    {
                        stillHolding = false; // We are no longer holding on the screen.
                        ievent.isHold = true; // A hold was registered.
                        holdTimes += 1; // Update the number of times a hold was made.
                        debugInputInfo = "Hold registered " + holdTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                        
                    }

                    // If the gesture was a tap with too much rotation.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) <= maxTapHorizontalDist) && (Mathf.Abs(y0) <= maxTapVerticalDist) && (Mathf.Abs(x1) <= maxTapHorizontalDist) && (Mathf.Abs(y1) <= maxTapVerticalDist) && (angle > 40))
                    {
                        ievent.isTapRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Tap rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                
                    }

                    // If the gesture was a tap with too much horizontal movement.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (((Mathf.Abs(x0) > maxTapHorizontalDist) && (Mathf.Abs(x0) <= (Screen.width * 0.08f))) || ((Mathf.Abs(x1) > maxTapHorizontalDist) && (Mathf.Abs(x1) <= (Screen.width * 0.08f)))) && (Mathf.Abs(y0) <= maxTapVerticalDist) && (Mathf.Abs(y1) <= maxTapVerticalDist))
                    {
                        ievent.isTapHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Tap horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                  
                    }

                    // If the gesture was a tap with too much vertical movement.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (((Mathf.Abs(y0) > maxTapVerticalDist) && (Mathf.Abs(y0) <= (Screen.height * 0.095f))) || ((Mathf.Abs(y1) > maxTapVerticalDist) && (Mathf.Abs(y1) <= (Screen.height * 0.095f)))) && (Mathf.Abs(x0) <= maxTapHorizontalDist) && (Mathf.Abs(x1) <= maxTapHorizontalDist))
                    {
                        ievent.isTapVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Tap vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                        
                    }

                    // If the gesture was a tap with too much horizontal and vertical movement.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (((Mathf.Abs(y0) > maxTapVerticalDist) && (Mathf.Abs(y0) <= (Screen.height * 0.095f))) || ((Mathf.Abs(y1) > maxTapVerticalDist) && (Mathf.Abs(y1) <= (Screen.height * 0.095f)))) && (((Mathf.Abs(x0) > maxTapHorizontalDist) && (Mathf.Abs(x0) <= (Screen.width * 0.08f))) || ((Mathf.Abs(x1) > maxTapHorizontalDist) && (Mathf.Abs(x1) <= (Screen.width * 0.08f)))))
                    {
                        ievent.isTapVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Tap horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                        
                    }

                    // If the gesture was a left or right swipe with too much rotation.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) > Mathf.Abs(y0)) && (Mathf.Abs(x0) >= minSwipeHorizontalDist) && (Mathf.Abs(x1) > Mathf.Abs(y1)) && (Mathf.Abs(x1) >= minSwipeHorizontalDist) && (angle >= 40))
                    {
                        // Swipe left detected.
                        if ((x0 < 0.0f) && (x1 < 0.0f))
                        {
                            ievent.isSwipeLeftRotationError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                                                    // debugInputInfo = "Swipe left rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            debugInputInfo = "Left. x0: " + touchEnd[0].x.ToString() + ", x1: " + touchEnd[1].x.ToString() + ", y0: " + touchEnd[0].y.ToString() + ", y1: " + touchEnd[1].y.ToString() + ", Angle: " + angle.ToString();
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                    
                        }
                        // Swipe right detected.
                        else if ((x0 > 0.0f) && (x1 > 0.0f))
                        {
                            ievent.isSwipeRightRotationError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            // debugInputInfo = "Swipe right rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            debugInputInfo = "Right. x0: " + touchEnd[0].x.ToString() + ", x1: " + touchEnd[1].x.ToString() + ", y0: " + touchEnd[0].y.ToString() + ", y1: " + touchEnd[1].y.ToString() + ", Angle: " + angle.ToString();
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        }
                    }

                    // If the gesture was a left or right swipe with not enough horizontal movement.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) > Mathf.Abs(y0)) && (Mathf.Abs(x1) > Mathf.Abs(y1)) && (((Mathf.Abs(x0) < minSwipeHorizontalDist) && (Mathf.Abs(x0) > (Screen.width * 0.08f))) || ((Mathf.Abs(x1) < minSwipeHorizontalDist) && (Mathf.Abs(x1) > (Screen.width * 0.08f)))))
                    {
                        // Swipe left detected.
                        if ((x0 < 0.0f) && (x1 < 0.0f))
                        {
                            ievent.isSwipeLeftHorizontalError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe left horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                      
                        }
                        // Swipe right detected.
                        else if ((x0 > 0.0f) && (x1 > 0.0f))
                        {
                            ievent.isSwipeRightHorizontalError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe right horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                           
                        }
                    }

                    // If the gesture was an up or down swipe with too much rotation.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(y0) > Mathf.Abs(x0)) && (Mathf.Abs(y0) >= minSwipeVerticalDist) && (Mathf.Abs(y1) > Mathf.Abs(x1)) && (Mathf.Abs(y1) >= minSwipeVerticalDist) && (angle >= 40))
                    {
                        // Swipe up detected.
                        if ((y0 > 0.0f) && (y1 > 0.0f))
                        {
                            ievent.isSwipeUpRotationError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe up rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        }
                        // Swipe down detected.
                        else if ((y0 < 0.0f) && (y1 < 0.0f))
                        {
                            ievent.isSwipeDownRotationError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe down rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                        }
                    }

                    // If the gesture was an up or down swipe with not enough vertical movement.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(y0) > Mathf.Abs(x0)) && (Mathf.Abs(y1) > Mathf.Abs(x1)) && (((Mathf.Abs(y0) < minSwipeVerticalDist) && (Mathf.Abs(y0) > (Screen.height * 0.095f))) || ((Mathf.Abs(y1) < minSwipeVerticalDist) && (Mathf.Abs(y1) > (Screen.height * 0.095f)))))
                    {
                        // Swipe up detected.
                        if ((y0 > 0.0f) && (y1 > 0.0f))
                        {
                            ievent.isSwipeUpVerticalError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe up vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                        }
                        // Swipe down detected.
                        else if ((y0 < 0.0f) && (y1 < 0.0f))
                        {
                            ievent.isSwipeDownVerticalError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe down vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                    
                        }
                    }

                    // If the gesture was a turn with not enough rotation.
                    else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) < minRotateHorizontalDist) && (Mathf.Abs(x1) < minRotateHorizontalDist) && (Mathf.Abs(y0) < minRotateVerticalDist) && (Mathf.Abs(y1) < minRotateVerticalDist) && (angle < 45))
                    {
                        ievent.isRotationAngleError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Turn rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                    }

                    // If the gesture was a hold with too much rotation.
                    else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && (angle >= 40))
                    {
                        stillHolding = false; // We are no longer holding on the screen.
                        ievent.isHoldRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Hold rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                    }

                    // If the gesture was a hold with too much horizontal movement.
                    else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && ((Mathf.Abs(x0) > maxHoldHorizontalDist) || (Mathf.Abs(x1) > maxHoldHorizontalDist)) && (Mathf.Abs(y0) <= maxHoldVerticalDist) && (Mathf.Abs(y1) <= maxHoldVerticalDist))
                    {
                        stillHolding = false; // We are no longer holding on the screen.
                        ievent.isHoldHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Hold horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                    }

                    // If the gesture was a hold with too much vertical movement.
                    else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && ((Mathf.Abs(y0) > maxHoldVerticalDist) || (Mathf.Abs(y1) > maxHoldVerticalDist)) && (Mathf.Abs(x0) <= maxHoldHorizontalDist) && (Mathf.Abs(x1) <= maxHoldHorizontalDist))
                    {
                        stillHolding = false; // We are no longer holding on the screen.
                        ievent.isHoldVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Hold vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                        
                    }

                    // If the gesture was a hold with too much horizontal and vertical movement.
                    else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && ((Mathf.Abs(y0) > maxHoldVerticalDist) || (Mathf.Abs(y1) > maxHoldVerticalDist)) && ((Mathf.Abs(x0) > maxHoldHorizontalDist) || (Mathf.Abs(x1) > maxHoldHorizontalDist)))
                    {
                        stillHolding = false; // We are no longer holding on the screen.
                        ievent.isHoldVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Hold horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                    }

                    else
                    {               
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Other gesture error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                           
                    }
                }
            }

            else if (touchRegister == 3)
            {
                // If a finger went off the screen.
                if ((touchEnd[0].x >= (Screen.width - 10.0f)) || (touchEnd[1].x >= (Screen.width - 10.0f)) || (touchEnd[2].x >= (Screen.width - 10.0f)) || (touchEnd[0].x <= 8.0f) || (touchEnd[1].x <= 8.0f) || (touchEnd[2].x <= 8.0f) || (touchEnd[0].y >= (Screen.height - 10.0f)) || (touchEnd[1].y >= (Screen.width - 10.0f)) || (touchEnd[2].y >= (Screen.width - 10.0f)) || (touchEnd[0].y <= 4.0f) || (touchEnd[1].y <= 4.0f) || (touchEnd[2].y <= 4.0f))
                {
                    wentOffscreen = true;
                }

                // Get the distance between the start and end positions for each touch in x and y coordinates.
                // float x0 = touchEnd[0].x - touchStart[0].x;
                // float y0 = touchEnd[0].y - touchStart[0].y;
                // float x1 = touchEnd[1].x - touchStart[1].x;
                // float y1 = touchEnd[1].y - touchStart[1].y;
                // float x2 = touchEnd[2].x - touchStart[2].x;
                // float y2 = touchEnd[2].y - touchStart[2].y;

                // debugTouch0Info = "XStart: " + touchStart[0].x.ToString() + "\nYStart: " + touchStart[0].y.ToString() + "\nXEnd: " + touchEnd[0].x.ToString() + "\nYEnd: " + touchEnd[0].y.ToString();
                // DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                // debugTouch1Info = "XStart: " + touchStart[1].x.ToString() + "\nYStart: " + touchStart[1].y.ToString() + "\nXEnd: " + touchEnd[1].x.ToString() + "\nYEnd: " + touchEnd[1].y.ToString();
                // DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                // debugTouch2Info = "XStart: " + touchStart[2].x.ToString() + "\nYStart: " + touchStart[2].y.ToString() + "\nXEnd: " + touchEnd[2].x.ToString() + "\nYEnd: " + touchEnd[2].y.ToString();
                // DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.

                // VecStart = vecStart1 - vecStart2; // Get the vector between the start positions of touch0 and touch1.
                // VecEnd = vecEnd1 - vecEnd2; // Get the vector between the end positions of touch0 and touch1.
                // float angle = Vector2.Angle(VecStart, VecEnd);
                // Vector3 cross = Vector3.Cross((Vector3)VecStart.normalized, (Vector3)VecEnd.normalized); // Get the cross product between the two vectors.
                // float crossPz = cross.z; // Get the z-component of the cross product.

                float time = gestureStopTime - gestureStartTime;
                // print("X0: " + Mathf.Abs(x0).ToString() + ", X1: " + Mathf.Abs(x1).ToString() + ", X2: " + Mathf.Abs(x2).ToString() + ", Y0: " + Mathf.Abs(y0).ToString() + ", Y1: " + Mathf.Abs(y1).ToString() + ", Y2: " + Mathf.Abs(y2).ToString());
                // print("VS1: " + vecStart1.ToString() + ", VS2: " + vecStart2.ToString() + ", VS: " + VecStart.ToString() + ", VE1: " + vecEnd1.ToString() + ", VE2: " + vecEnd2.ToString() + ", VE: " + VecEnd.ToString());
                // print("Time: " + time.ToString() + ", Angle: " + angle + ", CrossPz: " + crossPz.ToString() + ", Cross: " + cross.ToString());
                // print("MTH: " + maxTapHorizontalDist.ToString() + ", MTV: " + maxTapVerticalDist.ToString() + ", MSH: " + minSwipeHorizontalDist.ToString() + ", MSV: " + minSwipeVerticalDist.ToString() + ", MRH: " + minRotateHorizontalDist.ToString() + ", MRV: " + minRotateVerticalDist.ToString() + ", MHH: " + maxHoldHorizontalDist.ToString() + ", MHV: " + maxHoldVerticalDist.ToString());

                // If a finger went off the screen.
                if (wentOffscreen == true)
                {
                    unrecognizedTimes += 1;
                    debugInputInfo = "At least one finger went off the screen. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                 
                }

                // If the gesture was a tap.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) <= maxTapHorizontalDist) && (Mathf.Abs(y0) <= maxTapVerticalDist) && (Mathf.Abs(x1) <= maxTapHorizontalDist) && (Mathf.Abs(y1) <= maxTapVerticalDist) && (Mathf.Abs(x2) <= maxTapHorizontalDist) && (Mathf.Abs(y2) <= maxTapVerticalDist) && (angle <= 40))
                {
                    ievent.isTap = true; // A tap was registered.
                    tapTimes += 1; // Update the number of times a tap was made.
                    debugInputInfo = "Tapped " + tapTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                 
                }

                // If the gesture was a left or right swipe.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) > Mathf.Abs(y0)) && (Mathf.Abs(x0) >= minSwipeHorizontalDist) && (Mathf.Abs(x1) > Mathf.Abs(y1)) && (Mathf.Abs(x1) >= minSwipeHorizontalDist) && (Mathf.Abs(x2) > Mathf.Abs(y2)) && (Mathf.Abs(x2) >= minSwipeHorizontalDist) && (angle <= 40))
                {
                    // Swipe left detected.
                    if ((x0 < 0.0f) && (x1 < 0.0f) && (x2 < 0.0f))
                    {
                        ievent.isSwipe = true; // A swipe was registered.
                        ievent.isLeft = true; // The swipe was left.
                        swipeLeftTimes += 1; // Update the number of times a swipe left was made.
                        debugInputInfo = "Swiped left " + swipeLeftTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                      
                    }
                    // Swipe right detected.
                    else if ((x0 > 0.0f) && (x1 > 0.0f) && (x2 > 0.0f))
                    {
                        ievent.isSwipe = true; // A swipe was registered.
                        ievent.isRight = true; // The swipe was right.
                        swipeRightTimes += 1; // Update the number of times a swipe right was made.
                        debugInputInfo = "Swiped right " + swipeRightTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                    }
                }

                // If the gesture was an up or down swipe.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(y0) > Mathf.Abs(x0)) && (Mathf.Abs(y0) >= minSwipeVerticalDist) && (Mathf.Abs(y1) > Mathf.Abs(x1)) && (Mathf.Abs(y1) >= minSwipeVerticalDist) && (Mathf.Abs(y2) > Mathf.Abs(x2)) && (Mathf.Abs(y2) >= minSwipeVerticalDist) && (angle <= 40))
                {
                    // Swipe up detected.
                    if ((y0 > 0.0f) && (y1 > 0.0f) && (y2 > 0.0f))
                    {
                        ievent.isSwipe = true; // A swipe was registered.
                        ievent.isUp = true; // The swipe was right.
                        swipeUpTimes += 1; // Update the number of times a swipe up was made.
                        debugInputInfo = "Swiped up " + swipeUpTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                           
                    }
                    // Swipe down detected.
                    else if ((y0 < 0.0f) && (y1 < 0.0f) && (y2 < 0.0f))
                    {
                        ievent.isSwipe = true; // A swipe was registered.
                        ievent.isDown = true; // The swipe was down.
                        swipeDownTimes += 1; // Update the number of times a swipe down was made.
                        debugInputInfo = "Swiped down " + swipeDownTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                    }
                }

                // If the gesture was a left turn.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (crossPz >= 0) && ((Mathf.Abs(x0) >= minRotateHorizontalDist) || (Mathf.Abs(x1) >= minRotateHorizontalDist) || (Mathf.Abs(x2) >= minRotateHorizontalDist)) && ((Mathf.Abs(y0) >= minRotateVerticalDist) || (Mathf.Abs(y1) >= minRotateVerticalDist) || (Mathf.Abs(y2) >= minRotateVerticalDist)) && (angle >= 45))
                {
                    ievent.isRotate = true; // A rotation was registered.
                    ievent.isLeft = true; // The rotation was left.
                    rotateLeftTimes += 1; // Update the number of times a left rotation was made.
                    debugInputInfo = "Rotated left " + rotateLeftTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                      
                }

                // If the gesture was a right turn.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (crossPz < 0) && ((Mathf.Abs(x0) >= minRotateHorizontalDist) || (Mathf.Abs(x1) >= minRotateHorizontalDist) || (Mathf.Abs(x2) >= minRotateHorizontalDist)) && ((Mathf.Abs(y0) >= minRotateVerticalDist) || (Mathf.Abs(y1) >= minRotateVerticalDist) || (Mathf.Abs(y2) >= minRotateVerticalDist)) && (angle >= 45))
                {
                    ievent.isRotate = true; // A rotation was registered.
                    ievent.isRight = true; // The rotation was right.
                    rotateRightTimes += 1; // Update the number of times a right rotation was made.
                    debugInputInfo = "Rotated right " + rotateRightTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                       
                }

                // If the gesture was a hold.
                else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && (Mathf.Abs(x0) <= maxHoldHorizontalDist) && (Mathf.Abs(x1) <= maxHoldHorizontalDist) && (Mathf.Abs(x2) <= maxHoldHorizontalDist) && (Mathf.Abs(y0) <= maxHoldVerticalDist) && (Mathf.Abs(y1) <= maxHoldVerticalDist) && (Mathf.Abs(y2) <= maxHoldVerticalDist) && (angle <= 40))
                {
                    stillHolding = false; // We are no longer holding on the screen.
                    ievent.isHold = true; // A hold was registered.
                    holdTimes += 1; // Update the number of times a hold was made.
                    debugInputInfo = "Hold registered " + holdTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                        
                }

                // If the gesture was a tap with too much rotation.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) <= maxTapHorizontalDist) && (Mathf.Abs(y0) <= maxTapVerticalDist) && (Mathf.Abs(x1) <= maxTapHorizontalDist) && (Mathf.Abs(y1) <= maxTapVerticalDist) && (Mathf.Abs(x2) <= maxTapHorizontalDist) && (Mathf.Abs(y2) <= maxTapVerticalDist) && (angle > 40))
                {
                    ievent.isTapRotationError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Tap rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                 
                }

                // If the gesture was a tap with too much horizontal movement.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (((Mathf.Abs(x0) > maxTapHorizontalDist) && (Mathf.Abs(x0) <= (Screen.width * 0.08f))) || ((Mathf.Abs(x1) > maxTapHorizontalDist) && (Mathf.Abs(x1) <= (Screen.width * 0.08f))) || ((Mathf.Abs(x2) > maxTapHorizontalDist) && (Mathf.Abs(x2) <= (Screen.width * 0.08f)))) && (Mathf.Abs(y0) <= maxTapVerticalDist) && (Mathf.Abs(y1) <= maxTapVerticalDist) && (Mathf.Abs(y2) <= maxTapVerticalDist))
                {
                    ievent.isTapHorizontalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Tap horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                  
                }

                // If the gesture was a tap with too much vertical movement.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (((Mathf.Abs(y0) > maxTapVerticalDist) && (Mathf.Abs(y0) <= (Screen.height * 0.095f))) || ((Mathf.Abs(y1) > maxTapVerticalDist) && (Mathf.Abs(y1) <= (Screen.height * 0.095f))) || ((Mathf.Abs(y2) > maxTapVerticalDist) && (Mathf.Abs(y2) <= (Screen.height * 0.095f)))) && (Mathf.Abs(x0) <= maxTapHorizontalDist) && (Mathf.Abs(x1) <= maxTapHorizontalDist) && (Mathf.Abs(x2) <= maxTapHorizontalDist))
                {
                    ievent.isTapVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Tap vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                        
                }

                // If the gesture was a tap with too much horizontal and vertical movement.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (((Mathf.Abs(y0) > maxTapVerticalDist) && (Mathf.Abs(y0) <= (Screen.height * 0.095f))) || ((Mathf.Abs(y1) > maxTapVerticalDist) && (Mathf.Abs(y1) <= (Screen.height * 0.095f))) || ((Mathf.Abs(y2) > maxTapVerticalDist) && (Mathf.Abs(y2) <= (Screen.height * 0.095f)))) && (((Mathf.Abs(x0) > maxTapHorizontalDist) && (Mathf.Abs(x0) <= (Screen.width * 0.08f))) || ((Mathf.Abs(x1) > maxTapHorizontalDist) && (Mathf.Abs(x1) <= (Screen.width * 0.08f))) || ((Mathf.Abs(x2) > maxTapHorizontalDist) && (Mathf.Abs(x2) <= (Screen.width * 0.08f)))))
                {
                    ievent.isTapVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Tap horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                        
                }

                // If the gesture was a left or right swipe with too much rotation.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) > Mathf.Abs(y0)) && (Mathf.Abs(x0) >= minSwipeHorizontalDist) && (Mathf.Abs(x1) > Mathf.Abs(y1)) && (Mathf.Abs(x1) >= minSwipeHorizontalDist) && (Mathf.Abs(x2) > Mathf.Abs(y2)) && (Mathf.Abs(x2) >= minSwipeHorizontalDist) && (angle >= 40))
                {
                    // Swipe left detected.
                    if ((x0 < 0.0f) && (x1 < 0.0f) && (x2 < 0.0f))
                    {
                        ievent.isSwipeLeftRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        // debugInputInfo = "Swipe left rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        debugInputInfo = "Left. x0: " + touchEnd[0].x.ToString() + ", x1: " + touchEnd[1].x.ToString() + ", x2: " + touchEnd[2].x.ToString() + ", y0: " + touchEnd[0].y.ToString() + ", y1: " + touchEnd[1].y.ToString() + ", y2: " + touchEnd[2].y.ToString() + ", Angle: " + angle.ToString();
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                  
                    }
                    // Swipe right detected.
                    else if ((x0 > 0.0f) && (x1 > 0.0f) && (x2 > 0.0f))
                    {
                        ievent.isSwipeRightRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                                                // debugInputInfo = "Swipe right rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        debugInputInfo = "Right. x0: " + touchEnd[0].x.ToString() + ", x1: " + touchEnd[1].x.ToString() + ", x2: " + touchEnd[2].x.ToString() + ", y0: " + touchEnd[0].y.ToString() + ", y1: " + touchEnd[1].y.ToString() + ", y2: " + touchEnd[2].y.ToString() + ", Angle: " + angle.ToString();
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                    }
                }

                // If the gesture was a left or right swipe with not enough horizontal movement.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) > Mathf.Abs(y0)) && (Mathf.Abs(x1) > Mathf.Abs(y1)) && (Mathf.Abs(x2) > Mathf.Abs(y2)) && (((Mathf.Abs(x0) < minSwipeHorizontalDist) && (Mathf.Abs(x0) > (Screen.width * 0.08f))) || ((Mathf.Abs(x1) < minSwipeHorizontalDist) && (Mathf.Abs(x1) > (Screen.width * 0.08f))) || ((Mathf.Abs(x2) < minSwipeHorizontalDist) && (Mathf.Abs(x2) > (Screen.width * 0.08f)))))
                {
                    // Swipe left detected.
                    if ((x0 < 0.0f) && (x1 < 0.0f) && (x2 < 0.0f))
                    {
                        ievent.isSwipeLeftHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe left horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                      
                    }
                    // Swipe right detected.
                    else if ((x0 > 0.0f) && (x1 > 0.0f) && (x2 > 0.0f))
                    {
                        ievent.isSwipeRightHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe right horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                           
                    }
                }

                // If the gesture was an up or down swipe with too much rotation.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(y0) > Mathf.Abs(x0)) && (Mathf.Abs(y0) >= minSwipeVerticalDist) && (Mathf.Abs(y1) > Mathf.Abs(x1)) && (Mathf.Abs(y1) >= minSwipeVerticalDist) && (Mathf.Abs(y2) > Mathf.Abs(x2)) && (Mathf.Abs(y2) >= minSwipeVerticalDist) && (angle >= 40))
                {
                    // Swipe up detected.
                    if ((y0 > 0.0f) && (y1 > 0.0f) && (y2 > 0.0f))
                    {
                        ievent.isSwipeUpRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe up rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                    }
                    // Swipe down detected.
                    else if ((y0 < 0.0f) && (y1 < 0.0f) && (y2 < 0.0f))
                    {
                        ievent.isSwipeDownRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe down rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                    }
                }

                // If the gesture was an up or down swipe with not enough vertical movement.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(y0) > Mathf.Abs(x0)) && (Mathf.Abs(y1) > Mathf.Abs(x1)) && (Mathf.Abs(y2) > Mathf.Abs(x2)) && (((Mathf.Abs(y0) < minSwipeVerticalDist) && (Mathf.Abs(y0) > (Screen.height * 0.095f))) || ((Mathf.Abs(y1) < minSwipeVerticalDist) && (Mathf.Abs(y1) > (Screen.height * 0.095f))) || ((Mathf.Abs(y2) < minSwipeVerticalDist) && (Mathf.Abs(y2) > (Screen.height * 0.095f)))))
                {
                    // Swipe up detected.
                    if ((y0 > 0.0f) && (y1 > 0.0f) && (y2 > 0.0f))
                    {
                        ievent.isSwipeUpVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe up vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                    }
                    // Swipe down detected.
                    else if ((y0 < 0.0f) && (y1 < 0.0f) && (y2 < 0.0f))
                    {
                        ievent.isSwipeDownVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe down vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                                                    
                    }
                }

                // If the gesture was a turn with not enough rotation.
                else if (((gestureStopTime - gestureStartTime) < 1.0f) && (Mathf.Abs(x0) < minRotateHorizontalDist) && (Mathf.Abs(x1) < minRotateHorizontalDist) && (Mathf.Abs(x2) < minRotateHorizontalDist) && (Mathf.Abs(y0) < minRotateVerticalDist) && (Mathf.Abs(y1) < minRotateVerticalDist) && (Mathf.Abs(y2) < minRotateVerticalDist) && (angle < 45))
                {
                    ievent.isRotationAngleError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Turn rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                }

                // If the gesture was a hold with too much rotation.
                else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && (angle >= 40))
                {
                    stillHolding = false; // We are no longer holding on the screen.
                    ievent.isHoldRotationError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Hold rotation error error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                }

                // If the gesture was a hold with too much horizontal movement.
                else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && ((Mathf.Abs(x0) > maxHoldHorizontalDist) || (Mathf.Abs(x1) > maxHoldHorizontalDist) || (Mathf.Abs(x2) > maxHoldHorizontalDist)) && (Mathf.Abs(y0) <= maxHoldVerticalDist) && (Mathf.Abs(y1) <= maxHoldVerticalDist) && (Mathf.Abs(y2) <= maxHoldVerticalDist))
                {
                    stillHolding = false; // We are no longer holding on the screen.
                    ievent.isHoldHorizontalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Hold horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                }

                // If the gesture was a hold with too much vertical movement.
                else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && ((Mathf.Abs(y0) > maxHoldVerticalDist) || (Mathf.Abs(y1) > maxHoldVerticalDist) || (Mathf.Abs(y2) > maxHoldVerticalDist)) && (Mathf.Abs(x0) <= maxHoldHorizontalDist) && (Mathf.Abs(x1) <= maxHoldHorizontalDist) && (Mathf.Abs(x2) <= maxHoldHorizontalDist))
                {
                    stillHolding = false; // We are no longer holding on the screen.
                    ievent.isHoldVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Hold vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                }

                // If the gesture was a hold with too much horizontal and vertical movement.
                else if (((gestureStopTime - gestureStartTime) >= 1.0f) && (stillHolding == true) && ((Mathf.Abs(y0) > maxHoldVerticalDist) || (Mathf.Abs(y1) > maxHoldVerticalDist) || (Mathf.Abs(y2) > maxHoldVerticalDist)) && ((Mathf.Abs(x0) > maxHoldHorizontalDist) || (Mathf.Abs(x1) > maxHoldHorizontalDist) || (Mathf.Abs(x2) > maxHoldHorizontalDist)))
                {
                    stillHolding = false; // We are no longer holding on the screen.
                    ievent.isHoldVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Hold horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                }

                else
                {        
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Other gesture error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                }                                
            }

            touchDuration = 0.0f; // Reset duration of touch duration to 0, as nothing is touch the screen.          
            stillHolding = false; // Let the player register another hold.
            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.

            hasRegistered[0] = false; // Touch0 is no longer on the screen. Make sure it is not registered for the next time it touches so that its start position can be obtained.
			hasRegistered[1] = false; // Touch1 is no longer on the screen. Make sure it is not registered for the next time it touches so that its start position can be obtained.
			hasRegistered[2] = false; // Touch2 is no longer on the screen. Make sure it is not registered for the next time it touches so that its start position can be obtained.
		}
#endif
        // print(touchTime);
        // if no input, code should not reach here
        PassDataToListeners(ievent);
        NotifyLlisteners();
    }

    static private float Angle(Vector2 pos1, Vector2 pos2)
    {
        Vector2 from = pos2 - pos1;
        Vector2 to = new Vector2(1, 0);

        float result = Vector2.Angle(from, to);
        Vector3 cross = Vector3.Cross(from, to);

        if (cross.z > 0)
        {
            result = 360f - result;
        }

        return result;
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
