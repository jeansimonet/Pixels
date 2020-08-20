using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public class Central : SingletonMonoBehaviour<Central>
{
    /// <summary>
    /// What Central cares about when it comes to dice
    /// </summary>
    public interface IDie
    {
        string name { get; }
        string address { get; }
    }

    /// <summary>
    /// Internal Die definition, stores connection-relevant data
    /// </summary>
    class Die
        : IDie
    {
        public enum State
        {
            Advertising = 0,
            Connecting,
            Connected,
            Subscribing,
            Ready,
            Writing,
            Disconnecting,
        }

        public State state;
        public string name;
        public string address;

        public float startTime; // Used for timing out while looking for characteristics or subscribing to one
        public bool deviceConnected;
        public bool messageWriteCharacteristicFound;
        public bool messageReadCharacteristicFound;

        // These are set and cleared depending on what is going on
        public System.Action<IDie, int, byte[]> onCustomAdvertisingData;
        public System.Action<IDie, bool, string> onConnectionResult;
        public System.Action<IDie, bool, string> onWriteResult;
        public System.Action<IDie, byte[]> onData;
        public System.Action<IDie, bool, string> onDisconnectionResult;
        public System.Action<IDie, string> onUnexpectedDisconnection;

        public Die(string address, string name)
        {
            this.name = name;
            this.address = address;
            this.state = State.Advertising;
            this.startTime = float.MaxValue;
            this.deviceConnected = false;
            this.messageWriteCharacteristicFound = false;
            this.messageReadCharacteristicFound = false;
        }

        string IDie.name => name;
        string IDie.address => address;
    }

    const string serviceGUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";
    const string subscribeCharacteristic = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";
    const string writeCharacteristic = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E";

    const float DiscoverCharacteristicsTimeout = 5.0f; // seconds
    const float SubscribeCharacteristicsTimeout = 5.0f; // seconds

    public enum State
    {
        Uninitialized = 0,
        Initializing,
        Idle,
        Scanning,
        Error,
    }

    State _state = State.Uninitialized;
    public State state => _state;

    Dictionary<string, Die> _dice;

    public delegate void BluetoothErrorEvent(string errorString);
    public BluetoothErrorEvent onBluetoothError;

    /// <summary>
    /// Initiates a bluetooth scan
    /// </summary>
    public bool BeginScanForDice(System.Action<IDie> onDieDiscovered, System.Action<IDie, int, byte[]> onCustomAdvertisingData)
    {
        if (_state != State.Idle)
        {
            Debug.LogError("Die Manager not ready to start scanning, state: " + _state);
            return false;
        }

        Debug.Log("start scan");

        // Begin scanning
        _state = State.Scanning;

        // Notify of all the already known advertising dice
        foreach (var die in _dice.Values)
        {
            if (die.state == Die.State.Advertising)
            {
                die.onCustomAdvertisingData = onCustomAdvertisingData;
                onDieDiscovered?.Invoke(die);
            }
        }

        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
            new string[] { serviceGUID },
            (a, n) => OnDeviceDiscovered(a, n, onDieDiscovered, onCustomAdvertisingData),
            OnDeviceAdvertisingInfo, false, true);

        return true;
    }

    /// <summary>
    /// Stops scanning for new bluetooth devices
    /// </summary>
    public bool StopScanForDice()
    {
        if (_state != State.Scanning)
        {
            Debug.LogError("Die Manager not scanning, so can't stop scanning");
            return false;
        }

        Debug.Log("stop scan");

        // Stop scanning
        BluetoothLEHardwareInterface.StopScan();
        _state = State.Idle;
        return true;
    }

    /// <summary>
    /// Connect to a die
    /// </summary>
    public void ConnectDie(
        IDie die,
        System.Action<IDie, bool, string> connectionResultCallback,
        System.Action<IDie, byte[]> onDataCallback,
        System.Action<IDie, string> onUnexpectedDisconnectionCallback)
    {
        if (_dice.TryGetValue(die.address, out Die ddie))
        {
            if (ddie.state != Die.State.Advertising)
            {
                Debug.LogError("Die " + die.name + " in invalid state " + ddie.state);
                return;
            }

            ddie.state = Die.State.Connecting;
            ddie.startTime = Time.time;
            ddie.deviceConnected = false;
            ddie.messageReadCharacteristicFound = false;
            ddie.messageWriteCharacteristicFound = false;
            ddie.onConnectionResult = connectionResultCallback;
            ddie.onData = onDataCallback;
            ddie.onUnexpectedDisconnection = onUnexpectedDisconnectionCallback;

            Debug.Log("Connecting to die " + ddie.name);

            // And kick off the connection!
            BluetoothLEHardwareInterface.ConnectToPeripheral(die.address, OnDeviceConnected, OnServiceDiscovered, OnCharacteristicDiscovered, OnDeviceDisconnected);
        }
        else
        {
            Debug.LogError("Trying to connect to unknown die " + die.name);
        }
    }

    /// <summary>
    /// Disconnect from a given die
    /// </summary>
    public void DisconnectDie(
        IDie die,
        System.Action<IDie, bool, string> onDisconnectionResult)
    {
        if (_dice.TryGetValue(die.address, out Die ddie))
        {
            Debug.Log("Disconnecting die " + die.name);
            if (ddie.state == Die.State.Advertising)
            {
                Debug.LogError("Die " + die.name + " in invalid state " + ddie.state);
                return;
            }

            // And kick off the disconnection!
            ddie.state = Die.State.Disconnecting;
            ddie.onDisconnectionResult = onDisconnectionResult;
            BluetoothLEHardwareInterface.DisconnectPeripheral(die.address, null); // <-- we don't use this callback, we already have one
        }
        else
        {
            Debug.LogError("Trying to disconnect unknown die " + die.name);
        }
    }

    /// <summary>
    /// Writes data to a connected die
    /// </summary>
    /// <param name="die">The die to write to</param>
    /// <param name="bytes">The data to write</param>
    /// <param name="length">The length of the data (can be less than the buffer length)</param>
    /// <param name="bytesWrittenCallback">Callback for when the data is written</param>
    public void WriteDie(IDie die, byte[] bytes, int length, System.Action<IDie, bool, string> bytesWrittenCallback)
    {
        if (_dice.TryGetValue(die.address, out Die ddie))
        {
            if (ddie.state != Die.State.Ready)
            {
                Debug.LogError("Die " + die.name + " in invalid state " + ddie.state);
                return;
            }

            // Set the callbacks
            ddie.onWriteResult = bytesWrittenCallback;
            ddie.state = Die.State.Writing;

            // Write the data!
            BluetoothLEHardwareInterface.WriteCharacteristic(die.address, serviceGUID, writeCharacteristic, bytes, length, false, OnCharacteristicWritten);
        }
        else
        {
            Debug.LogError("Unknown die " + die.name + " received data!");
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        _state = State.Uninitialized;
        _dice = new Dictionary<string, Die>();
    }

    void Start()
    {
#if !UNITY_EDITOR_OSX
        BluetoothLEHardwareInterface.Initialize(true, false, OnBluetoothInitComplete, OnError);
#else
        OnBluetoothInitComplete();
#endif
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        BluetoothLEHardwareInterface.DeInitialize(null);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var die in _dice.Values)
        {
            if (die.state == Die.State.Connecting)
            {
                CheckDieCharacteristics(die);
            }
            else if (die.state == Die.State.Subscribing)
            {
                CheckSubscriptionState(die);
            }
        }
    }

    void OnBluetoothInitComplete()
    {
        // We're ready!
        _state = State.Idle;
    }

    void OnError(string error)
    {
        bool errorAttributed = false;
        var addressesToRemove = new List<string>();
        foreach (var die in _dice.Values)
        {
            switch (die.state)
            {
                case Die.State.Disconnecting:
                    // We got an error while this die was disconnecting,
                    // Just indicate it
                    die.onDisconnectionResult?.Invoke(die, false, error);
                    die.onDisconnectionResult = null;
                    addressesToRemove.Add(die.address);
                    Debug.LogError("Error while disconnecting " + die.name + ": " + error);
                    errorAttributed = true;
                    break;
                case Die.State.Advertising:
                    // Ignore this die
                    break;
                case Die.State.Connecting:
                    die.onConnectionResult?.Invoke(die, false, error);
                    die.onConnectionResult = null;
                    die.onUnexpectedDisconnection = null;
                    die.onData = null;

                    // Temporarily add the die to the connected list to avoid an error message during the disconnect
                    // And force a disconnect
                    // Note: I'm not completely sure if we should trigger the disconnect or just remove the die
                    die.state = Die.State.Disconnecting;
                    BluetoothLEHardwareInterface.DisconnectPeripheral(die.address, null);
                    errorAttributed = true;
                    break;
                case Die.State.Connected:
                    // Ignore this die
                    break;
                case Die.State.Subscribing:
                    {
                        Debug.LogError("Characteristic Error: " + die.name + ": " + error);
                        die.onConnectionResult?.Invoke(die, false, error);
                        die.onConnectionResult = null;
                        die.onUnexpectedDisconnection = null;
                        die.onData = null;

                        // Temporarily add the die to the connected list to avoid an error message during the disconnect
                        // And force a disconnect
                        die.state = Die.State.Disconnecting;
                        BluetoothLEHardwareInterface.DisconnectPeripheral(die.address, null);
                        errorAttributed = true;

                        // Only kick off the next subscription IF this was the 'current' subscription attempt
                        // Otherwise there will still be a subscription success/fail event and we'll trigger
                        // the next one then.
                        StartNextSubscribeToCharacteristic();
                    }
                    break;
                case Die.State.Writing:
                    {
                        die.onWriteResult?.Invoke(die, false, error);
                        die.onWriteResult = null;
                        die.state = Die.State.Ready;
                        Debug.LogError("Write Error: " + die.name + ": " + error);
                        errorAttributed = true;
                    }
                    break;
                case Die.State.Ready:
                default:
                    // ignore this die
                    break;
            }
        }

        // Remove all the dice that errored out!
        // In most every case this is only one die
        foreach (var add in addressesToRemove)
        {
            _dice.Remove(add);
        }

        // Print something!
        if (!errorAttributed)
        {
            Debug.LogError(error);

            // Then pass it onto the current error handler(s)
            onBluetoothError?.Invoke(error);
        }
    }

    void OnDeviceDiscovered(
        string address,
        string name,
        System.Action<IDie> onDieDiscovered,
        System.Action<IDie, int, byte[]> onCustomAdvertisingData)
    {
        if (_dice.TryGetValue(address, out Die die))
        {
            switch (die.state)
            {
            case Die.State.Advertising:
                // We already know about this die, just update the advertising data handler
                die.onCustomAdvertisingData = onCustomAdvertisingData;
                break;
            case Die.State.Connecting:
                // We're about to make a connection, ignore the die
                break;
            default:
                Debug.LogError("Advertising die " + die.name + " in incorrect state " + die.state);
                _dice.Remove(address);
                break;
            }
        }
        else
        {
            // We didn't know this die before, create it
            die = new Die(address, name);
            _dice.Add(address, die);

            Debug.Log("Discovered new die " + die.name);

            // Notify die!
            die.state = Die.State.Advertising; // <-- this is the default value, but it doesn't hurt to be explicit
            die.onCustomAdvertisingData = onCustomAdvertisingData;
            onDieDiscovered?.Invoke(die);
        }
    }

    void OnDeviceAdvertisingInfo(string address, string name, int rssi, byte[] data) 
    {
        if (_dice.TryGetValue(address, out Die d))
        {
            Debug.Log("Die advertising data" + data.ToString());
            d.onCustomAdvertisingData?.Invoke(d, rssi, data);
        }
        else 
        {
            Debug.LogError("Received advertising data for unknown die address" + address);
        }
    }

    void OnDeviceConnected(string address)
    {
        if (_dice.TryGetValue(address, out Die die))
        {
            if (die.state != Die.State.Connecting)
            {
                Debug.LogError("Advertising die " + die.name + " in incorrect state " + die.state);
                return;
            }

            // This die received notification that it was connected to, but not necessarily found the characteristics
            die.deviceConnected = true;
            die.onCustomAdvertisingData = null;

            // Are we ready to move onto the next phase?
            CheckDieCharacteristics(die);
        }
        else
        {
            Debug.LogError("Unknown die " + address + " connected!");
        }
    }

    void OnDeviceDisconnected(string address)
    {
        // Check that this isn't an error-triggered disconnect, if it is, skip sending messages to the die
        if (_dice.TryGetValue(address, out Die die))
        {
            switch (die.state)
            {
                case Die.State.Disconnecting:
                    // This is perfectly okay
                    die.onDisconnectionResult?.Invoke(die,true, null);
                    die.onDisconnectionResult = null;
                    die.state = Die.State.Advertising;
                    Debug.Log("Disconnected " + die.name);
                    break;
                case Die.State.Advertising:
                    {
                        string errorString = "Incorrect state " + die.state;
                        die.onUnexpectedDisconnection?.Invoke(die, errorString);
                        _dice.Remove(address);
                        Debug.LogError("Disconnected " + die.name + ":" + errorString);
                    }
                    break;
                case Die.State.Connecting:
                case Die.State.Connected:
                    {
                        string errorString = "Disconnected before subscribing (state = " + die.state + ")";
                        die.onConnectionResult?.Invoke(die, false, errorString);
                        _dice.Remove(address);
                        Debug.LogError("Disconnected " + die.name + ":" + errorString);
                    }
                    break;
                case Die.State.Subscribing:
                    {
                        string errorString = "Disconnected while subscribing";
                        die.onConnectionResult?.Invoke(die, false, errorString);
                        _dice.Remove(address);
                        Debug.LogError("Disconnected " + die.name + ":" + errorString);

                        // Only kick off the next subscription IF this was the 'current' subscription attempt
                        // Otherwise there will still be a subscription success/fail event and we'll trigger
                        // the next one then.
                        StartNextSubscribeToCharacteristic();
                    }
                    break;
                case Die.State.Writing:
                    {
                        string errorString = "Disconnected while writing data to device";
                        die.onWriteResult?.Invoke(die, false, errorString);
                        die.onUnexpectedDisconnection?.Invoke(die, errorString);
                        _dice.Remove(address);
                        Debug.LogError("Disconnected " + die.name + ":" + errorString);
                    }
                    break;
                case Die.State.Ready:
                default:
                    {
                        string errorString = "Device disconnected";
                        die.onUnexpectedDisconnection?.Invoke(die, errorString);
                        _dice.Remove(address);
                        Debug.LogWarning("Disconnected " + die.name + ":" + errorString);
                    }
                    break;
            }
        }
        else
        {
            Debug.LogError("Unknown die " + address + " disconnected!");
        }
    }

    void OnServiceDiscovered(string address, string service)
    {
        // Nothing to do for now
    }

    void OnCharacteristicDiscovered(string address, string service, string characteristic)
    {
        if (_dice.TryGetValue(address, out Die die))
        {
            // We are looking for 2 characteristics, a generic read and a generic write!
            if (string.Compare(service.ToLower(), serviceGUID.ToLower()) == 0)
            {
                if (die.state != Die.State.Connecting)
                {
                    Debug.LogError("Die " + die.name + " in invalid state " + die.state);
                    return;
                }

                if (string.Compare(characteristic.ToLower(), subscribeCharacteristic.ToLower()) == 0)
                    die.messageReadCharacteristicFound = true;
                else if (string.Compare(characteristic.ToLower(), writeCharacteristic.ToLower()) == 0)
                    die.messageWriteCharacteristicFound = true;

                // Are we ready to move onto the next step?
                CheckDieCharacteristics(die);
            }
            // Else ignore this characteristic
        }
        else
        {
            Debug.LogError("Unknown die " + address + " discovered characteristic!");
        }
    }

    void CheckDieCharacteristics(Die die)
    {
        // Check that the die has the read and write characteristics
        if (die.deviceConnected &&
            die.messageReadCharacteristicFound &&
            die.messageWriteCharacteristicFound)
        {
            die.state = Die.State.Connected;

            // Subscribe, but only subscribe to one characteristic at a time
            StartNextSubscribeToCharacteristic();
        }
        else
        {
            // Check timeout!
            if (Time.time - die.startTime > DiscoverCharacteristicsTimeout)
            {
                // Wrong characteristics, we can't talk to this die!
                string errorString = "Timeout looking for characteristics on Die";
                die.onConnectionResult?.Invoke(die, false, errorString);
                die.onConnectionResult = null;
                die.onUnexpectedDisconnection = null;
                die.onData = null;
                Debug.LogError("Characteristic Error: " + die.name + ": " + errorString);

                // Temporarily add the die to the connected list to avoid an error message during the disconnect
                // And force a disconnect
                die.state = Die.State.Disconnecting;
                BluetoothLEHardwareInterface.DisconnectPeripheral(die.address, null);
            }
            // Else just keep waiting
        }
    }

    void StartNextSubscribeToCharacteristic()
    {
        Die nextToSub = _dice.Values.FirstOrDefault(d => d.state == Die.State.Connected);
        if (nextToSub != null)
        {
            nextToSub.state = Die.State.Subscribing;

            // Set timeout...
            nextToSub.startTime = Time.time;

            // And subscribe!
            BluetoothLEHardwareInterface.SubscribeCharacteristic(
                nextToSub.address,
                serviceGUID,
                subscribeCharacteristic,
                OnCharacteristicSubscriptionChanged,
                (charac, data) => OnCharacteristicData(nextToSub.address, data));
        }
        // Else no more subscription pending
    }

    void OnCharacteristicSubscriptionChanged(string characteristic)
    {
        Die sub = _dice.Values.FirstOrDefault(d => d.state == Die.State.Subscribing);
        if (sub != null)
        {
            sub.state = Die.State.Ready;
            sub.onConnectionResult?.Invoke(sub, true, null);
            sub.onConnectionResult = null;

            StartNextSubscribeToCharacteristic();
        }
        else
        {
            sub = _dice.Values.FirstOrDefault(d => d.state == Die.State.Disconnecting);
            if (sub == null)
            {
                Debug.LogError("Subscription success but no subscribing die");
            }
        }
    }

    void CheckSubscriptionState(Die die)
    {
        if (Time.time - die.startTime > SubscribeCharacteristicsTimeout)
        {
            string errorString = "Timeout trying to subscribe to die";
            Debug.LogError("Characteristic Error: " + die.name + ": " + errorString);
            die.onConnectionResult?.Invoke(die, false, errorString);
            die.onConnectionResult = null;
            die.onUnexpectedDisconnection = null;
            die.onData = null;

            // Temporarily add the die to the connected list to avoid an error message during the disconnect
            // And force a disconnect
            die.state = Die.State.Disconnecting;
            BluetoothLEHardwareInterface.DisconnectPeripheral(die.address, null);

            StartNextSubscribeToCharacteristic();
        }
    }

    void OnCharacteristicWritten(string characteristicId)
    {
        // It sucks that the bluetooth interface doesn't tell us the die...
        // So assume it to be the first die that is currently writing
        // It's incorrect, but it's the best we can do.
        var die = _dice.Values.FirstOrDefault(d => d.state == Die.State.Writing);
        if (die != null)
        {
            die.state = Die.State.Ready;
            die.onWriteResult?.Invoke(die, true, null);
            die.onWriteResult = null;
        }
        else
        {
            Debug.LogError("Unknown die received data!");
        }
    }

    void OnCharacteristicData(string address, byte[] data)
    {
        if (_dice.TryGetValue(address, out Die die))
        {
            if (die.state != Die.State.Ready)
            {
                Debug.LogError("Die " + die.name + " in invalid state " + die.state);
                return;
            }

            // Pass on the data
            die.onData?.Invoke(die, data);
        }
        else
        {
            Debug.LogError("Unknown die " + address + " received data!");
        }
    }
}
