using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTouchDuration : MonoBehaviour
{
    Text debugTouchDurationText;
    string touchDurationText;

    public static DebugTouchDuration instance;
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
        debugTouchDurationText = GetComponent<Text>();
        touchDurationText = "Duration";
    }
	
	// Update is called once per frame
	void Update()
    {
        debugTouchDurationText.text = touchDurationText;
    }

    public void ChangeDebugTouchDurationText(string textToChangeTo)
    {
        touchDurationText = textToChangeTo;
    }
}
