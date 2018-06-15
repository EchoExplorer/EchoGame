using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTouch0 : MonoBehaviour
{
    Text debugTouch0Text;
    string touch0Text;

    public static DebugTouch0 instance;
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
        debugTouch0Text = GetComponent<Text>();
        touch0Text = "Touch0";
        //instance.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update()
    {
        debugTouch0Text.text = touch0Text;
    }

    public void ChangeDebugTouch0Text(string textToChangeTo)
    {
        touch0Text = textToChangeTo;
    }
}
