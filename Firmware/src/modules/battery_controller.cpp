#include "battery_controller.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "drivers_hw/battery.h"
#include "nrf_log.h"

using namespace DriversHW;
using namespace Bluetooth;

namespace Modules
{
namespace BatteryController
{
    void GetBatteryLevel(void* context, const Message* msg);

    void init() {
        MessageService::RegisterMessageHandler(Message::MessageType_RequestBatteryLevel, nullptr, GetBatteryLevel);
        NRF_LOG_INFO("Battery controller initialized");
    }

    void GetBatteryLevel(void* context, const Message* msg) {
        // Fetch battery level
        float level = Battery::checkVBat();
        MessageBatteryLevel lvl;
        lvl.level = level;
        MessageService::SendMessage(&lvl);
    }
}
}