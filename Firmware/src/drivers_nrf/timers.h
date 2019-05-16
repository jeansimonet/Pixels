#pragma once

#include "app_timer.h"
#include "nrf_log.h"

namespace DriversNRF
{
    // Initializes the sdk timers
    namespace Timers
    {
        void init();
        void createTimer(app_timer_id_t const * p_timer_id, app_timer_mode_t mode, app_timer_timeout_handler_t timeout_handler);
        void startTimer(app_timer_id_t timer_id, uint32_t timeout_ms, void * p_context);
        void stopTimer(app_timer_id_t timer_id);
        void stopAll(void);
        void pause(void);
        void resume(void);
        void selfTest();
    }
}
