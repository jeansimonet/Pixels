#pragma once

namespace Modules
{
	/// <summary>
	/// Controls the APA102 LEDs on the Dice through a simple interface
	/// </summary>
	namespace LEDs
	{
		void init();
		void stop();
		void set(int face, int led, uint32_t color, bool flush);
		void set(int ledIndex, uint32_t color, bool flush);
		void setLEDs(int* indices, uint32_t* colors, int count);
		void setAll(uint32_t color);
		void show();
		void clearAll();
		int ledIndex(int face, int led);
	}
}

