using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animations
{
    public enum AnimationSimpleLEDType : byte
    {
        OneLED,
        AllLEDs,
    };

	/// <summary>
	/// Procedural on off animation
	/// </summary>
	public class AnimationSimple
		: Animation
	{
		public AnimationType type { get; set; } = AnimationType.Simple;
		public byte padding_type { get; set; }
		public ushort duration { get; set; }

		public AnimationSimpleLEDType ledType;
        public ushort padding_ledType;
        public uint color;
	};

	/// <summary>
	/// Procedural on off animation instance data
	/// </summary>
	public class AnimationInstanceSimple
		: AnimationInstance
	{
		public AnimationInstanceSimple(Animation animation)
            : base(animation)
        {
        }

		public override void start(DataSet _set, int _startTime, byte _remapFace, bool _loop)
        {
            base.start(_set, _startTime, _remapFace, _loop);
        }

		public override int updateLEDs(DataSet set, int ms, int[] retIndices, uint[] retColors)
        {
            var preset = getPreset();

            // Compute color
            uint black = 0;
            uint color = 0;
            int halfTime = preset.duration / 2;
            int time = ms - startTime;

            if (time <= halfTime) {
                // Ramp up
                color = ColorUtils.interpolateColors(black, 0, preset.color, halfTime, time);
            } else {
                // Ramp down
                color = ColorUtils.interpolateColors(preset.color, halfTime, black, preset.duration, time);
            }

            // Fill the indices and colors for the anim controller to know how to update leds
            int retCount = 0;
            switch (preset.ledType) {
                case AnimationSimpleLEDType.OneLED:
                    retIndices[0] = 0; // this will get remapped to the current up face
                    retColors[0] = color;
                    retCount = 1;
                    break;
                case AnimationSimpleLEDType.AllLEDs:
                    retCount = 20; //Config::BoardManager::getBoard()->ledCount;
                    for (int i = 0; i < retCount; ++i) {
                        retIndices[i] = i;
                        retColors[i] = color;
                    }
                    break;
            }
            return retCount;
        }

		public override int stop(DataSet set, int[] retIndices)
        {
            var preset = getPreset();
            int retCount = 0;
            switch (preset.ledType) {
                case AnimationSimpleLEDType.OneLED:
                    retIndices[0] = 0;
                    retCount = 1;
                    break;
                case AnimationSimpleLEDType.AllLEDs:
                    retCount = 20;
                    for (int i = 0; i < retCount; ++i) {
                        retIndices[i] = i;
                    }
                    break;
            }
            return retCount;
        }

		public AnimationSimple getPreset()
        {
            return (AnimationSimple)animationPreset;
        }
	};
}