#pragma once

#include <stdint.h>

#pragma pack(push, 1)

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
	struct RGBKeyframe
	{
	public:
		uint16_t timeAndColor;

		uint16_t time() const; // unpack the time in ms
		uint32_t color() const;// unpack the color using the lookup table from the animation set

		void setTimeAndColorIndex(uint16_t timeInMS, uint16_t colorIndex);
	};

	/// <summary>
	/// An animation track is essentially an animation curve for a specific LED.
	/// size: 4 bytes (+ the actual keyframe data)
	/// </summary>
	struct AnimationTrack
	{
	public:
		uint16_t keyframesOffset; // offset into a global keyframe buffer
		uint8_t ledIndex;	// 0 - 20
		uint8_t keyFrameCount;		// Keyframe count

		RGBKeyframe getKeyframe(uint16_t keyframeIndex) const;
		uint32_t evaluate(int time) const;
	};

	/// <summary>
	/// A keyframe-based animation
	/// size: 8 bytes (+ actual track and keyframe data)
	/// </summary>
	class Animation
	{
	public:
		uint16_t duration; // in ms
		uint16_t tracksOffset; // offset into a global buffer of tracks
		uint16_t trackCount;
		uint16_t padding;

	public:
		AnimationTrack GetTrack(int index) const;
		int updateLEDs(int time, int retIndices[], uint32_t retColors[]) const;
		int stop(int retIndices[]) const;
	};

}

#pragma pack(pop)
