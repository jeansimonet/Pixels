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

    public DataSet ToDataSet()
    {
        DataSet set = new DataSet();

        // Add animations
        for (int animIndex = 0; animIndex < animations.Count; ++animIndex)
        {
            var editAnim = animations[animIndex];
            if (editAnim != null)
            {
                var anim = editAnim.ToAnimation(this, set.animationBits);
                set.animations.Add(anim);
            }
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

}
