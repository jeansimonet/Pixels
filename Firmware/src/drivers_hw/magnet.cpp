#include "magnet.h"
#include "../config/board_config.h"
#include "../drivers_nrf/log.h"
#include "../drivers_nrf/power_manager.h"
#include "../drivers_nrf/timers.h"
#include "../drivers_nrf/gpiote.h"
#include "nrf_gpio.h"
#include "../core/delegate_array.h"
#include "app_timer.h"

using namespace Config;
using namespace DriversNRF;

#define MAX_BATTERY_CLIENTS 2
#define MAGNET_TIMER_MS (1000)	// ms

namespace DriversHW
{
namespace Magnet
{
	DelegateArray<ClientMethod, MAX_BATTERY_CLIENTS> clients;

	APP_TIMER_DEF(magnetTimer);
    void update(void* context);

    bool magnetAvailable = false;

    void init() {

        // Status pin needs a pull-up, and is pulled low when charging
        auto pin = BoardManager::getBoard()->magnetPin;
        magnetAvailable = pin != 0xFFFFFFFF;
        if (magnetAvailable) {
            nrf_gpio_cfg_default(pin);

            // ret_code_t ret_code = app_timer_create(&magnetTimer, APP_TIMER_MODE_REPEATED, update);
            // APP_ERROR_CHECK(ret_code);

            // ret_code = app_timer_start(magnetTimer, APP_TIMER_TICKS(MAGNET_TIMER_MS), NULL);
            // APP_ERROR_CHECK(ret_code);

            // Read battery level and convert
            int magnet = checkMagnet() ? 1 : 0;
            NRF_LOG_INFO("Magnet initialized, Magnet=%d", magnet);
        } else {
            NRF_LOG_INFO("Magnet Disabled");
        }
    }

    bool checkMagnet() {
        if (magnetAvailable) {
            // Status pin needs a pull-up, and is pulled low when magnet is present
            uint32_t statePin = BoardManager::getBoard()->magnetPin;
            nrf_gpio_cfg_input(statePin, NRF_GPIO_PIN_NOPULL);
            bool ret = nrf_gpio_pin_read(statePin) == 0;
            nrf_gpio_cfg_default(statePin);
            return ret;
        } else {
            return false;
        }
    }

    bool canCheckMagnet() {
        return magnetAvailable;
    }

	/// <summary>
	/// Method used by clients to request timer callbacks when magnet triggers
	/// </summary>
	void hook(ClientMethod callback, void* parameter)
	{
		if (!clients.Register(parameter, callback))
		{
			NRF_LOG_ERROR("Too many magnet hooks registered.");
		}
	}

	/// <summary>
	/// Method used by clients to stop getting magnet callbacks
	/// </summary>
	void unHook(ClientMethod callback)
	{
		clients.UnregisterWithHandler(callback);
	}

	/// <summary>
	/// Method used by clients to stop getting magnet callbacks
	/// </summary>
	void unHookWithParam(void* param)
	{
		clients.UnregisterWithToken(param);
	}

    void update(void* context)
    {
        int magnet = checkMagnet() ? 1 : 0;
        NRF_LOG_INFO("Magnet=%d", magnet);
   }
}
}