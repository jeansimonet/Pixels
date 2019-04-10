#include "LEDs.h"
#include "Utils.h"

using namespace Core;

LEDs leds;

#define LED_COUNT (21)

LEDs::LEDs()
{
}

void LEDs::init()
{
	RGBLeds.init();
}

void LEDs::stop()
{
	RGBLeds.stop();
}

void LEDs::setLED(int face, int led, uint32_t color)
{
	setLED(ledIndex(face, led), color);
}

void LEDs::setLED(int index, uint32_t color)
{
	RGBLeds.set(index, color, true);
}

void LEDs::setLEDs(int indices[], uint32_t colors[], int count)
{
	for (int i = 0; i < count; ++i)
	{
		RGBLeds.set(indices[i], colors[i], false);
	}
	RGBLeds.show();
}

void LEDs::setAll(uint32_t color)
{
	for (int i = 0; i < LED_COUNT; ++i)
	{
		RGBLeds.set(i, color, false);
	}
	RGBLeds.show();
}

void LEDs::clearAll()
{
	RGBLeds.clear();
}

int LEDs::ledIndex(int face, int led)
{
	return Devices::APA102LEDs::ledIndex(face, led);
}
