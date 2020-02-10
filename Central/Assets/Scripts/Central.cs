using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public class Central : MonoBehaviour
{
    /// <summary>
    /// What Central cares about when it comes to dice
    /// </summary>
    public interface IDie
    {
        string name { get; }
        string address { get; }
        void OnAdvertising();
        void OnConnected();
        void OnDisconnected();
        void OnData(byte[] data);
    }

    /// <summary>
    /// Internal Die definition, stores connection-relevant data
    /// </summary>
    class Die
    {
        public enum State
        {
            Advertising = 0,
            Connecting,
            Connected,
            Subscribing,
            Ready,
            Disconnecting,
        }

        public State state;
        public IDie die;

        public float startTime; // Used for timing out while looking for characteristics or subscribing to one
        public bool deviceConnected;
        public bool messageWriteCharacteristicFound;
        public bool messageReadCharacteristicFound;

        public Die(IDie die)
        {
            this.state = State.Advertising;
            this.die = die;
            this.startTime = float.MaxValue;
            this.deviceConnected = false;
            this.messageWriteCharacteristicFound = false;
            this.messageReadCharacteristicFound = false;
        }

        public string name => die?.name;
        public string address => die?.address;
    }

    public static Central Instance { get; private set; }

    const string serviceGUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";
    const string subscribeCharacteristic = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";
    const string writeCharacteristic = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E";

    const float DiscoverCharacteristicsTimeout = 5.0f; // seconds
    const float SubscribeCharacteristicsTimeout = 5.0f; // seconds

    System.Func<string, string, IDie> dieCreationFunc;

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

    public delegate void DieEvent(IDie die);
    public DieEvent onDieDiscovered;
    public DieEvent onDieConnected;
    public DieEvent onDieDisconnected;

    /// <summary>
    /// Allows dependency injection to create die instances
    /// </summary>
    public void RegisterFactory(System.Func<string, string, IDie> dieCreationFunc)
    {
        if (this.dieCreationFunc != null)
        {
            Debug.LogError("die factory method already registered");
        }
        else
        {
            this.dieCreationFunc = dieCreationFunc;
        }
    }

    /// <summary>
    /// Initiates a bluetooth scan
    /// </summary>
    public void BeginScanForDice()
    {
        if (_state != State.Idle)
        {
            Debug.LogError("Die Manager not ready to start scanning");
            return;
        }

        if (dieCreationFunc == null)
        {
            Debug.LogError("Die Manager - No die creation factory registered");
            return;
        }

        // Begin scanning
        _state = State.Scanning;

        // Notify of all the already known advertising dice
        foreach (var die in _dice.Values)
        {
            if (die.state == Die.State.Advertising)
            {
                onDieDiscovered?.Invoke(die.die);
            }
        }

        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(new string[] { serviceGUID }, OnDeviceDiscovered, null, false, true);
    }

    /// <summary>
    /// Stops scanning for new bluetooth devices
    /// </summary>
    public void StopScanForDice()
    {
        if (_state != State.Scanning)
        {
            Debug.LogError("Die Manager not scanning, so can't stop scanning");
            return;
        }

        // Stop scanning
        BluetoothLEHardwareInterface.StopScan();
        _state = State.Idle;
    }

    /// <summary>
    /// Connect to a die
    /// </summary>
    public void ConnectDie(IDie die)
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
    public void DisconnectDie(IDie die)
    {
        if (_dice.TryGetValue(die.address, out Die ddie))
        {
            if (ddie.state == Die.State.Advertising)
            {
                Debug.LogError("Die " + die.name + " in invalid state " + ddie.state);
                return;
            }

            // And kick off the disconnection!
            ddie.state = Die.State.Disconnecting;
            BluetoothLEHardwareInterface.DisconnectPeripheral(die.address, null);
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
    public void WriteDie(IDie die, byte[] bytes, int length, System.Action bytesWrittenCallback)
    {
        if (_dice.TryGetValue(die.address, out Die ddie))
        {
            if (ddie.state != Die.State.Ready)
            {
                Debug.LogError("Die " + die.name + " in invalid state " + ddie.state);
                return;
            }

            // Write the data!
            System.Action<string> onWritten = (ignore) => bytesWrittenCallback?.Invoke();
            BluetoothLEHardwareInterface.WriteCharacteristic(die.address, serviceGUID, writeCharacteristic, bytes, length, false, onWritten);
        }
        else
        {
            Debug.LogError("Unknown die " + die.name + " received data!");
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple Die Managers in scene");
        }
        else
        {
            Instance = this;
        }

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

    void OnApplicationQuit()
    {
        BluetoothLEHardwareInterface.DeInitialize(null);
    }

    void OnDestroy()
    {
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
        // Print something!
        Debug.LogError(error);

        // Then pass it onto the current error handler(s)
        onBluetoothError?.Invoke(error);
    }

    void OnDeviceDiscovered(string address, string name)
    {
        if (_dice.TryGetValue(address, out Die die))
        {
            if (die.state != Die.State.Advertising)
            {
                Debug.LogError("Advertising die " + die.name + " in incorrect state " + die.state);
                _dice.Remove(address);
            }
            // Else we just already know about this die and don't need to do anything
        }
        else
        {
            // We didn't know this die before, create it
            var idie = dieCreationFunc(address, name);
            die = new Die(idie);
            _dice.Add(address, die);

            // Notify die!
            die.state = Die.State.Advertising; // <-- this is the default value, but it doesn't hurt to be explicit
            idie.OnAdvertising();
            onDieDiscovered?.Invoke(idie);
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
            System.Action<Die> finishDisconnect = (d) =>
            {
                // Notify the die!
                d.die.OnDisconnected();
                onDieDisconnected?.Invoke(d.die);

                _dice.Remove(address);
            };

            switch (die.state)
            {
                case Die.State.Disconnecting:
                    // This is perfectly okay
                    finishDisconnect(die);
                    break;
                case Die.State.Advertising:
                    Debug.LogError("Disconnecting " + die.die.name + " is in incorrect state " + die.state);
                    break;
                case Die.State.Connecting:
                case Die.State.Connected:
                    Debug.LogWarning("Die " + die.die.name + " disconnected before subscribing");
                    finishDisconnect(die);
                    break;
                case Die.State.Subscribing:
                    Debug.LogWarning("Die " + die.die.name + " disconnected while subscribing");

                    // Clear error handler etc...
                    onBluetoothError -= OnCharacteristicSubscriptionError;
                    finishDisconnect(die);

                    // Only kick off the next subscription IF this was the 'current' subscription attempt
                    // Otherwise there will still be a subscription success/fail event and we'll trigger
                    // the next one then.
                    StartNextSubscribeToCharacteristic();
                    break;
                case Die.State.Ready:
                default:
                    finishDisconnect(die);
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
            if (die.state != Die.State.Connecting)
            {
                Debug.LogError("Die " + die.name + " in invalid state " + die.state);
                return;
            }

            // We are looking for 2 characteristics, a generic read and a generic write!
            if (string.Compare(service.ToLower(), serviceGUID.ToLower()) == 0)
            {
                if (string.Compare(characteristic.ToLower(), subscribeCharacteristic.ToLower()) == 0)
                    die.messageReadCharacteristicFound = true;
                else if (string.Compare(characteristic.ToLower(), writeCharacteristic.ToLower()) == 0)
                    die.messageWriteCharacteristicFound = true;

                // Are we ready to move onto the next step?
                CheckDieCharacteristics(die);
            }
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
                Debug.LogError("Timeout looking for characteristics on Die " + die.die.name);

                // Temporarily add the die to the connected list to avoid an error message during the disconnect
                die.state = Die.State.Disconnecting;

                // And force a disconnect
                BluetoothLEHardwareInterface.DisconnectPeripheral(die.die.address, null);
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

            // Hook error handler
            onBluetoothError += OnCharacteristicSubscriptionError;

            // Add timeout...
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

    void OnCharacteristicSubscriptionError(string error)
    {
        Die sub = _dice.Values.FirstOrDefault(d => d.state == Die.State.Subscribing);
        if (sub != null)
        {
            Debug.LogError("Die " + sub.name + " couldn't subscribe to read characteristic!");
            onBluetoothError -= OnCharacteristicSubscriptionError;

            // Temporarily add the die to the connected list to avoid an error message during the disconnect
            // And force a disconnect
            sub.state = Die.State.Disconnecting;
            BluetoothLEHardwareInterface.DisconnectPeripheral(sub.address, null);

            StartNextSubscribeToCharacteristic();
        }
        else
        {
            Debug.LogError("Subscription error but no subscribing die");
        }
    }

    void OnCharacteristicSubscriptionChanged(string characteristic)
    {
        Die sub = _dice.Values.FirstOrDefault(d => d.state == Die.State.Subscribing);
        if (sub != null)
        {
            // Clean up error handler
            onBluetoothError -= OnCharacteristicSubscriptionError;

            sub.state = Die.State.Ready;
            sub.die.OnConnected();
            onDieConnected?.Invoke(sub.die);

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
            Debug.LogError("Timeout trying to subscribe to die " + die.name);
            onBluetoothError -= OnCharacteristicSubscriptionError;

            // Temporarily add the die to the connected list to avoid an error message during the disconnect
            // And force a disconnect
            die.state = Die.State.Disconnecting;
            BluetoothLEHardwareInterface.DisconnectPeripheral(die.address, null);

            StartNextSubscribeToCharacteristic();
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
            die.die.OnData(data);
        }
        else
        {
            Debug.LogError("Unknown die " + address + " received data!");
        }
    }
}
