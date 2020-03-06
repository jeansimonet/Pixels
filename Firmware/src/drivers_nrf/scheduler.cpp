#include "scheduler.h"
#include "app_error.h"
#include "app_timer.h"
#include "log.h"

#define SCHED_MAX_EVENT_DATA_SIZE      132        /**< Maximum size of scheduler events. */
#define SCHED_QUEUE_SIZE               10        /**< Maximum number of events in the scheduler queue. */

namespace DriversNRF
{
namespace Scheduler
{
    void init() {
        APP_SCHED_INIT(SCHED_MAX_EVENT_DATA_SIZE, SCHED_QUEUE_SIZE);
    }

    void update() {
        app_sched_execute();
    }

    void push(const void* eventData, uint16_t size, app_sched_event_handler_t handler) {
        ASSERT(size <= SCHED_MAX_EVENT_DATA_SIZE);
        ret_code_t ret = app_sched_event_put((void*)eventData, size, handler);
        APP_ERROR_CHECK(ret);
    }
}
}