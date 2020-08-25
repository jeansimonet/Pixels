using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRotationControl : MonoBehaviour
{
    public Color autoColor;
    public Color idleColor;
    public Image buttonBackgroundImage;
    public Button button;

    DiceRendererDice die;

    // Start is called before the first frame update
    public void Setup(DiceRendererDice die)
    {
        this.die = die;
        die.onRotationStateChange += OnRotationStateChange;
        OnRotationStateChange(die.rotationState);
    }

    void Awake()
    {
        button.onClick.AddListener(ToggleAuto);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnRotationStateChange(DiceRendererDice.RotationState newState)
    {
        switch (newState)
        {
            case DiceRendererDice.RotationState.Auto:
                buttonBackgroundImage.color = autoColor;
                break;
            default:
                buttonBackgroundImage.color = idleColor;
                break;
        }
    }

    void ToggleAuto()
    {
        switch (die.rotationState)
        {
            case DiceRendererDice.RotationState.Auto:
                die.SetAuto(false);
                break;
            default:
                die.SetAuto(true);
                break;
        }
    }
}
