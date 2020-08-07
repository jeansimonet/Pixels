using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorPicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public UIColorWheel colorWheel;
    public UIColorWheelSelection colorWheelSelection;
    public Button whiteButton;
    public Button brightButton;
    public Button dimButton;
    public Button blackButton;
    public Image whiteButtonSelection;
    public Image blackButtonSelection;

    [Header("Parameters")]
    public float valueDimColors = 0.35f;

    public bool isShown => gameObject.activeSelf;

    Color currentColor;
    System.Action<bool, Color> closeAction;

    /// <summary>
    /// Invoke the color picker
    /// </sumary>
    public void Show(string title, Color previousColor, System.Action<bool, Color> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Color picker still active");
            ForceHide();
        }

        gameObject.SetActive(true);
        currentColor = previousColor;
        titleText.text = title;

        float hue, sat, val;
        Color.RGBToHSV(previousColor, out hue, out sat, out val);
        const float valueEpsilon = 0.01f;
        if (previousColor == Color.white)
        {
            colorWheel.colorValue = 1.0f;
            colorWheelSelection.selectedHueIndex = -1;
            colorWheelSelection.selectedSatIndex = -1;
            whiteButtonSelection.gameObject.SetActive(true);
            blackButtonSelection.gameObject.SetActive(false);
        }
        else if (previousColor == Color.black)
        {
            colorWheel.colorValue = 1.0f;
            colorWheelSelection.selectedHueIndex = -1;
            colorWheelSelection.selectedSatIndex = -1;
            whiteButtonSelection.gameObject.SetActive(false);
            blackButtonSelection.gameObject.SetActive(true);
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
            whiteButtonSelection.gameObject.SetActive(false);
            blackButtonSelection.gameObject.SetActive(false);
        }

        this.closeAction = closeAction;
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false, currentColor);
    }

    void Awake()
    {
        backButton.onClick.AddListener(Back);
        colorWheel.onClicked += OnColorWheelClicked;
        whiteButton.onClick.AddListener(() => Hide(true, Color.white));
        blackButton.onClick.AddListener(() => Hide(true, Color.black));
        brightButton.onClick.AddListener(() => SwitchColorWheel(1.0f));
        dimButton.onClick.AddListener(() => SwitchColorWheel(valueDimColors));
    }

    void Hide(bool result, Color color)
    {
        gameObject.SetActive(false);
        closeAction?.Invoke(result, color);
        closeAction = null;
    }

    void Back()
    {
        Hide(false, currentColor);
    }

    void OnColorWheelClicked(Color color, int hueIndex, int saturationIndex)
    {
        Hide(true, color);
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
        colorWheelSelection.selectedHueIndex = selectedHueIndex;
        colorWheelSelection.selectedSatIndex = selectedSatIndex;
    }

}
