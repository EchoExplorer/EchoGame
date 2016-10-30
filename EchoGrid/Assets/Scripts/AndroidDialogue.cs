using UnityEngine;
using System.Collections;

//Display android system popup window
//"The MergeBlock"
public class AndroidDialogue : MonoBehaviour {

	const int ButtonWidth = 256;
	const int ButtonHeight = 64;

	private bool mYesPressed = false;
	private bool mNoPressed = false;

	void Awake(){
		mYesPressed = false;
		mNoPressed = false;
	}

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {

	}

	//the connector from outside
	public void DisplayAndroidWindow(string msg){
		showDialog(msg);
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

	private void showDialog(string msg) {

		#if UNITY_ANDROID
		// Obtain activity
		AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject activity = unityPlayer.GetStatic< AndroidJavaObject>  ("currentActivity");

		// Lets execute the code in the UI thread
		activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>  {
			//clear flag
			clearflag ();

			// Create an AlertDialog.Builder object
			AndroidJavaObject alertDialogBuilder = new AndroidJavaObject("android/app/AlertDialog$Builder", activity);

			// Call setMessage on the builder
			alertDialogBuilder.Call< AndroidJavaObject> ("setMessage", msg);

			// Call setCancelable on the builder
			alertDialogBuilder.Call< AndroidJavaObject> ("setCancelable", true);

			// Call setPositiveButton and set the message along with the listner
			// Listner is a proxy class
			alertDialogBuilder.Call< AndroidJavaObject> ("setPositiveButton", "Yes", new PositiveButtonListner(this));

			// Call setPositiveButton and set the message along with the listner
			// Listner is a proxy class
			alertDialogBuilder.Call< AndroidJavaObject> ("setNegativeButton", "No", new NegativeButtonListner(this));

			// Finally get the dialog instance and show it
			AndroidJavaObject dialog = alertDialogBuilder.Call< AndroidJavaObject> ("create");
			dialog.Call("show");
		}));
		#endif

	}
}
