using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    ICoroutineManager coroutineManager;
    Coroutine telemetryCoroutine;

    // When subscribed to, the die will use this to send data back!
    System.Action<byte[]> onData;
    System.Action onDisconnect;

    byte[] lastDataSent;

    public VirtualDie(string name, string address, ICoroutineManager coroutineManager)
    {
        this.name = name;
        this.address = address;
        this.coroutineManager = coroutineManager;
    }

    public void Connect(System.Action onDisconnect)
    {
        this.onDisconnect = onDisconnect;
    }

    public void Subscribe(System.Action<byte[]> onData)
    {
        this.onData = onData;
        telemetryCoroutine = coroutineManager.StartCoroutine(SendTelemetryData());
    }

    public byte[] Read()
    {
        return lastDataSent;
    }

    public void Write(byte[] data, int length)
    {
        // Nothing for now!
    }

    public void Die()
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
        }
    }


    IEnumerator SendTelemetryData()
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
            onData(DieMessages.ToByteArray(acc));
        }
    }
}
