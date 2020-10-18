using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animations;

public class UIGradientPatternViewToken : MonoBehaviour
{
    [Header("Controls")]
    public Button mainButton;
    public RawImage animRenderImage;
    public RawImage textureImage;
    public Text patternNameText;
    public Button menuButton;
    public Image menuButtonImage;
    public Canvas overrideCanvas;
    public Image backgroundImage;
    public RectTransform expandedRoot;
    public Button removeButton;
    public Button editButton;

    [Header("Properties")]
    public Sprite expandImage;
    public Sprite contractImage;
    public Color backgroundColor;
    public Color expandedColor;
    public Sprite backgroundSprite;
    public Sprite expandedSprite;


    public EditPattern editPattern { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;
    public Button.ButtonClickedEvent onRemove => removeButton.onClick;
    public Button.ButtonClickedEvent onEdit => editButton.onClick;
    public Button.ButtonClickedEvent onExpand => menuButton.onClick;

    public bool isExpanded => expandedRoot.gameObject.activeSelf;

    bool visible = true;

    public void Setup(EditPattern pattern)
    {
        this.editPattern = pattern;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(Dice.DesignAndColor.V5_Black);
        if (dieRenderer != null)
        {
            animRenderImage.texture = dieRenderer.renderTexture;
        }
        patternNameText.text = pattern.name;

        var anim = new EditAnimationKeyframed();
        anim.name = "temp anim";
        anim.pattern = pattern;
        anim.duration = pattern.duration;

        textureImage.texture = pattern.ToTexture();

        dieRenderer.SetAuto(true);
        dieRenderer.SetAnimation(anim);
        dieRenderer.Play(true);
        Expand(false);
    }

    public void Expand(bool expand)
    {
        if (expand)
        {
            menuButtonImage.sprite = contractImage;
            overrideCanvas.overrideSorting = true;
            backgroundImage.sprite = expandedSprite;
            backgroundImage.color = expandedColor;
            expandedRoot.gameObject.SetActive(true);
        }
        else
        {
            menuButtonImage.sprite = expandImage;
            overrideCanvas.overrideSorting = false;
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
