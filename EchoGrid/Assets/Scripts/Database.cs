using UnityEngine;
using System.Collections;

/// <summary>
/// A class containing a variety of resources loaded in the game to use.
///  It mostly contains audio clips of various sounds.
/// </summary>
public class Database : MonoBehaviour
{

    public static Database instance;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    // audios
    // Single click echo
    [HideInInspector]
    public static AudioClip attenuatedClick;
    public static AudioClip attenuatedaround;
    public static AudioClip attenuatedClickfront;

    public static AudioClip attenuatedaround_odeon;
    public static AudioClip attenuatedClickfront_odeon;

    // Game environment settings clips
    [HideInInspector]
    public static AudioClip[] settingsClips = new AudioClip[10];

    // Main menu clips
    [HideInInspector]
    public static AudioClip[] mainMenuClips = new AudioClip[24];

    // Pregame menu clips
    [HideInInspector]
    public static AudioClip[] preGameMenuClips = new AudioClip[15];

    // Gesture tutorial clips
    [HideInInspector]
    public static AudioClip[] tutorialClips = new AudioClip[30];

    // Main game clips
    [HideInInspector]
    public static AudioClip[] mainGameClips = new AudioClip[39];
    [HideInInspector]
    public static AudioClip[] levelStartClips = new AudioClip[150];
    [HideInInspector]
    public static AudioClip[] pauseMenuClips = new AudioClip[12];
    [HideInInspector]
    public static AudioClip[] hintClips = new AudioClip[4];

    // Error clips
    [HideInInspector]
    public static AudioClip[] errorClips = new AudioClip[20];

    // Sound effect clips
    [HideInInspector]
    public static AudioClip[] soundEffectClips = new AudioClip[10];

    // strings
    // Title Screen
    public static string titleText_main = "\n\nto contact the app developers \nor the research lab, \nemail auditory@andrew.cmu.edu";

    // T&C screen
    public static string tcText_main = "T & C\nSwipe Left to Continue";
    public static string tcmsg = "Please hold your phone horizontally for this game, \n " + "and please read the online consent form; \n " + "after finish, you can click back button to " + "return to the game";

    // Main

    void LoadData()
    {
        //Option1:
        attenuatedClickfront_odeon = Resources.Load("fx/00-0_F-6.75-D_B-w-D_L-w-D_R-w-D_fadeout") as AudioClip;
        attenuatedClick = Resources.Load("fx/attenuatedClick") as AudioClip;
        attenuatedaround_odeon = Resources.Load("fx/00-0_F-0.75-D_B-w-D_L-w-D_R-w-D_fadeout") as AudioClip;

        //Option1:
        attenuatedClickfront = Resources.Load("fx/attenuatedClick") as AudioClip;
        attenuatedaround = Resources.Load("fx/attenuatedClick") as AudioClip;

        // Game environment setting clips
        settingsClips[0] = Resources.Load("instructions/orient_phone") as AudioClip;
        settingsClips[1] = Resources.Load("instructions/listen_to_instructions") as AudioClip;
        settingsClips[2] = Resources.Load("instructions/using_talkback") as AudioClip;
        settingsClips[3] = Resources.Load("instructions/add_talkback_hold") as AudioClip;
        settingsClips[4] = Resources.Load("instructions/use_three_fingers_normal") as AudioClip;
        settingsClips[5] = Resources.Load("instructions/use_three_fingers_talkback") as AudioClip;        
        settingsClips[6] = Resources.Load("instructions/headphones_normal") as AudioClip;
        settingsClips[7] = Resources.Load("instructions/headphones_talkback") as AudioClip;        
        settingsClips[8] = Resources.Load("instructions/environment_setup_normal") as AudioClip;
        settingsClips[9] = Resources.Load("instructions/environment_setup_talkback") as AudioClip;

        // Main menu clips
        mainMenuClips[0] = Resources.Load("instructions/welcome_to_echo_adventure") as AudioClip;
        mainMenuClips[1] = Resources.Load("instructions/main_menu_swipe_right_normal") as AudioClip;
        mainMenuClips[2] = Resources.Load("instructions/main_menu_swipe_right_talkback") as AudioClip;
        mainMenuClips[3] = Resources.Load("instructions/main_menu_swipe_left_normal") as AudioClip;
        mainMenuClips[4] = Resources.Load("instructions/main_menu_swipe_left_talkback") as AudioClip;
        mainMenuClips[5] = Resources.Load("instructions/main_menu_swipe_up_normal") as AudioClip;
        mainMenuClips[6] = Resources.Load("instructions/main_menu_swipe_up_talkback") as AudioClip;
        mainMenuClips[7] = Resources.Load("instructions/main_menu_swipe_down_normal") as AudioClip;
        mainMenuClips[8] = Resources.Load("instructions/main_menu_swipe_down_talkback") as AudioClip;
        mainMenuClips[9] = Resources.Load("instructions/options_menu_normal") as AudioClip;
        mainMenuClips[10] = Resources.Load("instructions/options_menu_talkback") as AudioClip;
        mainMenuClips[11] = Resources.Load("instructions/options_menu_normal") as AudioClip;
        mainMenuClips[12] = Resources.Load("instructions/using_hrtf_echoes") as AudioClip;
        mainMenuClips[13] = Resources.Load("instructions/using_odeon_echoes") as AudioClip;
        mainMenuClips[14] = Resources.Load("instructions/command_list_swipe_up_normal") as AudioClip;
        mainMenuClips[15] = Resources.Load("instructions/command_list_swipe_up_talkback") as AudioClip;
        mainMenuClips[16] = Resources.Load("instructions/command_list_tap_normal") as AudioClip;
        mainMenuClips[17] = Resources.Load("instructions/command_list_tap_talkback") as AudioClip;
        mainMenuClips[18] = Resources.Load("instructions/command_list_swipe_down_normal") as AudioClip;
        mainMenuClips[19] = Resources.Load("instructions/command_list_swipe_down_talkback") as AudioClip;
        mainMenuClips[20] = Resources.Load("instructions/command_list_rotation_normal") as AudioClip;
        mainMenuClips[21] = Resources.Load("instructions/command_list_rotation_talkback") as AudioClip;
        mainMenuClips[22] = Resources.Load("instructions/command_list_hold_normal") as AudioClip;
        mainMenuClips[23] = Resources.Load("instructions/command_list_hold_talkback") as AudioClip;

        // Pregame menu clips
        preGameMenuClips[0] = Resources.Load("instructions/pregame_tutorial_swipe_left_normal") as AudioClip;
        preGameMenuClips[1] = Resources.Load("instructions/pregame_tutorial_swipe_left_talkback") as AudioClip;
        preGameMenuClips[2] = Resources.Load("instructions/pregame_tutorial_swipe_right_normal") as AudioClip;
        preGameMenuClips[3] = Resources.Load("instructions/pregame_tutorial_swipe_right_talkback") as AudioClip;
        preGameMenuClips[4] = Resources.Load("instructions/pregame_maingame_swipe_left_normal") as AudioClip;
        preGameMenuClips[5] = Resources.Load("instructions/pregame_maingame_swipe_left_talkback") as AudioClip;
        preGameMenuClips[6] = Resources.Load("instructions/pregame_maingame_swipe_right_normal") as AudioClip;
        preGameMenuClips[7] = Resources.Load("instructions/pregame_maingame_swipe_right_talkback") as AudioClip;
        preGameMenuClips[8] = Resources.Load("instructions/pregame_swipe_down_normal") as AudioClip;
        preGameMenuClips[9] = Resources.Load("instructions/pregame_swipe_down_talkback") as AudioClip;
        preGameMenuClips[10] = Resources.Load("instructions/overwrite_saves") as AudioClip;
        preGameMenuClips[11] = Resources.Load("instructions/pregame_confirm_normal") as AudioClip;
        preGameMenuClips[12] = Resources.Load("instructions/pregame_confirm_talkback") as AudioClip;
        preGameMenuClips[13] = Resources.Load("instructions/new_game_started") as AudioClip;
        preGameMenuClips[14] = Resources.Load("instructions/loaded_saved_game") as AudioClip;

        // Gesture tutorial clips      
        tutorialClips[0] = Resources.Load("instructions/tutorial_reminder") as AudioClip;
        tutorialClips[1] = Resources.Load("instructions/tap_instruction_normal") as AudioClip;
        tutorialClips[2] = Resources.Load("instructions/tap_instruction_talkback") as AudioClip;
        tutorialClips[3] = Resources.Load("instructions/sound_will_change") as AudioClip;
        tutorialClips[4] = Resources.Load("instructions/tap_three_times") as AudioClip;
        tutorialClips[5] = Resources.Load("instructions/tap_correct_2_more") as AudioClip;
        tutorialClips[6] = Resources.Load("instructions/tap_correct_1_more") as AudioClip;
        tutorialClips[7] = Resources.Load("instructions/finished_tap_section") as AudioClip;
        tutorialClips[8] = Resources.Load("instructions/swipe_up_instruction_normal") as AudioClip;
        tutorialClips[9] = Resources.Load("instructions/swipe_up_instruction_talkback") as AudioClip;
        tutorialClips[10] = Resources.Load("instructions/swipe_three_times") as AudioClip;
        tutorialClips[11] = Resources.Load("instructions/swipe_correct_2_more") as AudioClip;
        tutorialClips[12] = Resources.Load("instructions/swipe_correct_1_more") as AudioClip;
        tutorialClips[13] = Resources.Load("instructions/finished_swipe_section") as AudioClip;
        tutorialClips[14] = Resources.Load("instructions/pause_menu_instruction_normal") as AudioClip;
        tutorialClips[15] = Resources.Load("instructions/pause_menu_instruction_talkback") as AudioClip;
        tutorialClips[16] = Resources.Load("instructions/pause_menu_explanation_normal") as AudioClip;
        tutorialClips[17] = Resources.Load("instructions/pause_menu_explanation_talkback") as AudioClip;
        tutorialClips[18] = Resources.Load("instructions/exit_level_instruction_normal") as AudioClip;
        tutorialClips[19] = Resources.Load("instructions/exit_level_instruction_talkback") as AudioClip;
        tutorialClips[20] = Resources.Load("instructions/try_swipe_down") as AudioClip;
        tutorialClips[21] = Resources.Load("instructions/reached_right_corner") as AudioClip;
        tutorialClips[22] = Resources.Load("instructions/rotate_instruction_normal") as AudioClip;
        tutorialClips[23] = Resources.Load("instructions/rotate_instruction_talkback") as AudioClip;
        tutorialClips[24] = Resources.Load("instructions/tap_after_rotating") as AudioClip;
        tutorialClips[25] = Resources.Load("instructions/rotate_4_times") as AudioClip;
        tutorialClips[26] = Resources.Load("instructions/rotation_correct_3_more") as AudioClip;
        tutorialClips[27] = Resources.Load("instructions/rotation_correct_2_more") as AudioClip;
        tutorialClips[28] = Resources.Load("instructions/rotation_correct_1_more") as AudioClip;
        tutorialClips[29] = Resources.Load("instructions/get_around_corner") as AudioClip;

        // Main game clips
        mainGameClips[0] = Resources.Load("instructions/level_start") as AudioClip;
        mainGameClips[1] = Resources.Load("instructions/welcome_you_are_in_a_dark_maze") as AudioClip;
        mainGameClips[2] = Resources.Load("instructions/currently_in_straight_hallway") as AudioClip;
        mainGameClips[3] = Resources.Load("instructions/stairs_at_end_of_hallway") as AudioClip;
        mainGameClips[4] = Resources.Load("instructions/if_you_hit_a_wall") as AudioClip;
        mainGameClips[5] = Resources.Load("instructions/wall_hitting") as AudioClip;
        mainGameClips[6] = Resources.Load("instructions/echolocation_simulation") as AudioClip;
        mainGameClips[7] = Resources.Load("instructions/tap_before_step") as AudioClip;
        mainGameClips[8] = Resources.Load("instructions/swipe_up_to_move_forward") as AudioClip;
        mainGameClips[9] = Resources.Load("instructions/get_to_stairs_without_crashing") as AudioClip;
        mainGameClips[10] = Resources.Load("instructions/proceed_by_tapping") as AudioClip;
        mainGameClips[11] = Resources.Load("instructions/tap_at_exit") as AudioClip;
        mainGameClips[12] = Resources.Load("instructions/another_straight_hallway") as AudioClip;
        mainGameClips[13] = Resources.Load("instructions/swipe_down_to_attempt_exit") as AudioClip;
        mainGameClips[14] = Resources.Load("instructions/move_on_from_straight_hallways") as AudioClip;
        mainGameClips[15] = Resources.Load("instructions/this_is_level_3") as AudioClip;
        mainGameClips[16] = Resources.Load("instructions/approaching_corner") as AudioClip;
        mainGameClips[17] = Resources.Load("instructions/move_forward_until_stairs") as AudioClip;
        mainGameClips[18] = Resources.Load("instructions/good_job") as AudioClip;
        mainGameClips[19] = Resources.Load("instructions/hallway_has_another_right_turn") as AudioClip;
        mainGameClips[20] = Resources.Load("instructions/hallway_has_left_turn") as AudioClip;
        mainGameClips[21] = Resources.Load("instructions/reached_left_corner") as AudioClip;
        mainGameClips[22] = Resources.Load("instructions/you_have_turned_left") as AudioClip;
        mainGameClips[23] = Resources.Load("instructions/finished_first_part_of_tutorial") as AudioClip;
        mainGameClips[24] = Resources.Load("instructions/some_harder_levels") as AudioClip;
        mainGameClips[25] = Resources.Load("instructions/introduce_T_hallway") as AudioClip;
        mainGameClips[26] = Resources.Load("instructions/stairs_in_one_of_two_arms_of_T") as AudioClip;
        mainGameClips[27] = Resources.Load("instructions/turn_around_in_wrong_arm") as AudioClip;
        mainGameClips[28] = Resources.Load("instructions/in_T_fork") as AudioClip;
        mainGameClips[29] = Resources.Load("instructions/completed_all_of_tutorial") as AudioClip;
        mainGameClips[30] = Resources.Load("instructions/back_at_entrance_turn_around") as AudioClip;
        mainGameClips[31] = Resources.Load("instructions/good") as AudioClip;
        mainGameClips[32] = Resources.Load("instructions/good_progress") as AudioClip;
        mainGameClips[33] = Resources.Load("instructions/halfway_there") as AudioClip;
        mainGameClips[34] = Resources.Load("instructions/keep_moving_forward") as AudioClip;
        mainGameClips[35] = Resources.Load("instructions/not_the_exit") as AudioClip;
        mainGameClips[36] = Resources.Load("instructions/crashed_at_exit") as AudioClip;
        mainGameClips[37] = Resources.Load("instructions/crashed_right_corner") as AudioClip;
        mainGameClips[38] = Resources.Load("instructions/crashed_left_corner") as AudioClip;

        levelStartClips[0] = Resources.Load("instructions/1") as AudioClip;
        levelStartClips[1] = Resources.Load("instructions/2") as AudioClip;
        levelStartClips[2] = Resources.Load("instructions/3") as AudioClip;
        levelStartClips[3] = Resources.Load("instructions/4") as AudioClip;
        levelStartClips[4] = Resources.Load("instructions/5") as AudioClip;
        levelStartClips[5] = Resources.Load("instructions/6") as AudioClip;
        levelStartClips[6] = Resources.Load("instructions/7") as AudioClip;
        levelStartClips[7] = Resources.Load("instructions/8") as AudioClip;
        levelStartClips[8] = Resources.Load("instructions/9") as AudioClip;
        levelStartClips[9] = Resources.Load("instructions/10") as AudioClip;
        levelStartClips[10] = Resources.Load("instructions/11") as AudioClip;
        levelStartClips[11] = Resources.Load("instructions/12") as AudioClip;
        levelStartClips[12] = Resources.Load("instructions/13") as AudioClip;
        levelStartClips[13] = Resources.Load("instructions/14") as AudioClip;
        levelStartClips[14] = Resources.Load("instructions/15") as AudioClip;
        levelStartClips[15] = Resources.Load("instructions/16") as AudioClip;
        levelStartClips[16] = Resources.Load("instructions/17") as AudioClip;
        levelStartClips[17] = Resources.Load("instructions/18") as AudioClip;
        levelStartClips[18] = Resources.Load("instructions/19") as AudioClip;
        levelStartClips[19] = Resources.Load("instructions/20") as AudioClip;
        levelStartClips[20] = Resources.Load("instructions/21") as AudioClip;
        levelStartClips[21] = Resources.Load("instructions/22") as AudioClip;
        levelStartClips[22] = Resources.Load("instructions/23") as AudioClip;
        levelStartClips[23] = Resources.Load("instructions/24") as AudioClip;
        levelStartClips[24] = Resources.Load("instructions/25") as AudioClip;
        levelStartClips[25] = Resources.Load("instructions/26") as AudioClip;
        levelStartClips[26] = Resources.Load("instructions/27") as AudioClip;
        levelStartClips[27] = Resources.Load("instructions/28") as AudioClip;
        levelStartClips[28] = Resources.Load("instructions/29") as AudioClip;
        levelStartClips[29] = Resources.Load("instructions/30") as AudioClip;
        levelStartClips[30] = Resources.Load("instructions/31") as AudioClip;
        levelStartClips[31] = Resources.Load("instructions/32") as AudioClip;
        levelStartClips[32] = Resources.Load("instructions/33") as AudioClip;
        levelStartClips[33] = Resources.Load("instructions/34") as AudioClip;
        levelStartClips[34] = Resources.Load("instructions/35") as AudioClip;
        levelStartClips[35] = Resources.Load("instructions/36") as AudioClip;
        levelStartClips[36] = Resources.Load("instructions/37") as AudioClip;
        levelStartClips[37] = Resources.Load("instructions/38") as AudioClip;
        levelStartClips[38] = Resources.Load("instructions/39") as AudioClip;
        levelStartClips[39] = Resources.Load("instructions/40") as AudioClip;
        levelStartClips[40] = Resources.Load("instructions/41") as AudioClip;
        levelStartClips[41] = Resources.Load("instructions/42") as AudioClip;
        levelStartClips[42] = Resources.Load("instructions/43") as AudioClip;
        levelStartClips[43] = Resources.Load("instructions/44") as AudioClip;
        levelStartClips[44] = Resources.Load("instructions/45") as AudioClip;
        levelStartClips[45] = Resources.Load("instructions/46") as AudioClip;
        levelStartClips[46] = Resources.Load("instructions/47") as AudioClip;
        levelStartClips[47] = Resources.Load("instructions/48") as AudioClip;
        levelStartClips[48] = Resources.Load("instructions/49") as AudioClip;
        levelStartClips[49] = Resources.Load("instructions/50") as AudioClip;
        levelStartClips[50] = Resources.Load("instructions/51") as AudioClip;
        levelStartClips[51] = Resources.Load("instructions/52") as AudioClip;
        levelStartClips[52] = Resources.Load("instructions/53") as AudioClip;
        levelStartClips[53] = Resources.Load("instructions/54") as AudioClip;
        levelStartClips[54] = Resources.Load("instructions/55") as AudioClip;
        levelStartClips[55] = Resources.Load("instructions/56") as AudioClip;
        levelStartClips[56] = Resources.Load("instructions/57") as AudioClip;
        levelStartClips[57] = Resources.Load("instructions/58") as AudioClip;
        levelStartClips[58] = Resources.Load("instructions/59") as AudioClip;
        levelStartClips[59] = Resources.Load("instructions/60") as AudioClip;
        levelStartClips[60] = Resources.Load("instructions/61") as AudioClip;
        levelStartClips[61] = Resources.Load("instructions/62") as AudioClip;
        levelStartClips[62] = Resources.Load("instructions/63") as AudioClip;
        levelStartClips[63] = Resources.Load("instructions/64") as AudioClip;
        levelStartClips[64] = Resources.Load("instructions/65") as AudioClip;
        levelStartClips[65] = Resources.Load("instructions/66") as AudioClip;
        levelStartClips[66] = Resources.Load("instructions/67") as AudioClip;
        levelStartClips[67] = Resources.Load("instructions/68") as AudioClip;
        levelStartClips[68] = Resources.Load("instructions/69") as AudioClip;
        levelStartClips[69] = Resources.Load("instructions/70") as AudioClip;
        levelStartClips[70] = Resources.Load("instructions/71") as AudioClip;
        levelStartClips[71] = Resources.Load("instructions/72") as AudioClip;
        levelStartClips[72] = Resources.Load("instructions/73") as AudioClip;
        levelStartClips[73] = Resources.Load("instructions/74") as AudioClip;
        levelStartClips[74] = Resources.Load("instructions/75") as AudioClip;
        levelStartClips[75] = Resources.Load("instructions/76") as AudioClip;
        levelStartClips[76] = Resources.Load("instructions/77") as AudioClip;
        levelStartClips[77] = Resources.Load("instructions/78") as AudioClip;
        levelStartClips[78] = Resources.Load("instructions/79") as AudioClip;
        levelStartClips[79] = Resources.Load("instructions/80") as AudioClip;
        levelStartClips[80] = Resources.Load("instructions/81") as AudioClip;
        levelStartClips[81] = Resources.Load("instructions/82") as AudioClip;
        levelStartClips[82] = Resources.Load("instructions/83") as AudioClip;
        levelStartClips[83] = Resources.Load("instructions/84") as AudioClip;
        levelStartClips[84] = Resources.Load("instructions/85") as AudioClip;
        levelStartClips[85] = Resources.Load("instructions/86") as AudioClip;
        levelStartClips[86] = Resources.Load("instructions/87") as AudioClip;
        levelStartClips[87] = Resources.Load("instructions/88") as AudioClip;
        levelStartClips[88] = Resources.Load("instructions/89") as AudioClip;
        levelStartClips[89] = Resources.Load("instructions/90") as AudioClip;
        levelStartClips[90] = Resources.Load("instructions/91") as AudioClip;
        levelStartClips[91] = Resources.Load("instructions/92") as AudioClip;
        levelStartClips[92] = Resources.Load("instructions/93") as AudioClip;
        levelStartClips[93] = Resources.Load("instructions/94") as AudioClip;
        levelStartClips[94] = Resources.Load("instructions/95") as AudioClip;
        levelStartClips[95] = Resources.Load("instructions/96") as AudioClip;
        levelStartClips[96] = Resources.Load("instructions/97") as AudioClip;
        levelStartClips[97] = Resources.Load("instructions/98") as AudioClip;
        levelStartClips[98] = Resources.Load("instructions/99") as AudioClip;
        levelStartClips[99] = Resources.Load("instructions/100") as AudioClip;
        levelStartClips[100] = Resources.Load("instructions/101") as AudioClip;
        levelStartClips[101] = Resources.Load("instructions/102") as AudioClip;
        levelStartClips[102] = Resources.Load("instructions/103") as AudioClip;
        levelStartClips[103] = Resources.Load("instructions/104") as AudioClip;
        levelStartClips[104] = Resources.Load("instructions/105") as AudioClip;
        levelStartClips[105] = Resources.Load("instructions/106") as AudioClip;
        levelStartClips[106] = Resources.Load("instructions/107") as AudioClip;
        levelStartClips[107] = Resources.Load("instructions/108") as AudioClip;
        levelStartClips[108] = Resources.Load("instructions/109") as AudioClip;
        levelStartClips[109] = Resources.Load("instructions/110") as AudioClip;
        levelStartClips[110] = Resources.Load("instructions/111") as AudioClip;
        levelStartClips[111] = Resources.Load("instructions/112") as AudioClip;
        levelStartClips[112] = Resources.Load("instructions/113") as AudioClip;
        levelStartClips[113] = Resources.Load("instructions/114") as AudioClip;
        levelStartClips[114] = Resources.Load("instructions/115") as AudioClip;
        levelStartClips[115] = Resources.Load("instructions/116") as AudioClip;
        levelStartClips[116] = Resources.Load("instructions/117") as AudioClip;
        levelStartClips[117] = Resources.Load("instructions/118") as AudioClip;
        levelStartClips[118] = Resources.Load("instructions/119") as AudioClip;
        levelStartClips[119] = Resources.Load("instructions/120") as AudioClip;
        levelStartClips[120] = Resources.Load("instructions/121") as AudioClip;
        levelStartClips[121] = Resources.Load("instructions/122") as AudioClip;
        levelStartClips[122] = Resources.Load("instructions/123") as AudioClip;
        levelStartClips[123] = Resources.Load("instructions/124") as AudioClip;
        levelStartClips[124] = Resources.Load("instructions/125") as AudioClip;
        levelStartClips[125] = Resources.Load("instructions/126") as AudioClip;
        levelStartClips[126] = Resources.Load("instructions/127") as AudioClip;
        levelStartClips[127] = Resources.Load("instructions/128") as AudioClip;
        levelStartClips[128] = Resources.Load("instructions/129") as AudioClip;
        levelStartClips[129] = Resources.Load("instructions/130") as AudioClip;
        levelStartClips[130] = Resources.Load("instructions/131") as AudioClip;
        levelStartClips[131] = Resources.Load("instructions/132") as AudioClip;
        levelStartClips[132] = Resources.Load("instructions/133") as AudioClip;
        levelStartClips[133] = Resources.Load("instructions/134") as AudioClip;
        levelStartClips[134] = Resources.Load("instructions/135") as AudioClip;
        levelStartClips[135] = Resources.Load("instructions/136") as AudioClip;
        levelStartClips[136] = Resources.Load("instructions/137") as AudioClip;
        levelStartClips[137] = Resources.Load("instructions/138") as AudioClip;
        levelStartClips[138] = Resources.Load("instructions/139") as AudioClip;
        levelStartClips[139] = Resources.Load("instructions/140") as AudioClip;
        levelStartClips[140] = Resources.Load("instructions/141") as AudioClip;
        levelStartClips[141] = Resources.Load("instructions/142") as AudioClip;
        levelStartClips[142] = Resources.Load("instructions/143") as AudioClip;
        levelStartClips[143] = Resources.Load("instructions/144") as AudioClip;
        levelStartClips[144] = Resources.Load("instructions/145") as AudioClip;
        levelStartClips[145] = Resources.Load("instructions/146") as AudioClip;
        levelStartClips[146] = Resources.Load("instructions/147") as AudioClip;
        levelStartClips[147] = Resources.Load("instructions/148") as AudioClip;
        levelStartClips[148] = Resources.Load("instructions/149") as AudioClip;
        levelStartClips[149] = Resources.Load("instructions/150") as AudioClip;

        pauseMenuClips[0] = Resources.Load("instructions/pause_menu_opened") as AudioClip;
        pauseMenuClips[1] = Resources.Load("instructions/pause_menu_swipe_up_normal") as AudioClip;
        pauseMenuClips[2] = Resources.Load("instructions/pause_menu_swipe_up_talkback") as AudioClip;
        pauseMenuClips[3] = Resources.Load("instructions/pause_menu_swipe_left_normal") as AudioClip;
        pauseMenuClips[4] = Resources.Load("instructions/pause_menu_swipe_left_talkback") as AudioClip;
        pauseMenuClips[5] = Resources.Load("instructions/pause_menu_level_restart_normal") as AudioClip;
        pauseMenuClips[6] = Resources.Load("instructions/pause_menu_level_restart_talkback") as AudioClip;
        pauseMenuClips[7] = Resources.Load("instructions/pause_menu_swipe_right_normal") as AudioClip;
        pauseMenuClips[8] = Resources.Load("instructions/pause_menu_swipe_right_talkback") as AudioClip;
        pauseMenuClips[9] = Resources.Load("instructions/pause_menu_main_menu_normal") as AudioClip;
        pauseMenuClips[10] = Resources.Load("instructions/pause_menu_main_menu_talkback") as AudioClip;
        pauseMenuClips[11] = Resources.Load("instructions/pause_menu_closed") as AudioClip;

        hintClips[0] = Resources.Load("instructions/hint_should_move_forward") as AudioClip;
        hintClips[1] = Resources.Load("instructions/hint_should_turn_left") as AudioClip;
        hintClips[2] = Resources.Load("instructions/hint_should_turn_right") as AudioClip;
        hintClips[3] = Resources.Load("instructions/hint_should_exit") as AudioClip;

        // Error clips
        errorClips[0] = Resources.Load("instructions/tap_horizontal_error") as AudioClip;
        errorClips[1] = Resources.Load("instructions/tap_vertical_error") as AudioClip;
        errorClips[2] = Resources.Load("instructions/tap_rotation_error") as AudioClip;
        errorClips[3] = Resources.Load("instructions/swipe_left_horizontal_error") as AudioClip;
        errorClips[4] = Resources.Load("instructions/swipe_right_horizontal_error") as AudioClip;
        errorClips[5] = Resources.Load("instructions/swipe_up_vertical_error") as AudioClip;
        errorClips[6] = Resources.Load("instructions/swipe_down_vertical_error") as AudioClip;
        errorClips[7] = Resources.Load("instructions/swipe_rotation_error") as AudioClip;
        errorClips[8] = Resources.Load("instructions/rotation_angle_error") as AudioClip;
        errorClips[9] = Resources.Load("instructions/hold_horizontal_error") as AudioClip;
        errorClips[10] = Resources.Load("instructions/hold_vertical_error") as AudioClip;
        errorClips[11] = Resources.Load("instructions/hold_rotation_error") as AudioClip;
        errorClips[12] = Resources.Load("instructions/less_than_three_fingers") as AudioClip;
        errorClips[13] = Resources.Load("instructions/more_than_three_fingers") as AudioClip;
        errorClips[14] = Resources.Load("instructions/not_a_tap") as AudioClip;
        errorClips[15] = Resources.Load("instructions/not_a_swipe_up") as AudioClip;
        errorClips[16] = Resources.Load("instructions/not_a_hold") as AudioClip;
        errorClips[17] = Resources.Load("instructions/not_a_swipe_down") as AudioClip;
        errorClips[18] = Resources.Load("instructions/not_a_rotation") as AudioClip;
        errorClips[19] = Resources.Load("instructions/not_a_left_right_swipe") as AudioClip;

        // Sound effect clips
        soundEffectClips[0] = Resources.Load("fx/0_25_sec_silence") as AudioClip;
        soundEffectClips[1] = Resources.Load("fx/0_5_sec_silence") as AudioClip;
        soundEffectClips[2] = Resources.Load("fx/1_sec_silence") as AudioClip;
        soundEffectClips[3] = Resources.Load("fx/2_sec_silence") as AudioClip;
        soundEffectClips[4] = Resources.Load("fx/swipe-ahead") as AudioClip;
        soundEffectClips[5] = Resources.Load("fx/swipe-left") as AudioClip;
        soundEffectClips[6] = Resources.Load("fx/swipe-right") as AudioClip;
        soundEffectClips[7] = Resources.Load("fx/inputSFX") as AudioClip;
        soundEffectClips[8] = Resources.Load("fx/wall_hitting") as AudioClip;
        soundEffectClips[9] = Resources.Load("fx/winSound") as AudioClip;
    }
}
