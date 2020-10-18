using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;
using System.Linq;
using Dice;

public class UIPresetToken : MonoBehaviour
{
    [Header("Controls")]
    public Button mainButton;
    public RawImage presetRenderImage;
    public Text presetNameText;
    public Text presetDescriptionText;
    public Button menuButton;
    public Image menuButtonImage;
    public Image backgroundImage;
    public RectTransform expandedRoot;
    public Button removeButton;
    public Button duplicateButton;
    public Button editButton;

    [Header("Properties")]
    public Sprite expandImage;
    public Sprite contractImage;
    public Color backgroundColor;
    public Color expandedColor;
    public Sprite backgroundSprite;
    public Sprite expandedSprite;

    public EditPreset editPreset { get; private set; }
    public MultiDiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;
    public Button.ButtonClickedEvent onRemove => removeButton.onClick;
    public Button.ButtonClickedEvent onDuplicate => duplicateButton.onClick;
    public Button.ButtonClickedEvent onEdit => editButton.onClick;
    public Button.ButtonClickedEvent onExpand => menuButton.onClick;


    public bool isExpanded => expandedRoot.gameObject.activeSelf;

    bool visible = true;

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
        presetDescriptionText.text = preset.description;

        dieRenderer.rotating = true;
        for (int i = 0; i < preset.dieAssignments.Count; ++i)
        {
            if (preset.dieAssignments[i].behavior != null)
            {
                dieRenderer.SetDieAnimations(i, preset.dieAssignments[i].behavior.CollectAnimations().Where(anim => anim != null));
                dieRenderer.Play(i, false);
            }
        }
        Expand(false);
    }

    public void Expand(bool expand)
    {
        if (expand)
        {
            menuButtonImage.sprite = contractImage;
            backgroundImage.sprite = expandedSprite;
            backgroundImage.color = expandedColor;
            expandedRoot.gameObject.SetActive(true);
        }
        else
        {
            menuButtonImage.sprite = expandImage;
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = backgroundColor;
            expandedRoot.gameObject.SetActive(false);
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
