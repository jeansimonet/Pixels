#include "bulk_data_transfer.h"
#include "bluetooth_messages.h"
#include "bluetooth_message_service.h"
#include "drivers_nrf/timers.h"
#include "malloc.h"
#include "drivers_nrf/flash.h"

#define RETRY_MS (300) // ms
#define TIMEOUT_MS (3000) // ms
#define BLOCK_SIZE (MAX_DATA_SIZE)
#define MAX_RETRY_COUNT (5)

using namespace DriversNRF;

namespace Bluetooth
{
	namespace SendBulkData
	{
		// The buffer we want to send over and its size
		const uint8_t* data;
		uint16_t size;

		enum State
		{
			State_Init = 0,
			State_WaitingForSetupAck,
			State_WaitingForDataAck,
			State_Done
		};

		State currentState;
		uint16_t currentOffset;

		int retryCount;
		sendResultCallback callback;
		void* context;

		APP_TIMER_DEF(timeoutTimer);

		void sendSetupMessage() {
			NRF_LOG_DEBUG("Sending Setup Message");
			// Start the timeout timer before anything else
			Timers::startTimer(timeoutTimer, RETRY_MS, nullptr);

			// Then send the message
			MessageBulkSetup setupMsg;
			setupMsg.size = size;
			MessageService::SendMessage(&setupMsg);
		}

		void sendCurrentChunk() {
			NRF_LOG_DEBUG("Sending Chunk (offset: %d)", currentOffset);
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
		void send(const uint8_t* theData, uint16_t theSize, void* theContext, sendResultCallback theCallback)
		{
			data = theData;
			size = theSize;
			currentOffset = 0;
			retryCount = 0;
			callback = theCallback;
			context = theContext;

			currentState = State_Init;

			// Send setup message, and wait for setup ack, or timeout
			Timers::createTimer(&timeoutTimer, APP_TIMER_MODE_SINGLE_SHOT, [](void* context) {
				if (currentState == State_WaitingForSetupAck) {
					retryCount++;
					if (retryCount >= MAX_RETRY_COUNT) {
						// Fail!
						currentState = State_Done;
						MessageService::UnregisterMessageHandler(Message::MessageType_BulkSetupAck);
						callback(context, false, data, size);
					} else {
						// Try again...
						sendSetupMessage();
					}
				}
				// Else ignore
				});

			// We register for a response first to be sure and not miss the ack
			MessageService::RegisterMessageHandler(Message::MessageType_BulkSetupAck, nullptr, [](void* context, const Message* message) {
				NRF_LOG_DEBUG("Received Ack for Setup");
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
								callback(context, false, data, size);
							} else {
								// Try again
								sendCurrentChunk();
							}
						}
					});

					// We register for a response first to be sure and not miss the ack
					MessageService::RegisterMessageHandler(Message::MessageType_BulkDataAck, nullptr, [](void* context, const Message* message) {
						auto ack = (MessageBulkDataAck*)message;
						NRF_LOG_DEBUG("Received Ack for Chunk (offset: %d)", ack->offset);

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
								callback(context, true, data, size);
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

		void transferDone(void* context, bool result, const uint8_t* data, uint16_t size) {
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
			send(testData, 256, nullptr, transferDone);
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
		uint32_t flashAddress;
		uint16_t size;

		enum State
		{
			State_Init = 0,
			State_WaitingForSetup,
			State_WaitingForData,
			State_Done
		};

		State currentState;
		uint16_t currentOffset;

		int retryCount;
		receiveAllocator allocator;
		receiveResultCallback callback;
		receiveToFlashResultCallback flashCallback;
		void* context;

		#pragma pack(push, 4)
		uint8_t dataBuffer[132]; // data is 100 bytes so this should be enough
		#pragma pack(pop)

		APP_TIMER_DEF(timeoutTimer);

		void sendSetupAckMessage() {
			NRF_LOG_DEBUG("Sending Setup Ack Message");
			// Start the timeout timer before anything else
			Timers::startTimer(timeoutTimer, RETRY_MS, nullptr);

			// Then send the message
			MessageService::SendMessage(Message::MessageType_BulkSetupAck);
		}

		void sendBulkAckMessage(uint16_t offset) {
			NRF_LOG_DEBUG("Sending Bulk Ack Message");

			// Then send the message
			MessageBulkDataAck ackMsg;
			ackMsg.offset = offset;
			MessageService::SendMessage(&ackMsg);
		}


		/// <summary>
		/// Bulk data transfer
		/// </summary>
		void receive(void* theContext, receiveAllocator theAllocator, receiveResultCallback theCallback)
		{
			data = nullptr;
			size = 0;
			retryCount = 0;
			allocator = theAllocator;
			callback = theCallback;
			context = theContext;

			currentState = State_Init;

			// Wait for the setup message, or timeout
			Timers::createTimer(&timeoutTimer, APP_TIMER_MODE_SINGLE_SHOT, [](void* context) {
				if (currentState == State_Init) {
					// Fail!
					currentState = State_Done;
					MessageService::UnregisterMessageHandler(Message::MessageType_BulkSetup);
					callback(context, false, nullptr, 0);
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
					data = allocator(context, msg->size);
					size = msg->size;
					if (data == nullptr) {
						// Not enough memory
						currentState = State_Done;
						callback(context, false, nullptr, 0);
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
								callback(context, false, nullptr, 0);
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

						if (msg->offset + msg->size >= size) {
							// Done
							MessageService::UnregisterMessageHandler(Message::MessageType_BulkData);
							callback(context, true, data, size);
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

		/// <summary>
		/// Bulk data transfer directly to flash, note that the flash area must already be erased
		/// </summary>
		void receiveToFlash(uint32_t theFlashAddress, void* theContext, receiveToFlashResultCallback theCallback)
		{
			flashAddress = theFlashAddress;
			size = 0;
			retryCount = 0;
			flashCallback = theCallback;
			context = theContext;

			currentState = State_Init;

			// Wait for the setup message, or timeout
			Timers::createTimer(&timeoutTimer, APP_TIMER_MODE_SINGLE_SHOT,
				[](void* c) {
					if (currentState == State_Init) {
						// Fail!
						currentState = State_Done;
						MessageService::UnregisterMessageHandler(Message::MessageType_BulkSetup);
						flashCallback(context, false, 0);
					}
					// Else ignore
				}
			);

			// We register for the setup message
			MessageService::RegisterMessageHandler(Message::MessageType_BulkSetup, nullptr,
				[](void* c, const Message* message) {
					NRF_LOG_INFO("Received Bulk Setup");
					if (currentState == State_WaitingForSetup || currentState == State_WaitingForData) {

						// Cancel the timer first
						Timers::stopTimer(timeoutTimer);

						// Stop listening for setup
						MessageService::UnregisterMessageHandler(Message::MessageType_BulkSetup);

						auto msg = (const MessageBulkSetup*)message;
						size = msg->size;
						NRF_LOG_INFO("Transfer size: 0x%04x", size);
						currentState = State_WaitingForData;

						// Send Ack, and wait for data to come in, or timeout!
						Timers::createTimer(&timeoutTimer, APP_TIMER_MODE_SINGLE_SHOT,
							[](void* c) {
								if (currentState == State_WaitingForData) {
									retryCount++;
									if (retryCount >= MAX_RETRY_COUNT) {
										// Fail!
										currentState = State_Done;
										MessageService::UnregisterMessageHandler(Message::MessageType_BulkData);
										flashCallback(context, false, 0);
									} else {
										// Try again...
										sendSetupAckMessage();
									}
								}
								// Else ignore
							}
						);

						MessageService::RegisterMessageHandler(Message::MessageType_BulkData, nullptr,
							[](void* c, const Message* message) {

								// Cancel the timer first
								Timers::stopTimer(timeoutTimer);

								// Program the data
								auto msg = (const MessageBulkData*)message;
								NRF_LOG_DEBUG("Received Bulk Data (offset: 0x%04x, length: %d)", msg->offset, msg->size);

								// Copy the data to properly aligned buffer
								memcpy(dataBuffer, msg->data, msg->size);
								NRF_LOG_DEBUG("Writing data to flash at 0x%08x", flashAddress + msg->offset);

								// Round up the size of the data to write, which should be okay because the
								// temporary buffer is large enough
								uint32_t flashWriteSize = 4 * ((msg->size + 3) / 4);

								// Go ahead
								Flash::write(flashAddress + msg->offset, dataBuffer, flashWriteSize,
									[](bool result, uint32_t address, uint16_t s) {

										// And send an ack!
										uint16_t offset = (uint16_t)(address - flashAddress);
										sendBulkAckMessage(offset);

										// Are we done?
										if (offset + s >= size) {
											// Done
											NRF_LOG_DEBUG("Done!")
											MessageService::UnregisterMessageHandler(Message::MessageType_BulkData);
											if (flashCallback != nullptr) {
												flashCallback(context, true, size);
											}
										}
									}
								);
							}
						);

						// Send Setup ack
						sendSetupAckMessage();
					}
					// Else ignore this setup, we've probably already gotten it!
				}
			);

			currentState = State_WaitingForSetup;
		}

		#if DICE_SELFTEST && BULK_DATA_TRANSFER_SELFTEST
		void transferDone(void* context, bool result, uint8_t* data, uint16_t size) {
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
			receive(nullptr, transferDone);
		}

		void selfTest() {
			MessageService::RegisterMessageHandler(Message::MessageType_TestBulkReceive, nullptr, testBulkReceive);
		}
		#endif

	}
}
