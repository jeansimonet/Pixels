#include "magnet.h"
#include "../config/board_config.h"
#include "../drivers_nrf/log.h"
#include "../drivers_nrf/power_manager.h"
#include "../drivers_nrf/timers.h"
#include "../drivers_nrf/gpiote.h"
#include "nrf_gpio.h"

using namespace Config;
using namespace DriversNRF;

namespace DriversHW
{
namespace Magnet
{
    void init() {
        // Fetch config, check magnet state
        //nrf_gpio_cfg_input(BoardManager::getBoard()->magnetPin, NRF_GPIO_PIN_NOPULL);

        #if DICE_SELFTEST && MAGNET_SELFTEST
        selfTest();
        #endif
    }

    bool checkMagnet() {
        // Magnet pin needs a pull-up, and is pulled low when North pole is present
        return nrf_gpio_pin_read(BoardManager::getBoard()->magnetPin) == 0;
    }

    #if DICE_SELFTEST && MAGNET_SELFTEST
    APP_TIMER_DEF(readMagnetTimer);
    void readMagnet(void* context) {
        int magnet = checkMagnet();
        NRF_LOG_INFO("Magnet: %d", magnet);
    }

    void selfTest() {
        Timers::createTimer(&readMagnetTimer, APP_TIMER_MODE_REPEATED, readMagnet);
        NRF_LOG_INFO("Reading Magnet, press any key to abort");
        Log::process();

        Timers::startTimer(readMagnetTimer, 200, nullptr);
        while (!Log::hasKey()) {
            Log::process();
            PowerManager::feed();
            PowerManager::update();
        }
		Log::getKey();
        NRF_LOG_INFO("Stopping from reading magnet!");
        Timers::stopTimer(readMagnetTimer);
        Log::process();
    }
    #endif
}
}