#pragma once

#include "animations/Animation.h"

#pragma pack(push, 1)

namespace Animations
{
	/// <summary>
	/// Procedural gradient animation
	/// </summary>
	struct AnimationGradient
		: public Animation
	{
		uint32_t faceMask;
		uint16_t gradientTrackOffset;
        uint16_t gradientPadding;
	};

	/// <summary>
	/// Procedural rainbow animation instance data
	/// </summary>
	class AnimationInstanceGradient
		: public AnimationInstance
	{
	public:
		AnimationInstanceGradient(const AnimationGradient* preset, const DataSet::AnimationBits* bits);
		virtual ~AnimationInstanceGradient();
		virtual int animationSize() const;

		virtual void start(int _startTime, uint8_t _remapFace, bool _loop);
		virtual int updateLEDs(int ms, int retIndices[], uint32_t retColors[]);
		virtual int stop(int retIndices[]);

	private:
		const AnimationGradient* getPreset() const;
	};
}

#pragma pack(pop)