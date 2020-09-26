#include "behavior_controller.h"
#include "bluetooth/bluetooth_stack.h"
#include "modules/battery_controller.h"
#include "modules/accelerometer.h"
#include "data_set/data_set.h"
#include "config/settings.h"
#include "nrf_log.h"
#include "die.h"

using namespace Bluetooth;
using namespace Animations;
using namespace Config;

namespace Modules
{
namespace BehaviorController
{
    void onConnectionEvent(void* param, bool connected);
    void onBatterystateChange(void* param, BatteryController::BatteryState newState);
    void onRollStateChange(void* param, Accelerometer::RollState newState, int newFace);

	void init() {

		// Hook up the behavior controller to all the events it needs to know about to do its job!
        Bluetooth::Stack::hook(onConnectionEvent, nullptr);
        BatteryController::hook(onBatterystateChange, nullptr);
        Accelerometer::hookRollState(onRollStateChange, nullptr);

		NRF_LOG_INFO("Behavior Controller Initialized");
    }

    void onDiceInitialized() {
        // Do we have a hello goodbye condition
        auto bhv = DataSet::getBehavior();

        // Iterate the rules and look for one!
        for (int i = 0; i < bhv->rulesCount; ++i) {
            auto rule = DataSet::getRule(bhv->rulesOffset + i);
            auto condition = DataSet::getCondition(rule->condition);
            if (condition->type == Behaviors::Condition_HelloGoodbye) {
                // This is the right kind of condition, check it!
                auto cond = static_cast<const Behaviors::ConditionHelloGoodbye*>(condition);
                if (cond->checkTrigger(true)) {
                    NRF_LOG_DEBUG("Triggering a HelloGoodbye Condition");
                    // Go on, do the thing!
                    Behaviors::triggerActions(rule->actionOffset, rule->actionCount);

                    // We're done!
                    break;
                }
            }
        }
    }

	void onConnectionEvent(void* param, bool connected) {
        // Do we have a connection event condition?
        auto bhv = DataSet::getBehavior();

        // Iterate the rules and look for one!
        for (int i = 0; i < bhv->rulesCount; ++i) {
            auto rule = DataSet::getRule(bhv->rulesOffset + i);
            auto condition = DataSet::getCondition(rule->condition);
            if (condition->type == Behaviors::Condition_ConnectionState) {
                // This is the right kind of condition, check it!
                auto cond = static_cast<const Behaviors::ConditionConnectionState*>(condition);
                if (cond->checkTrigger(connected)) {
                    NRF_LOG_DEBUG("Triggering a Connection State Condition");
                    // Go on, do the thing!
                    Behaviors::triggerActions(rule->actionOffset, rule->actionCount);

                    // We're done!
                    break;
                }
            }
        }
    }

    void onBatterystateChange(void* param, BatteryController::BatteryState newState) {
        // Do we have a battery event condition?
        auto bhv = DataSet::getBehavior();

        // Iterate the rules and look for one!
        for (int i = 0; i < bhv->rulesCount; ++i) {
            auto rule = DataSet::getRule(bhv->rulesOffset + i);
            auto condition = DataSet::getCondition(rule->condition);
            if (condition->type == Behaviors::Condition_BatteryState) {
                // This is the right kind of condition, check it!
                auto cond = static_cast<const Behaviors::ConditionBatteryState*>(condition);
                if (cond->checkTrigger(newState)) {
                    NRF_LOG_DEBUG("Triggering a Battery State Condition");
                    // Go on, do the thing!
                    Behaviors::triggerActions(rule->actionOffset, rule->actionCount);

                    // We're done!
                    break;
                }
            }
        }
    }

    void onRollStateChange(void* param, Accelerometer::RollState newState, int newFace) {
        if (Die::getCurrentState() == Die::TopLevel_SoloPlay)
        {
            // Do we have a roll state event condition?
            auto bhv = DataSet::getBehavior();

            // Iterate the rules and look for one!
            for (int i = 0; i < bhv->rulesCount; ++i) {
                auto rule = DataSet::getRule(bhv->rulesOffset + i);
                auto condition = DataSet::getCondition(rule->condition);

                // This is the right kind of condition, check it!
                bool conditionTriggered = false;
                switch (condition->type) {
                    case Behaviors::Condition_Handling:
                        conditionTriggered = static_cast<const Behaviors::ConditionHandling*>(condition)->checkTrigger(newState, newFace);
                        break;
                    case Behaviors::Condition_Rolling:
                        conditionTriggered = static_cast<const Behaviors::ConditionRolling*>(condition)->checkTrigger(newState, newFace);
                        break;
                    case Behaviors::Condition_Crooked:
                        conditionTriggered = static_cast<const Behaviors::ConditionCrooked*>(condition)->checkTrigger(newState, newFace);
                        break;
                    case Behaviors::Condition_FaceCompare:
                        conditionTriggered = static_cast<const Behaviors::ConditionFaceCompare*>(condition)->checkTrigger(newState, newFace);
                        break;
                    default:
                        break;
                }

                if (conditionTriggered) {
                    // do the thing
                    Behaviors::triggerActions(rule->actionOffset, rule->actionCount);

                    // We're done
                    break;
                }
            }
        }
    }
}
}