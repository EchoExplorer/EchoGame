using UnityEngine;
using System.Collections;
using System;

using System.Collections.Generic;
using SimpleJSON;
using System.Security.Cryptography;
using System;
using System.Text;
using System.Diagnostics;

public class Utilities : MonoBehaviour {
	public static int MAZE_SIZE = 9;
	public static int SCALE_REF = 8;
	public static float exit_touch_time = 1.5f;

	public static bool OLD_ANDROID_SUPPORT = true;

	//encrypt related
	public static RSACryptoServiceProvider encrypter = new RSACryptoServiceProvider ();

	public static void initEncrypt ()
	{
		string publicKeyString = "MIGdMA0GCSqGSIb3DQEBAQUAA4GLADCBhwKBgQC1hBlMytDpiLGqCNGfx+IvbRH9edqFcxJoL5CuEPOjr31u9PXTgtSuZhldKc9KpPR4j62M6+UxSs9abDd1/C0txQEB4Jxe/FPMOBmlvNHNHLw6htPx5JRHzN1cegi3W6Qd8YRMi3XfSx5tGx0NNLxuf+EDrE5NIVUdp0hpQ7yMFQIBAw==";
		byte[] publicKeyBytes = Convert.FromBase64String (publicKeyString);

		byte[] Exponent = {3};


		//Create a new instance of RSAParameters.
		RSAParameters RSAKeyInfo = new RSAParameters ();

		//Set RSAKeyInfo to the public key values.
		RSAKeyInfo.Modulus = publicKeyBytes;
		RSAKeyInfo.Exponent = Exponent;

		//Import key parameters into RSA.
		Utilities.encrypter.ImportParameters (RSAKeyInfo);

		//UnityEngine.Debug.Log (encrypt ("This is a test String"));
	}

	public static String encrypt (String encryptThis)
	{
		string b64EncryptThis = Utilities.Base64Encode (encryptThis);

		//Encrypt the symmetric key and IV.
		byte[] encryptedString = encrypter.EncryptValue (Convert.FromBase64String (b64EncryptThis));

		//Add the encrypted test string to the form
		return Convert.ToBase64String (encryptedString);
	}

	//rw file
	public static string[] Loadfile(string fname){
		string filename = Application.persistentDataPath + fname;
		string[] svdata_split = new string[1];
		if (System.IO.File.Exists (filename)) {
			svdata_split = System.IO.File.ReadAllLines (filename);
			//local_stats = Array.ConvertAll<string, int>(svdata_split, int.Parse);
		}

		return svdata_split;
	}

	public static bool writefile(string fname, string toWrite){
		string filename = Application.persistentDataPath + fname;
		if (System.IO.File.Exists (filename)) {
			//TODO: this is a temp solution, we should take down the content of consent
			System.IO.File.WriteAllText (filename, toWrite);
		}
		return true;
	}

	//Makes HTTP requests and waits for response and checks for errors
	public static IEnumerator WaitForRequest (WWW www)
	{
		yield return www;

		//Check for errors
		if (www.error == null) {
			JSONNode data = JSON.Parse (www.data);
			//Debug.Log("this is the parsed json data: " + data["testData"]);
			//Debug.Log(data["testData"]);
			UnityEngine.Debug.Log ("WWW.Ok! " + www.data);
		} else {
			UnityEngine.Debug.Log ("WWWError: " + www.error);
		}
	}

	//helpers
	public static string Base64Encode (string plainText)
	{
		var plainTextBytes = System.Text.Encoding.UTF8.GetBytes (plainText);
		return System.Convert.ToBase64String (plainTextBytes);
	}
}
