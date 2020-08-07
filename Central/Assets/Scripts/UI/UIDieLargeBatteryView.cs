using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDieLargeBatteryView : MonoBehaviour
{
    [Header("Controls")]
    public Image batteryImage;
    public Text batteryLevelText;

    [Header("Properties")]
    public Sprite[] batteryLevelImages;
    public float[] batteryLevels;
    public Sprite notAvailableImage;

    public void SetLevel(float level)
    {
        // Find the first keyframe
        int index = 0;
        while (index < batteryLevels.Length && batteryLevels[index] > level) {
            index++;
        }

        var sprite = batteryLevelImages[index];
        batteryImage.sprite = sprite;

        batteryLevelText.text = level.ToString("P0");
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
