using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;
using Dice;

public class UIDiePickerDieToken : MonoBehaviour
{
    // Controls
    [Header("Controls")]
    public UIPairedDieView dieView;
    public Button mainButton;
    public Image frame;

    [Header("Parameters")]
    public Color defaultFrameColor;
    public Color selectedColor;

    public EditDie die => dieView.die;
    public SingleDiceRenderer dieRenderer { get; private set; }
    public bool selected { get; private set; }

    bool visible = true;

    public Button.ButtonClickedEvent onClick => mainButton.onClick;

    public void Setup(EditDie die)
    {
        dieView.Setup(die);
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        if (selected)
        {
            frame.color = selectedColor;
        }
        else
        {
            frame.color = defaultFrameColor;
        }
        dieView.SetSelected(selected);
    }

    void Update()
    {
        bool newVisible = GetComponent<RectTransform>().IsVisibleFrom();
        if (newVisible != visible)
        {
            visible = newVisible;
            DiceRendererManager.Instance.OnDiceRendererVisible(dieRenderer, visible);
        }
    }

}
