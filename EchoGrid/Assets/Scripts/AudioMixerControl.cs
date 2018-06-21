using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerControl : MonoBehaviour
{
    public AudioMixer masterMixer;

    public static AudioMixerControl instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
		
	}
	
	// Update is called once per frame
	void Update()
    {
		
	}

    public void SetSound(float soundLevel)
    {
        masterMixer.SetFloat("echoesVolume", soundLevel);
    }

    public void SetDistortion(float distortionLevel)
    {
        masterMixer.SetFloat("echoesDistortion", distortionLevel);
    }
}
