using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animations;

public class UIPatternPickerToken : MonoBehaviour
{
    [Header("Controls")]
    public Button mainButton;
    public RawImage animRenderImage;
    public RawImage textureImage;
    public Text animNameText;

    public EditPattern editPattern { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;

    public void Setup(EditPattern pattern)
    {
        this.editPattern = pattern;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(Dice.DesignAndColor.V5_Black);
        if (dieRenderer != null)
        {
            animRenderImage.texture = dieRenderer.renderTexture;
        }
        animNameText.text = pattern.name;

        var anim = new EditAnimationKeyframed();
        anim.name = "temp anim";
        anim.pattern = pattern;
        anim.duration = pattern.duration;

        textureImage.texture = pattern.ToTexture();

        dieRenderer.SetAuto(true);
        dieRenderer.SetAnimation(anim);
        dieRenderer.Play(true);
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
}
