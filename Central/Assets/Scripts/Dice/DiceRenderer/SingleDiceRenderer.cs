using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleDiceRenderer : DiceRenderer
{
    public Camera dieCamera;
    public Light[] dieLights;
    public GameObject dieRoot;

    public Transform cameraTiltRoot;
    public void SetAuto(bool auto) => die.SetAuto(auto);
    public DiceRendererDice die { get; private set; }

    public float tilt
    {
        get { return cameraTiltRoot.localEulerAngles.x; }
        set { var euler = cameraTiltRoot.localEulerAngles; euler.x = value; cameraTiltRoot.localEulerAngles = euler; }
    }

    public int renderIndex = 0;

    float initialTilt;

    /// <summary>
    /// Called after instantiation to setup the camera, render texture, etc...
    /// </sumary>
    public void Setup(Dice.DesignAndColor variant, int widthHeight)
    {
        base.Setup(widthHeight);
        initialTilt = cameraTiltRoot.localEulerAngles.x;
        dieCamera.targetTexture = renderTexture;

        // Instantiate the proper type of dice
        die = GameObject.Instantiate<DiceRendererDice>(diceVariantPrefabs[(int)variant], Vector3.zero, Quaternion.identity, dieRoot.transform);
    }

    public override void SetIndex(int index)
    {
        if (index != -1)
        {
            base.SetIndex(index);
            dieCamera.cullingMask = layerMask; // only render this die

            foreach (var light in dieLights)
            {
                light.cullingMask = layerMask;
            }

            // Make it visible to the lights and camera
            dieRoot.layer = layerIndex;
            die.gameObject.layer = layerIndex;
            foreach (var tr in die.gameObject.GetComponentsInChildren<Transform>())
            {
                tr.gameObject.layer = layerIndex;
            }

            foreach (var light in die.gameObject.GetComponentsInChildren<Light>())
            {
                light.cullingMask = layerMask;
            }
            renderIndex = layerIndex;
        }
    }

    public void SetAnimation(Animations.EditAnimation animation) => die.SetAnimation(animation);
    public void SetAnimations(IEnumerable<Animations.EditAnimation> animations) => die.SetAnimations(animations);
    public void ClearAnimations() => die.ClearAnimations();
    public void Play(bool loop) => die.Play(loop);
    public void Stop() => die.Stop();

    public void ResetTilt()
    {
        tilt = initialTilt;
    }
}
