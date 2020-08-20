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
    public struct Track
    {
        public ushort keyframesOffset;  // offset into a global keyframe buffer
        public byte keyFrameCount;      // Keyframe count
        public byte padding;            // 
        public uint ledMask;            // Each bit indicates whether the led is included in the animation track

        public ushort getDuration(DataSet set)
        {
            var kf = set.getRGBKeyframe((ushort)(keyframesOffset + keyFrameCount - 1));
            return kf.time();
        }

        public Keyframe getKeyframe(DataSet set, ushort keyframeIndex)
        {
            Debug.Assert(keyframeIndex < keyFrameCount);
            return set.getKeyframe((ushort)(keyframesOffset + keyframeIndex));
        }

        /// <summary>
        /// Evaluate an animation track's for a given time, in milliseconds, and fills returns arrays of led indices and colors
        /// Values outside the track's range are clamped to first or last keyframe value.
        /// </summary>
        public int evaluate(DataSet set, uint color, int time, int[] retIndices, uint[] retColors)
        {
            if (keyFrameCount == 0)
                return 0;

            uint mcolor = modulateColor(set, color, time);

            // Fill the return arrays
            int currentCount = 0;
            for (int i = 0; i < 20; ++i) { // <-- should come from somewhere!
                if ((ledMask & (1 << i)) != 0) {
                    retIndices[currentCount] = i;
                    retColors[currentCount] = mcolor;
                    currentCount++;
                }
            }
            return currentCount;
        }

        /// <summary>
        /// Evaluate an animation track's for a given time, in milliseconds
        /// Values outside the track's range are clamped to first or last keyframe value.
        /// </summary>
        public uint modulateColor(DataSet set, uint color, int time)
        {
            // Find the first keyframe
            int nextIndex = 0;
            while (nextIndex < keyFrameCount && getKeyframe(set, (ushort)nextIndex).time() < time) {
                nextIndex++;
            }

            byte intensity = 0;
            if (nextIndex == 0) {
                // The first keyframe is already after the requested time, clamp to first value
                intensity = getKeyframe(set, (ushort)nextIndex).intensity();
            } else if (nextIndex == keyFrameCount) {
                // The last keyframe is still before the requested time, clamp to the last value
                intensity = getKeyframe(set, (ushort)(nextIndex- 1)).intensity();
            } else {
                // Grab the prev and next keyframes
                var nextKeyframe = getKeyframe(set, (ushort)nextIndex);
                ushort nextKeyframeTime = nextKeyframe.time();
                byte nextKeyframeIntensity = nextKeyframe.intensity();

                var prevKeyframe = getKeyframe(set, (ushort)(nextIndex - 1));
                ushort prevKeyframeTime = prevKeyframe.time();
                byte prevKeyframeIntensity = prevKeyframe.intensity();

                // Compute the interpolation parameter
                intensity = ColorUtils.interpolateIntensity(prevKeyframeIntensity, prevKeyframeTime, nextKeyframeIntensity, nextKeyframeTime, time);
            }

            return ColorUtils.modulateColor(color, intensity);
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
	/// A keyframe-based animation with a gradient applied over
	/// size: 8 bytes (+ actual track and keyframe data)
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
	public class AnimationGradientPattern
		: Animation
	{
		public AnimationType type { get; set; } = AnimationType.GradientPattern;
		public byte padding_type { get; set; } // to keep duration 16-bit aligned
		public ushort duration { get; set; } // in ms

		public SpecialColor specialColorType; // is really SpecialColor
        public byte padding_specialColor;
		public ushort tracksOffset; // offset into a global buffer of tracks
		public ushort trackCount;
		public ushort gradientTrackOffset;

        public AnimationInstance CreateInstance()
        {
            return new AnimationInstanceGradientPattern(this);
        }
	};

	/// <summary>
	/// Keyframe-based animation instance data
	/// </summary>
	public class AnimationInstanceGradientPattern
		: AnimationInstance
		, IAnimationSpecialColorToken
	{
		public uint specialColorPayload; // meaning varies

        public AnimationInstanceGradientPattern(AnimationGradientPattern preset)
            : base(preset)
        {
        }

		public override void start(DataSet _set, int _startTime, byte _remapFace, bool _loop)
        {
            base.start(_set, _startTime, _remapFace, _loop);

            switch (getPreset().specialColorType)
            {
                case SpecialColor.Face:
                    // Store a color based on the face
                    //specialColorPayload = Rainbow.faceWheel(_remapFace, 20);
                    break;
                case SpecialColor.ColorWheel:
                    // Store the face index
                    specialColorPayload = _remapFace;
                    break;
                case SpecialColor.HeatStart:
                    {
                        var trk = set.getHeatTrack();
                        ushort heatMs = (ushort)(trk.getDuration(set) / 2);
                        specialColorPayload = trk.evaluateColor(set, null, heatMs);
                    }
                    break;
                default:
                    // Other cases don't need any per-instance payload
                    specialColorPayload = 0;
                    break;
            }
        }

        /// <summary>
        /// Computes the list of LEDs that need to be on, and what their intensities should be
        /// based on the different tracks of this animation.
        /// </summary>
		public override int updateLEDs(DataSet set, int ms, int[] retIndices, uint[] retColors)
        {
    		int time = ms - startTime;
            var preset = getPreset();

            // Figure out the color from the gradient
            var gradient = set.getRGBTrack(preset.gradientTrackOffset);
            int gradientTime = time * 512 / preset.duration;
            uint gradientColor = gradient.evaluateColor(set, this, gradientTime);

            // Each track will append its led indices and colors into the return array
            // The assumption is that led indices don't overlap between tracks of a single animation,
            // so there will always be enough room in the return arrays.
            int totalCount = 0;
            var indices = new int[20];
            var colors = new uint[20];
            for (int i = 0; i < preset.trackCount; ++i)
            {
                var track = set.getTrack((ushort)(preset.tracksOffset + i)); 
                int count = track.evaluate(set, gradientColor, time, indices, colors);
                for (int j = 0; j < count; ++j)
                {
                    retIndices[totalCount+j] = indices[j];
                    retColors[totalCount+j] = colors[j];
                }
                totalCount += count;
            }
            return totalCount;
        }

		public override int stop(DataSet set, int[] retIndices)
        {
            var preset = getPreset();
            // Each track will append its led indices and colors into the return array
            // The assumption is that led indices don't overlap between tracks of a single animation,
            // so there will always be enough room in the return arrays.
            int totalCount = 0;
            var indices = new int[20];
            for (int i = 0; i < preset.trackCount; ++i)
            {
                var track = set.getRGBTrack((ushort)(preset.tracksOffset + i)); 
                int count = track.extractLEDIndices(indices);
                for (int j = 0; j < count; ++j)
                {
                    retIndices[totalCount+j] = indices[j];
                }
                totalCount += count;
            }
            return totalCount;
        }

		public AnimationGradientPattern getPreset()
        {
            return (AnimationGradientPattern)animationPreset;
        }

		public uint getColor(DataSet set, uint colorIndex)
        {
            var preset = getPreset();
            switch (preset.specialColorType) {
                case SpecialColor.Face:
                case SpecialColor.HeatStart:
                    // The payload is the color
                    return specialColorPayload;
                case SpecialColor.ColorWheel:
                    {
                        // // Use the global rainbow
                        // int index = Modules::AnimController::getCurrentRainbowOffset();
                        // if (index < 0) {
                        //     index += 256;
                        // }
                        // return Rainbow::wheel((uint8_t)index);
                        return 0;
                    }
                case SpecialColor.HeatCurrent:
                    {
                        var trk = set.getHeatTrack();
                        ushort heatMs = (ushort)(trk.getDuration(set) / 2);
                        return trk.evaluateColor(set, null, heatMs);
                    }
                default:
                    return set.getColor32((ushort)colorIndex);
            }
        }
	};
}