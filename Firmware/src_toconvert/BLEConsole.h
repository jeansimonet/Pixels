// BLEConsole.h

#ifndef _BLECONSOLE_h
#define _BLECONSOLE_h

#if defined(_BLECONSOLE)
#include "arduino.h"
#include "Print.h"

namespace Systems
{
#define MAX_WORDS 4

	/// <summary>
	/// Serial port console, used to debug and configure a die
	/// </summary>
	class BLEConsole
		: public Print
	{
	public:
		BLEConsole();

		virtual size_t write(uint8_t);
		virtual size_t write(const uint8_t *buffer, size_t size);
	};

	extern BLEConsole bleConsole;
}


#endif
#endif

