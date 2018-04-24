using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// A central manager for playing voice instrctions, certain sound effects and echo
/// sounds (provided as ``AudioClip`` instances).
/// </summary>
public class SoundManager : MonoBehaviour
{

    public AudioSource[] efxSource;                 // Drag a reference to the audio source which will play the sound effects.
    public AudioSource voiceSource;
    public AudioSource clipSource;
    public AudioSource echoSource;
    public AudioSource crashSource;
    public AudioSource singleSource;
    public static SoundManager instance = null;     // Allows other scripts to call functions from SoundManager.				
    int max_sfx_playing = 5;
    bool voice_adjusted = false;

    public bool finishedClip = true; // Determines if we have finished the clip we wanted to play.
    public bool finishedAllClips = true; // Determines if we have gone through all the clips in our list.

    public static List<AudioClip> clipsCurrentlyPlaying;
    public static float[] currentBalances;
    public static Action currentCallback;
    public static int currentCallbackIndex;
    public static float currentVolume;

    void Awake()
    {
        if (instance == null)
        {
            // If not, set it to this.
            instance = this;
        }
        // If instance already exists:
        else if (instance != this)
        {
            // Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
            Destroy(gameObject);
        }

        // Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        // efxSource = new AudioSource[max_sfx_playing];
        // for (int i = 0; i < max_sfx_playing; ++i)
        // {
        //	 efxSource[i] = new AudioSource();
        // }
        // voiceSource = new AudioSource();
        // voiceSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Sets various parameters for the audio clips every frame.
    /// Retries upon failure until the first successful run.
    /// </summary>
    void Update()
    {
        if (!voice_adjusted)
        {
            for (int i = 0; i < efxSource.Length; ++i)
            {
                if (efxSource[i] != null)
                {
                    efxSource[i].volume = 1f;
                    voice_adjusted = true;
                }
                else
                {
                    voice_adjusted = false;
                    return;
                }
            }
            if (voiceSource != null)
            {
                voiceSource.volume = 1f;
                // voiceSource.pitch = 0.9f;
                voice_adjusted = true;
            }
            else
            {
                voice_adjusted = false;
            }

            if (clipSource != null)
            {
                clipSource.volume = 1f;
                voice_adjusted = true;
            }
            else
            {
                voice_adjusted = false;
            }

            if (crashSource != null)
            {
                crashSource.volume = 1f;
                voice_adjusted = true;
            }
            else
            {
                voice_adjusted = false;
            }
        }
    }

    /// <summary>
    /// Plays an arbitrary audio clip.
    /// </summary>
	public void PlaySingle(AudioClip clip)
    {
        singleSource.clip = clip;
        singleSource.Play();
        return;
        // Set the clip of our efxSource audio source to the clip passed in as a parameter.
        for (int i = 0; i < max_sfx_playing; ++i)
        {
            if (!efxSource[i].isPlaying)
            {
                efxSource[i].clip = clip;
                // Play the clip.
                efxSource[i].Play();
            }
        }
    }

    /// <summary>
    /// Plays the crash sound effect.
    /// </summary>
	public void playcrash(AudioClip clip)
    {
        crashSource.clip = clip;
        crashSource.Play();
    }

    /// <summary>
    /// Plays an echo sound.
    /// </summary>
	public void PlayEcho(AudioClip echoClip, Action callback = null)
    {
        echoSource.clip = Database.soundEffectClips[1];
        echoSource.Play();
        StartCoroutine(EchoWait(echoSource.clip.length, echoClip, callback));
    }

    private IEnumerator EchoWait(float waitLength, AudioClip echoClip, Action callback)
    {
        yield return new WaitForSeconds(waitLength);
        if (echoClip != null)
        {
            echoSource.clip = echoClip;
            echoSource.Play();
            StartCoroutine(EchoWait(echoSource.clip.length, null, callback));
        }
        else if (callback != null)
        {
            callback();
        }
    }

    /// <summary>
    /// Plays an instruction voice.
    /// </summary>
	public bool PlayVoice(AudioClip clip, bool reset = false, float balance = 0, float delay = 0.0f)
    {
        if ((voiceSource.isPlaying == false) || reset)
        {
            finishedClip = false;
            voiceSource.clip = clip;
            // Set balance (-1 to 1 from left to right, default 0)
            voiceSource.panStereo = balance;
            float clipLength = voiceSource.clip.length;
            // Play the clip.
            voiceSource.PlayDelayed(delay);
            StartCoroutine(WaitForVoice(clipLength, clip));
            return true;
        }

        return false;
    }

    private IEnumerator WaitForVoice(float clipLength, AudioClip clip)
    {
        yield return new WaitForSeconds(clipLength + 0.3f);
        if ((voiceSource.time == clipLength) || (voiceSource.time == 0.0f))
        {
            finishedClip = true;
        }
    }

    /// <summary>
    /// Plays an instruction voice.
    /// </summary>
	public bool PlayClip(AudioClip clip, bool reset = false, float balance = 0, float volume = 1.0f)
    {
        if ((clipSource.isPlaying == false) || reset)
        {
            clipSource.clip = clip;
            clipSource.volume = volume;
            // Set balance (-1 to 1 from left to right, default 0)
            clipSource.panStereo = balance;
            // Play the clip.
            clipSource.Play();
            return true;
        }

        return false;
    }

    // Play a list of clips in their order with 0.5 seconds pausing. Callback function and its index allowed.
    public void PlayClips(List<AudioClip> clips, float[] balances = null, int current = 0, Action callback = null, int callback_index = 0, float volume = 1.0f, bool isFirstClip = true)
    {
        // If this clip is the first clip in our list.
        if (isFirstClip == true)
        {
            finishedAllClips = false; // We have not finished all our clips yet.
        }

        clipsCurrentlyPlaying = new List<AudioClip>() { };
        clipsCurrentlyPlaying.Clear();
        currentBalances = new float[clips.Count];
        int i = 0;
        for (int j = current; j < clips.Count; j++)
        {
            clipsCurrentlyPlaying.Add(clips[j]);
            if (balances != null)
            {
                currentBalances[i] = balances[j];
            }
            else
            {
                currentBalances = balances;
            }
            i++;
        }

        currentVolume = volume;
        currentCallback = callback;
        if (callback_index != 0)
        {
            currentCallbackIndex = callback_index - current;
        }

        if (current == callback_index && callback != null)
        {
            callback();
        }
        AudioClip clip = clips[current];
        float clipLength = clip.length;


        if (balances == null)
        {
            PlayClip(clip, true, 0, volume);
        }
        else
        {
            PlayClip(clip, true, balances[current], volume);
        }

        StartCoroutine(WaitForLength(clipLength, clips, balances, current, callback, callback_index, volume));
    }

    private IEnumerator WaitForLength(float clipLength, List<AudioClip> clips, float[] balances, int current, Action callback, int callback_index, float volume)
    {
        yield return new WaitForSeconds(clipLength + 0.3f);
        // Check if this clip is the last clip in the list and make sure this clip has finished playing.
        if (((current + 1) == clips.Count) && (!clipSource.isPlaying))
        {
            finishedAllClips = true; // We have played all clips in the list.
        }
        else if (current + 1 < clips.Count && !clipSource.isPlaying && clipSource.clip == clips[current])
        {
            PlayClips(clips, balances, current + 1, callback, callback_index, volume, false);
        }
        if (callback_index >= clips.Count && current >= clips.Count - 1 && callback != null)
        {
            callback();
        }
    }
}
