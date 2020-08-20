#pragma once

#include "animations/Animation.h"

#pragma pack(push, 1)

namespace Animations
{
	/// <summary>
	/// Procedural rainbow animation data
	/// </summary>
	struct AnimationRainbow
		: public Animation
	{
		uint32_t faceMask;
        uint8_t count;
        uint8_t fade;
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

	private:
		const AnimationRainbow* getPreset() const;
	};
}

#pragma pack(pop)