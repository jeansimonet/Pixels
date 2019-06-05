using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CaptureVideo
    : MonoBehaviour
{
   
	// Use this for initialization
	void Start ()
    {
        foreach (var dev in WebCamTexture.devices)
        {
            Debug.Log(dev.name);
        }
        var cam = WebCamTexture.devices[0];
        WebCamTexture tex = new WebCamTexture(cam.name, 1920, 1080, 30);
        tex.Play();

		GetComponent<RawImage>().texture = tex;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
