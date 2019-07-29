#include "die.h"
#include "drivers_nrf/watchdog.h"
#include "drivers_nrf/scheduler.h"
#include "drivers_nrf/power_manager.h"
#include "bluetooth/bluetooth_messages.h"
#include "bluetooth/bluetooth_message_service.h"
#include "bluetooth/bluetooth_stack.h"
#include "config/board_config.h"
#include "config/settings.h"
#include "modules/accelerometer.h"
#include "modules/anim_controller.h"
#include "modules/battery_controller.h"
#include "animations/animation_set.h"
#include "nrf_log.h"

using namespace DriversNRF;
using namespace Modules;
using namespace Bluetooth;
using namespace Accelerometer;
using namespace Config;
using namespace Animations;

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
        TopLevel_BattlePlay,    // Some kind of battle play
        TopLevel_Animator,      // LED Animator
        TopLevel_LowPower,      // Die is low on power
    };

    TopLevelState currentTopLevelState = TopLevel_SoloPlay;

    void RequestStateHandler(void* token, const Message* message);
    void WhoAreYouHandler(void* token, const Message* message);
    void onBatteryStateChange(void* token, BatteryController::BatteryState newState);
    void onRollStateChange(void* token, Accelerometer::RollState newRollState, int newFace);
    void SendRollState(Accelerometer::RollState rollState, int face);
    void PlayLEDAnim(void* context, const Message* msg);
	void EnterStandardState(void* context, const Message* msg);
	void EnterLEDAnimState(void* context, const Message* msg);
	void EnterBattleState(void* context, const Message* msg);
    void onConnection(void* token, bool connected);

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
        Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_PlayAnim, nullptr, PlayLEDAnim);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_SetStandardState, nullptr, EnterStandardState);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_SetLEDAnimState, nullptr, EnterLEDAnimState);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_SetBattleState, nullptr, EnterBattleState);

        Bluetooth::Stack::hook(onConnection, nullptr);

        BatteryController::hook(onBatteryStateChange, nullptr);

        Accelerometer::hookRollState(onRollStateChange, nullptr);

		NRF_LOG_INFO("Main Die Logic Initialized");
    }

    void RequestStateHandler(void* token, const Message* message) {
        SendRollState(Accelerometer::currentRollState(), Accelerometer::currentFace());
    }

    void SendRollState(Accelerometer::RollState rollState, int face) {
        // Central asked for the die state, return it!
        Bluetooth::MessageDieState currentStateMsg;
        currentStateMsg.state = (uint8_t)rollState;
        currentStateMsg.face = (uint8_t)face;
        Bluetooth::MessageService::SendMessage(&currentStateMsg);
    }

    void WhoAreYouHandler(void* token, const Message* message) {
        // Central asked for the die state, return it!
        Bluetooth::MessageIAmADie identityMessage;
        identityMessage.id = (uint8_t)dieType;
        Bluetooth::MessageService::SendMessage(&identityMessage);
    }

    void onBatteryStateChange(void* token, BatteryController::BatteryState newState) {
        // switch (newState) {
        //     case BatteryController::BatteryState_Charging:
        //         // Die is now charging, disconnect from Bluetooth etc...
        //         if (Bluetooth::Stack::isConnected()) {
        //             Bluetooth::Stack::disableAdvertisingOnDisconnect();
        //             Bluetooth::Stack::disconnect();
        //         } else {
        //             Bluetooth::Stack::stopAdvertising();
        //         }
        //         currentTopLevelState = TopLevel_Charging;
        //         break;
        //     case BatteryController::BatteryState_Low:
        //         if (Bluetooth::Stack::isConnected()) {
        //             Bluetooth::Stack::disableAdvertisingOnDisconnect();
        //             Bluetooth::Stack::disconnect();
        //         } else {
        //             Bluetooth::Stack::stopAdvertising();
        //         }
        //         currentTopLevelState = TopLevel_LowPower;
        //         break;
        //     case BatteryController::BatteryState_Ok:
        //         currentTopLevelState = TopLevel_SoloPlay;
        //         if (!Bluetooth::Stack::isAdvertising()) {
        //             Bluetooth::Stack::startAdvertising();
        //         }
        //         break;
        //     default:
        //         break;
        // }
    }

    void onRollStateChange(void* token, Accelerometer::RollState newRollState, int newFace) {
        SendRollState(newRollState, newFace);

        if (currentTopLevelState == TopLevel_SoloPlay) {
            // Play animation
            switch (newRollState) {
                case Accelerometer::RollState_Handling:
                case Accelerometer::RollState_Rolling:
                    AnimController::play(AnimationEvent_Handling, newFace, false);
                    break;
                case Accelerometer::RollState_OnFace:
                    AnimController::play(AnimationEvent_OnFace, newFace, false);
                    break;
                case Accelerometer::RollState_Crooked:
                case Accelerometer::RollState_Unknown:
                    AnimController::play(AnimationEvent_Crooked, newFace, false);
                    break;
            }
        }
    }

    void onConnection(void* token, bool connected) {
        if (connected) {
            AnimController::play(AnimationEvent_Connected);
        } else {
            AnimController::play(AnimationEvent_Disconnected);
            // Return to solo play
            EnterStandardState(nullptr, nullptr);
        }
    }

    void PlayLEDAnim(void* context, const Message* msg) {
      auto playAnimMessage = (const MessagePlayAnim*)msg;
      NRF_LOG_INFO("Playing animation %d", playAnimMessage->animation);
      auto& animation = AnimationSet::getAnimation(playAnimMessage->animation);
      AnimController::play(&animation, playAnimMessage->remapFace);
    }

	void EnterStandardState(void* context, const Message* msg) {
        switch (currentTopLevelState) {
            case TopLevel_Unknown:
            case TopLevel_BattlePlay:
            case TopLevel_Animator:
            default:
                // Reactivate playing animations based on face
                currentTopLevelState = TopLevel_SoloPlay;
                break;
            case TopLevel_SoloPlay:
            case TopLevel_LowPower:
                // Nothing to do
                break;
       }
    }

	void EnterLEDAnimState(void* context, const Message* msg) {
        switch (currentTopLevelState) {
            case TopLevel_Unknown:
            case TopLevel_BattlePlay:
            case TopLevel_SoloPlay:
            default:
                // Reactivate playing animations based on face
                currentTopLevelState = TopLevel_Animator;
                break;
            case TopLevel_Animator:
            case TopLevel_LowPower:
                // Nothing to do
                break;
       }
    }
    
	void EnterBattleState(void* context, const Message* msg) {
        switch (currentTopLevelState) {
            case TopLevel_Animator:
            case TopLevel_Unknown:
            case TopLevel_SoloPlay:
            default:
                // Reactivate playing animations based on face
                currentTopLevelState = TopLevel_BattlePlay;
                break;
            case TopLevel_BattlePlay:
            case TopLevel_LowPower:
                // Nothing to do
                break;
       }
    }

    // Main loop!
    void update() {

        Scheduler::update();
        Watchdog::feed();
        PowerManager::update();
    }
}

int main() {
    Die::init();
    for (;;)
    {
        Die::update();
    }
    return 0;
}