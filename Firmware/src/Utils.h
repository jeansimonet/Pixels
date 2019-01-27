// ConsoleUtils.h

#ifndef _CONSOLEUTILS_h
#define _CONSOLEUTILS_h

#include "Arduino.h"

namespace Core
{
	int parseWord(char*& text, int& len, char* outWord, int outWordLen);

	constexpr uint32_t toColor(byte red, byte green, byte blue) { return red << 16 | green << 8 | blue; }
	constexpr byte getRed(uint32_t color) { return (color >> 16) & 0xFF; }
	constexpr byte getGreen(uint32_t color) { return (color >> 8) & 0xFF; }
	constexpr byte getBlue(uint32_t color) { return (color) & 0xFF; }
	constexpr byte getGreyscale(uint32_t color) {
		return max(getRed(color), max(getGreen(color), getBlue(color)));
	}
}

#endif

