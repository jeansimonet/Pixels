#include "BulkDataTransfer.h"
#include "Debug.h"
#include "BluetoothMessage.h"
#include "Die.h"

#define RETRY_MS (100) // ms
#define TIMEOUT_MS (3000) // ms
#define BLOCK_SIZE (16)

/// <summary>
/// Constructor
/// </summary>
SendBulkDataSM::SendBulkDataSM()
	: data(nullptr)
	, size(0)
	, currentState(State_Done)
	, currentOffset(0)
	, retryStart(0)
{
}

/// <summary>
/// Prepare for a bulk data transfer
/// </summary>
/// <param name="theData">The data we want to send</param>
/// <param name="theSize">The size of the data</param>
void SendBulkDataSM::Setup(const byte* theData, short theSize)
{
	data = theData;
	size = theSize;
	currentState = State_Setup;
	currentOffset = 0;

	die.RegisterUpdate(this, [](void* token)
	{
		((SendBulkDataSM*)token)->Update();
	});
}

/// <summary>
/// State machine update method
/// </summary>
void SendBulkDataSM::Update()
{
	switch (currentState)
	{
	case State_Setup:
		// Register for SetupAck message
		die.RegisterMessageHandler(DieMessage::MessageType_BulkSetupAck, this, [](void* token, DieMessage* msg)
		{
			((SendBulkDataSM*)token)->currentState = State_SetupAckReceived;
		});

		// Send setup message
		currentState = State_SendingSetupMsg;
		timeoutStart = millis();
		// voluntary fall-through
	case State_SendingSetupMsg:
		{
			// Send setup message over
			DieMessageBulkSetup setupMsg;
			setupMsg.size = size;

			// (we register for a response first to be sure and not miss the ack)
			if (die.SendMessage(&setupMsg, sizeof(setupMsg)))
			{
				currentState = State_WaitingForSetupAck;
				// Start a timeout timer
				retryStart = millis();
			}
			// Else we try again next update!
		}
		break;
	case State_WaitingForSetupAck:
		{
			if (millis() > timeoutStart + TIMEOUT_MS)
			{
				// Tried a bunch, no response,
				die.UnregisterMessageHandler(DieMessage::MessageType_BulkSetupAck);
				currentState = State_Timeout;
			}
			else if (millis() > retryStart + RETRY_MS)
			{
				// Timed out, send the setup again
				currentState = State_SendingSetupMsg;
			}
			// Else continue waiting for ack!
		}
		break;
	case State_SetupAckReceived:
		// Unregister from setupack
		die.UnregisterMessageHandler(DieMessage::MessageType_BulkSetupAck);
		// Register for data ack
		die.RegisterMessageHandler(DieMessage::MessageType_BulkDataAck, this, [](void* token, DieMessage* msg)
		{
			auto sbd = (SendBulkDataSM*)token;
			auto ack = (DieMessageBulkDataAck*)msg;
			if (ack->offset == sbd->currentOffset)
			{
				// Good, move onto the next chunk
				sbd->currentState = State_DataAckReceived;
				sbd->currentOffset += BLOCK_SIZE;
				sbd->timeoutStart = millis();
			}
			// Else ignore this ack, we've probably already gotten it!
		});

		timeoutStart = millis();
		// Voluntary fall through
	case State_DataAckReceived:
		{
			short remainingSize = min(size - currentOffset, BLOCK_SIZE);
			if (remainingSize > 0)
			{
				// Send next packet
				DieMessageBulkData dataMsg;
				dataMsg.size = min(size - currentOffset, BLOCK_SIZE);
				dataMsg.offset = currentOffset;
				memcpy(dataMsg.data, &data[currentOffset], dataMsg.size);

				// (we register for a response first to be sure and not miss the ack)
				if (die.SendMessage(&dataMsg, sizeof(dataMsg)))
				{
					currentState = State_WaitingForDataAck;
					// Start a timeout timer
					retryStart = millis();
				}
				// Else we try again next update
			}
			else
			{
				// We're done!
				die.UnregisterMessageHandler(DieMessage::MessageType_BulkDataAck);
				currentState = State_TransferComplete;
			}
		}
		break;
	case State_WaitingForDataAck:
		if (millis() > timeoutStart + TIMEOUT_MS)
		{
			// Tried a bunch, no response,
			die.UnregisterMessageHandler(DieMessage::MessageType_BulkDataAck);
			currentState = State_Timeout;
		}
		else if (millis() > retryStart + RETRY_MS)
		{
			// Timed out, send the data chunk again
			currentState = State_DataAckReceived;
		}
		// Else continue waiting for ack!
	default:
		// Nothing
		break;
	}
}

/// <summary>
/// Are we done?
/// </summary>
BulkDataState SendBulkDataSM::GetState() const
{
	switch (currentState)
	{
	case State_Done:
		return BulkDataState_Idle;
	case State_TransferComplete:
		return BulkDataState_Complete;
	case State_Timeout:
		return BulkDataState_Failed;
	default:
		return BulkDataState_Transferring;
	}
}

/// <summary>
/// Clean up!
/// </summary>
void SendBulkDataSM::Finish()
{
	currentOffset = 0;
	currentState = State_Done;
	die.UnregisterUpdateToken(this);
}


//-----------------------------------------------------------------------------

/// <summary>
/// Constructor
/// </summary>
ReceiveBulkDataSM::ReceiveBulkDataSM()
	: mallocData(nullptr)
	, mallocSize(0)
	, currentState(State_Done)
	, currentOffset(0)
	, lastOffset(0)
	, retryStart(0)
	, timeoutStart(0)
{
}

/// <summary>
/// Prepare to send bulk data to the phone
/// </summary>
void ReceiveBulkDataSM::Setup()
{
	if (mallocData != nullptr)
	{
		Finish();
	}
	mallocData = nullptr;
	mallocSize = 0;
	currentState = State_WaitingForSetup;
	currentOffset = 0;
	lastOffset = 0;
	retryStart = 0;
	timeoutStart = 0;

	// Register handler
	die.RegisterMessageHandler(DieMessage::MessageType_BulkSetup, this, [](void* token, DieMessage* msg)
	{
		auto bulkSetupMsg = static_cast<DieMessageBulkSetup*>(msg);
		auto me = ((ReceiveBulkDataSM*)token);
		me->currentState = State_SetupReceived;
		me->mallocSize = bulkSetupMsg->size;
		me->mallocData = (byte*)malloc(bulkSetupMsg->size);
		if (me->mallocData == nullptr)
		{
			debugPrint("Malloc failed on ");
			debugPrint(bulkSetupMsg->size);
			debugPrintln(" bytes");
		}
		else
		{
			debugPrint("Receiving bulk data of ");
			debugPrint(me->mallocSize);
			debugPrintln(" bytes");
		}
	});

	die.RegisterUpdate(this, [](void* token)
	{
		((ReceiveBulkDataSM*)token)->Update();
	});
}

/// <summary>
/// State machine update
/// </summary>
void ReceiveBulkDataSM::Update()
{
	switch (currentState)
	{
	case State_SetupReceived:
		// Unregister setup handler
		die.UnregisterMessageHandler(DieMessage::MessageType_BulkSetup);

		// Register handler for data
		die.RegisterMessageHandler(DieMessage::MessageType_BulkData, this, [](void* token, DieMessage* msg)
		{
			auto bulkDataMsg = static_cast<DieMessageBulkData*>(msg);
			auto me = ((ReceiveBulkDataSM*)token);
			if (bulkDataMsg->offset == me->currentOffset)
			{
				memcpy(&me->mallocData[bulkDataMsg->offset], bulkDataMsg->data, bulkDataMsg->size);
				me->currentState = State_DataReceived;
				me->currentOffset += bulkDataMsg->size;
				me->timeoutStart = millis();
			}
		});
		currentState = State_SendingSetupAck;
		timeoutStart = millis();
		// Voluntary fall-through
	case State_SendingSetupAck:
		{
			// Acknowledge the setup
			if (die.SendMessage(DieMessage::MessageType_BulkSetupAck))
			{
				currentState = State_WaitingForFirstData;

				// Start a timeout
				retryStart = millis();
			}
			// Else we try again next update
		}
		break;
	case State_WaitingForFirstData:
		if (millis() > timeoutStart + TIMEOUT_MS)
		{
			die.UnregisterMessageHandler(DieMessage::MessageType_BulkData);
			currentState = State_Timeout;
		}
		else if (millis() > retryStart + RETRY_MS)
		{
			// Send ack again
			currentState = State_SendingSetupAck;
		}
		// Else keep waiting for data
		break;
	case State_DataReceived:
		{
			DieMessageBulkDataAck ackMsg;
			ackMsg.offset = lastOffset;
			if (die.SendMessage(&ackMsg, sizeof(ackMsg)))
			{
				lastOffset = currentOffset;
				if (lastOffset == mallocSize)
				{
					// We're done
					die.UnregisterMessageHandler(DieMessage::MessageType_BulkData);
					currentState = State_TransferComplete;
				}
				else
				{
					currentState = State_WaitingForData;

					// Start a timeout
					retryStart = millis();
				}
			}
			// Else we try again next update
		}
		break;
	case State_WaitingForData:
		if (millis() > timeoutStart + TIMEOUT_MS)
		{
			die.UnregisterMessageHandler(DieMessage::MessageType_BulkData);
			currentState = State_Timeout;
		}
		else if (millis() > retryStart + RETRY_MS)
		{
			// Send ack again
			currentState = State_DataReceived;
		}
		// Else keep waiting for data
		break;
	default:
		break;
	}
}

/// <summary>
/// Are we done?
/// </summary>
BulkDataState ReceiveBulkDataSM::GetState() const
{
	switch (currentState)
	{
	case State_Done:
		return BulkDataState_Idle;
	case State_Timeout:
		return BulkDataState_Failed;
	case State_TransferComplete:
		return BulkDataState_Complete;
	default:
		return BulkDataState_Transferring;
	}
}

/// <summary>
/// Clean up!
/// </summary>
void ReceiveBulkDataSM::Finish()
{
	if (mallocData != nullptr)
	{
		free(mallocData);
		mallocData = nullptr;
	}
	mallocSize = 0;
	currentState = State_Done;

	die.UnregisterUpdateToken(this);
}

