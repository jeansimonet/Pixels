using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Animations
{
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

        public uint color(DataSet.AnimationBits bits)
        {
            return bits.getColor32(colorIndex());
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
    /// Stores a single keyframe of an LED animation
    /// size: 2 bytes, split this way:
    /// - 9 bits: time 0 - 511 in 50th of a second (i.e )
    ///   + 1    -> 0.02s
    ///   + 500  -> 10s
    /// - 7 bits: intensity (0 - 127)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public struct Keyframe
    {
		public ushort timeAndIntensity;

        public ushort time()
        {
            // Unpack
            uint time50th = ((uint)timeAndIntensity & 0b1111111110000000) >> 7;
            return (ushort)(time50th * 20);
        }

        public byte intensity()
        {
            // Unpack
            return (byte)((timeAndIntensity & 0b01111111) * 2); // Scale it to 0 -> 255
        }

        public void setTimeAndIntensity(ushort timeInMS, byte intensity)
        {
            timeAndIntensity = (ushort)(((((uint)timeInMS / 20) & 0b111111111) << 7) |
                           ((uint)(intensity / 2) & 0b1111111));
        }

        public bool Equals(Keyframe other)
        {
            return timeAndIntensity == other.timeAndIntensity;
        }
    }


}