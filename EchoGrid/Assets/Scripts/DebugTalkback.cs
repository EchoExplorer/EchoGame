using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTalkback : MonoBehaviour
{
    Text debugTalkbackText;
    string talkbackText;

    AndroidJavaClass unity;
    AndroidJavaObject currentActivity;
    AndroidJavaObject contentResolver;
    AndroidJavaClass secureSettings;
    AndroidJavaObject permission;
    AndroidJavaObject packageManager;
    int granted;
    int checkPermission;
    string isUsingTalkback = "No"; // If the player is using Google Talkback/Touch to Explore.

    public static DebugTalkback instance;
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

    // Use this for initialization
    void Start()
    {
        debugTalkbackText = GetComponent<Text>();
        talkbackText = "Talkback disabled.";
        debugTalkbackText.text = talkbackText; // Update the debug textbox.
    }
	
	// Update is called once per frame
	void Update()
    {
#if UNITY_ANDROID
        unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
        permission = new AndroidJavaObject("android.Manifest.permission.READ_SECURE_SETTINGS");
        checkPermission = currentActivity.Call<int>("checkSelfPermission", currentActivity, permission);
        packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        granted = packageManager.Get<int>("PERMISSION_GRANTED");

        talkbackText = "CP: " + checkPermission + " , G: " + granted;


        if (checkPermission == granted)
        {
            contentResolver = currentActivity.Call<AndroidJavaObject>("contentResolver");
            secureSettings = new AndroidJavaClass("android.provider.Settings$Secure");
            isUsingTalkback = secureSettings.CallStatic<string>("getString", contentResolver, "TOUCH_EXPLORATION_ENABLED"); // See if Talkback is enabled.

            // If Talkback is disabled.
            if (isUsingTalkback.Contains("touch_exploration_disabled") == true)
            {
                //talkbackText = "Not using Talkback.";
            }
            // If Talkback is enabled.
            else if (isUsingTalkback.Contains("touch_exploration_enabled") == true)
            {
                //talkbackText = "Using Talkback.";
            }
            // If it is not defined.
            else
            {
                //talkbackText = "Caanot be determined.";
            }
        }
        debugTalkbackText.text = talkbackText; // Update the debug textbox.
#endif
    }
}
