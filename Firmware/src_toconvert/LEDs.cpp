#include "LEDs.h"
#include "Utils.h"

using namespace Core;

LEDs leds;

#define LED_COUNT (21)

LEDs::LEDs()
#if !defined(RGB_LED)
	: controller(messageQueue)
#endif
{
}

void LEDs::init()
{
#if defined(RGB_LED)
	RGBLeds.init();
#else
	GPIOLeds.init();
	controller.begin();
#endif
}

void LEDs::stop()
{
#if !defined(RGB_LED)
	controller.stop();
#else
	RGBLeds.stop();
#endif
}

void LEDs::setLED(int face, int led, uint32_t color)
{
	setLED(ledIndex(face, led), color);
}

void LEDs::setLED(int index, uint32_t color)
{
#if defined(RGB_LED)
	RGBLeds.set(index, color, true);
#else
	controller.setLED(index, getGreyscale(color));
#endif
}

void LEDs::setLEDs(int indices[], uint32_t colors[], int count)
{
#if defined(RGB_LED)
	for (int i = 0; i < count; ++i)
	{
		RGBLeds.set(indices[i], colors[i], false);
	}
	RGBLeds.show();
#else
	int intensities[LED_COUNT];
	for (int i = 0; i < count; ++i)
	{
		intensities[i] = getGreyscale(colors[i]);
	}
	controller.setLEDs(indices, intensities, count);
#endif
}

void LEDs::setAll(uint32_t color)
{
#if defined(RGB_LED)
	for (int i = 0; i < LED_COUNT; ++i)
	{
		RGBLeds.set(i, color, false);
	}
	RGBLeds.show();
#else
	int indices[LED_COUNT];
	int intensities[LED_COUNT];
	byte greyscale = getGreyscale(color);
	for (int i = 0; i < LED_COUNT; ++i)
	{
		indices[i] = i;
		intensities[i] = getGreyscale(greyscale);
	}
	controller.setLEDs(indices, intensities, LED_COUNT);
#endif
}

void LEDs::clearAll()
{
#if defined(RGB_LED)
	RGBLeds.clear();
#else
	controller.clearAll();
#endif
}

int LEDs::ledIndex(int face, int led)
{
#if defined(RGB_LED)
	return Devices::APA102LEDs::ledIndex(face, led);
#else
	return Devices::GPIOLEDs::ledIndex(face, led);
#endif
}
