using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animations;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

public class Die
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

    public enum State
	{
		Unknown = 0,
        Idle,
		Handling,
		Falling,
		Rolling,
		Jerking,
		Crooked,
		Count
	}

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

    public enum SpecialColor
    {
        None = 0,
        Face,           // Uses the color of the face (based on a rainbow)
        ColorWheel,     // Uses how hot the die is (based on how much its being shaken)
        HeatCurrent,    // Uses the current 'heat' value to determine color
        HeatStart       // Evaluate the color based on heat only once at the start of the animation
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
    public State state { get; private set; } = State.Unknown;
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

    public delegate void StateChangedEvent(Die die, State newState);
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

    #region Message Handling Infrastructure
    void AddMessageHandler(DieMessageType msgType, MessageReceivedDelegate newDel)
    {
        MessageReceivedDelegate del;
        if (messageDelegates.TryGetValue(msgType, out del))
        {
            del += newDel;
            messageDelegates[msgType] = del;
        }
        else
        {
            messageDelegates.Add(msgType, newDel);
        }
    }

    void RemoveMessageHandler(DieMessageType msgType, MessageReceivedDelegate newDel)
    {
        MessageReceivedDelegate del;
        if (messageDelegates.TryGetValue(msgType, out del))
        {
            del -= newDel;
            if (del == null)
            {
                messageDelegates.Remove(msgType);
            }
            else
            {
                messageDelegates[msgType] = del;
            }
        }
    }

    void PostMessage<T>(T message)
        where T : DieMessage
    {
        byte[] msgBytes = DieMessages.ToByteArray(message);
        DicePool.Instance.WriteDie(this, msgBytes, msgBytes.Length, null);
    }

    IEnumerator WaitForMessageCr(DieMessageType msgType, System.Action<DieMessage> msgReceivedCallback)
    {
        bool msgReceived = false; 
        DieMessage msg = default(DieMessage);
        MessageReceivedDelegate callback = (ackMsg) =>
        {
            msgReceived = true;
            msg = ackMsg;
        };

        AddMessageHandler(msgType, callback);
        yield return new WaitUntil(() => msgReceived);
        RemoveMessageHandler(msgType, callback);
        if (msgReceivedCallback != null)
        {
            msgReceivedCallback.Invoke(msg);
        }
    }

    IEnumerator SendMessageWithAckOrTimeoutCr<T>(T message, DieMessageType ackType, float timeOut, System.Action<DieMessage> ackAction, System.Action timeoutAction)
        where T : DieMessage
    {
        DieMessage ackMessage = null;
        float startTime = Time.time;
        MessageReceivedDelegate callback = (ackMsg) =>
        {
            ackMessage = ackMsg;
        };

        AddMessageHandler(ackType, callback);
        byte[] msgBytes = DieMessages.ToByteArray(message);
        DicePool.Instance.WriteDie(this, msgBytes, msgBytes.Length, null);
        while (ackMessage == null && Time.time < startTime + timeOut)
        {
            yield return null;
        }
        RemoveMessageHandler(ackType, callback);
        if (ackMessage != null)
        {
            ackAction?.Invoke(ackMessage);
        }
        else
        {
            timeoutAction?.Invoke();
        }
    }

    IEnumerator SendMessageWithAckRetryCr<T>(T message, DieMessageType ackType, System.Action<DieMessage> ackAction)
        where T : DieMessage
    {
        DieMessage msgReceived = null;
        System.Action<DieMessage> msgAction = (msg) =>
        {
            msgReceived = msg;
        };
        while (msgReceived == null)
        {
            // Retry every half second if necessary
            yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(message, ackType, 0.5f, msgAction, null));
        }
        ackAction?.Invoke(msgReceived);
    }

    #endregion

        #region MessageHandlers
    void OnStateMessage(DieMessage message)
    {
        // Handle the message
        var stateMsg = (DieMessageState)message;
        Debug.Log("State: " + ((State)(stateMsg.state)).ToString() + ", " + stateMsg.face);

        var newState = (State)stateMsg.state;
        var newFace = stateMsg.face;
        if (newState != state || newFace != face)
        {
            state = newState;
            face = newFace;

            // Notify anyone who cares
            OnStateChanged?.Invoke(this, state);
        }
    }

    void OnTelemetryMessage(DieMessage message)
    {
        // Don't bother doing anything with the message if we don't have
        // anybody interested in telemetry data.
        if (_OnTelemetry != null)
        {
            // Notify anyone who cares
            var telem = (DieMessageAcc)message;
            _OnTelemetry.Invoke(this, telem.data);
        }
    }

    void OnDebugLogMessage(DieMessage message)
    {
        var dlm = (DieMessageDebugLog)message;
        string text = System.Text.Encoding.UTF8.GetString(dlm.data, 0, dlm.data.Length);
        Debug.Log(name + ": " + text);
    }

    void OnNotifyUserMessage(DieMessage message)
    {
        var notifyUserMsg = (DieMessageNotifyUser)message;
        bool ok = notifyUserMsg.ok != 0;
        bool cancel = notifyUserMsg.cancel != 0;
        float timeout = (float)notifyUserMsg.timeout_s;
        string text = System.Text.Encoding.UTF8.GetString(notifyUserMsg.data, 0, notifyUserMsg.data.Length);
        var uiInstance = NotificationUI.Instance;
        if (uiInstance != null)
        {
            // Show the message and tell the die when user clicks Ok!
            uiInstance.Show(text, ok, cancel, timeout, (res) =>
            {
                PostMessage(new DieMessageNotifyUserAck() { okCancel = (byte)(res ? 1 : 0) });
            });
        }
    }
    #endregion

    #region Bluetooth Operations
    Coroutine PerformBluetoothOperation(IEnumerator operationCr)
    {
        return StartCoroutine(PerformBluetoothOperationCr(operationCr));
    }

    Coroutine PerformBluetoothOperation(System.Action action)
    {
        return StartCoroutine(PerformBluetoothOperationCr(PerformActioShimCr(action)));
    }

    IEnumerator PerformActioShimCr(System.Action action)
    {
        action();
        yield break;
    }

    IEnumerator PerformBluetoothOperationCr(IEnumerator operationCr)
    {
        if (connectionState >= ConnectionState.Connected)
        {
            while (bluetoothOperationInProgress)
            {
                // Busy, wait until we can talk to the die
                yield return null;
            }
            bool errorOccured = false;
            DicePool.BluetoothErrorEvent errorHandler = (err) => errorOccured = true;
            try
            {
                DicePool.Instance.onBluetoothError += errorHandler;
                bluetoothOperationInProgress = true;
                // Attach to the error event
                yield return StartCoroutine(operationCr);
                if (errorOccured)
                {
                    Debug.LogError("Die " + name + " error while performing action");
                }
            }
            finally
            {
                DicePool.Instance.onBluetoothError -= errorHandler;
                bluetoothOperationInProgress = false;
            }
        }
        else
        {
            throw new System.Exception("Die not connected");
        }
    }

    public Coroutine PlayAnimation(int animationIndex)
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessagePlayAnim() { index = (byte)animationIndex }));
    }

    public Coroutine PlayAnimationEvent(AnimationEvent evt)
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessagePlayAnimEvent() { evt = (byte)evt }));
    }

    public Coroutine PlayAnimation(int animationIndex, int remapFace, bool loop)
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessagePlayAnim()
        {
            index = (byte)animationIndex, remapFace = (byte)remapFace, loop = loop ? (byte)1 : (byte)0
        }));
    }

    public Coroutine StopAnimation(int animationIndex, int remapIndex)
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessageStopAnim()
        {
            index = (byte)animationIndex,
            remapFace = (byte)remapIndex,
        }));
    }

    public Coroutine StartAttractMode()
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessageAttractMode()));
    }

    public Coroutine Ping()
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessageRequestState()));
    }

    public Coroutine GetDieType()
    {
        return PerformBluetoothOperation(GetDieTypeCr());
    }

    IEnumerator GetDieTypeCr()
    {
        var whoAreYouMsg = new DieMessageWhoAreYou();
        System.Action<DieMessage> setDieId = (msg) =>
        {
            var idMsg = (DieMessageIAmADie)msg;
            dieType = (DieType)idMsg.id;
        };
        yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(whoAreYouMsg, DieMessageType.IAmADie, 5, setDieId, null));
    }

    public Coroutine UploadBulkData(byte[] bytes)
    {
        return PerformBluetoothOperation(UploadBulkDataCr(bytes));
    }

    IEnumerator UploadBulkDataCr(byte[] bytes)
    {
        short remainingSize = (short)bytes.Length;

        Debug.Log("Sending " + remainingSize + " bulk data");
        // Send setup message
        var setup = new DieMessageBulkSetup();
        setup.size = remainingSize;
        yield return StartCoroutine(SendMessageWithAckRetryCr(setup, DieMessageType.BulkSetupAck, null));

        Debug.Log("Die is ready, sending data");

        // Then transfer data
        ushort offset = 0;
        while (remainingSize > 0)
        {
            var data = new DieMessageBulkData();
            data.offset = offset;
            data.size = (byte)Mathf.Min(remainingSize, DieMessages.maxDataSize);
            data.data = new byte[DieMessages.maxDataSize];
            System.Array.Copy(bytes, offset, data.data, 0, data.size);
            yield return StartCoroutine(SendMessageWithAckRetryCr(data, DieMessageType.BulkDataAck, null));
            remainingSize -= data.size;
            offset += data.size;
        }

        Debug.Log("Finished sending bulk data");
    }


    public Coroutine DownloadBulkData(System.Action<byte[]> onBufferReady)
    {
        return PerformBluetoothOperation(DownloadBulkDataCr(onBufferReady));
    }

    IEnumerator DownloadBulkDataCr(System.Action<byte[]> onBufferReady) 
    {
        // Wait for setup message
        short size = 0;
        yield return StartCoroutine(WaitForMessageCr(DieMessageType.BulkSetup, (msg) =>
        {
            var setupMsg = (DieMessageBulkSetup)msg;
            size = setupMsg.size;
        }));

        // Allocate a byte buffer
        byte[] buffer = new byte[size];
        ushort totalDataReceived = 0;

        // Setup bulk receive handler
        MessageReceivedDelegate bulkReceived = (msg) =>
        {
            var bulkMsg = (DieMessageBulkData)msg;
            System.Array.Copy(bulkMsg.data, 0, buffer, bulkMsg.offset, bulkMsg.size);

            // Create acknowledge message now
            var msgAck = new DieMessageBulkDataAck();
            msgAck.offset = totalDataReceived;

            // Sum data receive before sending any other message
            totalDataReceived += bulkMsg.size;

            // Send acknowledgment (no need to do it synchronously)
            PostMessage(msgAck);
        };
        AddMessageHandler(DieMessageType.BulkData, bulkReceived);

        // Send acknowledgement to the die, so it may transfer bulk data immediately
        PostMessage(new DieMessageBulkSetupAck());

        // Wait for all the bulk data to be received
        yield return new WaitUntil(() => totalDataReceived == size);

        // We're done
        RemoveMessageHandler(DieMessageType.BulkData, bulkReceived);
        onBufferReady.Invoke(buffer);
    }

    public Coroutine UploadAnimationSet(AnimationSet set, System.Action<bool> callBack)
    {
        return PerformBluetoothOperation(UploadAnimationSetCr(set, callBack));
    }

    IEnumerator UploadAnimationSetCr(AnimationSet set, System.Action<bool> callBack)
    {
        // Prepare the die
        var prepareDie = new DieMessageTransferAnimSet();
        prepareDie.paletteSize = set.getPaletteSize();
        prepareDie.keyFrameCount = set.getKeyframeCount();
        prepareDie.rgbTrackCount = set.getRGBTrackCount();
        prepareDie.trackCount = set.getTrackCount();
        prepareDie.animationCount = set.getAnimationCount();
        prepareDie.heatTrackIndex = set.heatTrackIndex;
        Debug.Log("Animation Data to be sent:");
        Debug.Log("palette: " + prepareDie.paletteSize * Marshal.SizeOf<byte>());
        Debug.Log("keyframes: " + prepareDie.keyFrameCount + " * " + Marshal.SizeOf<Animations.RGBKeyframe>());
        Debug.Log("rgb tracks: " + prepareDie.rgbTrackCount + " * " + Marshal.SizeOf<Animations.RGBTrack>());
        Debug.Log("tracks: " + prepareDie.trackCount + " * " + Marshal.SizeOf<Animations.AnimationTrack>());
        Debug.Log("animations: " + prepareDie.animationCount + " * " + Marshal.SizeOf<Animations.Animation>());
        Debug.Log("heat track: " + prepareDie.heatTrackIndex);
        bool timeout = false;
        yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(prepareDie, DieMessageType.TransferAnimSetAck, 3.0f, null, () => timeout = true));
        if (!timeout)
        {
            Debug.Log("die is ready, sending animations");
            Debug.Log("byte array should be: " + set.ComputeAnimationDataSize());
            var setData = set.ToByteArray();
            yield return StartCoroutine(UploadBulkDataCr(setData));

            // We're done!
            Debug.Log("Done!");
            callBack?.Invoke(true);
        }
        else
        {
            Debug.Log("TimedOut");
            callBack?.Invoke(false);
        }
    }

    //public IEnumerator DownloadAnimationSet(AnimationSet outSet)
    //{
    //    // Request the anim set from the die
    //    SendMessage(new DieMessageRequestAnimSet());

    //    // Now wait for the setup message back
    //    int animCount = 0;
    //    yield return StartCoroutine(WaitForMessage(DieMessageType.TransferAnimSet, (msg) =>
    //    {
    //        var setupMsg = (DieMessageTransferAnimSet)msg;
    //        animCount = setupMsg.count;
    //    }));

    //    // Got the message, acknowledge it
    //    StartCoroutine(SendMessage(new DieMessageTransferAnimSetAck()));

    //    outSet.animations = new RGBAnimation[animCount];
    //    for (int i = 0; i < animCount; ++i)
    //    {
    //        byte[] animData = null;
    //        yield return StartCoroutine(DownloadBulkData((buf) => animData = buf));
    //        outSet.animations[i] = RGBAnimation.FromByteArray(animData);

    //        // Tell die we're ready for next anim
    //        StartCoroutine(SendMessage(new DieMessageTransferAnimReadyForNextAnim()));
    //    }

    //    // We've read all the anims!
    //}


    public Coroutine UploadSettings(DieSettings settings)
    {
        return PerformBluetoothOperation(UploadSettingsCr(settings));
    }

    IEnumerator UploadSettingsCr(DieSettings settings)
    {
        // Prepare the die
        var prepareDie = new DieMessageTransferSettings();
        yield return StartCoroutine(SendMessageWithAckRetryCr(prepareDie, DieMessageType.TransferSettingsAck, null));

        // Die is ready, perform bulk transfer of the settings
        byte[] settingsBytes = DieSettings.ToByteArray(settings);
        yield return StartCoroutine(UploadBulkDataCr(settingsBytes));

        // We're done!
    }

    public Coroutine DownloadSettings(System.Action<DieSettings> settingsReadCallback)
    {
        return PerformBluetoothOperation(DownloadSettingsCr(settingsReadCallback));
    }

    IEnumerator DownloadSettingsCr(System.Action<DieSettings> settingsReadCallback)
    {
        // Request the settings from the die
        yield return StartCoroutine(SendMessageWithAckRetryCr(new DieMessageRequestSettings(), DieMessageType.TransferSettings, null));

        // Got the message, acknowledge it
        PostMessage(new DieMessageTransferSettingsAck());

        byte[] settingsBytes = null;
        yield return StartCoroutine(DownloadBulkDataCr((buf) => settingsBytes = buf));
        var newSettings = DieSettings.FromByteArray(settingsBytes);

        // We've read the settings
        settingsReadCallback.Invoke(newSettings);
    }

    public Coroutine RequestTelemetry(bool on)
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessageRequestTelemetry() { telemetry = on ? (byte)1 : (byte)0 }));
    }

    public Coroutine RequestBulkData()
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessageTestBulkSend()));
    }

    public Coroutine PrepareBulkData()
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessageTestBulkReceive()));
    }

    public Coroutine SetLEDsToRandomColor()
    {
        var msg = new DieMessageSetAllLEDsToColor();
        uint r = (byte)Random.Range(0, 256);
        uint g = (byte)Random.Range(0, 256);
        uint b = (byte)Random.Range(0, 256);
        msg.color = (r << 16) + (g << 8) + b;
        return PerformBluetoothOperation(() => PostMessage(msg));
    }

    public Coroutine SetLEDsToColor(Color color)
    {
        var msg = new DieMessageSetAllLEDsToColor();
        Color32 color32 = color;
        msg.color = (uint)((color32.r << 16) + (color32.g << 8) + color32.b);
        return PerformBluetoothOperation(() => PostMessage(msg));
    }

    public Coroutine GetBatteryLevel(System.Action<float?> outLevelAction)
    {
        return PerformBluetoothOperation(GetBatteryLevelCr(outLevelAction));
    }

    IEnumerator GetBatteryLevelCr(System.Action<float?> outLevelAction)
    {
        yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(
            new DieMessageRequestBatteryLevel(),
            DieMessageType.BatteryLevel,
            5.0f,
            (msg) =>
            {
                var lvlMsg = (DieMessageBatteryLevel)msg;
                outLevelAction?.Invoke(lvlMsg.level);
            },
            () =>
            {
                outLevelAction?.Invoke(null);
            }));
    }

    public void StartHardwareTest()
    {
        PostMessage(new DieMessageTestHardware());
    }

    public void StartCalibration()
    {
        PostMessage(new DieMessageCalibrate());
    }

    public void CalibrateFace(int face)
    {
        PostMessage(new DieMessageCalibrateFace() {face = (byte)face});
    }

    public void SetLEDAnimatorMode()
    {
        PostMessage(new DieMessageSetLEDAnimState());
    }

    public void SetBattleMode()
    {
        PostMessage(new DieMessageSetBattleState());
    }

    public void PrintNormals()
    {
        StartCoroutine(PrintNormalsCr());
    }

    IEnumerator PrintNormalsCr()
    {
        for (int i = 0; i < 20; ++i)
        {
            var msg = new DieMessagePrintNormals();
            msg.face = (byte)i;
            PostMessage(msg);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ResetParams()
    {
        PostMessage(new DieMessageProgramDefaultParameters());
    }

    #endregion
}
