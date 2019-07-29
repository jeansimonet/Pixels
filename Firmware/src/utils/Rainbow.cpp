// 
// 
// 

#include "rainbow.h"
#include "utils.h"
#include "nrf_delay.h"

#define NUMPIXELS 21

// Input a value 0 to 255 to get a color value.
// The colours are a transition r - g - b - back to r.
uint32_t Rainbow::wheel(uint8_t WheelPos, uint8_t intensity)
{
	if (WheelPos < 85)
	{
		return Utils::toColor(WheelPos * 3 * intensity / 255, (255 - WheelPos * 3) * intensity / 255, 0);
	}
	else if (WheelPos < 170)
	{
		WheelPos -= 85;
		return Utils::toColor((255 - WheelPos * 3) * intensity / 255, 0, WheelPos * 3 * intensity / 255);
	}
	else
	{
		WheelPos -= 170;
		return Utils::toColor(0, WheelPos * 3 * intensity / 255, (255 - WheelPos * 3) * intensity / 255);
	}
}

uint32_t Rainbow::faceWheel(uint8_t face, uint8_t count) {
	return wheel((face * 256) / count);
}
