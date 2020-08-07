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
    public EditBehavior currentBehavior;
    public EditAnimationKeyframed heatTrack;

    public DataSet ToDataSet()
    {
        DataSet set = new DataSet();

        // Check that the heat track is in the list of animations
        if (heatTrack != null && !animations.Contains(heatTrack))
        {
            Debug.LogWarning("Heat track animation not in list");
            animations.Add(heatTrack);
        }

        // Same for current behavior
        if (currentBehavior != null && !behaviors.Contains(currentBehavior))
        {
            behaviors.Add(currentBehavior);
        }

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

        set.currentBehaviorIndex = (ushort)behaviors.IndexOf(currentBehavior);
        set.heatTrackIndex = (ushort)animations.IndexOf(heatTrack);

        return set;
    }

    [System.Serializable]
    struct JsonData
    {
        public List<int> animationIndices;
        public List<int> behaviorIndices;
        public ushort currentBehaviorIndex;
        public ushort heatTrackIndex;
    }

    public string ToJson(AppDataSet appDataSet)
    {
        var data = new JsonData();
        data.animationIndices = new List<int>();
        foreach (var anim in animations)
        {
            data.animationIndices.Add(appDataSet.animations.IndexOf(anim));
        }
        data.behaviorIndices = new List<int>();
        foreach (var behavior in behaviors)
        {
            data.behaviorIndices.Add(appDataSet.behaviors.IndexOf(behavior));
        }
        data.currentBehaviorIndex = (ushort)behaviors.IndexOf(currentBehavior);
        data.heatTrackIndex = (ushort)animations.IndexOf(heatTrack);
        return JsonUtility.ToJson(data);
    }

    public void FromJson(AppDataSet appDataSet, string json)
    {
        // Parse json string in
        animations.Clear();
        behaviors.Clear();

        var data = JsonUtility.FromJson<JsonData>(json);
        foreach (var animData in data.animationIndices)
        {
            animations.Add(appDataSet.animations[animData]);
        }
        foreach (var behaviorData in data.behaviorIndices)
        {
            behaviors.Add(appDataSet.behaviors[behaviorData]);
        }
        currentBehavior = behaviors[data.currentBehaviorIndex];
        EditAnimation heatTrackAnim = animations[data.heatTrackIndex];
        Debug.Assert(heatTrack is EditAnimationKeyframed);
        heatTrack = (EditAnimationKeyframed)heatTrackAnim;
    }
}
