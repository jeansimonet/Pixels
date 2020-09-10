#include "animation_gradientpattern.h"
#include "keyframes.h"
#include "data_set/data_set.h"
#include "data_set/data_animation_bits.h"
#include "assert.h"
#include "../utils/utils.h"
#include "nrf_log.h"

// FIXME!!!
#include "modules/anim_controller.h"
#include "utils/rainbow.h"
#include "config/board_config.h"

namespace Animations
{
	/// <summary>
	/// constructor for keyframe-based animation instances
	/// Needs to have an associated preset passed in
	/// </summary>
	AnimationInstanceGradientPattern::AnimationInstanceGradientPattern(const AnimationGradientPattern* preset, const DataSet::AnimationBits* bits)
		: AnimationInstance(preset, bits) {
	}

	/// <summary>
	/// destructor
	/// </summary>
	AnimationInstanceGradientPattern::~AnimationInstanceGradientPattern() {
	}

	/// <summary>
	/// Small helper to return the expected size of the preset data
	/// </summary>
	int AnimationInstanceGradientPattern::animationSize() const {
		return sizeof(AnimationGradientPattern);
	}

	/// <summary>
	/// (re)Initializes the instance to animate leds. This can be called on a reused instance.
	/// </summary>
	void AnimationInstanceGradientPattern::start(int _startTime, uint8_t _remapFace, bool _loop) {
		AnimationInstance::start(_startTime, _remapFace, _loop);
	}

	/// <summary>
	/// Computes the list of LEDs that need to be on, and what their intensities should be
	/// based on the different tracks of this animation.
	/// </summary>
	/// <param name="ms">The animation time (in milliseconds)</param>
	/// <param name="retIndices">the return list of LED indices to fill, max size should be at least 21, the max number of leds</param>
	/// <param name="retColors">the return list of LED color to fill, max size should be at least 21, the max number of leds</param>
	/// <returns>The number of leds/intensities added to the return array</returns>
	int AnimationInstanceGradientPattern::updateLEDs(int ms, int retIndices[], uint32_t retColors[])
	{
        int time = ms - startTime;
        auto preset = getPreset();

        // Figure out the color from the gradient
        auto& gradient = animationBits->getRGBTrack(preset->gradientTrackOffset);

        int gradientTime = time * 1000 / preset->duration;
        uint32_t gradientColor = gradient.evaluateColor(animationBits, gradientTime);

        int trackTime = time * 256 / preset->speedMultiplier256;

        // Each track will append its led indices and colors into the return array
        // The assumption is that led indices don't overlap between tracks of a single animation,
        // so there will always be enough room in the return arrays.
        int totalCount = 0;
        int indices[20];
        uint32_t colors[20];
        for (int i = 0; i < preset->trackCount; ++i)
        {
            auto track = animationBits->getTrack((uint16_t)(preset->tracksOffset + i)); 
            int count = track.evaluate(animationBits, gradientColor, trackTime, indices, colors);
            for (int j = 0; j < count; ++j)
            {
                retIndices[totalCount+j] = indices[j];
                retColors[totalCount+j] = colors[j];
            }
            totalCount += count;
        }

        return totalCount;
	}

	/// <summary>
	/// Clear all LEDs controlled by this animation, for instance when the anim gets interrupted.
	/// </summary>
	int AnimationInstanceGradientPattern::stop(int retIndices[]) {
		auto preset = getPreset();
		// Each track will append its led indices and colors into the return array
		// The assumption is that led indices don't overlap between tracks of a single animation,
		// so there will always be enough room in the return arrays.
        int totalCount = 0;
        int indices[20];
        for (int i = 0; i < preset->trackCount; ++i)
        {
            auto track = animationBits->getTrack((uint16_t)(preset->tracksOffset + i)); 
            int count = track.extractLEDIndices(indices);
            for (int j = 0; j < count; ++j)
            {
                retIndices[totalCount+j] = indices[j];
            }
            totalCount += count;
        }
		return totalCount;
	}

	/// <summary>
	/// Small helper to get the correct type preset data pointer stored in the instance
	/// </summary
	const AnimationGradientPattern* AnimationInstanceGradientPattern::getPreset() const {
		return static_cast<const AnimationGradientPattern*>(animationPreset);
	}

	/// <summary>
	/// Returns a track
	/// </summary>
	const Track& AnimationInstanceGradientPattern::GetTrack(int index) const	{
		auto preset = getPreset();
		assert(index < preset->trackCount);
		return animationBits->getTrack(preset->tracksOffset + index);
	}

}