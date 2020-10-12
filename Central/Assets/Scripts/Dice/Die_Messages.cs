using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dice
{
public partial class Die
	: MonoBehaviour
{
    #region Message Infrastructure
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

    IEnumerator SendMessageWithAckOrTimeoutCr<T>(T message, DieMessageType ackType, float timeOut, System.Action<DieMessage> ackAction, System.Action timeoutAction, System.Action errorAction)
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

    IEnumerator SendMessageWithAckRetryCr<T>(T message, DieMessageType ackType, int retryCount, System.Action<DieMessage> ackAction, System.Action timeoutAction, System.Action errorAction)
        where T : DieMessage
    {
        bool msgReceived = false;
        System.Action<DieMessage> msgAction = (msg) =>
        {
            ackAction?.Invoke(msg);
        };
        int count = 0;
        while (!msgReceived && count < retryCount)
        {
            // Retry every half second if necessary
            yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(message, ackType, 0.5f, msgAction, timeoutAction, errorAction));
            count++;
        }
    }

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
        if (connectionState >= ConnectionState.Identifying)
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

    #endregion

    public Coroutine PlayAnimation(int animationIndex)
    {
        return PerformBluetoothOperation(() => PostMessage(new DieMessagePlayAnim() { index = (byte)animationIndex }));
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

    public Coroutine GetDieInfo(System.Action<bool> callback)
    {
        return PerformBluetoothOperation(GetDieInfoCr(callback));
    }

    IEnumerator GetDieInfoCr(System.Action<bool> callback)
    {
        void updateDieInfo(DieMessage msg)
        {
            var idMsg = (DieMessageIAmADie)msg;
	        bool appearanceChanged = faceCount != idMsg.faceCount || designAndColor != idMsg.designAndColor;
            faceCount = idMsg.faceCount;
	        designAndColor = idMsg.designAndColor;
	        deviceId = idMsg.deviceId;
            dataSetHash = idMsg.dataSetHash;
            flashSize = idMsg.flashSize;
            firmwareVersionId = System.Text.Encoding.UTF8.GetString(idMsg.versionInfo, 0, DieMessages.VERSION_INFO_SIZE);
            Debug.Log("Die " + name + " has " + flashSize + " bytes available for data");
            if (appearanceChanged)
            {
                OnAppearanceChanged?.Invoke(this, faceCount, designAndColor);
            }
            callback?.Invoke(true);
        }

        var whoAreYouMsg = new DieMessageWhoAreYou();
        yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(whoAreYouMsg, DieMessageType.IAmADie, 5, updateDieInfo, () => callback?.Invoke(false), () => callback?.Invoke(false)));
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

    public Coroutine GetBatteryLevel(System.Action<Die, float?> outLevelAction)
    {
        return PerformBluetoothOperation(GetBatteryLevelCr(outLevelAction));
    }

    IEnumerator GetBatteryLevelCr(System.Action<Die, float?> outLevelAction)
    {
        yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(
            new DieMessageRequestBatteryLevel(),
            DieMessageType.BatteryLevel,
            5.0f,
            (msg) =>
            {
                var lvlMsg = (DieMessageBatteryLevel)msg;
                batteryLevel = lvlMsg.level;
                OnBatteryLevelChanged?.Invoke(this, lvlMsg.level);
                outLevelAction?.Invoke(this, lvlMsg.level);
            },
            () =>
            {
                outLevelAction?.Invoke(this, null);
            },
            () =>
            {
                outLevelAction?.Invoke(this, null);
            }));
    }

    public Coroutine GetRssi(System.Action<Die, int?> outRssiAction)
    {
        return PerformBluetoothOperation(GetRssiCr(outRssiAction));
    }

    IEnumerator GetRssiCr(System.Action<Die, int?> outRssiAction)
    {
        yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(
            new DieMessageRequestRssi(),
            DieMessageType.Rssi,
            5.0f,
            (msg) =>
            {
                var rssiMsg = (DieMessageRssi)msg;
                rssi = rssiMsg.rssi;
                OnRssiChanged?.Invoke(this, rssiMsg.rssi);
                outRssiAction?.Invoke(this, rssiMsg.rssi);
            },
            () =>
            {
                outRssiAction?.Invoke(this, null);
            },
            () =>
            {
                outRssiAction?.Invoke(this, null);
            }));
    }

    public Coroutine SetCurrentDesignAndColor(DesignAndColor design, System.Action<bool> callback)
    {
       return StartCoroutine(SendMessageWithAckOrTimeoutCr(
           new DieMessageSetDesignAndColor() { designAndColor = design },
           DieMessageType.SetDesignAndColorAck,
           3,
           (ignore) =>
           {
               designAndColor = design;
               OnAppearanceChanged?.Invoke(this, faceCount, designAndColor);
               callback?.Invoke(true);
           },
           () => callback?.Invoke(false),
           () => callback?.Invoke(false)));
    }

    public Coroutine RenameDie(string newName, System.Action<bool> callback)
    {
        return StartCoroutine(SendMessageWithAckOrTimeoutCr(
            new DieMessageSetName() { name = System.Text.Encoding.UTF8.GetBytes(newName) },
            DieMessageType.SetNameAck,
            3,
            (ignore) => callback?.Invoke(true),
            () => callback?.Invoke(false),
            () => callback?.Invoke(false)));
    }

    public Coroutine Flash(Color color, int count, System.Action<bool> callback)
    {
        Color32 color32 = color;
        var msg = new DieMessageFlash();
        msg.color = (uint)((color32.r << 16) + (color32.g << 8) + color32.b);
        msg.flashCount = (byte)count;
        return StartCoroutine(SendMessageWithAckOrTimeoutCr(
            msg,
            DieMessageType.FlashFinished,
            3,
            (ignore) => callback?.Invoke(true),
            () => callback?.Invoke(false),
            () => callback?.Invoke(false)));
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

    public void SetStandardMode()
    {
        PostMessage(new DieMessageSetStandardState());
    }

    public void SetLEDAnimatorMode()
    {
        PostMessage(new DieMessageSetLEDAnimState());
    }

    public void SetBattleMode()
    {
        PostMessage(new DieMessageSetBattleState());
    }

    public void DebugAnimController()
    {
        PostMessage(new DieMessageDebugAnimController());
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


    #region MessageHandlers
    void OnStateMessage(DieMessage message)
    {
        // Handle the message
        var stateMsg = (DieMessageState)message;
        Debug.Log("State: " + ((RollState)(stateMsg.state)).ToString() + ", " + stateMsg.face);

        var newState = (RollState)stateMsg.state;
        var newFace = stateMsg.face;
        if (newState != state || newFace != face)
        {
            state = newState;
            face = newFace;

            // Notify anyone who cares
            OnStateChanged?.Invoke(this, state, face);
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
        PixelsApp.Instance.ShowDialogBox("Message from " + name, text, "Ok", cancel ? "Cancel" : null, (res) =>
        {
            PostMessage(new DieMessageNotifyUserAck() { okCancel = (byte)(res ? 1 : 0) });
        });
    }

    void OnPlayAudioClip(DieMessage message)
    {
        var playClipMessage = (DieMessagePlaySound)message;
        AudioClipManager.Instance.PlayAudioClip((uint)playClipMessage.clipId);
    }
    #endregion
}
}
