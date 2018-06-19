using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Text;
using System.IO;

/// <summary>
/// A class that handles mostly things related to the board and some unrelated.
///  It mostly seems to handle aspects of the global game state.
///  For more information on some functions used, see the Unity 2D Roguelike Tutorial.
/// </summary>
/// <remarks>
/// <para>One of the functions of this class is to set up the board from a text file,
///  solve the maze internally, and save player progress.</para>
/// <para>Another is to play the instruction voice clips in the game.</para>
/// <para>Floating point arithmetic is also used to determine which cell the player is
///  closest to, and which direction the player is facing.</para>
/// </remarks>
public class BoardManager : MonoBehaviour
{
    public static float tileSize = 1.5f; // in meters. I don't know if this should go here.

    public enum Direction
    {
        FRONT,
        BACK,
        LEFT,
        RIGHT,
        OTHER,
    }

    // FIXME: I think this should not be public
    public string asciiLevelRep;
    // should probably make a different type choice here. I don't know what would be better
    public string gamerecord;

    int columns = Utilities.MAZE_SIZE;
    int rows = Utilities.MAZE_SIZE;
    public int max_total_level; // same as max_level in Main mode, for local stats use only
    public int[] local_stats;

    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject exit;
    GameObject player_ref;
    Player player_script;
    public bool turning_lock = false;

    private Transform boardHolder;
    private Vector3[] gridPositions = new Vector3[121];
    private List<int> wallIdxes = new List<int>();
    public List<Vector3> wallPositions = new List<Vector3>();
    private List<Vector3> startPositions = new List<Vector3>();
    public string mazeSolution = "";
    public static Vector3 exitPos;
    public static Vector3 startDir;

    public static Vector2 start_idx = new Vector2();
    public static Vector2 exit_idx = new Vector2();
    public static Vector2 player_idx = new Vector2();

    public static int numCornersAndDeadends = 0;

    public static bool left_start_pt = false;

    eventHandler eh;

    public static bool hasTappedAtCorner = false;

    public static bool finishedTutorialLevel1 = false;
    public static bool finishedTutorialLevel3 = false;

    static bool tutorial1Finished;
    static bool tutorial3Finished;

    public static bool reachedExit = false;
    public static bool gotBackToStart = false;

    private void Start()
    {
        eh = new eventHandler(InputModule.instance);
    }

    // Clears our list gridPositions and prepares it to generate a new board.
    void InitialiseList()
    {
        // Clear our list gridPositions.
        gridPositions = new Vector3[121];

        float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

        // Loop through x axis (columns).
        for (int x = -1; x < (rows + 1); x++)
        {
            // Within each column, loop through y axis (rows).
            for (int y = -1; y < (columns + 1); y++)
            {
                // At each index add a new Vector3 to our list with the x and y coordinates of that position.
                gridPositions[((y + 1) * 11) + (x + 1)] = new Vector3((float)x * scale, (float)y * scale, 0f);
            }
        }
    }

    public bool restore_audio = false;

    /// <summary>
    /// A function to signal the fact that the player has moved at least once.
    /// </summary>
    public static void set_left_start_pt(bool hasLeftStart)
    {
        left_start_pt = hasLeftStart;
    }

    /// <summary>
    /// At every frame, the player's position and game state is used to determine if certain sounds should be played.
    /// </summary>
    void Update()
    {
        tutorial1Finished = finishedTutorialLevel1;
        tutorial3Finished = finishedTutorialLevel3;

        if ((player_idx == start_idx) && (left_start_pt == true))
        {
            gotBackToStart = true;
        }

        else if (player_idx != start_idx)
        {
            left_start_pt = true;
            gotBackToStart = false;
        }

        if (player_idx == exit_idx)
        {
            reachedExit = true;     
        }

        else if (player_idx != exit_idx)
        {
            reachedExit = false;
        }
    }
    /// <summary>
    /// Gets the player's direction in the grid space.
    /// </summary>
    /// <returns>A ``Direction`` enum representing the player's direction.</returns>
    public Direction get_player_dir_world()
    {
        float threshold = 0.001f;
        Vector3 player_dir = player_script.get_player_dir("FRONT");
        if ((player_dir - Vector3.up).magnitude <= threshold)
        {
            return Direction.FRONT;
        }
        else if ((player_dir - Vector3.down).magnitude <= threshold)
        {
            return Direction.BACK;
        }
        else if ((player_dir - Vector3.left).magnitude <= threshold)
        {
            return Direction.LEFT;
        }
        else if ((player_dir - Vector3.right).magnitude <= threshold)
        {
            return Direction.RIGHT;
        }

        return Direction.OTHER;
    }

    /// <summary>
    /// Deserializes a string representing a direction.
    /// </summary>
    public static Direction StringToDir(string str)
    {
        if (str == "FRONT")
        {
            return Direction.FRONT;
        }
        else if (str == "BACK")
        {
            return Direction.BACK;
        }
        else if (str == "LEFT")
        {
            return Direction.LEFT;
        }
        else if (str == "RIGHT")
        {
            return Direction.RIGHT;
        }

        return Direction.OTHER;
    }

    // Sets up the outer walls and floor (background) of the game board.
    void BoardSetup()
    {
        // Instantiate Board and set boardHolder to its transform.
        boardHolder = transform.Find("Board");
        for (int i = 0; i < boardHolder.childCount; ++i)
        {
            Destroy(boardHolder.GetChild(i).gameObject);
        }

        float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

        // Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
        for (int x = 0; x <= (columns + 1); x++)
        {
            // Loop along y axis, starting from -1 to place floor or outerwall tiles.
            for (int y = 0; y <= (rows + 1); y++)
            {
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)]; // Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.

                // Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                if ((x == 0) || (x == (columns + 1)) || (y == 0) || (y == (rows + 1)))
                {
                    toInstantiate = wallTiles[0];
                }

                GameObject instance = Instantiate(toInstantiate, gridPositions[(y * 11) + x], Quaternion.identity) as GameObject; // Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                if ((x == 0) || (x == (columns + 1)) || (y == 0) || (y == (rows + 1)))
                {
                    instance.name = "Wall_" + (gridPositions[(y * 11) + x].x + 1) + "_" + (gridPositions[(y * 11) + x].y + 1);
                }

                // Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                Vector3 new_scale = instance.transform.localScale;
                new_scale *= scale;
                instance.transform.localScale = new_scale;
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    // Returns a vector of the location where the exit tile will be placed.
    // Determined randomly from path locations excluding starting position.
    private Vector3 getRandomVector(List<Vector3> positions)
    {
        int index = Random.Range(0, positions.Count);
        return positions[index];
    }

    void setup_level(int level, bool finishedLevel1Tutorial, bool finishedLevel3Tutorial)
    {
        wallPositions = new List<Vector3>();
        wallIdxes = new List<int>();
        startPositions = new List<Vector3>();

        if (level <= 2)
        {
            turning_lock = true;
        }
        else
        {
            turning_lock = false;
        }

        float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

        // clear existing walls and exit
        GameObject wall_parent = transform.Find("Walls").gameObject;
        for (int i = 0; i < wall_parent.transform.childCount; ++i)
        {
            Destroy(wall_parent.transform.GetChild(i).gameObject);
        }

        GameObject player = GameObject.Find("Player");

        finishedTutorialLevel1 = finishedLevel1Tutorial;
        finishedTutorialLevel3 = finishedLevel3Tutorial;

        tutorial1Finished = finishedTutorialLevel1;
        tutorial3Finished = finishedTutorialLevel3;

        // build level
        load_level_from_file("GameData/levels", level);
        int randomDelta = Random.Range(0, startPositions.Count);
        if (randomDelta == startPositions.Count)
        {
            randomDelta = startPositions.Count - 1;
        }

        player.transform.position = startPositions[randomDelta];
        start_idx = get_idx_from_pos(player.transform.position);
        player_idx = start_idx;
        print("Player start position: X = " + start_idx.x.ToString() + ", Y = " + start_idx.y.ToString());

        left_start_pt = false;
        gamerecord = gamerecord + "s@(" + start_idx.x.ToString() + "," + start_idx.y.ToString() + ")";

        for (int i = 0; i < wallPositions.Count; i++)
        {
            Vector3 position = wallPositions[i];
            GameObject tileChoice = wallTiles[0];
            GameObject new_wall = Instantiate(tileChoice, position, Quaternion.identity) as GameObject; // Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
            new_wall.name = "Wall_" + (position.x + 1) + "_" + (position.y + 1);
            Vector3 new_scale = new_wall.transform.localScale;
            new_scale *= scale;
            new_wall.transform.localScale = new_scale;
            new_wall.transform.SetParent(wall_parent.transform);
        }
        exit_idx = get_idx_from_pos(exitPos);
        print("Exit position: X = " + exit_idx.x.ToString() + ", Y = " + exit_idx.y.ToString());
        GameObject new_exit = Instantiate(exit, exitPos, Quaternion.identity) as GameObject;
        new_exit.transform.SetParent(wall_parent.transform);

        mazeSolution = "";
        searched = new bool[(columns + 1) * (rows + 1)];
        for (int i = 0; i < searched.Length; ++i)
        {
            searched[i] = false;
        }

        searched_temp = new bool[(columns + 1) * (rows + 1)];
        for (int i = 0; i < searched_temp.Length; ++i)
        {
            searched_temp[i] = false;
        }

        solveMaze(start_idx, "s");
        player.transform.rotation = Quaternion.identity;
        if (mazeSolution.Length >= 2)
        {
            if (mazeSolution[mazeSolution.Length - 2] == 'u')
            {
                player_script.rotateplayer_no_update(StringToDir("FRONT"));
                print("Player start direction set to Front.");
            }
            if (mazeSolution[mazeSolution.Length - 2] == 'd')
            {
                player_script.rotateplayer_no_update(StringToDir("BACK"));
                print("Player start direction set to Back.");
            }
            if (mazeSolution[mazeSolution.Length - 2] == 'l')
            {
                player_script.rotateplayer_no_update(StringToDir("LEFT"));
                print("Player start direction set to Left.");
            }
            if (mazeSolution[mazeSolution.Length - 2] == 'r')
            {
                player_script.rotateplayer_no_update(StringToDir("RIGHT"));
                print("Player start direction set to Right.");
            }
        }
        startDir = player_script.get_player_dir("FRONT");
    }

    // private help function to replace list.contain()
    bool _searchWallIdxes(Vector2 idx)
    {
        float threshhold = 0.1f;

        if ((Mathf.Abs(0 - idx.x) < threshhold) || (Mathf.Abs(0 - idx.y) < threshhold) || (Mathf.Abs((columns + 1) - idx.x) < threshhold) || (Mathf.Abs((rows + 1) - idx.y) < threshhold))
        {
            return true;
        }

        for (int i = 0; i < wallIdxes.Count; i += 2)
        {
            if ((Mathf.Abs(wallIdxes[i] - idx.x) <= threshhold) && (Mathf.Abs(wallIdxes[i + 1] - idx.y) <= threshhold))
            {
                return true;
            }
        }
        return false;
    }

    bool _idx_is_equal(Vector2 idx1, Vector2 idx2)
    {
        float threshold = 0.001f;
        if ((idx1 - idx2).magnitude <= threshold)
        {
            return true;
        }

        return false;
    }

    // private helper to get the distance from  wall/edge
    Vector2 _getDist(Vector2 gridIdx, Vector2 dir)
    {
        // Vector2 playerPos = gridIdx;
        // Debug.Log("dir is: " + dir.x.ToString() + ", "+ dir.y.ToString());
        while ((gridIdx.x > 0) && (gridIdx.x < columns + 1) && (gridIdx.y > 0) && (gridIdx.y < rows + 1))
        {
            if (_searchWallIdxes(gridIdx)) // the first one we met
            {
                break;
            }
            else
            {
                gridIdx += new Vector2(dir.x, dir.y);
            }
        }
        // now gridIdx is either a wall or a border
        return gridIdx;
    }

    /// <summary>
    /// Converts a position from Unity coordinates to a grid position.
    /// </summary>
    public Vector2 get_idx_from_pos(Vector3 pos)
    {
        float threshhold = 0.01f;
        int y_idx = -1, x_idx = -1;

        // get which index of gridPos() player is in
        for (int i = 1; i < (rows + 1); i++)
        {
            for (int j = 1; j < (columns + 1); ++j)
            {
                if ((gridPositions[i * (columns + 2) + j] - pos).magnitude <= threshhold)
                {
                    y_idx = i; x_idx = j;
                    break;
                }
            }
        }

        Vector2 result = new Vector2(x_idx, y_idx);
        return result;
    }

    /// <summary>
    /// Sets up the game board (grid positions), sound clips and save data as an initialization step. 
    /// </summary>
    public void SetupScene(int level, bool finishedLevel1Tutorial, bool finishedLevel3Tutorial, GameMode.Game_Mode mode)
    {
        player_ref = GameObject.Find("Player"); // Find Player object.
        player_script = player_ref.GetComponent<Player>(); // Get the Player script.

        InitialiseList(); // Reset our list of gridpositions.
        BoardSetup(); // Creates the outer walls and floor.
        LoadLocalStats();
        setup_level(level, finishedLevel1Tutorial, finishedLevel3Tutorial);
    }

    /// <summary>
    /// Converts a 'True' or 'False' string to the appropriate boolean.
    /// </summary>
    /// <param name="convertString"></param>
    /// <returns></returns>
    public static bool StringToBool(string convertString)
    {
        if (convertString.Equals("True"))
        {
            return true;
        }
        else if (convertString.Equals("False"))
        {
            return false;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Converts an integer to a string.
    /// </summary>
    /// <param name="tochange"></param>
    /// <returns></returns>
    string _InttoString(int tochange)
    {
        return tochange.ToString();
    }

    /// <summary>
    /// Writes some game statistics into persistent data.
    /// </summary>
    public bool write_local_stats()
    {
        // string filename = Application.persistentDataPath + "echostats";
        string filename = Path.Combine(Application.persistentDataPath, "echostats");
        string[] toWrite = Array.ConvertAll<int, string>(local_stats, _InttoString);
        string final = "";
        foreach (string itm in toWrite)
        {
            final += itm;
            final += "\n";
        }
        System.IO.File.WriteAllText(filename, final);
        return true;
    }

    /// <summary>
    /// Loads the local game statistics.
    /// </summary>
    /// <returns></returns>
    bool LoadLocalStats()
    {
        // string filename = Application.persistentDataPath + "echostats";
        string filename = Path.Combine(Application.persistentDataPath, "echostats");
        local_stats = new int[max_total_level + 1];

        string[] svdata_split;
        if (System.IO.File.Exists(filename))
        {
            svdata_split = System.IO.File.ReadAllLines(filename);
            local_stats = Array.ConvertAll<string, int>(svdata_split, int.Parse);
        }
        else
        {
            for (int i = 0; i < local_stats.Length; ++i)
            {
                local_stats[i] = 0;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// Get information about level currently being loaded from levels.txt
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="level_wanted"></param>
    /// <returns></returns>
    public bool load_level_from_file(string filename, int level_wanted = 1)
    {
        TextAsset lvldata = Resources.Load(filename) as TextAsset;
        if (lvldata == null)
        {
            Logging.Log("Cannot open file at:", Logging.LogLevel.CRITICAL);
            Logging.Log(filename, Logging.LogLevel.CRITICAL);
            return false;
        }
        string[] lvldata_split = lvldata.text.Split('\n');
        bool reading_level = false;
        int cur_y = Utilities.MAZE_SIZE - 1; // start from top left corner

        asciiLevelRep = "";
        gamerecord = "";

        // read through the file until desired level is found
        foreach (string line in lvldata_split)
        {
            if (line.Substring(0, 3) == "END")
            { // reach end of a level layout
                reading_level = false;
            }

            if (reading_level)
            { // actually loading layout
                // check for valid index
                asciiLevelRep += line;
                if (cur_y >= 0)
                {
                    // do things
                    for (int i = 0; i < line.Length; ++i)
                    {
                        if (line[i] == 'w') // wall
                        {
                            wallPositions.Add(gridPositions[(cur_y + 1) * (columns + 2) + (i + 1)]);
                            wallIdxes.Add(i + 1);
                            wallIdxes.Add(cur_y + 1);
                        }
                        else if (line[i] == 'e') // exit
                        {
                            exitPos = gridPositions[(cur_y + 1) * (columns + 2) + (i + 1)];
                        }
                        else if (line[i] == 's') // start positions
                        {
                            startPositions.Add(gridPositions[(cur_y + 1) * (columns + 2) + (i + 1)]);
                        }
                    }
                    cur_y -= 1;
                }
            }

            // flow control
            if (line.Length >= 7)
            {
                if (line.Substring(0, 6) == "LEVEL_")
                {
                    // get the current level we are reading               
                    string currentLevelString = "";
                    int cornerInfoStart = 0;

                    // Find the level we are reading
                    for (int i = 0; i < 3; i++)
                    {
                        // If the current character is not a '_'
                        if (line.Substring((6 + i), 1) != "_")
                        {
                            // If the line has 'DEFAULT'
                            if (line.Substring((6 + i), 1) == "D")
                            {
                                currentLevelString = "DEFAULT";
                                print("Level Searched: Level Default.\n");
                            }
                            // Otherwise, add the character to the level string.
                            else if (line.Substring((6 + i), 1) != "D")
                            {
                                currentLevelString += line.Substring((6 + i), 1);
                                if (i == 2)
                                {
                                    // Get the start position of the corner information.
                                    cornerInfoStart = (6 + i) + 3;
                                }
                            }
                        }
                        // If the current character is a '_', we have found the level number, so stop searching.
                        else if (line.Substring((6 + i), 1) == "_")
                        {
                            // Get the start position of the corner information.
                            cornerInfoStart = (6 + i) + 2;
                            i = 3;
                        }
                    }

                    if ((currentLevelString != "DEFAULT") && (currentLevelString != "0"))
                    {
                        // Find the number of corners in the level
                        string cornerString = "";
                        int deadendInfoStart = 0;

                        for (int j = 0; j < 2; j++)
                        {
                            if ((currentLevelString != "0") && (cornerInfoStart != 0))
                            {
                                if (line.Substring((cornerInfoStart + j), 1) != "_")
                                {
                                    cornerString += line.Substring((cornerInfoStart + j), 1);
                                    if (j == 1)
                                    {
                                        // Get the start position of the deadend information.
                                        deadendInfoStart = (cornerInfoStart + j) + 3;
                                    }
                                }
                                else if (line.Substring((cornerInfoStart + j), 1) == "_")
                                {
                                    if (j == 0)
                                    {
                                        print("Level Searched: No corner information.");
                                    }
                                    // Get the start position of the deadend information.
                                    deadendInfoStart = (cornerInfoStart + j) + 2;
                                }
                            }
                        }

                        string deadendString = "";

                        if ((currentLevelString != "0") && (cornerInfoStart != 0) && (deadendInfoStart != 0))
                        {
                            int remainingCharacters = line.Length - deadendInfoStart;
                            if ((remainingCharacters == 1) || (remainingCharacters == 2))
                            {
                                deadendString = line.Substring(deadendInfoStart, remainingCharacters);
                            }
                            else if (remainingCharacters == 0)
                            {
                                print("Level Searched: No deadend information.");
                            }
                            else if (remainingCharacters > 2)
                            {
                                print("Level Searched: Too many deadend characters.");
                            }
                        }

                        int level_reading = Convert.ToInt32(currentLevelString);
                        if (level_reading == level_wanted)
                        { //  we found the level we want
                            print("Level Searched: Level = " + currentLevelString + "\n");
                            if (cornerString != "")
                            {
                                print("Level Searched: Corners in Level " + currentLevelString + " = " + cornerString);
                            }
                            if (deadendString != "")
                            {
                                print("Level Searched: Deadends in Level " + currentLevelString + " = " + deadendString);
                            }
                            if ((cornerString != "") && (deadendString != ""))
                            {
                                int levelCorners = Convert.ToInt32(cornerString);
                                int levelDeadends = Convert.ToInt32(deadendString);
                                numCornersAndDeadends = levelCorners + levelDeadends;
                            }
                            reading_level = true;
                        }
                        else if (level_reading > level_wanted)
                        {
                            reading_level = false;
                            return true;
                        }
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the maximum number of levels in the game.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public int get_level_count(string filename)
    {
        TextAsset lvldata = Resources.Load(filename) as TextAsset;
        if (lvldata == null)
        {
            Logging.Log("Cannot open file at:", Logging.LogLevel.CRITICAL);
            Logging.Log(filename, Logging.LogLevel.CRITICAL);
            return 0;
        }
        string[] lvldata_split = lvldata.text.Split('\n');

        int level_count = 0;
        foreach (string line in lvldata_split)
        {
            if (line.Length >= 7)
            {
                if (line.Substring(0, 6) == "LEVEL_")
                {
                    level_count += 1; // get the current level we are reading
                }
            }
        }
        return level_count;
    }

    bool[] searched;

    /// <summary>
    /// Solves the maze with a depth-first search.
    /// </summary>
    bool solveMaze(Vector2 idx, string dir)
    {
        if ((idx.x > columns) || (idx.x < 1) || (idx.y > rows) || (idx.y < 1)) // just in case, so I widen the range
        {
            return false;
        }

        if (searched[(int)(((columns + 1) * idx.y) + idx.x)])
        {
            return false;
        }

        searched[(int)(((columns + 1) * idx.y) + idx.x)] = true;

        if (_searchWallIdxes(idx))
        {
            return false;
        }

        if (exit_idx == idx)
        {
            mazeSolution += dir;
            return true;
        }

        bool result = false || solveMaze(new Vector2(idx.x, idx.y + 1f), "u") || solveMaze(new Vector2(idx.x, idx.y - 1f), "d") || solveMaze(new Vector2(idx.x + 1f, idx.y), "r") || solveMaze(new Vector2(idx.x - 1f, idx.y), "l");

        if (result)
        {
            mazeSolution += dir;
        }

        return result;
    }

    // solve maze during gameplay
    public string sol;
    public bool[] searched_temp;

    /// <summary>
    /// Solves the maze with a depth-first search.
    /// </summary>
    /// FIXME: This is code duplication.
    public bool solveMazeMid(Vector2 idx, string dir)
    {
        if ((idx.x > columns) || (idx.x < 1) || (idx.y > rows) || (idx.y < 1)) // just in case, so I widen the range
        {
            return false;
        }

        if (searched_temp[(int)(((columns + 1) * idx.y) + idx.x)])
        {
            return false;
        }

        searched_temp[(int)(((columns + 1) * idx.y) + idx.x)] = true;

        if (_searchWallIdxes(idx))
        {
            return false;
        }

        if (exit_idx == idx)
        {
            sol += dir;
            return true;
        }

        bool result = false || solveMazeMid(new Vector2(idx.x, idx.y + 1f), "u") || solveMazeMid(new Vector2(idx.x, idx.y - 1f), "d") || solveMazeMid(new Vector2(idx.x + 1f, idx.y), "r") || solveMazeMid(new Vector2(idx.x - 1f, idx.y), "l");

        if (result)
        {
            sol += dir;
        }

        return result;
    }

    /// <summary>
    /// Presents the full solution to the maze as a string.
    /// </summary>
    public string getHint(Vector2 idx, string dir)
    {
        string result = "";
        sol = "";
        solveMazeMid(idx, dir);
        if (sol.Length >= 2)
        {
            result = sol[GameManager.instance.boardScript.sol.Length - 2].ToString();
        }

        return result;
    }
}