#pragma once

#include "nrf_drv_wdt.h"

namespace DriversNRF
{
	namespace Watchdog
	{
		void init();
		void initClearResetFlagTimer();
		void feed();
		void selfTest();
	}
}
