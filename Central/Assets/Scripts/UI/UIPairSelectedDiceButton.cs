using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPairSelectedDiceButton : MonoBehaviour
{
    [Header("Controls")]
    public Image background;
    public Text text;

    [Header("Parameters")]
    public Color activeBackgroundColor;
    public Color activeTextColor;
    public Color inactiveBackgroundColor;
    public Color inactiveTextColor;

    public Button.ButtonClickedEvent onClick => GetComponent<Button>().onClick;

    public void SetActive(bool active)
    {
        background.color = active ? activeBackgroundColor : inactiveBackgroundColor;
        text.color = active ? activeTextColor : inactiveTextColor;
    }
}
