#include "settings.h"
#include "drivers_nrf/flash.h"
#include "nrf_log.h"
#include "app_error.h"

#define SETTINGS_ADDRESS 0x23000
#define SETTINGS_VALID_KEY (0x05E77165) // 0SETTINGS in leet speak ;)
#define SETTINGS_PAGE_COUNT 1

using namespace DriversNRF;

namespace Config
{
namespace SettingsManager
{
	Settings const * const settings = (Settings const * const)SETTINGS_ADDRESS;

	void init() {
		// if (!checkValid()) {
		// 	NRF_LOG_WARNING("Settings not found in flash, programming defaults");
		// 	//programDefaults();
		// }
		NRF_LOG_INFO("Settings initialized");
	}

	bool checkValid() {
		NRF_LOG_DEBUG("Head: %08x", settings->headMarker);
		NRF_LOG_DEBUG("Tail: %08x", settings->tailMarker);
		return (settings->headMarker == SETTINGS_VALID_KEY &&
			settings->tailMarker == SETTINGS_VALID_KEY);
	}

	Settings const * const getSettings() {
		if (!checkValid()) {
			programDefaults();
		}
		return settings;
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
		outSettings.sigmaDecayStart = 0.95f;
		outSettings.sigmaDecayStop = 0.05f;
		outSettings.sigmaThresholdStart = 100;
		outSettings.sigmaThresholdEnd = 0.5;
		outSettings.faceThreshold = 0.85f;
		outSettings.minRollTime = 300;
		outSettings.tailMarker = SETTINGS_VALID_KEY;
	}

	void programDefaults() {
		Flash::erase(SETTINGS_ADDRESS, SETTINGS_PAGE_COUNT,
			[](bool result, uint32_t address, uint16_t size) {
				Settings defaults;
				setDefaults(defaults);
				writeToFlash(&defaults);
			});
	}
}
}

