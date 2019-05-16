#pragma once

#include "stdint.h"
#include "stddef.h"

namespace Config
{
	struct Settings
	{
		// Indicates whether there is valid data
		uint32_t headMarker;
		char name[16];

		// Face detector
		float sigmaDecayStart;
		float sigmaDecayStop;
		float sigmaThresholdStart;
		float sigmaThresholdEnd;
		float faceThreshold;
		int minRollTime;
		// Indicates whether there is valid data
		uint32_t tailMarker;
	};

	namespace SettingsManager
	{
		void init();
		bool checkValid();
		Config::Settings const * const getSettings();

		void eraseSettings();
		void writeToFlash(Settings* sourceSettings);
		void writeToFlash(void* rawData, size_t rawDataSize);
		void setDefaults(Settings& outSettings);
		void programDefaults();
	}
}

