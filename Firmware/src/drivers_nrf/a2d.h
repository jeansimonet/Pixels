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

		void initBatteryPin();
        int16_t readBatteryPin();

        float readVBat();
        float readVBoard();

		void selfTest();
		void selfTestBatt();
	}
}

