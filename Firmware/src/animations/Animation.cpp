#include "animation.h"
#include "data_set/data_set.h"

#include "assert.h"
#include "../utils/utils.h"
#include "nrf_log.h"

#include "presets/animation_simple.h"
#include "presets/animation_keyframed.h"
#include "presets/animation_rainbow.h"


// Define new and delete
void* operator new(size_t size) { return malloc(size); }
void operator delete(void* ptr) { free(ptr); }
void operator delete(void* ptr, unsigned int) { free(ptr); }


#define MAX_LEVEL (256)

using namespace Utils;

namespace Animations
{
	/// Dims the passed in color by the passed in intensity (normalized 0 - 255)
	/// </summary>
	uint32_t scaleColor(uint32_t refColor, uint8_t intensity)
	{
		uint8_t r = getRed(refColor);
		uint8_t g = getGreen(refColor);
		uint8_t b = getBlue(refColor);
		return toColor(r * intensity / MAX_LEVEL, g * intensity / MAX_LEVEL, b * intensity / MAX_LEVEL);
	}

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
	/// Evaluate an animation track's for a given time, in milliseconds.
	/// Values outside the track's range are clamped to first or last keyframe value.
	/// </summary>
	uint32_t RGBTrack::evaluate(const IAnimationSpecialColorToken* token, int time) const {
		if (keyFrameCount == 0)
			return 0;

		// Find the first keyframe
		int nextIndex = 0;
		while (nextIndex < keyFrameCount && getKeyframe(nextIndex).time() < time) {
			nextIndex++;
		}

		if (nextIndex == 0) {
			// The first keyframe is already after the requested time, clamp to first value
			return getKeyframe(nextIndex).color(token);
		} else if (nextIndex == keyFrameCount) {
			// The last keyframe is still before the requested time, clamp to the last value
			return getKeyframe(nextIndex- 1).color(token);
		} else {
			// Grab the prev and next keyframes
			auto nextKeyframe = getKeyframe(nextIndex);
			uint16_t nextKeyframeTime = nextKeyframe.time();
			uint32_t nextKeyframeColor = nextKeyframe.color(token);

			auto prevKeyframe = getKeyframe(nextIndex - 1);
			uint16_t prevKeyframeTime = prevKeyframe.time();
			uint32_t prevKeyframeColor = prevKeyframe.color(token);

			// Compute the interpolation parameter
			return Utils::interpolateColors(prevKeyframeColor, prevKeyframeTime, nextKeyframeColor, nextKeyframeTime, time);
		}
	}

	const RGBTrack& LEDTrack::getLEDTrack() const {
		return DataSet::getRGBTrack(trackOffset);
	}

	uint32_t LEDTrack::evaluate(const IAnimationSpecialColorToken* token, int time) const {
		return getLEDTrack().evaluate(token, time);
	}

	AnimationInstance::AnimationInstance(const Animation* preset) 
		: animationPreset(preset) {
	}

	AnimationInstance::~AnimationInstance() {
	}

	void AnimationInstance::start(int _startTime, uint8_t _remapFace, bool _loop) {
		startTime = _startTime;
		remapFace = _remapFace;
		loop = _loop;
	}

	AnimationInstance* createAnimationInstance(int animationIndex) {
		// Grab the preset data
		const Animation* preset = DataSet::getAnimation(animationIndex);
		return createAnimationInstance(preset);
	}

	AnimationInstance* createAnimationInstance(const Animation* preset) {
		AnimationInstance* ret = nullptr;
		switch (preset->type) {
			case Animation_Simple:
				// Maybe we'll pass an allocator at some point, this is the only place I've ever used a new in the firmware...
				ret = new AnimationInstanceSimple(static_cast<const AnimationSimple*>(preset));
				break;
			case Animation_Rainbow:
				ret = new AnimationInstanceRainbow(static_cast<const AnimationRainbow*>(preset));
				break;
			case Animation_Keyframed:
				ret = new AnimationInstanceKeyframed(static_cast<const AnimationKeyframed*>(preset));
				break;
			default:
			NRF_LOG_ERROR("Unknown animation preset type");
				break;
		}
		return ret;
	}

	void destroyAnimationInstance(AnimationInstance* animationInstance) {
		// Eventually we might use an allocator
		delete animationInstance;
	}
}

