using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animations;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

public class Die
	: MonoBehaviour
{
	public const float SCALE_2G = 2.0f;
	public const float SCALE_4G = 4.0f;
	public const float SCALE_8G = 8.0f;
	float scale = SCALE_8G;

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
        PickUp = 0,
        Error,
        LowBattery,
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

    public bool connected { get; private set; } = false;

    public State state { get; private set; } = State.Unknown;

    // Name is already a part of Monobehaviour
    public string address { get; private set; } = "";

    public int face { get; private set; } = -1;

	public delegate void TelemetryEvent(Die die, Vector3 acc, int millis);
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
                if (connected)
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

    public delegate void ConnectionStateChangedEvent(Die die, bool newConnectionState);
    public ConnectionStateChangedEvent OnConnectionStateChanged;

    public delegate void FaceChangedEvent(Die die, int newFace);
    public FaceChangedEvent OnFaceChanged;

    public delegate void SettingsChangedEvent(Die die);
    public SettingsChangedEvent OnSettingsChanged;

	// For telemetry
	int lastSampleTime; // ms
	ISendBytes _sendBytes;
    TelemetryEvent _OnTelemetry;

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
        messageDelegates.Add(DieMessageType.Face, OnNewFaceMessage);
        messageDelegates.Add(DieMessageType.Telemetry, OnTelemetryMessage);
        messageDelegates.Add(DieMessageType.DebugLog, OnDebugLogMessage);
    }

    public void Setup(string name, string address)
    {
        this.name = name;
        this.address = address;
    }

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void Connect(ISendBytes sb)
	{
		_sendBytes = sb;
		state = State.Unknown;
        OnConnectionStateChanged?.Invoke(this, true);

        // Ping the die so we know its initial state
        Ping();
	}

    public void Disconnect()
    {
        connected = false;
        _sendBytes = null;

        OnConnectionStateChanged?.Invoke(this, false);
    }

    #region Message Handling Infrastructure
    public void DataReceived(byte[] data)
	{
		if (!connected)
		{
			Debug.LogError("Die " + name + " received data while disconnected!");
			return;
		}

        // Process the message coming from the actual die!
        var message = DieMessages.FromByteArray(data);
        MessageReceivedDelegate del;
        if (messageDelegates.TryGetValue(message.type, out del))
        {
            del.Invoke(message);
        }
	}

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
        _sendBytes.SendBytes(this, msgBytes, msgBytes.Length, null);
    }

    IEnumerator SendMessageCr<T>(T message)
        where T : DieMessage
    {
        bool msgReceived = false;
        byte[] msgBytes = DieMessages.ToByteArray(message);
        _sendBytes.SendBytes(this, msgBytes, msgBytes.Length, () => msgReceived = true);
        yield return new WaitUntil(() => msgReceived);
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

    IEnumerator SendMessageWithAckCr<T>(T message, DieMessageType ackType)
        where T : DieMessage
    {
        bool msgReceived = false;
        MessageReceivedDelegate callback = (ackMsg) =>
        {
            msgReceived = true;
        };

        AddMessageHandler(ackType, callback);
        byte[] msgBytes = DieMessages.ToByteArray(message);
        _sendBytes.SendBytes(this, msgBytes, msgBytes.Length, null);

        yield return new WaitUntil(() => msgReceived);
        RemoveMessageHandler(ackType, callback);
    }

    IEnumerator SendMessageWithAckOrTimeoutCr<T>(T message, DieMessageType ackType, float timeOut)
        where T : DieMessage
    {
        bool msgReceived = false;
        float startTime = Time.time;
        MessageReceivedDelegate callback = (ackMsg) =>
        {
            msgReceived = true;
        };

        AddMessageHandler(ackType, callback);
        byte[] msgBytes = DieMessages.ToByteArray(message);
        _sendBytes.SendBytes(this, msgBytes, msgBytes.Length, null);
        while (!msgReceived && Time.time < startTime + timeOut)
        {
            yield return null;
        }
        RemoveMessageHandler(ackType, callback);
    }
    #endregion

    #region MessageHandlers
    void OnStateMessage(DieMessage message)
    {
        // Handle the message
        var stateMsg = (DieMessageState)message;

        var newState = (State)stateMsg.face;
        if (newState != state)
        {
            state = newState;

            // Notify anyone who cares
            OnStateChanged?.Invoke(this, state);
        }
    }

    void OnNewFaceMessage(DieMessage message)
    {
        var faceMsg = (DieMessageFace)message;
        if (faceMsg.face != face)
        {
            face = faceMsg.face;

            // Notify anyone who cares
            OnFaceChanged?.Invoke(this, face);
        }
    }

    void OnTelemetryMessage(DieMessage message)
    {
        // Don't bother doing anything with the message if we don't have
        // anybody interested in telemetry data.
        if (_OnTelemetry != null)
        {
            var telem = (DieMessageAcc)message;

            for (int i = 0; i < 2; ++i)
            {
                // Compute actual accelerometer readings (in Gs)
                float cx = (float)telem.data[i].X / (float)(1 << 11) * (float)(scale);
                float cy = (float)telem.data[i].Y / (float)(1 << 11) * (float)(scale);
                float cz = (float)telem.data[i].Z / (float)(1 << 11) * (float)(scale);
                Vector3 acc = new Vector3(cx, cy, cz);
                lastSampleTime += telem.data[i].DeltaTime; 

                // Notify anyone who cares
                _OnTelemetry.Invoke(this, acc, lastSampleTime);
            }
        }
    }

    void OnDebugLogMessage(DieMessage message)
    {
        var dlm = (DieMessageDebugLog)message;
        string text = System.Text.Encoding.UTF8.GetString(dlm.data, 0, dlm.data.Length);
        Debug.Log(name + ": " + text);
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
        if (connected)
        {
            while (bluetoothOperationInProgress)
            {
                // Busy, wait until we can talk to the die
                yield return null;
            }
            try
            {
                bluetoothOperationInProgress = true;
                yield return StartCoroutine(operationCr);
            }
            finally
            {
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
        return PerformBluetoothOperation(SendMessageCr(new DieMessagePlayAnim() { index = (byte)animationIndex }));
    }

    public Coroutine Ping()
    {
        return PerformBluetoothOperation(SendMessageCr(new DieMessageRequestState()));
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
        yield return StartCoroutine(SendMessageWithAckCr(setup, DieMessageType.BulkSetupAck));

        Debug.Log("Die is ready, sending data");

        // Then transfer data
        ushort offset = 0;
        while (remainingSize > 0)
        {
            var data = new DieMessageBulkData();
            data.offset = offset;
            data.size = (byte)Mathf.Min(remainingSize, 16);
            data.data = new byte[data.size];
            System.Array.Copy(bytes, offset, data.data, 0, data.size);
            yield return StartCoroutine(SendMessageWithAckCr(data, DieMessageType.BulkDataAck));
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
            StartCoroutine(SendMessageCr(msgAck));
        };
        AddMessageHandler(DieMessageType.BulkData, bulkReceived);

        // Send acknowledgement to the die, so it may transfer bulk data immediately
        StartCoroutine(SendMessageCr(new DieMessageBulkSetupAck()));

        // Wait for all the bulk data to be received
        yield return new WaitUntil(() => totalDataReceived == size);

        // We're done
        RemoveMessageHandler(DieMessageType.BulkData, bulkReceived);
        onBufferReady.Invoke(buffer);
    }

    public Coroutine UploadAnimationSet(AnimationSet set)
    {
        return PerformBluetoothOperation(UploadAnimationSetCr(set));
    }

    IEnumerator UploadAnimationSetCr(AnimationSet set)
    {
        // Prepare the die
        var prepareDie = new DieMessageTransferAnimSet();
        prepareDie.paletteSize = set.getPaletteSize();
        prepareDie.keyFrameCount = set.getKeyframeCount();
        prepareDie.rgbTrackCount = set.getRGBTrackCount();
        prepareDie.trackCount = set.getTrackCount();
        prepareDie.animationCount = set.getAnimationCount();
        Debug.Log("Animation Data to be sent:");
        Debug.Log("palette: " + prepareDie.paletteSize * Marshal.SizeOf<byte>());
        Debug.Log("keyframes: " + prepareDie.keyFrameCount + " * " + Marshal.SizeOf<Animations.RGBKeyframe>());
        Debug.Log("rgb tracks: " + prepareDie.rgbTrackCount + " * " + Marshal.SizeOf<Animations.RGBTrack>());
        Debug.Log("tracks: " + prepareDie.trackCount + " * " + Marshal.SizeOf<Animations.AnimationTrack>());
        Debug.Log("animations: " + prepareDie.animationCount + " * " + Marshal.SizeOf<Animations.Animation>());
        yield return StartCoroutine(SendMessageWithAckCr(prepareDie, DieMessageType.TransferAnimSetAck));

        Debug.Log("die is ready, sending animations");
        Debug.Log("byte array should be: " + set.ComputeAnimationDataSize());
        var setData = set.ToByteArray();
        yield return StartCoroutine(UploadBulkDataCr(setData));

        // We're done!
        Debug.Log("Done!");
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
        yield return StartCoroutine(SendMessageWithAckCr(prepareDie, DieMessageType.TransferSettingsAck));

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
        SendMessageCr(new DieMessageRequestSettings());

        // Now wait for the setup message back
        yield return StartCoroutine(WaitForMessageCr(DieMessageType.TransferSettings, null));

        // Got the message, acknowledge it
        StartCoroutine(SendMessageCr(new DieMessageTransferSettingsAck()));

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

    public Coroutine SetNewColor(System.Action<Color> displayColor)
    {
        return PerformBluetoothOperation(SetNewColorCr(displayColor));
    }

    IEnumerator SetNewColorCr(System.Action<Color> displayColor)
    {
        float hue = Random.Range(0.0f, 1.0f);
        Color newDisplayColor = Color.HSVToRGB(hue, 1.0f, 1.0f);
        Color newColor = Color.HSVToRGB(hue, 1.0f, 0.5f);

        Color32 color32 = newColor;
        int colorRGB = color32.r << 16 | color32.g << 8 | color32.b;

        yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(new DieMessageProgramDefaultAnimSet() { color = (uint)colorRGB }, DieMessageType.ProgramDefaultAnimSetFinished, 5.0f));

        if (OnSettingsChanged != null)
        {
            OnSettingsChanged(this);
        }

        if (displayColor != null)
        {
            displayColor(newDisplayColor);
        }
    }

    public Coroutine Flash(int index)
    {
        return PerformBluetoothOperation(SendMessageWithAckOrTimeoutCr(new DieMessageFlash() { animIndex = (byte)index }, DieMessageType.FlashFinished, 5.0f));
    }

    public Coroutine Rename(string newName)
    {
        return PerformBluetoothOperation(RenameCr(newName));
    }

    IEnumerator RenameCr(string newName)
    {
        gameObject.name = newName;
        name = newName;
        yield return null;
        //yield return StartCoroutine(SendMessageWithAckOrTimeout(new DieMessageRename() { newName = newName }, DieMessageType.RenameFinished, 5.0f));
        //if (OnSettingsChanged != null)
        //{
        //    OnSettingsChanged(this);
        //}
    }

    public Coroutine GetDefaultAnimSetColor(System.Action<Color> retColor)
    {
        return PerformBluetoothOperation(GetDefaultAnimSetColorCr(retColor));
    }

    IEnumerator GetDefaultAnimSetColorCr(System.Action<Color> retColor)
    {
        // Setup message handler
        MessageReceivedDelegate defaultAnimSetColorHandler = (msg) =>
        {
            var bulkMsg = (DieMessageDefaultAnimSetColor)msg;
            Color32 msgColor = new Color32(
                (byte)((bulkMsg.color >> 16) & 0xFF),
                (byte)((bulkMsg.color >> 8) & 0xFF),
                (byte)((bulkMsg.color >> 0) & 0xFF),
                0xFF);
            float h, s, v;
            Color.RGBToHSV(msgColor, out h, out s, out v);
            retColor(Color.HSVToRGB(h, 1, 1));
        };
        AddMessageHandler(DieMessageType.DefaultAnimSetColor, defaultAnimSetColorHandler);

        yield return StartCoroutine(SendMessageCr(new DieMessageRequestDefaultAnimSetColor()));

        // We're done
        RemoveMessageHandler(DieMessageType.DefaultAnimSetColor, defaultAnimSetColorHandler);
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

    #endregion
}
