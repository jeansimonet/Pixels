// DiceWire.h
#ifndef _DICEWIRE_h
#define _DICEWIRE_h

#include "Arduino.h"

namespace Systems
{
	/// <summary>
	/// Wrapper for the Wire library that is set to use the Die pins
	/// </summary>
	class I2C
	{
	public:
		void begin();
		void beginTransmission(uint8_t);
		void beginTransmission(int);
		void end();

		uint8_t endTransmission(void);
		uint8_t endTransmission(uint8_t);
		uint8_t requestFrom(uint8_t, uint8_t);
		uint8_t requestFrom(uint8_t, uint8_t, uint8_t);
		uint8_t requestFrom(int, int);
		uint8_t requestFrom(int, int, int);
		size_t write(uint8_t);
		size_t write(const uint8_t *, size_t);
		int available(void);
		int read(void);
		int peek(void);
		void flush(void);
		void onReceive(void(*)(int));
		void onRequest(void(*)(void));
		void onRequestService(void);
		void onReceiveService(uint8_t*, int);
	};

	extern I2C wire;
}

#endif

