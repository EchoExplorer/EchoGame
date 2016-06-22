using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	public AudioSource efxSource;					//Drag a reference to the audio source which will play the sound effects.
	public static SoundManager instance = null;		//Allows other scripts to call functions from SoundManager.				

	void Awake ()
	{
		if (instance == null)
			//if not, set it to this.
			instance = this;
		//If instance already exists:
		else if (instance != this)
			//Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
			Destroy (gameObject);
		
		//Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
		efxSource = GetComponent<AudioSource>();
		DontDestroyOnLoad (gameObject);
	}
	
	
	//Used to play single sound clips.
	public void PlaySingle(AudioClip clip)
	{
		//Set the clip of our efxSource audio source to the clip passed in as a parameter.
		efxSource.clip = clip;
		
		//Play the clip.
		efxSource.Play ();
	}

	public bool isBusy()
	{
		return efxSource.isPlaying;
	}
	
}
