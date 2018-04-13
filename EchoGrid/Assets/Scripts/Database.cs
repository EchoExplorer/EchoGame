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
    // Game environment settings clips
    [HideInInspector]
    public static AudioClip[] settingsClips = new AudioClip[10];
    
    // Main menu clips
    [HideInInspector]
    public static AudioClip[] mainMenuClips = new AudioClip[17];
    
    // Pregame menu clips
    [HideInInspector]
    public static AudioClip[] preGameMenuClips = new AudioClip[13];
    
    // Gesture tutorial clips
    [HideInInspector]
    public static AudioClip[] tutorialClips = new AudioClip[30];

    // Main game clips
    [HideInInspector]
    public static AudioClip[] mainGameClips = new AudioClip[39];
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
        // Game environment setting clips
        settingsClips[0] = Resources.Load("instructions/listen_to_instructions") as AudioClip;
        settingsClips[1] = Resources.Load("instructions/using_talkback") as AudioClip;
        settingsClips[2] = Resources.Load("instructions/use_three_fingers_normal") as AudioClip;
        settingsClips[3] = Resources.Load("instructions/use_three_fingers_talkback") as AudioClip;
        settingsClips[4] = Resources.Load("instructions/add_talkback_hold") as AudioClip;
        settingsClips[5] = Resources.Load("instructions/headphones_normal") as AudioClip;
        settingsClips[6] = Resources.Load("instructions/headphones_talkback") as AudioClip;
        settingsClips[7] = Resources.Load("instructions/orient_phone") as AudioClip;
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
        mainMenuClips[7] = Resources.Load("instructions/command_list_swipe_up_normal") as AudioClip;
        mainMenuClips[8] = Resources.Load("instructions/command_list_swipe_up_talkback") as AudioClip;
        mainMenuClips[9] = Resources.Load("instructions/command_list_tap_normal") as AudioClip;
        mainMenuClips[10] = Resources.Load("instructions/command_list_tap_talkback") as AudioClip;
        mainMenuClips[11] = Resources.Load("instructions/command_list_swipe_down_normal") as AudioClip;
        mainMenuClips[12] = Resources.Load("instructions/command_list_swipe_down_talkback") as AudioClip;
        mainMenuClips[13] = Resources.Load("instructions/command_list_rotation_normal") as AudioClip;
        mainMenuClips[14] = Resources.Load("instructions/command_list_rotation_talkback") as AudioClip;
        mainMenuClips[15] = Resources.Load("instructions/command_list_hold_normal") as AudioClip;
        mainMenuClips[16] = Resources.Load("instructions/command_list_hold_talkback") as AudioClip;

        // Pregame menu clips
        preGameMenuClips[0] = Resources.Load("instructions/swipe_left_normal") as AudioClip;
        preGameMenuClips[1] = Resources.Load("instructions/swipe_left_talkback") as AudioClip;
        preGameMenuClips[2] = Resources.Load("instructions/swipe_right_tutorial_normal") as AudioClip;
        preGameMenuClips[3] = Resources.Load("instructions/swipe_right_tutorial_talkback") as AudioClip;
        preGameMenuClips[4] = Resources.Load("instructions/swipe_right_maingame_normal") as AudioClip;
        preGameMenuClips[5] = Resources.Load("instructions/swipe_right_maingame_talkback") as AudioClip;
        preGameMenuClips[6] = Resources.Load("instructions/swipe_down_normal") as AudioClip;
        preGameMenuClips[7] = Resources.Load("instructions/swipe_down_talkback") as AudioClip;
        preGameMenuClips[8] = Resources.Load("instructions/overwrite_saves") as AudioClip;
        preGameMenuClips[9] = Resources.Load("instructions/confirm_normal") as AudioClip;
        preGameMenuClips[10] = Resources.Load("instructions/confirm_talkback") as AudioClip;
        preGameMenuClips[11] = Resources.Load("instructions/new_game_started") as AudioClip;
        preGameMenuClips[12] = Resources.Load("instructions/loaded_saved_game") as AudioClip;

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
