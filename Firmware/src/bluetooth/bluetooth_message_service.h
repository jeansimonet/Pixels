#pragma once
#include "bluetooth_messages.h"

namespace Bluetooth
{
    namespace MessageService
    {
        #define GENERIC_DATA_SERVICE_UUID {{0x9E, 0xCA, 0xDC, 0x24, 0x0E, 0xE5, 0xA9, 0xE0, 0x93, 0xF3, 0xA3, 0xB5, 0x00, 0x00, 0x40, 0x6E}}
        #define GENERIC_DATA_SERVICE_UUID_SHORT 0x0001
        #define GENERIC_DATA_TX_CHARACTERISTIC 0x0001
        #define GENERIC_DATA_RX_CHARACTERISTIC 0x0002

        void init();
        void update();
        bool isConnected();

        bool SendMessage(Message::MessageType msgType);
        bool SendMessage(const Message* msg, int msgSize);

        template <typename Msg>
        bool SendMessage(const Msg* msg) {
            return SendMessage(msg, sizeof(Msg));
        }

        // Our bluetooth message handlers
        typedef void (*MessageHandler)(void* token, const Message* message);

        void RegisterMessageHandler(Message::MessageType msgType, void* token, MessageHandler handler);
        void UnregisterMessageHandler(Message::MessageType msgType);

        typedef void (*NotifyUserCallback)(bool result);
        void NotifyUser(const char* text, bool ok, bool cancel, uint8_t timeout_s, NotifyUserCallback callback);
    }
}
