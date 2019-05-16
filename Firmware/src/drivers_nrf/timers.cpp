#include "timers.h"
#include "power_manager.h"
#include "app_error.h"
#include "Log.h"
#include "nrf_delay.h"
#include "nrf_gpio.h"
#include "nrf_drv_clock.h"

namespace DriversNRF
{
namespace Timers
{
    void init() {
        ret_code_t err_code = nrf_drv_clock_init();
        APP_ERROR_CHECK(err_code);

        err_code = app_timer_init();
        APP_ERROR_CHECK(err_code);

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
        nrf_drv_clock_lfclk_request(NULL);
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
