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
    const int visibleLayerCount = 20;
    DiceRenderer[] visibleRenderers = new DiceRenderer[visibleLayerCount];
    List<DiceRenderer> allRenderers = new List<DiceRenderer>();

    public SingleDiceRenderer CreateDiceRenderer(Dice.DesignAndColor variant, int widthHeight = 256)
    {
        var singleRenderer = GameObject.Instantiate<SingleDiceRenderer>(DiceRendererPrefab, Vector3.zero, Quaternion.identity, transform);
        allRenderers.Add(singleRenderer);
        singleRenderer.Setup(variant, widthHeight);

        // Try to give the renderer a slot
        int newRendererIndex = FindEmptySlot();
        SetSlot(singleRenderer, newRendererIndex);
        return singleRenderer;
    }

    public MultiDiceRenderer CreateMultiDiceRenderer(List<Dice.DesignAndColor> variants, int widthHeight = 256)
    {
        var multiRenderer = GameObject.Instantiate<MultiDiceRenderer>(MultiDiceRendererPrefab, Vector3.zero, Quaternion.identity, transform);
        allRenderers.Add(multiRenderer);
        multiRenderer.Setup(variants, widthHeight);

        int newRendererIndex = FindEmptySlot();
        SetSlot(multiRenderer, newRendererIndex);
        return multiRenderer;
    }

    public void DestroyDiceRenderer(DiceRenderer renderer)
    {
        int slot = FindSlot(renderer);
        if (slot != -1)
        {
            visibleRenderers[slot] = null;
        }
        allRenderers.Remove(renderer);
        GameObject.Destroy(renderer.gameObject);

        if (slot != -1)
        {
            RecycleSlot(slot);
        }
    }

    public void OnDiceRendererVisible(DiceRenderer renderer, bool visible)
    {
        renderer.visible = visible; // Don't like this...
        if (visible)
        {
            // Is the renderer displayed?
            int index = FindSlot(renderer);
            if (index == -1)
            {
                // No, can it be?
                index = FindEmptySlot();
                if (index != -1)
                {
                    SetSlot(renderer, index);
                }
                // Else still can't display
            }
            // Else already displayed
        }
        else
        {
            // Is the renderer displayed?
            int index = FindSlot(renderer);
            if (index != -1)
            {
                // Yes, turn it off
                visibleRenderers[index] = null;
                SetSlot(renderer, -1);
                RecycleSlot(index);
            }
            // Else already off
        }
    }

    int FindEmptySlot()
    {
        for (int i = 0; i < visibleRenderers.Length; ++i)
        {
            if (visibleRenderers[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    int FindSlot(DiceRenderer diceRenderer)
    {
        for (int rendererIndex = 0; rendererIndex < visibleRenderers.Length; ++rendererIndex)
        {
            if (visibleRenderers[rendererIndex] == diceRenderer)
            {
                return rendererIndex;
            }
        }
        return -1;
    }

    void SetSlot(DiceRenderer renderer, int index)
    {
        if (index != -1)
        {
            renderer.SetIndex(index);
            visibleRenderers[index] = renderer;
            renderer.gameObject.SetActive(true);
        }
        else
        {
            renderer.gameObject.SetActive(false);
        }
    }

    void ClearSlot(int index)
    {
        var renderer = visibleRenderers[index];
        if (renderer != null)
        {
            renderer.gameObject.SetActive(false);
            visibleRenderers[index] = null;
        }
    }

    void RecycleSlot(int index)
    {
        // Try to find a renderer that needs a slot
        var newRenderer = allRenderers.Find(r => r.visible && !visibleRenderers.Contains(r));
        if (newRenderer != null)
        {
            SetSlot(newRenderer, index);
        }
    }
}
