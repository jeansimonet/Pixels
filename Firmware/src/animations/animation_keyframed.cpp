#include "animation_keyframed.h"
#include "data_set/data_set.h"
#include "assert.h"
#include "../utils/utils.h"

// FIXME!!!
#include "modules/anim_controller.h"
#include "utils/rainbow.h"
#include "config/board_config.h"

namespace Animations
{
	uint16_t RGBKeyframe::time() const {
		// Unpack
		uint16_t time50th = (timeAndColor & 0b1111111110000000) >> 7;
		return time50th * 20;
	}
	
	uint32_t RGBKeyframe::color(const IAnimationSpecialColorToken* token) const {
		// Unpack
		uint16_t index = timeAndColor & 0b01111111;
		return token->getColor(index);
	}

	void RGBKeyframe::setTimeAndColorIndex(uint16_t timeInMS, uint16_t colorIndex) {
		timeAndColor = (((timeInMS / 20) & 0b111111111) << 7) |
					   (colorIndex & 0b1111111);
	}

	uint16_t RGBTrack::getDuration() const {
		return DataSet::getKeyframe(keyframesOffset + keyFrameCount - 1).time();
	}

	const RGBKeyframe& RGBTrack::getKeyframe(uint16_t keyframeIndex) const {
		assert(keyframeIndex < keyFrameCount);
		return DataSet::getKeyframe(keyframesOffset + keyframeIndex);
	}

	/// <summary>
	/// Evaluate an animation track's for a given time, in milliseconds, and fills returns arrays of led indices and colors
	/// Values outside the track's range are clamped to first or last keyframe value.
	/// </summary>
	int RGBTrack::evaluate(const IAnimationSpecialColorToken* token, int time, int retIndices[], uint32_t retColors[]) const {
		if (keyFrameCount == 0)
			return 0;

		uint32_t color = evaluateColor(token, time);

		// Fill the return arrays
		int currentCount = 0;
		for (int i = 0; i < Config::BoardManager::getBoard()->ledCount; ++i) {
			if (ledMask & (1 << i)) {
				retIndices[currentCount] = i;
				retColors[currentCount] = color;
				currentCount++;
			}
		}
		return currentCount;
	}

	/// <summary>
	/// Evaluate an animation track's for a given time, in milliseconds
	/// Values outside the track's range are clamped to first or last keyframe value.
	/// </summary>
	uint32_t RGBTrack::evaluateColor(const IAnimationSpecialColorToken* token, int time) const
	{
		// Find the first keyframe
		int nextIndex = 0;
		while (nextIndex < keyFrameCount && getKeyframe(nextIndex).time() < time) {
			nextIndex++;
		}

		uint32_t color = 0;
		if (nextIndex == 0) {
			// The first keyframe is already after the requested time, clamp to first value
			color = getKeyframe(nextIndex).color(token);
		} else if (nextIndex == keyFrameCount) {
			// The last keyframe is still before the requested time, clamp to the last value
			color = getKeyframe(nextIndex- 1).color(token);
		} else {
			// Grab the prev and next keyframes
			auto nextKeyframe = getKeyframe(nextIndex);
			uint16_t nextKeyframeTime = nextKeyframe.time();
			uint32_t nextKeyframeColor = nextKeyframe.color(token);

			auto prevKeyframe = getKeyframe(nextIndex - 1);
			uint16_t prevKeyframeTime = prevKeyframe.time();
			uint32_t prevKeyframeColor = prevKeyframe.color(token);

			// Compute the interpolation parameter
			color = Utils::interpolateColors(prevKeyframeColor, prevKeyframeTime, nextKeyframeColor, nextKeyframeTime, time);
		}

		return color;
	}

	/// <summary>
	/// Extracts the LED indices from the led bit mask
	/// </sumary>
	int RGBTrack::extractLEDIndices(int retIndices[]) const {
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
	AnimationInstanceKeyframed::AnimationInstanceKeyframed(const AnimationKeyframed* preset)
		: AnimationInstance(preset) {
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
	int AnimationInstanceKeyframed::updateLEDs(int ms, int retIndices[], uint32_t retColors[])
	{
		int time = ms - startTime;
		auto preset = getPreset();
		const RGBTrack * tracks = DataSet::getRGBTracks(preset->tracksOffset);

		// Each track will append its led indices and colors into the return array
		// The assumption is that led indices don't overlap between tracks of a single animation,
		// so there will always be enough room in the return arrays.
		int* indices = retIndices;
		uint32_t* colors = retColors;
		int totalCount = 0;
		for (int i = 0; i < preset->trackCount; ++i)
		{
			auto& track = tracks[i]; 
			auto count = track.evaluate(this, time, indices, colors);
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
		const RGBTrack * tracks = DataSet::getRGBTracks(preset->tracksOffset);
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
		return DataSet::getRGBTrack(preset->tracksOffset + index);
	}

	/// <summary>
	/// returns a color RGB value for a given color index, taking into account special color indices
	/// </summary>
	uint32_t AnimationInstanceKeyframed::getColor(uint32_t colorIndex) const {
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