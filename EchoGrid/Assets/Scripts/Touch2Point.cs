using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch2Point : MonoBehaviour {

    GameObject touch2Point;

    public static Touch2Point instance;
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
