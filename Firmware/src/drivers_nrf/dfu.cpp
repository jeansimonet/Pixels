#include "dfu.h"
#include "ble_dfu.h"
#include "power_manager.h"
#include "nrf_sdh.h"
#include "app_error.h"
#include "app_error_weak.h"
#include "nrf_log.h"

#include "bluetooth/bluetooth_stack.h"

namespace DriversNRF
{
namespace DFU
{
    void buttonless_dfu_sdh_state_observer(nrf_sdh_state_evt_t state, void * p_context)
    {
        if (state == NRF_SDH_EVT_STATE_DISABLED)
        {
            DriversNRF::PowerManager::goToSystemOff();
        }
    }

    /**@brief Function for handling dfu events from the Buttonless Secure DFU service
     *
     * @param[in]   event   Event from the Buttonless Secure DFU service.
     */
    void ble_dfu_evt_handler(ble_dfu_buttonless_evt_type_t event)
    {
        switch (event)
        {
            case BLE_DFU_EVT_BOOTLOADER_ENTER_PREPARE:
            {
                NRF_LOG_INFO("Device is preparing to enter bootloader mode.");

                // Prevent device from advertising on disconnect.
                Bluetooth::Stack::disableAdvertisingOnDisconnect();

                // Disconnect all other bonded devices that currently are connected.
                // This is required to receive a service changed indication
                // on bootup after a successful (or aborted) Device Firmware Update.
                Bluetooth::Stack::disconnect();
                break;
            }

            case BLE_DFU_EVT_BOOTLOADER_ENTER:
                // YOUR_JOB: Write app-specific unwritten data to FLASH, control finalization of this
                //           by delaying reset by reporting false in app_shutdown_handler
                NRF_LOG_INFO("Device will enter bootloader mode.");
                break;

            case BLE_DFU_EVT_BOOTLOADER_ENTER_FAILED:
                NRF_LOG_ERROR("Request to enter bootloader mode failed asynchroneously.");
                // YOUR_JOB: Take corrective measures to resolve the issue
                //           like calling APP_ERROR_CHECK to reset the device.
                break;

            case BLE_DFU_EVT_RESPONSE_SEND_ERROR:
                NRF_LOG_ERROR("Request to send a response to client failed.");
                // YOUR_JOB: Take corrective measures to resolve the issue
                //           like calling APP_ERROR_CHECK to reset the device.
                APP_ERROR_CHECK(false);
                break;

            default:
                NRF_LOG_ERROR("Unknown event from ble_dfu_buttonless.");
                break;
        }
    }


    /* nrf_sdh state observer. */
    NRF_SDH_STATE_OBSERVER(m_buttonless_dfu_state_obs, 0) =
    {
        .handler = buttonless_dfu_sdh_state_observer,
    };

    void init() {

        #if !defined(DEBUG)
        // Initialize the async SVCI interface to bootloader before any interrupts are enabled.
        ret_code_t err_code = ble_dfu_buttonless_async_svci_init();
        APP_ERROR_CHECK(err_code);

        ble_dfu_buttonless_init_t dfus_init = {0};

        dfus_init.evt_handler = ble_dfu_evt_handler;

        err_code = ble_dfu_buttonless_init(&dfus_init);
        APP_ERROR_CHECK(err_code);
        #endif
    }
}
}
