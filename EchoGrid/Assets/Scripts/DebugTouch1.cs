using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTouch1 : MonoBehaviour
{
    Text debugTouch1Text;
    string touch1Text;

    public static DebugTouch1 instance;
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
        debugTouch1Text = GetComponent<Text>();
        touch1Text = "Touch1";
    }
	
	// Update is called once per frame
	void Update()
    {
        debugTouch1Text.text = touch1Text;
    }

    public void ChangeDebugTouch1Text(string textToChangeTo)
    {
        touch1Text = textToChangeTo;
    }
}
