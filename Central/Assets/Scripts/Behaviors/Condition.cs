using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Behaviors
{
    /// <summary>
    /// The types of conditions we support!
    /// </summary>
    public enum ConditionType : byte
    {
		[SkipEnumValue]
        Unknown = 0,
        [AdvancedEnumValue, Name("Die wakes up / sleeps")]
        HelloGoodbye,
        [Name("Die is picked up")]
        Handling,
        [Name("Die is rolling")]
		Rolling,
        [Name("Die roll is...")]
		FaceCompare,
        [Name("Die is crooked")]
		Crooked,
        [AdvancedEnumValue, Name("Bluetooth Event...")]
        ConnectionState,
        [AdvancedEnumValue, Name("Battery Event...")]
        BatteryState,
        [Name("Die is idle for...")]
        Idle,
    };

    /// <summary>
    /// The base struct for all conditions, stores a type identifier so we can tell the actual
    /// type of the condition and fetch the condition parameters correctly.
    /// </summary>
	public interface Condition
	{
		ConditionType type { get; set; }
    };

    /// <summary>
    /// Condition that triggers when the die is being handled
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ConditionIdle
        : Condition
    {
        public ConditionType type { get; set; } = ConditionType.Idle;
        public byte padding1;
        public ushort repeatPeriodMs;
    };

    /// <summary>
    /// Condition that triggers when the die is being handled
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ConditionHandling
        : Condition
    {
        public ConditionType type { get; set; } = ConditionType.Handling;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    };

    /// <summary>
    /// Condition that triggers when the die is being rolled
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ConditionRolling
        : Condition
    {
        public ConditionType type { get; set; } = ConditionType.Rolling;
        public byte padding1;
        public ushort repeatPeriodMs; // o means do NOT repeat
    };

    /// <summary>
    /// Condition that triggers when the die has landed by is crooked
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ConditionCrooked
        : Condition
    {
        public ConditionType type { get; set; } = ConditionType.Crooked;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    };

    /// <summary>
    /// Flags used to indicate how we treat the face, whether we want to trigger if the
    /// value is greater than the parameter, less, or equal, or any combination
    /// </summary>
    [System.Flags]
    public enum ConditionFaceCompare_Flags : byte
    {
        Less    = 1 << 0,
        Equal   = 1 << 1,
        Greater = 1 << 2
    };

    /// <summary>
    /// Condition that triggers when the die has landed on a face
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ConditionFaceCompare
        : Condition
    {
        public ConditionType type { get; set; } = ConditionType.FaceCompare;
        public byte faceIndex;
        public ConditionFaceCompare_Flags flags;
        public byte paddingFlags;
    };

    /// <summary>
    /// Indicate whether the condition should trigger on Hello, Goodbye or both
    /// </summary>
    [System.Flags]
    public enum ConditionHelloGoodbye_Flags : byte
    {
        Hello      = 1 << 0,
        Goodbye    = 1 << 1
    };

    /// <summary>
    /// Condition that triggers on a life state event
    /// </sumary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ConditionHelloGoodbye
        : Condition
    {
        public ConditionType type { get; set; } = ConditionType.HelloGoodbye;
        public ConditionHelloGoodbye_Flags flags;
        public byte padding1;
        public byte padding2;
    };

    /// <summary>
    /// Indicates when the condition should trigger, connected!, disconnected! or both
    /// </sumary>
    [System.Flags]
    public enum ConditionConnectionState_Flags : byte
    {
        Connected      = 1 << 0,
        Disconnected   = 1 << 1,
    };

    /// <summary>
    /// Condition that triggers on connection events
    /// </sumary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ConditionConnectionState
        : Condition
    {
        public ConditionType type { get; set; } = ConditionType.ConnectionState;
        public ConditionConnectionState_Flags flags;
        public byte padding1;
        public byte padding2;
    };

    /// <summary>
    /// Indicates which battery event the condition should trigger on
    /// </sumary>
    [System.Flags]
    public enum ConditionBatteryState_Flags : byte
    {
        Ok        = 1 << 0,
        Low       = 1 << 1,
		Charging  = 1 << 2,
		Done      = 1 << 3
    };

    /// <summary>
    /// Condition that triggers on battery state events
    /// </sumary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class ConditionBatteryState
        : Condition
    {
        public ConditionType type { get; set; } = ConditionType.BatteryState;
        public ConditionBatteryState_Flags flags;
        public ushort repeatPeriodMs;
    };
}
