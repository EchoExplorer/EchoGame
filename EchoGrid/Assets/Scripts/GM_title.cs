using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// A script to display the user agreement dialogues.
/// This is attached to the ``GameManager`` GameObject in the Title scene.
/// </summary>
public class GM_title : MonoBehaviour
{
    int cur_clip = 0;
    int orti_clip = 0;
    int cmd_cur_clip = 0;

    float time_interval = 2.0f;
    bool reset_audio = false;
    bool listenToCmd = false;
    public bool toMainflag = false;
    public Text titleText;
    bool doneTesting = false;
    eventHandler eh;

    enum Direction { NONE, UP, DOWN, LEFT, RIGHT }

    /// <summary>
    /// Sets up a reference to the GameMode module so it can set up its singleton.
    /// </summary>
    void Start()
    {
        eh = new eventHandler(InputModule.instance);
    }

    bool plugin_earphone = false;
    bool second_tap = false;
    bool orient_correction = false;
    bool clip0_reset = true;
    bool clip1_reset = true;
    bool clip2_reset = true;
    void play_audio()
    {
        if (!plugin_earphone)
        {
            if (SoundManager.instance.PlayVoice(Database.instance.settingClips[0], clip0_reset, 1))
            {
                clip0_reset = false;
            }
            return;
        }
        if (!orient_correction)
        {
            if (!Utilities.isDeviceLandscape())
            {//not landscape!
                if (SoundManager.instance.PlayVoice(Database.instance.settingClips[1], clip1_reset))
                {
                    clip1_reset = false;
                }
                return;

            }
            else
            {
                orient_correction = true;
                if (SoundManager.instance.PlayVoice(Database.instance.settingClips[2], clip2_reset))
                {
                    clip2_reset = false;
                }
            }
        }
        if (!listenToCmd)
        {
            if (SoundManager.instance.PlayVoice(Database.instance.TitleClips[cur_clip], reset_audio))
            {
                reset_audio = false;
                cur_clip += 1;
                if (cur_clip >= Database.instance.TitleClips.Length)
                    cur_clip = 0;
            }
        }
        else
        {
            if (SoundManager.instance.PlayVoice(Database.instance.TitleCmdlistClips[cmd_cur_clip], reset_audio))
            {
                reset_audio = false;
                cmd_cur_clip += 1;
                if (cmd_cur_clip >= Database.instance.TitleCmdlistClips.Length)
                    cmd_cur_clip = 0;
            }
        }
    }

    /// <summary>
    /// Checks for an internet connection, and plays instructions.
    ///  Progresses to the main_pre scene for regular gameplay, or the main scene
    ///  for the tutorial by analyzing the touch data.
    /// </summary>
    void Update()
    {
        if (Const.TEST_CONNECTION)
        {
            if (!doneTesting)
            {
                string str = Utilities.check_InternetConnection();
                if (str.Length == 0)
                {//we're good to go
                    doneTesting = true;
                    titleText.text = Database.titleText_main;
                }
                else
                    titleText.text = str;
            }
        }

        play_audio();

        Direction inputDirection = Direction.NONE;

        //Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR

        if (eh.isActivate() && doneTesting) // isActivate() has side effects so this order is required...
        {
            InputEvent ie = eh.getEventData();
            switch (ie.keycode)
            {
                case KeyCode.F:
                    if (!plugin_earphone) plugin_earphone = true;
                    else if (!second_tap)
                    {
                        second_tap = true;
                        reset_audio = true;
                    }
                    break;
                case KeyCode.RightArrow:
                    inputDirection = Direction.RIGHT;
                    break;
                case KeyCode.LeftArrow:
                    inputDirection = Direction.LEFT;
                    break;
                case KeyCode.UpArrow://Up
                    inputDirection = Direction.UP;
                    break;
                case KeyCode.DownArrow://BACK
                    inputDirection = Direction.DOWN;
                    break;
                default:
                    break;
            }
        }

        //Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Screen.orientation = ScreenOrientation.Landscape;

		if(eh.isActivate() && doneTesting) {  // isActivate() has side effects so this order is required...
			InputEvent ie = eh.getEventData();
            if (!plugin_earphone) {
                if (ie.touchNum>=1)
                {
                    plugin_earphone = true;
                    reset_audio = true;
                }
            }
            else if (!second_tap){
                if (ie.touchNum>=1)
                {
                    second_tap = true;
                }
            }
            else
            {
                if( (ie.touchNum == 1)&&(!ie.isRotate) ){
				    if (ie.isRight){
					    inputDirection = Direction.RIGHT;
				    } else if (ie.isLeft){
					    inputDirection = Direction.LEFT;
				    } else if (ie.isUp){
					    inputDirection = Direction.UP;
				    } else if (ie.isDown){
					    inputDirection = Direction.DOWN;
				    }
			    }
            }
		}
#endif //End of mobile platform dependendent compilation section started above with #elif
        if (!plugin_earphone || !second_tap) return;
        switch (inputDirection)
        {
            case Direction.RIGHT:
                GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
				SceneManager.LoadScene("Main_pre");
				SoundManager.instance.PlayVoice(Database.instance.TitletoMainClips[0], true);
                break;
            case Direction.LEFT:
                GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                SceneManager.LoadScene("Main_pre");
				SoundManager.instance.PlayVoice(Database.instance.TitletoMainClips[0], true);
				//SceneManager.LoadScene("Main");
                break;
            case Direction.UP:
                SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
				if(!listenToCmd){
					listenToCmd = true;
					reset_audio = true;
					cur_clip = 0;
					cmd_cur_clip = 0;
				}else{
					listenToCmd = false;
					reset_audio = true;
					cur_clip = 0;
					cmd_cur_clip = 0;
				}
                break;
            case Direction.DOWN:
                SoundManager.instance.PlaySingle(Database.instance.swipeAhead);
				//credit
                break;
            default:
                break;
        }
    }
}
