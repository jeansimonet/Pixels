#pragma once

#include "stdint.h"
#include "stddef.h"
#include "core/float3.h"

#define MAX_LED_COUNT 21

namespace Config
{
	enum D20Version : uint8_t
	{
		D20Version_Default = 0,
		D20Version_Black, // Was cast in the wrong orientation so indices are all swapped
		D20Version_White, // Was cast in the wrong orientation so indices are all swapped
	};

	struct Settings
	{
		// Indicates whether there is valid data
		uint32_t headMarker;

		// Face detector
		float jerkClamp;
		float sigmaDecay;
		float startMovingThreshold;
		float stopMovingThreshold;
		float faceThreshold;
		float fallingThreshold;
		float shockThreshold;
		float accDecay;
		float heatUpRate;
		float coolDownRate;
		int minRollTime; // ms

		// Battery
		float batteryLow;
		float batteryHigh;

		D20Version d20Version;
		uint8_t filler1;
		uint8_t filler2;
		uint8_t filler3;

		// Calibration data
		Core::float3 faceNormals[MAX_LED_COUNT];
		
		// Indicates whether there is valid data
		uint32_t tailMarker;
	};

	namespace SettingsManager
	{
		typedef void (*SettingsWrittenCallback)(bool success);

		void init(SettingsWrittenCallback callback);
		bool checkValid();
		uint32_t getSettingsStartAddress();
		uint32_t getSettingsEndAddress();
		Config::Settings const * const getSettings();

		void writeToFlash(Settings* sourceSettings, SettingsWrittenCallback callback);
		void setDefaults(Settings& outSettings);
		void programDefaults(SettingsWrittenCallback callback);
		void programDefaultParameters(SettingsWrittenCallback callback);
		void programNormals(const Core::float3* newNormals, int count, SettingsWrittenCallback callback);
	}
}

