#include "flash.h"
#include "nrf_log.h"
#include "nrf_fstorage.h"
#include "nrf_fstorage_sd.h"
#include "power_manager.h"
#include "app_error.h"

namespace DriversNRF
{
namespace Flash
{
static void fstorage_evt_handler(nrf_fstorage_evt_t * p_evt);

// NRF_FSTORAGE_DEF(nrf_fstorage_t fstorage) =
// {
//     /* Set a handler for fstorage events. */
//     .evt_handler = fstorage_evt_handler,

//     /* These below are the boundaries of the flash space assigned to this instance of fstorage.
//      * You must set these manually, even at runtime, before nrf_fstorage_init() is called.
//      * The function nrf5_flash_end_addr_get() can be used to retrieve the last address on the
//      * last page of flash available to write data. */
//     .start_addr = 0x3e000,
//     .end_addr   = 0x3ffff,
// };

    nrf_fstorage_t fstorage;

    void init() {
        fstorage.evt_handler = fstorage_evt_handler;
        fstorage.start_addr = 0x3e000;
        fstorage.end_addr   = 0x3ffff;
        ret_code_t rc = nrf_fstorage_init(&fstorage, &nrf_fstorage_sd, NULL);
        APP_ERROR_CHECK(rc);

        printFlashInfo();
    }

    static void fstorage_evt_handler(nrf_fstorage_evt_t * p_evt)
    {
        if (p_evt->result != NRF_SUCCESS)
        {
            NRF_LOG_INFO("--> Event received: ERROR while executing an fstorage operation.");
            return;
        }

        switch (p_evt->id)
        {
            case NRF_FSTORAGE_EVT_WRITE_RESULT:
            {
                NRF_LOG_INFO("--> Event received: wrote %d bytes at address 0x%x.",
                            p_evt->len, p_evt->addr);
            } break;

            case NRF_FSTORAGE_EVT_ERASE_RESULT:
            {
                NRF_LOG_INFO("--> Event received: erased %d page from address 0x%x.",
                            p_evt->len, p_evt->addr);
            } break;

            default:
                break;
        }
    }


    void printFlashInfo()
    {
        NRF_LOG_INFO("========| flash info |========");
        NRF_LOG_INFO("erase unit: \t%d bytes",      fstorage.p_flash_info->erase_unit);
        NRF_LOG_INFO("program unit: \t%d bytes",    fstorage.p_flash_info->program_unit);
        NRF_LOG_INFO("==============================");
    }


    void waitForFlashReady()
    {
        /* While fstorage is busy, sleep and wait for an event. */
        while (nrf_fstorage_is_busy(&fstorage))
        {
            // Sleep if necessary
            PowerManager::update();
        }
    }

    void write(uint32_t flashAddress, void* data, uint32_t size) {
        NRF_LOG_INFO("Writing \"%d\" bytes to flash.", size);
        ret_code_t rc = nrf_fstorage_write(&fstorage, flashAddress, data, size, NULL);
        APP_ERROR_CHECK(rc);

        waitForFlashReady();
        NRF_LOG_INFO("Done.");
    }

    void read(uint32_t flashAddress, void* outData, uint32_t size) {
        ret_code_t rc = nrf_fstorage_read(&fstorage, flashAddress, outData, size);
        APP_ERROR_CHECK(rc);
        waitForFlashReady();
    }

    void erase(uint32_t flashAddress, uint32_t pages) {
        ret_code_t rc = nrf_fstorage_erase(&fstorage, flashAddress, pages, NULL);
        APP_ERROR_CHECK(rc);
        waitForFlashReady();
    }}
}

