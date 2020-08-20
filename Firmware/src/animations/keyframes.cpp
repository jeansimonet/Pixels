#include "keyframes.h"
#include "assert.h"
#include "../utils/utils.h"
#include "config/board_config.h"
#include "data_set/data_animation_bits.h"

using namespace Config;

namespace Animations
{

	uint16_t RGBKeyframe::time() const {
		// Unpack
		uint16_t time50th = (timeAndColor & 0b1111111110000000) >> 7;
		return time50th * 20;
	}
	
	uint32_t RGBKeyframe::color(const DataSet::AnimationBits* bits) const {
		// Unpack
		uint16_t index = timeAndColor & 0b01111111;
		return bits->getPaletteColor(index);
	}

	void RGBKeyframe::setTimeAndColorIndex(uint16_t timeInMS, uint16_t colorIndex) {
		timeAndColor = (((timeInMS / 20) & 0b111111111) << 7) | (colorIndex & 0b1111111);
	}


	/// <summary>
	/// Returns the length of the track, based on the keyframes it is storing
	/// </sumary>
	uint16_t RGBTrack::getDuration(const DataSet::AnimationBits* bits) const {
		return bits->getRGBKeyframe(keyframesOffset + keyFrameCount - 1).time();
	}

	/// <summary>
	/// Grab a keyframe from the track
	/// </sumary>
	const RGBKeyframe& RGBTrack::getRGBKeyframe(const DataSet::AnimationBits* bits, uint16_t keyframeIndex) const {
		assert(keyframeIndex < keyFrameCount);
		return bits->getRGBKeyframe(keyframesOffset + keyframeIndex);
	}

	/// <summary>
	/// Evaluate an animation track's for a given time, in milliseconds, and fills returns arrays of led indices and colors
	/// Values outside the track's range are clamped to first or last keyframe value.
	/// </summary>
	int RGBTrack::evaluate(const DataSet::AnimationBits* bits, int time, int retIndices[], uint32_t retColors[]) const {
		if (keyFrameCount == 0)
			return 0;

		uint32_t color = evaluateColor(bits, time);

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
	uint32_t RGBTrack::evaluateColor(const DataSet::AnimationBits* bits, int time) const
	{
		// Find the first keyframe
		int nextIndex = 0;
		while (nextIndex < keyFrameCount && getRGBKeyframe(bits, nextIndex).time() < time) {
			nextIndex++;
		}

		uint32_t color = 0;
		if (nextIndex == 0) {
			// The first keyframe is already after the requested time, clamp to first value
			color = getRGBKeyframe(bits, nextIndex).color(bits);
		} else if (nextIndex == keyFrameCount) {
			// The last keyframe is still before the requested time, clamp to the last value
			color = getRGBKeyframe(bits, nextIndex- 1).color(bits);
		} else {
			// Grab the prev and next keyframes
			auto nextKeyframe = getRGBKeyframe(bits, nextIndex);
			uint16_t nextKeyframeTime = nextKeyframe.time();
			uint32_t nextKeyframeColor = nextKeyframe.color(bits);

			auto prevKeyframe = getRGBKeyframe(bits, nextIndex - 1);
			uint16_t prevKeyframeTime = prevKeyframe.time();
			uint32_t prevKeyframeColor = prevKeyframe.color(bits);

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



	uint16_t Keyframe::time() const {
		// Unpack
		uint16_t time50th = (timeAndIntensity & 0b1111111110000000) >> 7;
		return time50th * 20;
	}
	
	uint8_t Keyframe::intensity() const {
		// Unpack
		return (uint8_t)((timeAndIntensity & 0b01111111) * 2); // Scale it to 0 -> 255
	}

    void Keyframe::setTimeAndIntensity(uint16_t timeInMS, uint8_t intensity) {
        timeAndIntensity = (uint16_t)(((((uint32_t)timeInMS / 20) & 0b111111111) << 7) | ((uint32_t)(intensity / 2) & 0b1111111));
    }

	/// <summary>
	/// Returns the length of the track, based on the keyframes it is storing
	/// </sumary>
	uint16_t Track::getDuration(const DataSet::AnimationBits* bits) const {
		return bits->getKeyframe(keyframesOffset + keyFrameCount - 1).time();
	}

	/// <summary>
	/// Grab a keyframe from the track
	/// </sumary>
	const Keyframe& Track::getKeyframe(const DataSet::AnimationBits* bits, uint16_t keyframeIndex) const {
		assert(keyframeIndex < keyFrameCount);
		return bits->getKeyframe(keyframesOffset + keyframeIndex);
	}

	/// <summary>
	/// Evaluate an animation track's for a given time, in milliseconds, and fills returns arrays of led indices and colors
	/// Values outside the track's range are clamped to first or last keyframe value.
	/// </summary>
	int Track::evaluate(const DataSet::AnimationBits* bits, uint32_t color, int time, int retIndices[], uint32_t retColors[]) const {
		if (keyFrameCount == 0)
			return 0;

		uint32_t mcolor = modulateColor(bits, color, time);

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
	uint32_t Track::modulateColor(const DataSet::AnimationBits* bits, uint32_t color, int time) const
	{
        // Find the first keyframe
        int nextIndex = 0;
        while (nextIndex < keyFrameCount && getKeyframe(bits, (uint16_t)nextIndex).time() < time) {
            nextIndex++;
        }

        uint8_t intensity = 0;
        if (nextIndex == 0) {
            // The first keyframe is already after the requested time, clamp to first value
            intensity = getKeyframe(bits, (uint16_t)nextIndex).intensity();
        } else if (nextIndex == keyFrameCount) {
            // The last keyframe is still before the requested time, clamp to the last value
            intensity = getKeyframe(bits, (uint16_t)(nextIndex- 1)).intensity();
        } else {
            // Grab the prev and next keyframes
            auto& nextKeyframe = getKeyframe(bits, (uint16_t)nextIndex);
            uint16_t nextKeyframeTime = nextKeyframe.time();
            uint8_t nextKeyframeIntensity = nextKeyframe.intensity();

            auto& prevKeyframe = getKeyframe(bits, (uint16_t)(nextIndex - 1));
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



}
