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
    public int renderTextureSize = 256;

    public RenderTexture renderTexture { get; private set; }
    public int layerIndex { get; private set; }
    DiceRendererDice die;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// Called after instantiation to setup the camera, render texture, etc...
    /// </sumary>
    public void Setup(int index, DiceVariants.DesignAndColor variant)
    {
        layerIndex = LayerMask.NameToLayer("Dice 0") + index;
        renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture = null;
        }
    }
}
