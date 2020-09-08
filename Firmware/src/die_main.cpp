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
#include "data_set/data_set.h"
#include "nrf_log.h"

#if !defined(FIRMWARE_VERSION)
    #warning Firmware version not defined
    #define FIRMWARE_VERSION "Unknown"
#endif

using namespace DriversNRF;
using namespace Modules;
using namespace Bluetooth;
using namespace Accelerometer;
using namespace Config;
using namespace Animations;

namespace Die
{
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

        Bluetooth::MessageService::RegisterMessageHandler(Bluetooth::Message::MessageType_WhoAreYou, nullptr, WhoAreYouHandler);
        Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_PlayAnim, nullptr, PlayLEDAnim);
        Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_StopAnim, nullptr, StopLEDAnim);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_SetStandardState, nullptr, EnterStandardState);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_SetLEDAnimState, nullptr, EnterLEDAnimState);
		Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_SetBattleState, nullptr, EnterBattleState);
		//Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_AttractMode, nullptr, StartAttractMode);

        Bluetooth::Stack::hook(onConnection, nullptr);

        BatteryController::hook(onBatteryStateChange, nullptr);

        Accelerometer::hookRollState(onRollStateChange, nullptr);

        currentFace = Accelerometer::currentFace();

		NRF_LOG_INFO("Main Die Logic Initialized");
    }

    void initDebugLogic() {

        Bluetooth::MessageService::RegisterMessageHandler(Bluetooth::Message::MessageType_RequestState, nullptr, RequestStateHandler);
        Bluetooth::MessageService::RegisterMessageHandler(Bluetooth::Message::MessageType_WhoAreYou, nullptr, WhoAreYouHandler);
        Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_PlayAnim, nullptr, PlayLEDAnim);
        Bluetooth::MessageService::RegisterMessageHandler(Message::MessageType_StopAnim, nullptr, StopLEDAnim);

        BatteryController::hook(onBatteryStateChange, nullptr);

		NRF_LOG_INFO("DEBUG FIRMWARE Initialized");
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
        identityMessage.deviceId = getDeviceID();
        identityMessage.currentBehaviorIndex = SettingsManager::getSettings()->currentBehaviorIndex;
        identityMessage.dataSetHash = DataSet::dataHash();
        strncpy(identityMessage.versionInfo, FIRMWARE_VERSION, VERSION_INFO_SIZE);
        identityMessage.faceCount = (uint8_t)BoardManager::getBoard()->ledCount;
        identityMessage.designAndColor = SettingsManager::getSettings()->designAndColor;
        Bluetooth::MessageService::SendMessage(&identityMessage);
    }

    void onBatteryStateChange(void* token, BatteryController::BatteryState newState) {
        // switch (newState) {
        //     case BatteryController::BatteryState_Charging:
        //         AnimController::play(AnimationEvent_ChargingStart);
        //         break;
        //     case BatteryController::BatteryState_Low:
        //         AnimController::play(AnimationEvent_LowBattery);
        //         break;
        //     case BatteryController::BatteryState_Ok:
        //         AnimController::play(AnimationEvent_ChargingDone);
        //         break;
        //     default:
        //         break;
        // }
    }

    void onRollStateChange(void* token, Accelerometer::RollState newRollState, int newFace) {
        if (Bluetooth::MessageService::isConnected()) {
            SendRollState(newRollState, newFace);
        }

        currentRollState = newRollState;
        currentFace = newFace;
    }

    void onConnection(void* token, bool connected) {
        if (connected) {
            // Nothing
        } else {
            // Return to solo play
            EnterStandardState(nullptr, nullptr);
        }
    }

    void PlayLEDAnim(void* context, const Message* msg) {
        auto playAnimMessage = (const MessagePlayAnim*)msg;
        NRF_LOG_INFO("Playing animation %d", playAnimMessage->animation);
        AnimController::play(playAnimMessage->animation, playAnimMessage->remapFace, playAnimMessage->loop);
    }

    void StopLEDAnim(void* context, const Message* msg) {
        auto stopAnimMessage = (const MessageStopAnim*)msg;
        NRF_LOG_INFO("Stopping animation %d", stopAnimMessage->animation);
        AnimController::stop((int)stopAnimMessage->animation, stopAnimMessage->remapFace);
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

	uint32_t getDeviceID()
    {
		return NRF_FICR->DEVICEID[1] ^ NRF_FICR->DEVICEID[0];
    }

    // Main loop!
    void update() {

        Scheduler::update();
        Watchdog::feed();
        PowerManager::update();
        MessageService::update();
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