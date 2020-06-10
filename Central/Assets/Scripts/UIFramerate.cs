using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFramerate : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        // Make the game run as fast as possible
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
