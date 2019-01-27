#include "Console.h"
//#include "Utils.h"
//#include "Die.h"
//#include "LEDs.h"
#include <SEGGER_RTT.h>

#if defined(_CONSOLE)

using namespace Systems;
//using namespace Core;

Console Systems::console;

#define CONSOLE_RTT_CHANNEL 0

/// <summary>
/// Initialize the console
/// </summary>
Console::Console()
	: Print()
{
	SEGGER_RTT_Init();
	SEGGER_RTT_SetTerminal(0);
	println("Console ON");
}

size_t Console::write(uint8_t c)
{
	return SEGGER_RTT_Write(CONSOLE_RTT_CHANNEL, &c, 1);
}

size_t Console::write(const uint8_t *buffer, size_t size)
{
	return SEGGER_RTT_Write(CONSOLE_RTT_CHANNEL, buffer, size);
}

#endif