// APA102LEDs.h

#ifndef _APA102LEDS_h
#define _APA102LEDS_h

#include "Arduino.h"
#include "Adafruit_DotStar.h"

extern Adafruit_DotStar strip;

namespace Devices
{
#define LED_COUNT (21)

	/// <summary>
	/// Controls the APA102 LEDs on the Dice through a simple interface
	/// </summary>
	class APA102LEDs
	{
	public:
		void init();
		void stop();
		void set(int face, int led, uint32_t color, bool flush);
		void set(int ledIndex, uint32_t color, bool flush);
		void show();
		void clear();
		static int ledIndex(int face, int led);
	};
}


#endif

