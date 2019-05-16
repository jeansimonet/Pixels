#include "power_manager.h"
#include "nrf_pwr_mgmt.h"
#include "nrf_power.h"
#include "nrf_bootloader_info.h"
#include "app_error.h"
#include "nrf_log.h"

namespace DriversNRF
{
namespace PowerManager
{
    void init() {
        ret_code_t err_code;
        err_code = nrf_pwr_mgmt_init();
        APP_ERROR_CHECK(err_code);
        NRF_LOG_INFO("Power Management Initialized");
    }

    bool powerEventHandler(nrf_pwr_mgmt_evt_t event)
    {
        switch (event)
        {
            case NRF_PWR_MGMT_EVT_PREPARE_SYSOFF:
                NRF_LOG_INFO("NRF_PWR_MGMT_EVT_PREPARE_SYSOFF");
                break;

            case NRF_PWR_MGMT_EVT_PREPARE_WAKEUP:
                NRF_LOG_INFO("NRF_PWR_MGMT_EVT_PREPARE_WAKEUP");
                break;

            case NRF_PWR_MGMT_EVT_PREPARE_DFU:
                NRF_LOG_INFO("NRF_PWR_MGMT_EVT_PREPARE_DFU");
                break;

            case NRF_PWR_MGMT_EVT_PREPARE_RESET:
                NRF_LOG_INFO("NRF_PWR_MGMT_EVT_PREPARE_RESET");
                break;
        }
        return true;
    }

    /**@brief Register application shutdown handler with priority 0. */
    NRF_PWR_MGMT_HANDLER_REGISTER(powerEventHandler, 0);

    void feed() {
        nrf_pwr_mgmt_feed();
    }

    void update() {
        nrf_pwr_mgmt_run();
    }

    void goToSystemOff() {
        // Inform bootloader to skip CRC on next boot.
        nrf_power_gpregret2_set(BOOTLOADER_DFU_SKIP_CRC);

        // Go to system off.
        nrf_pwr_mgmt_shutdown(NRF_PWR_MGMT_SHUTDOWN_GOTO_SYSOFF);
    }
}
}

