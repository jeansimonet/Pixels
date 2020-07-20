#include "settings.h"
#include "drivers_nrf/flash.h"
#include "nrf_log.h"
#include "app_error.h"
#include "config/board_config.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_stack.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bulk_data_transfer.h"
#include "malloc.h"
#include "config/dice_variants.h"
#include "utils/utils.h"

#define SETTINGS_VALID_KEY (0x15E77165) // 1SETTINGS in leet speak ;)
#define SETTINGS_VERSION 2
#define SETTINGS_PAGE_COUNT 1

using namespace DriversNRF;
using namespace Bluetooth;
using namespace Config;

namespace Config
{

namespace SettingsManager
{
	Settings const * settings = nullptr;

	void ReceiveSettingsHandler(void* context, const Message* msg);
	void ProgramDefaultParametersHandler(void* context, const Message* msg);
	
	#if BLE_LOG_ENABLED
	void PrintNormals(void* context, const Message* msg);
	#endif
	void init(SettingsWrittenCallback callback) {
		static SettingsWrittenCallback _callback; // Don't initialize this static inline because it would only do it on first call!
		_callback = callback;

		settings = (Settings const * const)Flash::getFlashStartAddress();

		auto finishInit = [](bool success) {
			// Register as a handler to program settings
			MessageService::RegisterMessageHandler(Message::MessageType_TransferSettings, nullptr, ReceiveSettingsHandler);
			MessageService::RegisterMessageHandler(Message::MessateType_ProgramDefaultParameters, nullptr, ProgramDefaultParametersHandler);
			
			#if BLE_LOG_ENABLED
			MessageService::RegisterMessageHandler(Message::MessageType_PrintNormals, nullptr, PrintNormals);
			#endif

			NRF_LOG_INFO("Settings initialized");
			auto callBackCopy = _callback;
			_callback = nullptr;
			if (callBackCopy != nullptr) {
				callBackCopy(success);
			}
		};

		if (!checkValid()) {
			NRF_LOG_WARNING("Settings not found in flash, programming defaults");
			programDefaults(finishInit);
		} else {
			finishInit(true);
		}
	}

	bool checkValid() {
		return (settings->headMarker == SETTINGS_VALID_KEY &&
			settings->version == SETTINGS_VERSION &&
			settings->tailMarker == SETTINGS_VALID_KEY);
	}

	uint32_t getSettingsStartAddress() {
		return (uint32_t)settings;
	}
	uint32_t getSettingsEndAddress() {
		return (uint32_t)settings + Flash::getPageSize() * SETTINGS_PAGE_COUNT;
	}


	Settings const * const getSettings() {
		if (!checkValid()) {
			return nullptr;
		} else {
			return settings;
		}
	}

	void ReceiveSettingsHandler(void* context, const Message* msg) {

		NRF_LOG_INFO("Received Request to download new settings");

		// Start by erasing the flash
		Flash::erase((uint32_t)settings, 1,
			[](bool result, uint32_t address, uint16_t size) {
				NRF_LOG_DEBUG("done Erasing %d page", size);

				// Send Ack and receive data
				MessageService::SendMessage(Message::MessageType_TransferSettingsAck);

				// Receive all the buffers directly to flash
				ReceiveBulkData::receiveToFlash((uint32_t)settings, nullptr,
					[](void* context, bool success, uint16_t size) {
						if (success) {
							NRF_LOG_DEBUG("Finished flashing settings");
							// Restart the bluetooth stack
							Stack::disconnect();
							Stack::stopAdvertising();
							Stack::startAdvertising();
						} else {
							NRF_LOG_ERROR("Error transfering animation data");
						}
					}
				);
			}
		);
	}

	void ProgramDefaultParametersHandler(void* context, const Message* msg) {
		programDefaultParameters([] (bool result) {
			// Ignore result for now
			Bluetooth::MessageService::SendMessage(Message::MessateType_ProgramDefaultParametersFinished);
		});
	}

	void writeToFlash(Settings* sourceSettings, SettingsWrittenCallback callback) {
		// Temporary holders used in callbacks
		static SettingsWrittenCallback _writeToFlashCallback;  // Don't initialize this static inline because it would only do it on first call!
		_writeToFlashCallback = callback;

		static Settings* _sourceSettings; // Don't initialize this static inline because it would only do it on first call!
		_sourceSettings = (Settings*)malloc(sizeof(Settings));
		memcpy(_sourceSettings, sourceSettings, sizeof(Settings));

		static auto finishWrite = [] (bool result) {
			free(_sourceSettings);
			_sourceSettings = nullptr;

			// Clear callback pointer before invoking it, in case the callback decides to trigger another write to flash!
			auto callbackCopy = _writeToFlashCallback;
			_writeToFlashCallback = nullptr;
			if (callbackCopy != nullptr) {
				callbackCopy(result);
			}
		};

		// Start by erasing the flash!
		Flash::erase((uint32_t)settings, SETTINGS_PAGE_COUNT, [] (bool result, uint32_t address, uint16_t size) {
			if (result) {
				Flash::write((uint32_t)settings, _sourceSettings, sizeof(Settings),
					[](bool result, uint32_t address, uint16_t size) {
						if (result) {
							NRF_LOG_INFO("Settings written to flash");
						} else {
							NRF_LOG_INFO("Error writting to flash");
						}
						finishWrite(result);
				});
			} else {
				NRF_LOG_ERROR("Error erasing flash");
				finishWrite(false);
			}
		});
	}

	void setDefaultParameters(Settings& outSettings) {
		outSettings.designAndColor = DiceVariants::DesignAndColor::DesignAndColor_Unknown;
		outSettings.jerkClamp = 10.f;
		outSettings.sigmaDecay = 0.5f;
		outSettings.startMovingThreshold = 5.0f;
		outSettings.stopMovingThreshold = 0.5f;
		outSettings.faceThreshold = 0.98f;
		outSettings.fallingThreshold = 0.1f;
		outSettings.shockThreshold = 7.5f;
		outSettings.batteryLow = 3.0f;
		outSettings.batteryHigh = 4.0f;
		outSettings.accDecay = 0.9f;
		outSettings.heatUpRate = 0.0004f;
		outSettings.coolDownRate = 0.995f;
	}

	void setDefaultCalibrationData(Settings& outSettings) {
		// Copy normals from defaults
		int ledCount = BoardManager::getBoard()->ledCount;
		const Core::float3* defaultNormals = Config::DiceVariants::getDefaultNormals(ledCount);
		const uint8_t* defaultLookup = Config::DiceVariants::getDefaultLookup(ledCount);
		for (int i = 0; i < ledCount; ++i) {
			outSettings.faceNormals[i] = defaultNormals[i];
			outSettings.faceToLEDLookup[i] = defaultLookup[i];
		}
		outSettings.faceLayoutLookupIndex = 0;
	}


	void setDefaults(Settings& outSettings) {
		outSettings.headMarker = SETTINGS_VALID_KEY;
		outSettings.version = SETTINGS_VERSION;
		setDefaultParameters(outSettings);
		setDefaultCalibrationData(outSettings);
		outSettings.tailMarker = SETTINGS_VALID_KEY;
	}

	void programDefaults(SettingsWrittenCallback callback) {
		Settings defaults;
		setDefaults(defaults);
		writeToFlash(&defaults, callback);
	}

	void programDefaultParameters(SettingsWrittenCallback callback) {

		// Grab current settings
		Settings settingsCopy;

		// Begin by resetting our new settings
		setDefaults(settingsCopy);

		// Copy over everything
		memcpy(&settingsCopy, settings, sizeof(Settings));

		// Change normals
		setDefaultParameters(settingsCopy);

		// Reprogram settings
		writeToFlash(&settingsCopy, callback);
	}

	void programCalibrationData(const Core::float3* newNormals, int faceLayoutLookupIndex, const uint8_t* newFaceToLEDLookup, int count, SettingsWrittenCallback callback) {

		// Grab current settings
		Settings settingsCopy;

		// Begin by resetting our new settings
		setDefaults(settingsCopy);

		// Copy over everything
		memcpy(&settingsCopy, settings, sizeof(Settings));

		// Change normals
		memcpy(&(settingsCopy.faceNormals[0]), newNormals, count * sizeof(Core::float3));

		// Change remapping
		settingsCopy.faceLayoutLookupIndex = faceLayoutLookupIndex;
		memcpy(settingsCopy.faceToLEDLookup, newFaceToLEDLookup, count * sizeof(uint8_t));

		// Reprogram settings
		NRF_LOG_INFO("Programming settings in flash");
		writeToFlash(&settingsCopy, callback);
	}

	#if BLE_LOG_ENABLED
	void PrintNormals(void* context, const Message* msg) {
		auto m = static_cast<const MessagePrintNormals*>(msg);
		int i = m->face;
		auto settings = getSettings();
		BLE_LOG_INFO("Face %d: %d, %d, %d", i, (int)(settings->faceNormals[i].x * 100), (int)(settings->faceNormals[i].y * 100), (int)(settings->faceNormals[i].z * 100));
	}
	#endif

}
}

