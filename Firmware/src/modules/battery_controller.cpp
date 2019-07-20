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
#define MAX_BATTERY_CLIENTS 2

namespace Modules
{
namespace BatteryController
{
    void getBatteryLevel(void* context, const Message* msg);
    void update(void* context);
    void onBatteryEventHandler(void* context);
    BatteryState computeCurrentState();

    bool onCharger = false;
    bool charging = false;
    float vBat = 0.0f;
    BatteryState currentBatteryState = BatteryState_Unknown;

	DelegateArray<BatteryStateChangeHandler, MAX_BATTERY_CLIENTS> clients;

	_APP_TIMER_DEF(batteryControllerTimer);

    void init() {
        MessageService::RegisterMessageHandler(Message::MessageType_RequestBatteryLevel, nullptr, getBatteryLevel);

        onCharger = Battery::checkCoil();
        charging = Battery::checkCharging();
        vBat = Battery::checkVBat();

        // Register for battery events
        Battery::hook(onBatteryEventHandler, nullptr);

		ret_code_t ret_code = app_timer_create(&batteryControllerTimer, APP_TIMER_MODE_REPEATED, update);
		APP_ERROR_CHECK(ret_code);

		ret_code = app_timer_start(batteryControllerTimer, APP_TIMER_TICKS(BATTERY_TIMER_MS), NULL);
		APP_ERROR_CHECK(ret_code);

        // Set initial battery state
        currentBatteryState = computeCurrentState();

        NRF_LOG_INFO("Battery controller initialized");
    }

    BatteryState computeCurrentState() {
        BatteryState ret = BatteryState_Unknown;
        if (onCharger) {
            if (charging) {
                ret = BatteryState_Charging;
            } else {
                // Either we're done, or we haven't started
                if (vBat < SettingsManager::getSettings()->batteryLow) {
                    // Not started
                    ret = BatteryState_Low;
                } else {
                    ret = BatteryState_Ok;
                }
            }
        }
        return ret;
    }

    void getBatteryLevel(void* context, const Message* msg) {
        // Fetch battery level
        float level = Battery::checkVBat();
        MessageBatteryLevel lvl;
        lvl.level = level;
        NRF_LOG_INFO("Received Battery Level Request, returning " NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(level));
        MessageService::SendMessage(&lvl);
    }

    void update(void* context) {
        auto newState = computeCurrentState();
        if (newState != currentBatteryState) {
            currentBatteryState = newState;
            for (int i = 0; i < clients.Count(); ++i) {
    			clients[i].handler(clients[i].token, newState);
            }
        }
    }

    void onBatteryEventHandler(void* context) {
        update(nullptr);
    }

	/// <summary>
	/// Method used by clients to request timer callbacks when accelerometer readings are in
	/// </summary>
	void hook(BatteryStateChangeHandler callback, void* parameter)
	{
		if (!clients.Register(parameter, callback))
		{
			NRF_LOG_ERROR("Too many battery state hooks registered.");
		}
	}

	/// <summary>
	/// Method used by clients to stop getting accelerometer reading callbacks
	/// </summary>
	void unHook(BatteryStateChangeHandler callback)
	{
		clients.UnregisterWithHandler(callback);
	}

	/// <summary>
	/// Method used by clients to stop getting accelerometer reading callbacks
	/// </summary>
	void unHookWithParam(void* param)
	{
		clients.UnregisterWithToken(param);
	}

}
}