#pragma once

#include "stdint.h"
#include "stddef.h"
#include "core/float3.h"

#define MAX_LED_COUNT 21

namespace Config
{
	struct Settings
	{
		// Indicates whether there is valid data
		uint32_t headMarker;

		// Face detector
		float sigmaDecayFast;
		float sigmaDecaySlow;
		int minRollTime; // ms

		// Battery
		float batteryLow;
		float batteryHigh;

		// Calibration data
		Core::float3 faceNormals[MAX_LED_COUNT];
		
		// Indicates whether there is valid data
		uint32_t tailMarker;
	};

	namespace SettingsManager
	{
		void init();
		bool checkValid();
		uint32_t getSettingsStartAddress();
		uint32_t getSettingsEndAddress();
		Config::Settings const * const getSettings();

		void writeToFlash(Settings* sourceSettings);
		void writeToFlash(void* rawData, size_t rawDataSize);
		void setDefaults(Settings& outSettings);
		void programDefaults();
		void programNormals(const Core::float3* newNormals, int count);
	}
}

