#include "animation_rainbow.h"
#include "utils/rainbow.h"

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
	}

	/// <summary>
	/// Computes the list of LEDs that need to be on, and what their intensities should be.
	/// </summary>
	/// <param name="ms">The animation time (in milliseconds)</param>
	/// <param name="retIndices">the return list of LED indices to fill, max size should be at least 21, the max number of leds</param>
	/// <param name="retColors">the return list of LED color to fill, max size should be at least 21, the max number of leds</param>
	/// <returns>The number of leds/intensities added to the return array</returns>
	int AnimationInstanceRainbow::updateLEDs(int ms, int retIndices[], uint32_t retColors[]) {
		auto preset = getPreset();

		// Compute color
		uint32_t color = 0;
		int fadeTime = preset->duration * preset->fade / (255 * 2);
		int time = (ms - startTime);

		int wheelPos = (time * preset->count * 255 / preset->duration) % 256;

		uint8_t intensity = 255;
		if (time <= fadeTime) {
			// Ramp up
			intensity = (uint8_t)(time * 255 / fadeTime);
		} else if (time >= (preset->duration - fadeTime)) {
			// Ramp down
			intensity = (uint8_t)((preset->duration - time) * 255 / fadeTime);
		}

		color = Rainbow::wheel((uint8_t)wheelPos, intensity);

		// Fill the indices and colors for the anim controller to know how to update leds
		int retCount = 0;
		for (int i = 0; i < 20; ++i) {
			if ((preset->faceMask & (1 << i)) != 0)
			{
				retIndices[retCount] = i;
				retColors[retCount] = color;
				retCount++;
			}
		}
		return retCount;
	}

	/// <summary>
	/// Clear all LEDs controlled by this animation, for instance when the anim gets interrupted.
	/// </summary>
	int AnimationInstanceRainbow::stop(int retIndices[]) {
		auto preset = getPreset();
            int retCount = 0;
            for (int i = 0; i < 20; ++i) {
                if ((preset->faceMask & (1 << i)) != 0)
                {
                    retIndices[retCount] = i;
                    retCount++;
                }
            }
            return retCount;
	}

	const AnimationRainbow* AnimationInstanceRainbow::getPreset() const {
        return static_cast<const AnimationRainbow*>(animationPreset);
    }
}
