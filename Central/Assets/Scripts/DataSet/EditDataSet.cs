using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animations;
using Behaviors;
using System.Linq;
using System.Text;

/// <summary>
/// The animation set is a list of multiple animations
/// This class knows how to convert to/from the runtime data used by the dice
/// </summary>
[System.Serializable]
public class EditDataSet
{
    public List<EditAnimation> animations = new List<EditAnimation>();
    public List<EditBehavior> behaviors = new List<EditBehavior>();
    public ushort currentBehaviorIndex;
    public ushort heatTrackIndex;

    public DataSet ToDataSet()
    {
        DataSet set = new DataSet();

        // Add animations
        for (int animIndex = 0; animIndex < animations.Count; ++animIndex)
        {
            var editAnim = animations[animIndex];
            var anim = editAnim.ToAnimation(this, set);
            set.animations.Add(anim);
        }

        // Add behaviors
        for (int behaviorIndex = 0; behaviorIndex < behaviors.Count; ++behaviorIndex)
        {
            var editBehavior = behaviors[behaviorIndex];
            var behavior = editBehavior.ToBehavior(this, set);
            set.behaviors.Add(behavior);
        }

        return set;
    }

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
        public ushort currentBehaviorIndex;
        public ushort heatTrackIndex;
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
        data.currentBehaviorIndex = currentBehaviorIndex;
        data.heatTrackIndex = heatTrackIndex;
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
        currentBehaviorIndex = data.currentBehaviorIndex;
        heatTrackIndex = data.heatTrackIndex;
    }

    public EditAnimation DuplicateAnimation(EditAnimation animation)
    {
        var newAnim = animation.Duplicate();
        animations.Add(newAnim);
        return newAnim;
    }

    public EditBehavior DuplicateBehavior(EditBehavior behavior)
    {
        var newBehavior = behavior.Duplicate();
        behaviors.Add(newBehavior);
        return newBehavior;
    }

    public static EditDataSet CreateTestDataSet()
    {
        EditDataSet ret = new EditDataSet();
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
        ret.currentBehaviorIndex = 0;
        ret.heatTrackIndex = 0;

        return ret;
    }
}
