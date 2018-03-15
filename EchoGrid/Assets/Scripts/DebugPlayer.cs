using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPlayer : MonoBehaviour
{
    Text debugPlayerText;
    string playerText;

    public static DebugPlayer instance;
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

        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        debugPlayerText = GetComponent<Text>();
        playerText = "Player";
    }
	
	// Update is called once per frame
	void Update()
    {
        debugPlayerText.text = playerText;
    }

    public void ChangeDebugPlayerText(string textToChangeTo)
    {
        playerText = textToChangeTo;
    }
}
