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
    public EditBehavior behavior = null;

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

        // Now convert
        if (behavior != null)
        {
            set.behavior = behavior.ToBehavior(this, set);
        }

        return set;
    }

}
