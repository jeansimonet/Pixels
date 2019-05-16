#include "LEDs.h"
#include "apa102.h"
#include "nrf_gpio.h"

using namespace Modules;

LEDs Modules::leds;

/// <summary>
/// Define led index in strip as per the routing of the pcb
/// </summary>
int theLeds[] =
{
	// Face 1
	17,
	// Face 2
	15, 16,
	// Face 3
	18, 19, 20,
	// Face 4
	2, 3, 5, 4,
	// Face 5
	0, 1, 7, 8, 6,
	// Face 6
	11, 12, 10, 13, 9, 14,
};

///// <summary>
///// Define led index in strip as per the routing of the pcb
///// </summary>
//int theLeds[] =
//{
//	// Face 1
//	17,
//	// Face 2
//	15, 16,
//	// Face 3
//	18, 19, 20,
//	// Face 4
//	2, 3, 0, 1,
//	// Face 5
//	4, 5, 7, 6, 8,
//	// Face 6
//	9, 12, 10, 11, 13, 14
//};

/// <summary>
/// Defines how to look up the LEDs of a face in the led array above!
/// </summary>
struct Face
{
	int LedsStartIndex;
	int LedCount;
};

/// <summary>
/// Represent the offsets into the LEDs array for all 6 faces
/// </summary>
Face RGBFaces[] =
{
	{ 0, 1 },
	{ 1, 2 },
	{ 3, 3 },
	{ 6, 4 },
	{ 10, 5 },
	{ 15, 6 }
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

