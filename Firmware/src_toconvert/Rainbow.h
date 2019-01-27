// Rainbow.h

#ifndef _RAINBOW_h
#define _RAINBOW_h

#include "arduino.h"

namespace Rainbow
{
	// Input a value 0 to 255 to get a color value.
	// The colours are a transition r - g - b - back to r.
	uint32_t Wheel(byte WheelPos, byte intensity = 255);
	void rainbowCycle(uint8_t wait, byte intensity = 255);
	void rainbowAll(int repeat, uint8_t wait, byte intensity = 255);
}

#endif

