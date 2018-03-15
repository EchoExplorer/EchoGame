using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugCross : MonoBehaviour
{
    Text debugCrossText;
    string crossText;

    public static DebugCross instance;
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
        debugCrossText = GetComponent<Text>();
        crossText = "VS" + "\n" + "VE" + "\n" + "CrossZ";
    }
	
	// Update is called once per frame
	void Update()
    {
        debugCrossText.text = crossText;
    }

    public void ChangeDebugCrossText(string textToChangeTo)
    {
        crossText = textToChangeTo;
    }
}
