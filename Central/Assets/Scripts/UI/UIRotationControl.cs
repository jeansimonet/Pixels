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

    SingleDiceRenderer diceRenderer;

    // Start is called before the first frame update
    public void Setup(SingleDiceRenderer diceRenderer)
    {
        this.diceRenderer = diceRenderer;
        diceRenderer.die.onRotationStateChange += OnRotationStateChange;
        OnRotationStateChange(diceRenderer.die.rotationState);
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
        switch (diceRenderer.die.rotationState)
        {
            case DiceRendererDice.RotationState.Auto:
                diceRenderer.die.SetAuto(false);
                break;
            default:
                diceRenderer.die.SetAuto(true);
                diceRenderer.ResetTilt();
                break;
        }
    }
}
