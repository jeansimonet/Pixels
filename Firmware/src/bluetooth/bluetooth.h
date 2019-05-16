#pragma once

namespace bluetooth
{
    class Bluetooth
    {
    public:
        void init();
        void disconnect();
    };

    extern Bluetooth bluetooth;
}
