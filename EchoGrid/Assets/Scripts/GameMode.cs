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

    public static GameMode instance = null;	// Allows other scripts to call functions from SoundManager.			
    public Game_Mode gamemode = Game_Mode.NONE; // Initialize the game mode to None.

    public static bool finishedLevel1Tutorial = false; // The level 1 gesture tutorial starts out as unfinished.
    public static bool finishedLevel3Tutorial = false; // The level 3 gesture tutorial starts out as unfinished.

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

    // Get the current game mode.
    public Game_Mode get_mode()
    {
        return gamemode;
    }

    // Set the game mode.
    public void set_mode(Game_Mode gm)
    {
        gamemode = gm;
    }

    /// <summary>
    /// Saves information about the game state as persistent data.
    /// </summary>
    public static bool write_save_mode(int level, bool level1TutorialFinished, bool level3TutorialFinished, GameMode.Game_Mode mode)
    {
        string filename = ""; // Initialize the string to hold the filename.
        string[] fileLines = new string[3]; // Initialize the array to hold the lines of the file.

        // If the game mode is Restart or Continue.
        if ((mode == GameMode.Game_Mode.RESTART) || (mode == GameMode.Game_Mode.CONTINUE))
        {
            filename = Application.persistentDataPath + "echosaved"; // Set this string as the filename.
        }
        // If the game mode is Tutorial_Restart or Tutorial
        else
        {
            filename = Application.persistentDataPath + "echosaved_tutorial"; // Set this string as the filename.
        }        

        fileLines[0] = level.ToString(); // Set the first line of the file to be the level passed.

        if (level1TutorialFinished == true)
        {
            fileLines[1] = "True"; // Set the second line of the file to 'True' if the level 1 gesture tutorial has been finished.
        }
        else if (level1TutorialFinished == false)
        {
            fileLines[1] = "False"; // Set the second line of the file to 'False' if the level 1 gesture tutorial has not been finished.
        }

        if (level3TutorialFinished == true)
        {
            fileLines[2] = "True"; // Set the third line of the file to 'True' if the level 3 gesture tutorial has been finished.
        }
        else if (level3TutorialFinished == false)
        {
            fileLines[2] = "False"; // Set the third line of the file to 'False' if the level 3 gesture tutorial has not been finished.
        }

        System.IO.File.WriteAllLines(filename, fileLines); // Write each line in the array to the file with the given filename.
        return true;
    }
}
