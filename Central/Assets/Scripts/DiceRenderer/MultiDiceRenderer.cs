using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDiceRenderer : DiceRenderer
{
    public Light[] dieLights;
    public MultiDiceRendererRootList[] diceRootsAndCam;

    public int dieCount { get; private set; } = 1;
    public MultiDiceRendererRootList diceRoot => diceRootsAndCam[dieCount - 1];
    public Camera dieCamera => diceRoot.captureCam;
    public GameObject[] dieRoots => diceRoot.roots;

    List<DiceRendererDice> dice = new List<DiceRendererDice>();

    public bool rotating
    {
        get { return (diceRoot != null) ? diceRoot.rotating : false; }
        set { if (diceRoot != null) diceRoot.rotating = value; }
    }

    /// <summary>
    /// Called after instantiation to setup the camera, render texture, etc...
    /// </sumary>
    public void Setup(int index, List<DiceVariants.DesignAndColor> variants, int widthHeight)
    {
        base.Setup(index, widthHeight);

        // Find the proper camera
        dieCount = variants.Count;

        diceRoot.gameObject.SetActive(true);

        dieCamera.cullingMask = 1 << layerIndex; // only render this die
        dieCamera.targetTexture = renderTexture;

        foreach (var light in dieLights)
        {
            light.cullingMask = 1 << layerIndex;
        }

        // Instantiate the proper type of dice
        for (int dieIndex = 0; dieIndex < variants.Count; ++dieIndex)
        {
            var variant = variants[dieIndex];
            var root = dieRoots[dieIndex];
            var die = GameObject.Instantiate<DiceRendererDice>(diceVariantPrefabs[(int)variant], root.transform);

            // Make it visible to the lights and camera
            root.layer = layerIndex;
            die.gameObject.layer = layerIndex;
            foreach (var tr in die.gameObject.GetComponentsInChildren<Transform>())
            {
                tr.gameObject.layer = layerIndex;
            }
            foreach (var light in die.gameObject.GetComponentsInChildren<Light>())
            {
                light.cullingMask = 1 << layerIndex;
            }

            dice.Add(die);
        }
    }

    public void SetDieAnimations(int index, IEnumerable<Animations.EditAnimation> animations)
    {
        dice[index].SetAnimations(animations);
    }
    public void ClearAnimations(int index)
    {
        dice[index].ClearAnimations();
    }
    public void Play(int index, bool loop)
    {
        dice[index].Play(loop);
    }
    public void Stop(int index)
    {
        dice[index].Stop();
    }
}

