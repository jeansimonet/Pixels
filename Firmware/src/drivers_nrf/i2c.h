#pragma once

#include <stdint.h>
#include <stddef.h>

namespace DriversNRF
{
	/// <summary>
	/// Wrapper for the Wire library that is set to use the Die pins
	/// </summary>
	namespace I2C
	{
		void init();

		bool write(uint8_t device, uint8_t value, bool no_stop = false);
		bool write(uint8_t device, const uint8_t* data, size_t size, bool no_stop = false);
		bool read(uint8_t device, uint8_t* value);
		bool read(uint8_t device, uint8_t* data, size_t size);
	}
}

