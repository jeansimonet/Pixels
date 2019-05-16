// Rainbow.h

#ifndef _RAINBOW_h
#define _RAINBOW_h

#include <stdint.h>

namespace Rainbow
{
	// Input a value 0 to 255 to get a color value.
	// The colours are a transition r - g - b - back to r.
	uint32_t Wheel(uint8_t WheelPos, uint8_t intensity = 255);
	void rainbowCycle(uint8_t wait, uint8_t intensity = 255);
	void rainbowAll(int repeat, uint8_t wait, uint8_t intensity = 255);
}

#endif

