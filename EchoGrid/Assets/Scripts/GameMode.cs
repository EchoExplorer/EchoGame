using UnityEngine;
using System.Collections;

/// <summary>
/// A singleton instance whose sole purpose is to hold one aspect of the current game state
///  (whether it is in the tutorial, the game is played from the start, or if progress is continued).
/// </summary>
public class GameMode : MonoBehaviour
{
    public enum Game_Mode
    {
        NONE,
        TUTORIAL_RESTART, // Start a new tutorial
        TUTORIAL, // Continue saved tutorial
        RESTART, // Start a new normal game
        CONTINUE, // Coninue saved normal game 
    }

    public static GameMode instance = null;		//Allows other scripts to call functions from SoundManager.			
    public Game_Mode gamemode = Game_Mode.NONE;

    public static bool finishedLevel1Tutorial = false;
    public static bool finishedLevel3Tutorial = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public Game_Mode get_mode()
    {
        return gamemode;
    }

    public void set_mode(Game_Mode gm)
    {
        gamemode = gm;
    }

    /// <summary>
    /// Saves information about the game state as persistent data.
    /// </summary>
    public static bool write_save_mode(int lv, bool level1TutorialFinished, bool level3TutorialFinished, GameMode.Game_Mode mode)
    {
        string filename = "";
        string[] fileLines = new string[3];

        if ((mode == GameMode.Game_Mode.RESTART) || (mode == GameMode.Game_Mode.CONTINUE))
        {
            filename = Application.persistentDataPath + "echosaved";
        }
        else
        {
            filename = Application.persistentDataPath + "echosaved_tutorial";
        }        

        fileLines[0] = lv.ToString();

        if (level1TutorialFinished == true)
        {
            fileLines[1] = "True";
        }
        else if (level1TutorialFinished == false)
        {
            fileLines[1] = "False";
        }

        if (level3TutorialFinished == true)
        {
            fileLines[2] = "True";
        }
        else if (level3TutorialFinished == false)
        {
            fileLines[2] = "False";
        }

        System.IO.File.WriteAllLines(filename, fileLines);
        return true;
    }
}
