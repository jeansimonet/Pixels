// BluetoothMessage.h

#ifndef _BLUETOOTHMESSAGE_h
#define _BLUETOOTHMESSAGE_h

#include "Arduino.h"

#pragma pack(push, 1)

/// <summary>
///  Base class for messages from the die to the app
/// </summary>
struct DieMessage
{
	enum MessageType : byte
	{
		MessageType_None = 0,
		MessageType_State,
		MessageType_Telemetry,
		MessageType_BulkSetup,
		MessageType_BulkSetupAck,
		MessageType_BulkData,
		MessageType_BulkDataAck,
		MessageType_TransferAnimSet,
		MessageType_TransferAnimSetAck,
		MessageType_TransferAnimReadyForNextAnim,
		MessageType_TransferSettings,
		MessageType_TransferSettingsAck,
		MessageType_DebugLog,

		MessageType_PlayAnim,
		MessageType_RequestState,
		MessageType_RequestAnimSet,
		MessageType_RequestSettings,
		MessageType_RequestTelemetry,
		MessateType_ProgramDefaultAnimSet,
		MessateType_ProgramDefaultAnimSetFinished,
		MessageType_Rename,
		MessageType_RenameFinished,
		MessageType_Flash,
		MessageType_FlashFinished,
		MessageType_RequestDefaultAnimSetColor,
		MessageType_DefaultAnimSetColor,
		MessageType_Count
	};

	MessageType type;

	inline DieMessage(MessageType msgType) : type(msgType) {}
	static const char* GetMessageTypeString(MessageType msgType);

protected:
	inline DieMessage() : type(MessageType_None) {}
};

/// <summary>
/// Describes a face up detection message
/// </summary>
struct DieMessageState
	: public DieMessage
{
	byte state;

	inline DieMessageState() : DieMessage(DieMessage::MessageType_State) {}
};

/// <summary>
/// Describes an acceleration readings message (for telemetry)
/// </summary>
struct DieMessageAcc
	: public DieMessage
{
	struct AccelFrame
	{
		int16_t x;
		int16_t y;
		int16_t z;
		int16_t deltaTime;
	};

	AccelFrame data[2];

	inline DieMessageAcc() : DieMessage(DieMessage::MessageType_Telemetry) {}
};

struct DieMessageBulkSetup
	: DieMessage
{
	short size;

	inline DieMessageBulkSetup() : DieMessage(DieMessage::MessageType_BulkSetup) {}
};

struct DieMessageBulkData
	: DieMessage
{
	byte size;
	short offset;
	byte data[16];

	inline DieMessageBulkData() : DieMessage(DieMessage::MessageType_BulkData) {}
};

struct DieMessageBulkDataAck
	: DieMessage
{
	short offset;
	inline DieMessageBulkDataAck() : DieMessage(DieMessage::MessageType_BulkDataAck) {}
};

struct DieMessageTransferAnimSet
	: DieMessage
{
	byte count;
	short totalAnimationByteSize;

	inline DieMessageTransferAnimSet() : DieMessage(DieMessage::MessageType_TransferAnimSet) {}
};

struct DieMessageDebugLog
	: public DieMessage
{
	char text[19];

	inline DieMessageDebugLog() : DieMessage(DieMessage::MessageType_DebugLog) {}
};

struct DieMessagePlayAnim
	: public DieMessage
{
	byte animation;

	inline DieMessagePlayAnim() : DieMessage(DieMessage::MessageType_PlayAnim) {}
};

struct DieMessageRequestTelemetry
	: public DieMessage
{
	byte telemetry;

	inline DieMessageRequestTelemetry() : DieMessage(DieMessage::MessageType_RequestTelemetry) {}
};

struct DieMessageProgramDefaultAnimSet
	: public DieMessage
{
	uint32_t color;

	inline DieMessageProgramDefaultAnimSet() : DieMessage(DieMessage::MessateType_ProgramDefaultAnimSet) {}
};

struct DieMessageRename
	: public DieMessage
{
	char newName[16];

	inline DieMessageRename() : DieMessage(DieMessage::MessageType_Rename) {}
};


struct DieMessageFlash
	: public DieMessage
{
	byte animIndex;

	inline DieMessageFlash() : DieMessage(DieMessage::MessageType_Flash) {}
};

struct DieMessageDefaultAnimSetColor
	: public DieMessage
{
	uint32_t color;
	inline DieMessageDefaultAnimSetColor() : DieMessage(DieMessage::MessageType_DefaultAnimSetColor) {}
};
#pragma pack(pop)

#endif

