#include "telemetry.h"
#include "bluetooth_stack.h"
#include "app_error.h"
#include "nrf_log.h"
#include "ble.h"
#include "ble_srv_common.h"
#include "nrf_sdh_ble.h"
#include "modules/accelerometer.h"

using namespace Modules;
using namespace Bluetooth;

namespace Bluetooth
{
namespace Telemetry
{
    #define TELEMETRY_SERVICE_UUID {{0x9E, 0xCA, 0xDC, 0x24, 0x0E, 0xE5, 0xA9, 0xE0, 0x93, 0xF3, 0xA3, 0xB5, 0x00, 0x00, 0x40, 0x6E}}
    #define TELEMETRY_SERVICE_UUID_SHORT 0x1000
    #define TELEMETRY_TX_CHARACTERISTIC 0x1001
    #define TELEMETRY_RX_CHARACTERISTIC 0x1002

    void BLEObserver(ble_evt_t const * p_ble_evt, void * p_context);

    NRF_SDH_BLE_OBSERVER(TelemetryServiceObserver, 3, BLEObserver, nullptr);

    uint16_t service_handle;
    ble_gatts_char_handles_t tx_handles, rx_handles;
    bool subscribed = false;

	void onAccDataReceived(void* param, const Accelerometer::AccelFrame& accelFrame);

    void init() {

        ret_code_t            err_code;
        ble_uuid_t            ble_uuid;
        ble_uuid128_t         nus_base_uuid = TELEMETRY_SERVICE_UUID;
        ble_add_char_params_t add_char_params;

        // Add a custom base UUID.
        uint8_t uuid_type;
        err_code = sd_ble_uuid_vs_add(&nus_base_uuid, &uuid_type);
        APP_ERROR_CHECK(err_code);

        ble_uuid.type = uuid_type;
        ble_uuid.uuid = TELEMETRY_SERVICE_UUID_SHORT;

        // Add the service.
        err_code = sd_ble_gatts_service_add(BLE_GATTS_SRVC_TYPE_PRIMARY,
                                            &ble_uuid,
                                            &service_handle);
        APP_ERROR_CHECK(err_code);

        // Add the RX Characteristic.
        memset(&add_char_params, 0, sizeof(add_char_params));
        add_char_params.uuid                     = TELEMETRY_RX_CHARACTERISTIC;
        add_char_params.uuid_type                = uuid_type;
        add_char_params.max_len                  = NRF_SDH_BLE_GATT_MAX_MTU_SIZE;
        add_char_params.init_len                 = sizeof(uint8_t);
        add_char_params.is_var_len               = true;
        add_char_params.char_props.write         = 1;
        add_char_params.char_props.write_wo_resp = 1;

        add_char_params.read_access  = SEC_OPEN;
        add_char_params.write_access = SEC_OPEN;

        err_code = characteristic_add(service_handle, &add_char_params, &rx_handles);
        APP_ERROR_CHECK(err_code);

        // Add the TX Characteristic.
        memset(&add_char_params, 0, sizeof(add_char_params));
        add_char_params.uuid              = TELEMETRY_TX_CHARACTERISTIC;
        add_char_params.uuid_type         = uuid_type;
        add_char_params.max_len           = NRF_SDH_BLE_GATT_MAX_MTU_SIZE;
        add_char_params.init_len          = sizeof(uint8_t);
        add_char_params.is_var_len        = true;
        add_char_params.char_props.notify = 1;

        add_char_params.read_access       = SEC_OPEN;
        add_char_params.write_access      = SEC_OPEN;
        add_char_params.cccd_write_access = SEC_OPEN;

        err_code = characteristic_add(service_handle, &add_char_params, &tx_handles);
        APP_ERROR_CHECK(err_code);

   		NRF_LOG_INFO("Telemetry initialized");
    }

    void BLEObserver(ble_evt_t const * p_ble_evt, void * p_context)
    {
        switch (p_ble_evt->header.evt_id)
        {
            case BLE_GATTS_EVT_WRITE:
                {
                    ble_gatts_evt_write_t const * p_evt_write = &p_ble_evt->evt.gatts_evt.params.write;
                    if (p_evt_write->handle == rx_handles.value_handle)
                    {
                        if (p_evt_write->len == 1) {
                            if (p_evt_write->data[0] == 1) {
                                // Register with the accelerometer, to receive acc data
                                Accelerometer::hook(onAccDataReceived, nullptr);
                            } else if (p_evt_write->data[0] == 0) {
                                // Unregister with the accelerometer
                                Accelerometer::unHook(onAccDataReceived);
                            }
                        }
                    }
                    // Else its not meant for us
                }
                break;

            case BLE_GATTS_EVT_HVN_TX_COMPLETE:
                break;

            default:
                // No implementation needed.
                break;
        }
    }

	void onAccDataReceived(void* param, const Accelerometer::AccelFrame& accelFrame) {
        NRF_LOG_DEBUG("Telemetry Service Sending frame");
        Stack::send(tx_handles.value_handle, (const uint8_t*)&accelFrame, sizeof(accelFrame));
    }
}
}
