#include "flash.h"
#include "nrf_sdh.h"
#include "nrf_log.h"
#include "nrf_fstorage.h"
#include "nrf_fstorage_sd.h"
#include "power_manager.h"
#include "app_error.h"
#include "app_error_weak.h"
#include "log.h"
#include "nrf_soc.h"
#include "scheduler.h"
#include "core/delegate_array.h"
#include "config/settings.h"
#include "data_set/data_set.h"
#include "data_set/data_set_data.h"
#include "behaviors/behavior.h"

using namespace DriversNRF;
using namespace Config;
using namespace DataSet;
using namespace Behaviors;

#define MAX_ACC_CLIENTS 8

namespace DriversNRF
{
namespace Flash
{
    static void fstorage_evt_handler(nrf_fstorage_evt_t * p_evt);

    NRF_FSTORAGE_DEF(nrf_fstorage_t fstorage);

    FlashCallback callback; // This should be allocated per call...
    void* context;

	DelegateArray<ProgrammingEventMethod, MAX_ACC_CLIENTS> programmingClients;


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

        NRF_LOG_INFO("Flash Initialized - Address range: 0x%08x - 0x%08x", fstorage.start_addr, fstorage.end_addr);
        NRF_LOG_INFO(" - %d bytes available for user data", getUsableBytes());
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
            auto callbackCopy = callback;
            callback = nullptr;
            callbackCopy(context, info.result, info.address, info.size);
        } else {
            NRF_LOG_INFO("No callback");
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

    void write(void* theContext, uint32_t flashAddress, const void* data, uint32_t size, FlashCallback theCallback) {
        callback = theCallback;
        context = theContext;
        ret_code_t rc = nrf_fstorage_write(&fstorage, flashAddress, data, size, NULL);
        APP_ERROR_CHECK(rc);
    }

    void read(void* theContext, uint32_t flashAddress, void* outData, uint32_t size, FlashCallback theCallback) {
        callback = theCallback;
        context = theContext;
        ret_code_t rc = nrf_fstorage_read(&fstorage, flashAddress, outData, size);
        APP_ERROR_CHECK(rc);
    }

    void erase(void* theContext, uint32_t flashAddress, uint32_t pages, FlashCallback theCallback) {
        callback = theCallback;
        context = theContext;
        ret_code_t rc = nrf_fstorage_erase(&fstorage, flashAddress, pages, NULL);
        APP_ERROR_CHECK(rc);
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

    uint32_t getUsableBytes() {
        return fstorage.end_addr + 1 - fstorage.start_addr;
    }

    uint32_t getPageSize() {
        return fstorage.p_flash_info->erase_unit;
    }

	uint32_t getFlashByteSize(uint32_t totalDataByteSize) {
		auto pageSize = Flash::getPageSize();
		return pageSize * ((totalDataByteSize + pageSize - 1) / pageSize);
	}


	bool programFlash(
		const Data& newData,
		const Settings& newSettings,
		ProgramFlashFunc programFlashFunc,
		ProgramFlashNotification onProgramFinished) {

		static Data _newData __attribute__ ((aligned (4)));

        // Hack so we don't try to construct a new Settings in static initialization block
        static char _newSettingsBuffer[sizeof(Settings)]  __attribute__ ((aligned (4)));
		static Settings& _newSettings = *((Settings*)_newSettingsBuffer);
		static ProgramFlashFunc _programDataFunc;
		static ProgramFlashNotification _onProgramFinished;

		static auto beginProgramming = []() {
			// Notify clients
			for (int i = 0; i < programmingClients.Count(); ++i)
			{
				programmingClients[i].handler(programmingClients[i].token, ProgrammingEventType_Begin);
			}
		};

		static auto finishProgramming = []() {
			// Notify clients
			for (int i = 0; i < programmingClients.Count(); ++i)
			{
				programmingClients[i].handler(programmingClients[i].token, ProgrammingEventType_End);
			}
		};

        _newData = newData;
        _newSettings = newSettings;
        _programDataFunc = programFlashFunc;
        _onProgramFinished = onProgramFinished;

		uint32_t bufferSize = DataSet::computeDataSetDataSize(&_newData);
		if (availableDataSize() > bufferSize) {
			beginProgramming();

			uint32_t totalSize = bufferSize + sizeof(Data) + sizeof(Settings);
			uint32_t flashSize = Flash::getFlashByteSize(totalSize);
			uint32_t pageAddress = Flash::getFlashStartAddress();
			uint32_t pageCount = Flash::bytesToPages(flashSize);

			// Start by erasing the flash
			Flash::erase(nullptr, pageAddress, pageCount, [](void* context, bool result, uint32_t address, uint16_t data_size) {
				NRF_LOG_INFO("done Erasing %d page", data_size);
				if (result) {
					// Program settings
					Flash::write(nullptr, getSettingsStartAddress(), &_newSettings, sizeof(Settings), [](void* context, bool result, uint32_t address, uint16_t data_size) {
						if (result) {
							NRF_LOG_INFO("Finished flashing settings, flashing dataset data");
							// Receive all the buffers directly to flash
							_programDataFunc([](void* context, bool result, uint32_t address, uint16_t data_size) {
								if (result) {
									// Program the animation set itself
    								NRF_LOG_INFO("Finished flashing dataset data, flashing dataset itself");
									Flash::write(nullptr, getDataSetAddress(), &_newData, sizeof(Data),
										[](void* context, bool result, uint32_t address, uint16_t data_size) {
											if (result) {
												NRF_LOG_INFO("Data Set written to flash!");
											} else {
												NRF_LOG_ERROR("Error programming dataset to flash");
											}
											_onProgramFinished(result);
											finishProgramming();
									});
								} else {
									NRF_LOG_ERROR("Error transfering animation data");
									_onProgramFinished(false);
									finishProgramming();
								}
							});
						} else {
							NRF_LOG_ERROR("Error writing settings");
							_onProgramFinished(false);
							finishProgramming();
						}
					});
				} else {
					NRF_LOG_ERROR("Error erasing flash");
					_onProgramFinished(false);
					finishProgramming();
				}
			});
            return true;
		} else {
            return false;
		}
	}

	uint32_t getDataSetAddress() {
		return getSettingsEndAddress();
	}

	uint32_t getDataSetDataAddress() {
		return getDataSetAddress() + sizeof(Data);
	}

	uint32_t getSettingsStartAddress() {
		return (uint32_t)Flash::getFlashStartAddress();
	}
	uint32_t getSettingsEndAddress() {
		return getSettingsStartAddress() + sizeof(Settings);
	}


	void hookProgrammingEvent(ProgrammingEventMethod client, void* param)
	{
		if (!programmingClients.Register(param, client))
		{
			NRF_LOG_ERROR("Too many hooks registered.");
		}
	}

	void unhookProgrammingEvent(ProgrammingEventMethod client)
	{
		programmingClients.UnregisterWithHandler(client);
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

