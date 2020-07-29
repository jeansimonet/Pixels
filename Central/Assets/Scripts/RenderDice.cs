using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Animations;

public class RenderDice : MonoBehaviour
{
    public MeshRenderer[] FaceRenderers;
    public Light[] FaceLights;

    public Color[] FaceColors;

    public float rotationSpeedDeg = 10.0f;

    MaterialPropertyBlock[] propertyBlocks;

    DataSet animationSet;
    AnimationInstance currentAnimation;

    public int testingAnimationIndex;

    void SetAnimationSet(DataSet set)
    {
        animationSet = set;
    }

    void PlayAnimation(Animations.Animation animation)
    {
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
        // // TESTING
        // string path = System.IO.Path.Combine(Application.persistentDataPath, "D20_animation_set.json");
        // string jsonText = File.ReadAllText(path);
        // var editAnimSet = JsonUtility.FromJson<EditDataSet>(jsonText);
        // animationSet = editAnimSet.ToDataSet();
        // PlayAnimation(animationSet.animations[testingAnimationIndex]);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentAnimation != null)
        {
            // Update the animation time
            // currentAnimation.time += Time.deltaTime;
            // if (currentAnimation.time > (currentAnimation.animation.duration / 1000.0f))
            // {
            //     // TEST
            //     currentAnimation.time -= currentAnimation.animation.duration / 1000.0f;
            //     // for (int i = 0; i < 20; ++i)
            //     // {
            //     //     FaceColors[i] = Color.black;
            //     // }
            //     // currentAnimation = null;
            // }
            // else
            // {
            //     for (int t = 0; t < currentAnimation.animation.trackCount; ++t)
            //     {
            //         var track = currentAnimation.animation.GetTrack(animationSet, (ushort)t);
            //         var color = track.evaluate(animationSet, (int)(currentAnimation.time * 1000.0f));
            //         Color32 color32 = new Color32(
            //             ColorUtils.getRed(color),
            //             ColorUtils.getGreen(color),
            //             ColorUtils.getBlue(color),
            //             255);
            //         FaceColors[track.ledIndex] = color32;
            //     }
            // }
        }
        else
        {
            for (int i = 0; i < 20; ++i)
            {
                FaceColors[i] = Color.black;
            }
        }

        UpdateColors();
        transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeedDeg, Space.Self);
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
