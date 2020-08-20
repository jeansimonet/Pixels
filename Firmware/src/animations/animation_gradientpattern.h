#pragma once

#include "animations/Animation.h"

#pragma pack(push, 1)

namespace Animations
{
    class Keyframe;

	/// <summary>
	/// An animation track is essentially an animation curve and a set of leds.
	/// size: 8 bytes (+ the actual keyframe data).
	/// </summary>
	struct Track
	{
	public:
		uint16_t keyframesOffset;	// offset into a global keyframe buffer
		uint8_t keyFrameCount;		// Keyframe count
		uint8_t padding;
		uint32_t ledMask; 			// indicates which leds to drive

		uint16_t getDuration() const;
		const Keyframe& getKeyframe(uint16_t keyframeIndex) const;
		int evaluate(uint32_t color, int time, int retIndices[], uint32_t retColors[]) const;
        uint32_t modulateColor(uint32_t color, int time) const;
		int extractLEDIndices(int retIndices[]) const;
	};

	/// <summary>
	/// A keyframe-based animation with a gradient applied over
	/// size: 8 bytes (+ actual track and keyframe data)
	/// </summary>
	struct AnimationGradientPattern
		: public Animation
	{
		uint8_t specialColorType; // is really SpecialColor
        uint8_t padding_specialColor;
		uint16_t tracksOffset; // offset into a global buffer of tracks
		uint16_t trackCount;
		uint16_t gradientTrackOffset;
	};

	/// <summary>
	/// Keyframe-based animation instance data
	/// </summary>
	class AnimationInstanceGradientPattern
		: public IAnimationSpecialColorToken
		, public AnimationInstance
	{
	private:
		uint32_t specialColorPayload; // meaning varies

	public:
		AnimationInstanceGradientPattern(const AnimationGradientPattern* preset);
		virtual ~AnimationInstanceGradientPattern();

	public:
		virtual int animationSize() const;

		virtual void start(int _startTime, uint8_t _remapFace, bool _loop);
		virtual int updateLEDs(int ms, int retIndices[], uint32_t retColors[]);
		virtual int stop(int retIndices[]);

	private:
		const AnimationGradientPattern* getPreset() const;
		const Track& GetTrack(int index) const;

	public:
		virtual uint32_t getColor(uint32_t colorIndex) const;
	};

}

#pragma pack(pop)