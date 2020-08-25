using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleDiceRenderer : DiceRenderer
{
    public Camera dieCamera;
    public Light[] dieLights;
    public GameObject dieRoot;

    public void SetAuto(bool auto) => die.SetAuto(auto);
    public DiceRendererDice die { get; private set; }

    /// <summary>
    /// Called after instantiation to setup the camera, render texture, etc...
    /// </sumary>
    public void Setup(int index, DiceVariants.DesignAndColor variant, int widthHeight)
    {
        base.Setup(index, widthHeight);

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
    public void SetAnimations(IEnumerable<Animations.EditAnimation> animations) => die.SetAnimations(animations);
    public void ClearAnimations() => die.ClearAnimations();
    public void Play(bool loop) => die.Play(loop);
    public void Stop() => die.Stop();
}
