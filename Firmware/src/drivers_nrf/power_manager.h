#pragma once

namespace DriversNRF
{
    // Initializes the sdk log system
    namespace PowerManager
    {
        void init();
        void feed();
        void update();
        void goToSystemOff();
    }
}
