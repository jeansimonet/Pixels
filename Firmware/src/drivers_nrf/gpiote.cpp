#include "gpiote.h"
#include "nrf_drv_gpiote.h"
#include "app_error.h"
#include "nrf_log.h"

namespace DriversNRF
{
namespace GPIOTE
{
    void init()
    {
        ret_code_t err_code = nrf_drv_gpiote_init();
        APP_ERROR_CHECK(err_code);

        NRF_LOG_INFO("GPIOTE Initialized.");
    }

    void enableInterrupt(uint32_t pin, nrf_gpio_pin_pull_t pull, nrf_gpiote_polarity_t polarity, PinHandler handler) {
        nrf_drv_gpiote_in_config_t in_config;
        in_config.is_watcher = false;
        in_config.hi_accuracy = true;
        in_config.pull = pull;
        in_config.sense = polarity;
        in_config.skip_gpio_setup = false;

        ret_code_t err_code = nrf_drv_gpiote_in_init(pin, &in_config, handler);
        APP_ERROR_CHECK(err_code);

        nrf_drv_gpiote_in_event_enable(pin, true);
    }

    void disableInterrupt(uint32_t pin) {
        nrf_drv_gpiote_in_event_enable(pin, false);
    }
}
}




