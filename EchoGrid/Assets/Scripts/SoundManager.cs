using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// A central manager for playing voice instrctions, certain sound effects and echo
///  sounds (provided as ``AudioClip`` instances).
/// </summary>
public class SoundManager : MonoBehaviour
{

    public AudioSource[] efxSource;                 //Drag a reference to the audio source which will play the sound effects.
    public AudioSource voiceSource;
    public AudioSource echoSource;
    public AudioSource crashSource;
    public static SoundManager instance = null;     //Allows other scripts to call functions from SoundManager.				
    int max_sfx_playing = 5;
    bool voice_adjusted = false;

    void Awake()
    {
        if (instance == null)
            //if not, set it to this.
            instance = this;
        //If instance already exists:
        else if (instance != this)
            //Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
            Destroy(gameObject);

        //Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        //efxSource = new AudioSource[max_sfx_playing];
        //for (int i = 0; i < max_sfx_playing; ++i) {
        //	efxSource[i] = new AudioSource();
        //}
        //voiceSource = new AudioSource();
        //voiceSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Sets various parameters for the audio clips every frame.
    ///  Retries upon failure until the first successful run.
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
                //voiceSource.pitch = 0.9f;
                voice_adjusted = true;
            }
            else
                voice_adjusted = false;

            if (crashSource != null)
            {
                crashSource.volume = 1f;
                voice_adjusted = true;
            }
            else
                voice_adjusted = false;
        }
    }

    /// <summary>
    /// Plays an arbitrary audio clip.
    /// </summary>
	public void PlaySingle(AudioClip clip)
    {
        //Set the clip of our efxSource audio source to the clip passed in as a parameter.
        for (int i = 0; i < max_sfx_playing; ++i)
        {
            if (!efxSource[i].isPlaying)
            {
                efxSource[i].clip = clip;
                //Play the clip.
                efxSource[i].Play();
                return;
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
        echoSource.clip = Database.instance.TitletoMainClips[0];
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
	public bool PlayVoice(AudioClip clip, bool reset = false, float balance = 0)
    {
        if ((voiceSource.isPlaying == false) || reset)
        {
            voiceSource.clip = clip;
			// Set balance (-1 to 1 from left to right, default 0)
			voiceSource.panStereo = balance;
			//Play the clip.
            voiceSource.Play();
            return true;
        }

        return false;
    }

    // Play a list of clips in their order with 0.5 seconds pausing. Callback function and its index allowed.
    public void PlayClips(List<AudioClip> clips, int current = 0, Action callback = null, int callback_index = 0)
    {
        if (current == callback_index && callback != null)
        {
            callback();
        }
        AudioClip clip = clips[current];
        float clipLength = clip.length;
        PlayVoice(clip, true);
        StartCoroutine(WaitForLength(clipLength, clips, current, callback, callback_index));
    }

    private IEnumerator WaitForLength(float clipLength, List<AudioClip> clips, int current, Action callback, int callback_index)
    {
        yield return new WaitForSeconds(clipLength + 0.3f);
        if (current + 1 < clips.Count && !voiceSource.isPlaying && voiceSource.clip == clips[current])
            PlayClips(clips, current + 1, callback, callback_index);
        if (callback_index >= clips.Count && current >= clips.Count - 1 && callback != null)
        {
            callback();
        }
    }
}
