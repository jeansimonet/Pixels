#include "die.h"

#include "app_timer.h"
#include "app_error.h"

#include "drivers_nrf/watchdog.h"
#include "drivers_nrf/timers.h"
#include "drivers_nrf/scheduler.h"
#include "drivers_nrf/log.h"
#include "drivers_nrf/a2d.h"
#include "drivers_nrf/power_manager.h"
#include "drivers_nrf/i2c.h"
#include "drivers_nrf/flash.h"
#include "drivers_nrf/gpiote.h"
#include "drivers_nrf/dfu.h"

#include "config/board_config.h"
#include "config/settings.h"

#include "drivers_hw/apa102.h"
#include "drivers_hw/lis2de12.h"
#include "drivers_hw/battery.h"
#include "drivers_hw/magnet.h"

#include "bluetooth/bluetooth_stack.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bulk_data_transfer.h"
#include "bluetooth/telemetry.h"

#include "data_set/data_set.h"

#include "modules/led_color_tester.h"
#include "modules/accelerometer.h"
#include "modules/anim_controller.h"
#include "modules/animation_preview.h"
#include "modules/battery_controller.h"
#include "modules/behavior_controller.h"
#include "modules/hardware_test.h"
#include "modules/rssi_controller.h"

#include "nrf_sdh.h"
#include "nrf_sdh_ble.h"
#include "nrf_fstorage_sd.h"

#include "nrf_drv_clock.h"

using namespace DriversNRF;
using namespace Config;
using namespace DriversHW;
using namespace Bluetooth;
using namespace Animations;
using namespace Modules;

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
    void init() {
        //--------------------
        // Initialize NRF drivers
        //--------------------

        // Very first thing we want to init is the watchdog so we don't brick
        // later on if something bad happens.
        Watchdog::init();

        // Then the log system
        Log::init();

        // Then the timers
        Scheduler::init();

        // Then the timers
        Timers::init();

        // GPIO Interrupts
        GPIOTE::init();

        // Power manager handles going to sleep and resetting the board
        //PowerManager::init();
        
        // Analog to digital converter next, so we can
        // identify the board we're dealing with
        A2D::init();
        
        // Enable bluetooth
        Stack::init();

        // Add generic data service
        MessageService::init();

        // Initialize the DFU service
        DFU::init();

        // Now that the message service added its uuid to the softdevice, initialize the advertising
        Stack::initAdvertising();

        // Flash is needed to update settings/animations
        Flash::init();

        //--------------------
        // Fetch board configuration now, so we know how to initialize
        // the rest of the hardware (pins, led count, etc...)
        //--------------------
        // This will use the A2D converter to check the identifying resistor
        // on the board and determine what kind of die this is.
        BoardManager::init();

        // Magnet, so we know if ne need to go into quiet mode
        Magnet::init(); 
        
        // Now that we know which board we are, initialize the battery monitoring A2D
        A2D::initBoardPins();

        // The we read user settings from flash, or set some defaults if none are found
        SettingsManager::init([] (bool result) {

            // Now that the settings are set, update custom advertising data
            Stack::initCustomAdvertisingData();

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

            //--------------------
            // Initialize Modules
            //--------------------

            // Animation set needs flash and board info
            DataSet::init([] (bool result) {

                // Useful for development
                LEDColorTester::init();

                // Accelerometer
                Accelerometer::init();

                // Telemetry depends on accelerometer
                Telemetry::init();

                // Animation controller relies on animation set
                AnimController::init();

                // Battery controller relies on the battery driver
                BatteryController::init();

                // Behavior Controller relies on all the modules
                BehaviorController::init();

                // Animation preview depends on bluetooth
                AnimationPreview::init();

                // Rssi controller requires the bluetooth stack
                RssiController::init();

                //HardwareTest::init();

                // Start advertising!
                Stack::startAdvertising();

            #if defined(DEBUG_FIRMWARE)
                initDebugLogic();
            #else
                // Initialize main logic manager
                initMainLogic();

                // Entering the main loop! Play Hello! anim
                BehaviorController::onDiceInitialized();
            #endif
            });
        });
    }
}
