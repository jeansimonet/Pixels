#include "animation_gradientpattern.h"
#include "keyframes.h"
#include "data_set/data_set.h"
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
	/// Returns the length of the track, based on the keyframes it is storing
	/// </sumary>
	uint16_t Track::getDuration() const {
		return DataSet::getKeyframe(keyframesOffset + keyFrameCount - 1).time();
	}

	/// <summary>
	/// Grab a keyframe from the track
	/// </sumary>
	const Keyframe& Track::getKeyframe(uint16_t keyframeIndex) const {
		assert(keyframeIndex < keyFrameCount);
		return DataSet::getKeyframe(keyframesOffset + keyframeIndex);
	}

	/// <summary>
	/// Evaluate an animation track's for a given time, in milliseconds, and fills returns arrays of led indices and colors
	/// Values outside the track's range are clamped to first or last keyframe value.
	/// </summary>
	int Track::evaluate(uint32_t color, int time, int retIndices[], uint32_t retColors[]) const {
		if (keyFrameCount == 0)
			return 0;

		uint32_t mcolor = modulateColor(color, time);

		// Fill the return arrays
		int currentCount = 0;
		for (int i = 0; i < Config::BoardManager::getBoard()->ledCount; ++i) {
			if (ledMask & (1 << i)) {
				retIndices[currentCount] = i;
				retColors[currentCount] = mcolor;
				currentCount++;
			}
		}
		return currentCount;
	}

	/// <summary>
	/// Evaluate an animation track's for a given time, in milliseconds
	/// Values outside the track's range are clamped to first or last keyframe value.
	/// </summary>
	uint32_t Track::modulateColor(uint32_t color, int time) const
	{
        // Find the first keyframe
        int nextIndex = 0;
        while (nextIndex < keyFrameCount && getKeyframe((uint16_t)nextIndex).time() < time) {
            nextIndex++;
        }

        uint8_t intensity = 0;
        if (nextIndex == 0) {
            // The first keyframe is already after the requested time, clamp to first value
            intensity = getKeyframe((uint16_t)nextIndex).intensity();
        } else if (nextIndex == keyFrameCount) {
            // The last keyframe is still before the requested time, clamp to the last value
            intensity = getKeyframe((uint16_t)(nextIndex- 1)).intensity();
        } else {
            // Grab the prev and next keyframes
            auto& nextKeyframe = getKeyframe((uint16_t)nextIndex);
            uint16_t nextKeyframeTime = nextKeyframe.time();
            uint8_t nextKeyframeIntensity = nextKeyframe.intensity();

            auto& prevKeyframe = getKeyframe((uint16_t)(nextIndex - 1));
            uint16_t prevKeyframeTime = prevKeyframe.time();
            uint8_t prevKeyframeIntensity = prevKeyframe.intensity();

            // Compute the interpolation parameter
            intensity = Utils::interpolateIntensity(prevKeyframeIntensity, prevKeyframeTime, nextKeyframeIntensity, nextKeyframeTime, time);
        }

        return Utils::modulateColor(color, intensity);
	}

	/// <summary>
	/// Extracts the LED indices from the led bit mask
	/// </sumary>
	int Track::extractLEDIndices(int retIndices[]) const {
		// Fill the return arrays
		int currentCount = 0;
		for (int i = 0; i < Config::BoardManager::getBoard()->ledCount; ++i) {
			if (ledMask & (1 << i)) {
				retIndices[currentCount] = i;
				currentCount++;
			}
		}
		return currentCount;
	}

	/// <summary>
	/// constructor for keyframe-based animation instances
	/// Needs to have an associated preset passed in
	/// </summary>
	AnimationInstanceGradientPattern::AnimationInstanceGradientPattern(const AnimationGradientPattern* preset)
		: AnimationInstance(preset) {
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

		switch (getPreset()->specialColorType) {
			case SpecialColor_Face:
				// Store a color based on the face
				specialColorPayload = Rainbow::faceWheel(_remapFace, Config::BoardManager::getBoard()->ledCount);
				break;
			case SpecialColor_ColorWheel:
				// Store the face index
				specialColorPayload = _remapFace;
				break;
			case SpecialColor_Heat_Start:
				{
					// Use the global heat value
					auto& trk = DataSet::getHeatTrack();
					// FIXME!!! Need a separate heat module
					int heatMs = int(Modules::AnimController::getCurrentHeat() * trk.getDuration());
					specialColorPayload = trk.evaluateColor(nullptr, heatMs);
				}
				break;
			default:
				// Other cases don't need any per-instance payload
				specialColorPayload = 0;
				break;
		}
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
        auto& gradient = DataSet::getRGBTrack(preset->gradientTrackOffset);

        int gradientTime = time * 512 / preset->duration;
        uint32_t gradientColor = gradient.evaluateColor(this, gradientTime);


        // Each track will append its led indices and colors into the return array
        // The assumption is that led indices don't overlap between tracks of a single animation,
        // so there will always be enough room in the return arrays.
        int totalCount = 0;
        int indices[20];
        uint32_t colors[20];
        for (int i = 0; i < preset->trackCount; ++i)
        {
            auto track = DataSet::getTrack((uint16_t)(preset->tracksOffset + i)); 
            int count = track.evaluate(gradientColor, time, indices, colors);
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
            auto track = DataSet::getTrack((uint16_t)(preset->tracksOffset + i)); 
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
		return DataSet::getTrack(preset->tracksOffset + index);
	}

	/// <summary>
	/// returns a color RGB value for a given color index, taking into account special color indices
	/// </summary>
	uint32_t AnimationInstanceGradientPattern::getColor(uint32_t colorIndex) const {
		auto preset = getPreset();
		switch (preset->specialColorType) {
			case SpecialColor_Face:
			case SpecialColor_Heat_Start:
				// The payload is the color
				return specialColorPayload;
			case SpecialColor_ColorWheel:
				{
					// Use the global rainbow
					int index = Modules::AnimController::getCurrentRainbowOffset();
					if (index < 0) {
						index += 256;
					}
					return Rainbow::wheel((uint8_t)index);
				}
			case SpecialColor_Heat_Current:
				{
					auto& trk = DataSet::getHeatTrack();
					int heatMs = int(Modules::AnimController::getCurrentHeat() * trk.getDuration());
					return trk.evaluateColor(nullptr, heatMs);
				}
			default:
				return DataSet::getPaletteColor(colorIndex);
		}
	}

}