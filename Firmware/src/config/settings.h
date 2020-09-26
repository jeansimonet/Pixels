#pragma once

#include "stdint.h"
#include "stddef.h"
#include "core/float3.h"
#include "dice_variants.h"

#define MAX_LED_COUNT 21

namespace Config
{
	struct Settings
	{
		// Indicates whether there is valid data
		uint32_t headMarker;
		int version;

		// Physical Appearance
		DiceVariants::DesignAndColor designAndColor;

		char name[10];

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

		// Battery
		float batteryLow;
		float batteryHigh;

		// Calibration data
		int faceLayoutLookupIndex;
		int padding0;
		Core::float3 faceNormals[MAX_LED_COUNT];
		uint8_t faceToLEDLookup[MAX_LED_COUNT];
		
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

		void writeToFlash(const Settings* sourceSettings, SettingsWrittenCallback callback);
		void setDefaults(Settings& outSettings);
		void programDefaults(SettingsWrittenCallback callback);
		void programDefaultParameters(SettingsWrittenCallback callback);
		void programCalibrationData(const Core::float3* newNormals, int faceLayoutLookupIndex, const uint8_t* newFaceToLEDLookup, int count, SettingsWrittenCallback callback);

		void programDesignAndColor(DiceVariants::DesignAndColor design, SettingsWrittenCallback callback);
		void programCurrentBehavior(uint8_t behaviorIndex, SettingsWrittenCallback callback);
		void programName(const char* newName, SettingsWrittenCallback callback);

		enum ProgrammingEventType
		{
			ProgrammingEventType_Begin = 0,
			ProgrammingEventType_End
		};

		typedef void (*ProgrammingEventMethod)(void* param, ProgrammingEventType evt);
		void hookProgrammingEvent(ProgrammingEventMethod client, void* param);
		void unhookProgrammingEvent(ProgrammingEventMethod client);
	}
}

