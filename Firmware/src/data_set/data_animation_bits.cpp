#include "data_animation_bits.h"
#include "assert.h"
#include "nrf_log.h"
#include "utils/utils.h"
#include "utils/rainbow.h"
#include "modules/accelerometer.h"
#include "config/board_config.h"

using namespace Modules;
using namespace Config;

namespace DataSet
{
	uint32_t AnimationBits::getPaletteColor(uint16_t colorIndex) const {
		if (colorIndex == PALETTE_COLOR_FROM_FACE) {
			// Color is based on the face
			return Rainbow::faceWheel(Accelerometer::currentFace(), BoardManager::getBoard()->ledCount);
		}
		else if (colorIndex == PALETTE_COLOR_FROM_RANDOM) {
			// Not implemented
			return 0xFFFFFFFF;
		}
		else if (colorIndex < (paletteSize / 3)) {
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

	void AnimationBits::Clear() {
        palette = nullptr;
        paletteSize = 0;
        rgbKeyframes = nullptr;
        rgbKeyFrameCount = 0;
        rgbTracks = nullptr;
        rgbTrackCount = 0;
        keyframes = nullptr;
        keyFrameCount = 0;
        tracks = nullptr;
        trackCount = 0;
	}

}