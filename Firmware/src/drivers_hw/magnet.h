#pragma once

namespace DriversHW
{
    namespace Magnet
    {
        void init();

        bool checkMagnet();
        bool canCheckMagnet();

		typedef void(*ClientMethod)(void* param);

		// Notification management
		void hook(ClientMethod method, void* param);
		void unHook(ClientMethod client);
		void unHookWithParam(void* param);

        void selfTest();
    }
}

