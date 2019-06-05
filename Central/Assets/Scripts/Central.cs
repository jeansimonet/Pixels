using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public enum CentralState
{
    Uninitialized = 0,
    Initializing,
    Idle,
    Scanning,
    Connecting,
    Error,
}


public interface ICentral
{
    CentralState state { get; }
    void BeginScanForDice(System.Action<Die> foundDieCallback);
    void StopScanForDice();
    void ConnectToDie(Die die, System.Action<Die> dieConnectedCallback, System.Action<Die> dieDisconnectedCallback);
    void DisconnectDie(Die die, System.Action<Die> dieDisconnectedCallback);
    void ForgetDie(Die die, System.Action<Die> dieForgottenCallback);
    void DisconnectAllDice();
    IEnumerable<Die> diceList { get; }
}

public interface IClient
{
    void OnNewDie(Die die);
}

public interface ISendBytes
{
    void SendBytes(Die die, byte[] bytes, int length, System.Action bytesWrittenCallback);
}

public class Central
	: MonoBehaviour
    , ICentral
    , ISendBytes
{
    public string serviceGUID = "6E400000-B5A3-F393-E0A9-E50E24DCCA9E";
	public string subscribeCharacteristic = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";
	public string writeCharacteristic = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E";
    public string configFileName = "dicelist.json";
    public float connectingTimeout = 8.0f;

    CentralState _state = CentralState.Uninitialized;

    [System.Serializable]
    class DiceSetConfig
    {
        [System.Serializable]
        public class DiceAndAddress
        {
            public string name;
            public string address;
        }
        public List<DiceAndAddress> dice;
    }

    VirtualBluetoothInterface virtualBluetooth;

    public delegate void OnDieConnectionEvent(Die die);
    public OnDieConnectionEvent onDieDiscovered;
    public OnDieConnectionEvent onDieConnected;
    public OnDieConnectionEvent onDieDisconnected;
    public OnDieConnectionEvent onDieForgotten;

    HashSet<IClient> clients;

    public CentralState state
    {
        get { return _state; }
    }

    float connectingStartTime;

    void Awake()
    {
        clients = new HashSet<IClient>();
        virtualBluetooth = GetComponent<VirtualBluetoothInterface>();
    }

    void Start()
	{
		_state = CentralState.Initializing;
        #if !UNITY_EDITOR_OSX
		BluetoothLEHardwareInterface.Initialize(true, false,
		() =>
		{
			_state = CentralState.Idle;
		},
		(err) =>
		{
			_state = CentralState.Error;
            Debug.LogError("Error initializing Bluetooth Central: " + err);

        });
        #else
        _state = CentralState.Idle;
        #endif
	}

    void Update()
    {
        if (_state == CentralState.Connecting)
        {
            if (Time.time > connectingStartTime + connectingTimeout)
            {
                _state = CentralState.Idle;
            }
        }
    }

    public void RegisterClient(IClient client)
    {
        clients.Add(client);
        if (diceList != null)
        {
            foreach (var die in diceList)
            {
                client.OnNewDie(die);
            }
        }
    }

    public void UnregisterClient(IClient client)
    {
        clients.Remove(client);
    }

	public void BeginScanForDice(System.Action<Die> foundDieCallback)
	{
        if (_state == CentralState.Idle)
        {
            // Destroy any die that is not currently connected!
            foreach (var die in GetComponentsInChildren<Die>())
            {
                if (!die.connected)
                {
                    DestroyDie(die);
                }
            }

            System.Action<string, string> dieDiscovered =
                (address, name) =>
                {
                    if (!diceList.Any(dc => dc.address == address))
                    {
                        var die = CreateDie(name, address);
                        if (foundDieCallback != null)
                            foundDieCallback(die);
                        if (onDieDiscovered != null)
                            onDieDiscovered(die);
                    }
                };

            _state = CentralState.Scanning;
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

    Die CreateDie(string name, string address)
    {
        // Create a die game object
        var dieGO = new GameObject(name);
        dieGO.transform.SetParent(transform, false);

        // Add the Die component and return it!
        var ret = dieGO.AddComponent<Die>();
        ret.address = address;
        return ret;
    }

    public void StopScanForDice()
    {
        if (_state == CentralState.Scanning)
        {
            BluetoothLEHardwareInterface.StopScan();
            if (virtualBluetooth != null)
            {
                virtualBluetooth.StopScan();
            }
            _state = CentralState.Idle;
        }
        else
        {
            Debug.LogError("Central is not currently scanning for devices, current state is " + _state);
        }
    }

	public void ConnectToDie(Die die,
        System.Action<Die> dieConnectedCallback,
        System.Action<Die> dieDisconnectedCallback)
	{
        if (!die.connected && _state == CentralState.Idle)
        {
            _state = CentralState.Connecting;
            connectingStartTime = Time.time;
            bool readCharacDiscovered = false;
            bool writeCharacDiscovered = false;

            System.Action gotReadOrWriteCharacteristic = () =>
            {
                // Do we have both read and write access? If so we're good to go!
                if (readCharacDiscovered && writeCharacDiscovered)
                {
                    // If somehow we've timed out, skip this.
                    if (_state == CentralState.Connecting)
                    {
                        // We're ready to go
                        die.Connect(this);
                        _state = CentralState.Idle;
                        if (dieConnectedCallback != null)
                            dieConnectedCallback(die);
                        if (onDieConnected != null)
                            onDieConnected(die);
                        foreach (var client in clients)
                        {
                            client.OnNewDie(die);
                        }
                    }
                }
            };

            System.Action<string, string, string> onCharacteristicDiscovered =
                (ad, serv, charac) =>
                {
                    // Check for the service guid to match that for our dice (it's the Simblee one)
                    if (ad == die.address && serv.ToLower() == serviceGUID.ToLower())
                    {
                        // Check the discovered characteristic
                        if (charac.ToLower() == subscribeCharacteristic.ToLower())
                        {
                            // It's the read characteristic, subscribe to it!
                            System.Action<string, byte[]> onDataReceived =
                                (dev, data) =>
                                {
                                    die.DataReceived(data);
                                };

                            if (virtualBluetooth == null || !virtualBluetooth.IsVirtualDie(die.address))
                            {
                                BluetoothLEHardwareInterface.SubscribeCharacteristic(die.address,
                                serviceGUID,
                                subscribeCharacteristic,
                                null,
                                onDataReceived);
                            }
                            else
                            {
                                virtualBluetooth.SubscribeCharacteristic(die.address,
                                serviceGUID,
                                subscribeCharacteristic,
                                null,
                                onDataReceived);
                            }
                            readCharacDiscovered = true;
                            gotReadOrWriteCharacteristic();
                        }
                        else if (charac.ToLower() == writeCharacteristic.ToLower())
                        {
                            // It's the write characteristic, remember that
                            writeCharacDiscovered = true;
                            gotReadOrWriteCharacteristic();
                        }
                        // Else we don't care about this characteristic
                    }
                };

            System.Action<string> dieDisconnected =
                (ad) =>
                {
                    if (ad == die.address)
                    {
                        if (dieDisconnectedCallback != null)
                            dieDisconnectedCallback(die);
                        if (onDieDisconnected != null)
                            onDieDisconnected(die);
                        die.Disconnect();
                    }
                };

            if (virtualBluetooth == null || !virtualBluetooth.IsVirtualDie(die.address))
            {
                BluetoothLEHardwareInterface.ConnectToPeripheral(die.address,
                    null,
                    null,
                    onCharacteristicDiscovered,
                    dieDisconnected);
            }
            else
            {
                virtualBluetooth.ConnectToPeripheral(die.address,
                    null,
                    null,
                    onCharacteristicDiscovered,
                    dieDisconnected);
            }
        }
        else
        {
            Debug.LogError("Central is not ready to connect, current state is " + _state);
        }
	}

    public void DisconnectDie(Die die, System.Action<Die> dieDisconnectedCallback)
    {
        System.Action<string> dieDisconnected =
            (ad) =>
            {
                if (ad == die.address)
                {
                    if (dieDisconnectedCallback != null)
                        dieDisconnectedCallback(die);
                    // Other callbacks are already set
                }
            };

        if (virtualBluetooth == null || !virtualBluetooth.IsVirtualDie(die.address))
        {
            BluetoothLEHardwareInterface.DisconnectPeripheral(die.address, dieDisconnected);
        }
        else
        {
            virtualBluetooth.DisconnectPeripheral(die.address, dieDisconnected);
        }
    }

    public void DisconnectAllDice()
    {
        foreach (var die in GetComponentsInChildren<Die>())
        {
            die.Disconnect();
        }
        BluetoothLEHardwareInterface.DisconnectAll();
        if (virtualBluetooth != null)
        {
            virtualBluetooth.DisconnectAll();
        }
    }

    public void ForgetDie(Die die, System.Action<Die> dieForgottenCallback)
    {
        StartCoroutine(ForgetDieCr(die, dieForgottenCallback));
    }


    void DestroyDie(Die die)
    {
        GameObject.Destroy(die.gameObject);
    }

    public void SendBytes(Die die, byte[] bytes, int length, System.Action bytesWrittenCallback)
	{
        StringBuilder builder = new StringBuilder();
        builder.Append("Sending ");
        for (int i = 0; i < length; ++i)
        {
            builder.Append(bytes[i].ToString("X2"));
            builder.Append(" ");
        }
        Debug.Log(builder.ToString());
        if (virtualBluetooth == null || !virtualBluetooth.IsVirtualDie(die.address))
        {
            BluetoothLEHardwareInterface.WriteCharacteristic(die.address, serviceGUID, writeCharacteristic, bytes, length, false, (ignore) =>
                {
                    if (bytesWrittenCallback != null)
                    {
                        bytesWrittenCallback();
                    }
                });
        }
        else
        {
            virtualBluetooth.WriteCharacteristic(die.address, serviceGUID, writeCharacteristic, bytes, length, false, (ignore) =>
                {
                    if (bytesWrittenCallback != null)
                    {
                        bytesWrittenCallback();
                    }
                });
        }
	}

    public void OnApplicationQuit()
    {
        DisconnectAllDice();
    }

    public IEnumerable<Die> diceList
    {
        get
        {
            return GetComponentsInChildren<Die>();
        }
    }

    public void ConnectToDiceList(IEnumerable<Die> diceList, System.Action<Die> onDieConnected = null, System.Action<Die> onDieDisconnected = null)
    {
        StartCoroutine(ConnectToDiceListCr(diceList, onDieConnected, onDieDisconnected));
    }

    private IEnumerator ConnectToDiceListCr(IEnumerable<Die> diceList, System.Action<Die> onDieConnectedCallback = null, System.Action<Die> onDieDisconnectedCallback = null)
    {
        foreach(var die in diceList)
        {
            bool connected = false;
            ConnectToDie(die, (cdie) =>
            {
                connected = true;
                if (onDieConnectedCallback != null)
                    onDieConnectedCallback(cdie);
            },
            (cdie) =>
            {
                if (onDieDisconnectedCallback != null)
                    onDieDisconnectedCallback(cdie);
            });
            yield return new WaitUntil(() => connected);
        }
    }

    IEnumerator ForgetDieCr(Die die, System.Action<Die> dieForgottenCallback)
    {
        if (die.connected)
        {
            bool disconnected = false;
            DisconnectDie(die, (d) => disconnected = true);
            yield return new WaitUntil(() => disconnected);
        }

        if (dieForgottenCallback != null)
        {
            dieForgottenCallback(die);
        }

        if (onDieForgotten != null)
        {
            onDieForgotten(die);
        }

        DestroyDie(die);
    }


    //private void SaveDiceListToFile()
    //{

    //}

    //private bool LoadDiceListFromFile()
    //{
    //    string path = System.IO.Path.Combine(Application.persistentDataPath, configFileName);
    //    bool ret = System.IO.File.Exists(path);
    //    if (ret)
    //    {
    //        var jsonContent = System.IO.File.ReadAllText(path);
    //        ret = jsonContent != null;
    //        JsonUtility.FromJson<DiceListStruct>(jsonContent);
    //    }
    //}
}
