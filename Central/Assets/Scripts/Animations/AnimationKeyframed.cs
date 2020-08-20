using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


namespace Animations
{
    /// <summary>
    /// An animation track is essentially an animation curve for a specific LED.
    /// size: 8 bytes (+ the actual keyframe data)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public struct RGBTrack
    {
        public ushort keyframesOffset;  // offset into a global keyframe buffer
        public byte keyFrameCount;      // Keyframe count
        public byte padding;            // 
        public uint ledMask;            // Each bit indicates whether the led is included in the animation track

        public ushort getDuration(DataSet.AnimationBits bits)
        {
            var kf = bits.getRGBKeyframe((ushort)(keyframesOffset + keyFrameCount - 1));
            return kf.time();
        }

        public RGBKeyframe getKeyframe(DataSet.AnimationBits bits, ushort keyframeIndex)
        {
            Debug.Assert(keyframeIndex < keyFrameCount);
            return bits.getRGBKeyframe((ushort)(keyframesOffset + keyframeIndex));
        }

        /// <summary>
        /// Evaluate an animation track's for a given time, in milliseconds, and fills returns arrays of led indices and colors
        /// Values outside the track's range are clamped to first or last keyframe value.
        /// </summary>
        public int evaluate(DataSet.AnimationBits bits, int time, int[] retIndices, uint[] retColors)
        {
            if (keyFrameCount == 0)
                return 0;

            uint color = evaluateColor(bits, time);

            // Fill the return arrays
            int currentCount = 0;
            for (int i = 0; i < 20; ++i) { // <-- should come from somewhere!
                if ((ledMask & (1 << i)) != 0) {
                    retIndices[currentCount] = i;
                    retColors[currentCount] = color;
                    currentCount++;
                }
            }
            return currentCount;
        }

        /// <summary>
        /// Evaluate an animation track's for a given time, in milliseconds
        /// Values outside the track's range are clamped to first or last keyframe value.
        /// </summary>
        public uint evaluateColor(DataSet.AnimationBits bits, int time)
        {
            if (keyFrameCount == 0) {
                return 0;
            }
            
            // Find the first keyframe
            int nextIndex = 0;
            while (nextIndex < keyFrameCount && getKeyframe(bits, (ushort)nextIndex).time() < time) {
                nextIndex++;
            }

            uint color = 0;
            if (nextIndex == 0) {
                // The first keyframe is already after the requested time, clamp to first value
                color = getKeyframe(bits, (ushort)nextIndex).color(bits);
            } else if (nextIndex == keyFrameCount) {
                // The last keyframe is still before the requested time, clamp to the last value
                color = getKeyframe(bits, (ushort)(nextIndex- 1)).color(bits);
            } else {
                // Grab the prev and next keyframes
                var nextKeyframe = getKeyframe(bits, (ushort)nextIndex);
                ushort nextKeyframeTime = nextKeyframe.time();
                uint nextKeyframeColor = nextKeyframe.color(bits);

                var prevKeyframe = getKeyframe(bits, (ushort)(nextIndex - 1));
                ushort prevKeyframeTime = prevKeyframe.time();
                uint prevKeyframeColor = prevKeyframe.color(bits);

                // Compute the interpolation parameter
                color = ColorUtils.interpolateColors(prevKeyframeColor, prevKeyframeTime, nextKeyframeColor, nextKeyframeTime, time);
            }

            return color;
        }

        /// <summary>
        /// Extracts the LED indices from the led bit mask
        /// </sumary>
        public int extractLEDIndices(int[] retIndices)
        {
            // Fill the return arrays
            int currentCount = 0;
            for (int i = 0; i < 20; ++i) { // <-- 20 should come from somewhere...
                if ((ledMask & (1 << i)) != 0) {
                    retIndices[currentCount] = i;
                    currentCount++;
                }
            }
            return currentCount;
        }


        public bool Equals(RGBTrack other)
        {
            return keyframesOffset == other.keyframesOffset && keyFrameCount == other.keyFrameCount;
        }

    }

	/// <summary>
	/// A keyframe-based animation
	/// size: 8 bytes (+ actual track and keyframe data)
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
	public class AnimationKeyframed
		: Animation
	{
		public AnimationType type { get; set; } = AnimationType.Keyframed;
		public byte padding_type { get; set; } // to keep duration 16-bit aligned
		public ushort duration { get; set; } // in ms

		public byte padding2;
		public ushort tracksOffset; // offset into a global buffer of tracks
		public ushort trackCount;
		public byte padding3;
		public byte padding4;

        public AnimationInstance CreateInstance(DataSet.AnimationBits bits)
        {
            return new AnimationInstanceKeyframed(this, bits);
        }
	};

	/// <summary>
	/// Keyframe-based animation instance data
	/// </summary>
	public class AnimationInstanceKeyframed
		: AnimationInstance
	{
		public uint specialColorPayload; // meaning varies

        public AnimationInstanceKeyframed(AnimationKeyframed preset, DataSet.AnimationBits bits)
            : base(preset, bits)
        {
        }

		public override void start(int _startTime, byte _remapFace, bool _loop)
        {
            base.start(_startTime, _remapFace, _loop);
        }

        /// <summary>
        /// Computes the list of LEDs that need to be on, and what their intensities should be
        /// based on the different tracks of this animation.
        /// </summary>
		public override int updateLEDs(int ms, int[] retIndices, uint[] retColors)
        {
    		int time = ms - startTime;
            var preset = getPreset();
            // Each track will append its led indices and colors into the return array
            // The assumption is that led indices don't overlap between tracks of a single animation,
            // so there will always be enough room in the return arrays.
            int totalCount = 0;
            var indices = new int[20];
            var colors = new uint[20];
            for (int i = 0; i < preset.trackCount; ++i)
            {
                var track = animationBits.getRGBTrack((ushort)(preset.tracksOffset + i)); 
                int count = track.evaluate(animationBits, time, indices, colors);
                for (int j = 0; j < count; ++j)
                {
                    retIndices[totalCount+j] = indices[j];
                    retColors[totalCount+j] = colors[j];
                }
                totalCount += count;
            }
            return totalCount;
        }

		public override int stop(int[] retIndices)
        {
            var preset = getPreset();
            // Each track will append its led indices and colors into the return array
            // The assumption is that led indices don't overlap between tracks of a single animation,
            // so there will always be enough room in the return arrays.
            int totalCount = 0;
            var indices = new int[20];
            for (int i = 0; i < preset.trackCount; ++i)
            {
                var track = animationBits.getRGBTrack((ushort)(preset.tracksOffset + i)); 
                int count = track.extractLEDIndices(indices);
                for (int j = 0; j < count; ++j)
                {
                    retIndices[totalCount+j] = indices[j];
                }
                totalCount += count;
            }
            return totalCount;
        }

		public AnimationKeyframed getPreset()
        {
            return (AnimationKeyframed)animationPreset;
        }
	};
}