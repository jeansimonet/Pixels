#include "telemetry.h"
#include "bluetooth_message_service.h"
#include "bluetooth_messages.h"
#include "bluetooth_stack.h"
#include "app_error.h"
#include "nrf_log.h"
#include "ble.h"
#include "ble_srv_common.h"
#include "nrf_sdh_ble.h"
#include "modules/accelerometer.h"
#include "utils/utils.h"

using namespace Modules;
using namespace Bluetooth;

namespace Bluetooth
{
namespace Telemetry
{
    MessageAcc teleMessage;
    bool telemetryActive;

    void onAccDataReceived(void* param, const Accelerometer::AccelFrame& accelFrame);
    void onRequestTelemetryMessage(void* token, const Message* message);

    void init() {
        // Register for messages to send telemetry data over!
        MessageService::RegisterMessageHandler(Message::MessageType_RequestTelemetry, nullptr, onRequestTelemetryMessage);

   		NRF_LOG_INFO("Telemetry initialized");
    }

	void onAccDataReceived(void* param, const Accelerometer::AccelFrame& frame) {
        if (Stack::canSend()) {
            teleMessage.data = frame;
            // Send the message
            if (!MessageService::SendMessage(&teleMessage)) {
                NRF_LOG_DEBUG("Couldn't send message yet");
            }
        }
    }

    void onRequestTelemetryMessage(void* token, const Message* message) {
        auto reqTelem = static_cast<const MessageRequestTelemetry*>(message);
        if (reqTelem->telemetry != 0) {
            if (!telemetryActive) {
                NRF_LOG_INFO("Starting Telemetry");
                start();
            }
        } else {
            if (telemetryActive) {
                NRF_LOG_INFO("Stopping Telemetry");
                stop();
            }
        }
    }

    void start() {
        // Init our reuseable telemetry message
        memset(&teleMessage, 0, sizeof(teleMessage));
        teleMessage.type = Message::MessageType_Telemetry;

        // Ask the acceleration controller to be notified when
        // new acceleration data comes in!
        Accelerometer::hookFrameData(onAccDataReceived, nullptr);
        telemetryActive = true;
    }

    void stop() {
        // Stop being notified!
        Accelerometer::unHookFrameData(onAccDataReceived);
        telemetryActive = false;
    }
}
}
