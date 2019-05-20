#pragma once

#include <stdint.h>
#include <stddef.h>
#include "nrf_drv_gpiote.h"

namespace DriversNRF
{
	/// <summary>
	/// Wrapper for the GPIOTE library
	/// </summary>
	namespace GPIOTE
	{
		void init();
    
        typedef void (*PinHandler)(uint32_t pin, nrf_gpiote_polarity_t action);
        void enableInterrupt(uint32_t pin, nrf_gpio_pin_pull_t pull, nrf_gpiote_polarity_t polarity, PinHandler handler);
        void disableInterrupt(uint32_t pin);
	}
}

