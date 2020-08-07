using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;

/// <summary>
/// Data Set is the set of all behaviors, conditions, rules, animations and colors
/// stored in the die. This data gets transfered straight to the dice. For that purpose,
/// the data is essentially 'exploded' into flat buffers. i.e. a all the keyframes of
/// all the anims are stored in a single keyframe array, and individual tracks reference
/// 'their' keyframes using an offset and count into that array.
/// </summary>
[System.Serializable]
public class DataSet
{
    public const int MAX_COLOR_MAP_SIZE = (1 << 7);
    public const int MAX_PALETTE_SIZE = MAX_COLOR_MAP_SIZE * 3;
    public const int SPECIAL_COLOR_INDEX = (MAX_COLOR_MAP_SIZE - 1);

    public List<Color> palette = new List<Color>();
    public List<Animations.RGBKeyframe> keyframes = new List<Animations.RGBKeyframe>();
    public List<Animations.RGBTrack> rgbTracks = new List<Animations.RGBTrack>();
    public List<Animations.Animation> animations = new List<Animations.Animation>();
    public List<Behaviors.Condition> conditions = new List<Behaviors.Condition>();
    public List<Behaviors.Action> actions = new List<Behaviors.Action>();
    public List<Behaviors.Rule> rules = new List<Behaviors.Rule>();
    public List<Behaviors.Behavior> behaviors = new List<Behaviors.Behavior>();
    public ushort currentBehaviorIndex;
    public ushort padding;
    public ushort heatTrackIndex;

    public int ComputeDataSetDataSize()
    {
        return palette.Count * Marshal.SizeOf<byte>() * 3 + // 3 bytes per color
            keyframes.Count * Marshal.SizeOf<Animations.RGBKeyframe>() +
            rgbTracks.Count * Marshal.SizeOf<Animations.RGBTrack>() +
            Utils.roundUpTo4(animations.Count * Marshal.SizeOf<ushort>()) + // offsets
            animations.Sum((anim) => Marshal.SizeOf(anim.GetType())) + // actual animations
            Utils.roundUpTo4(conditions.Count * Marshal.SizeOf<ushort>()) + // offsets
            conditions.Sum((cond) => Marshal.SizeOf(cond.GetType())) + // actual conditions
            Utils.roundUpTo4(actions.Count * Marshal.SizeOf<ushort>()) + // offsets
            actions.Sum((action) => Marshal.SizeOf(action.GetType())) + // actual actions
            rules.Count * Marshal.SizeOf<Behaviors.Rule>() + 
            behaviors.Count * Marshal.SizeOf<Behaviors.Behavior>();
    }

    public uint getColor32(ushort colorIndex)
    {
        var cl32 = (Color32)(palette[colorIndex]);
        return ColorUtils.toColor(cl32.r, cl32.g, cl32.b);
    }

    public Color getColor(ushort colorIndex)
    {
        return palette[colorIndex];
    }

    public Animations.RGBKeyframe getKeyframe(ushort keyFrameIndex)
    {
        return keyframes[keyFrameIndex];
    }

    public ushort getPaletteSize()
    {
        return (ushort)palette.Count;
    }

    public ushort getKeyframeCount()
    {
        return (ushort)keyframes.Count;
    }

    public Animations.RGBTrack getRGBTrack(ushort trackIndex)
    {
        return rgbTracks[trackIndex];
    }

    public ushort getRGBTrackCount()
    {
        return (ushort)rgbTracks.Count;
    }

    public Animations.Animation getAnimation(ushort animIndex)
    {
        return animations[animIndex];
    }

    public ushort getAnimationCount()
    {
        return (ushort)animations.Count;
    }

	public Behaviors.Condition getCondition(int conditionIndex)
    {
        return conditions[conditionIndex];
    }

	public ushort getConditionCount()
    {
        return (ushort)conditions.Count;
    }

	public Behaviors.Action getAction(int actionIndex)
    {
        return actions[actionIndex];
    }

	public ushort getActionCount()
    {
        return (ushort)actions.Count;
    }

	// Rules
	public Behaviors.Rule getRule(int ruleIndex)
    {
        return rules[ruleIndex];
    }

	public ushort getRuleCount()
    {
        return (ushort)rules.Count;
    }

	public Behaviors.Behavior getBehavior(int behaviorIndex)
    {
        return behaviors[behaviorIndex];
    }

	public ushort getBehaviorCount()
    {
        return (ushort)behaviors.Count;
    }

    public Animations.RGBTrack getHeatTrack()
    {
        return rgbTracks[heatTrackIndex];
    }

    public byte[] ToByteArray()
    {
        int size = ComputeDataSetDataSize();
        System.IntPtr ptr = Marshal.AllocHGlobal(size);

        // Copy palette
        System.IntPtr current = ptr;
        foreach (var color in palette)
        {
            Color32 cl32 = color;
            Marshal.WriteByte(current, cl32.r);
            current += 1;
            Marshal.WriteByte(current, cl32.g);
            current += 1;
            Marshal.WriteByte(current, cl32.b);
            current += 1;
        }

        // Copy keyframes
        foreach (var keyframe in keyframes)
        {
            Marshal.StructureToPtr(keyframe, current, false);
            current += Marshal.SizeOf<Animations.RGBKeyframe>();
        }

        // Copy rgb tracks
        foreach (var track in rgbTracks)
        {
            Marshal.StructureToPtr(track, current, false);
            current += Marshal.SizeOf<Animations.RGBTrack>();
        }

        // Copy animations
        // Offsets first
        short offset = 0;
        foreach (var anim in animations)
        {
            Marshal.WriteInt16(current, offset);
            current += Marshal.SizeOf<ushort>();
            offset += (short)Marshal.SizeOf(anim.GetType());
        }

        // Round up to nearest multiple of 4
        current += (int)(current.ToInt64() % 4);

        // Then animations
        foreach (var anim in animations)
        {
            Marshal.StructureToPtr(anim, current, false);
            current += Marshal.SizeOf(anim.GetType());
        }

        // Copy conditions
        // Offsets first
        offset = 0;
        foreach (var cond in conditions)
        {
            Marshal.WriteInt16(current, offset);
            current += Marshal.SizeOf<ushort>();
            offset += (short)Marshal.SizeOf(cond.GetType());
        }

        // Round up to nearest multiple of 4
        current += (int)(current.ToInt64() % 4);

        // Then animations
        foreach (var cond in conditions)
        {
            Marshal.StructureToPtr(cond, current, false);
            current += Marshal.SizeOf(cond.GetType());
        }

        // Copy actions
        // Offsets first
        offset = 0;
        foreach (var action in actions)
        {
            Marshal.WriteInt16(current, offset);
            current += Marshal.SizeOf<ushort>();
            offset += (short)Marshal.SizeOf(action.GetType());
        }

        // Round up to nearest multiple of 4
        current += (int)(current.ToInt64() % 4);

        // Then animations
        foreach (var action in actions)
        {
            Marshal.StructureToPtr(action, current, false);
            current += Marshal.SizeOf(action.GetType());
        }

        // Rules
        foreach (var rule in rules)
        {
            Marshal.StructureToPtr(rule, current, false);
            current += Marshal.SizeOf<Behaviors.Rule>();
        }

        // Behaviors
        foreach (var behavior in behaviors)
        {
            Marshal.StructureToPtr(behavior, current, false);
            current += Marshal.SizeOf<Behaviors.Behavior>();
        }

        byte[] ret = new byte[size];
        Marshal.Copy(ptr, ret, 0, size);
        Marshal.FreeHGlobal(ptr);
        return ret;
    }

    public void Compress()
    {
        // // First try to find identical sets of keyframes in tracks
        // for (int t = 0; t < rgbTracks.Count; ++t)
        // {
        //     Animations.RGBTrack trackT = rgbTracks[t];
        //     for (int r = t + 1; r < rgbTracks.Count; ++r)
        //     {
        //         Animations.RGBTrack trackR = rgbTracks[r];

        //         // Only try to collapse tracks that are not exactly the same
        //         if (!trackT.Equals(trackR))
        //         {
        //             if (trackR.keyFrameCount == trackT.keyFrameCount)
        //             {
        //                 // Compare actual keyframes
        //                 bool kfEquals = true;
        //                 for (int k = 0; k < trackR.keyFrameCount; ++k)
        //                 {
        //                     var kfRk = trackR.GetKeyframe(this, (ushort)k);
        //                     var kfTk = trackT.GetKeyframe(this, (ushort)k);
        //                     if (!kfRk.Equals(kfTk))
        //                     {
        //                         kfEquals = false;
        //                         break;
        //                     }
        //                 }

        //                 if (kfEquals)
        //                 {
        //                     // Sweet, we can compress the keyframes
        //                     // Fix up any other tracks
        //                     for (int i = 0; i < rgbTracks.Count; ++i)
        //                     {
        //                         Animations.RGBTrack tr = rgbTracks[i];
        //                         if (tr.keyframesOffset > trackR.keyframesOffset)
        //                         {
        //                             tr.keyframesOffset -= trackR.keyFrameCount;
        //                             rgbTracks[i] = tr;
        //                         }
        //                     }

        //                     // Remove the duplicate keyframes
        //                     var newKeyframes = new List<Animations.RGBKeyframe>(keyframes.Count - trackR.keyFrameCount);
        //                     for (int i = 0; i < trackR.keyframesOffset; ++i)
        //                     {
        //                         newKeyframes.Add(keyframes[i]);
        //                     }
        //                     for (int i = trackR.keyframesOffset + trackR.keyFrameCount; i < keyframes.Count; ++i)
        //                     {
        //                         newKeyframes.Add(keyframes[i]);
        //                     }
        //                     keyframes = newKeyframes;

        //                     // And make R point to the keyframes of T
        //                     trackR.keyframesOffset = trackT.keyframesOffset;
        //                     rgbTracks[r] = trackR;
        //                 }
        //             }
        //         }
        //     }
        // }

        // // Then remove duplicate RGB tracks
        // for (int t = 0; t < rgbTracks.Count; ++t)
        // {
        //     Animations.RGBTrack trackT = rgbTracks[t];
        //     for (int r = t + 1; r < rgbTracks.Count; ++r)
        //     {
        //         Animations.RGBTrack trackR = rgbTracks[r];
        //         if (trackR.Equals(trackT))
        //         {
        //             // Remove track R and fix anim tracks
        //             // Fix up other animation tracks
        //             for (int j = 0; j < tracks.Count; ++j)
        //             {
        //                 Animations.AnimationTrack trj = tracks[j];
        //                 if (trj.trackOffset == r)
        //                 {
        //                     trj.trackOffset = (ushort)t;
        //                 }
        //                 else if (trj.trackOffset > r)
        //                 {
        //                     trj.trackOffset--;
        //                 }
        //                 tracks[j] = trj;
        //             }

        //             if (r == heatTrackIndex)
        //             {
        //                 heatTrackIndex = (ushort)t;
        //             }
        //             else if (r < heatTrackIndex)
        //             {
        //                 heatTrackIndex--;
        //             }

        //             // Remove the duplicate RGBTrack
        //             var newRGBTracks = new List<Animations.RGBTrack>();
        //             for (int j = 0; j < r; ++j)
        //             {
        //                 newRGBTracks.Add(rgbTracks[j]);
        //             }
        //             for (int j = r + 1; j < rgbTracks.Count; ++j)
        //             {
        //                 newRGBTracks.Add(rgbTracks[j]);
        //             }
        //             rgbTracks = newRGBTracks;
        //         }
        //     }
        // }

        // // We should also remove duplicate anim tracks and animation
    }
}
