#pragma once

#include "animations/Animation.h"

namespace Animations
{
	/// <summary>
	/// Procedural rainbow animation data
	/// </summary>
	struct AnimationRainbow
		: public Animation
	{
        uint8_t padding1;
        uint8_t padding2;
        uint8_t padding3;
		// TBD
	};

	/// <summary>
	/// Procedural rainbow animation instance data
	/// </summary>
	class AnimationInstanceRainbow
		: public AnimationInstance
	{
	public:
		AnimationInstanceRainbow(const AnimationRainbow* preset);
		virtual ~AnimationInstanceRainbow();
		virtual int animationSize() const;

		virtual void start(int _startTime, uint8_t _remapFace, bool _loop);
		virtual int updateLEDs(int ms, int retIndices[], uint32_t retColors[]);
		virtual int stop(int retIndices[]);
	};
}
