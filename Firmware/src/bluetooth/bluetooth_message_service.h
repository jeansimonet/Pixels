#pragma once
#include "bluetooth_messages.h"

namespace Bluetooth
{
    namespace MessageService
    {
        void init();

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
