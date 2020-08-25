using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animations;

public class UIPatternToken : MonoBehaviour
{
    [Header("Controls")]
    public Button mainButton;
    public RawImage animRenderImage;
    public Text animNameText;
    public Button menuButton;
    public Image menuButtonImage;
    public Canvas overrideCanvas;
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


    public EditAnimation editAnimation { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;
    public Button.ButtonClickedEvent onRemove => removeButton.onClick;
    public Button.ButtonClickedEvent onDuplicate => duplicateButton.onClick;
    public Button.ButtonClickedEvent onEdit => editButton.onClick;
    public Button.ButtonClickedEvent onExpand => menuButton.onClick;

    public bool isExpanded => expandedRoot.gameObject.activeSelf;

    public void Setup(EditAnimation anim)
    {
        this.editAnimation = anim;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(anim.defaultPreviewSettings.design);
        if (dieRenderer != null)
        {
            animRenderImage.texture = dieRenderer.renderTexture;
        }
        animNameText.text = anim.name;

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
}
