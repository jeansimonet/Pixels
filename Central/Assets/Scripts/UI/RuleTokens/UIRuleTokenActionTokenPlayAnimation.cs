using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;
using Dice;

public class UIRuleTokenActionTokenPlayAnimation
    : UIRuleTokenActionToken
{
    [Header("Controls")]
    public Text labelText;
    public Text actionText;
    public RawImage dieRenderImage;

    public SingleDiceRenderer dieRenderer { get; private set; }

    bool visible = true;

    static readonly ActionType[] supportedActionTypes = new ActionType[]
    {
        ActionType.PlayAnimation
    };

    public override IEnumerable<ActionType> actionTypes
    {
        get { return supportedActionTypes; }
    }

    void OnDestroy()
    {
        if (DiceRendererManager.Instance != null && this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
    }

    public override void Setup(EditAction action, bool first)
    {
        var playAnimAction = action as EditActionPlayAnimation;
        actionText.text = action.ToString();
        labelText.text = first ? "Then" : "And";

        if (this.dieRenderer == null)
        {
            var design = DesignAndColor.V5_Grey;
            if (playAnimAction.animation != null)
            {
                design = playAnimAction.animation.defaultPreviewSettings.design;
            }
            this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(design, 160);
            if (dieRenderer != null)
            {
                dieRenderImage.texture = dieRenderer.renderTexture;
            }

            dieRenderer.SetAuto(true);
        }

        if (dieRenderer != null && playAnimAction.animation != null)
        {
            dieRenderer.SetAnimation(playAnimAction.animation);
            dieRenderer.Play(true);
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
