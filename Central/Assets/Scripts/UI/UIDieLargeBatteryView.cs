using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDieLargeBatteryView : MonoBehaviour
{
    [Header("Controls")]
    public Image batteryImage;
    public Text batteryLevelText;
    public Text batteryNiceText;
    public Image chargingImage;

    [Header("Properties")]
    public Sprite[] batteryLevelImages;
    public float[] batteryLevels;
    public Sprite notAvailableImage;

    public void SetLevel(float? level, bool? charging)
    {
        if (level.HasValue)
        {
            // Find the first keyframe
            int index = 0;
            while (index < batteryLevels.Length && batteryLevels[index] > level.Value) {
                index++;
            }

            var sprite = batteryLevelImages[index];
            batteryImage.sprite = sprite;
            batteryLevelText.text = level.Value.ToString("P0");
        }
        else
        {
            int index = batteryLevels.Length-1;
            var sprite = batteryLevelImages[index];
            batteryImage.sprite = sprite;
            batteryLevelText.text = "Unknown";
            batteryNiceText.text = "Battery";
        }
        if (charging.HasValue && charging.Value)
        {
            chargingImage.gameObject.SetActive(true);
            batteryNiceText.text = "Charging";
        }
        else
        {
            chargingImage.gameObject.SetActive(false);
            if (level.HasValue && Mathf.RoundToInt(level.Value * 100.0f) == 69)
            {
                batteryNiceText.text = "Nice.";
            }
            else
            {
                batteryNiceText.text = "Battery";
            }
        }
    }
}
