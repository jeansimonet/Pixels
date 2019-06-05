#pragma once

#include "stdint.h"

namespace Bluetooth
{
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
	namespace SendBulkData
	{
		typedef void (*sendResultCallback)(bool result);
		void send(const uint8_t* theData, short theSize, sendResultCallback callback);
		void selfTest();
	};

	namespace ReceiveBulkData
	{
		typedef void (*receiveResultCallback)(bool result, uint8_t* data, short size);
		void receive(receiveResultCallback callback);
		void selfTest();
	};
}
