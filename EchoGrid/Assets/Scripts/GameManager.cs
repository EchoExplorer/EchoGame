using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.IO;

/// <summary>
/// This class coordinates the initial loading of the game and toggles
///  the on-screen text display.
/// </summary>
public class GameManager : MonoBehaviour
{
    public BoardManager boardScript;

    public static GameManager instance = null;
    public static bool levelImageActive = true;
    [HideInInspector]
    public bool playersTurn = true;

    string debugPlayerInfo = "";

    private const int MAX_TUTORIAL_LEVEL = 11;
    private const int MAX_LEVEL = 150;
    public float levelStartDelay = 2f;
    public int level = 0;
    public Text levelText;
    public GameObject levelImage;
    private bool doingSetup = true;
    public DbAccess db;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        boardScript = GetComponent<BoardManager>();

        // Duplicated loading: LoadSaved() will be called in InitGame().
        // LoadSaved();
    }

    bool LoadSaved()
    {
        string filename = "";
        string[] svdata_split;
        GameMode.Game_Mode current = GameMode.instance.get_mode();

        // choose save for tutorial and normal game
        if ((current == GameMode.Game_Mode.RESTART) || (current == GameMode.Game_Mode.CONTINUE))
        {
            filename = Application.persistentDataPath + "echosaved";
        }
        // load specific save for tutorial
        else
        {
            filename = Application.persistentDataPath + "echosaved_tutorial";
        }

        if (System.IO.File.Exists(filename))
        {
            svdata_split = System.IO.File.ReadAllLines(filename);
            //read existing data
            level = Int32.Parse(svdata_split[0]);
            string level1Finished = svdata_split[1];
            string level3Finished = svdata_split[2];
            if (level1Finished.Equals("True") == true)
            {
                GameMode.finishedLevel1Tutorial = true;
            }
            else if (level1Finished.Equals("False") == true)
            {
                GameMode.finishedLevel1Tutorial = false;
            }
            if (level3Finished.Equals("True") == true)
            {
                GameMode.finishedLevel3Tutorial = true;
            }
            else if (level3Finished.Equals("False") == true)
            {
                GameMode.finishedLevel3Tutorial = false;
            }

            if (level <= 11)
            {
                GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                GameMode.write_save_mode(level, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.Game_Mode.TUTORIAL);
            }
            else if (level >= 12)
            {
                GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
                GameMode.write_save_mode(level, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.Game_Mode.CONTINUE);
            }            
        }
        else
        {
            if ((current == GameMode.Game_Mode.RESTART) || (current == GameMode.Game_Mode.CONTINUE))
            {
                level = MAX_TUTORIAL_LEVEL + 1;
                GameMode.finishedLevel1Tutorial = true;
                GameMode.finishedLevel3Tutorial = true;
                GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
                GameMode.write_save_mode(level, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.Game_Mode.CONTINUE);
            }
            else
            {
                level = 1;
                GameMode.finishedLevel1Tutorial = false;
                GameMode.finishedLevel3Tutorial = false;
                GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                GameMode.write_save_mode(level, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.Game_Mode.TUTORIAL);
            }                      
        }
               
        return true;
    }    

    //Initializes the game for each level.
    //TODO(agotsis) Analyze database
    void InitGame()
    {
#if UNITY_IOS || UNITY_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Landscape;
#endif

        //Setup database for the first time
        using (db = new DbAccess("Data Source=LocalDataBase.db")) { }
        //db.CreateTable("PlayerInfo",new string[]{"id","name","high_score"}, new string[]{"integer","text","integer"});
        //db.CreateTable("AudioFiles",new string[]{"id","echo name","file_path", "game_level"}, new string[]{"integer","text","text", "integer"});

        doingSetup = true;
        //levelImage = UICanvas.instance.transform.Find("LevelImage").gameObject; //GameObject.Find("LevelImage");
        //levelText = levelImage.transform.Find("LevelText").gameObject.GetComponent<Text>();
        //Set the text of levelText to the string "Day" and append the current level number.;

        //Set levelImage to block player's view of the game board during setup.
        levelImage.SetActive(true);

        //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
        Invoke("StartGame", levelStartDelay);

        boardScript.max_total_level = boardScript.get_level_count("GameData/levels");

        if (Player.changingLevel == false)
        {
            Player.loadingScene = true;

            if (GM_main_pre.skippingTutorial == 0)
            {
                GameMode.Game_Mode current = GameMode.instance.get_mode();
                if (current == GameMode.Game_Mode.RESTART)
                {
                    GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
                    level = MAX_TUTORIAL_LEVEL + 1;
                }
                else if (current == GameMode.Game_Mode.TUTORIAL_RESTART)
                {
                    GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                    level = 1;
                }
                GameMode.finishedLevel1Tutorial = false;
                GameMode.finishedLevel3Tutorial = false;
                GameMode.write_save_mode(level, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, current);
            }
            else if (GM_main_pre.skippingTutorial == 1)
            {
                LoadSaved();
            }
            else if (GM_main_pre.skippingTutorial == 2)
            {
                GameMode.Game_Mode current = GameMode.instance.get_mode();
                if (current == GameMode.Game_Mode.RESTART)
                {
                    GameMode.instance.gamemode = GameMode.Game_Mode.CONTINUE;
                    level = MAX_TUTORIAL_LEVEL + 1;
                }
                else if (current == GameMode.Game_Mode.TUTORIAL_RESTART)
                {
                    GameMode.instance.gamemode = GameMode.Game_Mode.TUTORIAL;
                    level = 1;
                }
                GameMode.finishedLevel1Tutorial = true;
                GameMode.finishedLevel3Tutorial = true;
                GameMode.write_save_mode(level, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, current);
            }
        }
        levelText.text = "Loading level " + level.ToString();
        boardScript.SetupScene(level, GameMode.finishedLevel1Tutorial, GameMode.finishedLevel3Tutorial, GameMode.instance.gamemode);       
    }

    //Hides black image used between levels
    void StartGame()
    {
        //Disable the levelImage gameObject.
        if (!levelImageActive)
            HideLevelImage();
        else
            UnHideLevelImage();
        //Set doingSetup to false allowing player to move again.
        doingSetup = false;
        playersTurn = true;
        Player.loadingScene = false;    
    }

    /// <summary>
    /// Displays the on-screen text display.
    /// </summary>
    public void UnHideLevelImage()
    {
        levelText.text = "level " + level.ToString() + "\n";
        levelText.text += "Game In Progress" + "\n";
        levelText.text += "Hold two fingers" + "\n";
        levelText.text += "to open menu";        
        levelImage.SetActive(true);
        levelImageActive = true;
    }

    /// <summary>
    /// Hides the on-screen text display.
    /// </summary>
    public void HideLevelImage()
    {
        //Disable the levelImage gameObject.
        levelImage.SetActive(false);
        levelImageActive = false;
    }

    //This is called each time a scene is loaded.
    void OnLevelWasLoaded(int index)
    {
        // Since the gameObject is not destroyed automatically, the instance should be checked before calling this method.
        if (this != instance)
        {
            return;
        }
        // Call InitGame to initialize our level.
        levelImage = GameObject.Find("LevelImage").gameObject;
        levelText = levelImage.transform.Find("LevelText").gameObject.GetComponent<Text>();

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        levelText.transform.localScale -= new Vector3(0.3f, 0.3f, 0.3f);
#endif

        Player.reachedExit = false;
        BoardManager.reachedExit = false;
        levelImage.SetActive(false);
        InitGame();
    }

    /// <summary>
    /// Disables the instance.
    /// </summary>
    public void GameOver()
    {
        enabled = false;
    }

    /// <summary>
    /// Disables the screen timeout.
    /// </summary>
    void Update()
    {
        //HACK: this should not run once per frame.
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

        //FIXME: Dead code
        if (playersTurn || doingSetup)
            return;
    }
}
