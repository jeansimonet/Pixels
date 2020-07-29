#pragma once

#include "animations/Animation.h"

namespace Animations
{
	enum SpecialColor
	{
		SpecialColor_None = 0,
		SpecialColor_Face,
		SpecialColor_ColorWheel,
		SpecialColor_Heat_Current,
		SpecialColor_Heat_Start,
	};

	class IAnimationSpecialColorToken;

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
		uint32_t color(const IAnimationSpecialColorToken* token) const;// unpack the color using the lookup table from the animation set

		void setTimeAndColorIndex(uint16_t timeInMS, uint16_t colorIndex);
	};

	/// <summary>
	/// An animation track is essentially an animation curve and a set of leds.
	/// size: 8 bytes (+ the actual keyframe data).
	/// </summary>
	struct RGBTrack
	{
	public:
		uint16_t keyframesOffset;	// offset into a global keyframe buffer
		uint8_t keyFrameCount;		// Keyframe count
		uint8_t padding;
		uint32_t ledMask; 			// indicates which leds to drive

		uint16_t getDuration() const;
		const RGBKeyframe& getKeyframe(uint16_t keyframeIndex) const;
		int evaluate(const IAnimationSpecialColorToken* token, int time, int retIndices[], uint32_t retColors[]) const;
		uint32_t evaluateColor(const IAnimationSpecialColorToken* token, int time) const;
		int extractLEDIndices(int retIndices[]) const;
	};

	/// <summary>
	/// A keyframe-based animation
	/// size: 8 bytes (+ actual track and keyframe data)
	/// </summary>
	struct AnimationKeyframed
		: public Animation
	{
		uint8_t specialColorType; // is really SpecialColor
		uint16_t tracksOffset; // offset into a global buffer of tracks
		uint16_t trackCount;
		uint8_t padding2;
		uint8_t padding3;
	};

	/// <summary>
	/// Keyframe-based animation instance data
	/// </summary>
	class AnimationInstanceKeyframed
		: public IAnimationSpecialColorToken
		, public AnimationInstance
	{
	private:
		uint32_t specialColorPayload; // meaning varies

	public:
		AnimationInstanceKeyframed(const AnimationKeyframed* preset);
		virtual ~AnimationInstanceKeyframed();

	public:
		virtual int animationSize() const;

		virtual void start(int _startTime, uint8_t _remapFace, bool _loop);
		virtual int updateLEDs(int ms, int retIndices[], uint32_t retColors[]);
		virtual int stop(int retIndices[]);

	private:
		const AnimationKeyframed* getPreset() const;
		const RGBTrack& GetTrack(int index) const;

	public:
		virtual uint32_t getColor(uint32_t colorIndex) const;
	};

}
