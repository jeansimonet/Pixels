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
		public AnimationType type { get; set; } = AnimationType.Rainbow;
		public byte padding_type { get; set; }
		public ushort duration { get; set; }
		public uint faceMask;
        public byte count;
        public byte fade;

        public AnimationInstance CreateInstance()
        {
            return new AnimationInstanceRainbow(this);
        }
	};

	/// <summary>
	/// Procedural on off animation instance data
	/// </summary>
	public class AnimationInstanceRainbow
		: AnimationInstance
	{
		public AnimationInstanceRainbow(Animation animation)
            : base(animation)
        {
        }

		public override int updateLEDs(DataSet set, int ms, int[] retIndices, uint[] retColors)
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

            color = ColorUtils.rainbowWheel((byte)wheelPos, intensity);

            // Fill the indices and colors for the anim controller to know how to update leds
            int retCount = 0;
            for (int i = 0; i < 20; ++i) {
                if ((preset.faceMask & (1 << i)) != 0)
                {
                    retIndices[retCount] = i;
                    retColors[retCount] = color;
                    retCount++;
                }
            }
            return retCount;
        }

		public override int stop(DataSet set, int[] retIndices)
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