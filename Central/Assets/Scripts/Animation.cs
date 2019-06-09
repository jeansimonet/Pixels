using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;


namespace Animations
{
    public static class Utils
    {
        public static uint toColor(byte red, byte green, byte blue)
        {
            return (uint)red << 16 | (uint)green << 8 | (uint)blue;
        }
        public static byte getRed(uint color)
        {
            return (byte)((color >> 16) & 0xFF);
        }
        public static byte getGreen(uint color)
        {
            return (byte)((color >> 8) & 0xFF);
        }
        public static byte getBlue(uint color)
        {
            return (byte)((color) & 0xFF);
        }
        public static byte getGreyscale(uint color)
        {
            return (byte)Mathf.Max(getRed(color), Mathf.Max(getGreen(color), getBlue(color)));
        }
    }

    /// <summary>
    /// Stores a single keyframe of an LED animation
    /// size: 2 bytes, split this way:
    /// - 9 bits: time 0 - 511 in 50th of a second (i.e )
    ///   + 1    -> 0.02s
    ///   + 500  -> 10s
    /// - 7 bits: color lookup (128 values)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RGBKeyframe
    {
		public ushort timeAndColor;

        public ushort time()
        {
            // Unpack
            uint time50th = ((uint)timeAndColor & 0b1111111110000000) >> 7;
            return (ushort)(time50th * 20);
        }

        public ushort colorIndex()
        {
            // Unpack
            return (ushort)(timeAndColor & 0b01111111);
        }

        public uint color(AnimationSet set)
        {
            return set.getColor(colorIndex());
        }

        public void setTimeAndColorIndex(ushort timeInMS, ushort colorIndex)
        {
            timeAndColor = (ushort)(((((uint)timeInMS / 20) & 0b111111111) << 7) |
                           ((uint)colorIndex & 0b1111111));
        }
    }

    /// <summary>
    /// An animation track is essentially an animation curve for a specific LED.
    /// size: 4 bytes (+ the actual keyframe data)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AnimationTrack
    {
        public ushort keyframesOffset; // offset into a global keyframe buffer
        public byte ledIndex;   // 0 - 20
        public byte keyFrameCount;      // Keyframe count

        public ref RGBKeyframe GetKeyframe(AnimationSet set, ushort keyframeIndex)
        {
            Debug.Assert(keyframeIndex < keyFrameCount);
            return ref set.getKeyframe((ushort)(keyframesOffset + keyframeIndex));
        }

        public uint evaluate(AnimationSet set, int time)
        {
            if (keyFrameCount == 0)
                return 0;

            // Find the first keyframe
            ushort nextIndex = 0;
            while (nextIndex < keyFrameCount && GetKeyframe(set, nextIndex).time() < time)
            {
                nextIndex++;
            }

            if (nextIndex == 0)
            {
                // The first keyframe is already after the requested time, clamp to first value
                return GetKeyframe(set, nextIndex).color(set);
            }
            else if (nextIndex == keyFrameCount)
            {
                // The last keyframe is still before the requested time, clamp to the last value
                return GetKeyframe(set, (ushort)(nextIndex - 1)).color(set);
            }
            else
            {
                // Grab the prev and next keyframes
                var nextKeyframe = GetKeyframe(set, nextIndex);
                ushort nextKeyframeTime = nextKeyframe.time();
                uint nextKeyframeColor = nextKeyframe.color(set);

                var prevKeyframe = GetKeyframe(set, (ushort)(nextIndex - 1));
                ushort prevKeyframeTime = prevKeyframe.time();
                uint prevKeyframeColor = prevKeyframe.color(set);

                // Compute the interpolation parameter
                // To stick to integer math, we'll scale the values
                int scaler = 1024;
                int scaledPercent = (time - prevKeyframeTime) * scaler / (nextKeyframeTime - prevKeyframeTime);
                int scaledRed = Utils.getRed(prevKeyframeColor) * (scaler - scaledPercent) + Utils.getRed(nextKeyframeColor) * scaledPercent;
                int scaledGreen = Utils.getGreen(prevKeyframeColor) * (scaler - scaledPercent) + Utils.getGreen(nextKeyframeColor) * scaledPercent;
                int scaledBlue = Utils.getBlue(prevKeyframeColor) * (scaler - scaledPercent) + Utils.getBlue(nextKeyframeColor) * scaledPercent;
                return Utils.toColor((byte)(scaledRed / scaler), (byte)(scaledGreen / scaler), (byte)(scaledBlue / scaler));
            }
        }
    }

    /// <summary>
    /// A keyframe-based animation
    /// size: 8 bytes (+ actual track and keyframe data)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Animation
    {
		public ushort duration; // in ms
        public ushort tracksOffset; // offset into a global buffer of tracks
        public ushort trackCount;
        public ushort padding;

        public ref AnimationTrack GetTrack(AnimationSet set, ushort index)
        {
            Debug.Assert(index < trackCount);
            return ref set.getTrack((ushort)(tracksOffset + index));
        }
    };

    public class AnimationSet
    {
        public const int PALETTE_SIZE = 128 * 3;

        public byte[] palette;
        public RGBKeyframe[] keyframes;
        public AnimationTrack[] tracks;
        public Animation[] animations;

        public AnimationSet()
        {
            palette = new byte[PALETTE_SIZE];
        }

        public int ComputeAnimationDataSize()
        {
            return palette.Length * Marshal.SizeOf(typeof(byte)) +
                Marshal.SizeOf<RGBKeyframe>() * keyframes.Length +
                Marshal.SizeOf<AnimationTrack>() * tracks.Length +
                Marshal.SizeOf<Animation>() * animations.Length;
        }

        public uint getColor(ushort colorIndex)
        {
            return Utils.toColor(
                palette[colorIndex * 3 + 0],
                palette[colorIndex * 3 + 1],
                palette[colorIndex * 3 + 2]);
        }

        public ref RGBKeyframe getKeyframe(ushort keyFrameIndex)
        {
            return ref keyframes[keyFrameIndex];
        }

        public ushort getKeyframeCount()
        {
            return (ushort)keyframes.Length;
        }

        public ref AnimationTrack getTrack(ushort trackIndex)
        {
            return ref tracks[trackIndex];
        }

        public ushort getTrackCount()
        {
            return (ushort)tracks.Length;
        }

        public ref Animation getAnimation(ushort animIndex)
        {
            return ref animations[animIndex];
        }

        public ushort getAnimationCount()
        {
            return (ushort)animations.Length;
        }

        public byte[] ToByteArray()
        {
            int size = ComputeAnimationDataSize();
            System.IntPtr ptr = Marshal.AllocHGlobal(size);
            System.IntPtr current = ptr;

            // Copy palette
            Marshal.Copy(palette, 0, current, palette.Length);
            current += palette.Length;

            // Copy keyframes
            foreach (var keyframe in keyframes)
            {
                Marshal.StructureToPtr(keyframe, current, false);
                current += Marshal.SizeOf<RGBKeyframe>();
            }

            // Copy tracks
            foreach (var track in tracks)
            {
                Marshal.StructureToPtr(track, current, false);
                current += Marshal.SizeOf<AnimationTrack>();
            }

            // Copy animations
            foreach (var anim in animations)
            {
                Marshal.StructureToPtr(anim, current, false);
                current += Marshal.SizeOf<Animation>();
            }
            byte[] ret = new byte[size];
            Marshal.Copy(ptr, ret, 0, size);
            Marshal.FreeHGlobal(ptr);
            return ret;
        }

    }

    public class EditPalette
    {
        public Color[] colors = new Color[128];
    }

    public class EditKeyframe
    {
        public float time = -1;
        public int colorIndex = -1;
    }

    public class EditTrack
    {
        public int ledIndex = -1;
        public float duration { get { return keyframes.Max(k => k.time); } }
        public List<EditKeyframe> keyframes = new List<EditKeyframe>();
    }

    public class EditAnimation
    {
        public float duration { get { return tracks.Max(t => t.duration); } }
        public List<EditTrack> tracks = new List<EditTrack>();
    }

    public class EditAnimationSet
    {
        public EditPalette palette = new EditPalette();
        public List<EditAnimation> animations = new List<EditAnimation>();

        public void FromAnimationSet(AnimationSet set)
        {
            for (int i = 0; i < palette.colors.Length; ++i)
            {
                palette.colors[i] = new Color(
                    (float)set.palette[i * 3 + 0] / 255.0f,
                    (float)set.palette[i * 3 + 1] / 255.0f,
                    (float)set.palette[i * 3 + 2] / 255.0f);
            }

            // Reset the animations, and read them in!
            animations = new List<EditAnimation>();
            for (int i = 0; i < set.animations.Length; ++i)
            {
                var anim = set.getAnimation((ushort)i);
                var editAnim = new EditAnimation();
                for (int j = 0; j < anim.trackCount; ++j)
                {
                    var track = anim.GetTrack(set, (ushort)j);
                    var editTrack = new EditTrack();
                    editTrack.ledIndex = track.ledIndex;
                    for (int k = 0; k < track.keyFrameCount; ++k)
                    {
                        var kf = track.GetKeyframe(set, (ushort)k);
                        var editKf = new EditKeyframe();
                        editKf.time = (float)kf.time() / 1000.0f;
                        editKf.colorIndex = kf.colorIndex();
                        editTrack.keyframes.Add(editKf);
                    }
                    editAnim.tracks.Add(editTrack);
                }
                animations.Add(editAnim);
            }
        }

        public AnimationSet ToAnimationSet()
        {
            AnimationSet set = new AnimationSet();
            for (int i = 0; i < palette.colors.Length; ++i)
            {
                set.palette[i * 3 + 0] = (byte)(palette.colors[i].r * 255.0f);
                set.palette[i * 3 + 1] = (byte)(palette.colors[i].g * 255.0f);
                set.palette[i * 3 + 2] = (byte)(palette.colors[i].b * 255.0f);
            }

            int currentTrackOffset = 0;
            int currentKeyframeOffset = 0;

            var anims = new List<Animation>();
            var tracks = new List<AnimationTrack>();
            var keyframes = new List<RGBKeyframe>();

            // Add animations
            for (int i = 0; i < animations.Count; ++i)
            {
                var editAnim = animations[i];
                var anim = new Animation();
                anim.duration = (ushort)(editAnim.duration * 1000.0f);
                anim.tracksOffset = (ushort)currentTrackOffset;
                anim.trackCount = (ushort)editAnim.tracks.Count;
                anims.Add(anim);

                // Now add tracks
                for (int j = 0; j < editAnim.tracks.Count; ++j)
                {
                    var editTrack = editAnim.tracks[j];
                    var track = new AnimationTrack();
                    track.ledIndex = (byte)editTrack.ledIndex;
                    track.keyframesOffset = (ushort)currentKeyframeOffset;
                    track.keyFrameCount = (byte)editTrack.keyframes.Count;
                    tracks.Add(track);

                    // Now add keyframes
                    for (int k = 0; k < editTrack.keyframes.Count; ++k)
                    {
                        var editKeyframe = editTrack.keyframes[k];
                        var keyframe = new RGBKeyframe();
                        keyframe.setTimeAndColorIndex((ushort)(editKeyframe.time * 1000.0f), (ushort)editKeyframe.colorIndex);
                        keyframes.Add(keyframe);
                    }
                    currentKeyframeOffset += editTrack.keyframes.Count;
                }
                currentTrackOffset += editAnim.tracks.Count;
            }

            set.keyframes = keyframes.ToArray();
            set.tracks = tracks.ToArray();
            set.animations = anims.ToArray();

            return set;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("Animations: ");
            builder.Append(animations.Count);
            builder.AppendLine();
            for (int i = 0; i < animations.Count; ++i)
            {
                var anim = animations[i];
                builder.Append("\tAnim ");
                builder.Append(i);
                builder.Append(" contains ");
                builder.Append(anim.tracks.Count);
                builder.Append(" tracks");
                builder.AppendLine();
                for (int j = 0; j < anim.tracks.Count; ++j)
                {
                    var track = anim.tracks[j];
                    builder.Append("\t\tTrack ");
                    builder.Append(j);
                    builder.Append(" contains ");
                    builder.Append(track.keyframes.Count);
                    builder.Append(" keyframes");
                    builder.AppendLine();
                    for (int k = 0; k < track.keyframes.Count; ++k)
                    {
                        var keyframe = track.keyframes[k];
                        builder.Append("\t\t\tTime ");
                        builder.Append(keyframe.time);
                        builder.Append(" : ");
                        builder.Append(keyframe.colorIndex);
                        builder.AppendLine();
                    }
                }
            }
            return builder.ToString();
        }

        public static EditAnimationSet CreateTestSet()
        {
            EditAnimationSet set = new EditAnimationSet();
            for (int r = 0; r < 8; ++r)
            {
                float red = (float)r / 8;
                for (int g = 0; g < 4; ++g)
                {
                    float green = (float)g / 4;
                    for (int b = 0; b < 4; ++b)
                    {
                        float blue = (float)b / 4;
                        set.palette.colors[r * 4 * 4 + g * 4 + b] = new Color(red, green, blue);
                    }
                }
            }
            for (int a = 0; a < 5; ++a)
            {
                EditAnimation anim = new EditAnimation();
                for (int i = 0; i < a + 1; ++i)
                {
                    var track = new EditTrack();
                    track.ledIndex = i;
                    for (int j = 0; j < 3; ++j)
                    {
                        var kf = new EditKeyframe();
                        kf.time = j;
                        kf.colorIndex = a * 10 + i * 3 + j;
                        track.keyframes.Add(kf);
                    }
                    anim.tracks.Add(track);
                }
                set.animations.Add(anim);
            }

            return set;
        }
    }

}
