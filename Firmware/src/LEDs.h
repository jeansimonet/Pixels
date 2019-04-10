// LEDs.h

#ifndef _LEDS_h
#define _LEDS_h


#include "APA102LEDs.h"

/// <summary>
/// Adapter class that uses either RGB leds or GPIO leds (for version 1 of the dice)
/// </summary>
class LEDs
{
public:
	LEDs();
	void init();
	void stop();

	void setLED(int face, int led, uint32_t color); // Index 0 - 20
	void setLED(int index, uint32_t color); // Index 0 - 20
	void setLEDs(int indices[], uint32_t colors[], int count);
	void setAll(uint32_t color);
	void clearAll();

	static int ledIndex(int face, int led);

	Devices::APA102LEDs RGBLeds;

	int queuedIndices[21];
	uint32_t queuedColors[21];
};

extern LEDs leds; 

#endif

