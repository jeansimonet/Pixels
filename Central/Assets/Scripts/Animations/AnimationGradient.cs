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
    public class AnimationGradient
        : Animation
    {
        public AnimationType type { get; set; } = AnimationType.Gradient;
        public byte padding_type { get; set; }
        public ushort duration { get; set; }
        public uint faceMask;
        public ushort gradientTrackOffset;
        public ushort gradientPadding;

        public AnimationInstance CreateInstance(DataSet.AnimationBits bits)
        {
            return new AnimationInstanceGradient(this, bits);
        }
    };

    /// <summary>
    /// Procedural on off animation instance data
    /// </summary>
    public class AnimationInstanceGradient
        : AnimationInstance
    {
        public AnimationInstanceGradient(Animation animation, DataSet.AnimationBits bits)
            : base(animation, bits)
        {
        }

        public override int updateLEDs(int ms, int[] retIndices, uint[] retColors)
        {
            int time = ms - startTime;
            var preset = getPreset();

            // Figure out the color from the gradient
            var gradient = animationBits.getRGBTrack(preset.gradientTrackOffset);
            int gradientTime = time * 1000 / preset.duration;
            uint color = gradient.evaluateColor(animationBits, gradientTime);

            // Fill the indices and colors for the anim controller to know how to update leds
            int retCount = 0;
            for (int i = 0; i < 20; ++i)
            {
                if ((preset.faceMask & (1 << i)) != 0)
                {
                    retIndices[retCount] = i;
                    retColors[retCount] = color;
                    retCount++;
                }
            }
            return retCount;
        }

        public override int stop(int[] retIndices)
        {
            var preset = getPreset();
            int retCount = 0;
            for (int i = 0; i < 20; ++i)
            {
                if ((preset.faceMask & (1 << i)) != 0)
                {
                    retIndices[retCount] = i;
                    retCount++;
                }
            }
            return retCount;
        }

        public AnimationGradient getPreset()
        {
            return (AnimationGradient)animationPreset;
        }
    };
}