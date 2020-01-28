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
        TopLevel_Attract,
    };

    TopLevelState currentTopLevelState = TopLevel_SoloPlay;
    int currentFace = 0;
    RollState currentRollState = RollState_Unknown;

    void RequestStateHandler(void* token, const Message* message);
    void WhoAreYouHandler(void* token, const Message* message);
    void onBatteryStateChange(void* token, BatteryController::BatteryState newState);
    void onRollStateChange(void* token, Accelerometer::RollState newRollState, int newFace);
    void SendRollState(Accelerometer::RollState rollState, int face);
    void PlayLEDAnim(void* context, const Message* msg);
    void PlayAnimEvent(void* context, const Message* msg);
    void StopLEDAnim(void* context, const Message* msg);
	void EnterStandardState(void* context, const Message* msg);
	void EnterLEDAnimState(void* context, const Message* msg);
	void EnterBattleState(void* context, const Message* msg);
    void StartAttractMode(void* context, const Message* msg);
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
        Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_StopAnim, nullptr, StopLEDAnim);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_SetStandardState, nullptr, EnterStandardState);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_SetLEDAnimState, nullptr, EnterLEDAnimState);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_SetBattleState, nullptr, EnterBattleState);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_AttractMode, nullptr, StartAttractMode);

        Bluetooth::Stack::hook(onConnection, nullptr);

        BatteryController::hook(onBatteryStateChange, nullptr);

        Accelerometer::hookRollState(onRollStateChange, nullptr);

        currentFace = Accelerometer::currentFace();

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
        switch (newState) {
            case BatteryController::BatteryState_Charging:
                AnimController::play(AnimationEvent_ChargingStart);
                break;
            case BatteryController::BatteryState_Low:
                AnimController::play(AnimationEvent_LowBattery);
                break;
            case BatteryController::BatteryState_Ok:
                AnimController::play(AnimationEvent_ChargingDone);
                break;
            default:
                break;
        }
    }

    void onRollStateChange(void* token, Accelerometer::RollState newRollState, int newFace) {
        if (Bluetooth::MessageService::isConnected()) {
            SendRollState(newRollState, newFace);
        }

        if (currentTopLevelState == TopLevel_SoloPlay) {
            // Play animation
            switch (newRollState) {
                case RollState_OnFace:
                    if (currentRollState == RollState_Rolling) {
                        /// Check for an override animation first, then default
                        AnimationEvent faceEvent = (AnimationEvent)((int)AnimationEvent_OnFace_00 + newFace);
                        if (AnimController::hasAnimationForEvent(faceEvent)) {
                            AnimController::play(faceEvent, 0, false);
                        } else {
                            AnimController::play(AnimationEvent_OnFace_Default, newFace, false);
                        }
                    }
                    // Else don't play face anim
                    break;
                case RollState_Handling:
                    AnimController::play(AnimationEvent_Handling, newFace, false);
                    break;
                case RollState_Rolling:
                    AnimController::play(AnimationEvent_Rolling, newFace, false);
                    break;
                case RollState_Crooked:
                    AnimController::play(AnimationEvent_Crooked, newFace, false);
                    break;
                default:
                    break;
            }
        }

        currentRollState = newRollState;
        currentFace = newFace;
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
        uint8_t remapFace = 0;
        if (animation.specialColorType == SpecialColor_Face) {
            remapFace = Accelerometer::currentFace();
        }
        AnimController::play(&animation, remapFace, playAnimMessage->loop);
    }

    void StopLEDAnim(void* context, const Message* msg) {
        auto stopAnimMessage = (const MessageStopAnim*)msg;
        NRF_LOG_INFO("Stopping animation %d", stopAnimMessage->animation);
        auto& animation = AnimationSet::getAnimation(stopAnimMessage->animation);
        AnimController::stop(&animation, stopAnimMessage->remapFace);
    }

    void PlayAnimEvent(void* context, const Message* msg) {
        auto playAnimMessage = (const MessagePlayAnimEvent*)msg;
        AnimController::play((AnimationEvent)playAnimMessage->evt, playAnimMessage->remapFace, playAnimMessage->loop != 0);
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

    void StartAttractMode(void* context, const Message* msg) {
        currentTopLevelState = TopLevel_Attract;
        AnimController::play(AnimationEvent_AttractMode, 0, true);
    }


    // Main loop!
    void update() {

        Scheduler::update();
        Watchdog::feed();
        PowerManager::update();
        Bluetooth::MessageService::update();
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