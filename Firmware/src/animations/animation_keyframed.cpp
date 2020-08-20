#include "animation_keyframed.h"
#include "keyframes.h"
#include "data_set/data_set.h"
#include "data_set/data_animation_bits.h"
#include "assert.h"
#include "../utils/utils.h"

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
	AnimationInstanceKeyframed::AnimationInstanceKeyframed(const AnimationKeyframed* preset, const DataSet::AnimationBits* bits)
		: AnimationInstance(preset, bits) {
	}

	/// <summary>
	/// destructor
	/// </summary>
	AnimationInstanceKeyframed::~AnimationInstanceKeyframed() {
	}

	/// <summary>
	/// Small helper to return the expected size of the preset data
	/// </summary>
	int AnimationInstanceKeyframed::animationSize() const {
		return sizeof(AnimationKeyframed);
	}

	/// <summary>
	/// (re)Initializes the instance to animate leds. This can be called on a reused instance.
	/// </summary>
	void AnimationInstanceKeyframed::start(int _startTime, uint8_t _remapFace, bool _loop) {
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
	int AnimationInstanceKeyframed::updateLEDs(int ms, int retIndices[], uint32_t retColors[])
	{
		int time = ms - startTime;
		auto preset = getPreset();
		const RGBTrack * tracks = animationBits->getRGBTracks(preset->tracksOffset);

		// Each track will append its led indices and colors into the return array
		// The assumption is that led indices don't overlap between tracks of a single animation,
		// so there will always be enough room in the return arrays.
		int* indices = retIndices;
		uint32_t* colors = retColors;
		int totalCount = 0;
		for (int i = 0; i < preset->trackCount; ++i)
		{
			auto& track = tracks[i]; 
			auto count = track.evaluate(animationBits, time, indices, colors);
			indices += count;
			colors += count;
			totalCount += count;
		}
		return totalCount;
	}

	/// <summary>
	/// Clear all LEDs controlled by this animation, for instance when the anim gets interrupted.
	/// </summary>
	int AnimationInstanceKeyframed::stop(int retIndices[]) {
		auto preset = getPreset();
		const RGBTrack * tracks = animationBits->getRGBTracks(preset->tracksOffset);
		// Each track will append its led indices and colors into the return array
		// The assumption is that led indices don't overlap between tracks of a single animation,
		// so there will always be enough room in the return arrays.
		int* indices = retIndices;
		int totalCount = 0;
		for (int i = 0; i < preset->trackCount; ++i)
		{
			auto& track = tracks[i]; 
			auto count = track.extractLEDIndices(indices);
			indices += count;
			totalCount += count;
		}
		return totalCount;
	}

	/// <summary>
	/// Small helper to get the correct type preset data pointer stored in the instance
	/// </summary
	const AnimationKeyframed* AnimationInstanceKeyframed::getPreset() const {
		return static_cast<const AnimationKeyframed*>(animationPreset);
	}

	/// <summary>
	/// Returns a track
	/// </summary>
	const RGBTrack& AnimationInstanceKeyframed::GetTrack(int index) const	{
		auto preset = getPreset();
		assert(index < preset->trackCount);
		return animationBits->getRGBTrack(preset->tracksOffset + index);
	}
}