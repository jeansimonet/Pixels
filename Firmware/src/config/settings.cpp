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

#define SETTINGS_ADDRESS 0x28000
#define SETTINGS_VALID_KEY (0x05E77165) // 0SETTINGS in leet speak ;)
#define SETTINGS_PAGE_COUNT 1

using namespace DriversNRF;
using namespace Bluetooth;
using namespace Config;

namespace Config
{
namespace SettingsManager
{
	Settings const * const settings = (Settings const * const)SETTINGS_ADDRESS;

	void ReceiveSettingsHandler(void* context, const Message* msg);
	void init() {
		programDefaults();
		if (!checkValid()) {
			NRF_LOG_WARNING("Settings not found in flash, programming defaults");
			programDefaults();
		}

		// Register as a handler to program settings
		MessageService::RegisterMessageHandler(Message::MessageType_TransferSettings, nullptr, ReceiveSettingsHandler);

		NRF_LOG_INFO("Settings initialized");
	}

	bool checkValid() {
		return (settings->headMarker == SETTINGS_VALID_KEY &&
			settings->tailMarker == SETTINGS_VALID_KEY);
	}

	Settings const * const getSettings() {
		if (!checkValid()) {
			programDefaults();
		}
		return settings;
	}

	void ReceiveSettingsHandler(void* context, const Message* msg) {

		NRF_LOG_INFO("Received Request to download new settings");

		// Start by erasing the flash
		Flash::erase(SETTINGS_ADDRESS, 1,
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

	void writeToFlash(Settings* sourceSettings) {
		Settings* dest = (Settings*)SETTINGS_ADDRESS;
		Flash::write((uint32_t)&(dest), sourceSettings, sizeof(Settings),
			[](bool result, uint32_t address, uint16_t size) {
				NRF_LOG_INFO("Settings written to flash");
		});
	}

	void setDefaults(Settings& outSettings) {
		outSettings.headMarker = SETTINGS_VALID_KEY;
		outSettings.sigmaDecayFast = 0.15f;
		outSettings.sigmaDecaySlow = 0.95f;
		outSettings.minRollTime = 300;

		// Copy normals from defaults
		const Core::float3* defaultNormals = BoardManager::getBoard()->faceNormals;
		for (int i = 0; i < BoardManager::getBoard()->ledCount; ++i) {
			outSettings.faceNormals[i] = defaultNormals[i];
		}
		outSettings.tailMarker = SETTINGS_VALID_KEY;
	}

	void programDefaults() {
		Flash::eraseSynchronous(SETTINGS_ADDRESS, SETTINGS_PAGE_COUNT);
		Settings defaults;
		setDefaults(defaults);
		Flash::writeSynchronous(SETTINGS_ADDRESS, &defaults, sizeof(Settings));
		NRF_LOG_INFO("Settings written to flash");
	}

	void programNormals(const Core::float3* newNormals, int count) {

		// Grab current settings
		Settings settingsCopy;
		memcpy(&settingsCopy, &settings, sizeof(Settings));

		// Change normals
		memcpy(&settingsCopy.faceNormals, newNormals, count * sizeof(Core::float3));

		// Reprogram settings
		Flash::eraseSynchronous(SETTINGS_ADDRESS, SETTINGS_PAGE_COUNT);
		Flash::writeSynchronous(SETTINGS_ADDRESS, &settingsCopy, sizeof(Settings));
		NRF_LOG_INFO("Settings written to flash with new noramls");
	}
}
}

