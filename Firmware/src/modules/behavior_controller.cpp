#include "behavior_controller.h"
#include "bluetooth/bluetooth_stack.h"
#include "drivers_nrf/timers.h"
#include "data_set/data_set.h"
#include "modules/battery_controller.h"
#include "modules/accelerometer.h"
#include "data_set/data_set.h"
#include "config/settings.h"
#include "nrf_log.h"
#include "die.h"

using namespace Bluetooth;
using namespace Animations;
using namespace Config;
using namespace DriversNRF;
using namespace DataSet;

namespace Modules
{
namespace BehaviorController
{
    void onConnectionEvent(void* param, bool connected);
    void onBatterystateChange(void* param, BatteryController::BatteryState newState);
    void onRollStateChange(void* param, Accelerometer::RollState newState, int newFace);

    void idleTimerInit(void* param, int periodMs);
	void idleTimerRecheck(void* param);
    void idleRollStateChange(void* param, Accelerometer::RollState newState, int newFace);
    void idleDataSetProgramming(void* param, DataSet::ProgrammingEventType evt);

    void rollingTimerInit(void* param, int periodMs);
	void rollingTimerRecheck(void* param);
    void rollingRollStateChange(void* param, Accelerometer::RollState newState, int newFace);
    void rollingDataSetProgramming(void* param, DataSet::ProgrammingEventType evt);

    void onDataSetProgramming(void* param, DataSet::ProgrammingEventType evt);
    void onSettingsProgramming(void* param, SettingsManager::ProgrammingEventType evt);

	void init() {

		// Hook up the behavior controller to all the events it needs to know about to do its job!
        Bluetooth::Stack::hook(onConnectionEvent, nullptr);
        BatteryController::hook(onBatterystateChange, nullptr);
        Accelerometer::hookRollState(onRollStateChange, nullptr);
        DataSet::hookProgrammingEvent(onDataSetProgramming, nullptr);
        SettingsManager::hookProgrammingEvent(onSettingsProgramming, nullptr);

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
                }
            } else if (condition->type == Behaviors::Condition_Idle) {
                // We have an idle condition
                auto idleCondition = static_cast<const Behaviors::ConditionIdle*>(condition);
                
                // Setup a timer to repeat this check in a little bit if appropriate
                if (idleCondition->checkTrigger(Accelerometer::currentRollState(), Accelerometer::currentFace()) && idleCondition->repeatPeriodMs != 0) {
                    // Subscribe to be updated on a timer, so we can repeatedly check the condition
                    idleTimerInit((void*)rule, idleCondition->repeatPeriodMs);
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

    void idleTimerInit(void* param, int periodMs) {
        // Subscribe to be updated on a timer, so we can repeatedly check the condition
        if (Timers::setDelayedCallback(idleTimerRecheck, param, periodMs)) {
            // Also subscribe when rollstate changes so we can kill the timer
            Accelerometer::hookRollState(idleRollStateChange, param);
            DataSet::hookProgrammingEvent(idleDataSetProgramming, param);
        }
    }

	void idleTimerRecheck(void* param) {
		const Behaviors::Rule* rule = (const Behaviors::Rule*)param;
        auto condition = DataSet::getCondition(rule->condition);
        auto idleCondition = static_cast<const Behaviors::ConditionIdle*>(condition);
        if (idleCondition->checkTrigger(Accelerometer::currentRollState(), Accelerometer::currentFace())) {
            // do the thing
            Behaviors::triggerActions(rule->actionOffset, rule->actionCount);
            Timers::setDelayedCallback(idleTimerRecheck, (void*)rule, idleCondition->repeatPeriodMs);
        } else {
            // Stop Timer and unregister callback
            Timers::cancelDelayedCallback(idleTimerRecheck, (void*)rule);
            Accelerometer::unHookRollState(idleRollStateChange);
            DataSet::unhookProgrammingEvent(idleDataSetProgramming);
        }
	}

    void idleRollStateChange(void* param, Accelerometer::RollState newState, int newFace) {
		const Behaviors::Rule* rule = (const Behaviors::Rule*)param;
        auto condition = DataSet::getCondition(rule->condition);
        auto idleCondition = static_cast<const Behaviors::ConditionIdle*>(condition);
        if (!idleCondition->checkTrigger(newState, newFace)) {
            // Stop Timer and unregister callback
            Timers::cancelDelayedCallback(idleTimerRecheck, (void*)rule);
            Accelerometer::unHookRollState(idleRollStateChange);
            DataSet::unhookProgrammingEvent(idleDataSetProgramming);
        }
    }

    void idleDataSetProgramming(void* param, DataSet::ProgrammingEventType evt) {
        Timers::cancelDelayedCallback(idleTimerRecheck, param);
        Accelerometer::unHookRollState(idleRollStateChange);
        DataSet::unhookProgrammingEvent(idleDataSetProgramming);
    }

    void rollingTimerInit(void* param, int periodMs) {
        // Subscribe to be updated on a timer, so we can repeatedly check the condition
        if (Timers::setDelayedCallback(rollingTimerRecheck, param, periodMs)) {
            // Also subscribe when rollstate changes so we can kill the timer
            Accelerometer::hookRollState(rollingRollStateChange, param);
            DataSet::hookProgrammingEvent(rollingDataSetProgramming, param);
        }
    }

	void rollingTimerRecheck(void* param) {
		const Behaviors::Rule* rule = (const Behaviors::Rule*)param;
        auto condition = DataSet::getCondition(rule->condition);
        auto rollingCondition = static_cast<const Behaviors::ConditionRolling*>(condition);
        if (rollingCondition->checkTrigger(Accelerometer::currentRollState(), Accelerometer::currentFace())) {
            // do the thing
            Behaviors::triggerActions(rule->actionOffset, rule->actionCount);
            Timers::setDelayedCallback(rollingTimerRecheck, (void*)rule, rollingCondition->repeatPeriodMs);
        } else {
            // Stop Timer and unregister callback
            Timers::cancelDelayedCallback(rollingTimerRecheck, (void*)rule);
            Accelerometer::unHookRollState(rollingRollStateChange);
            DataSet::unhookProgrammingEvent(rollingDataSetProgramming);
        }
	}

    void rollingRollStateChange(void* param, Accelerometer::RollState newState, int newFace) {
		const Behaviors::Rule* rule = (const Behaviors::Rule*)param;
        auto condition = DataSet::getCondition(rule->condition);
        auto rollingCondition = static_cast<const Behaviors::ConditionRolling*>(condition);
        if (!rollingCondition->checkTrigger(newState, newFace)) {
            // Stop Timer and unregister callback
            Timers::cancelDelayedCallback(rollingTimerRecheck, (void*)rule);
            Accelerometer::unHookRollState(rollingRollStateChange);
            DataSet::unhookProgrammingEvent(rollingDataSetProgramming);
        }
    }

    void rollingDataSetProgramming(void* param, DataSet::ProgrammingEventType evt) {
        Timers::cancelDelayedCallback(rollingTimerRecheck, param);
        Accelerometer::unHookRollState(rollingRollStateChange);
        DataSet::unhookProgrammingEvent(rollingDataSetProgramming);
    }


    void onRollStateChange(void* param, Accelerometer::RollState newState, int newFace) {

        if (Die::getCurrentState() == Die::TopLevel_SoloPlay)
        {
            // Do we have a roll state event condition?
            auto bhv = DataSet::getBehavior();

            // Check for an rolling condition, we should trigger it even if we have an OnFace condition,
            // regardless of where it sits in the list
            for (int i = 0; i < bhv->rulesCount; ++i) {
                auto rule = DataSet::getRule(bhv->rulesOffset + i);
                auto condition = DataSet::getCondition(rule->condition);
                if (condition->type == Behaviors::Condition_Idle) {
                    // We have an idle condition
                    auto idleCondition = static_cast<const Behaviors::ConditionIdle*>(condition);
                    
                    // Setup a timer to repeat this check in a little bit if appropriate
                    if (idleCondition->checkTrigger(newState, newFace) && idleCondition->repeatPeriodMs != 0) {
                        idleTimerInit((void*)rule, idleCondition->repeatPeriodMs);
                    }
                    break;
                }
            }

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
                        {
                            auto rollingCondition = static_cast<const Behaviors::ConditionRolling*>(condition);
                            conditionTriggered = rollingCondition->checkTrigger(newState, newFace);
                            
                            // Setup a timer to repeat this check in a little bit if appropriate
                            if (conditionTriggered && rollingCondition->repeatPeriodMs != 0) {
                                rollingTimerInit((void*)rule, rollingCondition->repeatPeriodMs);
                            }
                        }
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

    void onDataSetProgramming(void* param, DataSet::ProgrammingEventType evt) {
        switch (evt) {
            case DataSet::ProgrammingEventType_Begin:
                Timers::pauseDelayedCallbacks();
                break;
            case DataSet::ProgrammingEventType_End:
                Timers::resumeDelayedCallbacks();
                break;
        }
    }
    
    void onSettingsProgramming(void* param, SettingsManager::ProgrammingEventType evt) {
        switch (evt) {
            case SettingsManager::ProgrammingEventType_Begin:
                Timers::pauseDelayedCallbacks();
                break;
            case SettingsManager::ProgrammingEventType_End:
                Timers::resumeDelayedCallbacks();
                break;
        }
    }

}
}