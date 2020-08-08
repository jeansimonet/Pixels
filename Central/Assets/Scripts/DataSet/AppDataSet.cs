using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Animations;
using Behaviors;

[System.Serializable]
public class PreviewSettings
{
    public DiceVariants.DesignAndColor design;
}

public class AppDataSet : SingletonMonoBehaviour<AppDataSet>
{
    public List<EditAnimation> animations = new List<EditAnimation>();
    public List<EditBehavior> behaviors = new List<EditBehavior>();

    [System.Serializable]
    struct JsonData
    {
        [System.Serializable]
        public struct Animation
        {
            public Animations.AnimationType type;
            public string json;
        }
        public List<Animation> animations;
        public List<string> behaviorJsons;
    }

    public string ToJson()
    {
        var data = new JsonData();
        data.animations = new List<JsonData.Animation>();
        foreach (var anim in animations)
        {
            data.animations.Add(new JsonData.Animation()
            {
                type = anim.type,
                json = anim.ToJson()
            });
        }
        data.behaviorJsons = new List<string>();
        foreach (var behavior in behaviors)
        {
            data.behaviorJsons.Add(behavior.ToJson(this));
        }
        return JsonUtility.ToJson(data);
    }

    public void FromJson(string json)
    {
        // Parse json string in
        animations.Clear();
        behaviors.Clear();

        var data = JsonUtility.FromJson<JsonData>(json);
        foreach (var animData in data.animations)
        {
            var anim = EditAnimation.Create(animData.type);
            anim.FromJson(animData.json);
            animations.Add(anim);
        }
        foreach (var behaviorData in data.behaviorJsons)
        {
            var behavior = new EditBehavior();
            behavior.FromJson(this, behaviorData);
            behaviors.Add(behavior);
        }
    }

    public EditAnimation AddNewDefaultAnimation()
    {
        var newAnim = new Animations.EditAnimationSimple();
        newAnim.duration = 3.0f;
        newAnim.color = new Color32(0xFF, 0x30, 0x00, 0xFF);
        newAnim.ledType = AnimationSimpleLEDType.AllLEDs;
        newAnim.name = "New Animation";
        animations.Add(newAnim);
        return newAnim;
    }

    public EditAnimation DuplicateAnimation(EditAnimation animation)
    {
        var newAnim = animation.Duplicate();
        animations.Add(newAnim);
        return newAnim;
    }

    public void ReplaceAnimation(EditAnimation oldAnimation, EditAnimation newAnimation)
    {
        foreach (var behavior in behaviors)
        {
            behavior.ReplaceAnimation(oldAnimation, newAnimation);
        }
        int oldAnimIndex = animations.IndexOf(oldAnimation);
        animations[oldAnimIndex] = newAnimation;
    }

    public EditBehavior AddNewDefaultBehavior()
    {
        var newBehavior = new Behaviors.EditBehavior();
        newBehavior.name = "New Behavior";
        newBehavior.description = "New Behavior Description";
        newBehavior.rules.Add(new Behaviors.EditRule()
        {
            condition = new Behaviors.EditConditionFaceCompare()
            {
                flags = ConditionFaceCompare_Flags.Equal,
                faceIndex = 19
            },
            action = new Behaviors.EditActionPlayAnimation()
            {
                animation = null,
                faceIndex = 0,
                loopCount = 1
            }
        });
        behaviors.Add(newBehavior);
        return newBehavior;
    }

    public EditBehavior DuplicateBehavior(EditBehavior behavior)
    {
        var newBehavior = behavior.Duplicate();
        behaviors.Add(newBehavior);
        return newBehavior;
    }

    void Start()
    {
        LoadData();
    }

    /// <summary>
    /// Load our pool from file
    /// </sumary>
    public void LoadData()
    {
        var path = System.IO.Path.Combine(Application.persistentDataPath, AppConstants.Instance.DataSetFilename);
        bool ret = File.Exists(path);
        if (ret)
        {
            string jsonText = File.ReadAllText(path);
            FromJson(jsonText);
        }
    }

    /// <summary>
    /// Save our pool to file
    /// </sumary>
    public void SaveData()
    {
        var path = System.IO.Path.Combine(Application.persistentDataPath, AppConstants.Instance.DataSetFilename);
        File.WriteAllText(path, ToJson());
    }

    public static AppDataSet CreateTestDataSet()
    {
        AppDataSet ret = new AppDataSet();
        EditAnimationSimple simpleAnim = new EditAnimationSimple();
        simpleAnim.duration = 1.0f;
        simpleAnim.color = Color.blue;
        simpleAnim.ledType = Animations.AnimationSimpleLEDType.AllLEDs;
        simpleAnim.name = "Simple Anim 1";
        ret.animations.Add(simpleAnim);

        EditAnimationKeyframed keyAnim = new EditAnimationKeyframed();
        keyAnim.duration = 3.0f;
        keyAnim.specialColorType = SpecialColor.None;
        keyAnim.name = "Keyframed Anim 2";
        keyAnim.tracks.Add(new EditTrack()
        {
            ledIndices = new List<int>() { 1, 5, 9 },
            keyframes = new List<EditKeyframe>() {
                new EditKeyframe() { time = 0.0f, color = Color.black },
                new EditKeyframe() { time = 1.5f, color = Color.red },
                new EditKeyframe() { time = 3.0f, color = Color.black },
            }
        });
        keyAnim.tracks.Add(new EditTrack()
        {
            ledIndices = new List<int>() { 0, 2, 3, 4 },
            keyframes = new List<EditKeyframe>() {
                new EditKeyframe() { time = 0.0f, color = Color.black },
                new EditKeyframe() { time = 1.0f, color = Color.cyan },
                new EditKeyframe() { time = 2.0f, color = Color.cyan },
                new EditKeyframe() { time = 3.0f, color = Color.black },
            }
        });
        ret.animations.Add(keyAnim);

        EditBehavior behavior = new EditBehavior();
        behavior.rules.Add(new EditRule() {
            condition = new EditConditionRolling(),
            action = new EditActionPlayAnimation() { animation = simpleAnim, faceIndex = 0, loopCount = 1 }
        });
        behavior.rules.Add(new EditRule() {
            condition = new EditConditionFaceCompare()
            {
                faceIndex = 19,
                flags = ConditionFaceCompare_Flags.Equal
            },
            action = new EditActionPlayAnimation() { animation = keyAnim, faceIndex = 19, loopCount = 1 }
        });
        behavior.rules.Add(new EditRule() {
            condition = new EditConditionFaceCompare()
            {
                faceIndex = 0,
                flags = ConditionFaceCompare_Flags.Less | ConditionFaceCompare_Flags.Equal | ConditionFaceCompare_Flags.Greater
            },
            action = new EditActionPlayAnimation() { animation = keyAnim, faceIndex = 2, loopCount = 1 }
        });
        ret.behaviors.Add(behavior);

        return ret;
    }
}
