using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;


namespace Animations
{
    public struct Constants
    {
        public const int MAX_KEYFRAMES = 8;
    }

    /// <summary>
    /// Stores a single keyframe of an LED animation
    /// </summary>
    public struct RGBKeyframe
    {
        public byte time;      // 0 - 255 (normalized)
        public byte red;       // 0 - 255 (normalized)
        public byte green;
        public byte blue;
    }

    /// <summary>
    /// An animation track is essentially a scaled animation curve for a
    /// specific LED. It defines how long the curve is stretched over and when it starts.
    /// With 8 keyframes, on the die, a track should take 8 * 4 + 8 = 40 bytes
    /// </summary>
    public struct RGBAnimationTrack
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.MAX_KEYFRAMES)]
        public RGBKeyframe[] keyframes;
        public short startTime;    // ms
        public short duration;     // ms
        public short padding;
        public byte ledIndex;      // 0 - 20
        public byte count;
    };

    /// <summary>
    /// A keyframe-based animation
    /// </summary>
    public class RGBAnimation
    {
	    public short duration; // ms
        public RGBAnimationTrack[] tracks;

        public static RGBAnimation ReadBytes(System.IntPtr ptr)
        {
            RGBAnimation ret = new RGBAnimation();
            ret.duration = Marshal.ReadInt16(ptr, 0);
            short count = Marshal.ReadInt16(ptr, 2);
            ret.tracks = new RGBAnimationTrack[count];

            int trackSize = Marshal.SizeOf(typeof(RGBAnimationTrack));
            for (int i = 0; i < count; ++i)
            {
                var trackPtr = new System.IntPtr(ptr.ToInt64() + 4 + i * trackSize);
                ret.tracks[i] = (RGBAnimationTrack)Marshal.PtrToStructure(trackPtr, typeof(RGBAnimationTrack));
            }

            return ret;
        }

        public static int ByteSize(RGBAnimation anim)
        {
            int trackSize = Marshal.SizeOf(typeof(RGBAnimationTrack));
            return trackSize * anim.tracks.Length + 4;
        }

        public static void WriteBytes(RGBAnimation anim, System.IntPtr ptr)
        {
            int trackSize = Marshal.SizeOf(typeof(RGBAnimationTrack));
            short count = (short)anim.tracks.Length;
            Marshal.WriteInt16(ptr, 0, anim.duration);
            Marshal.WriteInt16(ptr, 2, count);

            for (int i = 0; i < count; ++i)
            {
                var trackPtr = new System.IntPtr(ptr.ToInt64() + 4 + i * trackSize);
                Marshal.StructureToPtr(anim.tracks[i], trackPtr, false);
            }
        }

        public static byte[] ToByteArray(RGBAnimation anim)
        {
            int size = ByteSize(anim);
            System.IntPtr ptr = Marshal.AllocHGlobal(size);
            WriteBytes(anim, ptr);
            byte[] ret = new byte[size];
            Marshal.Copy(ptr, ret, 0, size);
            Marshal.FreeHGlobal(ptr);
            return ret;
        }

        public static RGBAnimation FromByteArray(byte[] data)
        {
            System.IntPtr ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, ptr, data.Length);
            RGBAnimation ret = ReadBytes(ptr);
            Marshal.FreeHGlobal(ptr);
            return ret;
        }
    }

    public struct AnimationSet
    {
        public RGBAnimation[] animations;
        public int GetTotalByteSize()
        {
            return animations.Sum(anim => RGBAnimation.ByteSize(anim));
        }
    }
}
