#include "battery_controller.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "drivers_hw/battery.h"
#include "config/settings.h"
#include "nrf_log.h"
#include "app_timer.h"
#include "app_error.h"
#include "die.h"
#include "drivers_hw/apa102.h"
#include "utils/utils.h"
#include "drivers_nrf/a2d.h"

using namespace DriversHW;
using namespace DriversNRF;
using namespace Bluetooth;
using namespace Config;
using namespace Utils;

#define BATTERY_TIMER_MS (3000)	// ms
#define BATTERY_TIMER_MS_QUICK (100) //ms
#define MAX_BATTERY_CLIENTS 2
#define MAX_LEVEL_CLIENTS 2
#define LAZY_CHARGE_DETECT
#define CHARGE_START_DETECTION_THRESHOLD (0.3f) // 0.3V
#define CHARGE_VCOIL_THRESHOLD (4.0) //0.4V
#define CHARGE_FULL (4.0f) // 4.0V
#define INVALID_CHARGE_TIMEOUT 5000
namespace Modules
{
namespace BatteryController
{
    void getBatteryLevel(void* context, const Message* msg);
    void update(void* context);
    void onBatteryEventHandler(void* context);
    void onLEDPowerEventHandler(void* context, bool powerOn);
    BatteryState computeCurrentState();
    float LookupChargeLevel(float voltage);

    float vcoil = 0.0f;
    float vBat = 0.0f;
    bool charging = false;
    float lowestVBat = 0.0f;
    bool lazyChargeDetect = false;
    BatteryState currentBatteryState = BatteryState_Unknown;
    uint32_t lastUpdateTime = 0;

    float vBatWhenChargingStart = 0.0f;
    uint32_t chargingStartedTime = 0;

	DelegateArray<BatteryStateChangeHandler, MAX_BATTERY_CLIENTS> clients;
    DelegateArray<BatteryLevelChangeHandler, MAX_LEVEL_CLIENTS> levelClients;

	APP_TIMER_DEF(batteryControllerTimer);

    static const float voltages[] =
    {
        4.149f,
        4.120f,
        4.085f,
        3.982f,
        3.824f,
        3.760f,
        3.716f,
        3.632f,
        3.578f,
        3.401f,
        3.303f,
        3.209f,
        3.012f,
        2.451f,
    };

    static const float levels[] =
    {
        1.000f,
        0.993f,
        0.970f,
        0.869f,
        0.643f,
        0.491f,
        0.317f,
        0.142f,
        0.082f,
        0.039f,
        0.026f,
        0.018f,
        0.008f,
        0.000f,
    };
    static const int ChargeLookupCount = 14;

    void init() {
        MessageService::RegisterMessageHandler(Message::MessageType_RequestBatteryLevel, nullptr, getBatteryLevel);

        vcoil = Battery::checkVCoil();
        vBat = Battery::checkVBat();
        charging = Battery::checkCharging();
        lowestVBat = vBat;
        lazyChargeDetect = !Battery::canCheckCharging();

        // Register for battery events
        Battery::hook(onBatteryEventHandler, nullptr);

        // Register for led events
        APA102::hookPowerState(onLEDPowerEventHandler, nullptr);

        // Set initial battery state
        currentBatteryState = computeCurrentState();

		ret_code_t ret_code = app_timer_create(&batteryControllerTimer, APP_TIMER_MODE_SINGLE_SHOT, update);
		APP_ERROR_CHECK(ret_code);

		ret_code = app_timer_start(batteryControllerTimer, APP_TIMER_TICKS(BATTERY_TIMER_MS), NULL);
		APP_ERROR_CHECK(ret_code);

        lastUpdateTime = millis();

        if (lazyChargeDetect) {
            NRF_LOG_INFO("Battery controller initialized - Lazy Charge Detect - Battery %s", getChargeStateString(currentBatteryState));
        } else {
            NRF_LOG_INFO("Battery controller initialized - Battery %s", getChargeStateString(currentBatteryState));
        }
        float level = LookupChargeLevel(vBat);
        NRF_LOG_INFO("\tBattery level %d%%", (int)(level * 100));
    }

	BatteryState getCurrentChargeState() {
        return currentBatteryState;
    }

    float getCurrentLevel() {
        return LookupChargeLevel(vBat);
    }


    const char* getChargeStateString(BatteryState state) {
        switch (currentBatteryState) {
			case BatteryState_Ok:
                return "Ok";
			case BatteryState_Low:
                return "Low";
			case BatteryState_Charging:
                return "Charging";
			case BatteryState_Unknown:
            default:
                return "Unknown";
        }
    }

    BatteryState computeCurrentState() {
        BatteryState ret = BatteryState_Unknown;

        // Measure new vBat
        float level = Battery::checkVBat();
        ret = currentBatteryState;
        switch (currentBatteryState)
        {
            case BatteryState_Done:
                if (!lazyChargeDetect) {
                    if (Battery::checkCharging()) {
                        // Started charging again
                        ret = BatteryState_Charging;
                    } else if (Battery::checkVCoil() < CHARGE_VCOIL_THRESHOLD) {
                        // No longer on charger
                        ret = BatteryState_Ok;
                    }
                }
                // In lazy charge detect mode, we're not sure if the die is still on the charger
                break;
            case BatteryState_Ok:
                if (level < SettingsManager::getSettings()->batteryLow) {
                    ret = BatteryState_Low;
                } else {
                    if ((lazyChargeDetect && (level > lowestVBat + CHARGE_START_DETECTION_THRESHOLD)) || Battery::checkCharging()) {
                        // Battery level going up, we must be charging
                        ret = BatteryState_Charging;
                        vBatWhenChargingStart = lowestVBat;
                        chargingStartedTime = millis();
                    } else {
                        // Update stored lowest level
                        if (level < lowestVBat) {
                            lowestVBat = level;
                        }
                    }
                }
                // Else still BatteryState_Ok
                break;
            case BatteryState_Charging:
                if (lazyChargeDetect) {
                    if (level > SettingsManager::getSettings()->batteryHigh) {
                        // Reset lowest level
                        ret = BatteryState_Ok;
                    } else
                    // Make sure we've waited enough to check state again
                    if (millis() - chargingStartedTime > INVALID_CHARGE_TIMEOUT) {
                        if (level < vBatWhenChargingStart + CHARGE_START_DETECTION_THRESHOLD) {
                            // It looks like we stopped charging
                            if (level > SettingsManager::getSettings()->batteryLow) {
                                ret = BatteryState_Ok;
                            } else {
                                ret = BatteryState_Low;
                            }
                        }
                        // Else still charging...
                    }
                } else {
                    if (Battery::checkVCoil() > CHARGE_VCOIL_THRESHOLD) {
                        if (!Battery::checkCharging()) {
                            // Still on charger, but done charging
                            ret = BatteryState_Done;
                        }
                        // Else still charging
                    } else {
                        // No longer on charger, but we hadn't finished charging, check vBat
                        if (level > SettingsManager::getSettings()->batteryLow) {
                            ret = BatteryState_Ok;
                        } else {
                            ret = BatteryState_Low;
                        }
                    }
                }
                // Else still not charged enough
                lowestVBat = level;
                break;
            case BatteryState_Low:
                if ((lazyChargeDetect && (level > lowestVBat + CHARGE_START_DETECTION_THRESHOLD)) || Battery::checkCharging()) {
                    // Battery level going up, we must be charging
                    ret = BatteryState_Charging;
                    vBatWhenChargingStart = lowestVBat;
                    chargingStartedTime = millis();
                } else {
                    // Update stored lowest level
                    if (level < lowestVBat) {
                        lowestVBat = level;
                    }
                }
                break;
            default:
                if (!lazyChargeDetect && Battery::checkCharging()) {
                    ret = BatteryState_Charging;
                } else if (level > SettingsManager::getSettings()->batteryLow) {
                    ret = BatteryState_Ok;
                } else {
                    ret = BatteryState_Low;
                }
                break;
        }

        // Always update the stored battery voltage
        vBat = level;
        return ret;
    }

    void getBatteryLevel(void* context, const Message* msg) {
        // Fetch battery level
        float voltage = Battery::checkVBat();
        float level = LookupChargeLevel(voltage);
        MessageBatteryLevel lvl;
        lvl.voltage = voltage;
        lvl.level = level;
        NRF_LOG_DEBUG("Received Battery Level Request, returning " NRF_LOG_FLOAT_MARKER " (" NRF_LOG_FLOAT_MARKER "v)", NRF_LOG_FLOAT(level), NRF_LOG_FLOAT(voltage));
        MessageService::SendMessage(&lvl);
    }

    void update(void* context) {
        // // DEBUG
        // Battery::printA2DReadings();
        // // DEBUG

        auto newState = computeCurrentState();
        if (newState != currentBatteryState) {
            switch (newState) {
                case BatteryState_Done:
                    NRF_LOG_INFO("Battery finished charging");
                    break;
                case BatteryState_Ok:
                    NRF_LOG_INFO(">>> Battery is now Ok, vBat = " NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vBat));
                    break;
                case BatteryState_Charging:
                    NRF_LOG_INFO(">>> Battery is now Charging, vBat = " NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vBat));
                    break;
                case BatteryState_Low:
                    NRF_LOG_INFO(">>> Battery is Low, vBat = " NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vBat));
                    break;
                default:
                    NRF_LOG_INFO(">>> Battery is Unknown, vBat = " NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vBat));
                    break;
            }
            float level = LookupChargeLevel(vBat);
            NRF_LOG_INFO("\tBat = " NRF_LOG_FLOAT_MARKER " %% ", NRF_LOG_FLOAT(level*100));
            NRF_LOG_INFO("\tvBat = " NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vBat));
            vcoil = Battery::checkVCoil();
            NRF_LOG_INFO("\tvCoil = " NRF_LOG_FLOAT_MARKER, NRF_LOG_FLOAT(vcoil));
            currentBatteryState = newState;
            for (int i = 0; i < clients.Count(); ++i) {
    			clients[i].handler(clients[i].token, newState);
            }
        }

        float level = LookupChargeLevel(vBat);
        for (int i = 0; i < levelClients.Count(); ++i) {
            levelClients[i].handler(levelClients[i].token, level);
        }

	    app_timer_start(batteryControllerTimer, APP_TIMER_TICKS(BATTERY_TIMER_MS), NULL);
    }

    void onBatteryEventHandler(void* context) {
        update(nullptr);
    }

    void onLEDPowerEventHandler(void* context, bool powerOn) {
        if (powerOn) {
            app_timer_stop(batteryControllerTimer);
        } else {
            app_timer_stop(batteryControllerTimer);

            // If it's been too long since we checked, check right away
            uint32_t delay = BATTERY_TIMER_MS;
            if (millis() - lastUpdateTime > BATTERY_TIMER_MS) {
                delay = BATTERY_TIMER_MS_QUICK;
            }
            // Restart the timer
		    app_timer_start(batteryControllerTimer, APP_TIMER_TICKS(delay), NULL);
        }
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
	/// </summary>
	void unHook(BatteryStateChangeHandler callback)
	{
		clients.UnregisterWithHandler(callback);
	}

	/// <summary>
	/// </summary>
	void unHookWithParam(void* param)
	{
		clients.UnregisterWithToken(param);
	}

	/// <summary>
	/// </summary>
	void hookLevel(BatteryLevelChangeHandler callback, void* parameter)
	{
		if (!levelClients.Register(parameter, callback))
		{
			NRF_LOG_ERROR("Too many battery state hooks registered.");
		}
	}

	/// <summary>
	/// </summary>
	void unHookLevel(BatteryLevelChangeHandler callback)
	{
		levelClients.UnregisterWithHandler(callback);
	}

	/// <summary>
	/// </summary>
	void unHookLevelWithParam(void* param)
	{
		levelClients.UnregisterWithToken(param);
	}

    float LookupChargeLevel(float voltage)
    {
		// Find the first keyframe
		int nextIndex = 0;
		while (nextIndex < ChargeLookupCount && voltages[nextIndex] >= voltage) {
		 	nextIndex++;
		}

		float level = 0.0f;
		if (nextIndex == 0) {
			level = 1.0f;
		} else if (nextIndex == ChargeLookupCount) {
			level = 0.0f;
		} else {
			// Grab the prev and next keyframes
			float nextVoltage = voltages[nextIndex];
			float prevVoltage = voltages[nextIndex - 1];
			float nextLevel = levels[nextIndex];
			float prevLevel = levels[nextIndex - 1];

			// Compute the interpolation parameter
    		int percent = (prevVoltage - voltage) / (prevVoltage - nextVoltage);
            level = prevLevel * (1.0f - percent) + nextLevel * percent;
		}

		return level;
    }

    // int16_t LookupChargeLevel(int16_t scaledVoltage)
    // {
	// 	// Find the first keyframe
    //     int keyFrameCount = sizeof(ChargeLookup) / sizeof(ChargeEntry);
	// 	int nextIndex = 0;
	// 	while (nextIndex < keyFrameCount && ChargeLookup[nextIndex].scaledVoltage >= scaledVoltage) {
	// 		nextIndex++;
	// 	}

	// 	int16_t scaledLevel = 0.0f;
	// 	if (nextIndex == 0) {
	// 		scaledLevel = SCALER;
	// 	} else if (nextIndex == keyFrameCount) {
	// 		scaledLevel = 0;
	// 	} else {
	// 		// Grab the prev and next keyframes
	// 		auto nextKeyframe = ChargeLookup[nextIndex];
	// 		auto prevKeyframe = ChargeLookup[nextIndex - 1];

	// 		// Compute the interpolation parameter
    // 		int32_t scaledPercent = (prevKeyframe.scaledVoltage - scaledVoltage) * SCALER / (prevKeyframe.scaledVoltage - nextKeyframe.scaledVoltage);
    //         int32_t doubleScaledLevel = (int32_t)prevKeyframe.scaledCharge * (SCALER - scaledPercent) + (int32_t)nextKeyframe.scaledCharge * scaledPercent;
    //         scaledLevel = (int16_t)(doubleScaledLevel / SCALER);
	// 	}

	// 	return scaledLevel;
    // }

}
}