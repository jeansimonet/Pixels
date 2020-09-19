using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animations;

public class UIAnimationSelectorPatternToken : MonoBehaviour
{
    [Header("Controls")]
    public Button mainButton;
    public RawImage animRenderImage;
    public Text animNameText;
    public Image frame;

    [Header("Parameters")]
    public Color defaultTextColor;
    public Color defaultFrameColor;
    public Color selectedColor;

    public EditAnimation editAnimation { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;
    public bool selected { get; private set; }

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
        SetSelected(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        if (selected)
        {
            animNameText.color = selectedColor;
            frame.color = selectedColor;
        }
        else
        {
            animNameText.color = defaultTextColor;
            frame.color = defaultFrameColor;
        }
    }

}
