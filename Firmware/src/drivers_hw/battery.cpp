#include "battery.h"
#include "nrf_log.h"
#include "board_config.h"
#include "nrf_gpio.h"
#include "nrf_delay.h"
#include "../drivers_nrf/a2d.h"
#include "../drivers_nrf/log.h"
#include "../drivers_nrf/timers.h"
#include "../drivers_nrf/power_manager.h"

using namespace DriversNRF;
using namespace Config;

namespace DriversHW
{
namespace Battery
{
    const float vBatMult = 1.4f; // Voltage divider 10M over 4M

    void init() {
        // Set charger and fault pins as input

        // Drive the status pin down for a moment
        uint32_t statePin = BoardManager::getBoard()->chargingStatePin;

        // Status pin needs a pull-up, and is pulled low when charging
        nrf_gpio_cfg_input(statePin, NRF_GPIO_PIN_PULLUP);

        // Fault pin needs a pull-up as well, and is pulled low when vcc fails
        // Unlikely to be useful, as I'm guessing the cpu will stop working...
        nrf_gpio_cfg_input(BoardManager::getBoard()->vledFaultPin, NRF_GPIO_PIN_PULLUP);

        // Read battery level and convert
        float vbattery = checkVBat();
        int charging = checkCharging() ? 1 : 0;
        int fault = checkVCCFault() ? 1 : 0;

        NRF_LOG_INFO("Battery initialized, Charging=%d, Fault=%d, Battery Voltage=" NRF_LOG_FLOAT_MARKER, charging, fault, NRF_LOG_FLOAT(vbattery));

        #if DICE_SELFTEST && BATTERY_SELFTEST
        selfTest();
        #endif
    }

    float checkVBat() {
        return A2D::readVBat() * vBatMult;
    }

    bool checkCharging() {
        // Status pin needs a pull-up, and is pulled low when charging
        return nrf_gpio_pin_read(BoardManager::getBoard()->chargingStatePin) == 0;
    }

    bool checkVCCFault() {
        // Fault pin needs a pull-up as well, and is pulled low when vcc fails
        // Unlikely to be useful, as I'm guessing the cpu will stop working...
        return nrf_gpio_pin_read(BoardManager::getBoard()->vledFaultPin) == 0;
    }

    #if DICE_SELFTEST && BATTERY_SELFTEST
    APP_TIMER_DEF(readBatTimer);
    void printBatStats(void* context) {
        float vbattery = checkVBat();
        int charging = checkCharging() ? 1 : 0;
        int fault = checkVCCFault() ? 1 : 0;
        NRF_LOG_INFO("Charging=%d, Fault=%d, Voltage=" NRF_LOG_FLOAT_MARKER, charging, fault, NRF_LOG_FLOAT(vbattery));
    }

    void selfTest() {
        Timers::createTimer(&readBatTimer, APP_TIMER_MODE_REPEATED, printBatStats);
        NRF_LOG_INFO("Reading battery status repeatedly, press any key to abort");
        Log::process();

        Timers::startTimer(readBatTimer, 200, nullptr);
        while (!Log::hasKey()) {
            Log::process();
            PowerManager::feed();
            PowerManager::update();
        }
		Log::getKey();
        NRF_LOG_INFO("Finished reading battery status!");
        Timers::stopTimer(readBatTimer);
    }
    #endif
}
}