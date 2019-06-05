#include "flash.h"
#include "nrf_sdh.h"
#include "nrf_log.h"
#include "nrf_fstorage.h"
#include "nrf_fstorage_sd.h"
#include "power_manager.h"
#include "app_error.h"
#include "log.h"
#include "nrf_soc.h"

namespace DriversNRF
{
namespace Flash
{
    static void fstorage_evt_handler(nrf_fstorage_evt_t * p_evt);

    NRF_FSTORAGE_DEF(nrf_fstorage_t fstorage);

    void init() {

        /* Set a handler for fstorage events. */
        fstorage.evt_handler = fstorage_evt_handler;

        /* These below are the boundaries of the flash space assigned to this instance of fstorage.
        * You must set these manually, even at runtime, before nrf_fstorage_init() is called.
        * The function nrf5_flash_end_addr_get() can be used to retrieve the last address on the
        * last page of flash available to write data. */
        fstorage.start_addr = 0x25000;
        fstorage.end_addr   = 0x27fff;

        ret_code_t rc = nrf_fstorage_init(&fstorage, &nrf_fstorage_sd, NULL);
        APP_ERROR_CHECK(rc);

        size_t size = fstorage.end_addr + 1 - fstorage.start_addr;
        NRF_LOG_INFO("Flash Initialized - %d bytes available for user data", size);

        #if DICE_SELFTEST && FLASH_SELFTEST
        selfTest();
        #endif
    }

    static void fstorage_evt_handler(nrf_fstorage_evt_t * p_evt)
    {
        if (p_evt->result != NRF_SUCCESS)
        {
            NRF_LOG_ERROR("--> Event received: ERROR while executing an fstorage operation.");
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

            case NRF_FSTORAGE_EVT_READ_RESULT:
            {
                NRF_LOG_INFO("--> Event received: read %d bytes from address 0x%x.",
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
            sd_app_evt_wait();
        }
    }

    void write(uint32_t flashAddress, void* data, uint32_t size) {
        ret_code_t rc = nrf_fstorage_write(&fstorage, flashAddress, data, size, NULL);
        APP_ERROR_CHECK(rc);
        waitForFlashReady();
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
    }

    uint32_t bytesToPages(uint32_t size) {
        uint32_t pageSize = fstorage.p_flash_info->erase_unit;
        return (size + pageSize - 1) / pageSize;
    }


    #if DICE_SELFTEST && FLASH_SELFTEST
    bool testing = false;
    bool dontShutDown(nrf_pwr_mgmt_evt_t event)
    {
        return !testing;
    }

    /**@brief Register application shutdown handler with priority 0. */
    NRF_PWR_MGMT_HANDLER_REGISTER(dontShutDown, 0);

    void selfTest() {
        testing = true;
        NRF_LOG_INFO("Erasing one page at %x", fstorage.start_addr);
        erase(fstorage.start_addr, 1);
        unsigned int pcheck1 = fstorage.start_addr;
        unsigned int check1 = 0xDEADBEEF;
        unsigned int pcheck2 = fstorage.start_addr + 0x100;
        unsigned int check2 = 0x55555555;
        NRF_LOG_INFO("Writing %x (addr: %x) to %x", check1, &check1, pcheck1);
        Log::process();
        write(pcheck1, &check1, sizeof(unsigned int));
        NRF_LOG_INFO("Writing %x (addr: %x) to %x", check2, &check2, pcheck2);
        Log::process();
        write(pcheck2, &check2, sizeof(unsigned int));
        NRF_LOG_INFO("Reading back values!");
        Log::process();
        unsigned int verify1 = 0xBAADF00D;
        unsigned int verify2 = 0xBAADF00D;
        read(pcheck1, &verify1, sizeof(unsigned int));
        read(pcheck2, &verify2, sizeof(unsigned int));
        bool success = verify1 == check1 && verify2 == check2;
        if (success) {
            NRF_LOG_INFO("Success: read back %x and %x", verify1, verify2);
        } else {
            NRF_LOG_WARNING("Error: read back %x and %x", verify1, verify2);
        }
        Log::process();
        testing = false;
        PowerManager::feed();
    }
    #endif
}
}

