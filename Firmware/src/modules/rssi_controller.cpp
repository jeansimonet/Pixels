#include "rssi_controller.h"
#include "bluetooth/bluetooth_stack.h"
#include "bluetooth/bluetooth_message_service.h"
#include "nrf_log.h"

using namespace Bluetooth;

namespace Modules
{
namespace RssiController
{
    void GetRssi(void* context, const Message* msg);
    void OnRssi(void* token, int rssi);
 
    void init() {
        Bluetooth::MessageService::RegisterMessageHandler(Bluetooth::Message::MessageType_RequestRssi, nullptr, GetRssi);
        NRF_LOG_INFO("Rssi controller initialized");
    }

    void GetRssi(void* context, const Message* msg) {
        NRF_LOG_INFO("Rssi requested");
        Stack::hookRssi(OnRssi, nullptr);
    }

    void OnRssi(void* token, int rssi) {
        NRF_LOG_INFO("Returning Rssi: %d", rssi);
        Stack::unHookRssi(OnRssi);
        MessageRssi retMsg;
        retMsg.rssi = rssi;
        MessageService::SendMessage(&retMsg);
    }
}
}