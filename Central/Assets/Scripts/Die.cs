using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animations;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

public partial class Die
	: MonoBehaviour
    , Central.IDie
{
	const float SCALE_2G = 2.0f;
	const float SCALE_4G = 4.0f;
	const float SCALE_8G = 8.0f;
	const float scale = SCALE_8G;

    public enum DieType
    {
        Unknown = 0,
        SixSided,
        TwentySided
    };

    public enum RollState : byte
    {
        Unknown = 0,
        OnFace,
        Handling,
        Rolling,
        Crooked
    };

    public enum AnimationEvent
    {
        None = 0,
        Hello,
        Connected,
        Disconnected,
        LowBattery,
        ChargingStart,
        ChargingDone,
        ChargingError,
        Handling,
        Rolling,
        OnFace_Default,
		OnFace_00,
		OnFace_01,
		OnFace_02,
		OnFace_03,
		OnFace_04,
		OnFace_05,
		OnFace_06,
		OnFace_07,
		OnFace_08,
		OnFace_09,
		OnFace_10,
		OnFace_11,
		OnFace_12,
		OnFace_13,
		OnFace_14,
		OnFace_15,
		OnFace_16,
		OnFace_17,
		OnFace_18,
		OnFace_19,
        Crooked,
        Battle_ShowTeam,
        Battle_FaceUp,
        Battle_WaitingForBattle,
        Battle_Duel,
        Battle_DuelWin,
        Battle_DuelLose,
        Battle_DuelDraw,
        Battle_TeamWin,
        Battle_TeamLoose,
        Battle_TeamDraw,
        AttractMode,
        Heat,
        // Etc...
        Count
    }

    [System.Serializable]
    public struct Settings
    {
        string name;
        float sigmaDecayFast;
        float sigmaDecaySlow;
        int minRollTime; // ms
    }

    public enum ConnectionState
    {
        Disconnected = 0,
        Advertising,
        Connecting,
        Connected,
        FetchingId,
        FetchingState,
        Ready
    }

    ConnectionState _connectionState = ConnectionState.Disconnected;
    public ConnectionState connectionState
    {
        get { return _connectionState; }
        private set
        {
            if (value != _connectionState)
            {
                _connectionState = value;
                OnConnectionStateChanged?.Invoke(this, value);
            }
        }
    }

    public DieType dieType { get; private set; } = DieType.Unknown;
    public RollState state { get; private set; } = RollState.Unknown;
    public string address { get; private set; } = "";
    public int face { get; private set; } = -1;

	public delegate void TelemetryEvent(Die die, AccelFrame frame);
    public TelemetryEvent _OnTelemetry;
    public event TelemetryEvent OnTelemetry
    {
        add
        {
            if (_OnTelemetry == null)
            {
                // The first time around, we make sure to request telemetry from the die
                RequestTelemetry(true);
            }
            _OnTelemetry += value;
        }
        remove
        {
            _OnTelemetry -= value;
            if (_OnTelemetry == null || _OnTelemetry.GetInvocationList().Length == 0)
            {
                if (connectionState == ConnectionState.Ready)
                {
                    // Deregister from the die telemetry
                    RequestTelemetry(false);
                }
                // Otherwise we can't send bluetooth packets to the die, can we?
            }
        }
    }

    public delegate void StateChangedEvent(Die die, RollState newState);
    public StateChangedEvent OnStateChanged;

    public delegate void ConnectionStateChangedEvent(Die die, ConnectionState newConnectionState);
    public ConnectionStateChangedEvent OnConnectionStateChanged;

    public delegate void SettingsChangedEvent(Die die);
    public SettingsChangedEvent OnSettingsChanged;

    // Lock so that only one 'operation' can happen at a time on a die
    // Note: lock is not a real multithreaded lock!
    bool bluetoothOperationInProgress = false;

    // Internal delegate per message type
    delegate void MessageReceivedDelegate(DieMessage msg);
    Dictionary<DieMessageType, MessageReceivedDelegate> messageDelegates;

    void Awake()
	{
        messageDelegates = new Dictionary<DieMessageType, MessageReceivedDelegate>();

        // Setup delegates for face and telemetry
        messageDelegates.Add(DieMessageType.State, OnStateMessage);
        messageDelegates.Add(DieMessageType.Telemetry, OnTelemetryMessage);
        messageDelegates.Add(DieMessageType.DebugLog, OnDebugLogMessage);
        messageDelegates.Add(DieMessageType.NotifyUser, OnNotifyUserMessage);
    }

    public void Setup(string name, string address)
    {
        this.name = name;
        this.address = address;
        this.connectionState = ConnectionState.Disconnected;
    }

    public void Connect()
    {
        StartCoroutine(ConnectCr());
    }

    IEnumerator ConnectCr()
    {
        // Kick off connection
        DicePool.Instance.ConnectDie(this);

        // Wait until the die is either ready or disconnected because of some error
        yield return new WaitUntil(() => connectionState == ConnectionState.Connected || connectionState == ConnectionState.Disconnected);

        if (connectionState == ConnectionState.Connected)
        {
            // Ask the die who it is!Upload
            connectionState = ConnectionState.FetchingId;
            yield return GetDieType();

            // Ping the die so we know its initial state
            connectionState = ConnectionState.FetchingState;
            yield return Ping();

            connectionState = ConnectionState.Ready;
        }
    }

    public void Disconnect()
    {
        DicePool.Instance.DisconnectDie(this);
    }

    public void OnAdvertising()
    {
        connectionState = ConnectionState.Advertising;
    }

	public void OnConnected()
	{
        connectionState = ConnectionState.Connected;
	}

    public void OnDisconnected()
    {
        connectionState = ConnectionState.Disconnected;
    }

    public void OnData(byte[] data)
    {
        // Process the message coming from the actual die!
        var message = DieMessages.FromByteArray(data);
        MessageReceivedDelegate del;
        if (messageDelegates.TryGetValue(message.type, out del))
        {
            del.Invoke(message);
        }
    }

}
