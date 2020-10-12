#pragma once

#include "modules/battery_controller.h"
#include "bluetooth/bluetooth_stack.h"
#include "modules/accelerometer.h"
#include <stdint.h>

#pragma pack(push, 1)

namespace Behaviors
{
    /// <summary>
    /// The types of conditions we support!
    /// </summary>
    enum ConditionType : uint8_t
    {
        Condition_Unknown = 0,
        Condition_HelloGoodbye,
        Condition_Handling,
		Condition_Rolling,
		Condition_FaceCompare,
		Condition_Crooked,
        Condition_ConnectionState,
        Condition_BatteryState,
        Condition_Idle,
    };

    /// <summary>
    /// The base struct for all conditions, stores a type identifier so we can tell the actual
    /// type of the condition and fetch the condition parameters correctly.
    /// </summary>
	struct Condition
	{
		ConditionType type;
    };

    /// <summary>
    /// Condition that triggers when the die is idle for a while
    /// </summary>
    struct ConditionIdle
        : public Condition
    {
        uint8_t padding1;
        uint16_t repeatPeriodMs;
        // No stored parameter for now
        bool checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const;
    };

    /// <summary>
    /// Condition that triggers when the die is being handled
    /// </summary>
    struct ConditionHandling
        : public Condition
    {
        uint8_t padding1;
        uint8_t padding2;
        uint8_t padding3;
        // No stored parameter for now
        bool checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const;
    };

    /// <summary>
    /// Condition that triggers when the die is being rolled
    /// </summary>
    struct ConditionRolling
        : public Condition
    {
        uint8_t padding1;
        uint16_t repeatPeriodMs; // 0 means do NOT repeat
        // No parameter for now
        bool checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const;
    };

    /// <summary>
    /// Condition that triggers when the die has landed by is crooked
    /// </summary>
    struct ConditionCrooked
        : public Condition
    {
        uint8_t padding1;
        uint8_t padding2;
        uint8_t padding3;
        // No parameter for now
        bool checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const;
    };

    /// <summary>
    /// Flags used to indicate how we treat the face, whether we want to trigger if the
    /// value is greater than the parameter, less, or equal, or any combination
    /// </summary>
    enum ConditionFaceCompare_Flags : uint8_t
    {
        ConditionFaceCompare_Less    = 1 << 0,
        ConditionFaceCompare_Equal   = 1 << 1,
        ConditionFaceCompare_Greater = 1 << 2
    };

    /// <summary>
    /// Condition that triggers when the die has landed on a face
    /// </summary>
    struct ConditionFaceCompare
        : public Condition
    {
        uint8_t faceIndex;
        uint8_t flags; // ConditionFaceCompare_Flags
        uint8_t paddingFlags;
        bool checkTrigger(Modules::Accelerometer::RollState newState, int newFaceIndex) const;
    };

    /// <summary>
    /// Indicate whether the condition should trigger on Hello, Goodbye or both
    /// </summary>
    enum ConditionHelloGoodbye_Flags : uint8_t
    {
        ConditionHelloGoodbye_Hello      = 1 << 0,
        ConditionHelloGoodbye_Goodbye    = 1 << 1
    };

    /// <summary>
    /// Condition that triggers on a life state event
    /// </sumary>
    struct ConditionHelloGoodbye
        : public Condition
    {
        uint8_t flags; // ConditionHelloGoodbye_Flags
        uint8_t padding1;
        uint8_t padding2;
        bool checkTrigger(bool isHello) const;
    };

    /// <summary>
    /// Indicates when the condition should trigger, connected!, disconnected! or both
    /// </sumary>
    enum ConditionConnectionState_Flags : uint8_t
    {
        ConditionConnectionState_Connected      = 1 << 0,
        ConditionConnectionState_Disconnected   = 1 << 1,
    };

    /// <summary>
    /// Condition that triggers on connection events
    /// </sumary>
    struct ConditionConnectionState
        : public Condition
    {
        uint8_t flags; // ConditionConnectionState_Flags
        uint8_t padding1;
        uint8_t padding2;
        bool checkTrigger(bool connected) const;
    };

    /// <summary>
    /// Indicates which battery event the condition should trigger on
    /// </sumary>
    enum ConditionBatteryState_Flags : uint8_t
    {
        ConditionBatteryState_Ok        = 1 << 0,
        ConditionBatteryState_Low       = 1 << 1,
		ConditionBatteryState_Charging  = 1 << 2,
		ConditionBatteryState_Done      = 1 << 3
    };

    /// <summary>
    /// Condition that triggers on battery state events
    /// </sumary>
    struct ConditionBatteryState
        : public Condition
    {
        uint8_t flags; // ConditionBatteryState_Flags
        uint8_t padding1;
        uint8_t padding2;
        bool checkTrigger(Modules::BatteryController::BatteryState newState) const;
    };
}

#pragma pack(pop)
