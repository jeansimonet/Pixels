#pragma once
#include "app_scheduler.h"

namespace DriversNRF
{
    namespace Scheduler
    {
        void init();
        void update();
        bool push(const void* eventData, uint16_t size, app_sched_event_handler_t handler);
    }
}
