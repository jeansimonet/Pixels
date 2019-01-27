// BulkDataTransfer.h

#ifndef _BULKDATATRANSFER_h
#define _BULKDATATRANSFER_h

#include "arduino.h"

enum BulkDataState
{
	BulkDataState_Idle,
	BulkDataState_Transferring,
	BulkDataState_Complete,
	BulkDataState_Failed
};

/// <summary>
/// This class defines a small state machine that can send bulk data
/// over bluetooth to the phone.
/// This is used for instance to transfer animation data from the die to the phone.
/// </summary>
class SendBulkDataSM
{
private:
	// The buffer we want to send over and its size
	const byte* data;
	short size;

	enum State
	{
		State_Setup = 0,
		State_SendingSetupMsg,
		State_WaitingForSetupAck,
		State_SetupAckReceived,
		State_WaitingForDataAck,
		State_DataAckReceived,
		State_TransferComplete,
		State_Timeout,
		State_Done
	};

	State currentState;
	short currentOffset;
	int retryStart;
	int timeoutStart;

public:
	SendBulkDataSM();
	void Setup(const byte* theData, short theSize);
	void Update();
	BulkDataState GetState() const;
	void Finish();
};


/// <summary>
/// This class defines a small state machine that can receive a buffer
/// of arbitrary data and make it available for a client.
/// This is used for instance to transfer animation data from the phone to the die.
/// </summary>
class ReceiveBulkDataSM
{
public:
	// The data we received, which we malloc
	// Make sure to call Finish() to free the memory once you're done!
	byte * mallocData;
	short mallocSize;

private:
	enum State
	{
		State_WaitingForSetup = 0,
		State_SetupReceived,
		State_SendingSetupAck,
		State_WaitingForFirstData,
		State_WaitingForData,
		State_DataReceived,
		State_TransferComplete,// Data is ready!
		State_Timeout,
		State_Done,
	};

	State currentState;
	short currentOffset;
	short lastOffset;
	int retryStart;
	int timeoutStart;

public:
	ReceiveBulkDataSM();
	void Setup();
	void Update();
	BulkDataState GetState() const;
	void Finish();
};


#endif

