#pragma once

#include <stdint.h>
#include <stddef.h>

namespace DriversNRF
{
	namespace Flash
	{
		void init();
        void printFlashInfo();
        //void waitForFlashReady();

        typedef void (*FlashCallback)(bool result, uint32_t address, uint16_t size);

        void write(uint32_t flashAddress, const void* data, uint32_t size, FlashCallback callback);
        void read(uint32_t flashAddress, void* outData, uint32_t size, FlashCallback callback);
        void erase(uint32_t flashAddress, uint32_t pages, FlashCallback callback);

        uint32_t getFlashStartAddress();
        uint32_t getFlashEndAddress();
        uint32_t getPageSize();
        uint32_t bytesToPages(uint32_t size);

        void selfTest();
	}
}

