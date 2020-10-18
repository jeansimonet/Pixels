using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DiceRenderer : MonoBehaviour
{
    // This list should match the dice variant enum
    public List<DiceRendererDice> diceVariantPrefabs;
    public RenderTexture renderTexture { get; private set; }
    public int index { get; private set; }
    public int layerIndex { get; private set; }

    public bool visible { get; set; } = true;

    /// <summary>
    /// Called after instantiation to setup the camera, render texture, etc...
    /// </sumary>
    protected void Setup(int index, int widthHeight)
    {
        this.index = index;
        layerIndex = LayerMask.NameToLayer("Dice 0") + index;
        renderTexture = new RenderTexture(widthHeight, widthHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();
    }

    public abstract void SetIndex(int index);

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture = null;
        }
    }
}
