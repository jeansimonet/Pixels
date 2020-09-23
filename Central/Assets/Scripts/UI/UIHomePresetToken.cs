using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;
using System.Linq;
using Dice;

public class UIHomePresetToken : MonoBehaviour
{
    [Header("Controls")]
    public Button mainButton;
    public RawImage presetRenderImage;
    public Text presetNameText;
    public Image backgroundImage;
    public RectTransform checkMarkRoot;
    public Image activePresetBorderImage;

    [Header("Parameters")]
    public Color textColor;
    public Color activeTextColor;
    public Sprite background;
    public Sprite activeBackground;

    public EditPreset editPreset { get; private set; }
    public MultiDiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;

    public enum State
    {
        Inactive = 0,
        Active
    }

    public State state {get; private set; }

    public void Setup(EditPreset preset)
    {
        this.editPreset = preset;
        var designs = new List<DesignAndColor>(preset.dieAssignments.Select(ass => (ass.die != null) ? ass.die.designAndColor : DesignAndColor.Unknown));

        this.dieRenderer = DiceRendererManager.Instance.CreateMultiDiceRenderer(designs, 400);
        if (dieRenderer != null)
        {
            presetRenderImage.texture = dieRenderer.renderTexture;
        }
        presetNameText.text = preset.name;

        dieRenderer.rotating = true;
        for (int i = 0; i < preset.dieAssignments.Count; ++i)
        {
            if (preset.dieAssignments[i].behavior != null)
            {
                dieRenderer.SetDieAnimations(i, preset.dieAssignments[i].behavior.CollectAnimations().Where(anim => anim != null));
                dieRenderer.Play(i, false);
            }
        }
        SetState(State.Inactive);
    }

    public void SetState(State newState)
    {
        state = newState;
        switch (newState)
        {
            case State.Inactive:
                backgroundImage.sprite = background;
                presetNameText.color = textColor;
                checkMarkRoot.gameObject.SetActive(false);
                activePresetBorderImage.gameObject.SetActive(false);
                break;            
            case State.Active:
                backgroundImage.sprite = activeBackground;
                presetNameText.color = activeTextColor;
                checkMarkRoot.gameObject.SetActive(true);
                activePresetBorderImage.gameObject.SetActive(true);
                break;            
        }
    }

    void OnDestroy()
    {
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
    }
}
