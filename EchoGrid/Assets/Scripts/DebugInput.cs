using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugInput : MonoBehaviour
{
    Text debugInputText;
    string inputText;

    public static DebugInput instance;
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
        debugInputText = GetComponent<Text>();
        inputText = "Input";
        instance.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        debugInputText.text = inputText;
	}

    public void ChangeDebugInputText(string textToChangeTo)
    {
        inputText = textToChangeTo;
    }
}
