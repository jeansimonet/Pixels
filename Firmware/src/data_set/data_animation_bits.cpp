#include "data_animation_bits.h"
#include "assert.h"
#include "nrf_log.h"
#include "utils/utils.h"

namespace DataSet
{
	uint32_t AnimationBits::getPaletteColor(uint16_t colorIndex) const {
		if (colorIndex < (paletteSize / 3)) {
			return Utils::toColor(
					palette[colorIndex * 3 + 0],
					palette[colorIndex * 3 + 1],
					palette[colorIndex * 3 + 2]);
		} else {
			return 0xFFFFFFFF;
		}
	}

	uint16_t AnimationBits::getPaletteSize() const {
		return paletteSize;
	}

	const RGBKeyframe& AnimationBits::getRGBKeyframe(uint16_t keyFrameIndex) const {
		assert(keyFrameIndex < rgbKeyFrameCount);
		return rgbKeyframes[keyFrameIndex];
	}

	uint16_t AnimationBits::getRGBKeyframeCount() const {
		return rgbKeyFrameCount;
	}

	const RGBTrack& AnimationBits::getRGBTrack(uint16_t trackIndex) const {
		assert(trackIndex < rgbTrackCount);
		return rgbTracks[trackIndex];
	}

	RGBTrack const * const AnimationBits::getRGBTracks(uint16_t tracksStartIndex)const  {
		assert(tracksStartIndex < rgbTrackCount);
		return &(rgbTracks[tracksStartIndex]);
	}

	uint16_t AnimationBits::getRGBTrackCount() const {
		return rgbTrackCount;
	}

	const Keyframe& AnimationBits::getKeyframe(uint16_t keyFrameIndex) const {
		assert(keyFrameIndex < keyFrameCount);
		return keyframes[keyFrameIndex];
	}

	uint16_t AnimationBits::getKeyframeCount() const {
		return keyFrameCount;
	}

	const Track& AnimationBits::getTrack(uint16_t trackIndex) const {
		assert(trackIndex < trackCount);
		return tracks[trackIndex];
	}

	Track const * const AnimationBits::getTracks(uint16_t tracksStartIndex) const {
		assert(tracksStartIndex < trackCount);
		return &(tracks[tracksStartIndex]);
	}

	uint16_t AnimationBits::getTrackCount() const {
		return trackCount;
	}

}