#include "BluetoothMessage.h"

const char* DieMessage::GetMessageTypeString(DieMessage::MessageType msgType)
{
	switch (msgType)
	{
	case MessageType_None:
		return "None";
	case MessageType_State:
		return "State";
	case MessageType_Telemetry:
		return "Telemetry";
	case MessageType_BulkSetup:
		return "BulkSetup";
	case MessageType_BulkSetupAck:
		return "BulkSetupAck";
	case MessageType_BulkData:
		return "BulkData";
	case MessageType_BulkDataAck:
		return "BulkDataAck";
	case MessageType_TransferAnimSet:
		return "TransferAnimSet";
	case MessageType_TransferAnimSetAck:
		return "TransferAnimSetAck";
	case MessageType_TransferAnimReadyForNextAnim:
		return "TransferAnimReadyForNextAnim";
	case MessageType_TransferSettings:
		return "TransferSettings";
	case MessageType_TransferSettingsAck:
		return "TransferSettingsAck";
	case MessageType_DebugLog:
		return "DebugLog";
	case MessageType_PlayAnim:
		return "PlayAnim";
	case MessageType_RequestState:
		return "RequestState";
	case MessageType_RequestAnimSet:
		return "RequestAnimSet";
	case MessageType_RequestSettings:
		return "RequestSettings";
	case MessageType_RequestTelemetry:
		return "RequestTelemetry";
	case MessateType_ProgramDefaultAnimSet:
		return "ProgramDefaultAnimSet";
	case MessageType_Rename:
		return "Rename";
	case MessageType_Flash:
		return "Flash";
	case MessageType_RequestDefaultAnimSetColor:
		return "RequestDefaultAnimSetColor";
	default:
		return "<missing>";
	}
}
