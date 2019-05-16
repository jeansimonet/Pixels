#include "watchdog.h"
#include "nrf_drv_wdt.h"

static nrf_drv_wdt_channel_id m_channel_id;

namespace DriversNRF
{
namespace Watchdog
{
    // WDT events handler.
    static void wdt_event_handler(void)
    {
        // Nothing to do!
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

    void feed()
    {
        nrf_drv_wdt_channel_feed(m_channel_id);
    }

    void selfTest()
    {

    }
}
}
