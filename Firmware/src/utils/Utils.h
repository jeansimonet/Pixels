#pragma once

#include <stdint.h>
#include <algorithm>

/**
 * @brief Macro to be used in a formatted string to a pass float number to the log.
 *
 * Use this macro in a formatted string instead of the %f specifier together with
 * @ref NRF_LOG_FLOAT macro.
 * Example: NRF_LOG_INFO("My float number" NRF_LOG_FLOAT_MARKER "\r\n", NRF_LOG_FLOAT(f)))
 */
#define SPRINTF_FLOAT_MARKER "%s%d.%02d"

/**
 * @brief Macro for dissecting a float number into two numbers (integer and residuum).
 */
#define SPRINTF_FLOAT(val) (((val) < 0 && (val) > -1.0) ? "-" : ""),   \
                           (int)(val),                                 \
                           (int)((((val) > 0) ? (val) - (int)(val)     \
                                              : (int)(val) - (val))*100)

namespace Core
{
	struct float3;
}

namespace Utils
{
	int parseWord(char*& text, int& len, char* outWord, int outWordLen);

	constexpr uint32_t toColor(uint8_t red, uint8_t green, uint8_t blue) { return (uint32_t)(red << 16) | (uint32_t)(green << 8) | (uint32_t)blue; }
	constexpr uint8_t getRed(uint32_t color) { return (color >> 16) & 0xFF; }
	constexpr uint8_t getGreen(uint32_t color) { return (color >> 8) & 0xFF; }
	constexpr uint8_t getBlue(uint32_t color) { return (color) & 0xFF; }
	constexpr uint8_t getGreyscale(uint32_t color) {
		return std::max(getRed(color), std::max(getGreen(color), getBlue(color)));
	}

	uint32_t millis( void );
	uint8_t sine8(uint8_t x);
	uint8_t gamma8(uint8_t x);
	uint32_t gamma(uint32_t color);

	void CalibrateNormals(
		int face1Index, const Core::float3& face1Normal,
		int face2Index, const Core::float3& face2Normal,
		Core::float3* inOutNormals, int count);
}

