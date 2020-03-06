using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DicePool : MonoBehaviour
{
    public delegate void BluetoothErrorEvent(string errorString);
    public BluetoothErrorEvent onBluetoothError;

    public delegate void DieEvent(Die die);
    public DieEvent onDieDiscovered;
    public DieEvent onDieConnected;
    public DieEvent onDieDisconnected;

    public static DicePool Instance => _instance;

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

    Dictionary<string, Die> _dice;
    static DicePool _instance = null;

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
        Central.Instance.RegisterFactory(CreateDie);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    void OnBluetoothError(string message)
    {
        onBluetoothError?.Invoke(message);
    }

    void OnDieDiscovered(Central.IDie die)
    {
        onDieDiscovered?.Invoke((Die)die);
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
