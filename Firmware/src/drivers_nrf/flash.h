#pragma once

#include <stdint.h>
#include <stddef.h>

namespace DataSet
{
    struct Data;
}

namespace Config
{
    struct Settings;
}

namespace DriversNRF
{
	namespace Flash
	{
		void init();
        void printFlashInfo();
        //void waitForFlashReady();

        typedef void (*FlashCallback)(void* context, bool result, uint32_t address, uint16_t size);

        void write(void* context, uint32_t flashAddress, const void* data, uint32_t size, FlashCallback callback);
        void read(void* context, uint32_t flashAddress, void* outData, uint32_t size, FlashCallback callback);
        void erase(void* context, uint32_t flashAddress, uint32_t pages, FlashCallback callback);

        uint32_t getFlashStartAddress();
        uint32_t getFlashEndAddress();
        uint32_t getUsableBytes();
        uint32_t getPageSize();
        uint32_t bytesToPages(uint32_t size);
        uint32_t getFlashByteSize(uint32_t totalDataByteSize);

        uint32_t getDataSetAddress();
        uint32_t getDataSetDataAddress();
        uint32_t getSettingsStartAddress();
        uint32_t getSettingsEndAddress();

        typedef void (*ProgramFlashNotification)(bool result);
        typedef void (*ProgramFlashFuncCallback)(void* context, bool result, uint32_t address, uint16_t size);
        typedef void (*ProgramFlashFunc)(ProgramFlashFuncCallback callback);

        bool programFlash(
            const DataSet::Data& newData,
            const Config::Settings& newSettings,
            ProgramFlashFunc programFlashFunc,
            ProgramFlashNotification onProgramFinished);


        enum ProgrammingEventType
        {
            ProgrammingEventType_Begin = 0,
            ProgrammingEventType_End
        };

        typedef void (*ProgrammingEventMethod)(void* param, ProgrammingEventType evt);
        void hookProgrammingEvent(ProgrammingEventMethod client, void* param);
        void unhookProgrammingEvent(ProgrammingEventMethod client);

        void selfTest();
	}
}

