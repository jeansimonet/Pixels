#include "LEDs.h"
#include "drivers_hw/apa102.h"

namespace Modules
{
namespace LEDs
{
	/// <summary>
	/// Defines how to look up the LEDs of a face in the led array above!
	/// </summary>
	struct Face
	{
		int LedsStartIndex;
		int LedCount;
	};

	void LEDs::init()
	{
		nrf_gpio_cfg_output(POWERPIN);
		nrf_gpio_pin_set(POWERPIN);

		strip.begin(); // Initialize pins for output
		for (int i = 0; i < NUMPIXELS; ++i)
		{
			strip.setPixelColor(i, 0);
		}
		strip.show();  // Turn all LEDs off ASAP
	}

	void LEDs::stop()
	{
		clearAll();
		nrf_gpio_pin_set(POWERPIN);
		nrf_gpio_pin_clear(DATAPIN);
		nrf_gpio_pin_clear(CLOCKPIN);
	}

	void LEDs::set(int face, int led, uint32_t color, bool flush)
	{
		set(ledIndex(face, led), color, flush);
	}

	void LEDs::set(int ledIndex, uint32_t color, bool flush)
	{
		int theled = theLeds[ledIndex];
		strip.setPixelColor(theled, color);
		if (flush)
		{
			show();
		}
	}

	void LEDs::setLEDs(int* indices, uint32_t* colors, int count)
	{
		for (int i = 0; i < count; ++i) {
			strip.setPixelColor(indices[i], colors[i]);
		}
		strip.show();
	}


	void LEDs::setAll(uint32_t color)
	{
		for (int i = 0; i < LED_COUNT; ++i) {
			strip.setPixelColor(i, color);
		}
		strip.show();
	}


	void LEDs::show()
	{
		strip.show();
	}

	void LEDs::clearAll()
	{
		for (int i = 0; i < NUMPIXELS; ++i)
		{
			strip.setPixelColor(i, 0);
		}
		strip.show();
	}

	int LEDs::ledIndex(int face, int led)
	{
		return RGBFaces[face].LedsStartIndex + led;
	}

}
}

