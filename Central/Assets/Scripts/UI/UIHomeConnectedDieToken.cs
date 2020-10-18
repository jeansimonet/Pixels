using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHomeConnectedDieToken : MonoBehaviour
{
    [Header("Controls")]
    public RawImage dieRenderImage;
    public Text dieNameText;

    public Dice.EditDie editDie { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }

    bool visible = true;

    public void Setup(Dice.EditDie die)
    {
        editDie = die;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(die.designAndColor);
        if (dieRenderer != null)
        {
            dieRenderImage.texture = dieRenderer.renderTexture;
        }
        dieNameText.text = die.name;
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
