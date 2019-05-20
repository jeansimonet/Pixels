#pragma once

#include <stdint.h>
#include <stddef.h>

namespace DriversNRF
{
	namespace Flash
	{
		void init();
        void printFlashInfo();
        void waitForFlashReady();
        void write(uint32_t flashAddress, void* data, uint32_t size);
        void read(uint32_t flashAddress, void* outData, uint32_t size);
        void erase(uint32_t flashAddress, uint32_t pages);

        void selfTest();
	}
}

