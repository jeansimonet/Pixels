#include "animation_gradient.h"
#include "data_set/data_set.h"
#include "data_set/data_animation_bits.h"

namespace Animations
{
	/// <summary>
	/// constructor for rainbow animations
	/// Needs to have an associated preset passed in
	/// </summary>
	AnimationInstanceGradient::AnimationInstanceGradient(const AnimationGradient* preset, const DataSet::AnimationBits* bits)
		: AnimationInstance(preset, bits) {
	}

	/// <summary>
	/// destructor
	/// </summary>
	AnimationInstanceGradient::~AnimationInstanceGradient() {
	}

	/// <summary>
	/// Small helper to return the expected size of the preset data
	/// </summary>
	int AnimationInstanceGradient::animationSize() const {
		return sizeof(AnimationGradient);
	}

	/// <summary>
	/// (re)Initializes the instance to animate leds. This can be called on a reused instance.
	/// </summary>
	void AnimationInstanceGradient::start(int _startTime, uint8_t _remapFace, bool _loop) {
		AnimationInstance::start(_startTime, _remapFace, _loop);
	}

	/// <summary>
	/// Computes the list of LEDs that need to be on, and what their intensities should be.
	/// </summary>
	/// <param name="ms">The animation time (in milliseconds)</param>
	/// <param name="retIndices">the return list of LED indices to fill, max size should be at least 21, the max number of leds</param>
	/// <param name="retColors">the return list of LED color to fill, max size should be at least 21, the max number of leds</param>
	/// <returns>The number of leds/intensities added to the return array</returns>
	int AnimationInstanceGradient::updateLEDs(int ms, int retIndices[], uint32_t retColors[]) {
        int time = ms - startTime;
        auto preset = getPreset();

        // Figure out the color from the gradient
        auto& gradient = animationBits->getRGBTrack(preset->gradientTrackOffset);

        int gradientTime = time * 1000 / preset->duration;
        uint32_t color = gradient.evaluateColor(animationBits, gradientTime);

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
	int AnimationInstanceGradient::stop(int retIndices[]) {
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

	const AnimationGradient* AnimationInstanceGradient::getPreset() const {
        return static_cast<const AnimationGradient*>(animationPreset);
    }
}
