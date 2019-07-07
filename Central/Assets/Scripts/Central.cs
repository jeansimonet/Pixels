using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;


public class Central
	: MonoBehaviour
{
    public string serviceGUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";
	public string subscribeCharacteristic = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";
	public string writeCharacteristic = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E";

    public enum State
    {
        Uninitialized = 0,
        Initializing,
        Idle,
        Scanning,
        Error,
    }

    State _state = State.Uninitialized;

    VirtualBluetoothInterface virtualBluetooth;

    public delegate void OnDieConnectionEvent(Die die);
    public OnDieConnectionEvent onDieDiscovered;
    public OnDieConnectionEvent onDieConnected;
    public OnDieConnectionEvent onDieDisconnected;
    public OnDieConnectionEvent onDieForgotten;
    public OnDieConnectionEvent onDieReady;
    public delegate void OnBluetoothErrorEvent(string message);
    public OnBluetoothErrorEvent onBluetoothError;

    List<Die> dice;

    public State state
    {
        get { return _state; }
    }

    float connectingStartTime;

    void Awake()
    {
        dice = new List<Die>();
        virtualBluetooth = GetComponent<VirtualBluetoothInterface>();
    }

    void Start()
	{
		_state = State.Initializing;
        #if !UNITY_EDITOR_OSX
		BluetoothLEHardwareInterface.Initialize(true, false,
		() =>
		{
			_state = State.Idle;
		},
		(err) =>
		{
            onBluetoothError?.Invoke(err);
        });
        #else
        _state = CentralState.Idle;
        #endif
	}

	public void BeginScanForDice()
	{
        if (_state == State.Idle)
        {
            System.Action<string, string> dieDiscovered =
                (address, name) =>
                {
                    // Do we already know about this die?
                    Die die = null;
                    if (!dice.Any(dc => dc.address == address))
                    {
                        // We do not, create a new die
                        die = CreateDie(name, address);
                        dice.Add(die);
                    }
                    else
                    {
                        die = dice.Find(dc => dc.address == address);
                    }
                    die.OnAdvertising();

                    if (onDieDiscovered != null)
                        onDieDiscovered(die);
                };

            _state = State.Scanning;
            var services = new string[] { serviceGUID };
            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(services, dieDiscovered, null, false, false);

            // Also notify virtual dice that we're trying to connect
            if (virtualBluetooth != null)
            {
                virtualBluetooth.ScanForPeripheralsWithServices(services, dieDiscovered, null, false, false);
            }
        }
        else
        {
            Debug.LogError("Central is not ready to scan, current state is " + _state);
        }
	}

    public void StopScanForDice()
    {
        if (_state == State.Scanning)
        {
            BluetoothLEHardwareInterface.StopScan();
            if (virtualBluetooth != null)
            {
                virtualBluetooth.StopScan();
            }
            _state = State.Idle;
        }
        else
        {
            Debug.LogError("Central is not currently scanning for devices, current state is " + _state);
        }
    }

    public void ConnectToDie(Die die)
    {
        System.Action<string> onConnected = (ignore) => die.OnConnected();
        System.Action<string> onDisconnected = (ignore) => die.OnLostConnection();
        System.Action<string, string> onService = (ignore, service) => die.OnServiceDiscovered(service);
        System.Action<string, string, string> onCharacteristic = (ignore, service, charact) => die.OnCharacterisicDiscovered(service, charact);

        if (virtualBluetooth == null || !virtualBluetooth.IsVirtualDie(die.address))
            BluetoothLEHardwareInterface.ConnectToPeripheral(die.address, onConnected, onService, onCharacteristic, onDisconnected);
        else
            virtualBluetooth.ConnectToPeripheral(die.address, onConnected, onService, onCharacteristic, onDisconnected);
    }

    public void DisconnectDie(Die die)
    {
        System.Action<string> onDisconnected = (ignore) => die.OnLostConnection();
        if (virtualBluetooth == null || !virtualBluetooth.IsVirtualDie(die.address))
            BluetoothLEHardwareInterface.DisconnectPeripheral(die.address, onDisconnected);
        else
            virtualBluetooth.DisconnectPeripheral(die.address, onDisconnected);
    }

    public void SubscribeCharacteristic(Die die, string serviceGUID, string subscribeCharacteristic, System.Action subscribedCallback)
    {
        System.Action<string> onSubscribed = (ignore) => subscribedCallback();
        System.Action<string, byte[]> onDataReceived = (ignore, data) => die.OnDataReceived(serviceGUID, subscribeCharacteristic, data);
        if (virtualBluetooth == null || !virtualBluetooth.IsVirtualDie(die.address))
            BluetoothLEHardwareInterface.SubscribeCharacteristic(die.address, serviceGUID, subscribeCharacteristic, onSubscribed, onDataReceived);
        else
            virtualBluetooth.SubscribeCharacteristic(die.address, serviceGUID, subscribeCharacteristic, onSubscribed, onDataReceived);
    }

    public void WriteCharacteristic(Die die, string serviceGUID, string writeCharacteristic, byte[] bytes, int length, System.Action bytesWrittenCallback)
    {
        System.Action<string> onWritten = (ignore) => bytesWrittenCallback?.Invoke();
        if (virtualBluetooth == null || !virtualBluetooth.IsVirtualDie(die.address))
            BluetoothLEHardwareInterface.WriteCharacteristic(die.address, serviceGUID, writeCharacteristic, bytes, length, false, onWritten);
        else
            virtualBluetooth.WriteCharacteristic(die.address, serviceGUID, writeCharacteristic, bytes, length, false, onWritten);
    }

    public void DieReady(Die die)
    {
        onDieReady?.Invoke(die);
    }

    public void UnresponsiveDie(Die die)
    {
        ForgetDie(die);
    }

    public void ForgetDie(Die die)
    {
        dice.Remove(die);
        DisconnectDie(die);
        Destroy(die);
    }

    Die CreateDie(string name, string address)
    {
        // Create a die game object
        var dieGO = new GameObject(name);
        dieGO.transform.SetParent(transform, false);

        // Add the Die component and return it!
        var ret = dieGO.AddComponent<Die>();
        ret.Setup(name, address, Die.ConnectionState.Advertising, this);
        return ret;
    }

    void DestroyDie(Die die)
    {
        GameObject.Destroy(die.gameObject);
    }

    public IEnumerable<Die> diceList
    {
        get { return dice; }
    }

    void OnApplicationQuit()
    {
        //DisconnectAllDice();
        BluetoothLEHardwareInterface.DeInitialize(null);
    }
}
