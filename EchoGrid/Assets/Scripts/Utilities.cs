using UnityEngine;
using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using SimpleJSON;

/// <summary>
/// This class handles various utility functions.
/// </summary>
/// <remarks>
/// Supported operations include:
/// * RSA encryption of strings
/// * Load files
/// * Save persistent data
/// Check for an internet connection
/// </remarks>
public class Utilities : MonoBehaviour
{
    public static int MAZE_SIZE = 9;
    public static int SCALE_REF = 9;
    public static float exit_touch_time = 1.5f;

    public static bool OLD_ANDROID_SUPPORT = true;

    public static string debugPlayerInfo;

    //encrypt related
    public static RSACryptoServiceProvider encrypter = new RSACryptoServiceProvider();

    public static void initEncrypt()
    {
        string publicKeyString = "MIGdMA0GCSqGSIb3DQEBAQUAA4GLADCBhwKBgQC1hBlMytDpiLGqCNGfx+IvbRH9edqFcxJoL5CuEPOjr31u9PXTgtSuZhldKc9KpPR4j62M6+UxSs9abDd1/C0txQEB4Jxe/FPMOBmlvNHNHLw6htPx5JRHzN1cegi3W6Qd8YRMi3XfSx5tGx0NNLxuf+EDrE5NIVUdp0hpQ7yMFQIBAw==";
        byte[] publicKeyBytes = Convert.FromBase64String(publicKeyString);

        byte[] Exponent = { 3 };


        //Create a new instance of RSAParameters.
        RSAParameters RSAKeyInfo = new RSAParameters();

        //Set RSAKeyInfo to the public key values.
        RSAKeyInfo.Modulus = publicKeyBytes;
        RSAKeyInfo.Exponent = Exponent;

        //Import key parameters into RSA.
        Utilities.encrypter.ImportParameters(RSAKeyInfo);

        //UnityEngine.Debug.Log (encrypt ("This is a test String"));
    }

    /// <summary>
    /// Encrypts a string and returns the resulting string.
    /// </summary>
	public static String encrypt(String encryptThis)
    {
        string b64EncryptThis = Utilities.Base64Encode(encryptThis);

        //Encrypt the symmetric key and IV.
        byte[] encryptedString = encrypter.EncryptValue(Convert.FromBase64String(b64EncryptThis));

        //Add the encrypted test string to the form
        return Convert.ToBase64String(encryptedString);
    }

    /// <summary>
    /// Loads a file and returns the lines of the file in an array.
    /// </summary>
    public static string[] Loadfile(string fname)
    {
        string filename = Application.persistentDataPath + fname;
        string[] svdata_split = new string[1];
        if (System.IO.File.Exists(filename))
        {
            svdata_split = System.IO.File.ReadAllLines(filename);
            //local_stats = Array.ConvertAll<string, int>(svdata_split, int.Parse);
        }

        return svdata_split;
    }

    /// <summary>
    /// Overwrites the contents of the specified file with the string. A new
    ///  file is created if one doesn't exist. Returns false on failure.
    /// </summary>
	public static bool writefile(string fname, string toWrite)
    {
        string filename = Application.persistentDataPath + fname;
        //it is possible to meet exceptions
        try
        {
            System.IO.File.WriteAllText(filename, toWrite);
        }
        catch (Exception ex)
        {
            Logging.Log(ex.Message, Logging.LogLevel.CRITICAL);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Makes a request over the internet and waits for all
    ///  data from the ``WWW``` instance to finish downloading.
    ///  This should be called in a coroutine.
    /// </summary>
	public static IEnumerator WaitForRequest(WWW www, string filename = null, string filePath = null)
    {
        yield return www;

        //Check for errors
        if (www.error == null)
        {
            if ((filename != null) && (filePath != null))
            {
                System.IO.File.Delete(filePath);
                print("Deleted file: " + filename);
            }
            //JSONNode data = JSON.Parse (www.text);
            //Debug.Log("this is the parsed json data: " + data["testData"]);
            //Debug.Log(data["testData"]);
            // Logging.Log("WWW.Ok! " + www.text, Logging.LogLevel.LOW_PRIORITY);
        }
        else
        {
            Logging.Log("WWWError: " + www.error, Logging.LogLevel.ABNORMAL);
        }
    }

    /// <summary>
    /// Encodes the string into Base64 format.
    /// </summary>
    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    /// <summary>
    /// Writes save data. The data is obtained through various singletons in the system
    ///  as well as the provided level number.
    /// </summary>
	public static bool write_save(int lv)
    {
        string filename = "";

        if (GameMode.instance.get_mode() == GameMode.Game_Mode.RESTART || GameMode.instance.get_mode() == GameMode.Game_Mode.CONTINUE)
            filename = Application.persistentDataPath + "echosaved";
        else//load specific save for tutorial
            filename = Application.persistentDataPath + "echosaved_tutorial";

        System.IO.File.WriteAllText(filename, lv.ToString());
        return true;
    }

    /// <summary>
    /// Checks if an internet connection is available.
    ///  The returned string is an error message, where an empty string
    ///  indicates the lack of any errors.
    /// </summary>
    public void check_InternetConnection()
    {
        
    }

    /// <summary>
    /// Tests if the device is in landscape orientation.
    /// </summary>
    public static bool isDeviceLandscape()
    {
        bool isLandscape = false;
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        isLandscape = true;
#endif
#if UNITY_ANDROID || UNITY_IOS
        if ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft) || (Input.deviceOrientation == DeviceOrientation.LandscapeRight))
        {
            isLandscape = true;
        }
#endif
        return isLandscape;
    }

// Platform specific Utility
#if UNITY_IOS || UNITY_ANDROID
#endif
}
