using UnityEngine;
using System.Collections;

/// <summary>
/// This is a class designed to display a dialog window on Android.
/// </summary>
/// <remarks>
/// It supports the following kinds of messages:
/// * A yes or no prompt
/// * A prompt purely for displaying a message
/// * A prompt acccepting user input
/// </remarks>
public class AndroidDialogue : MonoBehaviour
{
    /// <summary>
    /// The type of the dialogue, mostly determining what buttons and input fields exist.
    /// Normal refers to a yes or no prompt. Yes-only refers to a single button prompt.
    /// Input refers to a prompt with an embedded text field.
    /// </summary>
    public enum DialogueType
    {
        NORMAL = 0,
        YESONLY = 1,
        INPUT = 2
    }

    const int ButtonWidth = 256;
    const int ButtonHeight = 64;

    private bool mYesPressed = false;
    private bool mNoPressed = false;

    string inputStr = "";

    void Awake()
    {
        mYesPressed = false;
        mNoPressed = false;
    }

    /// <summary>
    /// The function used to display the message box.
    /// If the game is not running on Android, this function will do nothing.
    /// </summary>
    /// <param name="msg">The message to be displayed within the box.</param>
    /// <param name="type">The dialogue type to use.</param>
    public void DisplayAndroidWindow(string title, string message, DialogueType type = DialogueType.NORMAL, string yes = "Yes", string no = "No")
    {
        showDialog(type, title, message, yes, no);
    }

    /// <summary>
    /// Determines whether the user clicked 'yes' on the most recent dialogue.
    /// If the dialogue has only one button, it too represents 'yes'.
    /// </summary>
    /// <returns>Boolean, true if the user clicked on 'yes' or the lone button.</returns>
    public bool yesclicked()
    {
        return mYesPressed;
    }

    /// <summary>
    /// Determines whether the user clicked 'no' on the most recent dialogue.
    /// This only applies to the yes or no prompts.
    /// </summary>
    /// <returns>Boolean, true if the user clicked on 'no'</returns>
    public bool noclicked()
    {
        return mNoPressed;
    }

    /// <summary>
    /// Resets the flags for a future message prompt.
    /// </summary>
    public void clearflag()
    {
        //FIXME: This is bad design. Don't force the client do what the library can do.
        //TODO: set inputStr to the empty string too.
        mYesPressed = false;
        mNoPressed = false;
    }

    /// <summary>
    /// Retrieves the string typed into the input field.
    /// </summary>
    /// <returns>The string from the input field.</returns>
    public string getInputStr()
    {
        if (inputStr == null)
        {
            return " ";
        }
        else if (inputStr.Length == 0)
        {
            return " ";
        }

        return inputStr;
    }

    // Lets put our android specific code under the macro UNITY_ANDROID
#if UNITY_ANDROID
    // Lets create some listners.
    // These listners will be passed to android 

    // Create the postive action listner class
    // It has to be derived from the AndroidJavaProxy class
    // Make the methods as same as that of DialogInterface.OnClickListener
    private class PositiveButtonListner : AndroidJavaProxy
    {
        private AndroidDialogue mDialog;

        public PositiveButtonListner(AndroidDialogue d) : base("android.content.DialogInterface$OnClickListener")
        {
            mDialog = d;
        }

        public void onClick(AndroidJavaObject obj, int value)
        {
            mDialog.mYesPressed = true;
            mDialog.mNoPressed = false;
        }
    }

    private class InputTextFieldListner : AndroidJavaProxy
    {
        private AndroidDialogue mDialog;
        private AndroidJavaObject InputText;

        public InputTextFieldListner(AndroidDialogue d, AndroidJavaObject text) : base("android.content.DialogInterface$OnClickListener")
        {
            mDialog = d;
            InputText = text;
        }

        public void onClick(AndroidJavaObject obj, int value)
        {
            mDialog.mYesPressed = true;
            mDialog.mNoPressed = false;
            AndroidJavaObject editable = new AndroidJavaClass("android.text.Editable");
            editable = InputText.Call<AndroidJavaObject>("getText");
            mDialog.inputStr = editable.Call<string>("toString");
        }
    }

    // Create the postive action listner class
    // It has to be derived from the AndroidJavaProxy class
    // Make the methods as same as that of DialogInterface.OnClickListener
    private class NegativeButtonListner : AndroidJavaProxy
    {
        private AndroidDialogue mDialog;

        public NegativeButtonListner(AndroidDialogue d) : base("android.content.DialogInterface$OnClickListener")
        {
            mDialog = d;
        }

        public void onClick(AndroidJavaObject obj, int value)
        {
            mDialog.mYesPressed = false;
            mDialog.mNoPressed = true;
        }
    }


#endif
    private void showDialog(DialogueType type, string title, string message, string yesText, string noText)
    {

#if UNITY_ANDROID
        // Obtain activity
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject InputTextField = new AndroidJavaObject("android.widget.EditText", activity);

        // Lets execute the code in the UI thread
        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            //clear flag
            clearflag();

            // Create an AlertDialog.Builder object
            AndroidJavaObject alertDialogBuilder = new AndroidJavaObject("android/app/AlertDialog$Builder", activity);

            // Call setTitle on the builder
            alertDialogBuilder.Call<AndroidJavaObject>("setTitle", title);

            // Call setMessage on the builder
            alertDialogBuilder.Call<AndroidJavaObject>("setMessage", message);

            //You must answer it before proceed
            alertDialogBuilder.Call<AndroidJavaObject>("setCancelable", false);

            // Call setPositiveButton and set the message along with the listner
            // Listner is a proxy class
            switch (type)
            {
                case DialogueType.NORMAL:
                    alertDialogBuilder.Call<AndroidJavaObject>("setPositiveButton", yesText, new PositiveButtonListner(this));
                    alertDialogBuilder.Call<AndroidJavaObject>("setNegativeButton", noText, new NegativeButtonListner(this));
                    break;
                case DialogueType.YESONLY:
                    alertDialogBuilder.Call<AndroidJavaObject>("setPositiveButton", yesText, new PositiveButtonListner(this));
                    break;
                case DialogueType.INPUT:
                    alertDialogBuilder.Call<AndroidJavaObject>("setView", InputTextField);
                    alertDialogBuilder.Call<AndroidJavaObject>("setPositiveButton", yesText, new InputTextFieldListner(this, InputTextField));
                    break;
                default: // same as normal
                    alertDialogBuilder.Call<AndroidJavaObject>("setPositiveButton", yesText, new PositiveButtonListner(this));
                    alertDialogBuilder.Call<AndroidJavaObject>("setNegativeButton", noText, new NegativeButtonListner(this));
                    break;
            }
            // Finally get the dialog instance and show it
            AndroidJavaObject dialog = alertDialogBuilder.Call<AndroidJavaObject>("create");
            dialog.Call("show");
        }));
#endif
    }
}
// This was copied from http://blog.trsquarelab.com/2015_02_01_archive.html, it seems.
// To future maintainers: if comments like these are still here, then I haven't finished handling these
// copying cases (since in general they have poor design and aren't documented well). Please help me.
