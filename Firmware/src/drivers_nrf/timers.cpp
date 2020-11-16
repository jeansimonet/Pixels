#include "timers.h"
#include "power_manager.h"
#include "app_error.h"
#include "app_error_weak.h"
#include "Log.h"
#include "nrf_delay.h"
#include "nrf_gpio.h"
#include "nrf_drv_clock.h"

#define MAX_DELAYED_CALLS 4

namespace DriversNRF
{
namespace Timers
{
    APP_TIMER_DEF(delayedCallbacksTimer);
    void delayedCallbacksTimerCallback(void* ignore);

    struct delayedCallbacksTimerInfo
    {
        DelayedCallback callback;
        void* param;
        int callbackTime;
    };

    delayedCallbacksTimerInfo delayedCallbacks[MAX_DELAYED_CALLS];
    int delayedCallbacksCount;
    int delayedCallbackPauseRequestCount;

    void init() {
        ret_code_t err_code;
        
        // This doesn't seem needed when SD is enabled
        #if !SOFTDEVICE_PRESENT || DEBUG
        if (!nrf_drv_clock_init_check()) {
            nrf_drv_clock_init(); // No need to check return value, only indicates that the module is already initialized
        }
        #endif

        err_code = app_timer_init();
        APP_ERROR_CHECK(err_code);

        nrf_drv_clock_lfclk_request(NULL);
        
        // Wait for the clock to be ready.
        while (!nrf_clock_lf_is_running()) {;}

        // Create a temp timer that can be used by modules, like the behavior controller
        createTimer(&delayedCallbacksTimer, APP_TIMER_MODE_SINGLE_SHOT, delayedCallbacksTimerCallback);
        delayedCallbacksCount = 0;
        delayedCallbackPauseRequestCount = 0;

        NRF_LOG_INFO("App Timers initialized");

        #if DICE_SELFTEST && TIMERS_SELFTEST
        selfTest();
        #endif
    }

    void createTimer(app_timer_id_t const * p_timer_id, app_timer_mode_t mode, app_timer_timeout_handler_t timeout_handler) {
        ret_code_t err_code = app_timer_create(p_timer_id, mode, timeout_handler);
        APP_ERROR_CHECK(err_code);
    }

    void startTimer(app_timer_id_t timer_id, uint32_t timeout_ms, void * p_context) {
        ret_code_t err_code = app_timer_start(timer_id, APP_TIMER_TICKS(timeout_ms), p_context);
        APP_ERROR_CHECK(err_code);
    }

    void stopTimer(app_timer_id_t timer_id) {
        ret_code_t err_code = app_timer_stop(timer_id);
        APP_ERROR_CHECK(err_code);
    }

    void stopAll(void) {
        ret_code_t err_code = app_timer_stop_all();
        APP_ERROR_CHECK(err_code);
    }

    void pause(void) {
        app_timer_pause();
    }

    void resume(void) {
        app_timer_resume();
    }

	#define APP_TIMER_MS(TICKS) ((uint32_t)ROUNDED_DIV((TICKS) * 1000 * (APP_TIMER_CONFIG_RTC_FREQUENCY + 1), (uint64_t)APP_TIMER_CLOCK_FREQ))

	int millis()
	{
		auto ticks = app_timer_cnt_get();
		return APP_TIMER_MS(ticks);
	}

    void delayedCallbacksTimerCallback(void* ignore) {
        int time = millis();
        do
        {
            auto cb = delayedCallbacks[0].callback;
            auto p = delayedCallbacks[0].param;
            if (delayedCallbacksCount > 1) {
                // Shift items down
                for (int i = 0; i < delayedCallbacksCount - 1; ++i) {
                    delayedCallbacks[i] = delayedCallbacks[i+1];
                }
            }
            delayedCallbacksCount--;

            // Trigger the callback
            cb(p);
        }
        while (delayedCallbacksCount > 0 && delayedCallbacks[0].callbackTime <= time);

        if (delayedCallbacksCount > 0) {
            // Set the timer for the next call
            startTimer(delayedCallbacksTimer, delayedCallbacks[0].callbackTime - time, nullptr);
        }
    }

    bool setDelayedCallback(DelayedCallback callback, void* param, int periodMs) {
        bool ret = delayedCallbacksCount < MAX_DELAYED_CALLS;
        if (ret) {
            // Find where to insert this call
            int insertIndex = 0;
            int callbackTime = millis() + periodMs;
            while (insertIndex < delayedCallbacksCount && callbackTime > delayedCallbacks[insertIndex].callbackTime) {
                insertIndex++;
            }

            // Shift all the elements after the new one to insert
            if (delayedCallbacksCount > 1) {
                for (int i = delayedCallbacksCount - 1; i >= insertIndex; --i) {
                    delayedCallbacks[i+1] = delayedCallbacks[i];
                }
            }
            if (insertIndex == 0 && delayedCallbacksCount > 0) {
                // Stop the current timer since the new callback is sooner
                stopTimer(delayedCallbacksTimer);
            }

            // Insert the new callback
            auto& cb = delayedCallbacks[insertIndex];
            cb.callback = callback;
            cb.param = param;
            cb.callbackTime = callbackTime;
            delayedCallbacksCount++;

            if (insertIndex == 0) {
                // Start the timer
                startTimer(delayedCallbacksTimer, periodMs, nullptr);
            }
        }
        return ret;
    }

    bool cancelDelayedCallback(DelayedCallback callback, void* param) { 
        bool ret = false;
        for (int i = 0; i < delayedCallbacksCount; ++i) {
            if (delayedCallbacks[i].callback == callback && delayedCallbacks[i].param == param) {
                // Found the item to remove
                if (i == 0) {
                    stopTimer(delayedCallbacksTimer);
                    if (delayedCallbacksCount > 1) {
                        int nextMs = delayedCallbacks[1].callbackTime - delayedCallbacks[0].callbackTime;
                        // Start the timer
                        startTimer(delayedCallbacksTimer, nextMs, nullptr);
                    }
                }
                for (int j = i; j < delayedCallbacksCount - 1; ++j) {
                    delayedCallbacks[j] = delayedCallbacks[j+1];
                }
                delayedCallbacksCount--;
                ret = true;
                break;
            }
        }
        return ret;
    }

    void pauseDelayedCallbacks() {
        if (delayedCallbackPauseRequestCount == 0) {
            // Cancel current timer, if any
            if (delayedCallbacksCount > 0) {
                NRF_LOG_INFO("Pausing delayed callbacks");
                stopTimer(delayedCallbacksTimer);
            }
        }
        delayedCallbackPauseRequestCount++;
    }

    void resumeDelayedCallbacks() {
        delayedCallbackPauseRequestCount--;
        if (delayedCallbackPauseRequestCount == 0) {
            // Resume current timer, if any
            if (delayedCallbacksCount > 0 && delayedCallbacks[0].callbackTime < millis()) {
                NRF_LOG_INFO("Resuming delayed callbacks");
                delayedCallbacksTimerCallback(nullptr); 
            }
        }
    }

    #if DICE_SELFTEST && TIMERS_SELFTEST

    #define TX_PIN 16
    #define RX_PIN 20

    APP_TIMER_DEF(ticTocTimer);
    void printTicToc(void* context) {
        static bool tic = true;
        if (tic) {
            NRF_LOG_INFO("tic");
            nrf_gpio_pin_set(TX_PIN);
        } else {
            NRF_LOG_INFO("toc");
            nrf_gpio_pin_clear(TX_PIN);
        }
        tic = !tic;
    }

    APP_TIMER_DEF(fastTimer);
    void fastBlink(void* context) {
        static bool tic = true;
        if (tic) {
            nrf_gpio_pin_set(RX_PIN);
        } else {
            nrf_gpio_pin_clear(RX_PIN);
        }
        tic = !tic;
    }

    void selfTest() {
        nrf_gpio_cfg_output(TX_PIN);
        nrf_gpio_cfg_output(RX_PIN);

        createTimer(&ticTocTimer, APP_TIMER_MODE_REPEATED, printTicToc);
        createTimer(&fastTimer, APP_TIMER_MODE_REPEATED, fastBlink);
        NRF_LOG_INFO("Creating timers, press any key to abort");
        Log::process();

        startTimer(ticTocTimer, 1000, nullptr);
        startTimer(fastTimer, 100, nullptr);
        while (!Log::hasKey()) {
            Log::process();
            PowerManager::update();
        }
		Log::getKey();
        NRF_LOG_INFO("Stopping timer!");
        stopTimer(ticTocTimer);
        stopTimer(fastTimer);
        nrf_gpio_cfg_input(TX_PIN, NRF_GPIO_PIN_NOPULL);
        nrf_gpio_cfg_input(RX_PIN, NRF_GPIO_PIN_NOPULL);
        Log::process();
    }
    #endif
}
}
