#pragma once

#include "stdint.h"

namespace Bluetooth
{
    namespace Stack
    {
        void init();
        void initAdvertising();
        void disconnect();
        void startAdvertising();
        void disableAdvertisingOnDisconnect();
        bool canSend();
        bool send(uint16_t handle, const uint8_t* data, uint16_t len);
        void slowAdvertising();
        void stopAdvertising();
        bool isAdvertising();
        bool isConnected();

		typedef void(*ConnectionEventMethod)(void* param, bool connected);
		void hook(ConnectionEventMethod method, void* param);
		void unHook(ConnectionEventMethod client);
		void unHookWithParam(void* param);
    }
}