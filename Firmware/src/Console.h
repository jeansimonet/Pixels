// DiceCommands.h

#ifndef _DICECONSOLE_h
#define _DICECONSOLE_h

#if defined(_CONSOLE)
#include "Arduino.h"
#include "Print.h"

namespace Systems
{
	/// <summary>
	/// Serial port console, used to debug and configure a die
	/// </summary>
	class Console : public Print
	{
	public:
    	Console();
	    virtual size_t write(uint8_t);
    	virtual size_t write(const uint8_t *buffer, size_t size);
	};

	extern Console console;
}

#endif
#endif