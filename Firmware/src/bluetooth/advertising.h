#pragma once

namespace bluetooth
{
    class Advertising
    {
    public:
        void init();
        void startAdvertising();
        void disableAdvertisingOnDisconnect();
    };

    extern Advertising advertising;
}
