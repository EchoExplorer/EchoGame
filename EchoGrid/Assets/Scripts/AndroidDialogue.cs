using UnityEngine;
using System.Collections;

//Display android system popup window
//"The MergeBlock"
public class AndroidDialogue : MonoBehaviour {

	public enum DialogueType
	{
		NORMAL = 0,
		YESONLY = 1,
		INPUT = 2,
	}

	const int ButtonWidth = 256;
	const int ButtonHeight = 64;

	private bool mYesPressed = false;
	private bool mNoPressed = false;

	string inputStr = "";

	void Awake(){
		mYesPressed = false;
		mNoPressed = false;
	}

	//the connector from outside
	public void DisplayAndroidWindow(string msg, DialogueType type = DialogueType.NORMAL){
		showDialog(msg, type);
	}

	public bool yesclicked(){
		return mYesPressed;
	}

	public bool noclicked(){
		return mNoPressed;
	}

	public void clearflag (){
		mYesPressed = false;
		mNoPressed = false;
	}

	public string getInputStr(){
		return inputStr;
	}

	// Lets put our android specific code under the macro UNITY_ANDROID
	#if UNITY_ANDROID
	// Lets create some listners.
	// These listners will be passed to android 

	// Create the postive action listner class
	// It has to be derived from the AndroidJavaProxy class
	// Make the methods as same as that of DialogInterface.OnClickListener
	private class PositiveButtonListner : AndroidJavaProxy {
		private AndroidDialogue mDialog;

		public PositiveButtonListner(AndroidDialogue d)
			: base("android.content.DialogInterface$OnClickListener") {
			mDialog = d;
		}

		public void onClick(AndroidJavaObject obj, int value ) {
			mDialog.mYesPressed = true;
			mDialog.mNoPressed = false;
		}
	}

	private class InputTextFieldListner : AndroidJavaProxy {
		private AndroidDialogue mDialog;
		private AndroidJavaObject InputText;

		public InputTextFieldListner(AndroidDialogue d, AndroidJavaObject text)
			: base("android.content.DialogInterface$OnClickListener") {
			mDialog = d;
			InputText = text;
		}

		public void onClick(AndroidJavaObject obj, int value ) {
			mDialog.mYesPressed = true;
			mDialog.mNoPressed = false;
			AndroidJavaObject editable = new AndroidJavaClass ("android.text.Editable");
			editable = InputText.Call< AndroidJavaObject> ("getText");
			mDialog.inputStr = editable.Call< string > ("toString");
		}
	}

	// Create the postive action listner class
	// It has to be derived from the AndroidJavaProxy class
	// Make the methods as same as that of DialogInterface.OnClickListener
	private class NegativeButtonListner : AndroidJavaProxy {
		private AndroidDialogue mDialog;

		public NegativeButtonListner(AndroidDialogue d)
			: base("android.content.DialogInterface$OnClickListener") {
			mDialog = d;
		}

		public void onClick(AndroidJavaObject obj, int value ) {
			mDialog.mYesPressed = false;
			mDialog.mNoPressed = true;
		}
	}


	#endif
	private void showDialog(string msg, DialogueType type) {

		#if UNITY_ANDROID
		// Obtain activity
		AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject activity = unityPlayer.GetStatic< AndroidJavaObject>  ("currentActivity");
		AndroidJavaObject InputTextField = new AndroidJavaObject("android.widget.EditText", activity);

		// Lets execute the code in the UI thread
		activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>  {
			//clear flag
			clearflag ();

			// Create an AlertDialog.Builder object
			AndroidJavaObject alertDialogBuilder = new AndroidJavaObject("android/app/AlertDialog$Builder", activity);

			// Call setTitle on the builder
			alertDialogBuilder.Call< AndroidJavaObject> ("setTitle", "Info:");

			// Call setMessage on the builder
			alertDialogBuilder.Call< AndroidJavaObject> ("setMessage", msg);

			//You must answer it before proceed
			alertDialogBuilder.Call< AndroidJavaObject> ("setCancelable", false);

			// Call setPositiveButton and set the message along with the listner
			// Listner is a proxy class
			alertDialogBuilder.Call< AndroidJavaObject> ("setPositiveButton", "Yes", new PositiveButtonListner(this));

			// Call setPositiveButton and set the message along with the listner
			// Listner is a proxy class
			switch(type){
			case DialogueType.INPUT:
				alertDialogBuilder.Call< AndroidJavaObject> ("setTitle", "Enter Code:");
				alertDialogBuilder.Call< AndroidJavaObject> ("setView", InputTextField);
				alertDialogBuilder.Call< AndroidJavaObject> ("setPositiveButton", "Yes", new InputTextFieldListner(this, InputTextField));
				break;
			case DialogueType.YESONLY:
				break;
			case DialogueType.NORMAL:
				alertDialogBuilder.Call< AndroidJavaObject> ("setNegativeButton", "No", new NegativeButtonListner(this));
				break;
			default://same as normal
				alertDialogBuilder.Call< AndroidJavaObject> ("setNegativeButton", "No", new NegativeButtonListner(this));
				break;
			}
			// Finally get the dialog instance and show it
			AndroidJavaObject dialog = alertDialogBuilder.Call< AndroidJavaObject> ("create");
			dialog.Call("show");
		}));
		#endif

	}
}
