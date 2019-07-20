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
    [System.Serializable]
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
            return set.getColor32(colorIndex());
        }

        public void setTimeAndColorIndex(ushort timeInMS, ushort colorIndex)
        {
            timeAndColor = (ushort)(((((uint)timeInMS / 20) & 0b111111111) << 7) |
                           ((uint)colorIndex & 0b1111111));
        }

        public bool Equals(RGBKeyframe other)
        {
            return timeAndColor == other.timeAndColor;
        }
    }

    /// <summary>
    /// An animation track is essentially an animation curve for a specific LED.
    /// size: 4 bytes (+ the actual keyframe data)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public struct RGBTrack
    {
        public ushort keyframesOffset; // offset into a global keyframe buffer
        public byte keyFrameCount;      // Keyframe count
        public byte padding;   // 0 - 20

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

        public bool Equals(RGBTrack other)
        {
            return keyframesOffset == other.keyframesOffset && keyFrameCount == other.keyFrameCount;
        }

    }


    /// <summary>
    /// An animation track is essentially an animation curve for a specific LED.
    /// size: 4 bytes (+ the actual keyframe data)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public struct AnimationTrack
    {
        public ushort trackOffset; // offset into a global keyframe buffer
        public byte ledIndex;   // 0 - 20
        public byte padding;      // Keyframe count

        public ref RGBTrack GetTrack(AnimationSet set)
        {
            return ref set.getRGBTrack(trackOffset);
        }

        public uint evaluate(AnimationSet set, int time)
        {
            return GetTrack(set).evaluate(set, time);
        }

        public bool Equals(AnimationTrack other)
        {
            return trackOffset == other.trackOffset && ledIndex == other.ledIndex;
        }
    }

    /// <summary>
    /// A keyframe-based animation
    /// size: 8 bytes (+ actual track and keyframe data)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
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

        public bool Equals(Animation other)
        {
            return duration == other.duration && tracksOffset == other.tracksOffset && trackCount == other.trackCount;
        }
    };

    /// <summary>
    /// Animation Set is the set of all animations and colors stored in the die
    /// This data gets transfered straight to the dice. For that purpose, the data
    /// is essentially 'exploded' into flat buffers. i.e. a all the keyframes of all the
    /// anims are stored in a single keyframe array, and individual tracks reference
    /// 'their' keyframes using an offset and count into that array.
    /// </summary>
    [System.Serializable]
    public class AnimationSet
    {
        public const int MAX_PALETTE_SIZE = 128 * 3;

        public byte[] palette;
        public RGBKeyframe[] keyframes;
        public RGBTrack[] rgbTracks;
        public AnimationTrack[] tracks;
        public Animation[] animations;

        public int ComputeAnimationDataSize()
        {
            return palette.Length * Marshal.SizeOf(typeof(byte)) +
                Marshal.SizeOf<RGBKeyframe>() * keyframes.Length +
                Marshal.SizeOf<RGBTrack>() * rgbTracks.Length +
                Marshal.SizeOf<AnimationTrack>() * tracks.Length +
                Marshal.SizeOf<Animation>() * animations.Length;
        }

        public uint getColor32(ushort colorIndex)
        {
            return Utils.toColor(
                palette[colorIndex * 3 + 0],
                palette[colorIndex * 3 + 1],
                palette[colorIndex * 3 + 2]);
        }

        public Color getColor(ushort colorIndex)
        {
            return new Color(
                palette[colorIndex * 3 + 0] / 255.0f,
                palette[colorIndex * 3 + 1] / 255.0f,
                palette[colorIndex * 3 + 2] / 255.0f);
        }

        public ref RGBKeyframe getKeyframe(ushort keyFrameIndex)
        {
            return ref keyframes[keyFrameIndex];
        }

        public ushort getPaletteSize()
        {
            return (ushort)palette.Length;
        }

        public ushort getKeyframeCount()
        {
            return (ushort)keyframes.Length;
        }

        public ref RGBTrack getRGBTrack(ushort trackIndex)
        {
            return ref rgbTracks[trackIndex];
        }

        public ushort getRGBTrackCount()
        {
            return (ushort)rgbTracks.Length;
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

            // Copy rgb tracks
            foreach (var track in rgbTracks)
            {
                Marshal.StructureToPtr(track, current, false);
                current += Marshal.SizeOf<RGBTrack>();
            }

            // Copy animation tracks
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

        public void Compress()
        {
            // First try to find identical sets of keyframes in tracks
            for (int t = 0; t < rgbTracks.Length; ++t)
            {
                RGBTrack trackT = rgbTracks[t];
                for (int r = t + 1; r < rgbTracks.Length; ++r)
                {
                    RGBTrack trackR = rgbTracks[r];

                    // Only try to collapse tracks that are not exactly the same
                    if (!trackT.Equals(trackR))
                    {
                        if (trackR.keyFrameCount == trackT.keyFrameCount)
                        {
                            // Compare actual keyframes
                            bool kfEquals = true;
                            for (int k = 0; k < trackR.keyFrameCount; ++k)
                            {
                                var kfRk = trackR.GetKeyframe(this, (ushort)k);
                                var kfTk = trackT.GetKeyframe(this, (ushort)k);
                                if (!kfRk.Equals(kfTk))
                                {
                                    kfEquals = false;
                                    break;
                                }
                            }

                            if (kfEquals)
                            {
                                // Sweet, we can compress the keyframes
                                // Fix up any other tracks
                                for (int i = 0; i < rgbTracks.Length; ++i)
                                {
                                    RGBTrack tr = rgbTracks[i];
                                    if (tr.keyframesOffset > trackR.keyframesOffset)
                                    {
                                        tr.keyframesOffset -= trackR.keyFrameCount;
                                        rgbTracks[i] = tr;
                                    }
                                }

                                // Remove the duplicate keyframes
                                var newKeyframes = new RGBKeyframe[keyframes.Length - trackR.keyFrameCount];
                                for (int i = 0; i < trackR.keyframesOffset; ++i)
                                {
                                    newKeyframes[i] = keyframes[i];
                                }
                                for (int i = trackR.keyframesOffset + trackR.keyFrameCount; i < keyframes.Length; ++i)
                                {
                                    newKeyframes[i - trackR.keyFrameCount] = keyframes[i];
                                }
                                keyframes = newKeyframes;

                                // And make R point to the keyframes of T
                                trackR.keyframesOffset = trackT.keyframesOffset;
                                rgbTracks[r] = trackR;
                            }
                        }
                    }
                }
            }

            // Then remove duplicate RGB tracks
            for (int t = 0; t < rgbTracks.Length; ++t)
            {
                ref RGBTrack trackT = ref rgbTracks[t];
                for (int r = t + 1; r < rgbTracks.Length; ++r)
                {
                    ref RGBTrack trackR = ref rgbTracks[r];
                    if (trackR.Equals(trackT))
                    {
                        // Remove track R and fix anim tracks
                        // Fix up other animation tracks
                        for (int j = 0; j < tracks.Length; ++j)
                        {
                            ref AnimationTrack trj = ref tracks[j];
                            if (trj.trackOffset == r)
                            {
                                trj.trackOffset = (ushort)t;
                            }
                            else if (trj.trackOffset > r)
                            {
                                trj.trackOffset--;
                            }
                        }

                        // Remove the duplicate RGBTrack
                        var newRGBTracks = new RGBTrack[rgbTracks.Length - 1];
                        for (int j = 0; j < r; ++j)
                        {
                            newRGBTracks[j] = rgbTracks[j];
                        }
                        for (int j = r + 1; j < rgbTracks.Length; ++j)
                        {
                            newRGBTracks[j - 1] = rgbTracks[j];
                        }
                        rgbTracks = newRGBTracks;
                    }
                }
            }

            // We should also remove duplicate anim tracks and animation
        }
    }
}
