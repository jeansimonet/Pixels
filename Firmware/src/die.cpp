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
#include "drivers_nrf/gpiote.h"

#include "config/board_config.h"
#include "config/settings.h"

#include "drivers_hw/apa102.h"
#include "drivers_hw/lis2de12.h"
#include "drivers_hw/battery.h"
#include "drivers_hw/magnet.h"

#include "nrf_sdh.h"
#include "nrf_sdh_ble.h"
#include "nrf_fstorage_sd.h"

using namespace DriversNRF;
using namespace Config;
using namespace DriversHW;

#define APP_BLE_CONN_CFG_TAG    1

/**@brief Callback function for asserts in the SoftDevice.
 *
 * @details This function will be called in case of an assert in the SoftDevice.
 *
 * @warning This handler is an example only and does not fit a final product. You need to analyze 
 *          how your product is supposed to react in case of Assert.
 * @warning On assert from the SoftDevice, the system can only recover on reset.
 *
 * @param[in]   line_num   Line number of the failing ASSERT call.
 * @param[in]   file_name  File name of the failing ASSERT call.
 */
void assert_nrf_callback(uint16_t line_num, const uint8_t * p_file_name)
{
    app_error_handler(0xDEADBEEF, line_num, p_file_name);
}


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

        // GPIO Interrupts
        GPIOTE::init();

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

    // ret_code_t rc;
    // uint32_t   ram_start;

        /* Enable the SoftDevice. */
        ret_code_t rc = nrf_sdh_enable_request();
        APP_ERROR_CHECK(rc);

    // rc = nrf_sdh_ble_default_cfg_set(APP_BLE_CONN_CFG_TAG, &ram_start);
    // APP_ERROR_CHECK(rc);

    // rc = nrf_sdh_ble_enable(&ram_start);
    // APP_ERROR_CHECK(rc);

        // Flash is needed to update settings/animations
        Flash::init();

        // I2C is needed for the accelerometer, but depends on the board info
        I2C::init();

        //--------------------
        // Initialize Hardware drivers
        //--------------------

        // Lights also depend on board info
        APA102::init();

        // Accel pins depend on the board info
        LIS2DE12::init();

        // Battery sense pin depends on board info
        Battery::init();

        // Magnet, so we know if ne need to go into DFU mode immediately
        Magnet::init(); 
        
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