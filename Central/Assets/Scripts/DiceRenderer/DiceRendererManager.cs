using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DiceRendererManager : SingletonMonoBehaviour<DiceRendererManager>
{
    // The prefab containing a camera, dice, lights, etc... to render dice into textures
    public DiceRenderer DiceRendererPrefab;

    // Our renderer slots
    // The max of 24 renderers is a bit arbitrary, it needs to be less than 32 and leave some room
    // for a few other render layers for the rest of the app
    DiceRenderer[] renderers = new DiceRenderer[24]; 

    public void TestCreateRenderer(int v)
    {
        CreateDiceRenderer((DiceVariants.DesignAndColor)v);
    }

    public DiceRenderer CreateDiceRenderer(DiceVariants.DesignAndColor variant, int widthHeight = 256)
    {
        // Find the first empty slot
        int newRendererIndex = 0;
        for (; newRendererIndex < renderers.Length; ++newRendererIndex)
        {
            if (renderers[newRendererIndex] == null)
            {
                break;
            }
        }

        // If we had a slot, then go ahead and instantiate a renderer
        DiceRenderer renderer = null;
        if (newRendererIndex < renderers.Length)
        {
            renderer = GameObject.Instantiate<DiceRenderer>(DiceRendererPrefab, Vector3.zero, Quaternion.identity, transform);
            renderer.Setup(newRendererIndex, variant, widthHeight);
            renderers[newRendererIndex] = renderer;
        }
        return renderer;
    }

    public void DestroyDiceRenderer(DiceRenderer renderer)
    {
        int rendererIndex = 0;
        for (; rendererIndex < renderers.Length; ++rendererIndex)
        {
            if (renderers[rendererIndex] == renderer)
            {
                break;
            }
        }

        if (rendererIndex < renderers.Length)
        {
            renderers[rendererIndex] = null;
            GameObject.Destroy(renderer.gameObject);
        }
    }
}
