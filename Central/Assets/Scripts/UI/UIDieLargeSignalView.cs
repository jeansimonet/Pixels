using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDieLargeSignalView : MonoBehaviour
{
    [Header("Controls")]
    public Image signalImage;
    public Text signalLevelText;

    [Header("Properties")]
    public Sprite[] signalLevelImages;
    public float[] signalLevels;
    public Sprite notAvailableImage;

    public void SetRssi(int rssi)
    {
        // Find the first keyframe
        int index = 0;
        while (index < signalLevels.Length && signalLevels[index] > rssi) {
            index++;
        }

        var sprite = signalLevelImages[index];
        signalImage.sprite = sprite;

        signalLevelText.text = rssi.ToString("D0") + " dBm";
    }

    public void SetAvailable(bool available)
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
