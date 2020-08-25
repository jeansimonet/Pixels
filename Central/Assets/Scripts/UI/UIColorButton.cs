using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorButton : MonoBehaviour
{
    public Button mainButton;
    public Image colorImage;
    public Image Selection;
    public Color color => colorImage.color;
    public Button.ButtonClickedEvent onClick => mainButton.onClick;

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            Selection.gameObject.SetActive(true);
        }
        else
        {
            Selection.gameObject.SetActive(false);
        }
    }
}
