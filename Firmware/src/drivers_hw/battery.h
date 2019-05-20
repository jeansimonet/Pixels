#pragma once

namespace DriversHW
{
    namespace Battery
    {
        void init();
        float checkVBat();
        bool checkCharging();
        bool checkVCCFault();

        void selfTest();
    }
}

