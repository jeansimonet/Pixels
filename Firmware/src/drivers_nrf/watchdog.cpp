#include "watchdog.h"
#include "nrf_drv_wdt.h"
#include "power_manager.h"
#include "timers.h"

static nrf_drv_wdt_channel_id m_channel_id;

#define RESET_FLAG_TIME_MS 30000

namespace DriversNRF
{
namespace Watchdog
{
    // WDT events handler.
    static void wdt_event_handler(void)
    {
        if (PowerManager::getWatchdogTriggeredReset()) {
            PowerManager::setClearSettingsAndDataSet();
        } else {
            PowerManager::setWatchdogTriggeredReset();
        }
    }

    // Initialize the watchdog
    void init()
    {
        //Configure WDT.
        nrf_drv_wdt_config_t config = NRF_DRV_WDT_DEAFULT_CONFIG;
        ret_code_t err_code = nrf_drv_wdt_init(&config, wdt_event_handler);
        APP_ERROR_CHECK(err_code);

        err_code = nrf_drv_wdt_channel_alloc(&m_channel_id);
        APP_ERROR_CHECK(err_code);
        nrf_drv_wdt_enable();
    }

    APP_TIMER_DEF(clearResetFlagTimer);
    void clearResetFlag(void* context) {
        NRF_LOG_INFO("App seems stable, clearing watchdog flag");
        Timers::stopTimer(clearResetFlagTimer);
        PowerManager::clearWatchdogTriggeredReset();
    }

    void initClearResetFlagTimer()
    {
        if (PowerManager::getWatchdogTriggeredReset()) {
            Timers::createTimer(&clearResetFlagTimer, APP_TIMER_MODE_SINGLE_SHOT, clearResetFlag);
            NRF_LOG_INFO("Watchdog reset the device, setting timer to check app stability");
            Timers::startTimer(clearResetFlagTimer, RESET_FLAG_TIME_MS, nullptr);
        }
    }

    void feed()
    {
        nrf_drv_wdt_channel_feed(m_channel_id);
    }

    void selfTest()
    {

    }
}
}
