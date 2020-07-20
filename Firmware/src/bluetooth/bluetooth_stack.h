#pragma once

#include "stdint.h"

namespace Bluetooth
{
    namespace Stack
    {
        void init();
        void initAdvertising();
        void initCustomAdvertisingData();
        void disconnect();
        void startAdvertising();
        void disableAdvertisingOnDisconnect();
        bool canSend();

        enum SendResult
        {
            SendResult_Ok = 0,
            SendResult_Busy,            // Should try again later
            SendResult_NotConnected,    // No connection
            SendResult_Error,           // Any other error
        };

        SendResult send(uint16_t handle, const uint8_t* data, uint16_t len);
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