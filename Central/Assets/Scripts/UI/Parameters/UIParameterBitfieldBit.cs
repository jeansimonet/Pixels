using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIParameterBitfieldBit : MonoBehaviour
{
    [Header("Controls")]
    public Text faceText;
    public Toggle mainToggle;
    public Image backgroundImage;
    public Image toggleImage;

    public bool isOn => mainToggle.isOn;
    public Toggle.ToggleEvent onValueChanged => mainToggle.onValueChanged;

    public void Setup(string text, bool on, Sprite backgroundSprite, Color color, Color colorSelected)
    {
        faceText.text = text;
        backgroundImage.sprite = backgroundSprite;
        backgroundImage.color = color;
        toggleImage.sprite = backgroundSprite;
        toggleImage.color = colorSelected;
        mainToggle.isOn = on;
    }
}
