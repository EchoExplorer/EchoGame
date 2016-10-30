using UnityEngine;
using System.Collections;
using System;

public class Utilities : MonoBehaviour {
	public static int MAZE_SIZE = 9;
	public static int SCALE_REF = 8;
	public static float exit_touch_time = 1.5f;

	public static bool OLD_ANDROID_SUPPORT = true;

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
		System.IO.File.WriteAllText (filename, toWrite);
		return true;
	}
}
