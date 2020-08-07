using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Animations;

public class DiceRendererDice : MonoBehaviour
{
    public MeshRenderer[] FaceRenderers;
    public Light[] FaceLights;
    public Color[] FaceColors;
    MaterialPropertyBlock[] propertyBlocks;

    public bool rotating { get; set; }
    float rotationSpeedDeg;

    DataSet dataSet;
    EditAnimation currentAnimation;
    AnimationInstance currentInstance;

    public void SetAnimation(Animations.EditAnimation editAnimation)
    {
        currentAnimation = editAnimation;
        if (currentInstance != null)
        {
            // We're switching the animation from underneath the playback
            // Create a new dataset and instance
            EditDataSet tempEditSet = new EditDataSet();
            tempEditSet.animations.Add(currentAnimation);
            var newDataSet = tempEditSet.ToDataSet();
            var newInstance = newDataSet.animations[0].CreateInstance();
            newInstance.start(newDataSet, currentInstance.startTime, currentInstance.remapFace, currentInstance.loop);

            dataSet = newDataSet;
            currentInstance = newInstance;
        }
    }

    public void ClearAnimation()
    {
        // Shouldn't have an instance if we don't have an animation
        currentInstance = null;
        dataSet = null;

        // Clear the animation
        currentAnimation = null;
    }

    public void Play(bool loop)
    {
        if (currentAnimation != null)
        {
            // Create a temporary data set so we can play the animation
            EditDataSet tempEditSet = new EditDataSet();
            tempEditSet.animations.Add(currentAnimation);
            dataSet = tempEditSet.ToDataSet();
            currentInstance = dataSet.animations[0].CreateInstance();
            currentInstance.start(dataSet, (int)(Time.time * 1000), 0, loop);
        }
        else
        {
            Debug.LogWarning("Trying to play null animation on die renderer die");
        }
    }

    public void Stop()
    {
        currentInstance = null;
        dataSet = null;
    }

    void Awake()
    {
        propertyBlocks = new MaterialPropertyBlock[20];
        for (int i = 0; i < FaceColors.Length; ++i)
        {
            FaceColors[i] = Color.black;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_GlowColor", FaceColors[i]);

            propertyBlocks[i] = block;
            FaceRenderers[i].SetPropertyBlock(block);
            FaceLights[i].color = FaceColors[i];
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.Rotate(Vector3.up, Random.Range(0.0f, 360.0f), Space.Self);
        rotationSpeedDeg = AppConstants.Instance.DiceRotationSpeedAvg + Random.Range(-AppConstants.Instance.DiceRotationSpeedVar, AppConstants.Instance.DiceRotationSpeedVar);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentInstance != null)
        {
            // Update the animation time
            int time = (int)(Time.time * 1000);
            if (time > (currentInstance.startTime + currentInstance.animationPreset.duration))
            {
                if (currentInstance.loop)
                {
                    currentInstance.startTime += currentInstance.animationPreset.duration;
                }
                else
                {
                    for (int i = 0; i < 20; ++i)
                    {
                        FaceColors[i] = Color.black;
                    }
                    currentInstance = null;
                }
            }
            else
            {
                int [] retIndices = new int[20];
                uint[] retColors = new uint[20];
                int ledCount = currentInstance.updateLEDs(dataSet, time, retIndices, retColors);
                for (int t = 0; t < ledCount; ++t)
                {
                    uint color = retColors[t];
                    Color32 color32 = new Color32(
                        ColorUtils.getRed(color),
                        ColorUtils.getGreen(color),
                        ColorUtils.getBlue(color),
                        255);
                    FaceColors[retIndices[t]] = color32;
                }
            }
        }
        else
        {
            for (int i = 0; i < 20; ++i)
            {
                FaceColors[i] = Color.black;
            }
        }

        UpdateColors();
        if (rotating)
        {
            transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeedDeg, Space.Self);
        }
    }

    void UpdateColors()
    {
        if (FaceColors != null)
        {
            if (propertyBlocks == null)
            {
                propertyBlocks = new MaterialPropertyBlock[20];
                for (int i = 0; i < FaceColors.Length; ++i)
                {
                    propertyBlocks[i] = new MaterialPropertyBlock();
                }
            } 

            for (int i = 0; i < FaceColors.Length; ++i)
            {
                var block = propertyBlocks[i];
                block.SetColor("_GlowColor", FaceColors[i]);
                FaceRenderers[i].SetPropertyBlock(block);
                FaceLights[i].color = FaceColors[i];
            }
        }
    }

    void OnValidate()
    {
        UpdateColors();
    }
}
