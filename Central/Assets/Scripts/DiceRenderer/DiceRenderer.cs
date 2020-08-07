using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRenderer : MonoBehaviour
{
    // This list should match the dice variant enum
    public List<DiceRendererDice> diceVariantPrefabs;

    public Camera dieCamera;
    public Light[] dieLights;
    public GameObject dieRoot;

    public RenderTexture renderTexture { get; private set; }
    public int layerIndex { get; private set; }
    public bool rotating
    {
        get { return (die != null) ? die.rotating : false; }
        set { if (die != null) die.rotating = value; }
    }
    DiceRendererDice die;

    /// <summary>
    /// Called after instantiation to setup the camera, render texture, etc...
    /// </sumary>
    public void Setup(int index, DiceVariants.DesignAndColor variant, int widthHeight)
    {
        layerIndex = LayerMask.NameToLayer("Dice 0") + index;
        renderTexture = new RenderTexture(widthHeight, widthHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();

        dieCamera.cullingMask = 1 << layerIndex; // only render this die
        dieCamera.targetTexture = renderTexture;

        foreach (var light in dieLights)
        {
            light.cullingMask = 1 << layerIndex;
        }

        // Instantiate the proper type of dice
        die = GameObject.Instantiate<DiceRendererDice>(diceVariantPrefabs[(int)variant], Vector3.zero, Quaternion.identity, dieRoot.transform);

        // Make it visible to the lights and camera
        dieRoot.layer = layerIndex;
        die.gameObject.layer = layerIndex;
        foreach (var tr in die.gameObject.GetComponentsInChildren<Transform>())
        {
            tr.gameObject.layer = layerIndex;
        }

        foreach (var light in die.gameObject.GetComponentsInChildren<Light>())
        {
            light.cullingMask = 1 << layerIndex;
        }
    }

    public void SetAnimation(Animations.EditAnimation animation) => die.SetAnimation(animation);
    public void ClearAnimation() => die.ClearAnimation();
    public void Play(bool loop) => die.Play(loop);
    public void Stop() => die.Stop();

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture = null;
        }
    }
}
