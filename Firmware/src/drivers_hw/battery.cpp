#include "battery.h"
#include "nrf_log.h"
#include "board_config.h"
#include "nrf_gpio.h"
#include "nrf_delay.h"
#include "../drivers_nrf/gpiote.h"
#include "../drivers_nrf/a2d.h"
#include "../drivers_nrf/log.h"
#include "../drivers_nrf/timers.h"
#include "../drivers_nrf/power_manager.h"

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
        uint32_t coilPin = BoardManager::getBoard()->CoilStatePin;

        // Status pin needs a pull-up, and is pulled low when charging
        nrf_gpio_cfg_default(statePin);

        // +5V sense pin needs a pull-down and is pulled up while charging
        nrf_gpio_cfg_default(coilPin);

        // Read battery level and convert
        float vbattery = checkVBat();
        int charging = checkCharging() ? 1 : 0;
        int coil = checkCoil() ? 1 : 0;

		// Set interrupt pin
		GPIOTE::enableInterrupt(
			statePin,
			NRF_GPIO_PIN_NOPULL,
			NRF_GPIOTE_POLARITY_TOGGLE,
			batteryInterruptHandler);

		GPIOTE::enableInterrupt(
			coilPin,
			NRF_GPIO_PIN_NOPULL,
			NRF_GPIOTE_POLARITY_TOGGLE,
			batteryInterruptHandler);

        NRF_LOG_INFO("Battery initialized, Charging=%d, Coil=%d, Battery Voltage=" NRF_LOG_FLOAT_MARKER, charging, coil, NRF_LOG_FLOAT(vbattery));

        #if DICE_SELFTEST && BATTERY_SELFTEST
        selfTest();
        #endif
    }

    float checkVBat() {
        return A2D::readVBat() * vBatMult;
    }

    bool checkCharging() {
        // Status pin needs a pull-up, and is pulled low when charging
        uint32_t statePin = BoardManager::getBoard()->chargingStatePin;
        nrf_gpio_cfg_input(statePin, NRF_GPIO_PIN_NOPULL);
        bool ret = nrf_gpio_pin_read(BoardManager::getBoard()->chargingStatePin) == 0;
        nrf_gpio_cfg_default(statePin);
        return ret;
    }

    bool checkCoil() {
        uint32_t coilPin = BoardManager::getBoard()->CoilStatePin;
        nrf_gpio_cfg_input(coilPin, NRF_GPIO_PIN_NOPULL);
        bool ret = nrf_gpio_pin_read(BoardManager::getBoard()->CoilStatePin) != 0;
        nrf_gpio_cfg_default(coilPin);
        return ret;
    }

	void batteryInterruptHandler(uint32_t pin, nrf_gpiote_polarity_t action) {
		// Notify clients
		for (int i = 0; i < clients.Count(); ++i)
		{
			clients[i].handler(clients[i].token);
		}
	}

	/// <summary>
	/// Method used by clients to request timer callbacks when accelerometer readings are in
	/// </summary>
	void hook(ClientMethod callback, void* parameter)
	{
		if (!clients.Register(parameter, callback))
		{
			NRF_LOG_ERROR("Too many battery hooks registered.");
		}
	}

	/// <summary>
	/// Method used by clients to stop getting accelerometer reading callbacks
	/// </summary>
	void unHook(ClientMethod callback)
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