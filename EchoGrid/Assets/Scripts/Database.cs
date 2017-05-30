using UnityEngine;
using System.Collections;

/// <summary>
/// A class containing a variety of resources loaded in the game to use.
///  It mostly contains audio clips of various sounds.
/// </summary>
public class Database : MonoBehaviour
{

    public static Database instance;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    //audios
    //General
    [HideInInspector]
    public AudioClip[] oritClip = new AudioClip[2];
    //main
    [HideInInspector]
    public AudioClip[] menuClips = new AudioClip[5];
    //MainPre
    [HideInInspector]
    public AudioClip[] MainPreGameClips = new AudioClip[4];
    [HideInInspector]
    public AudioClip[] MainPreConfirmClips = new AudioClip[3];
    [HideInInspector]
    public AudioClip MainPreContinueGame = new AudioClip();
    [HideInInspector]
    public AudioClip MainPreNewGame = new AudioClip();
    //Title
    [HideInInspector]
    public AudioClip[] TitletoMainClips = new AudioClip[2];
    [HideInInspector]
    public AudioClip[] TitleClips = new AudioClip[7];
    [HideInInspector]
    public AudioClip[] TitleCmdlistClips = new AudioClip[9];
    //fx
    [HideInInspector]
    public AudioClip swipeAhead = new AudioClip();
    [HideInInspector]
    public AudioClip swipeRight = new AudioClip();
    [HideInInspector]
    public AudioClip swipeLeft = new AudioClip();
    [HideInInspector]
    public AudioClip inputSFX = new AudioClip();
    [HideInInspector]
    public AudioClip menuOn = new AudioClip();
    [HideInInspector]
    public AudioClip menuOff = new AudioClip();
    [HideInInspector]
    public AudioClip wallHit = new AudioClip();
    [HideInInspector]
    public AudioClip winSound = new AudioClip();
    [HideInInspector]
    public AudioClip walking = new AudioClip();

    //strings
    //Title Screen
    public static string titleText_main = "\n\nto contact the app developers \nor the research lab, \nemail auditory@andrew.cmu.edu";

    //T&C screen
    public static string tcText_main = "T & C\nSwipe Left to Continue";
    public static string tcmsg = "Please hold your phone horizontally for this game, \n " +
                                 "and please read the online consent form; \n " +
                                 "after finish, you can click back button to " +
                                 "return to the game";

    //Main


    void LoadData()
    {
        //general
        oritClip[0] = Resources.Load("instructions/Please hold your phone horizontally for this game") as AudioClip;
        oritClip[1] = Resources.Load("instructions/2sec_silence") as AudioClip;
        //Main
        menuOn = Resources.Load("instructions/Menu opened") as AudioClip;
        menuOff = Resources.Load("instructions/Menu closed") as AudioClip;
        menuClips[0] = Resources.Load("instructions/To close the menu, press and hold with two fingers") as AudioClip;
        menuClips[1] = Resources.Load("instructions/Swipe left to restart the current level") as AudioClip;
        menuClips[2] = Resources.Load("instructions/Swipe left to return to the tutorial, swipe right to return to the main menu, and swipe down to toggle the screen on and off") as AudioClip;
        menuClips[3] = Resources.Load("instructions/Swipe up to hear a hint") as AudioClip;
        menuClips[4] = Resources.Load("instructions/2sec_silence") as AudioClip;
        //Title
        TitleClips[0] = Resources.Load("instructions/Welcome to Echo Adventure") as AudioClip;
        TitleClips[1] = Resources.Load("instructions/To skip the tutorial, swipe right") as AudioClip;
        TitleClips[2] = Resources.Load("instructions/Swipe left to go to the tutorial") as AudioClip;
        TitleClips[3] = Resources.Load("instructions/Swipe up to hear a list of commands") as AudioClip;
        TitleClips[4] = Resources.Load("instructions/2sec_silence") as AudioClip;
        TitleClips[5] = Resources.Load("instructions/2sec_silence") as AudioClip;
        TitleClips[6] = Resources.Load("instructions/1sec_silence") as AudioClip;
        TitletoMainClips[0] = Resources.Load("instructions/0_5sec_silence") as AudioClip;
        TitletoMainClips[1] = Resources.Load("instructions/0_5sec_silence") as AudioClip;
        TitleCmdlistClips[0] = Resources.Load("instructions/Tap and hold to hear an echo") as AudioClip;
        TitleCmdlistClips[1] = Resources.Load("instructions/To open the pause menu, press two fingers on the screen and hold") as AudioClip;
        TitleCmdlistClips[2] = Resources.Load("instructions/Swipe up to move forward") as AudioClip;
        TitleCmdlistClips[3] = Resources.Load("instructions/Rotate two fingers counterclockwise to turn left") as AudioClip;
        TitleCmdlistClips[4] = Resources.Load("instructions/Rotate two fingers clockwise to turn right") as AudioClip;
        TitleCmdlistClips[5] = Resources.Load("instructions/Double tap to attempt to exit") as AudioClip;
        TitleCmdlistClips[6] = Resources.Load("instructions/0_5sec_silence") as AudioClip;
        TitleCmdlistClips[7] = Resources.Load("instructions/turn around by turning in the same direction twice") as AudioClip;
        TitleCmdlistClips[8] = Resources.Load("instructions/2sec_silence") as AudioClip;
        //Main_pre
        MainPreGameClips[0] = Resources.Load("instructions/To continue from where you left off, swipe right ") as AudioClip;
        MainPreGameClips[1] = Resources.Load("instructions/0_5sec_silence") as AudioClip;
        MainPreGameClips[2] = Resources.Load("instructions/Double tap to start a new game, then, swipe left to confirm, or double tap to cancel") as AudioClip;
        MainPreGameClips[3] = Resources.Load("instructions/2sec_silence") as AudioClip;
        MainPreConfirmClips[0] = Resources.Load("instructions/Are you sure you want to start a new game, this will overwrite existing saves") as AudioClip;
        MainPreConfirmClips[1] = Resources.Load("instructions/Swipe left to confirm or double tap to cancel") as AudioClip;
        MainPreConfirmClips[2] = Resources.Load("instructions/2sec_silence") as AudioClip;
        MainPreContinueGame = Resources.Load("instructions/Loaded saved game") as AudioClip;
        MainPreNewGame = Resources.Load("instructions/New game started") as AudioClip;

        //fx
        swipeAhead = Resources.Load("fx/swipe-ahead") as AudioClip;
        swipeRight = Resources.Load("fx/swipe-right") as AudioClip;
        swipeLeft = Resources.Load("fx/swipe-left") as AudioClip;
        inputSFX = Resources.Load("fx/inputSFX") as AudioClip;
        wallHit = Resources.Load("fx/wallHitting") as AudioClip;
        winSound = Resources.Load("fx/winSound") as AudioClip;
        walking = Resources.Load("fx/walking") as AudioClip;
    }
}
