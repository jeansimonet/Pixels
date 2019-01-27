#include "Settings.h"
#include "Debug.h"
#include "Die.h"

const Settings* settings = (const Settings*)SETTINGS_ADDRESS;

bool Settings::CheckValid() const
{
	return headMarker == SETTINGS_VALID_KEY && tailMarker == SETTINGS_VALID_KEY;
}

bool Settings::EraseSettings()
{
	return flashPageErase(SETTINGS_PAGE) == 0;
}

bool Settings::TransferSettings(Settings* sourceSettings)
{
	char* sourceRaw = (char*)sourceSettings;
	return TransferSettingsRaw(sourceRaw + sizeof(uint32_t), sizeof(Settings) - 2 * sizeof(uint32_t));
}

bool Settings::TransferSettingsRaw(void* rawData, size_t rawDataSize)
{
	int res = 0;
	uint32_t expected = sizeof(Settings) - 2 * sizeof(uint32_t);
	if (rawDataSize == expected)
	{
		uint32_t* settingsRaw = (uint32_t*)SETTINGS_ADDRESS;
		res = flashWrite(settingsRaw, SETTINGS_VALID_KEY);
		if (res == 0)
		{
			settingsRaw += 1;
			res = flashWriteBlock(settingsRaw, rawData, rawDataSize);
			if (res == 0)
			{
				settingsRaw += rawDataSize / 4;
				res = flashWrite(settingsRaw, SETTINGS_VALID_KEY);
			}
		}
	}
	else
	{
		res = 4;
	}

	// Print error message if any
	switch (res)
	{
	case 1:
		debugPrint("Settings could not be written, reserved page");
		break;
	case 2:
		debugPrint("Settings could not be written, sketch page");
		break;
	case 4:
		debugPrint("Bad settings raw data size ");
		debugPrint(rawDataSize);
		debugPrint(", expected ");
		debugPrintln(expected);
		break;
	default:
		break;
	}
	return res == 0;
}

void Settings::SetDefaults(Settings& outSettings)
{
	outSettings.sigmaDecayStart = 0.95f;
	outSettings.sigmaDecayStop = 0.05f;
	outSettings.sigmaThresholdStart = 100;
	outSettings.sigmaThresholdEnd = 0.5;
	outSettings.faceThreshold = 0.85f;
	outSettings.minRollTime = 300;
}

bool Settings::ProgramDefaults()
{
	Settings defaults;
	SetDefaults(defaults);
	return TransferSettings(&defaults);
}


ReceiveSettingsSM::ReceiveSettingsSM()
	: currentState(State_Done)
	, FinishedCallbackHandler(nullptr)
	, FinishedCallbackToken(nullptr)
{
}

void ReceiveSettingsSM::Setup(void* token, FinishedCallback handler)
{
	currentState = State_ErasingFlash;
	if (!Settings::EraseSettings())
	{
		debugPrintln("Error erasing flash for settings");
		currentState = State_Done;
		return;
	}

	FinishedCallbackHandler = handler;
	FinishedCallbackToken = token;

	// Register for update so we can try to send ack messages
	die.RegisterUpdate(this, [](void* token)
	{
		((ReceiveSettingsSM*)token)->Update();
	});
	currentState = State_SendingAck;
}

void ReceiveSettingsSM::Update()
{
	switch (currentState)
	{
	case State_SendingAck:
		if (die.SendMessage(DieMessage::MessageType_TransferSettingsAck))
		{
			currentState = State_TransferSettings;
			receiveBulkDataSM.Setup();
		}
		// Else try again next frame
		break;
	case State_TransferSettings:
		switch (receiveBulkDataSM.GetState())
		{
		case BulkDataState_Complete:
			{
				// Write the data to flash!
				if (!Settings::TransferSettingsRaw(receiveBulkDataSM.mallocData, receiveBulkDataSM.mallocSize))
				{
					debugPrint("Error writting settings");
				}

				// And we're done!
				receiveBulkDataSM.Finish();
				Finish();
			}
			break;
		case BulkDataState_Failed:
			currentState = State_Failed;
			break;
		default:
			// Else keep waiting
			break;
		}
	default:
		break;
	}
}

void ReceiveSettingsSM::Finish()
{
	currentState = State_Done;

	if (FinishedCallbackHandler != nullptr)
	{
		FinishedCallbackHandler(FinishedCallbackToken);
		FinishedCallbackHandler = nullptr;
		FinishedCallbackToken = nullptr;
	}
}





SendSettingsSM::SendSettingsSM()
	: currentState(State_Done)
	, FinishedCallbackHandler(nullptr)
	, FinishedCallbackToken(nullptr)
{
}

void SendSettingsSM::Setup(void* token, FinishedCallback handler)
{
	if (settings->CheckValid())
	{
		currentState = State_SendingSetup;

		FinishedCallbackHandler = handler;
		FinishedCallbackToken = token;

		die.RegisterUpdate(this, [](void* token)
		{
			((SendSettingsSM*)token)->Update();
		});
	}
}

void SendSettingsSM::Update()
{
	switch (currentState)
	{
	case State_SendingSetup:
		if (die.SendMessage(DieMessage::MessageType_TransferSettings))
		{
			die.RegisterMessageHandler(DieMessage::MessageType_TransferSettingsAck, this, [](void* token, DieMessage* msg)
			{
				((SendSettingsSM*)token)->currentState = State_SetupAckReceived;
			});

			currentState = State_WaitingForSetupAck;
		}
		// Else try again next frame
		break;
	case SendSettingsSM::State_SetupAckReceived:
		// Unregister ack
		die.UnregisterMessageHandler(DieMessage::MessageType_TransferSettingsAck);

		// Start the bulk transfer
		sendBulkDataSM.Setup((const byte*)&(settings->name), sizeof(Settings) - sizeof(uint32_t) * 2);
		currentState = State_SendingSettings;
		break;
	case SendSettingsSM::State_SendingSettings:
		switch (sendBulkDataSM.GetState())
		{
		case BulkDataState_Complete:
			// We're done!
			sendBulkDataSM.Finish();
			Finish();
			break;
		case BulkDataState_Failed:
			currentState = State_Failed;
			break;
		default:
			// Else keep waiting
			break;
		}
		break;
	default:
		break;
	}
}

void SendSettingsSM::Finish()
{
	currentState = State_Done;

	if (FinishedCallbackHandler != nullptr)
	{
		FinishedCallbackHandler(FinishedCallbackToken);
		FinishedCallbackHandler = nullptr;
		FinishedCallbackToken = nullptr;
	}
}

