// 
// 
// 

#include "Rainbow.h"
#include "APA102LEDs.h"
#include "nrf_delay.h"

#define NUMPIXELS 21

// Input a value 0 to 255 to get a color value.
// The colours are a transition r - g - b - back to r.
uint32_t Rainbow::Wheel(uint8_t WheelPos, uint8_t intensity)
{
	if (WheelPos < 85)
	{
		return strip.Color(WheelPos * 3 * intensity / 255, (255 - WheelPos * 3) * intensity / 255, 0);
	}
	else if (WheelPos < 170)
	{
		WheelPos -= 85;
		return strip.Color((255 - WheelPos * 3) * intensity / 255, 0, WheelPos * 3 * intensity / 255);
	}
	else
	{
		WheelPos -= 170;
		return strip.Color(0, WheelPos * 3 * intensity / 255, (255 - WheelPos * 3) * intensity / 255);
	}
}

// Slightly different, this makes the rainbow equally distributed throughout
void Rainbow::rainbowCycle(uint8_t wait, uint8_t intensity)
{
	uint16_t i, j;

	for (j = 0; j<256; j++)
	{
		for (i = 0; i< NUMPIXELS; i++)
		{
			strip.setPixelColor(i, Wheel(((i * 256 / strip.numPixels()) + j) & 255, intensity));
		}
		strip.show();
		nrf_delay_ms(wait);
	}
}

// Slightly different, this makes the rainbow equally distributed throughout
void Rainbow::rainbowAll(int repeat, uint8_t wait, uint8_t intensity)
{
	uint16_t i, j;

	for (int k = 0; k < repeat; ++k)
	{
		for (j = 0; j<256; j++)
		{
			uint32_t color = Wheel(j, intensity);
			for (i = 0; i< NUMPIXELS; i++)
			{
				strip.setPixelColor(i, color);
			}
			strip.show();
			nrf_delay_ms(wait);
		}
	}
}

