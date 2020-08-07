using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPatternView
    : PixelsApp.Page
{
    [Header("Controls")]
    public Button backButton;
    public Text animationNameText;
    public Button menuButton;
    public RawImage previewImage;
    public UIParameterEnum animationSelector;
    public RectTransform parametersRoot;

    public Animations.EditAnimation editAnimation { get; private set; }
    public DiceRenderer dieRenderer { get; private set; }

    public override void Enter(object context)
    {
        base.Enter(context);
        var anim = context as Animations.EditAnimation;
        if (anim != null)
        {
            Setup(anim);
        }
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        if (DiceRendererManager.Instance != null && this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }

        if (UIParameterManager.Instance != null && editAnimation != null)
        {
            UIParameterManager.Instance.DestroyControls(editAnimation);
        }
    }

    void Setup(Animations.EditAnimation anim)
    {
        editAnimation = anim;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(anim.defaultPreviewSettings.design, 600);
        if (dieRenderer != null)
        {
            previewImage.texture = dieRenderer.renderTexture;
        }
        animationNameText.text = anim.name;

        animationSelector.Setup("Animation Type", () => editAnimation.type, (t) => { Debug.Log("New animation type: " + t); });

        // Setup all other parameters
        var paramList = UIParameterManager.Instance.CreateControls(anim, parametersRoot);
        paramList.onParameterChanged += OnAnimParameterChanged;

        dieRenderer.rotating = true;
        dieRenderer.SetAnimation(anim);
        dieRenderer.Play(true);
    }

    void Awake()
    {
        backButton.onClick.AddListener(() => NavigationManager.Instance.GoBack());
    }

    void OnAnimParameterChanged(object animObject, UIParameter parameter, object newValue)
    {
        var theEditAnim = (Animations.EditAnimation)animObject;
        Debug.Assert(theEditAnim == editAnimation);
        dieRenderer.SetAnimation(theEditAnim);
    }
}
