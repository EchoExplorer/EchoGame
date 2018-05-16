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
	public static AudioClip hrtf_front;
    public static AudioClip hrtf_front_leftspeaker;
    public static AudioClip hrtf_front_rightspeaker;
    public static AudioClip hrtf_left_leftspeaker;
	public static AudioClip hrtf_left_rightspeaker;
    public static AudioClip hrtf_right_leftspeaker;
    public static AudioClip hrtf_right_rightspeaker;
    public static AudioClip hrtf_leftfront_leftspeaker;
    public static AudioClip hrtf_leftfront_rightspeaker;
    public static AudioClip hrtf_rightfront_leftspeaker;
    public static AudioClip hrtf_rightfront_rightspeaker;
    public static AudioClip hrtf_left;
    public static AudioClip hrtf_right;
    public static AudioClip hrtf_leftfront;
	public static AudioClip hrtf_rightfront;

    public static AudioClip attenuatedaround_odeon;
    public static AudioClip attenuatedClickfront_odeon;

    // Consent menu clips
    [HideInInspector]
    public static AudioClip[] consentClips = new AudioClip[12];

    // Game environment settings clips
    [HideInInspector]
    public static AudioClip[] settingsClips = new AudioClip[10];

    // Main menu clips
    [HideInInspector]
    public static AudioClip[] mainMenuClips = new AudioClip[24];

    // Pregame menu clips
    [HideInInspector]
    public static AudioClip[] preGameMenuClips = new AudioClip[35];

    // Gesture tutorial clips
    [HideInInspector]
    public static AudioClip[] tutorialClips = new AudioClip[30];

    // Main game clips
    [HideInInspector]
    public static AudioClip[] mainGameClips = new AudioClip[39];
    [HideInInspector]
    public static AudioClip[] levelStartClips = new AudioClip[151];
    [HideInInspector]
    public static AudioClip[] pauseMenuClips = new AudioClip[12];
    [HideInInspector]
    public static AudioClip[] hintClips = new AudioClip[5];

    // Error clips
    [HideInInspector]
    public static AudioClip[] errorClips = new AudioClip[19];

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
		attenuatedClick = Resources.Load("fx/echolocaterclicks_3rd_attenuated") as AudioClip;
        attenuatedaround_odeon = Resources.Load("fx/00-0_F-0.75-D_B-w-D_L-w-D_R-w-D_fadeout") as AudioClip;

        //Option1:
		hrtf_front =Resources.Load("fx/kemar_front") as AudioClip;
        hrtf_front_leftspeaker = Resources.Load("fx/kemar_front_leftspeaker") as AudioClip;
        hrtf_front_rightspeaker = Resources.Load("fx/kemar_front_rightspeaker") as AudioClip;
        hrtf_left_leftspeaker =Resources.Load("fx/kemar_left_leftspeaker") as AudioClip;
		hrtf_left_rightspeaker=Resources.Load("fx/kemar_left_rightspeaker") as AudioClip;
        hrtf_right_leftspeaker = Resources.Load("fx/kemar_right_leftspeaker") as AudioClip;
        hrtf_right_rightspeaker = Resources.Load("fx/kemar_right_rightspeaker") as AudioClip;
        hrtf_leftfront_leftspeaker = Resources.Load("fx/kemar_leftfront_leftspeaker") as AudioClip;
        hrtf_leftfront_rightspeaker = Resources.Load("fx/kemar_leftfront_rightspeaker") as AudioClip;
        hrtf_rightfront_leftspeaker = Resources.Load("fx/kemar_rightfront_leftspeaker") as AudioClip;
        hrtf_rightfront_rightspeaker = Resources.Load("fx/kemar_rightfront_rightspeaker") as AudioClip;
        hrtf_left = Resources.Load("fx/kemar_left") as AudioClip;
        hrtf_right =Resources.Load("fx/kemar_right") as AudioClip;
		hrtf_leftfront =Resources.Load("fx/kemar_leftfront") as AudioClip;
		hrtf_rightfront =Resources.Load("fx/kemar_rightfront") as AudioClip;

        // Consent menu clips
        consentClips[0] = Resources.Load("instructions/consent/consent_intro") as AudioClip;
        consentClips[1] = Resources.Load("instructions/consent/consent_form") as AudioClip;
        consentClips[2] = Resources.Load("instructions/consent/eighteen_plus") as AudioClip;
        consentClips[3] = Resources.Load("instructions/consent/read_and_understand") as AudioClip;
        consentClips[4] = Resources.Load("instructions/consent/want_to_participate") as AudioClip;
        consentClips[5] = Resources.Load("instructions/consent/are_you_sure_you_consent") as AudioClip;
        consentClips[6] = Resources.Load("instructions/consent/are_you_sure_no_consent") as AudioClip;
        consentClips[7] = Resources.Load("instructions/consent/thank_you_for_consenting") as AudioClip;
        consentClips[8] = Resources.Load("instructions/consent/did_not_consent") as AudioClip;
        consentClips[9] = Resources.Load("instructions/consent/not_eighteen_plus") as AudioClip;
        consentClips[10] = Resources.Load("instructions/consent/did_not_understand_consent") as AudioClip;
        consentClips[11] = Resources.Load("instructions/consent/does_not_want_to_participate") as AudioClip;

        // Game environment setting clips
        settingsClips[0] = Resources.Load("instructions/settings/orient_phone") as AudioClip;
        settingsClips[1] = Resources.Load("instructions/settings/listen_to_instructions") as AudioClip;
        settingsClips[2] = Resources.Load("instructions/settings/using_talkback") as AudioClip;
        settingsClips[3] = Resources.Load("instructions/settings/add_talkback_hold") as AudioClip;
        settingsClips[4] = Resources.Load("instructions/settings/use_two_three_fingers_normal") as AudioClip;
        settingsClips[5] = Resources.Load("instructions/settings/use_three_fingers_talkback") as AudioClip;
        settingsClips[6] = Resources.Load("instructions/settings/headphones_normal") as AudioClip;
        settingsClips[7] = Resources.Load("instructions/settings/headphones_talkback") as AudioClip;
        settingsClips[8] = Resources.Load("instructions/settings/environment_setup_normal") as AudioClip;
        settingsClips[9] = Resources.Load("instructions/settings/environment_setup_talkback") as AudioClip;

        // Main menu clips
        mainMenuClips[0] = Resources.Load("instructions/main_menu/welcome_to_echo_adventure") as AudioClip;
        mainMenuClips[1] = Resources.Load("instructions/main_menu/main_menu_swipe_right_normal") as AudioClip;
        mainMenuClips[2] = Resources.Load("instructions/main_menu/main_menu_swipe_right_talkback") as AudioClip;
        mainMenuClips[3] = Resources.Load("instructions/main_menu/main_menu_swipe_left_normal") as AudioClip;
        mainMenuClips[4] = Resources.Load("instructions/main_menu/main_menu_swipe_left_talkback") as AudioClip;
        mainMenuClips[5] = Resources.Load("instructions/main_menu/main_menu_swipe_up_normal") as AudioClip;
        mainMenuClips[6] = Resources.Load("instructions/main_menu/main_menu_swipe_up_talkback") as AudioClip;
        mainMenuClips[7] = Resources.Load("instructions/main_menu/main_menu_swipe_down_normal") as AudioClip;
        mainMenuClips[8] = Resources.Load("instructions/main_menu/main_menu_swipe_down_talkback") as AudioClip;
        mainMenuClips[9] = Resources.Load("instructions/main_menu/options_menu_normal") as AudioClip;
        mainMenuClips[10] = Resources.Load("instructions/main_menu/options_menu_talkback") as AudioClip;
        mainMenuClips[11] = Resources.Load("instructions/main_menu/closed_options_menu") as AudioClip;
        mainMenuClips[12] = Resources.Load("instructions/main_menu/using_hrtf_echoes") as AudioClip;
        mainMenuClips[13] = Resources.Load("instructions/main_menu/using_odeon_echoes") as AudioClip;
        mainMenuClips[14] = Resources.Load("instructions/main_menu/command_list_swipe_up_normal") as AudioClip;
        mainMenuClips[15] = Resources.Load("instructions/main_menu/command_list_swipe_up_talkback") as AudioClip;
        mainMenuClips[16] = Resources.Load("instructions/main_menu/command_list_tap_normal") as AudioClip;
        mainMenuClips[17] = Resources.Load("instructions/main_menu/command_list_tap_talkback") as AudioClip;
        mainMenuClips[18] = Resources.Load("instructions/main_menu/command_list_swipe_down_normal") as AudioClip;
        mainMenuClips[19] = Resources.Load("instructions/main_menu/command_list_swipe_down_talkback") as AudioClip;
        mainMenuClips[20] = Resources.Load("instructions/main_menu/command_list_rotation_normal") as AudioClip;
        mainMenuClips[21] = Resources.Load("instructions/main_menu/command_list_rotation_talkback") as AudioClip;
        mainMenuClips[22] = Resources.Load("instructions/main_menu/command_list_hold_normal") as AudioClip;
        mainMenuClips[23] = Resources.Load("instructions/main_menu/command_list_hold_talkback") as AudioClip;

        // Pregame menu clips
        preGameMenuClips[0] = Resources.Load("instructions/pregame_menu/tutorial_swipe_left_normal") as AudioClip;
        preGameMenuClips[1] = Resources.Load("instructions/pregame_menu/tutorial_swipe_left_talkback") as AudioClip;
        preGameMenuClips[2] = Resources.Load("instructions/pregame_menu/tutorial_swipe_right_normal") as AudioClip;
        preGameMenuClips[3] = Resources.Load("instructions/pregame_menu/tutorial_swipe_right_talkback") as AudioClip;
        preGameMenuClips[4] = Resources.Load("instructions/pregame_menu/maingame_swipe_left_normal") as AudioClip;
        preGameMenuClips[5] = Resources.Load("instructions/pregame_menu/maingame_swipe_left_talkback") as AudioClip;
        preGameMenuClips[6] = Resources.Load("instructions/pregame_menu/maingame_swipe_right_normal") as AudioClip;
        preGameMenuClips[7] = Resources.Load("instructions/pregame_menu/maingame_swipe_right_talkback") as AudioClip;
        preGameMenuClips[8] = Resources.Load("instructions/pregame_menu/swipe_down_normal") as AudioClip;
        preGameMenuClips[9] = Resources.Load("instructions/pregame_menu/swipe_down_talkback") as AudioClip;
        preGameMenuClips[10] = Resources.Load("instructions/pregame_menu/overwrite_saves") as AudioClip;
        preGameMenuClips[11] = Resources.Load("instructions/pregame_menu/confirm_normal") as AudioClip;
        preGameMenuClips[12] = Resources.Load("instructions/pregame_menu/confirm_talkback") as AudioClip;
        preGameMenuClips[13] = Resources.Load("instructions/pregame_menu/new_game_started") as AudioClip;
        preGameMenuClips[14] = Resources.Load("instructions/pregame_menu/loaded_saved_game") as AudioClip;
        preGameMenuClips[15] = Resources.Load("instructions/pregame_menu/tutorial_swipe_up_normal") as AudioClip;
        preGameMenuClips[16] = Resources.Load("instructions/pregame_menu/tutorial_swipe_up_talkback") as AudioClip;
        preGameMenuClips[17] = Resources.Load("instructions/pregame_menu/maingame_swipe_up_normal") as AudioClip;
        preGameMenuClips[18] = Resources.Load("instructions/pregame_menu/maingame_swipe_up_talkback") as AudioClip;
        preGameMenuClips[19] = Resources.Load("instructions/pregame_menu/default_start_level") as AudioClip;
        preGameMenuClips[20] = Resources.Load("instructions/pregame_menu/play_selected_level_normal") as AudioClip;
        preGameMenuClips[21] = Resources.Load("instructions/pregame_menu/play_selected_level_talkback") as AudioClip;
        preGameMenuClips[22] = Resources.Load("instructions/pregame_menu/select_higher_level_normal") as AudioClip;
        preGameMenuClips[23] = Resources.Load("instructions/pregame_menu/select_higher_level_talkback") as AudioClip;
        preGameMenuClips[24] = Resources.Load("instructions/pregame_menu/select_lower_level_normal") as AudioClip;
        preGameMenuClips[25] = Resources.Load("instructions/pregame_menu/select_lower_level_talkback") as AudioClip;
        preGameMenuClips[26] = Resources.Load("instructions/pregame_menu/tutorial_back_to_new_continue_normal") as AudioClip;
        preGameMenuClips[27] = Resources.Load("instructions/pregame_menu/tutorial_back_to_new_continue_talkback") as AudioClip;
        preGameMenuClips[28] = Resources.Load("instructions/pregame_menu/maingame_back_to_new_continue_normal") as AudioClip;
        preGameMenuClips[29] = Resources.Load("instructions/pregame_menu/maingame_back_to_new_continue_talkback") as AudioClip;
        preGameMenuClips[30] = Resources.Load("instructions/pregame_menu/playing_selected_level") as AudioClip;
        preGameMenuClips[31] = Resources.Load("instructions/pregame_menu/tutorial_at_lowest_level") as AudioClip;
        preGameMenuClips[32] = Resources.Load("instructions/pregame_menu/tutorial_at_highest_level") as AudioClip;
        preGameMenuClips[33] = Resources.Load("instructions/pregame_menu/maingame_at_lowest_level") as AudioClip;
        preGameMenuClips[34] = Resources.Load("instructions/pregame_menu/maingame_at_highest_level") as AudioClip;

        // Gesture tutorial clips      
        tutorialClips[0] = Resources.Load("instructions/tutorial/tutorial_reminder") as AudioClip;
        tutorialClips[1] = Resources.Load("instructions/tutorial/tap_instruction_normal") as AudioClip;
        tutorialClips[2] = Resources.Load("instructions/tutorial/tap_instruction_talkback") as AudioClip;
        tutorialClips[3] = Resources.Load("instructions/tutorial/sound_will_change") as AudioClip;
        tutorialClips[4] = Resources.Load("instructions/tutorial/tap_three_times") as AudioClip;
        tutorialClips[5] = Resources.Load("instructions/tutorial/tap_correct_2_more") as AudioClip;
        tutorialClips[6] = Resources.Load("instructions/tutorial/tap_correct_1_more") as AudioClip;
        tutorialClips[7] = Resources.Load("instructions/tutorial/finished_tap_section") as AudioClip;
        tutorialClips[8] = Resources.Load("instructions/tutorial/swipe_up_instruction_normal") as AudioClip;
        tutorialClips[9] = Resources.Load("instructions/tutorial/swipe_up_instruction_talkback") as AudioClip;
        tutorialClips[10] = Resources.Load("instructions/tutorial/swipe_three_times") as AudioClip;
        tutorialClips[11] = Resources.Load("instructions/tutorial/swipe_correct_2_more") as AudioClip;
        tutorialClips[12] = Resources.Load("instructions/tutorial/swipe_correct_1_more") as AudioClip;
        tutorialClips[13] = Resources.Load("instructions/tutorial/finished_swipe_section") as AudioClip;
        tutorialClips[14] = Resources.Load("instructions/tutorial/pause_menu_instruction_normal") as AudioClip;
        tutorialClips[15] = Resources.Load("instructions/tutorial/pause_menu_instruction_talkback") as AudioClip;
        tutorialClips[16] = Resources.Load("instructions/tutorial/pause_menu_explanation_normal") as AudioClip;
        tutorialClips[17] = Resources.Load("instructions/tutorial/pause_menu_explanation_talkback") as AudioClip;
        tutorialClips[18] = Resources.Load("instructions/tutorial/exit_level_instruction_normal") as AudioClip;
        tutorialClips[19] = Resources.Load("instructions/tutorial/exit_level_instruction_talkback") as AudioClip;
        tutorialClips[20] = Resources.Load("instructions/tutorial/try_swipe_down") as AudioClip;
        tutorialClips[21] = Resources.Load("instructions/tutorial/reached_corner") as AudioClip;
        tutorialClips[22] = Resources.Load("instructions/tutorial/rotate_instruction_normal") as AudioClip;
        tutorialClips[23] = Resources.Load("instructions/tutorial/rotate_instruction_talkback") as AudioClip;
        tutorialClips[24] = Resources.Load("instructions/tutorial/tap_after_rotating") as AudioClip;
        tutorialClips[25] = Resources.Load("instructions/tutorial/rotate_4_times") as AudioClip;
        tutorialClips[26] = Resources.Load("instructions/tutorial/rotation_correct_3_more") as AudioClip;
        tutorialClips[27] = Resources.Load("instructions/tutorial/rotation_correct_2_more") as AudioClip;
        tutorialClips[28] = Resources.Load("instructions/tutorial/rotation_correct_1_more") as AudioClip;
        tutorialClips[29] = Resources.Load("instructions/tutorial/get_around_corner") as AudioClip;

        // Main game clips
        mainGameClips[0] = Resources.Load("instructions/main_game/welcome_you_are_in_a_dark_maze") as AudioClip;
        mainGameClips[1] = Resources.Load("instructions/main_game/currently_in_straight_hallway") as AudioClip;
        mainGameClips[2] = Resources.Load("instructions/main_game/stairs_at_end_of_hallway") as AudioClip;
        mainGameClips[3] = Resources.Load("instructions/main_game/if_you_hit_a_wall") as AudioClip;
        mainGameClips[4] = Resources.Load("instructions/main_game/echolocation_simulation") as AudioClip;
        mainGameClips[5] = Resources.Load("instructions/main_game/tap_before_step") as AudioClip;
        mainGameClips[6] = Resources.Load("instructions/main_game/swipe_up_to_move_forward") as AudioClip;
        mainGameClips[7] = Resources.Load("instructions/main_game/get_to_stairs_without_crashing") as AudioClip;
        mainGameClips[8] = Resources.Load("instructions/main_game/proceed_by_tapping") as AudioClip;
        mainGameClips[9] = Resources.Load("instructions/main_game/tap_at_exit") as AudioClip;
        mainGameClips[10] = Resources.Load("instructions/main_game/another_straight_hallway") as AudioClip;
        mainGameClips[11] = Resources.Load("instructions/main_game/swipe_down_to_attempt_exit") as AudioClip;
        mainGameClips[12] = Resources.Load("instructions/main_game/move_on_from_straight_hallways") as AudioClip;
        mainGameClips[13] = Resources.Load("instructions/main_game/this_is_level_3") as AudioClip;
        mainGameClips[14] = Resources.Load("instructions/main_game/approaching_corner") as AudioClip;
        mainGameClips[15] = Resources.Load("instructions/main_game/reached_right_corner") as AudioClip;
        mainGameClips[16] = Resources.Load("instructions/main_game/get_confused") as AudioClip;
        mainGameClips[17] = Resources.Load("instructions/main_game/good_job") as AudioClip;
        mainGameClips[18] = Resources.Load("instructions/main_game/move_forward_until_stairs") as AudioClip;
        mainGameClips[19] = Resources.Load("instructions/main_game/hallway_has_another_right_turn") as AudioClip;
        mainGameClips[20] = Resources.Load("instructions/main_game/hallway_has_left_turn") as AudioClip;
        mainGameClips[21] = Resources.Load("instructions/main_game/reached_left_corner") as AudioClip;
        mainGameClips[22] = Resources.Load("instructions/main_game/you_have_turned_left") as AudioClip;
        mainGameClips[23] = Resources.Load("instructions/main_game/finished_first_part_of_tutorial") as AudioClip;
        mainGameClips[24] = Resources.Load("instructions/main_game/some_harder_levels") as AudioClip;
        mainGameClips[25] = Resources.Load("instructions/main_game/introduce_T_hallway") as AudioClip;
        mainGameClips[26] = Resources.Load("instructions/main_game/stairs_in_one_of_two_arms_of_T") as AudioClip;
        mainGameClips[27] = Resources.Load("instructions/main_game/turn_around_in_wrong_arm") as AudioClip;
        mainGameClips[28] = Resources.Load("instructions/main_game/in_T_fork") as AudioClip;
        mainGameClips[29] = Resources.Load("instructions/main_game/completed_all_of_tutorial") as AudioClip;
        mainGameClips[30] = Resources.Load("instructions/main_game/back_at_entrance_turn_around") as AudioClip;
        mainGameClips[31] = Resources.Load("instructions/main_game/good") as AudioClip;
        mainGameClips[32] = Resources.Load("instructions/main_game/good_progress") as AudioClip;
        mainGameClips[33] = Resources.Load("instructions/main_game/halfway_there") as AudioClip;
        mainGameClips[34] = Resources.Load("instructions/main_game/keep_moving_forward") as AudioClip;
        mainGameClips[35] = Resources.Load("instructions/main_game/not_the_exit") as AudioClip;
        mainGameClips[36] = Resources.Load("instructions/main_game/crashed_at_exit") as AudioClip;
        mainGameClips[37] = Resources.Load("instructions/main_game/crashed_right_corner") as AudioClip;
        mainGameClips[38] = Resources.Load("instructions/main_game/crashed_left_corner") as AudioClip;

        // Level start clips
        levelStartClips[0] = Resources.Load("instructions/level_start/start") as AudioClip;
        levelStartClips[1] = Resources.Load("instructions/level_start/1") as AudioClip;
        levelStartClips[2] = Resources.Load("instructions/level_start/2") as AudioClip;
        levelStartClips[3] = Resources.Load("instructions/level_start/3") as AudioClip;
        levelStartClips[4] = Resources.Load("instructions/level_start/4") as AudioClip;
        levelStartClips[5] = Resources.Load("instructions/level_start/5") as AudioClip;
        levelStartClips[6] = Resources.Load("instructions/level_start/6") as AudioClip;
        levelStartClips[7] = Resources.Load("instructions/level_start/7") as AudioClip;
        levelStartClips[8] = Resources.Load("instructions/level_start/8") as AudioClip;
        levelStartClips[9] = Resources.Load("instructions/level_start/9") as AudioClip;
        levelStartClips[10] = Resources.Load("instructions/level_start/10") as AudioClip;
        levelStartClips[11] = Resources.Load("instructions/level_start/11") as AudioClip;
        levelStartClips[12] = Resources.Load("instructions/level_start/12") as AudioClip;
        levelStartClips[13] = Resources.Load("instructions/level_start/13") as AudioClip;
        levelStartClips[14] = Resources.Load("instructions/level_start/14") as AudioClip;
        levelStartClips[15] = Resources.Load("instructions/level_start/15") as AudioClip;
        levelStartClips[16] = Resources.Load("instructions/level_start/16") as AudioClip;
        levelStartClips[17] = Resources.Load("instructions/level_start/17") as AudioClip;
        levelStartClips[18] = Resources.Load("instructions/level_start/18") as AudioClip;
        levelStartClips[19] = Resources.Load("instructions/level_start/19") as AudioClip;
        levelStartClips[20] = Resources.Load("instructions/level_start/20") as AudioClip;
        levelStartClips[21] = Resources.Load("instructions/level_start/21") as AudioClip;
        levelStartClips[22] = Resources.Load("instructions/level_start/22") as AudioClip;
        levelStartClips[23] = Resources.Load("instructions/level_start/23") as AudioClip;
        levelStartClips[24] = Resources.Load("instructions/level_start/24") as AudioClip;
        levelStartClips[25] = Resources.Load("instructions/level_start/25") as AudioClip;
        levelStartClips[26] = Resources.Load("instructions/level_start/26") as AudioClip;
        levelStartClips[27] = Resources.Load("instructions/level_start/27") as AudioClip;
        levelStartClips[28] = Resources.Load("instructions/level_start/28") as AudioClip;
        levelStartClips[29] = Resources.Load("instructions/level_start/29") as AudioClip;
        levelStartClips[30] = Resources.Load("instructions/level_start/30") as AudioClip;
        levelStartClips[31] = Resources.Load("instructions/level_start/31") as AudioClip;
        levelStartClips[32] = Resources.Load("instructions/level_start/32") as AudioClip;
        levelStartClips[33] = Resources.Load("instructions/level_start/33") as AudioClip;
        levelStartClips[34] = Resources.Load("instructions/level_start/34") as AudioClip;
        levelStartClips[35] = Resources.Load("instructions/level_start/35") as AudioClip;
        levelStartClips[36] = Resources.Load("instructions/level_start/36") as AudioClip;
        levelStartClips[37] = Resources.Load("instructions/level_start/37") as AudioClip;
        levelStartClips[38] = Resources.Load("instructions/level_start/38") as AudioClip;
        levelStartClips[39] = Resources.Load("instructions/level_start/39") as AudioClip;
        levelStartClips[40] = Resources.Load("instructions/level_start/40") as AudioClip;
        levelStartClips[41] = Resources.Load("instructions/level_start/41") as AudioClip;
        levelStartClips[42] = Resources.Load("instructions/level_start/42") as AudioClip;
        levelStartClips[43] = Resources.Load("instructions/level_start/43") as AudioClip;
        levelStartClips[44] = Resources.Load("instructions/level_start/44") as AudioClip;
        levelStartClips[45] = Resources.Load("instructions/level_start/45") as AudioClip;
        levelStartClips[46] = Resources.Load("instructions/level_start/46") as AudioClip;
        levelStartClips[47] = Resources.Load("instructions/level_start/47") as AudioClip;
        levelStartClips[48] = Resources.Load("instructions/level_start/48") as AudioClip;
        levelStartClips[49] = Resources.Load("instructions/level_start/49") as AudioClip;
        levelStartClips[50] = Resources.Load("instructions/level_start/50") as AudioClip;
        levelStartClips[51] = Resources.Load("instructions/level_start/51") as AudioClip;
        levelStartClips[52] = Resources.Load("instructions/level_start/52") as AudioClip;
        levelStartClips[53] = Resources.Load("instructions/level_start/53") as AudioClip;
        levelStartClips[54] = Resources.Load("instructions/level_start/54") as AudioClip;
        levelStartClips[55] = Resources.Load("instructions/level_start/55") as AudioClip;
        levelStartClips[56] = Resources.Load("instructions/level_start/56") as AudioClip;
        levelStartClips[57] = Resources.Load("instructions/level_start/57") as AudioClip;
        levelStartClips[58] = Resources.Load("instructions/level_start/58") as AudioClip;
        levelStartClips[59] = Resources.Load("instructions/level_start/59") as AudioClip;
        levelStartClips[60] = Resources.Load("instructions/level_start/60") as AudioClip;
        levelStartClips[61] = Resources.Load("instructions/level_start/61") as AudioClip;
        levelStartClips[62] = Resources.Load("instructions/level_start/62") as AudioClip;
        levelStartClips[63] = Resources.Load("instructions/level_start/63") as AudioClip;
        levelStartClips[64] = Resources.Load("instructions/level_start/64") as AudioClip;
        levelStartClips[65] = Resources.Load("instructions/level_start/65") as AudioClip;
        levelStartClips[66] = Resources.Load("instructions/level_start/66") as AudioClip;
        levelStartClips[67] = Resources.Load("instructions/level_start/67") as AudioClip;
        levelStartClips[68] = Resources.Load("instructions/level_start/68") as AudioClip;
        levelStartClips[69] = Resources.Load("instructions/level_start/69") as AudioClip;
        levelStartClips[70] = Resources.Load("instructions/level_start/70") as AudioClip;
        levelStartClips[71] = Resources.Load("instructions/level_start/71") as AudioClip;
        levelStartClips[72] = Resources.Load("instructions/level_start/72") as AudioClip;
        levelStartClips[73] = Resources.Load("instructions/level_start/73") as AudioClip;
        levelStartClips[74] = Resources.Load("instructions/level_start/74") as AudioClip;
        levelStartClips[75] = Resources.Load("instructions/level_start/75") as AudioClip;
        levelStartClips[76] = Resources.Load("instructions/level_start/76") as AudioClip;
        levelStartClips[77] = Resources.Load("instructions/level_start/77") as AudioClip;
        levelStartClips[78] = Resources.Load("instructions/level_start/78") as AudioClip;
        levelStartClips[79] = Resources.Load("instructions/level_start/79") as AudioClip;
        levelStartClips[80] = Resources.Load("instructions/level_start/80") as AudioClip;
        levelStartClips[81] = Resources.Load("instructions/level_start/81") as AudioClip;
        levelStartClips[82] = Resources.Load("instructions/level_start/82") as AudioClip;
        levelStartClips[83] = Resources.Load("instructions/level_start/83") as AudioClip;
        levelStartClips[84] = Resources.Load("instructions/level_start/84") as AudioClip;
        levelStartClips[85] = Resources.Load("instructions/level_start/85") as AudioClip;
        levelStartClips[86] = Resources.Load("instructions/level_start/86") as AudioClip;
        levelStartClips[87] = Resources.Load("instructions/level_start/87") as AudioClip;
        levelStartClips[88] = Resources.Load("instructions/level_start/88") as AudioClip;
        levelStartClips[89] = Resources.Load("instructions/level_start/89") as AudioClip;
        levelStartClips[90] = Resources.Load("instructions/level_start/90") as AudioClip;
        levelStartClips[91] = Resources.Load("instructions/level_start/91") as AudioClip;
        levelStartClips[92] = Resources.Load("instructions/level_start/92") as AudioClip;
        levelStartClips[93] = Resources.Load("instructions/level_start/93") as AudioClip;
        levelStartClips[94] = Resources.Load("instructions/level_start/94") as AudioClip;
        levelStartClips[95] = Resources.Load("instructions/level_start/95") as AudioClip;
        levelStartClips[96] = Resources.Load("instructions/level_start/96") as AudioClip;
        levelStartClips[97] = Resources.Load("instructions/level_start/97") as AudioClip;
        levelStartClips[98] = Resources.Load("instructions/level_start/98") as AudioClip;
        levelStartClips[99] = Resources.Load("instructions/level_start/99") as AudioClip;
        levelStartClips[100] = Resources.Load("instructions/level_start/100") as AudioClip;
        levelStartClips[101] = Resources.Load("instructions/level_start/101") as AudioClip;
        levelStartClips[102] = Resources.Load("instructions/level_start/102") as AudioClip;
        levelStartClips[103] = Resources.Load("instructions/level_start/103") as AudioClip;
        levelStartClips[104] = Resources.Load("instructions/level_start/104") as AudioClip;
        levelStartClips[105] = Resources.Load("instructions/level_start/105") as AudioClip;
        levelStartClips[106] = Resources.Load("instructions/level_start/106") as AudioClip;
        levelStartClips[107] = Resources.Load("instructions/level_start/107") as AudioClip;
        levelStartClips[108] = Resources.Load("instructions/level_start/108") as AudioClip;
        levelStartClips[109] = Resources.Load("instructions/level_start/109") as AudioClip;
        levelStartClips[110] = Resources.Load("instructions/level_start/110") as AudioClip;
        levelStartClips[111] = Resources.Load("instructions/level_start/111") as AudioClip;
        levelStartClips[112] = Resources.Load("instructions/level_start/112") as AudioClip;
        levelStartClips[113] = Resources.Load("instructions/level_start/113") as AudioClip;
        levelStartClips[114] = Resources.Load("instructions/level_start/114") as AudioClip;
        levelStartClips[115] = Resources.Load("instructions/level_start/115") as AudioClip;
        levelStartClips[116] = Resources.Load("instructions/level_start/116") as AudioClip;
        levelStartClips[117] = Resources.Load("instructions/level_start/117") as AudioClip;
        levelStartClips[118] = Resources.Load("instructions/level_start/118") as AudioClip;
        levelStartClips[119] = Resources.Load("instructions/level_start/119") as AudioClip;
        levelStartClips[120] = Resources.Load("instructions/level_start/120") as AudioClip;
        levelStartClips[121] = Resources.Load("instructions/level_start/121") as AudioClip;
        levelStartClips[122] = Resources.Load("instructions/level_start/122") as AudioClip;
        levelStartClips[123] = Resources.Load("instructions/level_start/123") as AudioClip;
        levelStartClips[124] = Resources.Load("instructions/level_start/124") as AudioClip;
        levelStartClips[125] = Resources.Load("instructions/level_start/125") as AudioClip;
        levelStartClips[126] = Resources.Load("instructions/level_start/126") as AudioClip;
        levelStartClips[127] = Resources.Load("instructions/level_start/127") as AudioClip;
        levelStartClips[128] = Resources.Load("instructions/level_start/128") as AudioClip;
        levelStartClips[129] = Resources.Load("instructions/level_start/129") as AudioClip;
        levelStartClips[130] = Resources.Load("instructions/level_start/130") as AudioClip;
        levelStartClips[131] = Resources.Load("instructions/level_start/131") as AudioClip;
        levelStartClips[132] = Resources.Load("instructions/level_start/132") as AudioClip;
        levelStartClips[133] = Resources.Load("instructions/level_start/133") as AudioClip;
        levelStartClips[134] = Resources.Load("instructions/level_start/134") as AudioClip;
        levelStartClips[135] = Resources.Load("instructions/level_start/135") as AudioClip;
        levelStartClips[136] = Resources.Load("instructions/level_start/136") as AudioClip;
        levelStartClips[137] = Resources.Load("instructions/level_start/137") as AudioClip;
        levelStartClips[138] = Resources.Load("instructions/level_start/138") as AudioClip;
        levelStartClips[139] = Resources.Load("instructions/level_start/139") as AudioClip;
        levelStartClips[140] = Resources.Load("instructions/level_start/140") as AudioClip;
        levelStartClips[141] = Resources.Load("instructions/level_start/141") as AudioClip;
        levelStartClips[142] = Resources.Load("instructions/level_start/142") as AudioClip;
        levelStartClips[143] = Resources.Load("instructions/level_start/143") as AudioClip;
        levelStartClips[144] = Resources.Load("instructions/level_start/144") as AudioClip;
        levelStartClips[145] = Resources.Load("instructions/level_start/145") as AudioClip;
        levelStartClips[146] = Resources.Load("instructions/level_start/146") as AudioClip;
        levelStartClips[147] = Resources.Load("instructions/level_start/147") as AudioClip;
        levelStartClips[148] = Resources.Load("instructions/level_start/148") as AudioClip;
        levelStartClips[149] = Resources.Load("instructions/level_start/149") as AudioClip;
        levelStartClips[150] = Resources.Load("instructions/level_start/150") as AudioClip;

        // Pause menu clips
        pauseMenuClips[0] = Resources.Load("instructions/pause_menu/menu_opened") as AudioClip;
        pauseMenuClips[1] = Resources.Load("instructions/pause_menu/swipe_up_normal") as AudioClip;
        pauseMenuClips[2] = Resources.Load("instructions/pause_menu/swipe_up_talkback") as AudioClip;
        pauseMenuClips[3] = Resources.Load("instructions/pause_menu/swipe_left_normal") as AudioClip;
        pauseMenuClips[4] = Resources.Load("instructions/pause_menu/swipe_left_talkback") as AudioClip;
        pauseMenuClips[5] = Resources.Load("instructions/pause_menu/level_restart_normal") as AudioClip;
        pauseMenuClips[6] = Resources.Load("instructions/pause_menu/level_restart_talkback") as AudioClip;
        pauseMenuClips[7] = Resources.Load("instructions/pause_menu/swipe_right_normal") as AudioClip;
        pauseMenuClips[8] = Resources.Load("instructions/pause_menu/swipe_right_talkback") as AudioClip;
        pauseMenuClips[9] = Resources.Load("instructions/pause_menu/main_menu_normal") as AudioClip;
        pauseMenuClips[10] = Resources.Load("instructions/pause_menu/main_menu_talkback") as AudioClip;
        pauseMenuClips[11] = Resources.Load("instructions/pause_menu/menu_closed") as AudioClip;

        // Hint clips
        hintClips[0] = Resources.Load("instructions/hints/should_move_forward") as AudioClip;
        hintClips[1] = Resources.Load("instructions/hints/should_turn_left") as AudioClip;
        hintClips[2] = Resources.Load("instructions/hints/should_turn_right") as AudioClip;
        hintClips[3] = Resources.Load("instructions/hints/should_exit") as AudioClip;
        hintClips[4] = Resources.Load("instructions/hints/should_turn_around") as AudioClip;

        // Error clips
        errorClips[0] = Resources.Load("instructions/errors/tap_horizontal_error") as AudioClip;
        errorClips[1] = Resources.Load("instructions/errors/tap_vertical_error") as AudioClip;
        errorClips[2] = Resources.Load("instructions/errors/tap_rotation_error") as AudioClip;
        errorClips[3] = Resources.Load("instructions/errors/swipe_left_horizontal_error") as AudioClip;
        errorClips[4] = Resources.Load("instructions/errors/swipe_right_horizontal_error") as AudioClip;
        errorClips[5] = Resources.Load("instructions/errors/swipe_up_vertical_error") as AudioClip;
        errorClips[6] = Resources.Load("instructions/errors/swipe_down_vertical_error") as AudioClip;
        errorClips[7] = Resources.Load("instructions/errors/swipe_rotation_error") as AudioClip;
        errorClips[8] = Resources.Load("instructions/errors/rotation_angle_error") as AudioClip;
        errorClips[9] = Resources.Load("instructions/errors/hold_horizontal_error") as AudioClip;
        errorClips[10] = Resources.Load("instructions/errors/hold_vertical_error") as AudioClip;
        errorClips[11] = Resources.Load("instructions/errors/hold_rotation_error") as AudioClip;
        errorClips[12] = Resources.Load("instructions/errors/not_a_tap") as AudioClip;
        errorClips[13] = Resources.Load("instructions/errors/not_a_swipe_up") as AudioClip;
        errorClips[14] = Resources.Load("instructions/errors/not_a_hold") as AudioClip;
        errorClips[15] = Resources.Load("instructions/errors/not_a_swipe_down") as AudioClip;
        errorClips[16] = Resources.Load("instructions/errors/not_a_rotation") as AudioClip;
        errorClips[17] = Resources.Load("instructions/errors/not_a_left_right_swipe") as AudioClip;
        errorClips[18] = Resources.Load("instructions/errors/finger_offscreen") as AudioClip;

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
