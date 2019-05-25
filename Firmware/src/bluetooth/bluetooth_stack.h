#pragma once

namespace Bluetooth
{
    namespace Stack
    {
        void init();
        void disconnect();
        void startAdvertising();
        void disableAdvertisingOnDisconnect();
    }
}