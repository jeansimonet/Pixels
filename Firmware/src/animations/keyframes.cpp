#include "keyframes.h"
#include "assert.h"
#include "../utils/utils.h"


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
		timeAndColor = (((timeInMS / 20) & 0b111111111) << 7) | (colorIndex & 0b1111111);
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
}