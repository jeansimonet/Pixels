using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class DicePool : MonoBehaviour
{
    /// <summary>
    /// This data structure mirrors the data in firmware/bluetooth/bluetooth_stack.cpp
    /// </sumary>
    public struct CustomAdvertisingData
    {
        // Die type identification
        DiceVariants.DesignAndColor designAndColor; // Physical look, also only 8 bits
        byte faceCount; // Which kind of dice this is

        // Current state
        Die.RollState rollState; // Indicates whether the dice is being shaken
        byte currentFace; // Which face is currently up
    };


    public delegate void BluetoothErrorEvent(string errorString);
    public delegate void DieEvent(Die die);
    public delegate void DieAdvertisingDataEvent(Die die, CustomAdvertisingData newData);

    public DieEvent onDieDiscovered;
    public DieEvent onDieConnected;
    public DieEvent onDieDisconnected;
    public BluetoothErrorEvent onBluetoothError;
    public DieAdvertisingDataEvent onDieAdvertisingData;

    public static DicePool Instance => _instance;
    static DicePool _instance = null; // Initialized in Awake

    Dictionary<string, Die> _dice;

    public void BeginScanForDice()
    {
        Central.Instance.BeginScanForDice();
    }

    public void StopScanForDice()
    {
        Central.Instance.StopScanForDice();
    }

    public void ConnectDie(Die die)
    {
        Central.Instance.ConnectDie(die);
    }

    public void DisconnectDie(Die die)
    {
        Central.Instance.DisconnectDie(die);
    }

    public void WriteDie(Die die, byte[] bytes, int length, System.Action bytesWrittenCallback)
    {
        Central.Instance.WriteDie(die, bytes, length, bytesWrittenCallback);
    }

    void Awake()
    {
        _dice = new Dictionary<string, Die>();

        if (_instance != null)
        {
            Debug.LogError("Multiple Dice Pools in scene");
        }
        else
        {
            _instance = this;
        }

        Central.Instance.onBluetoothError += OnBluetoothError;
        Central.Instance.onDieDiscovered += OnDieDiscovered;
        Central.Instance.onDieConnected += OnDieReady;
        Central.Instance.onDieDisconnected += OnDieDisconnected;
        Central.Instance.onDieAdvertisingData += OnDieAdvertisingData;
        Central.Instance.RegisterFactory(CreateDie);
    }

    void OnBluetoothError(string message)
    {
        onBluetoothError?.Invoke(message);
    }

    void OnDieDiscovered(Central.IDie die)
    {
        onDieDiscovered?.Invoke((Die)die);
    }

    void OnDieAdvertisingData(Central.IDie die, byte[] data)
    {
        // Marshall the data into the struct we expect
        int size = Marshal.SizeOf(typeof(CustomAdvertisingData));
        if (data.Length == size)
        {
            System.IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, ptr, size);
            var customData = (CustomAdvertisingData)Marshal.PtrToStructure(ptr, typeof(CustomAdvertisingData));
            Marshal.FreeHGlobal(ptr);
            onDieAdvertisingData?.Invoke((Die)die, customData);
        }
        else
        {
            Debug.LogError("Incorrect advertising data length " + data.Length + ", expected: " + size);
        }
    }

    void OnDieReady(Central.IDie die)
    {
        onDieConnected?.Invoke((Die)die);
    }

    void OnDieDisconnected(Central.IDie die)
    {
        onDieDisconnected?.Invoke((Die)die);
    }

    Central.IDie CreateDie(string address, string name)
    {
        if (!_dice.TryGetValue(address, out Die die))
        {
            // We don't already know about this die, create it!
            var dieObj = new GameObject(name);
            dieObj.transform.SetParent(transform);
            die = dieObj.AddComponent<Die>();
            die.Setup(name, address);
            _dice.Add(address, die);
        }

        return die;
    }

}
