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
    public List<EditPattern> patterns = new List<EditPattern>();
    public List<EditPattern> rgbPatterns = new List<EditPattern>();
    public List<EditAnimation> animations = new List<EditAnimation>();
    public EditBehavior behavior = null;

    public int getPatternTrackOffset(EditPattern pattern)
    {
        int ret = 0;
        for (int i = 0; i < patterns.Count; ++i)
        {
            if (patterns[i] == pattern)
            {
                return ret;
            }
            else
            {
                ret += pattern.gradients.Count;
            }
        }
        return -1;
    }

    public int getPatternRGBTrackOffset(EditPattern pattern)
    {
        int ret = 0;
        for (int i = 0; i < rgbPatterns.Count; ++i)
        {
            if (rgbPatterns[i] == pattern)
            {
                return ret;
            }
            else
            {
                ret += pattern.gradients.Count;
            }
        }
        return -1;
    }

    public DataSet ToDataSet()
    {
        DataSet set = new DataSet();

        // Add patterns
        for (int patternIndex = 0; patternIndex < patterns.Count; ++patternIndex)
        {
            var editPattern = patterns[patternIndex];
            if (editPattern != null)
            {
                var tracks = editPattern.ToTracks(this, set.animationBits);
                set.animationBits.tracks.AddRange(tracks);
            }
        }

        for (int patternIndex = 0; patternIndex < rgbPatterns.Count; ++patternIndex)
        {
            var editPattern = rgbPatterns[patternIndex];
            if (editPattern != null)
            {
                var tracks = editPattern.ToRGBTracks(this, set.animationBits);
                set.animationBits.rgbTracks.AddRange(tracks);
            }
        }

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
