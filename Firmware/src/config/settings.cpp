#include "settings.h"
#include "drivers_nrf/flash.h"
#include "nrf_log.h"
#include "app_error.h"

#define SETTINGS_ADDRESS 0x3F000
#define SETTINGS_VALID_KEY (0x05E77165) // 0SETTINGS in leet speak ;)
#define SETTINGS_PAGE_COUNT 1 // <-- FIXME!!!

using namespace DriversNRF;

namespace Config
{
namespace SettingsManager
{
	Settings const * const settings = (Settings const * const)SETTINGS_ADDRESS;

	void init() {
		if (!checkValid()) {
			programDefaults();
		}
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
		char* sourceRaw = (char*)sourceSettings;
		return writeToFlash(sourceRaw + sizeof(uint32_t), sizeof(Settings) - 2 * sizeof(uint32_t));
	}

	void writeToFlash(void* rawData, size_t rawDataSize) {
		uint32_t expected = sizeof(Settings) - 2 * sizeof(uint32_t);
		if (rawDataSize == expected) {
			uint32_t dest = SETTINGS_ADDRESS;
			uint32_t key = SETTINGS_VALID_KEY;
			Flash::write(dest, &key, sizeof(key));
			dest += sizeof(key);
			Flash::write(dest, rawData, rawDataSize);
			dest += sizeof(rawDataSize);
			Flash::write(dest, &key, sizeof(key));
		} else {
			NRF_LOG_ERROR("Wrong Settings size, could not program flash");
		}
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

