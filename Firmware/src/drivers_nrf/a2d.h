#pragma once

#include <stdint.h>
#include <stddef.h>

namespace DriversNRF
{
	/// <summary>
	/// Wrapper for the Wire library that is set to use the Die pins
	/// </summary>
	namespace A2D
	{
		void init();
        int16_t readConfigPin();

		void initBoardPins();
        int16_t readBatteryPin();
        int16_t read5VPin();
        int16_t readVLEDPin();

        float readVBat();
        float read5V();
        float readVLED();
        float readVBoard();

		void selfTest();
		void selfTestBatt();
	}
}

