using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;

public class UIBehaviorToken : MonoBehaviour
{
    [Header("Controls")]
    public Button mainButton;
    public RawImage behaviorRenderImage;
    public Text behaviorNameText;
    public Text behaviorDescriptionText;
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

    public EditBehavior editBehavior { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;
    public Button.ButtonClickedEvent onRemove => removeButton.onClick;
    public Button.ButtonClickedEvent onDuplicate => duplicateButton.onClick;
    public Button.ButtonClickedEvent onEdit => editButton.onClick;
    public Button.ButtonClickedEvent onExpand => menuButton.onClick;


    public bool isExpanded => expandedRoot.gameObject.activeSelf;


    public void Setup(EditBehavior bh)
    {
        this.editBehavior = bh;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(bh.defaultPreviewSettings.design);
        if (dieRenderer != null)
        {
            behaviorRenderImage.texture = dieRenderer.renderTexture;
        }
        behaviorNameText.text = bh.name;
        behaviorDescriptionText.text = bh.description;

        dieRenderer.SetAuto(true);
        dieRenderer.SetAnimations(this.editBehavior.CollectAnimations());
        dieRenderer.Play(true);
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
}
