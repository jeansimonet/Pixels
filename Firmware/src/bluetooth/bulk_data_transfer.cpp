#include "bulk_data_transfer.h"
#include "bluetooth_messages.h"
#include "bluetooth_message_service.h"
#include "drivers_nrf/timers.h"
#include "malloc.h"

#define RETRY_MS (300) // ms
#define TIMEOUT_MS (3000) // ms
#define BLOCK_SIZE (16)
#define MAX_RETRY_COUNT (5)

using namespace DriversNRF;

namespace Bluetooth
{
	namespace SendBulkData
	{
		// The buffer we want to send over and its size
		const uint8_t* data;
		short size;

		enum State
		{
			State_Init = 0,
			State_WaitingForSetupAck,
			State_WaitingForDataAck,
			State_Done
		};

		State currentState;
		short currentOffset;

		int retryCount;
		sendResultCallback callback;

		APP_TIMER_DEF(timeoutTimer);

		void sendSetupMessage() {
			NRF_LOG_INFO("Sending Setup Message");
			// Start the timeout timer before anything else
			Timers::startTimer(timeoutTimer, RETRY_MS, nullptr);

			// Then send the message
			MessageBulkSetup setupMsg;
			setupMsg.size = size;
			MessageService::SendMessage(&setupMsg);
		}

		void sendCurrentChunk() {
			NRF_LOG_INFO("Sending Chunk (offset: %d)", currentOffset);
			// Start the timeout timer before anything else
			Timers::startTimer(timeoutTimer, RETRY_MS, nullptr);

			// Then send the data chunk
			MessageBulkData dataMsg;
			dataMsg.size = MIN(size - currentOffset, BLOCK_SIZE);
			dataMsg.offset = currentOffset;
			memcpy(dataMsg.data, &data[currentOffset], dataMsg.size);
			MessageService::SendMessage(&dataMsg);
		}

		/// <summary>
		/// Bulk data transfer
		/// </summary>
		void send(const uint8_t* theData, short theSize, sendResultCallback theCallback)
		{
			data = theData;
			size = theSize;
			currentOffset = 0;
			retryCount = 0;
			callback = theCallback;

			currentState = State_Init;

			// Send setup message, and wait for setup ack, or timeout
			Timers::createTimer(&timeoutTimer, APP_TIMER_MODE_SINGLE_SHOT, [](void* context) {
				if (currentState == State_WaitingForSetupAck) {
					retryCount++;
					if (retryCount >= MAX_RETRY_COUNT) {
						// Fail!
						currentState = State_Done;
						MessageService::UnregisterMessageHandler(Message::MessageType_BulkSetupAck);
						callback(false);
					} else {
						// Try again...
						sendSetupMessage();
					}
				}
				// Else ignore
				});

			// We register for a response first to be sure and not miss the ack
			MessageService::RegisterMessageHandler(Message::MessageType_BulkSetupAck, nullptr, [](void* context, const Message* message) {
				NRF_LOG_INFO("Received Ack for Setup");
				if (currentState == State_WaitingForSetupAck) {
					// Cancel the timer first
					Timers::stopTimer(timeoutTimer);

					// Stop listening for ack
					MessageService::UnregisterMessageHandler(Message::MessageType_BulkSetupAck);

					// Start sending data, wait for timeout or ack
					Timers::createTimer(&timeoutTimer, APP_TIMER_MODE_SINGLE_SHOT, [](void* context) {
						if (currentState == State_WaitingForDataAck) {
							retryCount++;
							if (retryCount >= MAX_RETRY_COUNT) {
								// Fail!
								currentState = State_Done;
								MessageService::UnregisterMessageHandler(Message::MessageType_BulkDataAck);
								callback(false);
							} else {
								// Try again
								sendCurrentChunk();
							}
						}
					});

					// We register for a response first to be sure and not miss the ack
					MessageService::RegisterMessageHandler(Message::MessageType_BulkDataAck, nullptr, [](void* context, const Message* message) {
						auto ack = (MessageBulkDataAck*)message;
						NRF_LOG_INFO("Received Ack for Chunk (offset: %d)", ack->offset);

						if (ack->offset == currentOffset)
						{
							// Cancel the timer first
							Timers::stopTimer(timeoutTimer);

							if (currentOffset + BLOCK_SIZE < size) {
								// Good, move onto the next chunk
								currentOffset += BLOCK_SIZE;
								sendCurrentChunk();
							} else {
								// Done!
								currentState = State_Done;
								MessageService::UnregisterMessageHandler(Message::MessageType_BulkDataAck);
								callback(true);
							}
						}
						// Else ignore this ack, we've probably already gotten it!
					});

					currentState = State_WaitingForDataAck;
					sendCurrentChunk();
				}
				// Else ignore this ack, we've probably already gotten it!
			});

			currentState = State_WaitingForSetupAck;
			sendSetupMessage();
		}

		#if DICE_SELFTEST && BULK_DATA_TRANSFER_SELFTEST
		uint8_t* testData = nullptr;

		void transferDone(bool result) {
			if (result) {
				NRF_LOG_INFO("Success");
			} else {
				NRF_LOG_INFO("Failed");
			}
			free(testData);
			MessageService::UnregisterMessageHandler(Message::MessageType_TestBulkSend);
		}

		void testBulkSend(void* token, const Message* msg) {
			NRF_LOG_INFO("Received Message to send Bulk Data");
			uint8_t* testData = (uint8_t*)malloc(256);
			for (int i = 0; i < 256; ++i) {
				testData[i] = i;
			}

			NRF_LOG_INFO("Sending 256 bytes over");
			send(testData, 256, transferDone);
		}

		void selfTest() {
			MessageService::RegisterMessageHandler(Message::MessageType_TestBulkSend, nullptr, testBulkSend);
		}
		#endif

	}

	namespace ReceiveBulkData
	{
		// The buffer we want to send over and its size
		uint8_t* data;
		short size;

		enum State
		{
			State_Init = 0,
			State_WaitingForSetup,
			State_WaitingForData,
			State_Done
		};

		State currentState;
		short currentOffset;

		int retryCount;
		receiveResultCallback callback;

		APP_TIMER_DEF(timeoutTimer);

		void sendSetupAckMessage() {
			NRF_LOG_INFO("Sending Setup Ack Message");
			// Start the timeout timer before anything else
			Timers::startTimer(timeoutTimer, RETRY_MS, nullptr);

			// Then send the message
			MessageService::SendMessage(Message::MessageType_BulkSetupAck);
		}

		void sendBulkAckMessage(uint16_t offset) {
			NRF_LOG_INFO("Sending Bulk Ack Message");
			// Then send the message
			MessageBulkDataAck ackMsg;
			ackMsg.offset = offset;
			MessageService::SendMessage(&ackMsg);
		}

		/// <summary>
		/// Bulk data transfer
		/// </summary>
		void receive(receiveResultCallback theCallback)
		{
			data = nullptr;
			size = 0;
			retryCount = 0;
			callback = theCallback;

			currentState = State_Init;

			// Wait for the setup message, or timeout
			Timers::createTimer(&timeoutTimer, APP_TIMER_MODE_SINGLE_SHOT, [](void* context) {
				if (currentState == State_Init) {
					// Fail!
					currentState = State_Done;
					MessageService::UnregisterMessageHandler(Message::MessageType_BulkSetup);
					callback(false, nullptr, 0);
				}
				// Else ignore
			});

			// We register for the setup message
			MessageService::RegisterMessageHandler(Message::MessageType_BulkSetup, nullptr, [](void* context, const Message* message) {
				NRF_LOG_INFO("Received Bulk Setup");
				if (currentState == State_WaitingForSetup || currentState == State_WaitingForData) {

					// Cancel the timer first
					Timers::stopTimer(timeoutTimer);

					// Stop listening for setup
					MessageService::UnregisterMessageHandler(Message::MessageType_BulkSetup);

					// Allocate memory for the data
					auto msg = (const MessageBulkSetup*)message;
					data = (uint8_t*)malloc(msg->size);
					size = msg->size;
					if (data == nullptr) {
						// Not enough memory
						currentState = State_Done;
						callback(false, nullptr, 0);
						return;
					}

					currentState = State_WaitingForData;

					// Send Ack, and wait for data to come in, or timeout!
					Timers::createTimer(&timeoutTimer, APP_TIMER_MODE_SINGLE_SHOT, [](void* context) {
						if (currentState == State_WaitingForData) {
							retryCount++;
							if (retryCount >= MAX_RETRY_COUNT) {
								// Fail!
								currentState = State_Done;
								MessageService::UnregisterMessageHandler(Message::MessageType_BulkData);
								callback(false, nullptr, 0);
							} else {
								// Try again...
								sendSetupAckMessage();
							}
						}
						// Else ignore
					});

					MessageService::RegisterMessageHandler(Message::MessageType_BulkData, nullptr, [](void* context, const Message* message) {

						// Cancel the timer first
						Timers::stopTimer(timeoutTimer);

						// Copy the data
						auto msg = (const MessageBulkData*)message;
						memcpy(&data[msg->offset], msg->data, msg->size);

						NRF_LOG_INFO("Received Bulk Data (offset: %d)", msg->offset);

						if (msg->offset + msg->size >= size) {
							// Done
							MessageService::UnregisterMessageHandler(Message::MessageType_BulkData);
							callback(true, data, size);
						}

						// And send an ack!
						sendBulkAckMessage(msg->offset);
					});

					// Send Setup ack
					sendSetupAckMessage();
				}
				// Else ignore this setup, we've probably already gotten it!
			});

			currentState = State_WaitingForSetup;
		}

		#if DICE_SELFTEST && BULK_DATA_TRANSFER_SELFTEST
		void transferDone(bool result, uint8_t* data, short size) {
			if (result) {
				NRF_LOG_INFO("Received Bulk Data");
				NRF_LOG_HEXDUMP_INFO(data, size);
			} else {
				NRF_LOG_INFO("Failed");
			}
			free(data);
			MessageService::UnregisterMessageHandler(Message::MessageType_TestBulkReceive);
		}

		void testBulkReceive(void* token, const Message* msg) {
			NRF_LOG_INFO("Received Message to prepare for Bulk Data");
			receive(transferDone);
		}

		void selfTest() {
			MessageService::RegisterMessageHandler(Message::MessageType_TestBulkReceive, nullptr, testBulkReceive);
		}
		#endif

	}

	// //-----------------------------------------------------------------------------

	// /// <summary>
	// /// Constructor
	// /// </summary>
	// ReceiveBulkDataSM::ReceiveBulkDataSM()
	// 	: mallocData(nullptr)
	// 	, mallocSize(0)
	// 	, currentState(State_Done)
	// 	, currentOffset(0)
	// 	, lastOffset(0)
	// 	, retryStart(0)
	// 	, timeoutStart(0)
	// {
	// }

	// /// <summary>
	// /// Prepare to send bulk data to the phone
	// /// </summary>
	// void ReceiveBulkDataSM::Setup()
	// {
	// 	if (mallocData != nullptr)
	// 	{
	// 		Finish();
	// 	}
	// 	mallocData = nullptr;
	// 	mallocSize = 0;
	// 	currentState = State_WaitingForSetup;
	// 	currentOffset = 0;
	// 	lastOffset = 0;
	// 	retryStart = 0;
	// 	timeoutStart = 0;

	// 	// Register handler
	// 	die.RegisterMessageHandler(DieMessage::MessageType_BulkSetup, this, [](void* token, DieMessage* msg)
	// 	{
	// 		auto bulkSetupMsg = static_cast<DieMessageBulkSetup*>(msg);
	// 		auto me = ((ReceiveBulkDataSM*)token);
	// 		me->currentState = State_SetupReceived;
	// 		me->mallocSize = bulkSetupMsg->size;
	// 		me->mallocData = (uint8_t*)malloc(bulkSetupMsg->size);
	// 		if (me->mallocData == nullptr)
	// 		{
	// 			debugPrint("Malloc failed on ");
	// 			debugPrint(bulkSetupMsg->size);
	// 			debugPrintln(" bytes");
	// 		}
	// 		else
	// 		{
	// 			debugPrint("Receiving bulk data of ");
	// 			debugPrint(me->mallocSize);
	// 			debugPrintln(" bytes");
	// 		}
	// 	});

	// 	die.RegisterUpdate(this, [](void* token)
	// 	{
	// 		((ReceiveBulkDataSM*)token)->Update();
	// 	});
	// }

	// /// <summary>
	// /// State machine update
	// /// </summary>
	// void ReceiveBulkDataSM::Update()
	// {
	// 	switch (currentState)
	// 	{
	// 	case State_SetupReceived:
	// 		// Unregister setup handler
	// 		die.UnregisterMessageHandler(DieMessage::MessageType_BulkSetup);

	// 		// Register handler for data
	// 		die.RegisterMessageHandler(DieMessage::MessageType_BulkData, this, [](void* token, DieMessage* msg)
	// 		{
	// 			auto bulkDataMsg = static_cast<DieMessageBulkData*>(msg);
	// 			auto me = ((ReceiveBulkDataSM*)token);
	// 			if (bulkDataMsg->offset == me->currentOffset)
	// 			{
	// 				memcpy(&me->mallocData[bulkDataMsg->offset], bulkDataMsg->data, bulkDataMsg->size);
	// 				me->currentState = State_DataReceived;
	// 				me->currentOffset += bulkDataMsg->size;
	// 				me->timeoutStart = millis();
	// 			}
	// 		});
	// 		currentState = State_SendingSetupAck;
	// 		timeoutStart = millis();
	// 		// Voluntary fall-through
	// 	case State_SendingSetupAck:
	// 		{
	// 			// Acknowledge the setup
	// 			if (die.SendMessage(DieMessage::MessageType_BulkSetupAck))
	// 			{
	// 				currentState = State_WaitingForFirstData;

	// 				// Start a timeout
	// 				retryStart = millis();
	// 			}
	// 			// Else we try again next update
	// 		}
	// 		break;
	// 	case State_WaitingForFirstData:
	// 		if (millis() > timeoutStart + TIMEOUT_MS)
	// 		{
	// 			die.UnregisterMessageHandler(DieMessage::MessageType_BulkData);
	// 			currentState = State_Timeout;
	// 		}
	// 		else if (millis() > retryStart + RETRY_MS)
	// 		{
	// 			// Send ack again
	// 			currentState = State_SendingSetupAck;
	// 		}
	// 		// Else keep waiting for data
	// 		break;
	// 	case State_DataReceived:
	// 		{
	// 			DieMessageBulkDataAck ackMsg;
	// 			ackMsg.offset = lastOffset;
	// 			if (die.SendMessage(&ackMsg, sizeof(ackMsg)))
	// 			{
	// 				lastOffset = currentOffset;
	// 				if (lastOffset == mallocSize)
	// 				{
	// 					// We're done
	// 					die.UnregisterMessageHandler(DieMessage::MessageType_BulkData);
	// 					currentState = State_TransferComplete;
	// 				}
	// 				else
	// 				{
	// 					currentState = State_WaitingForData;

	// 					// Start a timeout
	// 					retryStart = millis();
	// 				}
	// 			}
	// 			// Else we try again next update
	// 		}
	// 		break;
	// 	case State_WaitingForData:
	// 		if (millis() > timeoutStart + TIMEOUT_MS)
	// 		{
	// 			die.UnregisterMessageHandler(DieMessage::MessageType_BulkData);
	// 			currentState = State_Timeout;
	// 		}
	// 		else if (millis() > retryStart + RETRY_MS)
	// 		{
	// 			// Send ack again
	// 			currentState = State_DataReceived;
	// 		}
	// 		// Else keep waiting for data
	// 		break;
	// 	default:
	// 		break;
	// 	}
	// }

	// /// <summary>
	// /// Are we done?
	// /// </summary>
	// BulkDataState ReceiveBulkDataSM::GetState() const
	// {
	// 	switch (currentState)
	// 	{
	// 	case State_Done:
	// 		return BulkDataState_Idle;
	// 	case State_Timeout:
	// 		return BulkDataState_Failed;
	// 	case State_TransferComplete:
	// 		return BulkDataState_Complete;
	// 	default:
	// 		return BulkDataState_Transferring;
	// 	}
	// }

	// /// <summary>
	// /// Clean up!
	// /// </summary>
	// void ReceiveBulkDataSM::Finish()
	// {
	// 	if (mallocData != nullptr)
	// 	{
	// 		free(mallocData);
	// 		mallocData = nullptr;
	// 	}
	// 	mallocSize = 0;
	// 	currentState = State_Done;

	// 	die.UnregisterUpdateToken(this);
	// }

}
