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

    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    public enum JunctionType
    {
        INVALID,
        DEADEND,
        T,
        LL,
        RL,
        CROSS,
    }

    public enum Direction
    {
        FRONT,
        BACK,
        LEFT,
        RIGHT,
        OTHER,
    }

    //FIXME: I think this should not be public
    public string asciiLevelRep;
    //should probably make a different type choice here. I don't know what would be better
    public string gamerecord;

    /// <summary>
    /// Stores information about an echo to be played.
    /// </summary>
    public struct echoDistData
    {
        public int front, back, left, right; //in blocks
        public float frontDist, backDist, leftDist, rightDist; //in meters
        public JunctionType fType, bType, lType, rType;
        public int exitpos;//0: no exit, 1:left, 2:right, 3:front, 4:back

        //TODO Weynu, is this a bad design choice? Call method to calcuate distances.
        public void updateDistances()
        {
            float halfSize = BoardManager.tileSize / 2;//0.75 in this case
            float wallDist = 0.75f, shortDist = 2.25f, midDist = 6.75f, longDist = 12.75f;
            //the old way:
            //frontDist = halfSize + (front - 1) * BoardManager.tileSize;
            //backDist = halfSize + (back - 1) * BoardManager.tileSize;
            //leftDist = halfSize + (left - 1) * BoardManager.tileSize;
            //rightDist = halfSize + (right - 1) * BoardManager.tileSize;
            //the new way:
            //front is still the raw data
            frontDist = halfSize + (front - 1) * BoardManager.tileSize;
            //the other three sides follows the short-medium-long format
            backDist = halfSize + (back - 1) * BoardManager.tileSize;
            leftDist = halfSize + (left - 1) * BoardManager.tileSize;
            rightDist = halfSize + (right - 1) * BoardManager.tileSize;

        }

        public string all_jun_to_string()
        {
            string juns;
            juns = jun_to_string(fType) + ", " + jun_to_string(bType) + ", " +
                jun_to_string(lType) + ", " + jun_to_string(rType);
            return juns;
        }

        public string jun_to_string(JunctionType jun)
        {
            if (jun == JunctionType.INVALID)
                return "Invalid";
            else if (jun == JunctionType.DEADEND)
                return "D";  //deadend
            else if (jun == JunctionType.T)
                return "T";  //T
            else if (jun == JunctionType.LL)
                return "EL"; //elbow left
            else if (jun == JunctionType.RL)
                return "ER"; //elbow right
            else if (jun == JunctionType.CROSS)
                return "Cross";

            return "oops an error";
        }
    }

    //HACK: this is not C++, structs shouldn't be classes.
    /// <summary>
    /// A struct holding audio clips and other miscellaneous information of some kind.
    /// </summary>
    public struct pos_and_action
    {
        public Vector2 pos;
        public bool once;//play the audio only once?(usually yes)
        public bool tap;//player tap the screen?(play an echo?)
        public Direction dir;//FRONT, BACK, LEFT, RIGHT
        public List<string> clip_names;
        public List<AudioClip> clips;

        public void init()
        {
            once = true;
            tap = false;
            pos = new Vector2();
            dir = Direction.OTHER;
            clip_names = new List<string>();
            clips = new List<AudioClip>();
        }
    }

    /// <summary>
    /// Contains a set of instruction voices to be played at various points in the game.
    /// </summary>
    public struct level_voice_list
    {
        public List<string> play_at_begin;
        public List<AudioClip> clip_at_begin;
        public int clip_begin;
        public List<string> play_at_exit;
        public List<AudioClip> clip_at_exit;
        public int clip_exit;
        public List<pos_and_action> ingame;
        public List<int> ingame_cur_clip;
        public List<string> play_when_return;//played when goes back to starting point somehow
        public List<AudioClip> clip_when_return;
        public int clip_return;

        public void init()
        {
            play_at_begin = new List<string>();
            clip_at_begin = new List<AudioClip>();
            clip_begin = 0;
            play_at_exit = new List<string>();
            clip_at_exit = new List<AudioClip>();
            clip_exit = 0;
            ingame = new List<pos_and_action>();
            ingame_cur_clip = new List<int>();
            play_when_return = new List<string>();//played when goes back to starting point somehow
            clip_when_return = new List<AudioClip>();
            clip_return = 0;
        }

        public void clear()
        {
            play_at_begin.Clear();
            clip_at_begin.Clear();
            clip_begin = 0;
            play_at_exit.Clear();
            clip_at_exit.Clear();
            clip_exit = 0;
            ingame.Clear();
            play_when_return.Clear();//played when goes back to starting point somehow
            clip_when_return.Clear();
            clip_return = 0;
            init();
        }
    }

    //HACK: lots of public values with little encapsulation.
    int columns = Utilities.MAZE_SIZE;
    int rows = Utilities.MAZE_SIZE;
    public int max_level;
    public int max_total_level;//same as max_level in Main mode, for local stats use only
    public int min_level;
    public int[] local_stats;

    // number of walls and such
    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] outerWallTiles;
    public Count wallCount = new Count(5, 9);	//Lower and upper limit for our random number of walls per level.
    public GameObject exit;
    GameObject player_ref;
    Player player_script;
    public bool turning_lock = false;

    private Transform boardHolder;
    private List<Vector3> gridPositions = new List<Vector3>();
    private List<int> wallIdxes = new List<int>();
    public List<Vector3> wallPositions = new List<Vector3>();
    private List<Vector3> playerPositions = new List<Vector3>();
    public string mazeSolution = "";
    level_voice_list level_voices = new level_voice_list();
    public Vector3 exitPos;
    Vector3 startPos;
    Vector3 startDir;

    //audios
    int cur_clip = 1;
    int cur_level;
    int total_clip = 11;
    public AudioClip latest_clip = new AudioClip();//used to repeat instructions
    int latest_clip_idx;
    AudioClip lv_1_move, lv_1_exit;

    bool resest_audio = true;
    bool skip_clip = false;

    //Clears our list gridPositions and prepares it to generate a new board.
    void InitialiseList()
    {
        //Clear our list gridPositions.
        gridPositions.Clear();

        float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

        //Loop through x axis (columns).
        for (int x = -1; x < rows + 1; x++)
        {
            //Within each column, loop through y axis (rows).
            for (int y = -1; y < columns + 1; y++)
            {
                //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                gridPositions.Add(new Vector3((float)y * scale, (float)x * scale, 0f));
            }
        }
    }

    /// <summary>
    /// Loads audio as an initialization step.
    /// </summary>
    void LoadAudio()
    {
        //load the audio with those filename
        level_voices.clip_at_begin.Clear();
        foreach (string clip_name in level_voices.play_at_begin)
        {
            string filename = "instructions/" + clip_name.Substring(0, clip_name.Length - 1);
            AudioClip clip = Resources.Load(filename) as AudioClip;
            level_voices.clip_at_begin.Add(clip);
        }

        level_voices.clip_at_exit.Clear();
        foreach (string clip_name in level_voices.play_at_exit)
        {
            string filename = "instructions/" + clip_name.Substring(0, clip_name.Length - 1);
            AudioClip clip = Resources.Load(filename) as AudioClip;
            level_voices.clip_at_exit.Add(clip);
        }

        foreach (pos_and_action itm in level_voices.ingame)
        {
            foreach (string clip_name in itm.clip_names)
            {
                string filename = "instructions/" + clip_name.Substring(0, clip_name.Length - 1);
                AudioClip clip = Resources.Load(filename) as AudioClip;
                itm.clips.Add(clip);
            }
        }

        level_voices.clip_when_return.Clear();
        foreach (string clip_name in level_voices.play_when_return)
        {
            string filename = "instructions/" + clip_name.Substring(0, clip_name.Length - 1);
            AudioClip clip = Resources.Load(filename) as AudioClip;
            level_voices.clip_when_return.Add(clip);
        }
    }

    public bool restore_audio = false;
    /// <summary>
    /// Plays an audio clip with some playback properties stored in the parameters.
    /// </summary>
    public int play_audio(List<AudioClip> audios, int cur, bool restore = false, bool isReturn = false)
    {
        if (!SoundManager.instance.voiceSource.isPlaying)
            SoundManager.instance.voiceSource.clip = null;

        if (restore_audio)
        {
            if (latest_clip != null)
            {
                if (SoundManager.instance.PlayVoice(latest_clip))
                {
                    restore_audio = false;
                    latest_clip = null;
                }
            }
            else
                restore_audio = false;
        }
        else
        {
            if (!skip_clip)
            {
                if ((cur == 0) && (!isReturn))
                {
                    if (restore)
                    {
                        latest_clip = SoundManager.instance.voiceSource.clip;
                        restore_audio = true;
                    }
                    SoundManager.instance.PlayVoice(audios[cur], true);
                    //update history clip list
                    return (cur + 1);
                }
                else if ((cur == 0) && (isReturn))
                {
                    if (restore)
                    {
                        latest_clip = SoundManager.instance.voiceSource.clip;
                        restore_audio = true;
                    }
                    SoundManager.instance.PlayVoice(audios[cur], true);
                    //update history clip list
                    return -1;
                }
                else if ((cur == -1) && (isReturn))
                {
                    SoundManager.instance.PlayVoice(audios[0], false);
                    //update history clip list
                    return -1;
                }
                else if (SoundManager.instance.PlayVoice(audios[cur]))
                {
                    //update history clip list
                    return (cur + 1);
                }
            }
            else
            {//skip instruction
                /*
                restore_audio = true;
                skip_clip = false;
                if (cur + 1 < audios.Count){
                    SoundManager.instance.PlayVoice (audios [cur + 1], true);
                    return (cur + 2);
                }else
                    return (cur + 1);
                    */
            }
        }

        return cur;
    }

    /// <summary>
    /// Replays the previous instruction voice clip.
    /// </summary>
    public void repeat_latest_instruction()
    {
        //if (latest_clip_idx >= 0) {
        //	SoundManager.instance.PlayVoice (latest_clips [latest_clip_idx], true);
        //	latest_clip_idx -= 1;
        //}
    }

    /// <summary>
    /// Sets up the object such that the next instruction will be skipped.
    /// </summary>
    public void skip_instruction()
    {
        skip_clip = true;
    }

    /// <summary>
    /// A function to signal the fact that the player has moved at least once.
    /// </summary>
    bool left_start_pt = false;
    public void set_left_start_pt(bool newf)
    {
        left_start_pt = newf;
    }

    /// <summary>
    /// At every frame, the player's position and game state is used to determine if certain sounds should be played.
    /// </summary>
    void Update()
    {

        float threshold = 0.001f;
        Vector2 idx_pos = get_idx_from_pos(player_ref.transform.position);
        bool ingame_playing = false;
        //play sounds according to positions
        if ((idx_pos - get_idx_from_pos(exitPos)).magnitude <= threshold)
        {
            if (level_voices.clip_exit < level_voices.clip_at_exit.Count)
                level_voices.clip_exit = play_audio(level_voices.clip_at_exit, level_voices.clip_exit);
        }
        else if (((idx_pos - get_idx_from_pos(startPos)).magnitude <= threshold) && left_start_pt && (player_script.get_player_dir("BACK") == startDir))
        {
            if (level_voices.clip_return < level_voices.clip_when_return.Count)
                level_voices.clip_return = play_audio(level_voices.clip_when_return, level_voices.clip_return, true, true);
        }
        else
        {
            for (int i = 0; i < level_voices.ingame.Count; ++i)
            {//find out that if player is in specific position and dir
                if (((idx_pos - level_voices.ingame[i].pos).magnitude <= threshold) &&
                     ((get_player_dir_world() == level_voices.ingame[i].dir) || (level_voices.ingame[i].dir == Direction.OTHER)))
                {
                    //if player tapped(played echo)
                    if ((level_voices.ingame[i].tap) && (player_script.tapped_at_this_block()))
                    {
                        ingame_playing = true;
                        //play voice
                        if (level_voices.ingame_cur_clip[i] < level_voices.ingame[i].clips.Count)
                            level_voices.ingame_cur_clip[i] = play_audio(level_voices.ingame[i].clips, level_voices.ingame_cur_clip[i], true);
                        else
                            ingame_playing = false;
                    }
                    else if (!level_voices.ingame[i].tap)
                    {
                        ingame_playing = true;
                        if (level_voices.ingame_cur_clip[i] < level_voices.ingame[i].clips.Count)
                            level_voices.ingame_cur_clip[i] = play_audio(level_voices.ingame[i].clips, level_voices.ingame_cur_clip[i], true);
                        else
                            ingame_playing = false;
                    }
                }
            }
        }

        //play voices that should be played from beginning
        if (!ingame_playing)
        {
            if (level_voices.clip_begin < level_voices.clip_at_begin.Count)
                level_voices.clip_begin = play_audio(level_voices.clip_at_begin, level_voices.clip_begin);
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
            return Direction.FRONT;
        else if ((player_dir - Vector3.down).magnitude <= threshold)
            return Direction.BACK;
        else if ((player_dir - Vector3.left).magnitude <= threshold)
            return Direction.LEFT;
        else if ((player_dir - Vector3.right).magnitude <= threshold)
            return Direction.RIGHT;

        return Direction.OTHER;
    }

    /// <summary>
    /// Deserializes a string representing a direction.
    /// </summary>
    public Direction StringToDir(string str)
    {
        if (str == "FRONT")
            return Direction.FRONT;
        else if (str == "BACK")
            return Direction.BACK;
        else if (str == "LEFT")
            return Direction.LEFT;
        else if (str == "RIGHT")
            return Direction.RIGHT;

        return Direction.OTHER;
    }

    //Sets up the outer walls and floor (background) of the game board.
    void BoardSetup()
    {
        //Instantiate Board and set boardHolder to its transform.
        boardHolder = transform.FindChild("Board");
        for (int i = 0; i < boardHolder.childCount; ++i)
        {
            Destroy(boardHolder.GetChild(i).gameObject);
        }

        float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

        //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
        for (int x = 0; x <= columns + 1; x++)
        {
            //Loop along y axis, starting from -1 to place floor or outerwall tiles.
            for (int y = 0; y <= rows + 1; y++)
            {
                //Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                if (x == 0 || x == columns + 1 || y == 0 || y == rows + 1)
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

                //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                GameObject instance =
                    Instantiate(toInstantiate, gridPositions[y * (rows + 2) + x], Quaternion.identity) as GameObject;
                //Instantiate (toInstantiate, new Vector3 (x*scale, y*scale, 0f), Quaternion.identity) as GameObject;

                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                Vector3 new_scale = instance.transform.localScale;
                new_scale *= scale;
                instance.transform.localScale = new_scale;
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    /* Returns a vector of the location where the exit tile will be placed.
     * Determined randomly from path locations excluding starting position */
    private Vector3 getRandomVector(List<Vector3> positions)
    {
        int index = Random.Range(0, positions.Count);
        return positions[index];
    }

    /*TODO(agotsis) I will rewrite my Python Code in C#, for these purposes.*/
    void setup_level(int level)
    {
        //Clear our list gridPositions.
        wallPositions.Clear();
        wallIdxes.Clear();
        playerPositions.Clear();

        if (level <= 2)
            turning_lock = true;
        else
            turning_lock = false;

        float scale = (float)Utilities.SCALE_REF / (float)Utilities.MAZE_SIZE;

        //clear existing walls and exit
        GameObject wall_parent = transform.FindChild("Walls").gameObject;
        for (int i = 0; i < wall_parent.transform.childCount; ++i)
        {
            Destroy(wall_parent.transform.GetChild(i).gameObject);
        }

        //Determine a random position for the player on the path
        GameObject player = GameObject.Find("Player");

        //return to level 1 if the index is not correct
        if ((level < min_level) || (level > max_level))
            level = min_level;

        //give the right instruction to play
        cur_clip = level;
        cur_level = level;

        //build level
        load_level_from_file("GameData/levels", level);
        int randomDelta = Random.Range(0, playerPositions.Count);
        if (randomDelta == playerPositions.Count)
            randomDelta = playerPositions.Count - 1;

        player.transform.position = playerPositions[randomDelta];
        Vector2 start_idx = get_idx_from_pos(player.transform.position);
        startPos = player.transform.position;

        left_start_pt = false;
        level_voices.init();
        level_voices.clear();
        load_level_voices_from_file("GameData/voices", level_voices, level);
        //load audio
        LoadAudio();
        gamerecord = gamerecord + "s@(" + start_idx.x.ToString() + "," + start_idx.y.ToString() + ")";

        for (int i = 0; i < wallPositions.Count; i++)
        {
            Vector3 position = wallPositions[i];
            //Choose a random tile from tileArray and assign it to tileChoice
            GameObject tileChoice = wallTiles[0];
            //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
            GameObject new_wall = Instantiate(tileChoice, position, Quaternion.identity) as GameObject;
            Vector3 new_scale = new_wall.transform.localScale;
            new_scale *= scale;
            new_wall.transform.localScale = new_scale;
            new_wall.transform.SetParent(wall_parent.transform);
        }
        GameObject new_exit = Instantiate(exit, exitPos, Quaternion.identity) as GameObject;
        new_exit.transform.SetParent(wall_parent.transform);

        //now let the player face the right dir
        mazeSolution = "";
        searched = new bool[(columns + 1) * (rows + 1)];
        for (int i = 0; i < searched.Length; ++i)
            searched[i] = false;

        searched_temp = new bool[(columns + 1) * (rows + 1)];
        for (int i = 0; i < searched_temp.Length; ++i)
            searched_temp[i] = false;

        solveMaze(start_idx, "s");
        player.transform.rotation = Quaternion.identity;
        if (mazeSolution.Length >= 2)
        {
            if (mazeSolution[mazeSolution.Length - 2] == 'u')
                player_script.rotateplayer_no_update(StringToDir("FRONT"));
            if (mazeSolution[mazeSolution.Length - 2] == 'd')
                player_script.rotateplayer_no_update(StringToDir("BACK"));
            if (mazeSolution[mazeSolution.Length - 2] == 'l')
                player_script.rotateplayer_no_update(StringToDir("LEFT"));
            if (mazeSolution[mazeSolution.Length - 2] == 'r')
                player_script.rotateplayer_no_update(StringToDir("RIGHT"));
        }
        startDir = player_script.get_player_dir("FRONT");
    }

    //private help function to replace list.contain()
    bool _searchWallIdxes(Vector2 idx)
    {
        //Debug.Log (idx);
        //Debug.Log ("[" + x.ToString() + " " + y.ToString() + "]");
        float threshhold = 0.1f;

        if ((Mathf.Abs(0 - idx.x) < threshhold) || (Mathf.Abs(0 - idx.y) < threshhold) ||
            (Mathf.Abs((columns + 1) - idx.x) < threshhold) || (Mathf.Abs((rows + 1) - idx.y) < threshhold))
            return true;

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
            return true;

        return false;
    }

    //private helper to get the distance from  wall/edge
    Vector2 _getDist(Vector2 gridIdx, Vector2 dir)
    {
        //Vector2 playerPos = gridIdx;
        //Debug.Log("dir is: " + dir.x.ToString() + ", "+ dir.y.ToString());
        while ((gridIdx.x > 0) && (gridIdx.x < columns + 1) && (gridIdx.y > 0) && (gridIdx.y < rows + 1))
        {
            if (_searchWallIdxes(gridIdx))//the first one we met
                break;
            else
                gridIdx += new Vector2(dir.x, dir.y);
        }
        //now gridIdx is either a wall or a border
        //print("result:");
        //print (gridIdx);
        return gridIdx;
    }

    /// <summary>
    /// Converts a position from Unity coordinates to a grid position.
    /// </summary>
    public Vector2 get_idx_from_pos(Vector3 pos)
    {
        float threshhold = 0.01f;
        int y_idx = -1, x_idx = -1;

        //get which index of gridPos() player is in
        for (int i = 1; i <= rows; i++)
        {
            for (int j = 1; j <= columns; ++j)
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
    /// Computes the fields for the proper echo sound to play.
    /// </summary>
    public echoDistData getEchoDistData(Vector3 playerPos, Vector3 playerFront, Vector3 playerLeft)
    {

        //setup the return value
        echoDistData result = new echoDistData();
        Vector2 gridIdx = get_idx_from_pos(playerPos);
        Vector2 gridTemp;
        Vector2 exitIdx = get_idx_from_pos(exitPos);
        bool check_exit = true;
        result.exitpos = 0;

        gridTemp = _getDist(gridIdx, playerFront);
        result.front = (int)(gridTemp - gridIdx).magnitude;
        result.fType = getJunctionType(gridTemp - new Vector2(playerFront.x, playerFront.y), gridIdx);
        //0: no exit, 1:left, 2:right, 3:front, 4:back
        if (check_exit)
        {
            Vector2 searchIdx = gridIdx;
            while ((searchIdx.x > 0) && (searchIdx.x < columns + 1) && (searchIdx.y > 0) && (searchIdx.y < rows + 1))
            {
                if ((_idx_is_equal(searchIdx, exitIdx)) && ((int)(searchIdx - gridIdx).magnitude) <= result.front)
                {
                    result.exitpos = 3;
                    check_exit = false;
                    break;
                }
                else
                    searchIdx += new Vector2(playerFront.x, playerFront.y);
            }
        }

        gridTemp = _getDist(gridIdx, -playerFront);
        result.back = (int)(gridTemp - gridIdx).magnitude;
        result.bType = getJunctionType(gridTemp + new Vector2(playerFront.x, playerFront.y), gridIdx);
        if (check_exit)
        {
            Vector2 searchIdx = gridIdx;
            while ((searchIdx.x > 0) && (searchIdx.x < columns + 1) && (searchIdx.y > 0) && (searchIdx.y < rows + 1))
            {
                if ((_idx_is_equal(searchIdx, exitIdx)) && ((int)(searchIdx - gridIdx).magnitude) <= result.back)
                {
                    result.exitpos = 4;
                    check_exit = false;
                    break;
                }
                else
                    searchIdx -= new Vector2(playerFront.x, playerFront.y);
            }
        }

        //_debug_print_wallidx ();
        //print ("LEFT!");
        gridTemp = _getDist(gridIdx, playerLeft);
        result.left = (int)(gridTemp - gridIdx).magnitude;
        result.lType = getJunctionType(gridTemp - new Vector2(playerLeft.x, playerLeft.y), gridIdx);
        if (check_exit)
        {
            Vector2 searchIdx = gridIdx;
            while ((searchIdx.x > 0) && (searchIdx.x < columns + 1) && (searchIdx.y > 0) && (searchIdx.y < rows + 1))
            {
                if ((_idx_is_equal(searchIdx, exitIdx)) && ((int)(searchIdx - gridIdx).magnitude) <= result.left)
                {
                    result.exitpos = 1;
                    check_exit = false;
                    break;
                }
                else
                    searchIdx += new Vector2(playerLeft.x, playerLeft.y);
            }
        }

        gridTemp = _getDist(gridIdx, -playerLeft);
        result.right = (int)(gridTemp - gridIdx).magnitude;
        result.rType = getJunctionType(gridTemp + new Vector2(playerLeft.x, playerLeft.y), gridIdx);
        if (check_exit)
        {
            Vector2 searchIdx = gridIdx;
            while ((searchIdx.x > 0) && (searchIdx.x < columns + 1) && (searchIdx.y > 0) && (searchIdx.y < rows + 1))
            {
                if ((_idx_is_equal(searchIdx, exitIdx)) && ((int)(searchIdx - gridIdx).magnitude) <= result.right)
                {
                    result.exitpos = 2;
                    check_exit = false;
                    break;
                }
                else
                    searchIdx -= new Vector2(playerLeft.x, playerLeft.y);
            }
        }

        result.updateDistances();

        return result;
    }

    /// <summary>
    /// Finds the type of junction present at an intersection.
    /// </summary>
    public JunctionType getJunctionType(Vector2 pos, Vector2 entrance)
    {
        if ((pos.x <= 0) || (pos.x >= columns + 1) || (pos.y <= 0) || (pos.y >= rows + 1))
            return JunctionType.INVALID;

        float threshhold = 0.1f;

        if ((pos - entrance).magnitude <= threshhold)
            return JunctionType.DEADEND;

        int path_count = 0;
        bool entranceX = true;
        if (Mathf.Abs(pos.x - entrance.x) < threshhold)
            entranceX = false;
        bool xrf = false, xlf = false, ytf = false, ydf = false;

        if (!_searchWallIdxes(new Vector2(pos.x + 1, pos.y)))
        {
            xrf = true;
            path_count += 1;
        }
        if (!_searchWallIdxes(new Vector2(pos.x - 1, pos.y)))
        {
            xlf = true;
            path_count += 1;
        }
        if (!_searchWallIdxes(new Vector2(pos.x, pos.y + 1)))
        {
            ytf = true;
            path_count += 1;
        }
        if (!_searchWallIdxes(new Vector2(pos.x, pos.y - 1)))
        {
            ydf = true;
            path_count += 1;
        }

        if (path_count <= 1)
            return JunctionType.DEADEND;
        else if (path_count == 3)
            return JunctionType.T;
        else if (path_count == 4)//not possible, but I leave it here
            return JunctionType.CROSS;
        else if (path_count == 2)
        {
            if ((xrf == ytf) && entranceX)
                return JunctionType.RL;
            else if ((xrf == ytf) && !entranceX)
                return JunctionType.LL;
            else if ((xrf != ytf) && entranceX)
                return JunctionType.LL;
            else if ((xrf != ytf) && !entranceX)
                return JunctionType.RL;
        }

        return JunctionType.INVALID;
    }

    void _debug_print_wallidx()
    {
        string toPrint = "";
        for (int i = 0; i < wallIdxes.Count; i += 2)
        {
            toPrint += "[" + (wallIdxes[i]).ToString();
            toPrint += " " + (wallIdxes[i + 1]).ToString();
            toPrint += "] ";
        }
        Logging.Log(toPrint, Logging.LogLevel.VERBOSE);
    }

    //RandomPosition returns a random position from our list gridPositions.
    //WARNING: get rid of the position chosen from the list, be very careful when calling it
    Vector3 RandomPosition()
    {
        //Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
        int randomIndex = Random.Range(0, gridPositions.Count);

        //Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
        Vector3 randomPosition = gridPositions[randomIndex];

        //Remove the entry at randomIndex from the list so that it can't be re-used.
        gridPositions.RemoveAt(randomIndex);

        //Return the randomly selected Vector3 position.
        return randomPosition;
    }


    //LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        //Choose a random number of objects to instantiate within the minimum and maximum limits
        int objectCount = Random.Range(minimum, maximum + 1);

        //Instantiate objects until the randomly chosen limit objectCount is reached
        for (int i = 0; i < objectCount; i++)
        {
            //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
            Vector3 randomPosition = RandomPosition();

            //Choose a random tile from tileArray and assign it to tileChoice
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

            //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }


    /// <summary>
    /// Sets up the game board (grid positions), sound clips and save data as an initialization step. 
    /// </summary>
    public void SetupScene(int level)
    {
        //find player
        player_ref = GameObject.Find("Player");//Player.instance.gameObject;
        player_script = player_ref.GetComponent<Player>();
        //SoundManager.instance.PlaySingle (clips[cur_clip]);
        //Reset our list of gridpositions.
        InitialiseList();
        //Creates the outer walls and floor.
        BoardSetup();
        LoadLoaclStats();
        setup_level(level);
        //if( (GameMode.instance.get_mode() == GameMode.Game_Mode.MAIN)||
        //	(GameMode.instance.get_mode() == GameMode.Game_Mode.CONTINUE) )
        write_save(level);

        //local_stats[level] += 1;
        //write_local_stats ();

        //setup clip history list
        //latest_clips = new List<AudioClip>();
        //latest_clips.Clear();
        latest_clip_idx = 0;
        skip_clip = false;
    }

    /// <summary>
    /// Saves information about the game state as persistent data.
    /// </summary>
    public bool write_save(int lv)
    {
        string filename = "";

        if (GameMode.instance.get_mode() != GameMode.Game_Mode.TUTORIAL)
            filename = Application.persistentDataPath + "echosaved";
        else//load specific save for tutorial
            filename = Application.persistentDataPath + "echosaved_tutorial";

        System.IO.File.WriteAllText(filename, lv.ToString());
        return true;
    }

    string _InttoString(int tochange)
    {
        return tochange.ToString();
    }

    /// <summary>
    /// Writes some game statistics into persistent data.
    /// </summary>
    public bool write_local_stats()
    {
        string filename = Application.persistentDataPath + "echostats";
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

    bool LoadLoaclStats()
    {
        string filename = Application.persistentDataPath + "echostats";
        local_stats = new int[max_total_level + 1];
        //for(int i = 0; i < local_stats.Length; ++i)
        //	local_stats[i] = 0;
        string[] svdata_split;
        if (System.IO.File.Exists(filename))
        {
            svdata_split = System.IO.File.ReadAllLines(filename);
            local_stats = Array.ConvertAll<string, int>(svdata_split, int.Parse);
        }
        else
        {
            for (int i = 0; i < local_stats.Length; ++i)
                local_stats[i] = 0;

            return false;
        }

        return true;
    }

    public bool load_level_voices_from_file(string filename, level_voice_list data, int level_wanted = 1)
    {
        TextAsset lvldata = Resources.Load(filename) as TextAsset;
        if (lvldata == null)
        {
            UnityEngine.Debug.Log("Cannot open file at:");
            UnityEngine.Debug.Log(filename);
            return false;
        }
        string[] lvldata_split = lvldata.text.Split('\n');
        bool reading_level = false;
        bool reading_begin = false;//is it reading the playlist at the start of new game?
        bool reading_exit = false;
        bool reading_ingame = false;
        bool reading_ingame_data = false;
        bool reading_return = false;
        pos_and_action ingame_data = new pos_and_action();

        //read through the file until desired level is found
        foreach (string line in lvldata_split)
        {
            if (line.Length >= 3)
            {
                if (line.Substring(0, 3) == "END")
                { //reach end of a level layout
                    reading_level = false;
                }
            }

            if (reading_level)
            {//actually loading layout
                if (line.Substring(0, 2) == "IN")
                {
                    reading_ingame = true;
                    reading_begin = false;
                    reading_exit = false;
                    reading_ingame_data = false;
                    reading_return = false;
                }
                else if (line.Substring(0, 4) == "Exit")
                {
                    reading_exit = true;
                    reading_ingame = false;
                    reading_begin = false;
                    reading_ingame_data = false;
                    reading_return = false;
                }
                else if (line.Substring(0, 5) == "Begin")
                {
                    reading_begin = true;
                    reading_ingame = false;
                    reading_exit = false;
                    reading_ingame_data = false;
                    reading_return = false;
                }
                else if (line.Substring(0, 5) == "Inend")
                {
                    reading_ingame_data = false;
                }
                else if (line.Substring(0, 6) == "RETURN")
                {
                    reading_begin = false;
                    reading_ingame = false;
                    reading_exit = false;
                    reading_ingame_data = false;
                    reading_return = true;
                }
                else
                {
                    if (reading_begin)
                        data.play_at_begin.Add(line);
                    else if (reading_ingame)
                    {
                        reading_ingame = false;
                        reading_ingame_data = true;
                        ingame_data = new pos_and_action();
                        ingame_data.init();
                        Vector2 start_idx = get_idx_from_pos(player_ref.transform.position);
                        float ingame_x, ingame_y;
                        if (line.Substring(3, 1) == "s")
                            ingame_x = start_idx.x;
                        else if (line.Substring(3, 1) == "n")
                            ingame_x = start_idx.x + 1f;
                        else
                            ingame_x = float.Parse(line.Substring(3, 1));

                        if (line.Substring(5, 1) == "s")
                            ingame_y = start_idx.y;
                        else if (line.Substring(5, 1) == "n")
                            ingame_y = start_idx.y + 1f;
                        else
                            ingame_y = float.Parse(line.Substring(5, 1));

                        ingame_data.pos = new Vector2(ingame_x, ingame_y);
                        if (line.Substring(8, 1) == "T")
                            ingame_data.once = true;
                        else
                            ingame_data.once = false;

                        if (line.Substring(10, 1) == "T")
                            ingame_data.tap = true;
                        else
                            ingame_data.tap = false;

                        ingame_data.dir = StringToDir(line.Substring(12, line.Length - 13));
                        data.ingame.Add(ingame_data);
                        data.ingame_cur_clip.Add(0);
                    }
                    else if (reading_ingame_data)
                    {
                        ingame_data.clip_names.Add(line);
                    }
                    else if (reading_exit)
                    {
                        data.play_at_exit.Add(line);
                    }
                    else if (reading_return)
                    {
                        data.play_when_return.Add(line);
                    }
                }
            }

            //flow control
            int level_reading = -1;
            if (line.Length >= 7)
            {
                if (line.Substring(0, 6) == "LEVEL_")
                {
                    //get the current level we are reading
                    int remain_length = line.Length - 6;
                    if (line.Substring(6, remain_length - 1) != "DEFAULT")
                    {
                        level_reading = Int32.Parse(line.Substring(6, remain_length));
                        if (level_reading == level_wanted)
                        {//we found the level we want
                            reading_level = true;
                        }
                        else if (level_reading > level_wanted)
                        {
                            reading_level = false;
                            return true;
                        }
                    }
                    else if (level_reading != level_wanted)
                    {
                        reading_level = true;
                    }
                }
            }
        }

        return true;
    }

    public bool load_level_from_file(string filename, int level_wanted = 1)
    {
        TextAsset lvldata = Resources.Load(filename) as TextAsset;
        if (lvldata == null)
        {
            UnityEngine.Debug.Log("Cannot open file at:");
            UnityEngine.Debug.Log(filename);
            return false;
        }
        string[] lvldata_split = lvldata.text.Split('\n');
        bool reading_level = false;
        int cur_y = Utilities.MAZE_SIZE - 1;//start from top left corner

        asciiLevelRep = "";
        gamerecord = "";

        //read through the file until desired level is found
        foreach (string line in lvldata_split)
        {
            if (line.Substring(0, 3) == "END")
            { //reach end of a level layout
                reading_level = false;
            }

            if (reading_level)
            {//actually loading layout
                //check for valid index
                asciiLevelRep += line;
                if (cur_y >= 0)
                {
                    //do things
                    for (int i = 0; i < line.Length; ++i)
                    {
                        if (line[i] == 'w')
                        {//wall
                            wallPositions.Add(gridPositions[(cur_y + 1) * (columns + 2) + (i + 1)]);
                            wallIdxes.Add(i + 1);
                            wallIdxes.Add(cur_y + 1);
                        }
                        else if (line[i] == 'e')//exit
                            exitPos = gridPositions[(cur_y + 1) * (columns + 2) + (i + 1)];
                        else if (line[i] == 's')//start positions
                            playerPositions.Add(gridPositions[(cur_y + 1) * (columns + 2) + (i + 1)]);
                    }
                    cur_y -= 1;
                }
            }

            //flow control
            if (line.Length >= 7)
            {
                if (line.Substring(0, 6) == "LEVEL_")
                {
                    //get the current level we are reading
                    int remain_length = line.Length - 6;
                    if (line.Substring(6, remain_length - 1) != "DEFAULT")
                    {
                        int level_reading = Int32.Parse(line.Substring(6, remain_length));
                        if (level_reading == level_wanted)
                        {//we found the level we want
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

    public int get_level_count(string filename)
    {
        TextAsset lvldata = Resources.Load(filename) as TextAsset;
        if (lvldata == null)
        {
            UnityEngine.Debug.Log("Cannot open file at:");
            UnityEngine.Debug.Log(filename);
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
                    //get the current level we are reading
                    level_count += 1;
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
        if ((idx.x > columns) || (idx.x < 1) || (idx.y > rows) || (idx.y < 1))//just in case, so I widen the range
            return false;

        if (searched[(int)((columns + 1) * idx.y + idx.x)])
            return false;
        searched[(int)((columns + 1) * idx.y + idx.x)] = true;

        if (_searchWallIdxes(idx))
            return false;

        if (get_idx_from_pos(exitPos) == idx)
        {
            mazeSolution += dir;
            return true;
        }

        bool result = false || solveMaze(new Vector2(idx.x, idx.y + 1f), "u") || solveMaze(new Vector2(idx.x, idx.y - 1f), "d") ||
                               solveMaze(new Vector2(idx.x + 1f, idx.y), "r") || solveMaze(new Vector2(idx.x - 1f, idx.y), "l");

        if (result)
            mazeSolution += dir;

        return result;
    }

    //solve maze during gameplay
    public string sol;
    public bool[] searched_temp;

    /// <summary>
    /// Solves the maze with a depth-first search.
    /// </summary>
    /// FIXME: This is code duplication.
    public bool solveMazeMid(Vector2 idx, string dir)
    {
        if ((idx.x > columns) || (idx.x < 1) || (idx.y > rows) || (idx.y < 1))//just in case, so I widen the range
            return false;

        if (searched_temp[(int)((columns + 1) * idx.y + idx.x)])
            return false;
        searched_temp[(int)((columns + 1) * idx.y + idx.x)] = true;

        if (_searchWallIdxes(idx))
            return false;

        if (get_idx_from_pos(exitPos) == idx)
        {
            sol += dir;
            return true;
        }

        bool result = false || solveMazeMid(new Vector2(idx.x, idx.y + 1f), "u") || solveMazeMid(new Vector2(idx.x, idx.y - 1f), "d") ||
            solveMazeMid(new Vector2(idx.x + 1f, idx.y), "r") || solveMazeMid(new Vector2(idx.x - 1f, idx.y), "l");

        if (result)
            sol += dir;

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
            result = sol[GameManager.instance.boardScript.sol.Length - 2].ToString();

        return result;
    }
}