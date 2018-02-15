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
        TUTORIAL,
        RESTART,
        CONTINUE
    }

    public static GameMode instance = null;		//Allows other scripts to call functions from SoundManager.			
    public Game_Mode gamemode;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void init()
    {
        gamemode = Game_Mode.RESTART;
    }

    public Game_Mode get_mode()
    {
        return gamemode;
    }

    public void set_mode(Game_Mode gm)
    {
        gamemode = gm;
    }
}
