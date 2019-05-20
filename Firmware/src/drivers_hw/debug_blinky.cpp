#include "debug_blinky.h"
#include "nrf_gpio.h"

#define TX_PIN 20
#define RX_PIN 16

namespace DriversHW
{
namespace DebugBlinky
{
    void init() {
        nrf_gpio_cfg_output(TX_PIN);
        nrf_gpio_pin_clear(TX_PIN);

        nrf_gpio_cfg_output(RX_PIN);
        nrf_gpio_pin_clear(RX_PIN);
    }
}
}