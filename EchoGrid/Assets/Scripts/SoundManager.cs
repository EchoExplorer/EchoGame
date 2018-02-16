﻿using UnityEngine;
using System.Collections;

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
                    efxSource[i].volume = 0.5f;
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
                voiceSource.volume = 0.1f;
                //voiceSource.pitch = 0.9f;
                voice_adjusted = true;
            }
            else
                voice_adjusted = false;

            if (crashSource != null)
            {
                crashSource.volume = 0.1f;
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
	public void PlayEcho(AudioClip clip)
    {
        if (!echoSource.isPlaying)
        {
            echoSource.clip = clip;
            //Play the clip.
            echoSource.Play();
            return;
        }
    }

    /// <summary>
    /// Plays an instruction voice.
    /// </summary>
    public bool PlayVoice(AudioClip clip, bool reset = false)
    {
        if ((voiceSource.isPlaying == false) || reset)
        {
            voiceSource.clip = clip;
            //Play the clip.
            voiceSource.Play();
            return true;
        }

        return false;
    }
}
