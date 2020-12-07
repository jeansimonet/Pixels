using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Animations
{
	/// <summary>
	/// Procedural rainbow animation
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
	public class AnimationRainbow
		: Animation
	{
        // face -> led:
        //  0   1   2   3   4    5  6    7  8    9 10   11 12   13 14   15  16  17  18 19
        // 15,	1,	17,	4,	13,	7,	19,	9,	6,	10,	5,	11,	14,	3,	12,	8,	18,	0,	16,	2
        // led -> face:
        // 0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19
        // 17, 1, 19, 13, 3, 10, 8, 5, 15, 7, 9, 11, 14, 4, 12, 0, 18, 2, 16, 6
        public static int[] faceIndices = new int[] {  17, 1, 19, 13, 3, 10, 8, 5, 15, 7, 9, 11, 14, 4, 12, 0, 18, 2, 16, 6 };
		public AnimationType type { get; set; } = AnimationType.Rainbow;
		public byte padding_type { get; set; }
		public ushort duration { get; set; }
		public uint faceMask;
        public byte count;
        public byte fade;
        public byte traveling;
        public byte paddingTraveling;

        public AnimationInstance CreateInstance(DataSet.AnimationBits bits)
        {
            return new AnimationInstanceRainbow(this, bits);
        }
	};

	/// <summary>
	/// Procedural on off animation instance data
	/// </summary>
	public class AnimationInstanceRainbow
		: AnimationInstance
	{
		public AnimationInstanceRainbow(Animation animation, DataSet.AnimationBits bits)
            : base(animation, bits)
        {
        }

		public override int updateLEDs(int ms, int[] retIndices, uint[] retColors)
        {
            var preset = getPreset();

            // Compute color
            uint color = 0;
            int fadeTime = preset.duration * preset.fade / (255 * 2);
            int time = (ms - startTime);

            int wheelPos = (time * preset.count * 255 / preset.duration) % 256;

            byte intensity = 255;
            if (time <= fadeTime) {
                // Ramp up
                intensity = (byte)(time * 255 / fadeTime);
            } else if (time >= (preset.duration - fadeTime)) {
                // Ramp down
                intensity = (byte)((preset.duration - time) * 255 / fadeTime);
            }

            int retCount = 0;
            if (preset.traveling != 0) {
                // Fill the indices and colors for the anim controller to know how to update leds
                for (int i = 0; i < 20; ++i) {
                    if ((preset.faceMask & (1 << i)) != 0)
                    {
                        retIndices[retCount] = AnimationRainbow.faceIndices[i];
                        retColors[retCount] = ColorUtils.gamma(ColorUtils.rainbowWheel((byte)((wheelPos + i * 256 / 20) % 256), intensity));
                        retCount++;
                    }
                }
            } else {
                // All leds same color
                color = ColorUtils.gamma(ColorUtils.rainbowWheel((byte)wheelPos, intensity));

                // Fill the indices and colors for the anim controller to know how to update leds
                for (int i = 0; i < 20; ++i) {
                    if ((preset.faceMask & (1 << i)) != 0)
                    {
                        retIndices[retCount] = i;
                        retColors[retCount] = color;
                        retCount++;
                    }
                }
            }

            return retCount;
        }

		public override int stop(int[] retIndices)
        {
            var preset = getPreset();
            int retCount = 0;
            for (int i = 0; i < 20; ++i) {
                if ((preset.faceMask & (1 << i)) != 0)
                {
                    retIndices[retCount] = i;
                    retCount++;
                }
            }
            return retCount;
        }

		public AnimationRainbow getPreset()
        {
            return (AnimationRainbow)animationPreset;
        }
	};
}