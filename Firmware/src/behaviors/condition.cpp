#include "condition.h"

namespace Behaviors
{
    /// <summary>
    /// Called by the Behavior Controller when a roll state event happens to see if this condition should trigger
    /// </summary>
    bool ConditionIdle::checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const {
        return newState == Modules::Accelerometer::RollState_OnFace || newState == Modules::Accelerometer::RollState_Crooked;
    }

    /// <summary>
    /// Called by the Behavior Controller when a roll state event happens to see if this condition should trigger
    /// </summary>
    bool ConditionHandling::checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const {
        return newState == Modules::Accelerometer::RollState_Handling;
    }

    /// <summary>
    /// Called by the Behavior Controller when a roll state event happens to see if this condition should trigger
    /// </summary>
    bool ConditionRolling::checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const {
        return newState == Modules::Accelerometer::RollState_Rolling;
    }

    /// <summary>
    /// Called by the Behavior Controller when a roll state event happens to see if this condition should trigger
    /// </summary>
    bool ConditionCrooked::checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const {
        return newState == Modules::Accelerometer::RollState_Crooked;
    }

    /// <summary>
    /// Called by the Behavior Controller when a roll state event happens to see if this condition should trigger
    /// </summary>
    bool ConditionFaceCompare::checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const {
        bool ret = false;
        if (newState == Modules::Accelerometer::RollState_OnFace) {
            if (flags & ConditionFaceCompare_Less) {
                // The flag says we should trigger if the face is less than our parameter
                ret = ret || (newFaceIndex < faceIndex);
            }
            if (flags & ConditionFaceCompare_Equal) {
                // The flag says we should trigger if the face is equal to our parameter
                ret = ret || (newFaceIndex == faceIndex);
            }
            if (flags & ConditionFaceCompare_Greater) {
                // The flag says we should trigger if the face is greater than our parameter
                ret = ret || (newFaceIndex > faceIndex);
            }
        }
        return ret;
    }

    /// <summary>
    /// Called by the Behavior Controller when a life state event happens to see if this condition should trigger
    /// </summary>
    bool ConditionHelloGoodbye::checkTrigger(bool isHello) const {
        bool ret = false;
        if (flags & ConditionHelloGoodbye_Hello) {
            // The flag says we should trigger if the die is saying hello!
            ret = ret || isHello;
        }
        if (flags & ConditionHelloGoodbye_Goodbye) {
            // The flag says we should trigger if the die is saying goodbye!
            ret = ret || !isHello;
        }
        return ret;
    }

    /// <summary>
    /// Called by the Behavior Controller when a connection state event happens to see if this condition should trigger
    /// </summary>
    bool ConditionConnectionState::checkTrigger(bool connected) const {
        bool ret = false;
        if (flags & ConditionConnectionState_Connected) {
            // The flag says we should trigger if the die just connected
            ret = ret || connected;
        }
        if (flags & ConditionConnectionState_Disconnected) {
            // The flag says we should trigger if the die just disconnected
            ret = ret || !connected;
        }
        return ret;
    }

    /// <summary>
    /// Called by the Behavior Controller when a battery state event happens to see if this condition should trigger
    /// </summary>
    bool ConditionBatteryState::checkTrigger(Modules::BatteryController::BatteryState newState) const {
        bool ret = false;
        if (flags & ConditionBatteryState_Ok) {
            // The flag says we should trigger is the battery is now Ok
            ret = ret || (newState == Modules::BatteryController::BatteryState_Ok);
        }
        if (flags & ConditionBatteryState_Low) {
            ret = ret || (newState == Modules::BatteryController::BatteryState_Low);
        }
        if (flags & ConditionBatteryState_Charging) {
            ret = ret || (newState == Modules::BatteryController::BatteryState_Charging);
        }
        if (flags & ConditionBatteryState_Done) {
            ret = ret || (newState == Modules::BatteryController::BatteryState_Done);
        }
        return ret;
    }

}
