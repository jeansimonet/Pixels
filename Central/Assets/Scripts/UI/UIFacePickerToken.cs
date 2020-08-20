using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFacePickerToken : MonoBehaviour
{
    // Controls
    [Header("Controls")]
    public Text faceText;
    public Toggle mainToggle;

    public int faceIndex { get; private set; }
    public bool isOn => mainToggle.isOn;
    public Toggle.ToggleEvent onValueChanged => mainToggle.onValueChanged;

    public void Setup(int faceIndex, bool selected)
    {
        this.faceIndex = faceIndex;
        faceText.text = (faceIndex + 1).ToString();
        mainToggle.isOn = selected;
    }
}
