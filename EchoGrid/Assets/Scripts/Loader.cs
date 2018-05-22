using UnityEngine;
using System.Collections;

/// <summary>
/// A simple class to load the game and sound managers.
/// </summary>
public class Loader : MonoBehaviour
{

    public GameObject gameManager;
    public GameObject soundManager;

    // Use this for initialization
    void Awake()
    {
        if (GameManager.instance == null)
        {
            Instantiate(gameManager); // Instantiate GameManager prefab.
        }

        if (SoundManager.instance == null)
        {            
            Instantiate(soundManager); // Instantiate SoundManager prefab.
        }
    }

}
