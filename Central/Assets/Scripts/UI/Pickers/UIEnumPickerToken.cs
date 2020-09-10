using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnumPickerToken : MonoBehaviour
{
    public Button mainButton;
    public Image selectedImage;
    public Image selectedCheckmark;
    public Text label;
    public Color selectedTextColor;
    public Color textColor;

    public delegate void EnumSelectedEvent(string enumString, System.Enum enumValue);
    public EnumSelectedEvent onEnumSelected;

    public string enumString => label.text;
    public System.Enum enumValue;

    public void Setup(string enumString, System.Enum value)
    {
        label.text = enumString;
        enumValue = value;

        mainButton.onClick.AddListener(() => onEnumSelected?.Invoke(this.enumString, this.enumValue));
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            selectedImage.gameObject.SetActive(true);
            selectedCheckmark.gameObject.SetActive(true);
            label.color = selectedTextColor;
            selectedCheckmark.color = selectedTextColor;
        }
        else
        {
            selectedImage.gameObject.SetActive(false);
            selectedCheckmark.gameObject.SetActive(false);
            label.color = textColor;
            selectedCheckmark.color = textColor;
        }
    }
}
