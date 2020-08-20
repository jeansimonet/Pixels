using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIEnumPicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public Button saveButton;
    public InfiniWheel wheel;
    public LayoutElement wheelElement;
    public float elementHeight = 140.0f;

    System.Enum currentValue;
    System.Enum newValue;
    System.Action<bool, System.Enum> closeAction;

    public bool isShown => gameObject.activeSelf;

    /// <summary>
    /// Invoke the die picker
    /// </sumary>
    public void Show(string title, System.Enum previousValue, System.Action<bool, System.Enum> closeAction, int min, int max)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Die picker still active");
            ForceHide();
        }

        gameObject.SetActive(true);
        currentValue = previousValue;
        titleText.text = title;
        this.closeAction = closeAction;

        var enumType = previousValue.GetType();
        List<string> enumValueNames;
        System.Array enumValues;

        int count = max - min + 1;
        enumValues = System.Array.CreateInstance(typeof(object), count);
        System.Array.Copy(System.Enum.GetValues(enumType), min, enumValues, 0, count);
        enumValueNames = System.Enum.GetNames(enumType).ToList().GetRange(min, count);

        int visibleItemCount = Mathf.Clamp((enumValueNames.Count-1)|1, 1, 9);
        wheel.itemCount = visibleItemCount;
        wheelElement.minHeight = visibleItemCount * elementHeight;
        wheel.SetData(enumValueNames.ToArray());
        wheel.Select(System.Array.IndexOf(enumValues, previousValue));

        wheel.ValueChange += (index, text) =>
        {
            if (index >= 0 && index < enumValues.Length)
            {
                newValue = (System.Enum)enumValues.GetValue(index);
                saveButton.gameObject.SetActive(true);
            }
        };

        saveButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false, currentValue);
    }

    void Awake()
    {
        backButton.onClick.AddListener(DiscardAndBack);
        saveButton.onClick.AddListener(SaveAndBack);
    }

    void Hide(bool result, System.Enum value)
    {
        gameObject.SetActive(false);
        closeAction?.Invoke(result, value);
        closeAction = null;
    }

    void DiscardAndBack()
    {
        Hide(false, currentValue);
    }

    void SaveAndBack()
    {
        Hide(true, newValue);
    }
}
