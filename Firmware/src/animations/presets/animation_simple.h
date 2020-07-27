#pragma once

#include "animations/Animation.h"

namespace Animations
{
    enum AnimationSimpleLEDType : uint8_t
    {
        AnimationSimple_OneLED,
        AnimationSimple_AllLEDs,
    };

	/// <summary>
	/// Procedural on off animation
	/// </summary>
	struct AnimationSimple
		: public Animation
	{
		AnimationSimpleLEDType ledType;
        uint16_t padding_ledType;
        uint32_t color;
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
