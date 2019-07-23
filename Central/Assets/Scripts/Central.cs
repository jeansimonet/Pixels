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
    public OnDieConnectionEvent onDieLostConnection;
    public OnDieConnectionEvent onDieForgotten;
    public OnDieConnectionEvent onDieReady;

    public delegate void OnBluetoothErrorEvent(string message);
    public OnBluetoothErrorEvent onBluetoothError;

    Dictionary<string, Die> _dicePool;
    Dictionary<string, Die> _lostDice;

    public State state
    {
        get { return _state; }
    }

    bool userScan = false;
    float connectingStartTime;
    Coroutine reconnectCoroutine;
    const float ReconnectDelay = 15.0f;
    List<KeyValuePair<string, string>> discoveredDice; // This is cleared at the begining of a scan

    void Awake()
    {
        _dicePool = new Dictionary<string, Die>();
        _lostDice = new Dictionary<string, Die>();
        discoveredDice = new List<KeyValuePair<string, string>>();
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
        _state = State.Idle;
        #endif
	}

    void BeginScanInternal()
    {
        _state = State.Scanning;
        var services = new string[] { serviceGUID };

        discoveredDice.Clear();

        System.Action<string, string> dieDiscovered =
            (address, name) =>
            {
                // Do we already know about this die?
                Die die = null;
                if (_lostDice.TryGetValue(address, out die))
                {
                    // Yes, we should reconnect automatically
                    ReconnectDie(address, die);
                }
                else
                {
                    if (userScan)
                    {
                        // Already created an object for this?
                        if (!_dicePool.TryGetValue(address, out die))
                        {
                            // Yes, notify!
                            NewDie(address, name);
                        }
                        else
                        {
                            DiscoverDie(die);
                        }
                    }
                    else
                    {
                        Debug.Log("Remembering " + name + " for user scan");
                        discoveredDice.Add(new KeyValuePair<string, string>(address, name));
                    }
                }
            };

        //BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(services, dieDiscovered, null, false, true);
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, dieDiscovered, null, false, true);

        // Also notify virtual dice that we're trying to connect
        if (virtualBluetooth != null)
        {
            virtualBluetooth.ScanForPeripheralsWithServices(services, dieDiscovered, null, false, false);
        }
    }

    void StopScanInternal()
    {
        BluetoothLEHardwareInterface.StopScan();
        if (virtualBluetooth != null)
        {
            virtualBluetooth.StopScan();
        }
        _state = State.Idle;
        discoveredDice.Clear();
    }

    void NewDie(string address, string name)
    {
        Debug.Log("New die " + name);
        var die = CreateDie(name, address);
        _dicePool.Add(address, die);
        DiscoverDie(die);
    }

    void DiscoverDie(Die die)
    {
        Debug.Log(name + " discovered");
        die.OnAdvertising();
        onDieDiscovered?.Invoke(die);
    }

    void ReconnectDie(string address, Die die)
    {
        Debug.Log("Reconnecting " + die.name);
        _lostDice.Remove(address);
        _dicePool.Add(address, die);
        die.Connect();
    }

    public void BeginScanForDice()
	{
        if (_state == State.Idle || (_state == State.Scanning && !userScan))
        {
            userScan = true;
            if (_state == State.Idle)
            {
                BeginScanInternal();
            }
            else
            {
                // Process the internal list first
                Debug.Log("User scan while reconnect scan in progress");
                foreach (var kv in discoveredDice)
                {
                    NewDie(kv.Key, kv.Value);
                }
                // Then let the discover action do its thing
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
            if (reconnectCoroutine == null)
            {
                StopScanInternal();
            }
            userScan = false;
        }
        else
        {
            Debug.LogError("Central is not currently scanning for devices, current state is " + _state);
        }
    }

    IEnumerator ReconnectCr(System.Action reconnectFinished)
    {
        // Start a scan
        if (_state != State.Scanning)
        {
            Debug.Log("Triggering scan to reconnect");
            userScan = false;
            BeginScanInternal();
        }

        float startTime = Time.time;
        while (_lostDice.Count > 0 && Time.time < startTime + ReconnectDelay)
        {
            yield return null;
        }

        if (!userScan)
        {
            StopScanInternal();
        }

        if (_lostDice.Count > 0)
        {
            List<Die> lostCopy = new List<Die>(_lostDice.Values);
            foreach (var d in lostCopy)
            {
                ForgetDie(d);
            }
            _lostDice.Clear();
        }

        reconnectFinished?.Invoke();
    }

    public void ConnectToDie(Die die)
    {
        System.Action<string> onConnected = (ignore) =>
            {
                Debug.Log(die.name + " connected");
                die.OnConnected();
                onDieConnected?.Invoke(die);
            };

        System.Action<string> onDisconnected = (ignore) =>
            {
                Debug.Log(die.name + " lost connection");
                die.OnLostConnection();
                onDieLostConnection?.Invoke(die);
            };

        System.Action<string, string> onService = (ignore, service) =>
            {
                die.OnServiceDiscovered(service);
            };

        System.Action<string, string, string> onCharacteristic = (ignore, service, charact) =>
            {
                die.OnCharacterisicDiscovered(service, charact);
            };

        if (virtualBluetooth == null || !virtualBluetooth.IsVirtualDie(die.address))
            BluetoothLEHardwareInterface.ConnectToPeripheral(die.address, onConnected, onService, onCharacteristic, onDisconnected);
        else
            virtualBluetooth.ConnectToPeripheral(die.address, onConnected, onService, onCharacteristic, onDisconnected);
    }

    public void DisconnectDie(Die die, System.Action disconnectionComplete = null)
    {
        Debug.Log("Disconnecting " + die.name);
        System.Action<string> onDisconnected = (ignore) =>
            {
                Debug.Log(die.name + " disconnected");
                die.OnDisconnected();
                onDieDisconnected?.Invoke(die);
                disconnectionComplete?.Invoke();
            };
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
        Debug.Log("Die " + die.name + " ready");
        onDieReady?.Invoke(die);
    }

    public void TryReconnectDie(Die die)
    {
        Debug.Log("Die " + die.name + " unresponsive, attempting to reconnect");
        _dicePool.Remove(die.address);
        _lostDice.Add(die.address, die);
        reconnectCoroutine = StartCoroutine(ReconnectCr(() => reconnectCoroutine = null));
    }

    public void ForgetDie(Die die)
    {
        Debug.Log("Forgetting die " + die.name + ", current state " + die.connectionState.ToString());
        if (die.connectionState >= Die.ConnectionState.Connected)
        {
            die.Disconnect();
        }
        _dicePool.Remove(die.address);
        _lostDice.Remove(die.address);
        onDieForgotten?.Invoke(die);
        DestroyDie(die);
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
        GameObject.Destroy(die.gameObject, 3.0f);
    }

    public IEnumerable<Die> dicePool
    {
        get { return _dicePool.Values; }
    }

    void OnApplicationQuit()
    {
        //DisconnectAllDice();
        BluetoothLEHardwareInterface.DeInitialize(null);
    }
}
