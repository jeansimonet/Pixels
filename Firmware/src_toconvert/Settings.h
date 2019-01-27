// DiceSettings.h

#ifndef _DICESETTINGS_h
#define _DICESETTINGS_h

#include "Arduino.h"
#include "BulkDataTransfer.h"

// Per readme file here: https://github.com/blieber/arduino-flash-queue
// Many examples in Simblee docs say up to 251 is available, but CAUTION - if use OTA 
// unfortunately this also uses some address space. According to 
// http://forum.rfduino.com/index.php?topic=1347.0 240-251 are used by the OTA bootloader. 
// Hence to be safe even in this example we only write up to 239.
#define HIGHEST_FLASH_PAGE (235)

#define SETTINGS_PAGE HIGHEST_FLASH_PAGE
#define SETTINGS_VALID_KEY (0x05E77165) // 0SETTINGS in leet speak ;)
#define SETTINGS_ADDRESS (SETTINGS_PAGE * 1024)

class Settings
{
public:
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

	uint32_t tailMarker;

	bool CheckValid() const;

	static bool EraseSettings();
	static bool TransferSettings(Settings* sourceSettings);
	static bool TransferSettingsRaw(void* rawData, size_t rawDataSize);
	static void SetDefaults(Settings& outSettings);
	static bool ProgramDefaults();
};

extern const Settings* settings;

/// <summary>
/// This defines a state machine that can manage receiving the dice settings over bluetooth
/// and then update them in flash
/// </summary>
class ReceiveSettingsSM
{
private:
	enum State
	{
		State_ErasingFlash = 0,
		State_SendingAck,
		State_TransferSettings,
		State_Failed,
		State_Done
	};

	State currentState;
	ReceiveBulkDataSM receiveBulkDataSM;

	typedef void(*FinishedCallback)(void* token);
	FinishedCallback FinishedCallbackHandler;
	void* FinishedCallbackToken;

private:
	void Finish();

public:
	ReceiveSettingsSM();
	void Setup(void* token, FinishedCallback handler);
	void Update();
};

/// <summary>
/// This defines a state machine that can send the current settings over
/// bluetooth to the phone. Typically so the phone can edit it and redownload it.
/// </summary>
class SendSettingsSM
{
private:
	enum State
	{
		State_SendingSetup,
		State_WaitingForSetupAck,
		State_SetupAckReceived,
		State_SendingSettings,
		State_Failed,
		State_Done
	};

	State currentState;

	// Temporarily stores animation pointers as we program them in flash
	SendBulkDataSM sendBulkDataSM;

	typedef void(*FinishedCallback)(void* token);
	FinishedCallback FinishedCallbackHandler;
	void* FinishedCallbackToken;

private:
	void Finish();

public:
	SendSettingsSM();
	void Setup(void* token, FinishedCallback handler);
	void Update();
};

#endif

