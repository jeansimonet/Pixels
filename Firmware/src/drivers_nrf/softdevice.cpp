#include "softdevice.h"
#include "nrf_sdh.h"

namespace DriversNRF
{
namespace PowerManager
{
    void init() {
        ret_code_t err_code = sd_softdevice_enable(clock_source, softdevice_assertion_handler);
        APP_ERROR_CHECK(err_code);
    }
}
}

