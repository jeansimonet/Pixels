#pragma once

#include "animations/Animation.h"

#pragma pack(push, 1)

namespace Animations
{
    struct Keyframe;
    struct Track;

	/// <summary>
	/// A keyframe-based animation with a gradient applied over
	/// size: 8 bytes (+ actual track and keyframe data)
	/// </summary>
	struct AnimationGradientPattern
		: public Animation
	{
		uint16_t speedMultiplier256; // A multiplier to the duration, scaled to 256
		uint16_t tracksOffset; // offset into a global buffer of tracks
		uint16_t trackCount;
		uint16_t gradientTrackOffset;
		uint8_t overrideWithFace;
		uint8_t overridePadding;
	};

	/// <summary>
	/// Keyframe-based animation instance data
	/// </summary>
	class AnimationInstanceGradientPattern
		: public AnimationInstance
	{
	private:
		uint32_t rgb;

	public:
		AnimationInstanceGradientPattern(const AnimationGradientPattern* preset, const DataSet::AnimationBits* bits);
		virtual ~AnimationInstanceGradientPattern();

	public:
		virtual int animationSize() const;

		virtual void start(int _startTime, uint8_t _remapFace, bool _loop);
		virtual int updateLEDs(int ms, int retIndices[], uint32_t retColors[]);
		virtual int stop(int retIndices[]);

	private:
		const AnimationGradientPattern* getPreset() const;
		const Track& GetTrack(int index) const;
	};

}

#pragma pack(pop)