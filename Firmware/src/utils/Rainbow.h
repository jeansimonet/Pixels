#pragma once

#include <stdint.h>

namespace Rainbow
{
	// Input a value 0 to 255 to get a color value.
	// The colours are a transition r - g - b - back to r.
	uint32_t wheel(uint8_t WheelPos, uint8_t intensity = 255);

	uint32_t faceWheel(uint8_t face, uint8_t count);
}

