#pragma once

namespace DriversHW
{
    namespace Battery
    {
        void init();
        float checkVBat();
        float checkVCoil();
        bool canCheckVCoil();
        float checkVLED();
        bool canCheckVLED();
        bool checkCharging();
        bool canCheckCharging();

		typedef void(*ClientMethod)(void* param);

		// Notification management
		void hook(ClientMethod method, void* param);
		void unHook(ClientMethod client);
		void unHookWithParam(void* param);

        void selfTest();
    }
}

