using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DiceRendererManager : SingletonMonoBehaviour<DiceRendererManager>
{
    // The prefab containing a camera, dice, lights, etc... to render dice into textures
    public SingleDiceRenderer DiceRendererPrefab;
    public MultiDiceRenderer MultiDiceRendererPrefab;

    // Our renderer slots
    // The max of 24 renderers is a bit arbitrary, it needs to be less than 32 and leave some room
    // for a few other render layers for the rest of the app
    const int invisibleRendererLayerIndex = 20;
    DiceRenderer[] visibleRenderers = new DiceRenderer[invisibleRendererLayerIndex];
    List<DiceRenderer> allRenderers = new List<DiceRenderer>();

    public SingleDiceRenderer CreateDiceRenderer(Dice.DesignAndColor variant, int widthHeight = 256)
    {
        // Find the first empty slot
        int newRendererIndex = 0;
        for (; newRendererIndex < visibleRenderers.Length; ++newRendererIndex)
        {
            if (visibleRenderers[newRendererIndex] == null)
            {
                break;
            }
        }

        // If we had a slot, then go ahead and instantiate a renderer
        var singleRenderer = GameObject.Instantiate<SingleDiceRenderer>(DiceRendererPrefab, Vector3.zero, Quaternion.identity, transform);
        allRenderers.Add(singleRenderer);
        singleRenderer.Setup(newRendererIndex, variant, widthHeight);
        if (newRendererIndex != invisibleRendererLayerIndex)
        {
            visibleRenderers[newRendererIndex] = singleRenderer;
        }
        return singleRenderer;
    }

    public MultiDiceRenderer CreateMultiDiceRenderer(List<Dice.DesignAndColor> variants, int widthHeight = 256)
    {
        // Find the first empty slot
        int newRendererIndex = 0;
        for (; newRendererIndex < visibleRenderers.Length; ++newRendererIndex)
        {
            if (visibleRenderers[newRendererIndex] == null)
            {
                break;
            }
        }

        // If we had a slot, then go ahead and instantiate a renderer
        var multiRenderer = GameObject.Instantiate<MultiDiceRenderer>(MultiDiceRendererPrefab, Vector3.zero, Quaternion.identity, transform);
        allRenderers.Add(multiRenderer);
        multiRenderer.Setup(newRendererIndex, variants, widthHeight);
        if (newRendererIndex != invisibleRendererLayerIndex)
        {
            visibleRenderers[newRendererIndex] = multiRenderer;
        }
        return multiRenderer;
    }

    public void DestroyDiceRenderer(DiceRenderer renderer)
    {
        int rendererIndex = 0;
        for (; rendererIndex < visibleRenderers.Length; ++rendererIndex)
        {
            if (visibleRenderers[rendererIndex] == renderer)
            {
                break;
            }
        }

        if (rendererIndex < visibleRenderers.Length)
        {
            visibleRenderers[rendererIndex] = null;
        }
        allRenderers.Remove(renderer);
        GameObject.Destroy(renderer.gameObject);

        if (rendererIndex < visibleRenderers.Length)
        {
            // Try to find a renderer that needs a slot
            var newRenderer = allRenderers.Find(r => r.visible && r.index == invisibleRendererLayerIndex);
            if (newRenderer != null)
            {
                newRenderer.SetIndex(rendererIndex);
                visibleRenderers[rendererIndex] = newRenderer;
                newRenderer.gameObject.SetActive(true);
            }
        }
    }

    public void OnDiceRendererVisible(DiceRenderer renderer, bool visible)
    {
        renderer.visible = visible; // Don't like this...
        if (visible)
        {
            // Can we display the renderer?
            int newRendererIndex = 0;
            for (; newRendererIndex < visibleRenderers.Length; ++newRendererIndex)
            {
                if (visibleRenderers[newRendererIndex] == null)
                {
                    break;
                }
            }

            // If we had a slot, then go ahead and instantiate a renderer
            if (newRendererIndex != invisibleRendererLayerIndex)
            {
                renderer.SetIndex(newRendererIndex);
                visibleRenderers[newRendererIndex] = renderer;
            }
            renderer.gameObject.SetActive(true);
        }
        else
        {
            int rendererIndex = 0;
            for (; rendererIndex < visibleRenderers.Length; ++rendererIndex)
            {
                if (visibleRenderers[rendererIndex] == renderer)
                {
                    break;
                }
            }

            if (rendererIndex < visibleRenderers.Length)
            {
                visibleRenderers[rendererIndex].SetIndex(invisibleRendererLayerIndex);
                visibleRenderers[rendererIndex] = null;
            }
            renderer.gameObject.SetActive(false);

            if (rendererIndex < visibleRenderers.Length)
            {
                // Try to find a renderer that needs a slot
                var newRenderer = allRenderers.Find(r => r.visible && r.index == invisibleRendererLayerIndex);
                if (newRenderer != null)
                {
                    newRenderer.SetIndex(rendererIndex);
                    visibleRenderers[rendererIndex] = newRenderer;
                    newRenderer.gameObject.SetActive(true);
                }
            }
        }
    }
}
