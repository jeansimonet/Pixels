#include "dfu.h"
#include "ble_dfu.h"
#include "power_manager.h"
#include "nrf_sdh.h"
#include "app_error.h"

static void buttonless_dfu_sdh_state_observer(nrf_sdh_state_evt_t state, void * p_context)
{
    if (state == NRF_SDH_EVT_STATE_DISABLED)
    {
        DriversNRF::PowerManager::goToSystemOff();
    }
}

namespace DriversNRF
{
namespace DFU
{
    /* nrf_sdh state observer. */
    NRF_SDH_STATE_OBSERVER(m_buttonless_dfu_state_obs, 0) =
    {
        .handler = buttonless_dfu_sdh_state_observer,
    };

    void init() {

        // Initialize the async SVCI interface to bootloader before any interrupts are enabled.
        ret_code_t err_code = ble_dfu_buttonless_async_svci_init();
        APP_ERROR_CHECK(err_code);

    }
}
}
