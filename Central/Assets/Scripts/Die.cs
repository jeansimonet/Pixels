using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animations;
using System.Text;
using System.Runtime.InteropServices;

public class Die
	: MonoBehaviour
{
	public const float SCALE_2G = 2.0f;
	public const float SCALE_4G = 4.0f;
	public const float SCALE_8G = 8.0f;
	float scale = SCALE_8G;

	public enum State
	{
		Disconnected = -1,
		Unknown = 0,
		Face1 = 1,
		Face2,
		Face3,
		Face4,
		Face5,
		Face6,
		Handling,
		Falling,
		Rolling,
		Jerking,
		Crooked,
		Count
	}

    public State state { get; private set; } = State.Disconnected;

	// Name is already a part of Monobehaviour
	public string address
	{
		get;
		set;
	}

	public bool connected
	{
		get
		{
			return state != State.Disconnected;
		}
	}

	public int face
	{
		get
		{
			int ret = (int)state;
			if (ret < 1 || ret > 6)
			{
				ret = -1;
			}
			return ret;
		}
	}

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
                if (state != State.Disconnected)
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

    public delegate void SettingsChangedEvent(Die die);
    public SettingsChangedEvent OnSettingsChanged;

	// For telemetry
	int lastSampleTime; // ms
	ISendBytes _sendBytes;
    TelemetryEvent _OnTelemetry;

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
    }

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void PlayAnimation(int animationIndex)
	{
		if (connected)
		{
            StartCoroutine(SendMessage(new DieMessagePlayAnim() { index = (byte)animationIndex }));
		}
	}


	public void Ping()
	{
		if (connected)
		{
            StartCoroutine(SendMessage(new DieMessageRequestState()));
        }
    }

	public void ForceState(State forcedState)
	{
		Debug.Log("Forcing state of die " + name + " to " + forcedState);
		state = forcedState;
	}

	public void Connect(ISendBytes sb)
	{
		_sendBytes = sb;
		state = State.Unknown;
        if (OnStateChanged != null)
        {
            OnStateChanged(this, State.Unknown);
        }

		// Ping the die so we know its initial state
		Ping();
	}

    public void Disconnect()
    {
        state = State.Disconnected;
        if (OnStateChanged != null)
        {
            OnStateChanged(this, State.Disconnected);
        }
        _sendBytes = null;
    }


    public void DataReceived(byte[] data)
	{
		if (!connected)
		{
			Debug.LogError("Die " + name + " received data while disconnected!");
			return;
		}

        //StringBuilder builder = new StringBuilder();
        //builder.Append("Received ");
        //for (int i = 0; i < data.Length; ++i)
        //{
        //    builder.Append(data[i].ToString("X2"));
        //    builder.Append(" ");
        //}
        //Debug.Log(builder.ToString());

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

    IEnumerator SendMessage<T>(T message)
        where T : DieMessage
    {
        bool msgReceived = false;
        byte[] msgBytes = DieMessages.ToByteArray(message);
        _sendBytes.SendBytes(this, msgBytes, msgBytes.Length, () => msgReceived = true);
        yield return new WaitUntil(() => msgReceived);
    }

    IEnumerator WaitForMessage(DieMessageType msgType, System.Action<DieMessage> msgReceivedCallback)
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

    IEnumerator SendMessageWithAck<T>(T message, DieMessageType ackType)
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

    IEnumerator SendMessageWithAckOrTimeout<T>(T message, DieMessageType ackType, float timeOut)
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

    void OnStateMessage(DieMessage message)
    {
        // Handle the message
        var stateMsg = (DieMessageState)message;
        state = (State)stateMsg.face;

        // Notify anyone who cares
        if (OnStateChanged != null)
        {
            OnStateChanged.Invoke(this, state);
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

    public IEnumerator UploadBulkData(byte[] bytes)
    {
        short remainingSize = (short)bytes.Length;

        Debug.Log("Sending " + remainingSize + " bulk data");
        // Send setup message
        var setup = new DieMessageBulkSetup();
        setup.size = remainingSize;
        yield return StartCoroutine(SendMessageWithAck(setup, DieMessageType.BulkSetupAck));

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
            yield return StartCoroutine(SendMessageWithAck(data, DieMessageType.BulkDataAck));
            remainingSize -= data.size;
            offset += data.size;
        }

        Debug.Log("Finished sending bulk data");
    }

    public IEnumerator DownloadBulkData(System.Action<byte[]> onBufferReady) 
    {
        // Wait for setup message
        short size = 0;
        yield return StartCoroutine(WaitForMessage(DieMessageType.BulkSetup, (msg) =>
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
            StartCoroutine(SendMessage(msgAck));
        };
        AddMessageHandler(DieMessageType.BulkData, bulkReceived);

        // Send acknowledgement to the die, so it may transfer bulk data immediately
        StartCoroutine(SendMessage(new DieMessageBulkSetupAck()));

        // Wait for all the bulk data to be received
        yield return new WaitUntil(() => totalDataReceived == size);

        // We're done
        RemoveMessageHandler(DieMessageType.BulkData, bulkReceived);
        onBufferReady.Invoke(buffer);
    }

    public IEnumerator UploadAnimationSet(AnimationSet set)
    {
        // Prepare the die
        var prepareDie = new DieMessageTransferAnimSet();
        prepareDie.keyFrameCount = set.getKeyframeCount();
        prepareDie.trackCount = set.getTrackCount();
        prepareDie.animationCount = set.getAnimationCount();
        Debug.Log("Animation Data to be sent:");
        Debug.Log("keyframes: " + prepareDie.keyFrameCount + " * " + Marshal.SizeOf<Animations.RGBKeyframe>());
        Debug.Log("tracks: " + prepareDie.trackCount + " * " + Marshal.SizeOf<Animations.AnimationTrack>());
        Debug.Log("animations: " + prepareDie.animationCount + " * " + Marshal.SizeOf<Animations.Animation>());
        Debug.Log("palette: " + AnimationSet.PALETTE_SIZE);

        //StringBuilder builder = new StringBuilder();
        //builder.AppendLine("Sending animation set setup");
        //builder.Append("keyframes: ");
        //builder.Append(prepareDie.keyFrameCount);
        //builder.AppendLine();
        //builder.Append("tracks: ");
        //builder.Append(prepareDie.trackCount);
        //builder.AppendLine();
        //builder.Append("animations: ");
        //builder.Append(prepareDie.animationCount);
        //builder.AppendLine();
        //Debug.Log(builder.ToString());
        yield return StartCoroutine(SendMessageWithAck(prepareDie, DieMessageType.TransferAnimSetAck));

        Debug.Log("die is ready, sending animations");
        Debug.Log("byte array should be: " + set.ComputeAnimationDataSize());
        var setData = set.ToByteArray();
        yield return StartCoroutine(UploadBulkData(setData));

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

    public IEnumerator UploadSettings(DieSettings settings)
    {
        // Prepare the die
        var prepareDie = new DieMessageTransferSettings();
        yield return StartCoroutine(SendMessageWithAck(prepareDie, DieMessageType.TransferSettingsAck));

        // Die is ready, perform bulk transfer of the settings
        byte[] settingsBytes = DieSettings.ToByteArray(settings);
        yield return StartCoroutine(UploadBulkData(settingsBytes));

        // We're done!
    }

    public IEnumerator DownloadSettings(System.Action<DieSettings> settingsReadCallback)
    {
        // Request the settings from the die
        SendMessage(new DieMessageRequestSettings());

        // Now wait for the setup message back
        yield return StartCoroutine(WaitForMessage(DieMessageType.TransferSettings, null));

        // Got the message, acknowledge it
        StartCoroutine(SendMessage(new DieMessageTransferSettingsAck()));

        byte[] settingsBytes = null;
        yield return StartCoroutine(DownloadBulkData((buf) => settingsBytes = buf));
        var newSettings = DieSettings.FromByteArray(settingsBytes);

        // We've read the settings
        settingsReadCallback.Invoke(newSettings);
    }

    void RequestTelemetry(bool on)
    {
        PostMessage(new DieMessageRequestTelemetry() { telemetry = on ? (byte)1 : (byte)0 });
    }

    public Coroutine SetNewColor(System.Action<Color> displayColor)
    {
        return StartCoroutine(SetNewColorCr(displayColor));
    }

    IEnumerator SetNewColorCr(System.Action<Color> displayColor)
    {
        float hue = Random.Range(0.0f, 1.0f);
        Color newDisplayColor = Color.HSVToRGB(hue, 1.0f, 1.0f);
        Color newColor = Color.HSVToRGB(hue, 1.0f, 0.5f);

        Color32 color32 = newColor;
        int colorRGB = color32.r << 16 | color32.g << 8 | color32.b;

        yield return StartCoroutine(SendMessageWithAckOrTimeout(new DieMessageProgramDefaultAnimSet() { color = (uint)colorRGB }, DieMessageType.ProgramDefaultAnimSetFinished, 5.0f));

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
        return StartCoroutine(SendMessageWithAckOrTimeout(new DieMessageFlash() { animIndex = (byte)index }, DieMessageType.FlashFinished, 5.0f));
    }

    public Coroutine Rename(string newName)
    {
        return StartCoroutine(RenameCr(newName));
    }

    IEnumerator RenameCr(string newName)
    {
        gameObject.name = newName;
        name = newName;
        yield return StartCoroutine(SendMessageWithAckOrTimeout(new DieMessageRename() { newName = newName }, DieMessageType.RenameFinished, 5.0f));
        if (OnSettingsChanged != null)
        {
            OnSettingsChanged(this);
        }
    }

    public Coroutine GetDefaultAnimSetColor(System.Action<Color> retColor)
    {
        return StartCoroutine(GetDefaultAnimSetColorCr(retColor));
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

        yield return StartCoroutine(SendMessage(new DieMessageRequestDefaultAnimSetColor()));

        // We're done
        RemoveMessageHandler(DieMessageType.DefaultAnimSetColor, defaultAnimSetColorHandler);
    }

    public void RequestBulkData()
    {
        PostMessage(new DieMessageTestBulkSend());
    }

    public void PrepareBulkData()
    {
        PostMessage(new DieMessageTestBulkReceive());
    }

    public void SetLEDsToRandomColor()
    {
        var msg = new DieMessageSetAllLEDsToColor();
        uint r = (byte)Random.Range(0, 256);
        uint g = (byte)Random.Range(0, 256);
        uint b = (byte)Random.Range(0, 256);
        msg.color = (r << 16) + (g << 8) + b;
        PostMessage(msg);
    }

    public void SetLEDsToColor(Color color)
    {
        var msg = new DieMessageSetAllLEDsToColor();
        Color32 color32 = color;
        msg.color = (uint)((color32.r << 16) + (color32.g << 8) + color32.b);
        PostMessage(msg);
    }
}
