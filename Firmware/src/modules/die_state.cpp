#include "die_state.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "config/board_config.h"
#include "accelerometer.h"
#include "nrf_log.h"

using namespace Modules;
using namespace Bluetooth;
using namespace Accelerometer;
using namespace Config;

namespace Modules
{
namespace DieState
{
    enum DieType
    {
        DieType_Unknown = 0,
        DieType_6Sided,
        DieType_20Sided
    };

    DieType dieType = DieType_Unknown;

    enum State
    {
        State_Unknown = 0,
        State_Idle,
		State_Handling,
		State_Falling,
		State_Rolling,
		State_Jerking,
		State_Crooked,
    };

    State currentState = State_Idle;

    void RequestStateHandler(void* token, const Message* message);
    void WhoAreYouHandler(void* token, const Message* message);

    void init()
    {
        Bluetooth::MessageService::RegisterMessageHandler(Bluetooth::Message::MessageType_RequestState, nullptr, RequestStateHandler);

        switch (BoardManager::getBoard()->ledCount)
        {
            case 6:
                dieType = DieType_6Sided;
                break;
            case 20:
                dieType = DieType_20Sided;
                break;
            default:
                break;
        }

        Bluetooth::MessageService::RegisterMessageHandler(Bluetooth::Message::MessageType_WhoAreYou, nullptr, WhoAreYouHandler);

        // Register with the accelerometer
        //Accelerometer::hook(onAccelData)

		NRF_LOG_INFO("Die State initialized");
    }

    void update()
    {

    }

    void RequestStateHandler(void* token, const Message* message)
    {
        // Central asked for the die state, return it!
        Bluetooth::MessageDieState currentStateMsg;
        currentStateMsg.state = (uint8_t)currentState;
        Bluetooth::MessageService::SendMessage(&currentStateMsg);
    }

    void WhoAreYouHandler(void* token, const Message* message)
    {
        // Central asked for the die state, return it!
        Bluetooth::MessageIAmADie identityMessage;
        identityMessage.id = (uint8_t)dieType;
        Bluetooth::MessageService::SendMessage(&identityMessage);
    }
}
}
