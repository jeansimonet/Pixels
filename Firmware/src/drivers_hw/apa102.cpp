#include "apa102.h"
#include "nrf_gpio.h"
#include "nrf_delay.h"
#include "config/board_config.h"
#include "string.h" // for memset
#include "../utils/Rainbow.h"
#include "core/delegate_array.h"
#include "../drivers_nrf/log.h"
#include "../drivers_nrf/power_manager.h"

using namespace Config;
using namespace DriversNRF;

#define OFFSET_RED 2
#define OFFSET_GREEN 1
#define OFFSET_BLUE 0

#define MAX_APA102_CLIENTS 2
namespace DriversHW
{
namespace APA102
{
	static uint8_t pixels[MAX_LED_COUNT * 3]; // LED RGB values (3 bytes ea.)
	static uint8_t numLEDs;
	static uint8_t dataPin;
	static uint8_t clockPin;
	static uint8_t powerPin;

	DelegateArray<APA102ClientMethod, MAX_APA102_CLIENTS> ledPowerClients;

	void init() {

		// Cache configuration data
		auto board = BoardManager::getBoard();
		dataPin = board->ledDataPin;
		clockPin = board->ledClockPin;
		powerPin = board->ledPowerPin;
		numLEDs = board->ledCount;
		clear();

		// Initialize the pins
		nrf_gpio_cfg(dataPin,
    		NRF_GPIO_PIN_DIR_OUTPUT,
    		NRF_GPIO_PIN_INPUT_DISCONNECT,
    		NRF_GPIO_PIN_NOPULL,
			NRF_GPIO_PIN_S0S1,
    		NRF_GPIO_PIN_NOSENSE);

		nrf_gpio_cfg(clockPin,
    		NRF_GPIO_PIN_DIR_OUTPUT,
    		NRF_GPIO_PIN_INPUT_DISCONNECT,
    		NRF_GPIO_PIN_NOPULL,
			NRF_GPIO_PIN_S0S1,
    		NRF_GPIO_PIN_NOSENSE);

		nrf_gpio_cfg_output(powerPin);
		nrf_gpio_pin_clear(dataPin);
		nrf_gpio_pin_clear(clockPin);
		nrf_gpio_pin_clear(powerPin);


		#if DICE_SELFTEST && APA102_SELFTEST
		selfTest();
		#endif

		NRF_LOG_INFO("APA102 Initialized");
	}

	void clear() {
		memset(pixels, 0, numLEDs * 3);
	}

	#define spi_out(n) swSpiOut(n)

	void swSpiOut(uint8_t n) { // Bitbang SPI write
		for (uint8_t i = 8; i--; n <<= 1) {
			if (n & 0x80) {
				nrf_gpio_pin_set(dataPin);
			} else {
				nrf_gpio_pin_clear(dataPin);
			}
			nrf_gpio_pin_set(clockPin);
			nrf_gpio_pin_clear(clockPin);
		}
	}

	void prepare(void) {
		// Sets the power pin on, but doesn't write any data
		// Turn power on so we display something!!!
		if (nrf_gpio_pin_out_read(powerPin) == 0) {

			// Notify clients we're turning led power on
			for (int i = 0; i < ledPowerClients.Count(); ++i) {
				ledPowerClients[i].handler(ledPowerClients[i].token, true);
			}

			nrf_gpio_pin_set(powerPin);

			for (int j = 0; j < 10; ++j) {
				nrf_delay_ms(1);
				for (int i = 0; i < 4; i++) {
					swSpiOut(0);    // Start-frame marker
				}
				for (int i = 0; i < numLEDs; ++i) {
					swSpiOut(0xFF); // start
					swSpiOut(0x00); // r
					swSpiOut(0x00); // g
					swSpiOut(0x00); // b
				}
				for (int i = 0; i < ((numLEDs + 15) / 16); i++) {
					swSpiOut(0xFF); // End-frame marker
				}
			}
		}
	}

	void show(void) {

		if (!pixels) return;

		// Turn power on so we display something!!!
		prepare();

		uint8_t *ptr = pixels;            // -> LED data
		uint16_t n = numLEDs;              // Counter

		bool allOff = true;
		for (int i = 0; i < 4; i++) {
			swSpiOut(0);    // Start-frame marker
		}
		do {                               // For each pixel...
			swSpiOut(0xFF);                //  Pixel start
			for (int i = 0; i < 3; i++) {
				uint8_t comp = *ptr;
				swSpiOut(comp); // R,G,B
				if (comp != 0) {
					// At least one component of one led was not 0
					allOff = false;
				}
				ptr++;
			}
		} while (--n);
		for (int i = 0; i < ((numLEDs + 15) / 16); i++) {
			swSpiOut(0xFF); // End-frame marker (see note above)
		}

		if (allOff) {
			// Turn power off too
			nrf_delay_ms(1);
			nrf_gpio_pin_clear(powerPin);
			nrf_gpio_pin_clear(dataPin);
			nrf_gpio_pin_clear(clockPin);

			// Notify clients we're turning led power off
			for (int i = 0; i < ledPowerClients.Count(); ++i) {
				ledPowerClients[i].handler(ledPowerClients[i].token, false);
			}
		}
	}

	// Set pixel color, separate R,G,B values (0-255 ea.)
	void setPixelColor(
		uint16_t n, uint8_t r, uint8_t g, uint8_t b) {
		if (n < numLEDs) {
			uint8_t *p = &pixels[n * 3];
			p[OFFSET_RED] = r;
			p[OFFSET_GREEN] = g;
			p[OFFSET_BLUE] = b;
		}
	}

	// Set pixel color, 'packed' RGB value (0x000000 - 0xFFFFFF)
	void setPixelColor(uint16_t n, uint32_t c) {
		if (n < numLEDs) {
			uint8_t *p = &pixels[n * 3];
			p[OFFSET_RED] = (uint8_t)(c >> 16);
			p[OFFSET_GREEN] = (uint8_t)(c >> 8);
			p[OFFSET_BLUE] = (uint8_t)c;
		}
	}

    void setAll(uint32_t c) {
		for (int i = 0; i < numLEDs; ++i) {
			uint8_t *p = &pixels[i * 3];
			p[OFFSET_RED] = (uint8_t)(c >> 16);
			p[OFFSET_GREEN] = (uint8_t)(c >> 8);
			p[OFFSET_BLUE] = (uint8_t)c;
		}
	}

	void setPixelColors(int* indices, uint32_t* colors, int count)
	{
		for (int i = 0; i < count; ++i) {
			int n = indices[i];
			uint32_t c = colors[i];
			if (n < numLEDs) {
				uint8_t *p = &pixels[n * 3];
				p[OFFSET_RED] = (uint8_t)(c >> 16);
				p[OFFSET_GREEN] = (uint8_t)(c >> 8);
				p[OFFSET_BLUE] = (uint8_t)c;
			}
		}
	}

    void setPixelColors(uint32_t* colors) {
		for (int i = 0; i < numLEDs; ++i) {
			uint32_t c = colors[i];
			uint8_t *p = &pixels[i * 3];
			p[OFFSET_RED] = (uint8_t)(c >> 16);
			p[OFFSET_GREEN] = (uint8_t)(c >> 8);
			p[OFFSET_BLUE] = (uint8_t)c;
		}
	}


	// Convert separate R,G,B to packed value
	uint32_t color(uint8_t r, uint8_t g, uint8_t b) {
		return ((uint32_t)r << 16) | ((uint32_t)g << 8) | b;
	}

	// Read color from previously-set pixel, returns packed RGB value.
	uint32_t getPixelColor(uint16_t n) {
		if (n >= numLEDs) return 0;
		uint8_t *p = &pixels[n * 3];
		return ((uint32_t)p[OFFSET_RED] << 16) |
			((uint32_t)p[OFFSET_GREEN] << 8) |
			(uint32_t)p[OFFSET_BLUE];
	}

	uint16_t numPixels() { // Ret. strip length
		return numLEDs;
	}

	// Return pointer to the library's pixel data buffer.  Use carefully,
	// much opportunity for mayhem.  It's mostly for code that needs fast
	// transfers, e.g. SD card to LEDs.  Color data is in BGR order.
	uint8_t* getPixels() {
		return pixels;
	}

	void hookPowerState(APA102ClientMethod method, void* param) {
		ledPowerClients.Register(param, method);
	}

	void unHookPowerState(APA102ClientMethod method) {
		ledPowerClients.UnregisterWithHandler(method);
	}

	void unHookPowerStateWithParam(void* param) {
		ledPowerClients.UnregisterWithToken(param);
	}


	#if DICE_SELFTEST && APA102_SELFTEST
	void selfTest() {

        NRF_LOG_INFO("Turning LEDs On, press any key to stop");
		for (int i = 0; i < numLEDs; ++i) {
			int phase = 255 * i / numLEDs;
			uint32_t color = Rainbow::wheel(phase);
			setPixelColor(i, color);
		}
		show();
		int loop = 0;
        while (true) {
			// PowerManager::feed();
            // PowerManager::update();

			for (int i = 0; i < numLEDs; ++i) {
				int phase = ((int)(255 * i / numLEDs) + loop) % 256;
				uint32_t color = Rainbow::wheel(phase);
				setPixelColor(i, color);
			}
			show();

			loop++;
			nrf_delay_ms(100);
        }
		Log::getKey();
        NRF_LOG_INFO("Turning LEDs Off!");
    	clear();
		show();
	}
	#endif
}
}

// // Slightly different, this makes the rainbow equally distributed throughout
// void rainbowCycle(uint8_t wait, uint8_t intensity)
// {
// 	uint16_t i, j;

// 	for (j = 0; j<256; j++)
// 	{
// 		for (i = 0; i< NUMPIXELS; i++)
// 		{
// 			strip.setPixelColor(i, Wheel(((i * 256 / strip.numPixels()) + j) & 255, intensity));
// 		}
// 		strip.show();
// 		nrf_delay_ms(wait);
// 	}
// }

// // Slightly different, this makes the rainbow equally distributed throughout
// void rainbowAll(int repeat, uint8_t wait, uint8_t intensity)
// {
// 	uint16_t i, j;

// 	for (int k = 0; k < repeat; ++k)
// 	{
// 		for (j = 0; j<256; j++)
// 		{
// 			uint32_t color = Wheel(j, intensity);
// 			for (i = 0; i< NUMPIXELS; i++)
// 			{
// 				strip.setPixelColor(i, color);
// 			}
// 			strip.show();
// 			nrf_delay_ms(wait);
// 		}
// 	}
// }

