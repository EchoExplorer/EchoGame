using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTouch2 : MonoBehaviour
{
    Text debugTouch2Text;
    string touch2Text;
    bool usingTalkback = true;

    public static DebugTouch2 instance;
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
    void Start ()
    {
        debugTouch2Text = GetComponent<Text>();
        touch2Text = "Touch2";
        //instance.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        debugTouch2Text.text = touch2Text;
    }

    public void ChangeDebugTouch2Text(string textToChangeTo)
    {
        touch2Text = textToChangeTo;
    }
}
