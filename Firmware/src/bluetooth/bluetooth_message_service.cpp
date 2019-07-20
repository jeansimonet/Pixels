#include "bluetooth_message_service.h"
#include "bluetooth_stack.h"
#include "app_error.h"
#include "nrf_log.h"
#include "ble.h"
#include "ble_srv_common.h"
#include "nrf_sdh_ble.h"
#include "drivers_nrf/scheduler.h"

using namespace DriversNRF;

namespace Bluetooth
{
namespace MessageService
{
    #define GENERIC_DATA_SERVICE_UUID {{0x9E, 0xCA, 0xDC, 0x24, 0x0E, 0xE5, 0xA9, 0xE0, 0x93, 0xF3, 0xA3, 0xB5, 0x00, 0x00, 0x40, 0x6E}}
    #define GENERIC_DATA_SERVICE_UUID_SHORT 0x0001
    #define GENERIC_DATA_TX_CHARACTERISTIC 0x0001
    #define GENERIC_DATA_RX_CHARACTERISTIC 0x0002

    void BLEObserver(ble_evt_t const * p_ble_evt, void * p_context);

    NRF_SDH_BLE_OBSERVER(GenericServiceObserver, 3, BLEObserver, nullptr);

    uint16_t service_handle;
    ble_gatts_char_handles_t rx_handles;
    ble_gatts_char_handles_t tx_handles;
    uint8_t sendMessageBuffer[32];

	struct HandlerAndToken
	{
		MessageHandler handler;
		void* token;
	};
	HandlerAndToken messageHandlers[Message::MessageType_Count];

    void init() {
        // Clear message handle array
    	memset(messageHandlers, 0, sizeof(HandlerAndToken) * Message::MessageType_Count);

        ret_code_t            err_code;
        ble_uuid_t            ble_uuid;
        ble_uuid128_t         nus_base_uuid = GENERIC_DATA_SERVICE_UUID;
        ble_add_char_params_t add_char_params;

        // Add a custom base UUID.
        uint8_t uuid_type;
        err_code = sd_ble_uuid_vs_add(&nus_base_uuid, &uuid_type);
        APP_ERROR_CHECK(err_code);

        ble_uuid.type = uuid_type;
        ble_uuid.uuid = GENERIC_DATA_SERVICE_UUID_SHORT;

        // Add the service.
        err_code = sd_ble_gatts_service_add(BLE_GATTS_SRVC_TYPE_PRIMARY,
                                            &ble_uuid,
                                            &service_handle);
        APP_ERROR_CHECK(err_code);

        // Add the RX Characteristic.
        memset(&add_char_params, 0, sizeof(add_char_params));
        add_char_params.uuid                     = GENERIC_DATA_RX_CHARACTERISTIC;
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
        add_char_params.uuid              = GENERIC_DATA_TX_CHARACTERISTIC;
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
    }

    void OnMessageReceived(const uint8_t* data, uint16_t len);

    void BLEObserver(ble_evt_t const * p_ble_evt, void * p_context)
    {
        switch (p_ble_evt->header.evt_id)
        {
            case BLE_GATTS_EVT_WRITE:
                {
                    ble_gatts_evt_write_t const * p_evt_write = &p_ble_evt->evt.gatts_evt.params.write;
                    if (p_evt_write->handle == rx_handles.value_handle)
                    {
                        NRF_LOG_DEBUG("Generic Service Message Received: %d bytes", p_evt_write->len);
                        NRF_LOG_HEXDUMP_DEBUG(p_evt_write->data, p_evt_write->len);
                        OnMessageReceived(p_evt_write->data, p_evt_write->len);
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

    bool send(const uint8_t* data, uint16_t size) {
        NRF_LOG_DEBUG("Generic Service Message Sending: %d bytes", size);
        NRF_LOG_HEXDUMP_DEBUG(data, size);
        return Stack::send(tx_handles.value_handle, data, size);
    }

    bool SendMessage(Message::MessageType msgType) {
        Message msg(msgType);
        return SendMessage(&msg, sizeof(Message));
    }

    bool SendMessage(const Message* msg, int msgSize) {
        memcpy(sendMessageBuffer, msg, msgSize);
        return send(sendMessageBuffer, msgSize);
    }

    void RegisterMessageHandler(Message::MessageType msgType, void* token, MessageHandler handler) {
        if (messageHandlers[msgType].handler != nullptr)
        {
            NRF_LOG_WARNING("Handler for message %d already set.", msgType);
        }
        else
        {
            messageHandlers[msgType].handler = handler;
            messageHandlers[msgType].token = token;
            NRF_LOG_DEBUG("Setting message handler for %d to %08x", msgType, handler);
        }
    }

    void UnregisterMessageHandler(Message::MessageType msgType) {
        messageHandlers[msgType].handler = nullptr;
        messageHandlers[msgType].token = nullptr;
    }

    void MessageSchedulerHandler(void* data, uint16_t size) {
        // Cast the data
        auto msg = reinterpret_cast<const Message*>(data);
        // #if defined(_CONSOLE)
        // debugPrint("Received ");
        // debugPrint(DieMessage::GetMessageTypeString(msg->type));
        // debugPrint("(");
        // debugPrint(msg->type);
        // debugPrintln(")");
        // #endif
        auto handler = messageHandlers[(int)msg->type];
        if (handler.handler != nullptr)
        {
            NRF_LOG_DEBUG("Calling message handler %08x", handler.handler);
            handler.handler(handler.token, msg);
        }
    }

    void OnMessageReceived(const uint8_t* data, uint16_t len) {
        if (len >= sizeof(Message))
        {
            auto msg = reinterpret_cast<const Message*>(data);
            if (msg->type >= Message::MessageType_WhoAreYou && msg->type < Message::MessageType_Count) {
    		    Scheduler::push(data, len, MessageSchedulerHandler);
            } else {
                NRF_LOG_ERROR("Bad message type %d", msg->type);
            }
        } else {
            NRF_LOG_ERROR("Bad Message Length %d", len);
        }
    }

    void NotifyUser(const char* text, NotifyUserCallback callback) {
		MessageNotifyUser notifyMsg;
		strcpy(notifyMsg.text, text);
		MessageService::RegisterMessageHandler(Message::MessageType_NotifyUserAck, (void*)callback, [] (void* ctx, const Message* ackMsg)
		{
			MessageService::UnregisterMessageHandler(Message::MessageType_NotifyUserAck);
            ((NotifyUserCallback)ctx)();
        });
		MessageService::SendMessage(&notifyMsg);
    }

}
}
