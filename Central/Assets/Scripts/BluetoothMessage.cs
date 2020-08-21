using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// These message identifiers have to match up with the ones on the firmware of course!
/// </summary>
public enum DieMessageType : byte
{
	None = 0,
	WhoAreYou,
	IAmADie,
	State,
	Telemetry,
	BulkSetup,
	BulkSetupAck,
	BulkData,
	BulkDataAck,
	TransferAnimSet,
	TransferAnimSetAck,
	TransferAnimSetFinished,
	TransferSettings,
	TransferSettingsAck,
	TransferSettingsFinished,
    TransferTestAnimSet,
    TransferTestAnimSetAck,
    TransferTestAnimSetFinished,
	DebugLog,
	PlayAnim,
	PlayAnimEvent,
	StopAnim,
	RequestState,
	RequestAnimSet,
	RequestSettings,
	RequestTelemetry,
	ProgramDefaultAnimSet,
	ProgramDefaultAnimSetFinished,
	Flash,
	FlashFinished,
	RequestDefaultAnimSetColor,
	DefaultAnimSetColor,
	RequestBatteryLevel,
	BatteryLevel,
	Calibrate,
	CalibrateFace,
	NotifyUser,
	NotifyUserAck,
	TestHardware,
	SetStandardState,
	SetLEDAnimState,
	SetBattleState,
	ProgramDefaultParameters,
	ProgramDefaultParametersFinished,
    SetDesignAndColor,
    SetDesignAndColorAck,
    SetCurrentBehavior,
    SetCurrentBehaviorAck,

    // Testing
    TestBulkSend, 
	TestBulkReceive,
	SetAllLEDsToColor,
	AttractMode,
	PrintNormals,
	PrintA2DReadings,
	LightUpFace,
	SetLEDToColor,
}

public interface DieMessage
{
    DieMessageType type { get; set; }
}

public static class DieMessages
{
    public const int maxDataSize = 100;
    public const int VERSION_INFO_SIZE = 6;

    public static DieMessage FromByteArray(byte[] data)
    {
        DieMessage ret = null;
        if (data.Length > 0)
        {
            DieMessageType type = (DieMessageType)data[0];
            switch (type)
            {
                case DieMessageType.State:
                    ret = FromByteArray<DieMessageState>(data);
                    break;
                case DieMessageType.WhoAreYou:
                    ret = FromByteArray<DieMessageWhoAreYou>(data);
                    break;
                case DieMessageType.IAmADie:
                    ret = FromByteArray<DieMessageIAmADie>(data);
                    break;
                case DieMessageType.Telemetry:
                    ret = FromByteArray<DieMessageAcc>(data);
                    break;
                case DieMessageType.BulkSetup:
                    ret = FromByteArray<DieMessageBulkSetup>(data);
                    break;
                case DieMessageType.BulkData:
                    ret = FromByteArray<DieMessageBulkData>(data);
                    break;
                case DieMessageType.BulkSetupAck:
                    ret = FromByteArray<DieMessageBulkSetupAck>(data);
                    break;
                case DieMessageType.BulkDataAck:
                    ret = FromByteArray<DieMessageBulkDataAck>(data);
                    break;
                case DieMessageType.TransferAnimSet:
                    ret = FromByteArray<DieMessageTransferAnimSet>(data);
                    break;
                case DieMessageType.TransferAnimSetAck:
                    ret = FromByteArray<DieMessageTransferAnimSetAck>(data);
                    break;
                case DieMessageType.TransferAnimSetFinished:
                    ret = FromByteArray<DieMessageTransferAnimSetFinished>(data);
                    break;
                case DieMessageType.TransferTestAnimSet:
                    ret = FromByteArray<DieMessageTransferTestAnimSet>(data);
                    break;
                case DieMessageType.TransferTestAnimSetAck:
                    ret = FromByteArray<DieMessageTransferTestAnimSetAck>(data);
                    break;
                case DieMessageType.TransferTestAnimSetFinished:
                    ret = FromByteArray<DieMessageTransferTestAnimSetFinished>(data);
                    break;
                case DieMessageType.TransferSettings:
                    ret = FromByteArray<DieMessageTransferSettings>(data);
                    break;
                case DieMessageType.TransferSettingsAck:
                    ret = FromByteArray<DieMessageTransferSettingsAck>(data);
                    break;
                case DieMessageType.TransferSettingsFinished:
                    ret = FromByteArray<DieMessageTransferSettingsFinished>(data);
                    break;
                case DieMessageType.DebugLog:
                    ret = FromByteArray<DieMessageDebugLog>(data);
                    break;
                case DieMessageType.PlayAnim:
                    ret = FromByteArray<DieMessagePlayAnim>(data);
                    break;
                case DieMessageType.PlayAnimEvent:
                    ret = FromByteArray<DieMessagePlayAnimEvent>(data);
                    break;
                case DieMessageType.StopAnim:
                    ret = FromByteArray<DieMessageStopAnim>(data);
                    break;
                case DieMessageType.RequestState:
                    ret = FromByteArray<DieMessageRequestState>(data);
                    break;
                case DieMessageType.RequestAnimSet:
                    ret = FromByteArray<DieMessageRequestAnimSet>(data);
                    break;
                case DieMessageType.RequestSettings:
                    ret = FromByteArray<DieMessageRequestSettings>(data);
                    break;
                case DieMessageType.RequestTelemetry:
                    ret = FromByteArray<DieMessageRequestTelemetry>(data);
                    break;
                case DieMessageType.FlashFinished:
                    ret = FromByteArray<DieMessageFlashFinished>(data);
                    break;
                case DieMessageType.ProgramDefaultAnimSetFinished:
                    ret = FromByteArray<DieMessageProgramDefaultAnimSetFinished>(data);
                    break;
                case DieMessageType.DefaultAnimSetColor:
                    ret = FromByteArray<DieMessageDefaultAnimSetColor>(data);
                    break;
                case DieMessageType.BatteryLevel:
                    ret = FromByteArray<DieMessageBatteryLevel>(data);
                    break;
                case DieMessageType.RequestBatteryLevel:
                    ret = FromByteArray<DieMessageRequestBatteryLevel>(data);
                    break;
                case DieMessageType.Calibrate:
                    ret = FromByteArray<DieMessageCalibrate>(data);
                    break;
                case DieMessageType.CalibrateFace:
                    ret = FromByteArray<DieMessageCalibrateFace>(data);
                    break;
                case DieMessageType.NotifyUser:
                    ret = FromByteArray<DieMessageNotifyUser>(data);
                    break;
                case DieMessageType.NotifyUserAck:
                    ret = FromByteArray<DieMessageNotifyUserAck>(data);
                    break;
                case DieMessageType.TestHardware:
                    ret = FromByteArray<DieMessageTestHardware>(data);
                    break;
                case DieMessageType.SetStandardState:
                    ret = FromByteArray<DieMessageSetStandardState>(data);
                    break;
                case DieMessageType.SetLEDAnimState:
                    ret = FromByteArray<DieMessageSetLEDAnimState>(data);
                    break;
                case DieMessageType.SetBattleState:
                    ret = FromByteArray<DieMessageSetBattleState>(data);
                    break;
                case DieMessageType.ProgramDefaultParameters:
                    ret = FromByteArray<DieMessageProgramDefaultParameters>(data);
                    break;
                case DieMessageType.ProgramDefaultParametersFinished:
                    ret = FromByteArray<DieMessageProgramDefaultParametersFinished>(data);
                    break;
                case DieMessageType.AttractMode:
                    ret = FromByteArray<DieMessageAttractMode>(data);
                    break;
                case DieMessageType.PrintNormals:
                    ret = FromByteArray<DieMessagePrintNormals>(data);
                    break;
                case DieMessageType.SetDesignAndColor:
                    ret = FromByteArray<DieMessageSetDesignAndColor>(data);
                    break;
                case DieMessageType.SetDesignAndColorAck:
                    ret = FromByteArray<DieMessageSetDesignAndColorAck>(data);
                    break;
                case DieMessageType.SetCurrentBehavior:
                    ret = FromByteArray<DieMessageSetCurrentBehavior>(data);
                    break;
                case DieMessageType.SetCurrentBehaviorAck:
                    ret = FromByteArray<DieMessageSetCurrentBehaviorAck>(data);
                    break;
                default:
                    throw new System.Exception("Unhandled Message type " + type.ToString() + " for marshalling");
            }
        }
        return ret;
    }

    static DieMessage FromByteArray<T>(byte[] data)
        where T : DieMessage
    {
        int size = Marshal.SizeOf<T>();
        if (data.Length == size)
        {
            System.IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, ptr, size);
            var retMessage = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return retMessage;
        }
        else
        {
            Debug.LogError("Wrong message length for type " + typeof(T).Name);
            return null;
        }
    }

    // For virtual dice!
    public static byte[] ToByteArray<T>(T message)
        where T : DieMessage
    {
        int size = Marshal.SizeOf(typeof(T));
        System.IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(message, ptr, false);
        byte[] ret = new byte[size];
        Marshal.Copy(ptr, ret, 0, size);
        Marshal.FreeHGlobal(ptr);
        return ret;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageWhoAreYou
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.WhoAreYou;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageIAmADie
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.IAmADie;

	public byte faceCount; // Which kind of dice this is
	public DiceVariants.DesignAndColor designAndColor; // Physical look
	public byte currentBehaviorIndex;
    public uint dataSetHash;
	public System.UInt64 deviceId; // A unique identifier
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DieMessages.VERSION_INFO_SIZE)]
	public byte[] versionInfo; // Firmware version string, i.e. "10_05"
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageState
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.State;
    public Die.RollState state;
    public byte face;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageAcc
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.Telemetry;

    public AccelFrame data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageBulkSetup
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.BulkSetup;
    public short size;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageBulkSetupAck
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.BulkSetupAck;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageBulkData
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.BulkData;
    public byte size;
    public ushort offset;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DieMessages.maxDataSize)]
    public byte[] data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageBulkDataAck
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.BulkDataAck;
    public ushort offset;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTransferAnimSet
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TransferAnimSet;
	public ushort paletteSize;
	public ushort rgbKeyFrameCount;
	public ushort rgbTrackCount;
	public ushort keyFrameCount;
	public ushort trackCount;
	public ushort animationCount;
	public ushort animationSize;
	public ushort conditionCount;
	public ushort conditionSize;
	public ushort actionCount;
	public ushort actionSize;
	public ushort ruleCount;
	public ushort behaviorCount;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTransferAnimSetAck
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TransferAnimSetAck;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTransferAnimSetFinished
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TransferAnimSetFinished;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTransferTestAnimSet
	: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TransferTestAnimSet;

	public ushort paletteSize;
	public ushort rgbKeyFrameCount;
	public ushort rgbTrackCount;
	public ushort keyFrameCount;
	public ushort trackCount;
	public ushort animationSize;
	public uint hash;
}

public enum TransferTestAnimSetAckType : byte
{
	Download = 0,
	UpToDate,
    NoMemory
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTransferTestAnimSetAck
	: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TransferTestAnimSetAck;
	public TransferTestAnimSetAckType ackType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTransferTestAnimSetFinished
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TransferTestAnimSetFinished;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageRequestAnimSet
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.RequestAnimSet;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTransferSettings
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TransferSettings;
    public byte count;
    public short totalAnimationByteSize;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTransferSettingsAck
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TransferSettingsAck;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTransferSettingsFinished
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TransferSettingsFinished;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageRequestSettings
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.RequestSettings;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageRequestTelemetry
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.RequestTelemetry;
    public byte telemetry;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageDebugLog
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.DebugLog;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DieMessages.maxDataSize)]
    public byte[] data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessagePlayAnim
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.PlayAnim;
    public byte index;
    public byte remapFace;
    public byte loop;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessagePlayAnimEvent
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.PlayAnimEvent;
    public byte evt;
    public byte remapFace;
    public byte loop;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageStopAnim
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.StopAnim;
    public byte index;
    public byte remapFace;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageRequestState
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.RequestState;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageProgramDefaultAnimSet
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.ProgramDefaultAnimSet;
    public uint color;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageProgramDefaultAnimSetFinished
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.ProgramDefaultAnimSetFinished;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageFlash
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.Flash;
    public byte animIndex;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageFlashFinished
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.FlashFinished;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageRequestDefaultAnimSetColor
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.RequestDefaultAnimSetColor;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageDefaultAnimSetColor
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.DefaultAnimSetColor;
    public uint color;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTestBulkSend
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TestBulkSend;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTestBulkReceive
    : DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TestBulkReceive;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageSetAllLEDsToColor
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.SetAllLEDsToColor;
    public uint color;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageBatteryLevel
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.BatteryLevel;
    public float level;
    public float voltage;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageRequestBatteryLevel
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.RequestBatteryLevel;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageCalibrate
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.Calibrate;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageCalibrateFace
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.CalibrateFace;
    public byte face;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageNotifyUser
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.NotifyUser;
    public byte timeout_s;
    public byte ok; // Boolean
    public byte cancel; // Boolean
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DieMessages.maxDataSize - 4)]
    public byte[] data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageNotifyUserAck
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.NotifyUserAck;
    public byte okCancel; // Boolean
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageTestHardware
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.TestHardware;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageSetStandardState
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.SetStandardState;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageSetLEDAnimState
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.SetLEDAnimState;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageSetBattleState
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.SetBattleState;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageProgramDefaultParameters
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.ProgramDefaultParameters;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageProgramDefaultParametersFinished
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.ProgramDefaultParametersFinished;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageAttractMode
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.AttractMode;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessagePrintNormals
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.PrintNormals;
    public byte face;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageSetDesignAndColor
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.SetDesignAndColor;
    public DiceVariants.DesignAndColor designAndColor;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageSetDesignAndColorAck
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.SetDesignAndColorAck;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageSetCurrentBehavior
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.SetCurrentBehavior;
    public byte currentBehaviorIndex;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DieMessageSetCurrentBehaviorAck
: DieMessage
{
    public DieMessageType type { get; set; } = DieMessageType.SetCurrentBehaviorAck;
}

