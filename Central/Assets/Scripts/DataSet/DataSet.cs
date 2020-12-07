using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using System.Text;

/// <summary>
/// Data Set is the set of all behaviors, conditions, rules, animations and colors
/// stored in the die. This data gets transfered straight to the dice. For that purpose,
/// the data is essentially 'exploded' into flat buffers. i.e. a all the keyframes of
/// all the anims are stored in a single keyframe array, and individual tracks reference
/// 'their' keyframes using an offset and count into that array.
/// </summary>
[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public class DataSet
{
    public const int MAX_COLOR_MAP_SIZE = (1 << 7);
    public const int MAX_PALETTE_SIZE = MAX_COLOR_MAP_SIZE * 3;
    public const int SPECIAL_COLOR_INDEX = (MAX_COLOR_MAP_SIZE - 1);

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class AnimationBits
    {
        public List<Color> palette = new List<Color>();
        public List<Animations.RGBKeyframe> rgbKeyframes = new List<Animations.RGBKeyframe>();
        public List<Animations.RGBTrack> rgbTracks = new List<Animations.RGBTrack>();
        public List<Animations.Keyframe> keyframes = new List<Animations.Keyframe>();
        public List<Animations.Track> tracks = new List<Animations.Track>();

        public uint getColor32(ushort colorIndex)
        {

            var cl32 = (Color32)(getColor(colorIndex));
            return ColorUtils.toColor(cl32.r, cl32.g, cl32.b);
        }

        public const ushort PALETTE_COLOR_FROM_FACE = 127;
        public const ushort PALETTE_COLOR_FROM_RANDOM = 126;

        public Color getColor(ushort colorIndex)
        {
            if (colorIndex == PALETTE_COLOR_FROM_FACE)
            {
                return Color.blue;
            }
            else if (colorIndex == PALETTE_COLOR_FROM_RANDOM)
            {
                return Color.black;
            }
            else
            {
                return palette[colorIndex];
            }
        }
        public Animations.RGBKeyframe getRGBKeyframe(ushort keyFrameIndex) => rgbKeyframes[keyFrameIndex];
        public Animations.Keyframe getKeyframe(ushort keyFrameIndex) => keyframes[keyFrameIndex];
        public ushort getPaletteSize() => (ushort)(palette.Count * 3);
        public ushort getRGBKeyframeCount() => (ushort)rgbKeyframes.Count;
        public Animations.RGBTrack getRGBTrack(ushort trackIndex) => rgbTracks[trackIndex];
        public ushort getRGBTrackCount() => (ushort)rgbTracks.Count;
        public ushort getKeyframeCount() => (ushort)keyframes.Count;
        public Animations.Track getTrack(ushort trackIndex) => tracks[trackIndex];
        public ushort getTrackCount() => (ushort)tracks.Count;

        public int ComputeDataSize()
        {
            return Utils.roundUpTo4(palette.Count * Marshal.SizeOf<byte>() * 3) + // 3 bytes per color
                rgbKeyframes.Count * Marshal.SizeOf<Animations.RGBKeyframe>() +
                rgbTracks.Count * Marshal.SizeOf<Animations.RGBTrack>() +
                keyframes.Count * Marshal.SizeOf<Animations.Keyframe>() +
                tracks.Count * Marshal.SizeOf<Animations.Track>();
        }

        public System.IntPtr WriteBytes(System.IntPtr ptr)
        {
            // Copy palette
            System.IntPtr current = ptr;
            var currentCopy = current;
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

            // Round up to nearest multiple of 4
            current = currentCopy + Utils.roundUpTo4(palette.Count * 3 * Marshal.SizeOf<byte>());

            // Copy keyframes
            foreach (var keyframe in rgbKeyframes)
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

            // Copy keyframes
            foreach (var keyframe in keyframes)
            {
                Marshal.StructureToPtr(keyframe, current, false);
                current += Marshal.SizeOf<Animations.Keyframe>();
            }

            // Copy tracks
            foreach (var track in tracks)
            {
                Marshal.StructureToPtr(track, current, false);
                current += Marshal.SizeOf<Animations.Track>();
            }

            return current;
        }
    }

    public AnimationBits animationBits = new AnimationBits();
    public List<Animations.Animation> animations = new List<Animations.Animation>();
    public List<Behaviors.Condition> conditions = new List<Behaviors.Condition>();
    public List<Behaviors.Action> actions = new List<Behaviors.Action>();
    public List<Behaviors.Rule> rules = new List<Behaviors.Rule>();
    public Behaviors.Behavior behavior = null;
    public ushort padding;

    public int ComputeDataSetDataSize()
    {
        return animationBits.ComputeDataSize() +
            Utils.roundUpTo4(animations.Count * Marshal.SizeOf<ushort>()) + // offsets
            animations.Sum((anim) => Marshal.SizeOf(anim.GetType())) + // actual animations
            Utils.roundUpTo4(conditions.Count * Marshal.SizeOf<ushort>()) + // offsets
            conditions.Sum((cond) => Marshal.SizeOf(cond.GetType())) + // actual conditions
            Utils.roundUpTo4(actions.Count * Marshal.SizeOf<ushort>()) + // offsets
            actions.Sum((action) => Marshal.SizeOf(action.GetType())) + // actual actions
            rules.Count * Marshal.SizeOf<Behaviors.Rule>() + 
            Marshal.SizeOf<Behaviors.Behavior>();
    }

    public uint ComputeHash()
    {
        byte[] dataSetDataBytes = ToByteArray();

        StringBuilder hexdumpBuilder = new StringBuilder();
        for (int i = 0; i < dataSetDataBytes.Length; ++i)
        {
            if (i % 8 == 0)
            {
                hexdumpBuilder.AppendLine();
            }
            hexdumpBuilder.Append(dataSetDataBytes[i].ToString("X02") + " ");
        }
        Debug.Log(hexdumpBuilder.ToString());

        return Utils.computeHash(dataSetDataBytes);
    }

    public Animations.Animation getAnimation(ushort animIndex) => animations[animIndex];
    public ushort getAnimationCount() => (ushort)animations.Count;
	public Behaviors.Condition getCondition(int conditionIndex) => conditions[conditionIndex];
	public ushort getConditionCount() => (ushort)conditions.Count;
	public Behaviors.Action getAction(int actionIndex) => actions[actionIndex];
	public ushort getActionCount() => (ushort)actions.Count;
	public Behaviors.Rule getRule(int ruleIndex) => rules[ruleIndex];
	public ushort getRuleCount() => (ushort)rules.Count;
	public Behaviors.Behavior getBehavior() => behavior;

    public byte[] ToTestAnimationByteArray()
    {
        Debug.Assert(animations.Count == 1);
        int size = animationBits.ComputeDataSize() + Marshal.SizeOf(animations[0].GetType());
        System.IntPtr ptr = Marshal.AllocHGlobal(size);
        for (int i = 0; i < size; ++i)
        {
            Marshal.WriteByte(ptr + i, 0);
        }

        System.IntPtr current = animationBits.WriteBytes(ptr);
        Marshal.StructureToPtr(animations[0], current, false);

        byte[] ret = new byte[size];
        Marshal.Copy(ptr, ret, 0, size);
        Marshal.FreeHGlobal(ptr);
        return ret;
    }

    public byte[] ToByteArray()
    {
        int size = ComputeDataSetDataSize();
        System.IntPtr ptr = Marshal.AllocHGlobal(size);
        for (int i = 0; i < size; ++i)
        {
            Marshal.WriteByte(ptr + i, 0);
        }

        WriteBytes(ptr);

        byte[] ret = new byte[size];
        Marshal.Copy(ptr, ret, 0, size);
        Marshal.FreeHGlobal(ptr);

        return ret;
    }

    public System.IntPtr WriteBytes(System.IntPtr ptr)
    {
        // Copy palette
        System.IntPtr current = ptr;
        current = animationBits.WriteBytes(current);

        // Copy animations
        // Offsets first
        short offset = 0;
        var currentCopy = current;
        foreach (var anim in animations)
        {
            Marshal.WriteInt16(current, offset);
            current += Marshal.SizeOf<ushort>();
            offset += (short)Marshal.SizeOf(anim.GetType());
        }

        // Round up to nearest multiple of 4
        current = currentCopy + Utils.roundUpTo4(animations.Count * Marshal.SizeOf<ushort>());

        // Then animations
        foreach (var anim in animations)
        {
            Marshal.StructureToPtr(anim, current, false);
            current += Marshal.SizeOf(anim.GetType());
        }

        // Copy conditions
        // Offsets first
        offset = 0;
        currentCopy = current;
        foreach (var cond in conditions)
        {
            Marshal.WriteInt16(current, offset);
            current += Marshal.SizeOf<ushort>();
            offset += (short)Marshal.SizeOf(cond.GetType());
        }

        // Round up to nearest multiple of 4
        current = currentCopy + Utils.roundUpTo4(conditions.Count * Marshal.SizeOf<ushort>());

        // Then conditions
        foreach (var cond in conditions)
        {
            Marshal.StructureToPtr(cond, current, false);
            current += Marshal.SizeOf(cond.GetType());
        }

        // Copy actions
        // Offsets first
        offset = 0;
        currentCopy = current;
        foreach (var action in actions)
        {
            Marshal.WriteInt16(current, offset);
            current += Marshal.SizeOf<ushort>();
            offset += (short)Marshal.SizeOf(action.GetType());
        }

        // Round up to nearest multiple of 4
        current = currentCopy + Utils.roundUpTo4(actions.Count * Marshal.SizeOf<ushort>());

        // Then actions
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
        Marshal.StructureToPtr(behavior, current, false);
        current += Marshal.SizeOf<Behaviors.Behavior>();

        return current;
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
