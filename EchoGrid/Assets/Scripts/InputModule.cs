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
    public bool isTapHorizontalVerticalError;
    public bool isTapRotationError; // If the player rotated too much during the tap.
    public bool isSwipeLeftHorizontalError; // If the player did not move enough horizontally during the swipe left.
    public bool isSwipeRightHorizontalError; // If the player did not move enough horizontally during the swipe right.
    public bool isSwipeUpVerticalError; // If the player did not move enough vertically during the swipe up.
    public bool isSwipeDownVerticalError; // If the player did not move enough vertically during the swipe down.
    public bool isSwipeHorizontalVerticalError;
    public bool isSwipeLeftRotationError; // If the player rotated too much during the swipe left.
    public bool isSwipeRightRotationError; // If the player rotated too much during the swipe right.
    public bool isSwipeUpRotationError; // If the player rotated too much during the swipe up.
    public bool isSwipeDownRotationError; // If the player rotated too much during the swipe down.
    public bool isRotationAngleError; // If the player did not rotate far enough during the rotation.   
    public bool isHoldHorizontalError; // If the player moved too much horizontally during the hold.
    public bool isHoldVerticalError; // If the player moved too much vertically during the hold.
    public bool isHoldHorizontalVerticalError;
    public bool isHoldRotationError; // If the player rotated too much during the hold.
    public bool isSwipeDirectionError;
    public bool isBetweenTapSwipeError;
    public bool isBetweenHoldSwipeError;
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
        isTapHorizontalError = false; isTapVerticalError = false; isTapHorizontalVerticalError = false; isTapRotationError = false;
        isSwipeLeftHorizontalError = false; isSwipeRightHorizontalError = false; isSwipeUpVerticalError = false; isSwipeDownVerticalError = false; isSwipeHorizontalVerticalError = false;
        isSwipeLeftRotationError = false; isSwipeRightRotationError = false; isSwipeUpRotationError = false; isSwipeDownRotationError = false;
        isRotationAngleError = false; isHoldHorizontalError = false; isHoldVerticalError = false; isHoldHorizontalVerticalError = false; isHoldRotationError = false;
        isSwipeDirectionError = false; isBetweenTapSwipeError = false; isBetweenHoldSwipeError = false;
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
    float maxTapHorizontalDist = Screen.width * 0.08f;  // The maximum horizontal distance the player can move their fingers to register a gesture as a tap.
    float maxTapVerticalDist = Screen.height * 0.08f; // The maximum vertical distance the player can move their fingers to register a gesture as a tap.
    float maxHoldHorizontalDist = Screen.width * 0.08f; // The maximum horizontal distance the player can move their fingers to register a gesture as a hold.
    float maxHoldVerticalDist = Screen.height * 0.08f; // The maximum vertical distance the player can move their fingers to register a gesture as a hold.   
    float minSwipeHorizontalDist = Screen.width * 0.15f; // The minimum horizontal distance the player needs to move their fingers to register a gesture as a swipe.
    float minSwipeVerticalDist = Screen.height * 0.15f; // The minimum vertical distance the player needs to move their fingers to register a gesture as a swipe.

    List<eventHandler> listeners = new List<eventHandler>();

    string debugInputInfo; // String for debugging what inputs the game has registered from the player.
    string debugTouch0Info; // String for debugging what is happening with touch0.
    string debugTouch1Info; // String for debugging what is happening with touch1.
    string debugTouch2Info; // String for debugging what is happening with touch2.
    string debugTouchDurationInfo; // String for debugging the duration the player has been holding a gesture.

    Vector2 vecStart0 = new Vector2(); // Vector for the start position of touch0.
    Vector2 vecEnd0 = new Vector2(); // Vector for the end position of touch0.
    Vector2 vecStart1 = new Vector2(); // Vector for the start position of touch1.
    Vector2 vecEnd1 = new Vector2(); // Vector for the end position of touch1.
    Vector2 vecStart2 = new Vector2(); // Vector for the start position of touch2.
    Vector2 vecEnd2 = new Vector2(); // Vector for the end position of touch2.
    Vector2 VecStart = new Vector2(); // Vector for the difference between the start position vectors of touch0 and touch1.
    Vector2 VecEnd = new Vector2(); // Vector for the difference between the end position vectors of touch0 and touch1.
    float totalX0;
    float totalX1;
    float totalX2;
    float totalY0;
    float totalY1;
    float totalY2;
    float angle;

    float touchDuration = 0.0f; // How long the player has been holding on the screen for. Used to determine the difference between a hold and a tap/swipe/rotation.
    int touchRegister = 0; // Used to determine how many fingers have left the screen after initial touches have been made. Gestures are only recognized if this is equal to 3.
    bool[] hasRegistered = { false, false, false }; // For some reason TouchPhase.Began does not seem to be recognized. This fills a similar purpose, determining if the touch has been on the screen during a frame or not.

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
            if (activeScene.name.Equals("Agreement") || activeScene.name.Equals("Main_pre") || activeScene.name.Equals("Title_Screen") || (activeScene.name.Equals("Main") && (Player.want_exit == true)) || (activeScene.name.Equals("Main") && (Player.want_exit == false) && (Player.hasStartedConsent == true) && (Player.hasFinishedConsentForm == false)) || (activeScene.name.Equals("Main") && (Player.want_exit == false) && (Player.survey_activated == true)) || (activeScene.name.Equals("Main") && (Player.canDoGestureTutorial == true) && (Player.curLevel == 1)))
            {
                ievent.isSwipe = true; // A swipe was registered.
                ievent.isRight = true; // Register a right rotation.
                swipeRightTimes += 1; // Update the number of times the right arrow key has been pressed by swiping right.
                int totalRightTimes = swipeRightTimes + rotateRightTimes; // Get the total number of times the right arrow key has been pressed.
                debugInputInfo = "Right arrow key pressed " + totalRightTimes + " times.";
            }
            // For right rotations.
            else if (activeScene.name.Equals("Main") && (Player.want_exit == false) && (Player.hasFinishedConsentForm == true))
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
            if (activeScene.name.Equals("Agreement") || activeScene.name.Equals("Main_pre") || activeScene.name.Equals("Title_Screen") || (activeScene.name.Equals("Main") && (Player.want_exit == true)) || (activeScene.name.Equals("Main") && (Player.want_exit == false) && (Player.hasStartedConsent == true) && (Player.hasFinishedConsentForm == false)) || (activeScene.name.Equals("Main") && (Player.want_exit == false) && (Player.survey_activated == true)) || (activeScene.name.Equals("Main") && (Player.canDoGestureTutorial == true) && (Player.curLevel == 1)))
            {
                ievent.isSwipe = true; // A swipe was registered.
                ievent.isLeft = true; // Register a left rotation.
                swipeLeftTimes += 1; // Update the number of times the left arrow key has been pressed by swiping left.
                int totalLeftTimes = swipeLeftTimes + rotateLeftTimes; // Get the total number of times the left arrow key has been pressed.
                debugInputInfo = "Left arrow key pressed " + totalLeftTimes + " times.";
            }
            // For left rotations.
            else if (activeScene.name.Equals("Main") && (Player.want_exit == false) && (Player.hasFinishedConsentForm == true))
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
        bool wentOffscreen = false;

        // If touch0 or touch1 have moved and that touch has been registered, get the angle between them. 
        if (Input.touchCount >= 2)
        {
            Touch touch0 = Input.touches[0]; // Initialized to this on default.
            Touch touch1 = Input.touches[1]; // Initialized to this on default.
            bool touch0Registered = hasRegistered[0]; // Initialized to this on default.
            bool touch1Registered = hasRegistered[1]; // Initialized to this on default.

            if (touch0.phase != TouchPhase.Canceled)
            {
                if ((touch0.fingerId == 0) && (hasRegistered[0] == true))
                {
                    touch0Registered = hasRegistered[0];
                }
                else if ((touch0.fingerId == 1) && (hasRegistered[1] == true))
                {
                    touch0Registered = hasRegistered[1];
                }
                else if ((touch0.fingerId == 2) && (hasRegistered[2] == true))
                {
                    touch0Registered = hasRegistered[2];
                }
            }
            else if ((touch0.phase == TouchPhase.Canceled) && (Input.touchCount >= 3))
            {
                if ((Input.touches[1].phase != TouchPhase.Canceled) && (Input.touches[2].phase != TouchPhase.Canceled))
                {
                    touch0 = Input.touches[1];
                    touch1 = Input.touches[2];

                    if ((Input.touches[1].fingerId == 0) && (Input.touches[2].fingerId == 1))
                    {
                        if ((hasRegistered[0] == true) && (hasRegistered[1] == true))
                        {
                            touch0Registered = hasRegistered[0];
                            touch1Registered = hasRegistered[1];
                        }
                    }
                    if ((Input.touches[1].fingerId == 0) && (Input.touches[2].fingerId == 2))
                    {
                        if ((hasRegistered[0] == true) && (hasRegistered[2] == true))
                        {
                            touch0Registered = hasRegistered[0];
                            touch1Registered = hasRegistered[2];
                        }
                    }
                    if ((Input.touches[1].fingerId == 1) && (Input.touches[2].fingerId == 2))
                    {
                        if ((hasRegistered[1] == true) && (hasRegistered[2] == true))
                        {
                            touch0Registered = hasRegistered[1];
                            touch1Registered = hasRegistered[2];
                        }
                    }
                }
            }
            if (touch1.phase != TouchPhase.Canceled)
            {
                if ((touch1.fingerId == 0) && (hasRegistered[0] == true))
                {
                    touch1Registered = hasRegistered[0];
                }
                else if ((touch1.fingerId == 1) && (hasRegistered[1] == true))
                {
                    touch1Registered = hasRegistered[1];
                }
                else if ((touch1.fingerId == 2) && (hasRegistered[2] == true))
                {
                    touch1Registered = hasRegistered[2];
                }
            }
            else if ((touch1.phase == TouchPhase.Canceled) && (Input.touchCount >= 3))
            {
                if ((Input.touches[0].phase != TouchPhase.Canceled) && (Input.touches[2].phase != TouchPhase.Canceled))
                {
                    touch0 = Input.touches[0];
                    touch1 = Input.touches[2];

                    if ((Input.touches[0].fingerId == 0) && (Input.touches[2].fingerId == 1))
                    {
                        if ((hasRegistered[0] == true) && (hasRegistered[1] == true))
                        {
                            touch0Registered = hasRegistered[0];
                            touch1Registered = hasRegistered[1];
                        }
                    }
                    if ((Input.touches[0].fingerId == 0) && (Input.touches[2].fingerId == 2))
                    {
                        if ((hasRegistered[0] == true) && (hasRegistered[2] == true))
                        {
                            touch0Registered = hasRegistered[0];
                            touch1Registered = hasRegistered[2];
                        }
                    }
                    if ((Input.touches[0].fingerId == 1) && (Input.touches[2].fingerId == 2))
                    {
                        if ((hasRegistered[1] == true) && (hasRegistered[2] == true))
                        {
                            touch0Registered = hasRegistered[1];
                            touch1Registered = hasRegistered[2];
                        }
                    }
                }
            }

            if ((touch0Registered == true) && (touch1Registered == true))
            {
                Vector2 currentStart = touch1.position - touch0.position; // Get the difference between touch1's current position and touch0's current position.
                Vector2 currentEnd = new Vector2(1, 0); // Set the current end position.

                float currentAngle = Vector2.Angle(currentStart, currentEnd); // Get the angle between the current start and end positions.
                Vector3 currentCross = Vector3.Cross(currentStart, currentEnd); // Get the cross product between the current start and end positions.

                // If the x-component of the cross product is greater than zero.
                if (currentCross.z > 0.0f)
                {
                    currentAngle = 360.0f - currentAngle; // Flip the angle around the 'x-axis' (i.e. if the angle was 140, it would now be 220, as if you went 140 degrees in the other direction).
                }

                Vector2 touch0Diff = touch0.position - touch0.deltaPosition; // Get the difference between touch0's current position and its change in position since last frame.
                Vector2 touch1Diff = touch1.position - touch1.deltaPosition; // Get the difference between touch1's current position and its change in position since last frame.
                Vector2 previousStart = touch1Diff - touch0Diff; // Get the difference between touch1Diff and touch0Diff.
                Vector2 previousEnd = new Vector2(1, 0); // Set the previous end position.

                float previousAngle = Vector2.Angle(previousStart, previousEnd); // Get the angle between the previous start and end positions.
                Vector3 previousCross = Vector3.Cross(previousStart, previousEnd); // Get the cross product between the previous start and end positions.

                // If the x-component of the cross product is greater than zero.
                if (previousCross.z > 0.0f)
                {
                    previousAngle = 360.0f - previousAngle; // Flip the angle around the 'x-axis' (i.e. if the angle was 140, it would now be 220, as if you went 140 degrees in the other direction).
                }

                angle += Mathf.DeltaAngle(currentAngle, previousAngle); // Add the difference between the angle during the last frame and this frame to the total angle.

                VecStart = vecStart0 - vecStart1; // Get the vector between the start position vectors of touch0 and touch1.
                VecEnd = vecEnd0 - vecEnd1; // Get the vector between the end position vectors of touch0 and touch1. 
            }
        }

        foreach (Touch touch in Input.touches)
        {
            // If the touch has begun, get its current position.
            if (touch.phase == TouchPhase.Began)
            {
                if (touch.fingerId == 0)
                {
                    touchRegister = Input.touchCount;  // Update the number of touches that have left the screen.
                    vecStart0 = touch.position; // Set the start position vector for this touch.
                    vecEnd0 = touch.position; // Set the end position vector for this touch (should be the same as the start vector for now).
                    totalX0 = vecEnd0.x - vecStart0.x; // Get the total x distance covered by this finger (currently 0).
                    totalY0 = vecEnd0.y - vecStart0.y; // Get the total y distance covered by this finger (currently 0).
                    debugTouch0Info = "XStart: " + vecStart0.x.ToString() + "\nYStart: " + vecStart0.y.ToString() + "\nXEnd: " + vecEnd0.x.ToString() + "\nYEnd: " + vecEnd0.y.ToString();
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox. 
                    hasRegistered[0] = true; // This touch has been registered on the screen, so we can start updating its position if it moves.                    
                }
                else if (touch.fingerId == 1)
                {
                    touchRegister = Input.touchCount;  // Update the number of touches that have left the screen.
                    vecStart1 = touch.position; // Set the start position vector for this touch.
                    vecEnd1 = touch.position; // Set the end position vector for this touch (should be the same as the start vector for now).
                    totalX1 = vecEnd1.x - vecStart1.x; // Get the total x distance covered by this finger (currently 0).
                    totalY1 = vecEnd1.y - vecStart1.y; // Get the total y distance covered by this finger (currently 0).
                    debugTouch1Info = "XStart: " + vecStart1.x.ToString() + "\nYStart: " + vecStart1.y.ToString() + "\nXEnd: " + vecEnd1.x.ToString() + "\nYEnd: " + vecEnd1.y.ToString();
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox. 
                    hasRegistered[1] = true; // This touch has been registered on the screen, so we can start updating its position if it moves.    
                }
                else if (touch.fingerId == 2)
                {
                    touchRegister = Input.touchCount;  // Update the number of touches that have left the screen.
                    vecStart2 = touch.position; // Set the start position vector for this touch.
                    vecEnd2 = touch.position; // Set the end position vector for this touch (should be the same as the start vector for now).
                    totalX2 = vecEnd2.x - vecStart2.x; // Get the total x distance covered by this finger (currently 0).
                    totalY2 = vecEnd2.y - vecStart2.y; // Get the total y distance covered by this finger (currently 0).
                    debugTouch2Info = "XStart: " + vecStart2.x.ToString() + "\nYStart: " + vecStart2.y.ToString() + "\nXEnd: " + vecEnd2.x.ToString() + "\nYEnd: " + vecEnd2.y.ToString();
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox. 
                    hasRegistered[2] = true; // This touch has been registered on the screen, so we can start updating its position if it moves.
                }
            }
            // If the touch is stationary.
            if (touch.phase == TouchPhase.Stationary)
            {
                if ((touch.fingerId == 0) && (hasRegistered[0] == true))
                {
                    vecEnd0 = touch.position; // Update the end position vector of this touch.
                    totalX0 = vecEnd0.x - vecStart0.x; // Update the total x distance covered by this touch.
                    totalY0 = vecEnd0.y - vecStart0.y; // Update the total y distance covered by this touch.
                    debugTouch0Info = "XStart: " + vecStart0.x.ToString() + "\nYStart: " + vecStart0.y.ToString() + "\nXEnd: " + vecEnd0.x.ToString() + "\nYEnd: " + vecEnd0.y.ToString();
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                }
                else if ((touch.fingerId == 0) && (hasRegistered[0] == false))
                {
                    debugTouch2Info = "Touch0";
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox. 
                }
                else if ((touch.fingerId == 1) && (hasRegistered[1] == true))
                {
                    vecEnd1 = touch.position; // Update the end position vector of this touch.
                    totalX1 = vecEnd1.x - vecStart1.x; // Update the total x distance covered by this touch.
                    totalY1 = vecEnd1.y - vecStart1.y; // Update the total y distance covered by this touch.
                    debugTouch1Info = "XStart: " + vecStart1.x.ToString() + "\nYStart: " + vecStart1.y.ToString() + "\nXEnd: " + vecEnd1.x.ToString() + "\nYEnd: " + vecEnd1.y.ToString();
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                }
                else if ((touch.fingerId == 1) && (hasRegistered[1] == false))
                {
                    debugTouch2Info = "Touch1";
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox. 
                }
                else if ((touch.fingerId == 2) && (hasRegistered[2] == true))
                {
                    vecEnd2 = touch.position; // Update the end position vector of this touch.
                    totalX2 = vecEnd2.x - vecStart2.x; // Update the total x distance covered by this touch.
                    totalY2 = vecEnd2.y - vecStart2.y; // Update the total y distance covered by this touch.
                    debugTouch2Info = "XStart: " + vecStart2.x.ToString() + "\nYStart: " + vecStart2.y.ToString() + "\nXEnd: " + vecEnd2.x.ToString() + "\nYEnd: " + vecEnd2.y.ToString();
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.
                }
                else if ((touch.fingerId == 2) && (hasRegistered[2] == false))
                {
                    debugTouch2Info = "Touch2";
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox. 
                }
            }
            // If the touch has moved, update its position.
            if (touch.phase == TouchPhase.Moved)
            {
                if ((touch.fingerId == 0) && (hasRegistered[0] == true))
                {
                    vecEnd0 = touch.position; // Update the end position vector of this touch.
                    totalX0 = vecEnd0.x - vecStart0.x; // Update the total x distance covered by this touch.
                    totalY0 = vecEnd0.y - vecStart0.y; // Update the total y distance covered by this touch.
                    debugTouch0Info = "XStart: " + vecStart0.x.ToString() + "\nYStart: " + vecStart0.y.ToString() + "\nXEnd: " + vecEnd0.x.ToString() + "\nYEnd: " + vecEnd0.y.ToString();
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                }
                else if ((touch.fingerId == 0) && (hasRegistered[0] == false))
                {
                    debugTouch2Info = "Touch0";
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox. 
                }
                else if ((touch.fingerId == 1) && (hasRegistered[1] == true))
                {
                    vecEnd1 = touch.position; // Update the end position vector of this touch.
                    totalX1 = vecEnd1.x - vecStart1.x; // Update the total x distance covered by this touch.
                    totalY1 = vecEnd1.y - vecStart1.y; // Update the total y distance covered by this touch.
                    debugTouch1Info = "XStart: " + vecStart1.x.ToString() + "\nYStart: " + vecStart1.y.ToString() + "\nXEnd: " + vecEnd1.x.ToString() + "\nYEnd: " + vecEnd1.y.ToString();
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                }
                else if ((touch.fingerId == 1) && (hasRegistered[1] == false))
                {
                    debugTouch2Info = "Touch1";
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox. 
                }
                else if ((touch.fingerId == 2) && (hasRegistered[2] == true))
                {
                    vecEnd2 = touch.position; // Update the end position vector of this touch.
                    totalX2 = vecEnd2.x - vecStart2.x; // Update the total x distance covered by this touch.
                    totalY2 = vecEnd2.y - vecStart2.y; // Update the total y distance covered by this touch.
                    debugTouch2Info = "XStart: " + vecStart2.x.ToString() + "\nYStart: " + vecStart2.y.ToString() + "\nXEnd: " + vecEnd2.x.ToString() + "\nYEnd: " + vecEnd2.y.ToString();
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.
                }
                else if ((touch.fingerId == 2) && (hasRegistered[2] == false))
                {
                    debugTouch2Info = "Touch2";
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox. 
                }
            }

            // If the touch has ended, update its position.
            if (touch.phase == TouchPhase.Ended)
            {
                if ((touch.fingerId == 0) && (hasRegistered[0] == true))
                {
                    vecEnd0 = touch.position; // Update the end position vector of this touch.
                    totalX0 = vecEnd0.x - vecStart0.x; // Update the total x distance covered by this touch.
                    totalY0 = vecEnd0.y - vecStart0.y; // Update the total y distance covered by this touch.
                    debugTouch0Info = "XStart: " + vecStart0.x.ToString() + "\nYStart: " + vecStart0.y.ToString() + "\nXEnd: " + vecEnd0.x.ToString() + "\nYEnd: " + vecEnd0.y.ToString();
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                    hasRegistered[0] = false; // Touch0 is no longer on the screen. Make sure that the end position for this touch cannot be updated based on another finger's position.
                }
                else if ((touch.fingerId == 0) && (hasRegistered[0] == false))
                {
                    debugTouch2Info = "Touch0";
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox. 
                }
                else if ((touch.fingerId == 1) && (hasRegistered[1] == true))
                {
                    vecEnd1 = touch.position; // Update the end position vector of this touch.
                    totalX1 = vecEnd1.x - vecStart1.x; // Update the total x distance covered by this touch.
                    totalY1 = vecEnd1.y - vecStart1.y; // Update the total y distance covered by this touch.
                    debugTouch1Info = "XStart: " + vecStart1.x.ToString() + "\nYStart: " + vecStart1.y.ToString() + "\nXEnd: " + vecEnd1.x.ToString() + "\nYEnd: " + vecEnd1.y.ToString();
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                    hasRegistered[1] = false; // Touch1 is no longer on the screen. Make sure that the end position for this touch cannot be updated based on another finger's position.                    
                }
                else if ((touch.fingerId == 1) && (hasRegistered[1] == false))
                {
                    debugTouch2Info = "Touch1";
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox. 
                }
                else if ((touch.fingerId == 2) && (hasRegistered[2] == true))
                {
                    vecEnd2 = touch.position; // Update the end position vector of this touch.
                    totalX2 = vecEnd2.x - vecStart2.x; // Update the total x distance covered by this touch.
                    totalY2 = vecEnd2.y - vecStart2.y; // Update the total y distance covered by this touch.
                    debugTouch2Info = "XStart: " + vecStart2.x.ToString() + "\nYStart: " + vecStart2.y.ToString() + "\nXEnd: " + vecEnd2.x.ToString() + "\nYEnd: " + vecEnd2.y.ToString();
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.
                    hasRegistered[2] = false; // Touch2 is no longer on the screen. Make sure that the end position for this touch cannot be updated based on another finger's position.                    
                }
                else if ((touch.fingerId == 2) && (hasRegistered[2] == false))
                {
                    debugTouch2Info = "Touch2";
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox. 
                }
            }
            // If the touch was canceled.
            if (touch.phase == TouchPhase.Canceled)
            {
                if (touch.fingerId == 0)
                {
                    touchRegister = Input.touchCount;
                    debugTouch0Info = "Touch0 canceled";
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                }
                else if (touch.fingerId == 1)
                {
                    touchRegister = Input.touchCount;
                    debugTouch1Info = "Touch1 canceled";
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                }
                else if (touch.fingerId == 2)
                {
                    touchRegister = Input.touchCount;
                    debugTouch2Info = "Touch2 canceled";
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.
                }
            }
            // If something else happened.
            if ((touch.phase != TouchPhase.Began) && (touch.phase != TouchPhase.Stationary) && (touch.phase != TouchPhase.Moved) && (touch.phase != TouchPhase.Ended) && (touch.phase != TouchPhase.Canceled))
            {
                if (touch.fingerId == 0)
                {
                    debugTouch0Info = "Cannot compute";
                    DebugTouch0.instance.ChangeDebugTouch0Text(debugTouch0Info); // Update the debug textbox.
                    print("Touch0.phase: " + touch.phase.ToString() + ", hasRegistered: " + hasRegistered[0].ToString());
                }
                else if (touch.fingerId == 1)
                {
                    debugTouch1Info = "Cannot compute";
                    DebugTouch1.instance.ChangeDebugTouch1Text(debugTouch1Info); // Update the debug textbox.
                    print("Touch1.phase: " + touch.phase.ToString() + ", hasRegistered: " + hasRegistered[1].ToString());
                }
                else if (touch.fingerId == 2)
                {
                    debugTouch2Info = "Cannot compute";
                    DebugTouch2.instance.ChangeDebugTouch2Text(debugTouch2Info); // Update the debug textbox.
                    print("Touch2.phase: " + touch.phase.ToString() + ", hasRegistered: " + hasRegistered[2].ToString());
                }
            }
        }

        // If there is at least one touch currently on the screen.
        if (Input.touchCount > 0)
        {
            debugTouchDurationInfo = "Hold: " + touchDuration.ToString() + "\nAngle: " + angle.ToString() + "\nTouches: " + touchRegister.ToString();
            DebugTouchDuration.instance.ChangeDebugTouchDurationText(debugTouchDurationInfo); // Update the debug textbox.

            // If the player has not told us if they are using Talkback or not, accept two and three finger inputs.
            if (((Input.touchCount == 2) || (Input.touchCount == 3)) && (GM_title.determined_talkback == false))
            {
                // If a finger has not gone offscreen, check if one has this frame.
                if (wentOffscreen == false)
                {
                    // If a finger went off the screen.
                    if ((vecEnd0.x >= (Screen.width - 15.0f)) || (vecEnd1.x >= (Screen.width - 15.0f)) || (vecEnd0.x <= 15.0f) || (vecEnd1.x <= 15.0f) || (vecEnd0.y >= (Screen.height - 10.0f)) || (vecEnd1.y >= (Screen.height - 10.0f)) || (vecEnd0.y <= 5.0f) || (vecEnd1.y <= 5.0f))
                    {
                        wentOffscreen = true;
                    }
                    // If a finger went off the screen and there was three fingers on the screen.
                    else if ((Input.touchCount == 3) && (wentOffscreen == false))
                    {
                        if ((vecEnd2.x >= (Screen.width - 15.0f)) || (vecEnd2.x <= 15.0f) || (vecEnd2.y >= (Screen.height - 10.0f)) || (vecEnd2.y <= 5.0f))
                        {
                            wentOffscreen = true;
                        }
                    }
                }

                touchDuration = touchDuration + Time.deltaTime; // Update the length of the touch.                  

                debugTouchDurationInfo = "Hold: " + touchDuration.ToString() + "\nAngle: " + angle.ToString() + "\nTouches: " + touchRegister.ToString();
                DebugTouchDuration.instance.ChangeDebugTouchDurationText(debugTouchDurationInfo); // Update the debug textbox.
            }
            // If the player has told us they are not using Talkback and there are currently two or three fingers on the screen.
            else if (((Input.touchCount == 2) || (Input.touchCount == 3)) && (GM_title.determined_talkback == true) && (GM_title.isUsingTalkback == false))
            {
                // If a finger has not gone offscreen, check if one has this frame.
                if (wentOffscreen == false)
                {
                    // If a finger went off the screen.
                    if ((vecEnd0.x >= (Screen.width - 15.0f)) || (vecEnd1.x >= (Screen.width - 15.0f)) || (vecEnd0.x <= 15.0f) || (vecEnd1.x <= 15.0f) || (vecEnd0.y >= (Screen.height - 10.0f)) || (vecEnd1.y >= (Screen.height - 10.0f)) || (vecEnd0.y <= 5.0f) || (vecEnd1.y <= 5.0f))
                    {
                        wentOffscreen = true;
                    }
                    // If a finger went off the screen and there was three fingers on the screen.
                    else if ((Input.touchCount == 3) && (wentOffscreen == false))
                    {
                        if ((vecEnd2.x >= (Screen.width - 15.0f)) || (vecEnd2.x <= 15.0f) || (vecEnd2.y >= (Screen.height - 10.0f)) || (vecEnd2.y <= 5.0f))
                        {
                            wentOffscreen = true;
                        }
                    }
                }

                touchDuration = touchDuration + Time.deltaTime; // Update the length of the touch.         

                debugTouchDurationInfo = "Hold: " + touchDuration.ToString() + "\nAngle: " + angle.ToString() + "\nTouches: " + touchRegister.ToString();
                DebugTouchDuration.instance.ChangeDebugTouchDurationText(debugTouchDurationInfo); // Update the debug textbox.
            }
            // If the player has told us they are using Talkback and there are currently three fingers on the screen.
            else if ((Input.touchCount == 3) && (GM_title.determined_talkback == true) && (GM_title.isUsingTalkback == true))
            {
                // If a finger has not gone offscreen, check if one has this frame.
                if (wentOffscreen == false)
                {
                    // If a finger went off the screen.
                    if ((vecEnd0.x >= (Screen.width - 15.0f)) || (vecEnd1.x >= (Screen.width - 15.0f)) || (vecEnd2.x >= (Screen.width - 15.0f)) || (vecEnd0.x <= 15.0f) || (vecEnd1.x <= 15.0f) || (vecEnd2.x <= 15.0f) || (vecEnd0.y >= (Screen.height - 10.0f)) || (vecEnd1.y >= (Screen.height - 10.0f)) || (vecEnd2.y >= (Screen.height - 10.0f)) || (vecEnd0.y <= 5.0f) || (vecEnd1.y <= 5.0f) || (vecEnd2.y <= 5.0f))
                    {
                        wentOffscreen = true;
                    }
                }

                touchDuration = touchDuration + Time.deltaTime; // Update the length of the touch.

                debugTouchDurationInfo = "Hold: " + touchDuration.ToString() + "\nAngle: " + angle.ToString() + "\nTouches: " + touchRegister.ToString();
                DebugTouchDuration.instance.ChangeDebugTouchDurationText(debugTouchDurationInfo); // Update the debug textbox.
            }
        }

        // If there are currently no fingers on the screen, determine if a tap/swipe/rotation gesture was made.
        if (Input.touchCount == 0)
        {
            bool canMakeGesture = false;

            // If one finger was on the screen.
            if (touchRegister == 1)
            {
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                bool hasWindowFocus = activity.Call<bool>("hasWindowFocus");

                if ((hasWindowFocus == true) && (touchDuration > 0.0f))
                {
                    unrecognizedTimes += 1;
                    debugInputInfo = "Only one finger was registered as leaving the screen. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                    touchDuration = 0.0f; // Reset duration of touch duration to 0, as nothing is touch the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }
#endif
#if UNITY_EDITOR
                if (touchDuration > 0.0f)
                {
                    unrecognizedTimes += 1;
                    debugInputInfo = "Only one finger was registered as leaving the screen. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                    touchDuration = 0.0f; // Reset duration of touch duration to 0, as nothing is touch the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }
#endif
#if UNITY_IOS || UNITY_ANDROID
            }

            // If two fingers were on the screen.
            else if (touchRegister == 2)
            {
                // If the player has not informed us if they are using Talkback, they can make a gesture.
                if (GM_title.determined_talkback == false)
                {
                    canMakeGesture = true;
                }
                // If the player has informed us that they are not using Talkback, they can make a gesture.
                else if ((GM_title.determined_talkback == true) && (GM_title.isUsingTalkback == false))
                {
                    canMakeGesture = true;
                }
                // If the player has informed us that they are using Talkback, they cannot make a gesture.
                else if ((GM_title.determined_talkback == true) && (GM_title.isUsingTalkback == true))
                {
                    canMakeGesture = false;
                }

                // If the player is able to make a gesture.
                if (canMakeGesture == true)
                {
                    // If a finger went off the screen.
                    if ((wentOffscreen == true) && (touchDuration > 0.0f))
                    {
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "At least one finger went off the screen. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        List<AudioClip> clips = new List<AudioClip>() { Database.errorClips[24] };
                        SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true); // Play the appropriate clips.
                    }

                    // If the gesture was a tap.
                    else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && (Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && (Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && (Mathf.Abs(angle) <= 40.0f))
                    {
                        ievent.isTap = true; // A tap was registered.
                        tapTimes += 1; // Update the number of times a tap was made.
                        debugInputInfo = "Tapped " + tapTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a left or right swipe.
                    else if ((touchDuration > 0.0f) && (Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX1) > Mathf.Abs(totalY1)) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist) && (Mathf.Abs(angle) <= 40.0f))
                    {
                        // Swipe left detected.
                        if ((totalX0 < 0.0f) && (totalX1 < 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isLeft = true; // The swipe was left.
                            swipeLeftTimes += 1; // Update the number of times a swipe left was made.
                            debugInputInfo = "Swiped left " + swipeLeftTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                        // Swipe right detected.
                        else if ((totalX0 > 0.0f) && (totalX1 > 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isRight = true; // The swipe was right.
                            swipeRightTimes += 1; // Update the number of times a swipe right was made.
                            debugInputInfo = "Swiped right " + swipeRightTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                    }

                    // If the gesture was an up or down swipe.
                    else if ((touchDuration > 0.0f) && (Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist) && (Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist) && (Mathf.Abs(angle) <= 40.0f))
                    {
                        // Swipe up detected.
                        if ((totalY0 > 0.0f) && (totalY1 > 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isUp = true; // The swipe was right.
                            swipeUpTimes += 1; // Update the number of times a swipe up was made.
                            debugInputInfo = "Swiped up " + swipeUpTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                        // Swipe down detected.
                        else if ((totalY0 < 0.0f) && (totalY1 < 0.0f))
                        {
                            ievent.isSwipe = true; // A swipe was registered.
                            ievent.isDown = true; // The swipe was down.
                            swipeDownTimes += 1; // Update the number of times a swipe down was made.
                            debugInputInfo = "Swiped down " + swipeDownTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.      
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                    }

                    // If the gesture was a left turn.
                    else if ((touchDuration > 0.0f) && (angle <= (-45.0f)))
                    {
                        ievent.isRotate = true; // A rotation was registered.
                        ievent.isLeft = true; // The rotation was left.
                        rotateLeftTimes += 1; // Update the number of times a left rotation was made.
                        debugInputInfo = "Rotated left " + rotateLeftTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a right turn.
                    else if ((touchDuration > 0.0f) && (angle >= 45.0f))
                    {
                        ievent.isRotate = true; // A rotation was registered.
                        ievent.isRight = true; // The rotation was right.
                        rotateRightTimes += 1; // Update the number of times a right rotation was made.
                        debugInputInfo = "Rotated right " + rotateRightTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a hold.
                    else if ((touchDuration >= 1.0f) && (Mathf.Abs(totalX0) <= maxHoldHorizontalDist) && (Mathf.Abs(totalX1) <= maxHoldHorizontalDist) && (Mathf.Abs(totalY0) <= maxHoldVerticalDist) && (Mathf.Abs(totalY1) <= maxHoldVerticalDist) && (Mathf.Abs(angle) <= 40.0f))
                    {
                        ievent.isHold = true; // A hold was registered.
                        holdTimes += 1; // Update the number of times a hold was made.
                        debugInputInfo = "Hold registered " + holdTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.       
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a tap with too much rotation.
                    else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && (Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && (Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && (Mathf.Abs(angle) > 40.0f))
                    {
                        ievent.isTapRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Tap rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.         
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a tap with too much horizontal movement.
                    else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && ((Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && (Mathf.Abs(totalX1) > maxTapHorizontalDist)) || ((Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && (Mathf.Abs(totalX0) > maxTapHorizontalDist)))
                    {
                        ievent.isTapHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Tap horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a tap with too much vertical movement.
                    else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && ((Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && (Mathf.Abs(totalY1) > maxTapVerticalDist)) || ((Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && (Mathf.Abs(totalY0) > maxTapVerticalDist)))
                    {
                        ievent.isTapVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Tap vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a tap with too much horizontal and vertical movement.
                    else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && (((Mathf.Abs(totalX0) > maxTapHorizontalDist) && (Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) <= maxTapHorizontalDist)) || ((Mathf.Abs(totalX1) > maxTapHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) <= maxTapHorizontalDist))) && (((Mathf.Abs(totalY0) > maxTapVerticalDist) && (Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist)) || ((Mathf.Abs(totalY1) > maxTapVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist))))
                    {
                        ievent.isTapHorizontalVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Tap horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a left or right swipe with too much rotation.
                    else if ((touchDuration > 0.0f) && (Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX1) > Mathf.Abs(totalY1)) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist) && (Mathf.Abs(angle) > 40.0f))
                    {
                        // Swipe left detected.
                        if ((totalX0 < 0.0f) && (totalX1 < 0.0f))
                        {
                            ievent.isSwipeLeftRotationError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe left rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                        // Swipe right detected.
                        else if ((totalX0 > 0.0f) && (totalX1 > 0.0f))
                        {
                            ievent.isSwipeRightRotationError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe right rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                    }

                    // If the gesture was a left or right swipe with not enough horizontal movement.
                    else if ((touchDuration > 0.0f) && (Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && (Mathf.Abs(totalX1) > Mathf.Abs(totalY1)) && (((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) > maxTapHorizontalDist) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) > maxTapHorizontalDist) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist))))
                    {
                        // Swipe left detected.
                        if ((totalX0 < 0.0f) && (totalX1 < 0.0f))
                        {
                            ievent.isSwipeLeftHorizontalError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe left horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                        // Swipe right detected.
                        else if ((totalX0 > 0.0f) && (totalX1 > 0.0f))
                        {
                            ievent.isSwipeRightHorizontalError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe right horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                    }

                    // If the gesture was an up or down swipe with too much rotation.
                    else if ((touchDuration > 0.0f) && (Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist) && (Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist) && (Mathf.Abs(angle) > 40.0f))
                    {
                        // Swipe up detected.
                        if ((totalY0 > 0.0f) && (totalY1 > 0.0f))
                        {
                            ievent.isSwipeUpRotationError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe up rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                        // Swipe down detected.
                        else if ((totalY0 < 0.0f) && (totalY1 < 0.0f))
                        {
                            ievent.isSwipeDownRotationError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe down rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                    }

                    // If the gesture was an up or down swipe with not enough vertical movement.
                    else if ((touchDuration > 0.0f) && (Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && (((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) > maxTapVerticalDist) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) > maxTapVerticalDist) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist))))
                    {
                        // Swipe up detected.
                        if ((totalY0 > 0.0f) && (totalY1 > 0.0f))
                        {
                            ievent.isSwipeUpVerticalError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe up vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                        // Swipe down detected.
                        else if ((totalY0 < 0.0f) && (totalY1 < 0.0f))
                        {
                            ievent.isSwipeDownVerticalError = true; // This error was registered.
                            ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                            unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                            debugInputInfo = "Swipe down vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                            DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                        }
                    }

                    // If there was an error determining the direction of a swipe.
                    else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY1) < Mathf.Abs(totalX1))) || ((Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && (Mathf.Abs(totalY0) < Mathf.Abs(totalX0)))) && (((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist))) && (((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist))))
                    {
                        ievent.isSwipeDirectionError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Cannot determine if the swipe was horizontal or vertical. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If a swipe had not enough horizontal and vertical movement.
                    else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && (Mathf.Abs(totalX1) < Mathf.Abs(totalY1))) || ((Mathf.Abs(totalX1) > Mathf.Abs(totalY1)) && (Mathf.Abs(totalX0) < Mathf.Abs(totalY0)))) && (((Mathf.Abs(totalX0) >= minSwipeHorizontalDist) && ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) > maxHoldHorizontalDist))) || ((Mathf.Abs(totalX1) >= minSwipeHorizontalDist) && ((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) > maxHoldHorizontalDist)))))
                    {
                        ievent.isSwipeHorizontalVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If a swipe had not enough horizontal and vertical movement.
                    else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY1) < Mathf.Abs(totalX1))) || ((Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && (Mathf.Abs(totalY0) < Mathf.Abs(totalX0)))) && (((Mathf.Abs(totalY0) >= minSwipeVerticalDist) && ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) > maxHoldVerticalDist))) || ((Mathf.Abs(totalY1) >= minSwipeVerticalDist) && ((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) > maxHoldVerticalDist)))))
                    {
                        ievent.isSwipeHorizontalVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If both fingers moved too much for a tap and too little for a swipe.
                    else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && ((((Mathf.Abs(totalX0) > maxTapHorizontalDist) && (Mathf.Abs(totalX0) < minSwipeHorizontalDist)) && ((Mathf.Abs(totalX1) > maxTapHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist))) || (((Mathf.Abs(totalY0) > maxTapVerticalDist) && (Mathf.Abs(totalY0) < minSwipeVerticalDist)) && ((Mathf.Abs(totalY1) > maxTapVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist)))))
                    {
                        ievent.isBetweenTapSwipeError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Gesture not recognized as either a tap or a swipe. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If both fingers moved too much for a hold and too little for a swipe.
                    else if ((touchDuration >= 1.0f) && ((((Mathf.Abs(totalX0) > maxTapHorizontalDist) && (Mathf.Abs(totalX0) < minSwipeHorizontalDist)) && ((Mathf.Abs(totalX1) > maxTapHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist))) || (((Mathf.Abs(totalY0) > maxTapVerticalDist) && (Mathf.Abs(totalY0) < minSwipeVerticalDist)) && ((Mathf.Abs(totalY1) > maxTapVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist)))))
                    {
                        ievent.isBetweenHoldSwipeError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Gesture not recognized as either a hold or a swipe. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a hold with too much rotation.
                    else if ((touchDuration >= 1.0f) && (Mathf.Abs(totalX0) <= maxHoldHorizontalDist) && (Mathf.Abs(totalY0) <= maxHoldVerticalDist) && (Mathf.Abs(totalX1) <= maxHoldHorizontalDist) && (Mathf.Abs(totalY1) <= maxHoldVerticalDist) && (Mathf.Abs(angle) > 40.0f))
                    {
                        ievent.isHoldRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Hold rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.         
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a hold with too much horizontal movement.
                    else if ((touchDuration >= 1.0f) && ((Mathf.Abs(totalX0) <= maxHoldHorizontalDist) && (Mathf.Abs(totalY0) <= maxHoldVerticalDist) && (Mathf.Abs(totalX1) > maxHoldHorizontalDist)) || ((Mathf.Abs(totalX1) <= maxHoldHorizontalDist) && (Mathf.Abs(totalY1) <= maxHoldVerticalDist) && (Mathf.Abs(totalX0) > maxHoldHorizontalDist)))
                    {
                        ievent.isHoldHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Hold horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a hold with too much vertical movement.
                    else if ((touchDuration >= 1.0f) && ((Mathf.Abs(totalX0) <= maxHoldHorizontalDist) && (Mathf.Abs(totalY0) <= maxHoldVerticalDist) && (Mathf.Abs(totalY1) > maxHoldVerticalDist)) || ((Mathf.Abs(totalX1) <= maxHoldHorizontalDist) && (Mathf.Abs(totalY1) <= maxHoldVerticalDist) && (Mathf.Abs(totalY0) > maxHoldVerticalDist)))
                    {
                        ievent.isHoldVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Hold vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a hold with too much horizontal and vertical movement.
                    else if ((touchDuration >= 1.0f) && (((Mathf.Abs(totalX0) > maxHoldHorizontalDist) && (Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) <= maxHoldHorizontalDist)) || ((Mathf.Abs(totalX1) > maxHoldHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) <= maxHoldHorizontalDist))) && (((Mathf.Abs(totalY0) > maxHoldVerticalDist) && (Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) <= maxHoldVerticalDist)) || ((Mathf.Abs(totalY1) > maxHoldVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) <= maxHoldVerticalDist))))
                    {
                        ievent.isHoldHorizontalVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Hold horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If the gesture was a turn with not enough rotation.
                    else if ((touchDuration > 0.0f) && (Mathf.Abs(angle) < 45.0f))
                    {
                        ievent.isRotationAngleError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Turn rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }

                    // If some other gesture error was recognized.
                    else
                    {
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Other gesture error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                        print("TotalX0: " + totalX0.ToString() + ", TotalX1: " + totalX1.ToString());
                        print("TotalY0: " + totalY0.ToString() + ", TotalY1: " + totalY1.ToString());
                        print("Hold: " + touchDuration.ToString() + ", Angle: " + angle.ToString());
                        print("MaxTapHorizontal: " + maxTapHorizontalDist.ToString() + ", MaxTapVertical: " + maxTapVerticalDist.ToString());
                        print("MinSwipeHorizontal: " + minSwipeHorizontalDist.ToString() + ", MinSwipeVertical: " + minSwipeVerticalDist.ToString());
                        print("MaxHoldHorizontal: " + maxHoldHorizontalDist.ToString() + ", MaxHoldVertical: " + maxHoldVerticalDist.ToString());
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                }

                // If the player cannot make a gesture with two fingers at this time.
                else if (canMakeGesture == false)
                {
                    debugInputInfo = "Cannot make gesture with two fingers because you are using Talkback.";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }
            }

            // If three fingers were on the screen.
            else if (touchRegister == 3)
            {
                // If a finger went off the screen.
                if ((wentOffscreen == true) && (touchDuration > 0.0f))
                {
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "At least one finger went off the screen. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    List<AudioClip> clips = new List<AudioClip>() { Database.errorClips[24] };
                    SoundManager.instance.PlayClips(clips, null, 0, null, 0, null, true);
                }

                // If the gesture was a tap.
                else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && (Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && (Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && (Mathf.Abs(totalX2) <= maxTapHorizontalDist) && (Mathf.Abs(totalY2) <= maxTapVerticalDist) && (Mathf.Abs(angle) <= 40.0f))
                {
                    ievent.isTap = true; // A tap was registered.
                    tapTimes += 1; // Update the number of times a tap was made.
                    debugInputInfo = "Tapped " + tapTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a left or right swipe.
                else if ((touchDuration > 0.0f) && (Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX1) > Mathf.Abs(totalY1)) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX2) > Mathf.Abs(totalY2)) && (Mathf.Abs(totalX2) >= minSwipeHorizontalDist) && (Mathf.Abs(angle) <= 40.0f))
                {
                    // Swipe left detected.
                    if ((totalX0 < 0.0f) && (totalX1 < 0.0f) && (totalX2 < 0.0f))
                    {
                        ievent.isSwipe = true; // A swipe was registered.
                        ievent.isLeft = true; // The swipe was left.
                        swipeLeftTimes += 1; // Update the number of times a swipe left was made.
                        debugInputInfo = "Swiped left " + swipeLeftTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                    // Swipe right detected.
                    else if ((totalX0 > 0.0f) && (totalX1 > 0.0f) && (totalX2 > 0.0f))
                    {
                        ievent.isSwipe = true; // A swipe was registered.
                        ievent.isRight = true; // The swipe was right.
                        swipeRightTimes += 1; // Update the number of times a swipe right was made.
                        debugInputInfo = "Swiped right " + swipeRightTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                }

                // If the gesture was an up or down swipe.
                else if ((touchDuration > 0.0f) && (Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist) && (Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist) && (Mathf.Abs(totalY2) > Mathf.Abs(totalX2)) && (Mathf.Abs(totalY2) >= minSwipeVerticalDist) && (Mathf.Abs(angle) <= 40.0f))
                {
                    // Swipe up detected.
                    if ((totalY0 > 0.0f) && (totalY1 > 0.0f) && (totalY2 > 0.0f))
                    {
                        ievent.isSwipe = true; // A swipe was registered.
                        ievent.isUp = true; // The swipe was right.
                        swipeUpTimes += 1; // Update the number of times a swipe up was made.
                        debugInputInfo = "Swiped up " + swipeUpTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                    // Swipe down detected.
                    else if ((totalY0 < 0.0f) && (totalY1 < 0.0f) && (totalY2 < 0.0f))
                    {
                        ievent.isSwipe = true; // A swipe was registered.
                        ievent.isDown = true; // The swipe was down.
                        swipeDownTimes += 1; // Update the number of times a swipe down was made.
                        debugInputInfo = "Swiped down " + swipeDownTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.      
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                }

                // If the gesture was a left turn.
                else if ((touchDuration > 0.0f) && (angle <= (-45.0f)))
                {
                    ievent.isRotate = true; // A rotation was registered.
                    ievent.isLeft = true; // The rotation was left.
                    rotateLeftTimes += 1; // Update the number of times a left rotation was made.
                    debugInputInfo = "Rotated left " + rotateLeftTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a right turn.
                else if ((touchDuration > 0.0f) && (angle >= 45.0f))
                {
                    ievent.isRotate = true; // A rotation was registered.
                    ievent.isRight = true; // The rotation was right.
                    rotateRightTimes += 1; // Update the number of times a right rotation was made.
                    debugInputInfo = "Rotated right " + rotateRightTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a hold.
                else if ((touchDuration >= 1.0f) && (Mathf.Abs(totalX0) <= maxHoldHorizontalDist) && (Mathf.Abs(totalX1) <= maxHoldHorizontalDist) && (Mathf.Abs(totalX2) <= maxHoldHorizontalDist) && (Mathf.Abs(totalY0) <= maxHoldVerticalDist) && (Mathf.Abs(totalY1) <= maxHoldVerticalDist) && (Mathf.Abs(totalY2) <= maxHoldVerticalDist) && (Mathf.Abs(angle) <= 40.0f))
                {
                    ievent.isHold = true; // A hold was registered.
                    holdTimes += 1; // Update the number of times a hold was made.
                    debugInputInfo = "Hold registered " + holdTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.       
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a tap with too much rotation.
                else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && (Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && (Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && (Mathf.Abs(totalX2) <= maxTapHorizontalDist) && (Mathf.Abs(totalY2) <= maxTapVerticalDist) && (Mathf.Abs(angle) > 40.0f))
                {
                    ievent.isTapRotationError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Tap rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.         
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a tap with too much horizontal movement.
                else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && (((Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && ((Mathf.Abs(totalX1) > maxTapHorizontalDist) || (Mathf.Abs(totalX2) > maxTapHorizontalDist))) || ((Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && ((Mathf.Abs(totalX0) > maxTapHorizontalDist) || (Mathf.Abs(totalX2) > maxTapHorizontalDist))) || ((Mathf.Abs(totalX2) <= maxTapHorizontalDist) && (Mathf.Abs(totalY2) <= maxTapVerticalDist) && ((Mathf.Abs(totalX0) > maxTapHorizontalDist) || (Mathf.Abs(totalX1) > maxTapHorizontalDist)))))
                {
                    ievent.isTapHorizontalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Tap horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a tap with too much vertical movement.
                else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && (((Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && ((Mathf.Abs(totalY1) > maxTapVerticalDist) || (Mathf.Abs(totalY2) > maxTapVerticalDist))) || ((Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && ((Mathf.Abs(totalY0) > maxTapVerticalDist) || (Mathf.Abs(totalY2) > maxTapVerticalDist))) || ((Mathf.Abs(totalX2) <= maxTapHorizontalDist) && (Mathf.Abs(totalY2) <= maxTapVerticalDist) && ((Mathf.Abs(totalY0) > maxTapVerticalDist) || (Mathf.Abs(totalY1) > maxTapVerticalDist)))))
                {
                    ievent.isTapVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Tap vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a tap with too much horizontal and vertical movement.
                else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && (((Mathf.Abs(totalX0) > maxTapHorizontalDist) && (Mathf.Abs(totalX0) < minSwipeHorizontalDist) && ((Mathf.Abs(totalX1) <= maxTapHorizontalDist) || (Mathf.Abs(totalX2) <= maxTapHorizontalDist))) || ((Mathf.Abs(totalX1) > maxTapHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist) && ((Mathf.Abs(totalX0) <= maxTapHorizontalDist) || (Mathf.Abs(totalX2) <= maxTapHorizontalDist))) || ((Mathf.Abs(totalX2) > maxTapHorizontalDist) && (Mathf.Abs(totalX2) < minSwipeHorizontalDist) && ((Mathf.Abs(totalX0) <= maxTapHorizontalDist) || (Mathf.Abs(totalX1) <= maxTapHorizontalDist)))) && (((Mathf.Abs(totalY0) > maxTapVerticalDist) && (Mathf.Abs(totalY0) < minSwipeVerticalDist) && ((Mathf.Abs(totalY1) <= maxTapVerticalDist) || (Mathf.Abs(totalY2) <= maxTapVerticalDist))) || ((Mathf.Abs(totalY1) > maxTapVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist) && ((Mathf.Abs(totalY0) <= maxTapVerticalDist) || (Mathf.Abs(totalY2) <= maxTapVerticalDist))) || ((Mathf.Abs(totalY2) > maxTapVerticalDist) && (Mathf.Abs(totalY2) < minSwipeVerticalDist) && ((Mathf.Abs(totalY0) <= maxTapVerticalDist) || (Mathf.Abs(totalY1) <= maxTapVerticalDist)))))
                {
                    ievent.isTapHorizontalVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Tap horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a left or right swipe with too much rotation.
                else if ((touchDuration > 0.0f) && (Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX1) > Mathf.Abs(totalY1)) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX2) > Mathf.Abs(totalY2)) && (Mathf.Abs(totalX2) >= minSwipeHorizontalDist) && (Mathf.Abs(angle) > 40.0f))
                {
                    // Swipe left detected.
                    if ((totalX0 < 0.0f) && (totalX1 < 0.0f) && (totalX2 < 0.0f))
                    {
                        ievent.isSwipeLeftRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe left rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                    // Swipe right detected.
                    else if ((totalX0 > 0.0f) && (totalX1 > 0.0f) && (totalX2 > 0.0f))
                    {
                        ievent.isSwipeRightRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe right rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                }

                // If the gesture was a left or right swipe with not enough horizontal movement.
                else if ((touchDuration > 0.0f) && (Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && (Mathf.Abs(totalX1) > Mathf.Abs(totalY1)) && (Mathf.Abs(totalX2) > Mathf.Abs(totalY2)) && (((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) > maxTapHorizontalDist) && ((Mathf.Abs(totalX1) >= minSwipeHorizontalDist) || (Mathf.Abs(totalX2) >= minSwipeHorizontalDist))) || ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) > maxTapHorizontalDist) && ((Mathf.Abs(totalX0) >= minSwipeHorizontalDist) || (Mathf.Abs(totalX2) >= minSwipeHorizontalDist))) || ((Mathf.Abs(totalX2) < minSwipeHorizontalDist) && (Mathf.Abs(totalX2) > maxTapHorizontalDist) && ((Mathf.Abs(totalX0) >= minSwipeHorizontalDist) || (Mathf.Abs(totalX1) >= minSwipeHorizontalDist)))))
                {
                    // Swipe left detected.
                    if (((totalX0 > 0.0f) && (totalX1 > 0.0f)) || ((totalX0 > 0.0f) && (totalX2 > 0.0f)) || ((totalX1 > 0.0f) && (totalX2 > 0.0f)))
                    {
                        ievent.isSwipeLeftHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe left horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                    // Swipe right detected.
                    else if (((totalX0 < 0.0f) && (totalX1 < 0.0f)) || ((totalX0 < 0.0f) && (totalX2 < 0.0f)) || ((totalX1 < 0.0f) && (totalX2 < 0.0f)))
                    {
                        ievent.isSwipeRightHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe right horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                }

                // If the gesture was a left or right swipe with not enough horizontal movement.
                else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && (Mathf.Abs(totalX1) > Mathf.Abs(totalY1))) || ((Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && (Mathf.Abs(totalX2) > Mathf.Abs(totalY2))) || ((Mathf.Abs(totalX1) > Mathf.Abs(totalY1)) && (Mathf.Abs(totalX2) > Mathf.Abs(totalY2)))) && (((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) > maxTapHorizontalDist) && ((Mathf.Abs(totalX1) >= minSwipeHorizontalDist) || (Mathf.Abs(totalX2) >= minSwipeHorizontalDist))) || ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) > maxTapHorizontalDist) && ((Mathf.Abs(totalX0) >= minSwipeHorizontalDist) || (Mathf.Abs(totalX2) >= minSwipeHorizontalDist))) || ((Mathf.Abs(totalX2) < minSwipeHorizontalDist) && (Mathf.Abs(totalX2) > maxTapHorizontalDist) && ((Mathf.Abs(totalX0) >= minSwipeHorizontalDist) || (Mathf.Abs(totalX1) >= minSwipeHorizontalDist)))))
                {
                    // Swipe left detected.
                    if (((totalX0 > 0.0f) && (totalX1 > 0.0f)) || ((totalX0 > 0.0f) && (totalX2 > 0.0f)) || ((totalX1 > 0.0f) && (totalX2 > 0.0f)))
                    {
                        ievent.isSwipeLeftHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe left horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                    // Swipe right detected.
                    else if (((totalX0 < 0.0f) && (totalX1 < 0.0f)) || ((totalX0 < 0.0f) && (totalX2 < 0.0f)) || ((totalX1 < 0.0f) && (totalX2 < 0.0f)))
                    {
                        ievent.isSwipeRightHorizontalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe right horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                }

                // If the gesture was an up or down swipe with too much rotation.
                else if ((touchDuration > 0.0f) && (Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist) && (Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist) && (Mathf.Abs(totalY2) > Mathf.Abs(totalX2)) && (Mathf.Abs(totalY2) >= minSwipeVerticalDist) && (Mathf.Abs(angle) > 40.0f))
                {
                    // Swipe up detected.
                    if ((totalY0 > 0.0f) && (totalY1 > 0.0f) && (totalY2 > 0.0f))
                    {
                        ievent.isSwipeUpRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe up rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                    // Swipe down detected.
                    else if ((totalY0 < 0.0f) && (totalY1 < 0.0f) && (totalY2 < 0.0f))
                    {
                        ievent.isSwipeDownRotationError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe down rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                }

                // If the gesture was an up or down swipe with not enough vertical movement.
                else if ((touchDuration > 0.0f) && (Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && (Mathf.Abs(totalY2) > Mathf.Abs(totalX2)) && (((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) > maxTapVerticalDist) && ((Mathf.Abs(totalY1) >= minSwipeVerticalDist) || (Mathf.Abs(totalY2) >= minSwipeVerticalDist))) || ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) > maxTapVerticalDist) && ((Mathf.Abs(totalY0) >= minSwipeVerticalDist) || (Mathf.Abs(totalY2) >= minSwipeVerticalDist))) || ((Mathf.Abs(totalY2) < minSwipeVerticalDist) && (Mathf.Abs(totalY2) > maxTapVerticalDist) && ((Mathf.Abs(totalY0) >= minSwipeVerticalDist) || (Mathf.Abs(totalY1) >= minSwipeVerticalDist)))))
                {
                    // Swipe up detected.
                    if (((totalY0 > 0.0f) && (totalY1 > 0.0f)) || ((totalY0 > 0.0f) && (totalY2 > 0.0f)) || ((totalY1 > 0.0f) && (totalY2 > 0.0f)))
                    {
                        ievent.isSwipeUpVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe up vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                    // Swipe down detected.
                    else if (((totalY0 < 0.0f) && (totalY1 < 0.0f)) || ((totalY0 < 0.0f) && (totalY2 < 0.0f)) || ((totalY1 < 0.0f) && (totalY2 < 0.0f)))
                    {
                        ievent.isSwipeDownVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe down vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                }

                // If the gesture was an up or down swipe with not enough vertical movement.
                else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY1) > Mathf.Abs(totalX1))) || ((Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && (Mathf.Abs(totalY2) > Mathf.Abs(totalX2))) || ((Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && (Mathf.Abs(totalY2) > Mathf.Abs(totalX2)))) && (((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) > maxTapVerticalDist) && ((Mathf.Abs(totalY1) >= minSwipeVerticalDist) || (Mathf.Abs(totalY2) >= minSwipeVerticalDist))) || ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) > maxTapVerticalDist) && ((Mathf.Abs(totalY0) >= minSwipeVerticalDist) || (Mathf.Abs(totalY2) >= minSwipeVerticalDist))) || ((Mathf.Abs(totalY2) < minSwipeVerticalDist) && (Mathf.Abs(totalY2) > maxTapVerticalDist) && ((Mathf.Abs(totalY0) >= minSwipeVerticalDist) || (Mathf.Abs(totalY1) >= minSwipeVerticalDist)))))
                {
                    // Swipe up detected.
                    if (((totalY0 > 0.0f) && (totalY1 > 0.0f)) || ((totalY0 > 0.0f) && (totalY2 > 0.0f)) || ((totalY1 > 0.0f) && (totalY2 > 0.0f)))
                    {
                        ievent.isSwipeUpVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe up vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                    // Swipe down detected.
                    else if (((totalY0 < 0.0f) && (totalY1 < 0.0f)) || ((totalY0 < 0.0f) && (totalY2 < 0.0f)) || ((totalY1 < 0.0f) && (totalY2 < 0.0f)))
                    {
                        ievent.isSwipeDownVerticalError = true; // This error was registered.
                        ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                        unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                        debugInputInfo = "Swipe down vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                        DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                        touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                        touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                    }
                }

                // If there was an error determining the direction of a swipe.
                else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalY0) < Mathf.Abs(totalX0)) && (Mathf.Abs(totalY1) < Mathf.Abs(totalX1))) || ((Mathf.Abs(totalY0) < Mathf.Abs(totalX0)) && (Mathf.Abs(totalY2) < Mathf.Abs(totalX2))) || ((Mathf.Abs(totalY1) < Mathf.Abs(totalX1)) && (Mathf.Abs(totalY2) < Mathf.Abs(totalX2)))) && ((((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX2) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX2) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX2) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist))) || (((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY2) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY2) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY2) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist)))))
                {
                    ievent.isSwipeDirectionError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Cannot determine if the swipe was horizontal or vertical. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If there was an error determining the direction of a swipe.
                else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalX0) < Mathf.Abs(totalY0)) && (Mathf.Abs(totalX1) < Mathf.Abs(totalY1))) || ((Mathf.Abs(totalX0) < Mathf.Abs(totalY0)) && (Mathf.Abs(totalX2) < Mathf.Abs(totalY2))) || ((Mathf.Abs(totalX1) < Mathf.Abs(totalY1)) && (Mathf.Abs(totalX2) < Mathf.Abs(totalY2)))) && ((((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX2) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX2) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX2) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist))) || (((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY2) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY2) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY2) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist)))))
                {
                    ievent.isSwipeDirectionError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Cannot determine if the swipe was horizontal or vertical. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If there was an error determining the direction of a swipe.
                else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalY0) < Mathf.Abs(totalX0)) && (Mathf.Abs(totalY1) < Mathf.Abs(totalX1))) || ((Mathf.Abs(totalY0) < Mathf.Abs(totalX0)) && (Mathf.Abs(totalY2) < Mathf.Abs(totalX2))) || ((Mathf.Abs(totalY1) < Mathf.Abs(totalX1)) && (Mathf.Abs(totalY2) < Mathf.Abs(totalX2)))) && ((((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX2) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX2) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX2) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist))) || (((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist) && (Mathf.Abs(totalY2) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist) && (Mathf.Abs(totalY2) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY2) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist)))))
                {
                    ievent.isSwipeDirectionError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Cannot determine if the swipe was horizontal or vertical. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If there was an error determining the direction of a swipe.
                else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalX0) < Mathf.Abs(totalY0)) && (Mathf.Abs(totalX1) < Mathf.Abs(totalY1))) || ((Mathf.Abs(totalX0) < Mathf.Abs(totalY0)) && (Mathf.Abs(totalX2) < Mathf.Abs(totalY2))) || ((Mathf.Abs(totalX1) < Mathf.Abs(totalY1)) && (Mathf.Abs(totalX2) < Mathf.Abs(totalY2)))) && ((((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX2) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX2) >= minSwipeHorizontalDist)) || ((Mathf.Abs(totalX2) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) >= minSwipeHorizontalDist) && (Mathf.Abs(totalX1) >= minSwipeHorizontalDist))) || (((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist) && (Mathf.Abs(totalY2) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist) && (Mathf.Abs(totalY2) >= minSwipeVerticalDist)) || ((Mathf.Abs(totalY2) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) >= minSwipeVerticalDist) && (Mathf.Abs(totalY1) >= minSwipeVerticalDist)))))
                {
                    ievent.isSwipeDirectionError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Cannot determine if the swipe was horizontal or vertical. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.  
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If a swipe had not enough horizontal and vertical movement.
                else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalX0) > Mathf.Abs(totalY0)) && ((Mathf.Abs(totalX1) < Mathf.Abs(totalY1)) || (Mathf.Abs(totalX2) < Mathf.Abs(totalY2)))) || ((Mathf.Abs(totalX1) > Mathf.Abs(totalY1)) && ((Mathf.Abs(totalX0) < Mathf.Abs(totalY0)) || (Mathf.Abs(totalX2) < Mathf.Abs(totalY2)))) || ((Mathf.Abs(totalX2) > Mathf.Abs(totalY2)) && ((Mathf.Abs(totalX0) < Mathf.Abs(totalY0)) || (Mathf.Abs(totalX1) < Mathf.Abs(totalY1))))) && ((((Mathf.Abs(totalX1) >= minSwipeHorizontalDist) || (Mathf.Abs(totalX2) >= minSwipeHorizontalDist)) && ((Mathf.Abs(totalX0) < minSwipeHorizontalDist) && (Mathf.Abs(totalX0) > maxHoldHorizontalDist))) || (((Mathf.Abs(totalX0) >= minSwipeHorizontalDist) || (Mathf.Abs(totalX2) >= minSwipeHorizontalDist)) && ((Mathf.Abs(totalX1) < minSwipeHorizontalDist) && (Mathf.Abs(totalX1) > maxHoldHorizontalDist))) || (((Mathf.Abs(totalX0) >= minSwipeHorizontalDist) || (Mathf.Abs(totalX1) >= minSwipeHorizontalDist)) && ((Mathf.Abs(totalX2) < minSwipeHorizontalDist) && (Mathf.Abs(totalX2) > maxHoldHorizontalDist)))))
                {
                    ievent.isSwipeHorizontalVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Swipe horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If a swipe had not enough horizontal and vertical movement.
                else if ((touchDuration > 0.0f) && (((Mathf.Abs(totalY0) > Mathf.Abs(totalX0)) && ((Mathf.Abs(totalY1) < Mathf.Abs(totalX1)) || (Mathf.Abs(totalY2) < Mathf.Abs(totalX2)))) || ((Mathf.Abs(totalY1) > Mathf.Abs(totalX1)) && ((Mathf.Abs(totalY0) < Mathf.Abs(totalX0)) || (Mathf.Abs(totalY2) < Mathf.Abs(totalX2)))) || ((Mathf.Abs(totalY2) > Mathf.Abs(totalX2)) && ((Mathf.Abs(totalY0) < Mathf.Abs(totalX0)) || (Mathf.Abs(totalY1) < Mathf.Abs(totalX1))))) && ((((Mathf.Abs(totalY1) >= minSwipeVerticalDist) || (Mathf.Abs(totalY2) >= minSwipeVerticalDist)) && ((Mathf.Abs(totalY0) < minSwipeVerticalDist) && (Mathf.Abs(totalY0) > maxHoldVerticalDist))) || (((Mathf.Abs(totalY0) >= minSwipeVerticalDist) || (Mathf.Abs(totalY2) >= minSwipeVerticalDist)) && ((Mathf.Abs(totalY1) < minSwipeVerticalDist) && (Mathf.Abs(totalY1) > maxHoldVerticalDist))) || (((Mathf.Abs(totalY0) >= minSwipeVerticalDist) || (Mathf.Abs(totalY1) >= minSwipeVerticalDist)) && ((Mathf.Abs(totalY2) < minSwipeVerticalDist) && (Mathf.Abs(totalY2) > maxHoldVerticalDist)))))
                {
                    ievent.isSwipeHorizontalVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Swipe horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.   
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If both fingers moved too much for a tap and too little for a swipe.
                else if (((touchDuration < 1.0f) && (touchDuration > 0.0f)) && ((((Mathf.Abs(totalX0) > maxTapHorizontalDist) && (Mathf.Abs(totalX0) < minSwipeHorizontalDist)) && ((Mathf.Abs(totalX1) > maxTapHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist)) && ((Mathf.Abs(totalX2) > maxTapHorizontalDist) && (Mathf.Abs(totalX2) < minSwipeHorizontalDist))) || (((Mathf.Abs(totalY0) > maxTapVerticalDist) && (Mathf.Abs(totalY0) < minSwipeVerticalDist)) && ((Mathf.Abs(totalY1) > maxTapVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist)) && ((Mathf.Abs(totalY2) > maxTapVerticalDist) && (Mathf.Abs(totalY2) < minSwipeVerticalDist)))))
                {
                    ievent.isBetweenTapSwipeError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Gesture not recognized as either a tap or a swipe. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If both fingers moved too much for a hold and too little for a swipe.
                else if ((touchDuration >= 1.0f) && ((((Mathf.Abs(totalX0) > maxTapHorizontalDist) && (Mathf.Abs(totalX0) < minSwipeHorizontalDist)) && ((Mathf.Abs(totalX1) > maxTapHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist)) && ((Mathf.Abs(totalX2) > maxTapHorizontalDist) && (Mathf.Abs(totalX2) < minSwipeHorizontalDist))) || (((Mathf.Abs(totalY0) > maxTapVerticalDist) && (Mathf.Abs(totalY0) < minSwipeVerticalDist)) && ((Mathf.Abs(totalY1) > maxTapVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist)) && ((Mathf.Abs(totalY2) > maxTapVerticalDist) && (Mathf.Abs(totalY2) < minSwipeVerticalDist)))))
                {
                    ievent.isBetweenHoldSwipeError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Gesture not recognized as either a hold or a swipe. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a hold with too much rotation.
                else if ((touchDuration >= 1.0f) && (Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && (Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && (Mathf.Abs(totalX2) <= maxTapHorizontalDist) && (Mathf.Abs(totalY2) <= maxTapVerticalDist) && (Mathf.Abs(angle) > 40.0f))
                {
                    ievent.isHoldRotationError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Hold rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.         
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a hold with too much horizontal movement.
                else if ((touchDuration >= 1.0f) && (((Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && ((Mathf.Abs(totalX1) > maxTapHorizontalDist) || (Mathf.Abs(totalX2) > maxTapHorizontalDist))) || ((Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && ((Mathf.Abs(totalX0) > maxTapHorizontalDist) || (Mathf.Abs(totalX2) > maxTapHorizontalDist))) || ((Mathf.Abs(totalX2) <= maxTapHorizontalDist) && (Mathf.Abs(totalY2) <= maxTapVerticalDist) && ((Mathf.Abs(totalX0) > maxTapHorizontalDist) || (Mathf.Abs(totalX1) > maxTapHorizontalDist)))))
                {
                    ievent.isHoldHorizontalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Hold horizontal distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a hold with too much vertical movement.
                else if ((touchDuration >= 1.0f) && (((Mathf.Abs(totalX0) <= maxTapHorizontalDist) && (Mathf.Abs(totalY0) <= maxTapVerticalDist) && ((Mathf.Abs(totalY1) > maxTapVerticalDist) || (Mathf.Abs(totalY2) > maxTapVerticalDist))) || ((Mathf.Abs(totalX1) <= maxTapHorizontalDist) && (Mathf.Abs(totalY1) <= maxTapVerticalDist) && ((Mathf.Abs(totalY0) > maxTapVerticalDist) || (Mathf.Abs(totalY2) > maxTapVerticalDist))) || ((Mathf.Abs(totalX2) <= maxTapHorizontalDist) && (Mathf.Abs(totalY2) <= maxTapVerticalDist) && ((Mathf.Abs(totalY0) > maxTapVerticalDist) || (Mathf.Abs(totalY1) > maxTapVerticalDist)))))
                {
                    ievent.isHoldVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Hold vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a hold with too much horizontal and vertical movement.
                else if ((touchDuration >= 1.0f) && (((Mathf.Abs(totalX0) > maxTapHorizontalDist) && (Mathf.Abs(totalX0) < minSwipeHorizontalDist) && ((Mathf.Abs(totalX1) <= maxTapHorizontalDist) || (Mathf.Abs(totalX2) <= maxTapHorizontalDist))) || ((Mathf.Abs(totalX1) > maxTapHorizontalDist) && (Mathf.Abs(totalX1) < minSwipeHorizontalDist) && ((Mathf.Abs(totalX0) <= maxTapHorizontalDist) || (Mathf.Abs(totalX2) <= maxTapHorizontalDist))) || ((Mathf.Abs(totalX2) > maxTapHorizontalDist) && (Mathf.Abs(totalX2) < minSwipeHorizontalDist) && ((Mathf.Abs(totalX0) <= maxTapHorizontalDist) || (Mathf.Abs(totalX1) <= maxTapHorizontalDist)))) && (((Mathf.Abs(totalY0) > maxTapVerticalDist) && (Mathf.Abs(totalY0) < minSwipeVerticalDist) && ((Mathf.Abs(totalY1) <= maxTapVerticalDist) || (Mathf.Abs(totalY2) <= maxTapVerticalDist))) || ((Mathf.Abs(totalY1) > maxTapVerticalDist) && (Mathf.Abs(totalY1) < minSwipeVerticalDist) && ((Mathf.Abs(totalY0) <= maxTapVerticalDist) || (Mathf.Abs(totalY2) <= maxTapVerticalDist))) || ((Mathf.Abs(totalY2) > maxTapVerticalDist) && (Mathf.Abs(totalY2) < minSwipeVerticalDist) && ((Mathf.Abs(totalY0) <= maxTapVerticalDist) || (Mathf.Abs(totalY1) <= maxTapVerticalDist)))))
                {
                    ievent.isHoldHorizontalVerticalError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.                      
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Hold horizontal and vertical distance error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.        
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                // If the gesture was a turn with not enough rotation.
                else if ((touchDuration > 0.0f) && (Mathf.Abs(angle) < 45.0f))
                {
                    ievent.isRotationAngleError = true; // This error was registered.
                    ievent.isUnrecognized = true; // An unrecognized gesture was registered.
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Turn rotation error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.     
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }

                else
                {
                    unrecognizedTimes += 1; // Update the number of times an unrecognized gesture was made.
                    debugInputInfo = "Other gesture error. An unrecognized gesture has been made " + unrecognizedTimes + " times";
                    DebugInput.instance.ChangeDebugInputText(debugInputInfo); // Update the debug textbox.                            
                    print("TotalX0: " + totalX0.ToString() + ", TotalX1: " + totalX1.ToString() + ", TotalX2: " + totalX2.ToString());
                    print("TotalY0: " + totalY0.ToString() + ", TotalY1: " + totalY1.ToString() + ", TotalY2: " + totalY2.ToString());
                    print("Hold: " + touchDuration.ToString() + ", Angle: " + angle.ToString());
                    print("MaxTapHorizontal: " + maxTapHorizontalDist.ToString() + ", MaxTapVertical: " + maxTapVerticalDist.ToString());
                    print("MinSwipeHorizontal: " + minSwipeHorizontalDist.ToString() + ", MinSwipeVertical: " + minSwipeVerticalDist.ToString());
                    print("MaxHoldHorizontal: " + maxHoldHorizontalDist.ToString() + ", MaxHoldVertical: " + maxHoldVerticalDist.ToString());
                    touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
                    touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
                }
            }

            touchDuration = 0.0f; // Reset touchDuration to 0, as nothing is touching the screen.
            touchRegister = 0; // Reset the touchRegister just to make sure no inputs are recognized when there are no fingers touching the screen.
            angle = 0.0f; // Reset the angle to 0.

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
