// APA102LEDs.h

#ifndef _APA102LEDS_h
#define _APA102LEDS_h

#include "apa102.h"

extern Adafruit_DotStar strip;

namespace Modules
{
#define LED_COUNT (20)

	/// <summary>
	/// Controls the APA102 LEDs on the Dice through a simple interface
	/// </summary>
	class LEDs
	{
	public:
		void init();
		void stop();
		void set(int face, int led, uint32_t color, bool flush);
		void set(int ledIndex, uint32_t color, bool flush);
		void setLEDs(int* indices, uint32_t* colors, int count);
		void setAll(uint32_t color);
		void show();
		void clearAll();
		static int ledIndex(int face, int led);
	};

	extern LEDs leds;
}


#endif

