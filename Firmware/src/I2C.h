// DiceWire.h
#ifndef _DICEWIRE_h
#define _DICEWIRE_h

#include <stdint.h>
#include <stddef.h>

namespace Systems
{
	/// <summary>
	/// Wrapper for the Wire library that is set to use the Die pins
	/// </summary>
	class I2C
	{
	public:
		void begin();
		void end();

		bool write(uint8_t device, uint8_t value, bool no_stop = false);
		bool write(uint8_t device, const uint8_t* data, size_t size, bool no_stop = false);
		bool read(uint8_t device, uint8_t* value);
		bool read(uint8_t device, uint8_t* data, size_t size);
	};

	extern I2C wire;
}

#endif

