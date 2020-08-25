using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIColorEditor : MonoBehaviour
{
    [Header("Controls")]
    public UIColorWheel colorWheel;
    public UIColorWheelSelection colorWheelSelection;
    public Button dimButton;
    public Button brightButton;
    public List<UIColorButton> colorButtons;

    [Header("Parameters")]
    public float valueDimColors = 0.35f;

    public delegate void ColorSelectedEvent(Color color);
    public ColorSelectedEvent onColorSelected;

    Color currentColor;

    /// <summary>
    /// Invoke the color picker
    /// </sumary>
    public void SelectColor(Color previousColor)
    {

        gameObject.SetActive(true);
        currentColor = previousColor;

        float hue, sat, val;
        Color.RGBToHSV(previousColor, out hue, out sat, out val);
        const float valueEpsilon = 0.01f;
        var btn = colorButtons.FirstOrDefault(b => b.color == previousColor);
        if (btn != null)
        {
            colorWheel.colorValue = 1.0f;
            colorWheelSelection.SetSelection(Color.black, -1, -1);
            SetSelectedColorButton(btn);
        }
        else
        {
            if (Mathf.Abs(valueDimColors - val) < valueEpsilon)
            {
                SwitchColorWheel(valueDimColors);
            }
            else
            {
                // Any other case, initialize the color wheel to the bright one
                SwitchColorWheel(1.0f);
            }
            SetSelectedColorButton(null);
        }
    }

    public void ClearColorSelection()
    {
        // Any other case, initialize the color wheel to the bright one
        SwitchColorWheel(1.0f);
        SetSelectedColorButton(null);
        colorWheelSelection.SetSelection(Color.black, -1, -1);
    }

    void Awake()
    {
        colorWheel.onClicked += OnColorWheelClicked;
        foreach (var btn in colorButtons)
        {
            btn.onClick.AddListener(() => onColorSelected?.Invoke(btn.color));
        }
        brightButton.onClick.AddListener(() => SwitchColorWheel(1.0f));
        dimButton.onClick.AddListener(() => SwitchColorWheel(valueDimColors));
    }

    void OnColorWheelClicked(Color color, int hueIndex, int saturationIndex)
    {
        onColorSelected?.Invoke(color);
    }

    void SwitchColorWheel(float value)
    {
        colorWheel.colorValue = value;
        int selectedHueIndex = -1;
        int selectedSatIndex = -1;
        if (!colorWheel.FindColor(currentColor, out selectedHueIndex, out selectedSatIndex))
        {
            selectedHueIndex = -1;
            selectedSatIndex = -1;
        }
        colorWheelSelection.SetSelection(currentColor, selectedHueIndex, selectedSatIndex);
    }

    void SetSelectedColorButton(UIColorButton btn)
    {
        foreach (var b in colorButtons)
        {
            b.SetSelected(b == btn);
        }
    }


}
