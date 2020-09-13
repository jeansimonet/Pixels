#pragma once

#include <stdint.h>
#include "config/sdk_config.h"
#include "config/dice_variants.h"
#include "modules/accelerometer.h"

#define MAX_DATA_SIZE 100
#define VERSION_INFO_SIZE 6

#pragma pack(push, 1)

namespace Bluetooth
{
/// <summary>
///  Base class for messages from the die to the app
/// </summary>
struct Message
{
	enum MessageType : uint8_t
	{
		MessageType_None = 0,
		MessageType_WhoAreYou,
		MessageType_IAmADie,
		MessageType_State,
		MessageType_Telemetry,
		MessageType_BulkSetup,
		MessageType_BulkSetupAck,
		MessageType_BulkData,
		MessageType_BulkDataAck,
		MessageType_TransferAnimSet,
		MessageType_TransferAnimSetAck,
		MessageType_TransferAnimSetFinished,
		MessageType_TransferSettings,
		MessageType_TransferSettingsAck,
		MessageType_TransferSettingsFinished,
		MessageType_TransferTestAnimSet,
		MessageType_TransferTestAnimSetAck,
		MessageType_TransferTestAnimSetFinished,
		MessageType_DebugLog,

		MessageType_PlayAnim,
		MessageType_PlayAnimEvent,
		MessageType_StopAnim,
		MessageType_PlaySound,
		MessageType_RequestState,
		MessageType_RequestAnimSet,
		MessageType_RequestSettings,
		MessageType_RequestTelemetry,
		MessateType_ProgramDefaultAnimSet,
		MessateType_ProgramDefaultAnimSetFinished,
		MessageType_Flash,
		MessageType_FlashFinished,
		MessageType_RequestDefaultAnimSetColor,
		MessageType_DefaultAnimSetColor,
		MessageType_RequestBatteryLevel,
		MessageType_BatteryLevel,
		MessageType_RequestRssi,
		MessageType_Rssi,
		MessageType_Calibrate,
		MessageType_CalibrateFace,
		MessageType_NotifyUser,
		MessageType_NotifyUserAck,
		MessageType_TestHardware,
		MessageType_SetStandardState,
		MessageType_SetLEDAnimState,
		MessageType_SetBattleState,
		MessageType_ProgramDefaultParameters,
		MessageType_ProgramDefaultParametersFinished,
		MessageType_SetDesignAndColor,
		MessageType_SetDesignAndColorAck,
		MessageType_SetCurrentBehavior,
		MessageType_SetCurrentBehaviorAck,
		MessageType_SetName,
		MessageType_SetNameAck,

		// TESTING 
		MessageType_TestBulkSend, 
		MessageType_TestBulkReceive,
		MessageType_SetAllLEDsToColor,
		MessageType_AttractMode,
		MessageType_PrintNormals,
		MessageType_PrintA2DReadings,
		MessageType_LightUpFace,
		MessageType_SetLEDToColor,

		MessageType_Count
	};

	MessageType type;

	inline Message(MessageType msgType) : type(msgType) {}
	static const char* GetMessageTypeString(MessageType msgType);

protected:
	inline Message() : type(MessageType_None) {}
};


/// <summary>
/// Identifies the dice
/// </summary>
struct MessageIAmADie
: public Message
{
	uint8_t faceCount; // Which kind of dice this is
	Config::DiceVariants::DesignAndColor designAndColor; // Physical look
	uint8_t currentBehaviorIndex;
	uint32_t dataSetHash;
	uint32_t deviceId; // A unique identifier
	char versionInfo[VERSION_INFO_SIZE]; // Firmware version string, i.e. "10_05"
	inline MessageIAmADie() : Message(Message::MessageType_IAmADie) { versionInfo[0] = 0; }
};

/// <summary>
/// Describes a state change detection message
/// </summary>
struct MessageDieState
	: public Message
{
	uint8_t state;
	uint8_t face;

	inline MessageDieState() : Message(Message::MessageType_State) {}
};

/// <summary>
/// Describes an acceleration readings message (for telemetry)
/// </summary>
struct MessageAcc
	: public Message
{
	Modules::Accelerometer::AccelFrame data;

	inline MessageAcc() : Message(Message::MessageType_Telemetry) {}
};

struct MessageBulkSetup
	: Message
{
	uint16_t size;

	inline MessageBulkSetup() : Message(Message::MessageType_BulkSetup) {}
};

struct MessageBulkData
	: Message
{
	uint8_t size;
	uint16_t offset;
	uint8_t data[MAX_DATA_SIZE];

	inline MessageBulkData() : Message(Message::MessageType_BulkData) {}
};

struct MessageBulkDataAck
	: Message
{
	uint16_t offset;
	inline MessageBulkDataAck() : Message(Message::MessageType_BulkDataAck) {}
};

struct MessageTransferAnimSet
	: Message
{
	uint16_t paletteSize;
	uint16_t rgbKeyFrameCount;
	uint16_t rgbTrackCount;
	uint16_t keyFrameCount;
	uint16_t trackCount;
	uint16_t animationCount;
	uint16_t animationSize;
	uint16_t conditionCount;
	uint16_t conditionSize;
	uint16_t actionCount;
	uint16_t actionSize;
	uint16_t ruleCount;
	uint16_t behaviorCount;

	inline MessageTransferAnimSet() : Message(Message::MessageType_TransferAnimSet) {}
};

struct MessageTransferTestAnimSet
	: Message
{
	uint16_t paletteSize;
	uint16_t rgbKeyFrameCount;

	uint16_t rgbTrackCount;
	uint16_t keyFrameCount;

	uint16_t trackCount;
	uint16_t animationSize;

	uint32_t hash;

	inline MessageTransferTestAnimSet() : Message(Message::MessageType_TransferTestAnimSet) {}
};

enum TransferTestAnimSetAckType : uint8_t
{
	TransferTestAnimSetAck_Download = 0,
	TransferTestAnimSetAck_UpToDate,
	TransferTestAnimSetAck_NoMemory
};

struct MessageTransferTestAnimSetAck
	: Message
{
	TransferTestAnimSetAckType ackType;
	inline MessageTransferTestAnimSetAck() : Message(Message::MessageType_TransferTestAnimSetAck) {}
};

struct MessageDebugLog
	: public Message
{
	char text[MAX_DATA_SIZE];

	inline MessageDebugLog() : Message(Message::MessageType_DebugLog) {}
};

struct MessagePlayAnim
	: public Message
{
	uint8_t animation;
	uint8_t remapFace;  // Assumes that an animation was made for face 0
	uint8_t loop; 		// 1 == loop, 0 == once

	inline MessagePlayAnim() : Message(Message::MessageType_PlayAnim) {}
};

struct MessagePlaySound
	: public Message
{
	uint16_t clipId;

	inline MessagePlaySound() : Message(Message::MessageType_PlaySound) {}
};

struct MessagePlayAnimEvent
	: public Message
{
	uint8_t evt;
	uint8_t remapFace;
	uint8_t loop;

	inline MessagePlayAnimEvent() : Message(Message::MessageType_PlayAnimEvent) {}
};

struct MessageStopAnim
	: public Message
{
	uint8_t animation;
	uint8_t remapFace;  // Assumes that an animation was made for face 0

	inline MessageStopAnim() : Message(Message::MessageType_StopAnim) {}
};

struct MessageRequestTelemetry
	: public Message
{
	uint8_t telemetry;

	inline MessageRequestTelemetry() : Message(Message::MessageType_RequestTelemetry) {}
};

struct MessageProgramDefaultAnimSet
	: public Message
{
	uint32_t color;

	inline MessageProgramDefaultAnimSet() : Message(Message::MessateType_ProgramDefaultAnimSet) {}
};

struct MessageFlash
	: public Message
{
	uint8_t animIndex;

	inline MessageFlash() : Message(Message::MessageType_Flash) {}
};

struct MessageDefaultAnimSetColor
	: public Message
{
	uint32_t color;
	inline MessageDefaultAnimSetColor() : Message(Message::MessageType_DefaultAnimSetColor) {}
};

struct MessageSetAllLEDsToColor
: public Message
{
	uint32_t color;
	inline MessageSetAllLEDsToColor() : Message(Message::MessageType_SetAllLEDsToColor) {}
};

struct MessageBatteryLevel
: public Message
{
	float level;
	float voltage;
	inline MessageBatteryLevel() : Message(Message::MessageType_BatteryLevel) {}
};

struct MessageRssi
: public Message
{
	int16_t rssi;
	inline MessageRssi() : Message(Message::MessageType_Rssi) {}
};

struct MessageSetDesignAndColor
: public Message
{
	Config::DiceVariants::DesignAndColor designAndColor;
	inline MessageSetDesignAndColor() : Message(Message::MessageType_SetDesignAndColor) {}
};

struct MessageSetCurrentBehavior
: public Message
{
	uint8_t currentBehavior;
	inline MessageSetCurrentBehavior() : Message(Message::MessageType_SetCurrentBehavior) {}
};

struct MessageSetName
: public Message
{
	char name[10];
	inline MessageSetName() : Message(Message::MessageType_SetName) {}
};

struct MessageNotifyUser
: public Message
{
	uint8_t timeout_s;
	uint8_t ok; // Boolean
	uint8_t cancel; // Boolean
	char text[MAX_DATA_SIZE - 4];
	inline MessageNotifyUser() : Message(Message::MessageType_NotifyUser) {
		timeout_s = 30;
		ok = 1;
		cancel = 0;
		text[0] = '\0';
	}
};

struct MessageNotifyUserAck
: public Message
{
	uint8_t okCancel; // Boolean
	inline MessageNotifyUserAck() : Message(Message::MessageType_NotifyUserAck) {}
};

struct MessageCalibrateFace
: public Message
{
	uint8_t face;
	inline MessageCalibrateFace() : Message(MessageType_CalibrateFace) {}
};

struct MessagePrintNormals
: public Message
{
	uint8_t face;
	inline MessagePrintNormals() : Message(MessageType_PrintNormals) {}
};

struct MessageLightUpFace
: public Message
{
	uint8_t face; // face to light up
	uint8_t opt_remapFace; // "up" face, 0 is default (no remapping), 0xFF to use current up face
	uint8_t opt_layoutIndex; // layout index override, 0xFF to use index stored in settings
	uint8_t opt_remapRot; // internal rotation index, 0 is default (no remapping), 0xFF to use index stored in settings
	uint32_t color;

	// For reference, the transformation is:
	// animFaceIndex
	//	-> rotatedOutsideAnimFaceIndex (based on remapFace and remapping table, i.e. what actual face should light up to "retarget" the animation around the current up face)
	//		-> rotatedInsideFaceIndex (based on internal pcb rotation, i.e. what face the electronics should light up to account for the fact that the pcb is probably rotated inside the dice)
	//			-> ledIndex (based on pcb face to led mapping, i.e. to account for the fact that the LEDs are not accessed in the same order as the number of the faces)

	inline MessageLightUpFace() : Message(MessageType_LightUpFace) {}
};


struct MessageSetLEDToColor
: public Message
{
	uint8_t ledIndex; // Starts at 0
	uint32_t color;
	inline MessageSetLEDToColor() : Message(Message::MessageType_SetLEDToColor) {}
};


}

#pragma pack(pop)
