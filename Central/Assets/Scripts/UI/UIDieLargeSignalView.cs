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

    public void SetRssi(int? rssi)
    {
        if (rssi.HasValue)
        {
            // Find the first keyframe
            int index = 0;
            while (index < signalLevels.Length && signalLevels[index] > rssi.Value) {
                index++;
            }

            var sprite = signalLevelImages[index];
            signalImage.sprite = sprite;

            signalLevelText.text = rssi.Value.ToString("D0") + " dBm";
        }
        else
        {
            int index = signalLevels.Length - 1;
            var sprite = signalLevelImages[index];
            signalImage.sprite = sprite;

            signalLevelText.text = "Unknown";
        }
    }
}
