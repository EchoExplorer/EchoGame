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
    }

    // Use this for initialization
    void Start()
    {
        debugPlayerText = GetComponent<Text>();
        playerText = "Player";
        //instance.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update()
    {
        debugPlayerText.text = playerText;
    }

    // Change the text of DebugPlayer's textbox.
    public void ChangeDebugPlayerText(string textToChangeTo)
    {
        playerText = textToChangeTo;
    }
}
