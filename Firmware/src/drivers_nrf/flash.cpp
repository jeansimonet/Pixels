#include "flash.h"
#include "nrf_sdh.h"
#include "nrf_log.h"
#include "nrf_fstorage.h"
#include "nrf_fstorage_sd.h"
#include "power_manager.h"
#include "app_error.h"
#include "log.h"
#include "nrf_soc.h"
#include "scheduler.h"

using namespace DriversNRF;

namespace DriversNRF
{
namespace Flash
{
    static void fstorage_evt_handler(nrf_fstorage_evt_t * p_evt);

    NRF_FSTORAGE_DEF(nrf_fstorage_t fstorage);

    FlashCallback callback; // This should be allocated per call...

    /**@brief   Helper function to obtain the last address on the last page of the on-chip flash that
     *          can be used to write user data.
     */
    static uint32_t nrf5_flash_end_addr_get()
    {
        uint32_t const bootloader_addr = NRF_UICR->NRFFW[0];
        uint32_t const page_sz         = NRF_FICR->CODEPAGESIZE;
        uint32_t const code_sz         = NRF_FICR->CODESIZE;

        return (bootloader_addr != 0xFFFFFFFF ?
                bootloader_addr : (code_sz * page_sz));
    }

    void init() {

        /* Set a handler for fstorage events. */
        fstorage.evt_handler = fstorage_evt_handler;

        /* These below are the boundaries of the flash space assigned to this instance of fstorage.
        * You must set these manually, even at runtime, before nrf_fstorage_init() is called.
        * The function nrf5_flash_end_addr_get() can be used to retrieve the last address on the
        * last page of flash available to write data. */
        fstorage.start_addr = FSTORAGE_START;
        fstorage.end_addr   = nrf5_flash_end_addr_get();

        ret_code_t rc = nrf_fstorage_init(&fstorage, &nrf_fstorage_sd, NULL);
        APP_ERROR_CHECK(rc);

        size_t size = fstorage.end_addr + 1 - fstorage.start_addr;
        NRF_LOG_INFO("Flash Initialized - Address range: 0x%08x - 0x%08x", fstorage.start_addr, fstorage.end_addr);
        NRF_LOG_INFO(" - %d bytes available for user data", size);
        NRF_LOG_INFO(" - erase unit: \t%d bytes",      fstorage.p_flash_info->erase_unit);
        NRF_LOG_INFO(" - program unit: \t%d bytes",    fstorage.p_flash_info->program_unit);

        #if DICE_SELFTEST && FLASH_SELFTEST
        selfTest();
        #endif
    }

    struct FlashCallbackInfo
    {
        uint32_t address;
        uint32_t size;
        bool result;
    };

    static void fstorage_evt_handler(nrf_fstorage_evt_t * p_evt)
    {
        FlashCallbackInfo info = {
            address: p_evt->addr,
            size: p_evt->len,
            result: false };

        APP_ERROR_CHECK(p_evt->result);
        if (p_evt->result != NRF_SUCCESS)
        {
            NRF_LOG_ERROR("--> Event received: ERROR while executing an fstorage operation.");
            info.result = false;
        }
        else
        {
            info.result = true;
            switch (p_evt->id)
            {
                case NRF_FSTORAGE_EVT_WRITE_RESULT:
                {
                    NRF_LOG_DEBUG("--> Event received: wrote %d bytes at address 0x%x.",
                                p_evt->len, p_evt->addr);
                } break;

                case NRF_FSTORAGE_EVT_ERASE_RESULT:
                {
                    NRF_LOG_DEBUG("--> Event received: erased %d page from address 0x%x.",
                                p_evt->len, p_evt->addr);
                } break;

                case NRF_FSTORAGE_EVT_READ_RESULT:
                {
                    NRF_LOG_DEBUG("--> Event received: read %d bytes from address 0x%x.",
                                p_evt->len, p_evt->addr);
                } break;

                default:
                    break;
            }
        }
        
        if (callback != nullptr) {
            callback(info.result, info.address, info.size);
        } else {
            NRF_LOG_DEBUG("No callback");
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

    void write(uint32_t flashAddress, const void* data, uint32_t size, FlashCallback theCallback) {
        callback = theCallback;
        ret_code_t rc = nrf_fstorage_write(&fstorage, flashAddress, data, size, NULL);
        APP_ERROR_CHECK(rc);
    }

    void writeSynchronous(uint32_t flashAddress, const void* data, uint32_t size) {
        callback = nullptr;
        ret_code_t rc = nrf_fstorage_write(&fstorage, flashAddress, data, size, NULL);
        APP_ERROR_CHECK(rc);
        waitForFlashReady();
    }

    void read(uint32_t flashAddress, void* outData, uint32_t size, FlashCallback theCallback) {
        callback = theCallback;
        ret_code_t rc = nrf_fstorage_read(&fstorage, flashAddress, outData, size);
        APP_ERROR_CHECK(rc);
    }

    void erase(uint32_t flashAddress, uint32_t pages, FlashCallback theCallback) {
        callback = theCallback;
        ret_code_t rc = nrf_fstorage_erase(&fstorage, flashAddress, pages, NULL);
        APP_ERROR_CHECK(rc);
    }

    void eraseSynchronous(uint32_t flashAddress, uint32_t pages) {
        callback = nullptr;
        ret_code_t rc = nrf_fstorage_erase(&fstorage, flashAddress, pages, NULL);
        APP_ERROR_CHECK(rc);
        waitForFlashReady();
    }

    uint32_t bytesToPages(uint32_t size) {
        uint32_t pageSize = fstorage.p_flash_info->erase_unit;
        return (size + pageSize - 1) / pageSize;
    }

    uint32_t getFlashStartAddress() {
        return fstorage.start_addr;
    }

    uint32_t getFlashEndAddress() {
        return fstorage.end_addr;
    }

    uint32_t getPageSize() {
        return fstorage.p_flash_info->erase_unit;
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

