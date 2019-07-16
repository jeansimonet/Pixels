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
    Accelerometer::AccelFrame lastAccelFrame;
    bool lastAccelWasSent;
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
            if (lastAccelWasSent) {
                // Store new data in frame 0
                teleMessage.data[0].x = frame.X;
                teleMessage.data[0].y = frame.Y;
                teleMessage.data[0].z = frame.Z;
                teleMessage.data[0].deltaTime = frame.Time - lastAccelFrame.Time;
                lastAccelWasSent = false;
            } else {
                // Store new data in frame 1
                teleMessage.data[1].x = frame.X;
                teleMessage.data[1].y = frame.Y;
                teleMessage.data[1].z = frame.Z;
                teleMessage.data[1].deltaTime = frame.Time - lastAccelFrame.Time;

                // Send the message
                if (!MessageService::SendMessage(&teleMessage)) {
                    NRF_LOG_DEBUG("Couldn't send message yet");
                }
                lastAccelWasSent = true;
//              NRF_LOG_INFO("Sending Telemetry %d ms", Utils::millis());
            }

            // Always remember the last frame, so we can extract delta time!
            lastAccelFrame = frame;
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
        teleMessage.type = Message::MessageType_Telemetry;
        for (int i = 0; i < 1; ++i)
        {
            teleMessage.data[i] = { 0,0,0,0 };
        }

        lastAccelWasSent = false;

        // Ask the acceleration controller to be notified when
        // new acceleration data comes in!
        Accelerometer::hook(onAccDataReceived, nullptr);
        telemetryActive = true;
    }

    void stop() {
        // Stop being notified!
        Accelerometer::unHook(onAccDataReceived);
        telemetryActive = false;
    }
}
}
