using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientImage : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Screen.autorotateToPortrait = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;
    }

    // Update is called once per frame
    void Update () {
        switch (Screen.orientation)
        {
            case ScreenOrientation.LandscapeLeft:
                transform.rotation = Quaternion.Euler(0, 0, 180.0f);
                break;
            case ScreenOrientation.LandscapeRight:
                transform.rotation = Quaternion.identity;
                break;
            default:
                //Nothing
                break;
        }
		
	}
}
