#include "power_manager.h"
#include "nrf_pwr_mgmt.h"
#include "nrf_power.h"
#include "nrf_bootloader_info.h"
#include "app_error.h"
#include "app_error_weak.h"
#include "log.h"
#include "core/delegate_array.h"

#define MAX_POWER_CLIENTS 4
#define GPREGRET_ID 0
#define WATCHDOG_TRIGGERED_RESET    0x0001 // This bit gets set the first time the watchdog resets the mcu
#define CLEAR_SETTINGS_AND_DATASET  0x0002 // If above bit was still set when watchdog triggered reset,
                                           // then this bit gets set to tell boot up sequence to reprogram flash.

namespace DriversNRF
{
namespace PowerManager
{
	DelegateArray<PowerManagerClientMethod, MAX_POWER_CLIENTS> clients;

    void init() {
        ret_code_t err_code;
        err_code = nrf_pwr_mgmt_init();
        APP_ERROR_CHECK(err_code);
        NRF_LOG_INFO("Power Management Initialized");
    }

    void hook(PowerManagerClientMethod method, void* param) {
		clients.Register(param, method);
    }

    void unHook(PowerManagerClientMethod client) {
        clients.UnregisterWithHandler(client);
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
        Log::process();

        // Notify clients
        for (int i = 0; i < clients.Count(); ++i) {
            clients[i].handler(clients[i].token, event);
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

    void reset() {
        nrf_pwr_mgmt_shutdown(NRF_PWR_MGMT_SHUTDOWN_RESET);
    }

    void setWatchdogTriggeredReset() {
        sd_power_gpregret_set(GPREGRET_ID, WATCHDOG_TRIGGERED_RESET);
    }

    void clearWatchdogTriggeredReset() {
        sd_power_gpregret_clr(GPREGRET_ID, WATCHDOG_TRIGGERED_RESET);
    }

    bool getWatchdogTriggeredReset() {
        // Read gpregret register
        uint32_t reg = 0;
        ret_code_t err_code = sd_power_gpregret_get(GPREGRET_ID, &reg);
        APP_ERROR_CHECK(err_code);
        return (reg & WATCHDOG_TRIGGERED_RESET) != 0;
    }

    void setClearSettingsAndDataSet() {
        sd_power_gpregret_set(GPREGRET_ID, CLEAR_SETTINGS_AND_DATASET);
    }

    void clearClearSettingsAndDataSet() {
        sd_power_gpregret_clr(GPREGRET_ID, CLEAR_SETTINGS_AND_DATASET);
    }

    bool getClearSettingsAndDataSet() {
        // Read gpregret register
        uint32_t reg = 0;
        ret_code_t err_code = sd_power_gpregret_get(GPREGRET_ID, &reg);
        APP_ERROR_CHECK(err_code);
        return (reg & CLEAR_SETTINGS_AND_DATASET) != 0;
    }

}
}

