#include "battery_controller.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "drivers_hw/battery.h"
#include "config/settings.h"
#include "nrf_log.h"
#include "app_timer.h"
#include "app_error.h"
#include "die.h"

using namespace DriversHW;
using namespace Bluetooth;
using namespace Config;

#define BATTERY_TIMER_MS (5000)	// ms

namespace Modules
{
namespace BatteryController
{
    void GetBatteryLevel(void* context, const Message* msg);
    void update(void* context);
    void onBatteryEventHandler(void* context);

    bool onCharger = false;
    bool charging = false;
    float vBat = 0.0f;

	_APP_TIMER_DEF(batteryControllerTimer);

    void init() {
        MessageService::RegisterMessageHandler(Message::MessageType_RequestBatteryLevel, nullptr, GetBatteryLevel);

        onCharger = Battery::checkCoil();
        charging = Battery::checkCharging();
        vBat = Battery::checkVBat();

        // Register for battery events
        Battery::hook(onBatteryEventHandler, nullptr);

		ret_code_t ret_code = app_timer_create(&batteryControllerTimer, APP_TIMER_MODE_REPEATED, update);
		APP_ERROR_CHECK(ret_code);

		ret_code = app_timer_start(batteryControllerTimer, APP_TIMER_TICKS(BATTERY_TIMER_MS), NULL);
		APP_ERROR_CHECK(ret_code);

        NRF_LOG_INFO("Battery controller initialized");
    }

    void GetBatteryLevel(void* context, const Message* msg) {
        // Fetch battery level
        float level = Battery::checkVBat();
        MessageBatteryLevel lvl;
        lvl.level = level;
        MessageService::SendMessage(&lvl);
    }

    void update(void* context) {
        bool currentlyChargingState = charging && onCharger;
        if (!currentlyChargingState) {
            // If the battery is not on the coil, check its level
            float level = Battery::checkVBat();
            if (level < SettingsManager::getSettings()->batteryLow) {
                // Notify the die that battery is low
                Die::onChargingNeeded();
            }
        }
    }

    void onBatteryEventHandler(void* context) {
        bool newCoil = Battery::checkCoil();
        bool newCharging = Battery::checkCharging();
        bool newVBat = Battery::checkVBat();

        bool currentlyChargingState = charging && onCharger;
        bool newChargingState = newCharging && newCoil;

        if (currentlyChargingState) {
            if (newChargingState) {
                // No change!
            } else {
                // Are we charged enough
                if (newVBat > SettingsManager::getSettings()->batteryHigh) {
                    // Yes
                    Die::onChargingComplete();
                } else {
                    // Notify die
                    Die::onChargingInterrupted();
                }
            }
        } else {
            if (newChargingState) {
                Die::onChargingStarted();
            } else {
                // No change!
            }
        }

        onCharger = newCoil;
        charging = newCharging;
        vBat = newVBat;
    }

}
}