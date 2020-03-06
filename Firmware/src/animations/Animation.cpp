#include "animation.h"
#include "animation_set.h"

#include "assert.h"
#include "../utils/utils.h"

#define MAX_LEVEL (256)

using namespace Utils;

namespace Animations
{
	const char* getEventName(AnimationEvent event) {
		switch (event) {
			case AnimationEvent_None:
				return "None";
			case AnimationEvent_Hello:
				return "Hello";
			case AnimationEvent_Connected:
				return "Connected";
			case AnimationEvent_Disconnected:
				return "Disconnected";
			case AnimationEvent_LowBattery:
				return "LowBattery";
			case AnimationEvent_ChargingStart:
				return "ChargingStart";
			case AnimationEvent_ChargingDone:
				return "ChargingDone";
			case AnimationEvent_ChargingError:
				return "ChargingError";
			case AnimationEvent_Handling:
				return "Handling";
			case AnimationEvent_Rolling:
				return "Rolling";
			case AnimationEvent_OnFace_Default:
				return "OnFace";
			case AnimationEvent_Crooked:
				return "Crooked";
			case AnimationEvent_Battle_ShowTeam:
				return "Battle_ShowTeam";
			case AnimationEvent_Battle_FaceUp:
				return "Battle_FaceUp";
			case AnimationEvent_Battle_WaitingForBattle:
				return "Battle_WaitingForBattle";
			case AnimationEvent_Battle_Duel:
				return "Battle_Duel";
			case AnimationEvent_Battle_DuelWin:
				return "Battle_DuelWin";
			case AnimationEvent_Battle_DuelLose:
				return "Battle_DuelLose";
			case AnimationEvent_Battle_DuelDraw:
				return "Battle_DuelDraw";
			case AnimationEvent_Battle_TeamWin:
				return "Battle_TeamWin";
			case AnimationEvent_Battle_TeamLoose:
				return "Battle_TeamLoose";
			case AnimationEvent_Battle_TeamDraw:
				return "Battle_TeamDraw";
			default:
				return "Unknown";
		}
	}

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
	
	uint32_t RGBKeyframe::color(void* token) const {
		// Unpack
		uint16_t index = timeAndColor & 0b01111111;
		return AnimationSet::getColor(token, index);
	}

	void RGBKeyframe::setTimeAndColorIndex(uint16_t timeInMS, uint16_t colorIndex) {
		timeAndColor = (((timeInMS / 20) & 0b111111111) << 7) |
					   (colorIndex & 0b1111111);
	}

	uint16_t RGBTrack::getDuration() const {
		return AnimationSet::getKeyframe(keyframesOffset + keyFrameCount - 1).time();
	}

	const RGBKeyframe& RGBTrack::getKeyframe(uint16_t keyframeIndex) const {
		assert(keyframeIndex < keyFrameCount);
		return AnimationSet::getKeyframe(keyframesOffset + keyframeIndex);
	}

	/// <summary>
	/// Evaluate an animation track's for a given time, in milliseconds.
	/// Values outside the track's range are clamped to first or last keyframe value.
	/// </summary>
	uint32_t RGBTrack::evaluate(void* token, int time) const {
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
			// To stick to integer math, we'll scale the values
			int scaler = 1024;
			int scaledPercent = (time - prevKeyframeTime) * scaler / (nextKeyframeTime - prevKeyframeTime);
			int scaledRed = getRed(prevKeyframeColor)* (scaler - scaledPercent) + getRed(nextKeyframeColor) * scaledPercent;
			int scaledGreen = getGreen(prevKeyframeColor) * (scaler - scaledPercent) + getGreen(nextKeyframeColor) * scaledPercent;
			int scaledBlue = getBlue(prevKeyframeColor) * (scaler - scaledPercent) + getBlue(nextKeyframeColor) * scaledPercent;
			return toColor(scaledRed / scaler, scaledGreen / scaler, scaledBlue / scaler);
		}
	}

	const RGBTrack& AnimationTrack::getTrack() const {
		return AnimationSet::getRGBTrack(trackOffset);
	}

	uint32_t AnimationTrack::evaluate(void* token, int time) const {
		return getTrack().evaluate(token, time);
	}

	/// <summary>
	/// Computes the list of LEDs that need to be on, and what their intensities should be
	/// based on the different tracks of this animation.
	/// </summary>
	/// <param name="time">The animation time (in milliseconds)</param>
	/// <param name="retIndices">the return list of LED indices to fill, max size should be at least 21, the total number of leds</param>
	/// <param name="retColors">the return list of LED color to fill, max size should be at least 21, the total number of leds</param>
	/// <returns>The number of leds/intensities added to the return array</returns>
	int Animation::updateLEDs(void* token, int time, int retIndices[], uint32_t retColors[]) const
	{
		AnimationTrack const * const tracks = AnimationSet::getTracks(tracksOffset);
		for (uint16_t i = 0; i < trackCount; ++i)
		{
			const RGBTrack& rgbTrack = tracks[i].getTrack();
			retIndices[i] = tracks[i].ledIndex;
			retColors[i] = rgbTrack.evaluate(token, time);
		}
		return trackCount;
	}

	/// <summary>
	/// Clear all LEDs controlled by this animation, for instance when
	/// the anim gets interrupted.
	/// </summary>
	int Animation::stop(int retIndices[]) const
	{
		AnimationTrack const * const tracks = AnimationSet::getTracks(tracksOffset);
		for (uint16_t i = 0; i < trackCount; ++i)
		{
			retIndices[i] = tracks[i].ledIndex;
		}
		return trackCount;
	}

	/// <summary>
	/// Returns a track
	/// </summary>
	const AnimationTrack& Animation::GetTrack(int index) const
	{
		assert(index < trackCount);
		return AnimationSet::getTrack(tracksOffset + index);
	}
}
