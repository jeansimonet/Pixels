#pragma once
#include "core/delegate_array.h"

namespace DriversHW
{
    namespace Battery
    {
        void init();
        float checkVBat();
        bool checkCharging();
        bool checkCoil();

		typedef void(*ClientMethod)(void* param);

		// Notification management
		void hook(ClientMethod method, void* param);
		void unHook(ClientMethod client);
		void unHookWithParam(void* param);

        void selfTest();
    }
}

