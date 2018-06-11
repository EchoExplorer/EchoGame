using UnityEngine;
using System.Collections;

/// <summary>
/// A class containing a variety of constants that the game accesses at runtime.
///  It is primarily used for tweaking.
/// </summary>
public class Const : MonoBehaviour
{
    public static Const instance;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    //Global flag
    public const bool TEST_CONNECTION = true;

    //Game input
    public const float multiTapCD = 0.45f;//make multitap easier
    public const float echoCD = 0.08f;//shortest time between two PlayEcho() calls. currently not used, using opsToEchoCDinstead
    public const float menuUpdateCD = 0.5f;//shortest time between turn on/off pause menu
    public const float rotateGestCD = 0.3f;

    public const float MENU_TOUCH_TIME = 1.0f;
    public const float echoTapTime = 0.16f;//tap for how long to get an echo
    public const float opsToEchoCD = 0.14f;//CD between any operation and an echo, this is used to prevent accidential echo   
}
