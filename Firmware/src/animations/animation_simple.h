#pragma once

#include "animations/Animation.h"

#pragma pack(push, 1)

namespace Animations
{
	/// <summary>
	/// Procedural on off animation
	/// </summary>
	struct AnimationSimple
		: public Animation
	{
		uint32_t faceMask;
        uint32_t color;
        uint8_t count;
        uint8_t fade;
	};

	/// <summary>
	/// Procedural on off animation instance data
	/// </summary>
	class AnimationInstanceSimple
		: public AnimationInstance
	{
	public:
		AnimationInstanceSimple(const AnimationSimple* preset);
		virtual ~AnimationInstanceSimple();
		virtual int animationSize() const;

		virtual void start(int _startTime, uint8_t _remapFace, bool _loop);
		virtual int updateLEDs(int ms, int retIndices[], uint32_t retColors[]);
		virtual int stop(int retIndices[]);

	private:
		const AnimationSimple* getPreset() const;
	};
}

#pragma pack(pop)