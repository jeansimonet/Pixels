#include "bluetooth_stack.h"
#include "bluetooth_message_service.h"
#include "app_error.h"
#include "nrf_sdh.h"
#include "nrf_sdh_ble.h"
#include "nrf_sdh_soc.h"
#include "nrf_log.h"
#include "nrf_ble_qwr.h"
#include "ble_conn_state.h"
#include "ble_advdata.h"
#include "ble_advertising.h"
#include "ble_conn_params.h"
#include "nrf_ble_gatt.h"
#include "app_timer.h"
#include "peer_manager.h"
#include "peer_manager_handler.h"
#include "config/settings.h"
#include "drivers_nrf/power_manager.h"
#include "core/delegate_array.h"

using namespace Config;
using namespace DriversNRF;

namespace Bluetooth
{
namespace Stack
{
    #define DEVICE_NAME                     "Dice"
    #define MANUFACTURER_NAME               "Systemic Games, LLC"                   /**< Manufacturer. Will be passed to Device Information Service. */

    #define APP_ADV_INTERVAL                300                                     /**< The advertising interval (in units of 0.625 ms. This value corresponds to 187.5 ms). */
    #define APP_ADV_DURATION                BLE_GAP_ADV_TIMEOUT_GENERAL_UNLIMITED   /**< The advertising duration (180 seconds) in units of 10 milliseconds. */

    #define APP_BLE_OBSERVER_PRIO           3                                       /**< Application's BLE observer priority. You shouldn't need to modify this value. */
    #define APP_BLE_CONN_CFG_TAG            1                                       /**< A tag identifying the SoftDevice BLE configuration. */

    #define MIN_CONN_INTERVAL               MSEC_TO_UNITS(20, UNIT_1_25_MS)        /**< Minimum acceptable connection interval (0.02 seconds). */
    #define MAX_CONN_INTERVAL               MSEC_TO_UNITS(200, UNIT_1_25_MS)        /**< Maximum acceptable connection interval (0.2 second). */
    #define SLAVE_LATENCY                   1                                       /**< Slave latency. */
    #define CONN_SUP_TIMEOUT                MSEC_TO_UNITS(3000, UNIT_10_MS)         /**< Connection supervisory timeout (4 seconds). */

    #define FIRST_CONN_PARAMS_UPDATE_DELAY  APP_TIMER_TICKS(5000)                  /**< Time from initiating event (connect or start of notification) to first time sd_ble_gap_conn_param_update is called (5 seconds). */
    #define NEXT_CONN_PARAMS_UPDATE_DELAY   APP_TIMER_TICKS(30000)                   /**< Time between each call to sd_ble_gap_conn_param_update after the first call (30 seconds). */
    #define MAX_CONN_PARAMS_UPDATE_COUNT    3                                       /**< Number of attempts before giving up the connection parameter negotiation. */

    #define SEC_PARAM_BOND                  1                                       /**< No bonding. */
    #define SEC_PARAM_MITM                  0                                       /**< Man In The Middle protection not required. */
    #define SEC_PARAM_LESC                  0                                       /**< LE Secure Connections not enabled. */
    #define SEC_PARAM_KEYPRESS              0                                       /**< Keypress notifications not enabled. */
    #define SEC_PARAM_IO_CAPABILITIES       BLE_GAP_IO_CAPS_NONE                    /**< No I/O capabilities. */
    #define SEC_PARAM_OOB                   0                                       /**< Out Of Band data not available. */
    #define SEC_PARAM_MIN_KEY_SIZE          7                                       /**< Minimum encryption key size. */
    #define SEC_PARAM_MAX_KEY_SIZE          16                                      /**< Maximum encryption key size. */

    #define MAX_CLIENTS 2

    uint16_t m_conn_handle = BLE_CONN_HANDLE_INVALID;                        /**< Handle of the current connection. */
    NRF_BLE_QWR_DEF(m_qwr);                                                  /**< Context for the Queued Write module.*/
    NRF_BLE_GATT_DEF(m_gatt);                                                       /**< GATT module instance. */

    BLE_ADVERTISING_DEF(m_advertising); /**< Advertising module instance. */

    bool notificationPending = false;
    bool connected = false;
    bool currentlyAdvertising = false;

    char advertisingName[16];

    /**< Universally unique service identifiers. */
    ble_uuid_t m_adv_uuids[] = 
    {
        {BLE_UUID_DEVICE_INFORMATION_SERVICE, BLE_UUID_TYPE_BLE},
    };

    ble_uuid_t m_srv_uuids[] = 
    {
        {GENERIC_DATA_SERVICE_UUID_SHORT, BLE_UUID_TYPE_VENDOR_BEGIN},
    };

	DelegateArray<ConnectionEventMethod, MAX_CLIENTS> clients;

    /**@brief Function for handling BLE events.
     *
     * @param[in]   p_ble_evt   Bluetooth stack event.
     * @param[in]   p_context   Unused.
     */
    void ble_evt_handler(ble_evt_t const * p_ble_evt, void * p_context)
    {
        ret_code_t err_code = NRF_SUCCESS;

        switch (p_ble_evt->header.evt_id)
        {
            case BLE_GAP_EVT_DISCONNECTED:
                NRF_LOG_INFO("Disconnected, reason: 0x%02x", p_ble_evt->evt.gap_evt.params.disconnected.reason);
                connected = false;
                for (int i = 0; i < clients.Count(); ++i) {
                    clients[i].handler(clients[i].token, false);
                }
                break;

            case BLE_GAP_EVT_CONNECTED:
                NRF_LOG_INFO("Connected.");
                m_conn_handle = p_ble_evt->evt.gap_evt.conn_handle;
                err_code = nrf_ble_qwr_conn_handle_assign(&m_qwr, m_conn_handle);
                APP_ERROR_CHECK(err_code);
                connected = true;
                for (int i = 0; i < clients.Count(); ++i) {
                    clients[i].handler(clients[i].token, true);
                }
                break;

            case BLE_GAP_EVT_PHY_UPDATE_REQUEST:
            {
                NRF_LOG_DEBUG("PHY update request.");
                ble_gap_phys_t const phys =
                {
                    .tx_phys = BLE_GAP_PHY_AUTO,
                    .rx_phys = BLE_GAP_PHY_AUTO,
                };
                err_code = sd_ble_gap_phy_update(p_ble_evt->evt.gap_evt.conn_handle, &phys);
                APP_ERROR_CHECK(err_code);
            } break;

            case BLE_GATTC_EVT_TIMEOUT:
                // Disconnect on GATT Client timeout event.
                NRF_LOG_DEBUG("GATT Client Timeout.");
                err_code = sd_ble_gap_disconnect(p_ble_evt->evt.gattc_evt.conn_handle,
                                                BLE_HCI_REMOTE_USER_TERMINATED_CONNECTION);
                APP_ERROR_CHECK(err_code);
                break;

            case BLE_GATTS_EVT_TIMEOUT:
                // Disconnect on GATT Server timeout event.
                NRF_LOG_DEBUG("GATT Server Timeout.");
                err_code = sd_ble_gap_disconnect(p_ble_evt->evt.gatts_evt.conn_handle,
                                                BLE_HCI_REMOTE_USER_TERMINATED_CONNECTION);
                APP_ERROR_CHECK(err_code);
                break;

            case BLE_GAP_EVT_SEC_PARAMS_REQUEST:
                // Pairing not supported
                NRF_LOG_DEBUG("Pairing not supported!");
                err_code = sd_ble_gap_sec_params_reply(m_conn_handle, BLE_GAP_SEC_STATUS_PAIRING_NOT_SUPP, NULL, NULL);
                APP_ERROR_CHECK(err_code);
                break;

            case BLE_GATTS_EVT_SYS_ATTR_MISSING:
                // No system attributes have been stored.
                NRF_LOG_DEBUG("System Attributes Missing!");
                err_code = sd_ble_gatts_sys_attr_set(m_conn_handle, NULL, 0, 0);
                APP_ERROR_CHECK(err_code);
                break;

            case BLE_GATTS_EVT_HVN_TX_COMPLETE:
                // Last notification was cleared!
                NRF_LOG_DEBUG("Notification Complete!");
                notificationPending = false;
                break;

            case BLE_GATTS_EVT_HVC:
                // Last notification was cleared!
                NRF_LOG_INFO("Confirmation Received!");
                notificationPending = false;
                break;

            default:
                // No implementation needed.
                break;
        }

        PowerManager::feed();
    }

    // Function for handling advertising events.
    // This function will be called for advertising events which are passed to the application.
    void on_adv_evt(ble_adv_evt_t ble_adv_evt) {

        switch (ble_adv_evt)
        {
            case BLE_ADV_EVT_FAST:
                NRF_LOG_INFO("Fast advertising.");
                currentlyAdvertising = true;
                break;

            case BLE_ADV_EVT_IDLE:
                NRF_LOG_INFO("Advertising Idle.");
                currentlyAdvertising = false;
                //sleep_mode_enter();
                break;

            default:
                break;
        }
    }

    void nrf_qwr_error_handler(uint32_t nrf_error)
    {
        APP_ERROR_HANDLER(nrf_error);
    }

    /**@brief Function for handling the Connection Parameters Module.
     *
     * @details This function will be called for all events in the Connection Parameters Module which
     *          are passed to the application.
     *          @note All this function does is to disconnect. This could have been done by simply
     *                setting the disconnect_on_fail config parameter, but instead we use the event
     *                handler mechanism to demonstrate its use.
     *
     * @param[in] p_evt  Event received from the Connection Parameters Module.
     */
    void on_conn_params_evt(ble_conn_params_evt_t * p_evt)
    {
        ret_code_t err_code;

        if (p_evt->evt_type == BLE_CONN_PARAMS_EVT_FAILED)
        {
            err_code = sd_ble_gap_disconnect(m_conn_handle, BLE_HCI_CONN_INTERVAL_UNACCEPTABLE);
            APP_ERROR_CHECK(err_code);
        }
    }


    /**@brief Function for handling a Connection Parameters error.
     *
     * @param[in] nrf_error  Error code containing information about what went wrong.
     */
    void conn_params_error_handler(uint32_t nrf_error)
    {
        APP_ERROR_HANDLER(nrf_error);
    }

    /**@brief Function for handling Peer Manager events.
     *
     * @param[in] p_evt  Peer Manager event.
     */
    void pm_evt_handler(pm_evt_t const * p_evt)
    {
        pm_handler_on_pm_evt(p_evt);
        pm_handler_flash_clean(p_evt);
    }

    void advertising_config_get(ble_adv_modes_config_t * p_config) {
        memset(p_config, 0, sizeof(ble_adv_modes_config_t));

        p_config->ble_adv_fast_enabled  = true;
        p_config->ble_adv_fast_interval = APP_ADV_INTERVAL;
        p_config->ble_adv_fast_timeout  = APP_ADV_DURATION;
    }

    void init() {

        ret_code_t err_code;

        err_code = nrf_sdh_enable_request();
        APP_ERROR_CHECK(err_code);

        // Configure the BLE stack using the default settings.
        // Fetch the start address of the application RAM.
        uint32_t ram_start = 0;
        err_code = nrf_sdh_ble_default_cfg_set(APP_BLE_CONN_CFG_TAG, &ram_start);
        APP_ERROR_CHECK(err_code);

        // Enable BLE stack.
        err_code = nrf_sdh_ble_enable(&ram_start);
        APP_ERROR_CHECK(err_code);

        // Register a handler for BLE events.
        NRF_SDH_BLE_OBSERVER(m_ble_observer, APP_BLE_OBSERVER_PRIO, ble_evt_handler, NULL);

        // Generate our name
        advertisingName[0] = '\0';
        strcpy(advertisingName, "D_");
		uint32_t uniqueId = NRF_FICR->DEVICEID[0] ^ NRF_FICR->DEVICEID[1];
        for (int i = 0; i < 8; ++i) {
            advertisingName[2+i] = '0' + uniqueId % 10;
            uniqueId /= 10;
        }
        advertisingName[2+8] = '\0';

        //  GAP Params
        ble_gap_conn_params_t   gap_conn_params;
        ble_gap_conn_sec_mode_t sec_mode;

        BLE_GAP_CONN_SEC_MODE_SET_OPEN(&sec_mode);

        err_code = sd_ble_gap_device_name_set(&sec_mode,
                                            (const uint8_t *)advertisingName,
                                            strlen(advertisingName));
        APP_ERROR_CHECK(err_code);


        memset(&gap_conn_params, 0, sizeof(gap_conn_params));

        gap_conn_params.min_conn_interval = MIN_CONN_INTERVAL;
        gap_conn_params.max_conn_interval = MAX_CONN_INTERVAL;
        gap_conn_params.slave_latency     = SLAVE_LATENCY;
        gap_conn_params.conn_sup_timeout  = CONN_SUP_TIMEOUT;

        err_code = sd_ble_gap_ppcp_set(&gap_conn_params);
        APP_ERROR_CHECK(err_code);

        err_code = nrf_ble_gatt_init(&m_gatt, NULL);
        APP_ERROR_CHECK(err_code);

        NRF_LOG_INFO("Bluetooth Stack Initialized");
    }

    void initAdvertising() {
       ble_advertising_init_t init;
        memset(&init, 0, sizeof(init));

        init.advdata.name_type               = BLE_ADVDATA_FULL_NAME;
        init.advdata.include_appearance      = true;
        init.advdata.flags                   = BLE_GAP_ADV_FLAGS_LE_ONLY_GENERAL_DISC_MODE;
        init.advdata.uuids_complete.uuid_cnt = sizeof(m_adv_uuids) / sizeof(m_adv_uuids[0]);
        init.advdata.uuids_complete.p_uuids  = m_adv_uuids;

        init.srdata.uuids_complete.uuid_cnt = sizeof(m_srv_uuids) / sizeof(m_srv_uuids[0]);
        init.srdata.uuids_complete.p_uuids  = m_srv_uuids;


        advertising_config_get(&init.config);

        init.evt_handler = on_adv_evt;

        ret_code_t err_code = ble_advertising_init(&m_advertising, &init);
        APP_ERROR_CHECK(err_code);

        ble_advertising_conn_cfg_tag_set(&m_advertising, APP_BLE_CONN_CFG_TAG);

        nrf_ble_qwr_init_t qwr_init = {0};

        // Initialize Queued Write Module.
        qwr_init.error_handler = nrf_qwr_error_handler;

        err_code = nrf_ble_qwr_init(&m_qwr, &qwr_init);
        APP_ERROR_CHECK(err_code);

        ble_conn_params_init_t cp_init;

        memset(&cp_init, 0, sizeof(cp_init));

        cp_init.p_conn_params                  = NULL;
        cp_init.first_conn_params_update_delay = FIRST_CONN_PARAMS_UPDATE_DELAY;
        cp_init.next_conn_params_update_delay  = NEXT_CONN_PARAMS_UPDATE_DELAY;
        cp_init.max_conn_params_update_count   = MAX_CONN_PARAMS_UPDATE_COUNT;
        cp_init.start_on_notify_cccd_handle    = BLE_GATT_HANDLE_INVALID;
        cp_init.disconnect_on_fail             = false;
        cp_init.evt_handler                    = on_conn_params_evt;
        cp_init.error_handler                  = conn_params_error_handler;

        err_code = ble_conn_params_init(&cp_init);
        APP_ERROR_CHECK(err_code);

        // ble_gap_sec_params_t sec_param;

        // err_code = pm_init();
        // APP_ERROR_CHECK(err_code);

        // memset(&sec_param, 0, sizeof(ble_gap_sec_params_t));

        // // Security parameters to be used for all security procedures.
        // sec_param.bond           = SEC_PARAM_BOND;
        // sec_param.mitm           = SEC_PARAM_MITM;
        // sec_param.lesc           = SEC_PARAM_LESC;
        // sec_param.keypress       = SEC_PARAM_KEYPRESS;
        // sec_param.io_caps        = SEC_PARAM_IO_CAPABILITIES;
        // sec_param.oob            = SEC_PARAM_OOB;
        // sec_param.min_key_size   = SEC_PARAM_MIN_KEY_SIZE;
        // sec_param.max_key_size   = SEC_PARAM_MAX_KEY_SIZE;
        // sec_param.kdist_own.enc  = 1;
        // sec_param.kdist_own.id   = 1;
        // sec_param.kdist_peer.enc = 1;
        // sec_param.kdist_peer.id  = 1;

        // err_code = pm_sec_params_set(&sec_param);
        // APP_ERROR_CHECK(err_code);

        // err_code = pm_register(pm_evt_handler);
        // APP_ERROR_CHECK(err_code);
    }


    void disconnectLink(uint16_t conn_handle, void * p_context) {
        UNUSED_PARAMETER(p_context);

        ret_code_t err_code = sd_ble_gap_disconnect(conn_handle, BLE_HCI_REMOTE_USER_TERMINATED_CONNECTION);
        if (err_code != NRF_SUCCESS)
        {
            NRF_LOG_WARNING("Failed to disconnect connection. Connection handle: %d Error: %d", conn_handle, err_code);
        }
        else
        {
            NRF_LOG_DEBUG("Disconnected connection handle %d", conn_handle);
        }
    }

    void disconnect() {
        // Disconnect all other bonded devices that currently are connected.
        // This is required to receive a service changed indication
        // on bootup after a successful (or aborted) Device Firmware Update.
        uint32_t conn_count = ble_conn_state_for_each_connected(disconnectLink, NULL);
        NRF_LOG_INFO("Disconnected %d links.", conn_count);
    }

    bool isAdvertising() {
        return currentlyAdvertising;
    }

    void startAdvertising() {
        ret_code_t err_code = ble_advertising_start(&m_advertising, BLE_ADV_MODE_FAST);
        APP_ERROR_CHECK(err_code);
        NRF_LOG_INFO("Starting advertising as %s", advertisingName);
    }

    void disableAdvertisingOnDisconnect() {

        // Prevent device from advertising on disconnect.
        ble_adv_modes_config_t config;
        advertising_config_get(&config);
        config.ble_adv_on_disconnect_disabled = true;
        ble_advertising_modes_config_set(&m_advertising, &config);
    }

    bool canSend() {
        return !notificationPending;
    }

    SendResult send(uint16_t handle, const uint8_t* data, uint16_t len) {

        PowerManager::feed();
        if (connected) {
            if (!notificationPending) {
                ble_gatts_hvx_params_t hvx_params;
                memset(&hvx_params, 0, sizeof(hvx_params));

                hvx_params.handle = handle;
                hvx_params.p_data = data;
                hvx_params.p_len = &len;
                hvx_params.type = BLE_GATT_HVX_NOTIFICATION;
                notificationPending = true;
                ret_code_t err_code = sd_ble_gatts_hvx(m_conn_handle, &hvx_params);
                if (err_code == NRF_SUCCESS) {
                    // Message was sent!
                    return SendResult_Ok;
                } else {
                    // Some other error happened
                    NRF_LOG_ERROR("Could not send Notification for Message type %d of size %d, Error %s(0x%x)", data[0], len, NRF_LOG_ERROR_STRING_GET(err_code), err_code);

                    // Reset flag, since we won't be getting an event back
                    notificationPending = false;
                    return SendResult_Error;
                }
            } else {
                return SendResult_Busy;
            }
        } else {
            return SendResult_NotConnected;
        }
    }

    void slowAdvertising() {
        ble_advertising_start(&m_advertising, BLE_ADV_MODE_SLOW);
    }

    void stopAdvertising() {
        ble_advertising_start(&m_advertising, BLE_ADV_MODE_IDLE);
    }

    bool isConnected() {
        return connected;
    }

	void hook(ConnectionEventMethod method, void* param) {
		clients.Register(param, method);
	}

	void unHook(ConnectionEventMethod method) {
		clients.UnregisterWithHandler(method);
	}

	void unHookWithParam(void* param) {
		clients.UnregisterWithToken(param);
	}
}
}
