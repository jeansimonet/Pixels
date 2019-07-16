#pragma once

#include "stdint.h"

namespace Bluetooth
{
    namespace Stack
    {
        void init();
        void disconnect();
        void startAdvertising();
        void disableAdvertisingOnDisconnect();
        bool canSend();
        bool send(uint16_t handle, const uint8_t* data, uint16_t len);
        void slowAdvertising();
        void stopAdvertising();
    }
}