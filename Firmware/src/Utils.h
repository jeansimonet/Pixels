// ConsoleUtils.h

#ifndef _CONSOLEUTILS_h
#define _CONSOLEUTILS_h

#include <stdint.h>
#include <algorithm>

namespace Core
{
	int parseWord(char*& text, int& len, char* outWord, int outWordLen);

	constexpr uint32_t toColor(uint8_t red, uint8_t green, uint8_t blue) { return red << 16 | green << 8 | blue; }
	constexpr uint8_t getRed(uint32_t color) { return (color >> 16) & 0xFF; }
	constexpr uint8_t getGreen(uint32_t color) { return (color >> 8) & 0xFF; }
	constexpr uint8_t getBlue(uint32_t color) { return (color) & 0xFF; }
	constexpr uint8_t getGreyscale(uint32_t color) {
		return std::max(getRed(color), std::max(getGreen(color), getBlue(color)));
	}

	uint32_t millis( void );
}

#endif

