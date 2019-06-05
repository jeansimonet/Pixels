#include "settings.h"
#include "drivers_nrf/flash.h"
#include "nrf_log.h"
#include "app_error.h"

#define SETTINGS_ADDRESS 0x27000
#define SETTINGS_VALID_KEY (0x05E77165) // 0SETTINGS in leet speak ;)
#define SETTINGS_PAGE_COUNT 1

using namespace DriversNRF;

namespace Config
{
namespace SettingsManager
{
	Settings const * const settings = (Settings const * const)SETTINGS_ADDRESS;

	void init() {
		if (!checkValid()) {
			NRF_LOG_WARNING("Settings not found in flash, programming defaults");
			programDefaults();
		}
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

	void eraseSettings() {
		Flash::erase(SETTINGS_ADDRESS, SETTINGS_PAGE_COUNT);
	}

	void writeToFlash(Settings* sourceSettings) {
		Settings* dest = (Settings*)SETTINGS_ADDRESS;
		uint32_t key = SETTINGS_VALID_KEY;
		Flash::write((uint32_t)&(dest->headMarker), &key, sizeof(key));
		Flash::write((uint32_t)&(dest->name), &(sourceSettings->name), sizeof(Settings) - 2 * sizeof(key));
		Flash::write((uint32_t)&(dest->tailMarker), &key, sizeof(key));
	}

	void setDefaults(Settings& outSettings) {
		outSettings.sigmaDecayStart = 0.95f;
		outSettings.sigmaDecayStop = 0.05f;
		outSettings.sigmaThresholdStart = 100;
		outSettings.sigmaThresholdEnd = 0.5;
		outSettings.faceThreshold = 0.85f;
		outSettings.minRollTime = 300;
	}

	void programDefaults() {
		Settings defaults;
		setDefaults(defaults);
		writeToFlash(&defaults);
	}
}
}

