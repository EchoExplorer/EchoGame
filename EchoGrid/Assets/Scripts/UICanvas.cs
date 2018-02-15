using UnityEngine;
using System.Collections;

/// <summary>
/// A class simply designed to maintain a singleton.
///  All the useful components of the UI are accessed through the parent
///  ``GameObject`` of the instance.
/// </summary>
public class UICanvas : MonoBehaviour
{

    public static UICanvas instance;

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnLevelWasLoaded(int index)
    {
        // Since the gameObject is not destroyed automatically, the instance should be checked before calling this method.
        if (this != instance) return;
        //Call InitGame to initialize our level.
        //transform.FindChild("LevelImage").gameObject.SetActive(true);
    }
}
