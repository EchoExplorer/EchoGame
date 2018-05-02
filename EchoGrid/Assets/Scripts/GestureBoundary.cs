using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureBoundary : MonoBehaviour {

    GameObject gestureBoundary;

    public static GestureBoundary instance;
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

    }

    // Update is called once per frame
    void Update()
    {

    }
}
