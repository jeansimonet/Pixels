#include "die.h"

#include "app_timer.h"
#include "app_error.h"

#include "drivers_nrf/watchdog.h"
#include "drivers_nrf/timers.h"
#include "drivers_nrf/log.h"
#include "drivers_nrf/dfu.h"
#include "drivers_nrf/a2d.h"
#include "drivers_nrf/power_manager.h"
#include "drivers_nrf/i2c.h"
#include "drivers_nrf/flash.h"

#include "config/board_config.h"
#include "config/settings.h"

#include "drivers_hw/apa102.h"
#include "drivers_hw/lis2de12.h"
#include "drivers_hw/battery.h"
#include "drivers_hw/magnet.h"

using namespace DriversNRF;
using namespace Config;
using namespace DriversHW;

namespace Die
{
    // Start the die please!
    void init() {

        //--------------------
        // Initialize NRF drivers
        //--------------------

        // Very first thing we want to init is the watchdog so we don't brick
        // later on if something bad happens.
#if !DICE_SELFTEST
        Watchdog::init();
#endif

        // Then DFU interrupt service, it's important to initialize
        // this as soon as possible
        DriversNRF::DFU::init();

        // Then the log system
        Log::init();

        // Then the timers
        Timers::init();

        // Power manager handles going to sleep and resetting the board
        PowerManager::init();
        
        // Analog to digital converter next, so we can
        // identify the board we're dealing with
        A2D::init();
        
        //--------------------
        // Fetch board configuration now, so we know how to initialize
        // the rest of the hardware (pins, led count, etc...)
        //--------------------
        // This will use the A2D converter to check the identifying resistor
        // on the board and determine what kind of die this is.
        BoardManager::init();

        // Now that we know which board we are, initialize the battery monitoring A2D
        A2D::initBatteryPin();

        // // Flash is needed to update settings/animations
        // Flash::init();

        // // I2C is needed for the accelerometer, but depends on the board info
        // I2C::init();

        // //--------------------
        // // Initialize Hardware drivers
        // //--------------------

        // // Lights also depend on board info
        // APA102::init();

        // // Accel pins depend on the board info
        // LIS2DE12::init();

        // Battery sense pin depends on board info
        Battery::init();

        // // Magnet, so we know if ne need to go into DFU mode immediately
        // Magnet::init(); 
        
        // // The we read user settings from flash, or set some defaults if none are found
        // SettingsManager::init();

    }

    // Main loop!
    void update() {
        if (!Log::process())
        {
            PowerManager::update();
        }
    }
}

int main() {
    Die::init();
    for (;;)
    {
        Die::update();
    }
    return 0;
}