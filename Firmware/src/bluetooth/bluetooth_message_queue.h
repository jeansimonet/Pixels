#pragma once

//#define DEBUG_MESSAGE_QUEUE

#include "nrf_nvic.h"

#if defined(DEBUG_MESSAGE_QUEUE)
#include "nrf_log.h"
#endif


namespace Bluetooth
{
    class Message;

	/// <summary>
	/// FIFO queue for bluetooth messages
	/// </summary>
	template <int Size>
	class MessageQueue
	{
        struct QueuedMessage {
            int size;
            int stride;
            uint8_t data[1]; // Real size is indicated by size
        };
		
        uint8_t data[Size];
		int count;
		int reader;
		int writer;

	public:
		/// <summary>
		/// Constructor
		/// </summary>
		MessageQueue()
			: count(0)
			, reader(0)
			, writer(0)
		{
		}

        static int computeStride(int msgSize) {
            return (sizeof(int) * 2 + msgSize + 3) & ~3;
        }

		/// <summary>
		/// Add a message to the queue
		/// Returns true if the message could be queued
		/// </summary>
		bool enqueue(const Message* msg, int msgSize)
		{
            uint8_t nested;
            sd_nvic_critical_region_enter(&nested);
            #if defined(DEBUG_MESSAGE_QUEUE)
            NRF_LOG_INFO("Before queueing");
            debugPrint();
            #endif
            bool ret = false;
            int msgStride = computeStride(msgSize);
            #if defined(DEBUG_MESSAGE_QUEUE)
            NRF_LOG_INFO("reader: %d, writer: %d, stride: %d", reader, writer, msgStride);
            #endif
            if (count == 0 || writer > reader) {
                // Buffer looks like this:
                // [...RMMMMW...]
                // Does it fit at the end?
                if (writer + msgStride <= Size) {

                    // Yes, add the message there
                    QueuedMessage* newMsg = reinterpret_cast<QueuedMessage*>(&data[writer]);
                    newMsg->size = msgSize;
                    newMsg->stride = msgStride;
                    memcpy(newMsg->data, msg, msgSize);

                    // Update the count and writer index
                    writer += msgStride;
                    count += 1;
                    ret = true;
                }
                // Nope, does it fit at the begining?
                else if (reader > msgSize) {
                    // Yes, add a dummy message
                    QueuedMessage* dummyMsg = reinterpret_cast<QueuedMessage*>(&data[writer]);
                    dummyMsg->size = 0;
                    dummyMsg->stride = Size - writer;

                    // And then add the new message at the begining
                    QueuedMessage* newMsg = reinterpret_cast<QueuedMessage*>(&data[0]);
                    newMsg->size = msgSize;
                    newMsg->stride = msgStride;
                    memcpy(newMsg->data, msg, msgSize);

                    // Update the count and writer index
                    writer = msgStride;
                    count += 1;
                    ret = true;
                }
                // Else nope, no room
            } else {
                // Buffer looks like this
                // [MMMW...RMMM]
                if (writer + msgStride <= reader) {
                    // There is room, add a message there!
                    QueuedMessage* newMsg = reinterpret_cast<QueuedMessage*>(&data[writer]);
                    newMsg->size = msgSize;
                    newMsg->stride = msgStride;
                    memcpy(newMsg->data, msg, msgSize);

                    // Update the count and writer index
                    writer += msgStride;
                    count += 1;
                    ret = true;
                }
                // else no room!
            }

            #if defined(DEBUG_MESSAGE_QUEUE)
            NRF_LOG_INFO("After queueing");
            debugPrint();
            #endif
            sd_nvic_critical_region_exit(nested);
            return ret;
		}

		/// <summary>
		/// returns a pointer to the next message
		/// </summary>
		const Message* peekNext(int& outMsgSize) {
			if (count > 0) {
                const QueuedMessage* msg = reinterpret_cast<const QueuedMessage*>(&data[reader]);
                if (outMsgSize == 0) {
                    msg = reinterpret_cast<const QueuedMessage*>(&data[0]);
                }
                outMsgSize = msg->size;
                return reinterpret_cast<const Message*>(msg->data);
            } else {
                outMsgSize = 0;
                return nullptr;
            }
		}

        bool dequeue() {
            uint8_t nested;
            sd_nvic_critical_region_enter(&nested);
            #if defined(DEBUG_MESSAGE_QUEUE)
            NRF_LOG_INFO("Before dequeueing");
            debugPrint();
            #endif

            bool ret = false;
            if (count > 0) {
                QueuedMessage* msg = reinterpret_cast<QueuedMessage*>(&data[reader]);
                if (msg->size == 0) {
                    reader = 0;
                    msg = reinterpret_cast<QueuedMessage*>(&data[0]);
                }
                reader += msg->stride;
                if (reader == Size) {
                    reader = 0;
                }
                count -= 1;
                ret = true;
            }

            #if defined(DEBUG_MESSAGE_QUEUE)
            NRF_LOG_INFO("After dequeueing");
            debugPrint();
            #endif
            sd_nvic_critical_region_exit(nested);
            return ret;
        }

        int getCount() {
            return count;
        }

		/// <summary>
		/// Clear the event queue
		/// </summary>
		void clear()
		{
			count = 0;
			writer = 0;
			reader = 0;
		}
        
        #if defined(DEBUG_MESSAGE_QUEUE)
        void debugPrint() {
            NRF_LOG_INFO("Message Queue has %d messages", count);
            if (count > 0) {
                int i = 0;
                int current = reader;
                do {
                    const QueuedMessage* msg = reinterpret_cast<const QueuedMessage*>(&data[current]);
                    NRF_LOG_INFO(" [%d] Offset %d, size %d, stride %d", i, current, msg->size, msg->stride);
                    ++i;
                    current += msg->stride;
                    if (current == Size) {
                        current = 0;
                    }
                }
                while (current != writer);
            }
        }
        #endif
	};

}
