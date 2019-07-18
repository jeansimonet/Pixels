#include "die.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bluetooth_stack.h"
#include "config/board_config.h"
#include "modules/accelerometer.h"
#include "modules/anim_controller.h"
#include "modules/battery_controller.h"
#include "nrf_log.h"

using namespace Modules;
using namespace Bluetooth;
using namespace Accelerometer;
using namespace Config;

namespace Die
{
    enum DieType
    {
        DieType_Unknown = 0,
        DieType_6Sided,
        DieType_20Sided
    };

    DieType dieType = DieType_Unknown;

    enum TopLevelState
    {
        TopLevel_Unknown = 0,
        TopLevel_SoloPlay,      // Playing animations as a result of landing on faces
        TopLevel_PairedPlay,    // Some kind of battle play
        TopLevel_LowPower,      // Die is low on power
        TopLevel_Charging,      // Die is now charging
    };

    TopLevelState currentTopLevelState = TopLevel_SoloPlay;

    enum RollState
    {
        RollState_Unknown = 0,
        RollState_Idle,
		RollState_Handling,
		RollState_Falling,
		RollState_Rolling,
		RollState_Jerking,
		RollState_Crooked,
    };

    RollState currentRollState = RollState_Idle;

    void RequestStateHandler(void* token, const Message* message);
    void WhoAreYouHandler(void* token, const Message* message);
    void onBatteryStateChange(void* token, BatteryController::BatteryState newState);

    void initMainLogic() {
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

        BatteryController::hook(onBatteryStateChange, nullptr);

        // Register with the accelerometer
        //Accelerometer::hook(onAccelData)

		NRF_LOG_INFO("Die State initialized");
    }

    void RequestStateHandler(void* token, const Message* message) {
        // Central asked for the die state, return it!
        Bluetooth::MessageDieState currentStateMsg;
        currentStateMsg.state = (uint8_t)currentRollState;
        Bluetooth::MessageService::SendMessage(&currentStateMsg);
    }

    void WhoAreYouHandler(void* token, const Message* message) {
        // Central asked for the die state, return it!
        Bluetooth::MessageIAmADie identityMessage;
        identityMessage.id = (uint8_t)dieType;
        Bluetooth::MessageService::SendMessage(&identityMessage);
    }

    void onBatteryStateChange(void* token, BatteryController::BatteryState newState) {
        switch (newState) {
            case BatteryController::BatteryState_Charging:
                // Die is now charging, disconnect from Bluetooth etc...
                if (Bluetooth::Stack::isConnected()) {
                    Bluetooth::Stack::disableAdvertisingOnDisconnect();
                    Bluetooth::Stack::disconnect();
                } else {
                    Bluetooth::Stack::stopAdvertising();
                }
                currentTopLevelState = TopLevel_Charging;
                break;
            case BatteryController::BatteryState_Low:
                if (Bluetooth::Stack::isConnected()) {
                    Bluetooth::Stack::disableAdvertisingOnDisconnect();
                    Bluetooth::Stack::disconnect();
                } else {
                    Bluetooth::Stack::stopAdvertising();
                }
                currentTopLevelState = TopLevel_LowPower;
                break;
            case BatteryController::BatteryState_Ok:
                currentTopLevelState = TopLevel_SoloPlay;
                Bluetooth::Stack::startAdvertising();
                break;
            default:
                break;
        }
    }
}
