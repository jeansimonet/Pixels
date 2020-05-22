#include "battery.h"
#include "nrf_log.h"
#include "board_config.h"
#include "nrf_gpio.h"
#include "nrf_delay.h"
#include "nrf_saadc.h"
#include "../drivers_nrf/gpiote.h"
#include "../drivers_nrf/a2d.h"
#include "../drivers_nrf/log.h"
#include "../drivers_nrf/timers.h"
#include "../drivers_nrf/power_manager.h"
#include "../drivers_nrf/scheduler.h"
#include "../core/delegate_array.h"

using namespace DriversNRF;
using namespace Config;

#define MAX_BATTERY_CLIENTS 2

namespace DriversHW
{
namespace Battery
{
    const float vBatMult = 1.4f; // Voltage divider 10M over 4M
	DelegateArray<ClientMethod, MAX_BATTERY_CLIENTS> clients;

    void batteryInterruptHandler(uint32_t pin, nrf_gpiote_polarity_t action);

    void init() {
        // Set charger and fault pins as input

        // Drive the status pin down for a moment
        uint32_t statePin = BoardManager::getBoard()->chargingStatePin;

        // Status pin needs a pull-up, and is pulled low when charging
        if (statePin != 0xFFFFFFFF) {
            nrf_gpio_cfg_default(statePin);
        }

        // Read battery level and convert
        int charging = checkCharging() ? 1 : 0;

		// // Set interrupt pin
		// GPIOTE::enableInterrupt(
		// 	statePin,
		// 	NRF_GPIO_PIN_PULLUP,
		// 	NRF_GPIOTE_POLARITY_TOGGLE,
		// 	batteryInterruptHandler);

		// GPIOTE::enableInterrupt(
		// 	coilPin,
		// 	NRF_GPIO_PIN_NOPULL,
		// 	NRF_GPIOTE_POLARITY_TOGGLE,
		// 	batteryInterruptHandler);

        NRF_LOG_INFO("Battery initialized, Charging=%d", charging);

        #if DICE_SELFTEST && BATTERY_SELFTEST
        selfTest();
        #endif
    }

    float checkVBat() {
        float ret = A2D::readVBat() * vBatMult;
        return ret;
    }

    float checkVCoil() {
        float ret = A2D::read5V() * vBatMult;
        return ret;
    }

    bool canCheckVCoil() {
        return Config::BoardManager::getBoard()->coilSensePin != NRF_SAADC_INPUT_DISABLED;
    }

    float checkVLED() {
        float ret = A2D::readVLED() * vBatMult;
        return ret;
    }

    bool canCheckVLED() {
        return Config::BoardManager::getBoard()->vledSensePin != NRF_SAADC_INPUT_DISABLED;
    }

    bool checkCharging() {
        bool ret = false;
        // Status pin needs a pull-up, and is pulled low when charging
        uint32_t statePin = BoardManager::getBoard()->chargingStatePin;
        if (statePin != 0xFFFFFFFF) {
            nrf_gpio_cfg_input(statePin, NRF_GPIO_PIN_PULLUP);
            ret = nrf_gpio_pin_read(statePin) == 0;
            nrf_gpio_cfg_default(statePin);
        }
        return ret;
    }

    bool canCheckCharging() {
        return Config::BoardManager::getBoard()->chargingStatePin != 0xFFFFFFFF;
    }

    void handleBatteryEvent(void * p_event_data, uint16_t event_size) {
		// Notify clients
		for (int i = 0; i < clients.Count(); ++i)
		{
			clients[i].handler(clients[i].token);
		}
    }

	void batteryInterruptHandler(uint32_t pin, nrf_gpiote_polarity_t action) {
        Scheduler::push(nullptr, 0, handleBatteryEvent);
	}

	/// <summary>
	/// Method used by clients to request callbacks when battery changes state
	/// </summary>
	void hook(ClientMethod callback, void* parameter)
	{
		if (!clients.Register(parameter, callback))
		{
			NRF_LOG_ERROR("Too many battery hooks registered.");
		}
	}

	/// <summary>
	/// Method used by clients to stop getting battery callbacks
	/// </summary>
	void unHook(ClientMethod callback)
	{
		clients.UnregisterWithHandler(callback);
	}

	/// <summary>
	/// Method used by clients to stop getting battery callbacks
	/// </summary>
	void unHookWithParam(void* param)
	{
		clients.UnregisterWithToken(param);
	}

    #if DICE_SELFTEST && BATTERY_SELFTEST
    APP_TIMER_DEF(readBatTimer);
    void printBatStats(void* context) {
        float vbattery = checkVBat();
        int charging = checkCharging() ? 1 : 0;
        int coil = checkCoil() ? 1 : 0;
        NRF_LOG_INFO("Charging=%d, Coil=%d, Voltage=" NRF_LOG_FLOAT_MARKER, charging, coil, NRF_LOG_FLOAT(vbattery));
    }

    void selfTest() {
        Timers::createTimer(&readBatTimer, APP_TIMER_MODE_REPEATED, printBatStats);
        NRF_LOG_INFO("Reading battery status repeatedly, press any key to abort");
        Log::process();

        Timers::startTimer(readBatTimer, 200, nullptr);
        while (!Log::hasKey()) {
            Log::process();
            PowerManager::feed();
            PowerManager::update();
        }
		Log::getKey();
        NRF_LOG_INFO("Finished reading battery status!");
        Timers::stopTimer(readBatTimer);
    }
    #endif
}
}