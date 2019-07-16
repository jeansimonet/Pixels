using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;

public interface ICoroutineManager
{
    Coroutine StartCoroutine(IEnumerator en);
    void StopCoroutine(Coroutine cor);
}

[System.Serializable]
public class VirtualDie
{
    public string name;
    public string address;
    public bool advertising = true;

    ICoroutineManager coroutineManager;
    Coroutine telemetryCoroutine;

    // When subscribed to, the die will use this to send data back!
    System.Action<byte[]> onData;
    System.Action onDisconnect;
    byte[] lastDataSent = new byte[1] { 0 };

    // Internal delegate per message type
    delegate void MessageReceivedDelegate(DieMessage msg);
    Dictionary<DieMessageType, MessageReceivedDelegate> messageDelegates;

    // Internal (fake) die state
    public Die.DieType dieType = Die.DieType.SixSided;
    public Die.State currentState = Die.State.Idle;
    public int currentFace = 0;

    public VirtualDie(string name, string address, Die.DieType type, ICoroutineManager coroutineManager)
    {
        this.name = name;
        this.address = address;
        this.dieType = type;
        this.coroutineManager = coroutineManager;

        messageDelegates = new Dictionary<DieMessageType, MessageReceivedDelegate>();

        // Setup delegates for face and telemetry
        messageDelegates.Add(DieMessageType.RequestState, OnRequestStateMessage);
        messageDelegates.Add(DieMessageType.WhoAreYou, OnWhoAreYouMessage);
    }

    public void Connect(System.Action onDisconnect)
    {
        this.onDisconnect = onDisconnect;
        this.advertising = false;
    }

    public void Subscribe(System.Action<byte[]> onData)
    {
        this.onData = onData;
        //telemetryCoroutine = coroutineManager.StartCoroutine(SendTelemetryData());
    }

    public byte[] Read()
    {
        return lastDataSent;
    }

    public void Write(byte[] data, int length)
    {
        OnMessageReceived(data);
    }

    public void GoAway()
    {
        if (telemetryCoroutine != null)
        {
            coroutineManager.StopCoroutine(telemetryCoroutine);
        }
        onData = null;
        if (onDisconnect != null)
        {
            onDisconnect();
            onDisconnect = null;
            advertising = true;
        }
    }

    int RandomFace()
    {
        if (dieType == Die.DieType.SixSided)
            return Random.Range(0, 6);
        else
            return Random.Range(0, 20);
    }

    public void Roll()
    {
        coroutineManager.StartCoroutine(RollCr());
    }

    IEnumerator RollCr()
    {
        yield return new WaitForSeconds(Random.Range(0.01f, 3.0f));
        SetCurrentState(Die.State.Handling);
        yield return new WaitForSeconds(Random.Range(0.01f, 1.0f));
        SetCurrentState(Die.State.Falling);
        if (Random.Range(0.0f, 1.0f) < 0.5f)
        {
            yield return new WaitForSeconds(Random.Range(0.01f, 0.5f));
            SetCurrentState(Die.State.Jerking);
        }
        yield return new WaitForSeconds(Random.Range(0.01f, 1.0f));
        SetCurrentState(Die.State.Rolling);
        yield return new WaitForSeconds(Random.Range(0.01f, 1.0f));
        SetCurrentState(Die.State.Idle);

        SetCurrentFace(RandomFace());
    }

    void SetCurrentState(Die.State newState)
    {
        currentState = newState;
        SendMessage(new DieMessageState()
        {
            state = (byte)currentState,
            face = (byte)currentFace,
        });
    }

    void SetCurrentFace(int newFace)
    {
        currentFace = newFace;
        SendMessage(new DieMessageState()
        {
            state = (byte)currentState,
            face = (byte)currentFace,
        });
    }

    void OnMessageReceived(byte[] data)
    {
        if (data.Length >= 1)
        {
            var message = DieMessages.FromByteArray(data);
            MessageReceivedDelegate del;
            if (messageDelegates.TryGetValue(message.type, out del))
            {
                del.Invoke(message);
            }
        }
    }

    void SendMessage<T>(T message)
        where T : DieMessage
    {
        lastDataSent = DieMessages.ToByteArray(message);
        onData?.Invoke(lastDataSent);
    }


    #region Message Handlers
    void OnRequestStateMessage(DieMessage msg)
    {
        coroutineManager.StartCoroutine(SendStateCr());
    }

    IEnumerator SendStateCr()
    {
        // Wait a few frames!
        yield return null;
        yield return null;
        yield return null;

        // Send the identity message
        SendMessage(new DieMessageState() { state = (byte)currentState });
    }

    void OnWhoAreYouMessage(DieMessage msg)
    {
        coroutineManager.StartCoroutine(OnWhoAreMessageCr());
    }

    IEnumerator OnWhoAreMessageCr()
    {
        // Wait a few frames!
        yield return null;
        yield return null;

        SendMessage(new DieMessageIAmADie() { id = (byte)dieType });
    }

    IEnumerator SendTelemetryDataCr()
    {
        while (true)
        {
            float dt = 0.05f;
            yield return new WaitForSecondsRealtime(dt * 2);
            var acc = new DieMessageAcc();
            acc.type = DieMessageType.Telemetry;
            acc.data = new AccelFrame[2];

            // Vector3 accVec = Vector3.down;
            // int x = Mathf.RoundToInt(accVec.x / 8.0f * (float)(1 << 11));
            // int y = Mathf.RoundToInt(accVec.y / 8.0f * (float)(1 << 11));
            // int z = Mathf.RoundToInt(accVec.z / 8.0f * (float)(1 << 11));

            acc.data[0].DeltaTime = (short)(dt * 1000);
            //acc.data[0].X = (short)x;
            //acc.data[0].Y = (short)y;
            //acc.data[0].Z = (short)z;
            acc.data[0].X = (short)(Random.Range(0, 1 << 12) - (1 << 11));
            acc.data[0].Y = (short)(Random.Range(0, 1 << 12) - (1 << 11));
            acc.data[0].Z = (short)(Random.Range(0, 1 << 12) - (1 << 11));
            acc.data[1].DeltaTime = (short)(dt * 1000); 
            //acc.data[1].X = (short)x;
            //acc.data[1].Y = (short)y;
            //acc.data[1].Z = (short)z;
            acc.data[1].X = (short)(Random.Range(0, 1 << 12) - (1 << 11));
            acc.data[1].Y = (short)(Random.Range(0, 1 << 12) - (1 << 11));
            acc.data[1].Z = (short)(Random.Range(0, 1 << 12) - (1 << 11));
            SendMessage(acc);
        }
    }

    #endregion

}
