#include "utils.h"
#include <nrf_delay.h>
#include <app_timer.h>

/// <summary>
/// Parses the first word out of a string (typically a command or parameter)
/// </summary>
/// <param name="text">The string to parse the first word from</param>
/// <param name="len">The length of the string</param>
/// <param name="outWord">The return string buffer</param>
/// <param name="outWordLen">The max length of the return string buffer</param>
/// <returns>The length of the found word, otherwise 0</returns>
int Utils::parseWord(char*& text, int& len, char* outWord, int outWordLen)
{
	while (len > 0&& (*text == ' ' || *text == '\t'))
	{
		text++;
		len--;
	}

	int wordLen = 0;
	if (len > 0)
	{
		while (len > 0 && wordLen < outWordLen && *text != ' ' && *text != '\t' && *text != '\n' && *text != '\r' && *text != 0)
		{
			*outWord = *text;
			outWord++;
			text++;
			len--;
			wordLen++;
		}

		*outWord = 0;
		wordLen++;
	}

	return wordLen;
}

#define APP_TIMER_MS(TICKS) ((uint32_t)ROUNDED_DIV((TICKS) * 1000 * (APP_TIMER_CONFIG_RTC_FREQUENCY + 1), (uint64_t)APP_TIMER_CLOCK_FREQ))

uint32_t Utils::millis( void )
{
	auto ticks = app_timer_cnt_get();
	return APP_TIMER_MS(ticks);
}

