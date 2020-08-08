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

    public EditBehavior editBehavior { get; private set; }
    public DiceRenderer dieRenderer { get; private set; }

    public Button.ButtonClickedEvent onClick => mainButton.onClick;

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

        //dieRenderer.rotating = true;
        //dieRenderer.SetAnimation(anim);
        //dieRenderer.Play(true);
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
