#include "BLEConsole.h"
#include "Die.h"
#include "SimbleeBLE.h"

#if defined(_BLECONSOLE)

#define MAX_SIZE (19)

using namespace Systems;

BLEConsole Systems::bleConsole;

BLEConsole::BLEConsole()
	: Print()
{
}

size_t BLEConsole::write(uint8_t c)
{
	DieMessageDebugLog msg;
	msg.text[0] = c;
	msg.text[1] = '\0';
	SimbleeBLE.send(reinterpret_cast<const char*>(&msg), sizeof(DieMessageDebugLog));
}

size_t BLEConsole::write(const uint8_t *buffer, size_t size)
{
	size_t rem = size;
	while (rem > MAX_SIZE)
	{
		DieMessageDebugLog msg;
		memcpy(msg.text, buffer, MAX_SIZE);
		SimbleeBLE.send(reinterpret_cast<const char*>(&msg), sizeof(DieMessageDebugLog));
		buffer += MAX_SIZE;
		rem -= MAX_SIZE;
	}

	if (rem > 0)
	{
		DieMessageDebugLog msg;
		memcpy(msg.text, buffer, rem);
		msg.text[rem] = '\0';
		SimbleeBLE.send(reinterpret_cast<const char*>(&msg), sizeof(DieMessageDebugLog));
	}
}

#endif