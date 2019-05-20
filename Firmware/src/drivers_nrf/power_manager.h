#pragma once

#include "nrf_pwr_mgmt.h"

namespace DriversNRF
{
    // Initializes the sdk log system
    namespace PowerManager
    {
        void init();
        void feed();
        void update();
        void pause();
        void resume();
        void goToSystemOff();
    }
}
