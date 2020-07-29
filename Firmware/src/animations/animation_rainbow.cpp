#include "animation_rainbow.h"

namespace Animations
{
	/// <summary>
	/// constructor for rainbow animations
	/// Needs to have an associated preset passed in
	/// </summary>
	AnimationInstanceRainbow::AnimationInstanceRainbow(const AnimationRainbow* preset)
		: AnimationInstance(preset) {
	}

	/// <summary>
	/// destructor
	/// </summary>
	AnimationInstanceRainbow::~AnimationInstanceRainbow() {
	}

	/// <summary>
	/// Small helper to return the expected size of the preset data
	/// </summary>
	int AnimationInstanceRainbow::animationSize() const {
		return sizeof(AnimationRainbow);
	}

	/// <summary>
	/// (re)Initializes the instance to animate leds. This can be called on a reused instance.
	/// </summary>
	void AnimationInstanceRainbow::start(int _startTime, uint8_t _remapFace, bool _loop) {
		AnimationInstance::start(_startTime, _remapFace, _loop);

		// Placeholder
	}

	/// <summary>
	/// Computes the list of LEDs that need to be on, and what their intensities should be.
	/// </summary>
	/// <param name="ms">The animation time (in milliseconds)</param>
	/// <param name="retIndices">the return list of LED indices to fill, max size should be at least 21, the max number of leds</param>
	/// <param name="retColors">the return list of LED color to fill, max size should be at least 21, the max number of leds</param>
	/// <returns>The number of leds/intensities added to the return array</returns>
	int AnimationInstanceRainbow::updateLEDs(int ms, int retIndices[], uint32_t retColors[]) {
		// Placeholder
		return 0;
	}

	/// <summary>
	/// Clear all LEDs controlled by this animation, for instance when the anim gets interrupted.
	/// </summary>
	int AnimationInstanceRainbow::stop(int retIndices[]) {
		// Placeholder
		return 0;
	}
}
