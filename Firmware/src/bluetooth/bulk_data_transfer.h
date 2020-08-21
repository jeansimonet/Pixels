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
		typedef void (*sendResultCallback)(void* context, bool result, const uint8_t* data, uint16_t size);
		void send(const uint8_t* theData, uint16_t theSize, void* context,   sendResultCallback callback);
		void selfTest();
	};

	namespace ReceiveBulkData
	{
		typedef uint8_t* (*receiveAllocator)(void* context, uint16_t size);
		typedef void (*receiveResultCallback)(void* context, bool result, uint8_t* data, uint16_t size);
		void receive(void* context, receiveAllocator allocator, receiveResultCallback callback);
		typedef void (*receiveToFlashResultCallback)(void* context, bool result, uint16_t size);
		void receiveToFlash(uint32_t flashAddress, void* context, receiveToFlashResultCallback callback);
		void selfTest();
	};
}
