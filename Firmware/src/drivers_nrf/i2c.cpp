#include "i2c.h"
#include "nrf_drv_twi.h"
#include "app_error.h"
#include "config/board_config.h"
#include "nrf_log.h"

namespace DriversNRF
{
namespace I2C
{
    /* TWI instance. */
    static const nrf_drv_twi_t m_twi = NRF_DRV_TWI_INSTANCE(0);

    void init()
    {
        auto board = Config::BoardManager::getBoard();
        const nrf_drv_twi_config_t twi_config = {
        .scl                = board->i2cClockPin,
        .sda                = board->i2cDataPin,
        .frequency          = NRF_DRV_TWI_FREQ_100K,
        .interrupt_priority = APP_IRQ_PRIORITY_HIGH,
        .clear_bus_init     = false
        };

        nrf_drv_twi_init(&m_twi, &twi_config, NULL, NULL);
        //APP_ERROR_CHECK(err_code);

        nrf_drv_twi_enable(&m_twi);

        NRF_LOG_INFO("I2C Initialized.");
    }

    bool write(uint8_t device, uint8_t value, bool no_stop)
    {
        return write(device, &value, 1, no_stop);
    }

    bool write(uint8_t device, const uint8_t* data, size_t size, bool no_stop)
    {
        auto err = nrf_drv_twi_tx(&m_twi, device, data, size, no_stop);
        return err == NRF_SUCCESS;
    }

    bool read(uint8_t device, uint8_t* value)
    {
        return read(device, value, 1);
    }

    bool read(uint8_t device, uint8_t* data, size_t size)
    {
        auto err = nrf_drv_twi_rx(&m_twi, device, data, size);
        return err == NRF_SUCCESS;
    }
}
}




