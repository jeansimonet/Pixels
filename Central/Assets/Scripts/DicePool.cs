using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

public class DicePool : SingletonMonoBehaviour<DicePool>
{
    /// <summary>
    /// This enum indicates whether the die is actually there, connectable to, etc...
    /// </sumary>
    public enum DieAvailabilityState
    {
        Invalid = -1,// This is the value right after creation
        Unknown = 0, // After loading die info from file, we don't know if the die is there
        New,         // This is a new die we scanned, we didn't know about
        Available,   // This is a die we new about and scanned
        Connected,   // This die is connected, we just need to get some info from it
        Ready        // Die is ready for general use
    }

    public delegate void BluetoothErrorEvent(string errorString);
    public delegate void DieCreationEvent(Die die);
    public delegate void DieAvailabilityEvent(Die die, DieAvailabilityState oldState, DieAvailabilityState newState);
    public delegate void ScanEvent(bool scanning);

    // A bunch of events for UI to hook onto and display pool state updates
    public ScanEvent onScanStartStop;
    public DieCreationEvent onDieCreated;
    public DieAvailabilityEvent onDieAvailabilityChanged;
    public DieCreationEvent onWillDestroyDie;
    public BluetoothErrorEvent onBluetoothError;

    // A number of helpers to get various sublists of dice
    public IEnumerable<Die> unknownDice => dice.Where(d => d.state == DieAvailabilityState.Unknown).Select(d => d.die);
    public IEnumerable<Die> newDice => dice.Where(d => d.state == DieAvailabilityState.New).Select(d => d.die);
    public IEnumerable<Die> availableDice => dice.Where(d => d.state == DieAvailabilityState.Available).Select(d => d.die);
    public IEnumerable<Die> connectedDice => dice.Where(d => d.state == DieAvailabilityState.Connected).Select(d => d.die);
    public IEnumerable<Die> readyDice => dice.Where(d => d.state == DieAvailabilityState.Ready).Select(d => d.die);

    public IEnumerable<Die> allPoolDice => dice.Where(d => d.state != DieAvailabilityState.New).Select(d => d.die);
    public IEnumerable<Die> allPresentDice => dice.Where(d => d.state == DieAvailabilityState.Available ||
            d.state == DieAvailabilityState.Connected || d.state == DieAvailabilityState.Ready).Select(d => d.die);

    /// <summary>
    /// Small class to hold a die and associated pool status (whether it's actually there, etc..)
    /// </sumary>
    class DicePoolDie
    {
        public Die die;
        public DieAvailabilityState state;
    }
    List<DicePoolDie> dice = new List<DicePoolDie>();

    /// <summary>
    /// Start scanning for new and existing dice, filling our lists in the process from
    /// events triggered by Central.
    /// </sumary>
    public void BeginScanForDice()
    {
        Central.Instance.onDieAdvertisingData += OnDieAdvertisingData;
        Central.Instance.onDieDiscovered += OnDieDiscovered;

        // Remove any previously new die first, since we're about to "find" them again
        dice.RemoveAll(d => d.state == DieAvailabilityState.New);

        Central.Instance.BeginScanForDice();
        onScanStartStop?.Invoke(true);
    }

    /// <summary>
    /// Stops the current scan
    /// </sumary>
    public void StopScanForDice()
    {
        Central.Instance.StopScanForDice();
        Central.Instance.onDieAdvertisingData -= OnDieAdvertisingData;
        Central.Instance.onDieDiscovered -= OnDieDiscovered;
        onScanStartStop?.Invoke(false);
    }

    /// <summary>
    /// Tells the pool to connect to the specified die
    /// Note that the connection may fail if the die is not available
    /// </sumary>
    public void ConnectDie(Die die)
    {
        // Does the die have an address? This means we have scanned it already
        ConnectDice(Enumerable.Repeat(die, 1));
    }

    /// <summary>
    /// Tells the pool to connect to a list of dice
    /// </sumary>
    public void ConnectDice(IEnumerable<Die> dice)
    {
        List<Die> unknownDice = new List<Die>();
        foreach (var die in dice)
        {
            if (!string.IsNullOrEmpty(die.address))
            {
                Central.Instance.ConnectDie(die);
            }
            else
            {
                unknownDice.Add(die);
            }
        }

        if (unknownDice.Count > 0)
        {
            StartCoroutine(ScanAndConnectDiceCr(unknownDice));
        }
    }

    /// <summary>
    /// Disconnects a die, doesn't remove it from the pool though
    /// </sumary>
    public void DisconnectDie(Die die)
    {
        Central.Instance.DisconnectDie(die);
    }

    /// <summary>
    /// Removes a die from the pool, as if we never new it.
    /// Note: We may very well 'discover' it again the next time we scan.
    /// </sumary>
    public void ForgetDie(Die die)
    {
        // Remove the die from our pool
        int dieIndex = dice.FindIndex(d => d.die == die);
        if (dieIndex != -1)
        {
            dice.RemoveAt(dieIndex);
        }
        // Destroy it (this will cleanly disconnect if necessary)
        DestroyDie(die);

        // And make sure we save the pool data
        SavePool();
    }

    /// <summary>
    /// Write some data to the die
    /// </sumary>
    public void WriteDie(Die die, byte[] bytes, int length, System.Action bytesWrittenCallback)
    {
        Central.Instance.WriteDie(die, bytes, length, bytesWrittenCallback);
    }

    void Awake()
    {
        Central.Instance.onBluetoothError += OnBluetoothError;
        Central.Instance.onDieConnected += OnDieConnected;
        Central.Instance.onDieDisconnected += OnDieDisconnected;
        Central.Instance.onDieData += OnDieData;
    }

    void Start()
    {
        LoadPool();
    }

    void OnBluetoothError(string message)
    {
        onBluetoothError?.Invoke(message);
    }

    /// <summary>
    /// Called by Central when a new die is discovered!
    /// </sumary>
    void OnDieDiscovered(Central.IDie die)
    {
        // If the die exists, tell it that it's advertising now
        // otherwise create it (and tell it that its advertising :)
        var ourDie = dice.Find(d => {
            if (!string.IsNullOrEmpty(d.die.address))
                return d.die.address == die.address;
            else
                return d.die.name == die.name;
            });

        if (ourDie == null)
        {
            // Never seen this die before
            ourDie = new DicePoolDie() { die = CreateDie(die.address, die.name), state = DieAvailabilityState.New };
            // Tell the die that it's advertising
            ourDie.die.OnAdvertising();
            // And trigger availability event
            onDieAvailabilityChanged?.Invoke(ourDie.die, DieAvailabilityState.Invalid, DieAvailabilityState.New);
            // Add to the pool
            dice.Add(ourDie);
        }
        else
        {
            // Update die address if necessary
            if (string.IsNullOrEmpty(ourDie.die.address))
            {
                ourDie.die.UpdateAddress(die.address);
            }
            // Tell the die that it's advertising now!
            ourDie.die.OnAdvertising();
            // And remember the new state
            ourDie.state = DieAvailabilityState.Available;
            // Trigger availability event
            onDieAvailabilityChanged?.Invoke(ourDie.die, DieAvailabilityState.Unknown, DieAvailabilityState.Available);
        }

    }

    /// <summary>
    /// Called by Central when it receives custom advertising data, this allows us to change the
    /// appearance of the scanned die before even connecting to it!
    /// </sumary>
    void OnDieAdvertisingData(Central.IDie die, byte[] data)
    {
        // Find the die by its address, in both lists of dice we expect to know and new dice
        var ourDie = dice.Find(d => d.die.address == die.address);
        if (ourDie != null)
        {
            // Marshall the data into the struct we expect
            int size = Marshal.SizeOf(typeof(Die.CustomAdvertisingData));
            if (data.Length == size)
            {
                System.IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(data, 0, ptr, size);
                var customData = Marshal.PtrToStructure<Die.CustomAdvertisingData>(ptr);
                Marshal.FreeHGlobal(ptr);

                // Update die data
                ourDie.die.UpdateAdvertisingData(customData);
            }
            else
            {
                Debug.LogError("Incorrect advertising data length " + data.Length + ", expected: " + size);
            }
        }
    }

    /// <summary>
    /// Called by Central when it receives data for a connected die!
    /// </sumary>
    void OnDieData(Central.IDie die, byte[] data)
    {
        // Pass on the data
        var ourDie = dice.Find(d => d.die.address == die.address);
        if (ourDie != null)
        {
            ourDie.die.OnData(data);
        }
        else
        {
            Debug.LogError("Received data for die " + die.name + " that isn't part of the pool");
        }
    }

    /// <summary>
    /// Called by central when a die is properly connected to (i.e. two-way communication is working)
    /// We still need to do a bit of work before the die can be available for general us though
    /// </sumary>
    void OnDieConnected(Central.IDie die)
    {
        var ourDie = dice.Find(d => d.die.address == die.address);
        if (ourDie != null)
        {
            // If we successfully connected to a new die, then it is no longer new, and should be
            // considered part of the persistent pool, so save the config
            var prevState = ourDie.state;
            if (ourDie.state == DieAvailabilityState.New)
            {
                // Changing the state to available implies that this is now a persistent die
                // and it will be saved by the SavePool() call below.
                ourDie.state = DieAvailabilityState.Available;
                SavePool();
            }

            // Tell the die that it is connected now
            ourDie.die.OnConnected();

            // Remember that the die was just connected to (and trigger events)
            ourDie.state = DieAvailabilityState.Connected;
            onDieAvailabilityChanged?.Invoke(ourDie.die, prevState, DieAvailabilityState.Connected);

            // And have it update its info (unique Id, appearance, etc...) so it can finally be ready
            ourDie.die.UpdateInfo(OnDieReady);
        }
        else
        {
            Debug.LogError("Received Die connected notification for unknown die " + die.name);
        }
    }

    /// <summary>
    /// Called by the die once it has fetched its updated information (appearance, unique Id, etc.)
    /// </sumary>
    void OnDieReady(Die die, bool ready)
    {
        var ourDie = dice.Find(d => d.die.address == die.address);
        if (ourDie != null)
        {
            if (ready)
            {
                // Die is finally ready, awesome!
                ourDie.state = DieAvailabilityState.Ready;

                // Trigger events
                onDieAvailabilityChanged?.Invoke(ourDie.die, DieAvailabilityState.Connected, DieAvailabilityState.Ready);
            }
            else
            {
                // Updating info didn't work, disconnect the die
                DisconnectDie(die);
            }
        }
        else
        {
            Debug.LogError("Received Die ready notification for unknown die " + die.name);
        }
    }

    /// <summary>
    /// Called by Central when a die gets disconnected (on purpose or not)
    /// </sumary>
    void OnDieDisconnected(Central.IDie die)
    {
        var ourDie = dice.Find(d => d.die.address == die.address);
        if (ourDie != null)
        {
            ourDie.die.OnDisconnected();
            var oldState = ourDie.state;
            ourDie.state = DieAvailabilityState.Unknown;
            onDieAvailabilityChanged?.Invoke(ourDie.die, oldState, DieAvailabilityState.Unknown);
        }
    }

    /// <summary>
    /// Creates a new die for the pool
    /// </sumary>
    Die CreateDie(string name, string address, System.UInt64 deviceId = 0, int faceCount = 0, DiceVariants.DesignAndColor design = DiceVariants.DesignAndColor.Unknown)
    {
        var dieObj = new GameObject(name);
        dieObj.transform.SetParent(transform);
        Die ret = dieObj.AddComponent<Die>();
        ret.Setup(name, address, deviceId, faceCount, design);

        // Trigger event
        onDieCreated?.Invoke(ret);

        return ret;
    }

    /// <summary>
    /// Cleanly destroys a die, disconnecting if necessary and triggering events in the process
    /// </sumary>
    void DestroyDie(Die die)
    {
        // Disconnect first
        CheckDisconnectDie(die);
        // Trigger event
        onWillDestroyDie?.Invoke(die);
        GameObject.Destroy(die.gameObject);
    }

    /// <summary>
    /// Empties the pool, disconnecting/destroying dice in the process
    /// </sumary>
    void ClearPool()
    {
        foreach(var die in dice)
        {
            // Disconnect first
            CheckDisconnectDie(die.die);
            onWillDestroyDie?.Invoke(die.die);
            GameObject.Destroy(die.die.gameObject);
        }
        dice.Clear();
    }

    /// <summary>
    /// When asked to connect to a list of dice that haven't even been scanned yet
    /// </sumary>
    IEnumerator ScanAndConnectDiceCr(List<Die> dice)
    {
        List<Die> discoveredDice = new List<Die>();
        void localDieDiscovered(Central.IDie die)
        {
            var ourDie = dice.Find(d => d.name == die.name);
            if (ourDie != null)
            {
                ourDie.UpdateAddress(die.address);
                ourDie.OnAdvertising();
                discoveredDice.Add(ourDie);
            }
            // Else we don't care about that die
        }

        Central.Instance.onDieDiscovered += localDieDiscovered;
        Central.Instance.BeginScanForDice();
        try
        {
            float startTime = Time.time;
            float endTime = startTime + AppConstants.Instance.ScanTimeout;
            while (Time.time < endTime && discoveredDice.Count < dice.Count)
            {
                yield return null;
            }
        }
        finally
        {
            Central.Instance.StopScanForDice();
            Central.Instance.onDieDiscovered -= localDieDiscovered;
        }

        // Connect to all the dice we discovered
        foreach (var die in discoveredDice)
        {
            Central.Instance.ConnectDie(die);
        }
    }

    /// <summary>
    /// Checks if a die is connected, and if so, disconnects it :) yay!
    /// </sumary>
    void CheckDisconnectDie(Die die)
    {
        var ourDie = dice.Find(d => d.die.address == die.address);
        if (ourDie != null)
        {
            switch (ourDie.state)
            {
                case DieAvailabilityState.Connected:
                case DieAvailabilityState.Ready:
                    DisconnectDie(die);
                    break;
                default:
                    // Do nothing
                    break;
            }
        }
    }

    /// <summary>
    /// Small data structure used to write out our pool of dice to json
    /// </sumary>
    [System.Serializable]
    struct JsonData
    {
        [System.Serializable]
        public struct Dice
        {
            public string name;
            public System.UInt64 deviceId;
            public int faceCount; // Which kind of dice this is
            public DiceVariants.DesignAndColor designAndColor; // Physical look
        }
        public List<Dice> dice;
    }

    /// <summary>
    /// Save our pool to JSON!
    /// </sumary>
    string ToJson()
    {
        JsonData data = new JsonData();
        data.dice = new List<JsonData.Dice>();
        // We only save the dice that we have indicated to be in the pool
        // (i.e. ignore dice that are 'new' and we didn't connect to)
        foreach (var dice in allPoolDice)
        {
            data.dice.Add(new JsonData.Dice()
            {
                name = dice.name,
                deviceId = dice.deviceId,
                faceCount = dice.faceCount,
                designAndColor = dice.designAndColor
            });
        }
        return JsonUtility.ToJson(data);
    }

    /// <summary>
    /// Load our pool from JSON!
    /// </sumary>
    void FromJson(string json)
    {
        var data = JsonUtility.FromJson<JsonData>(json);

        // Clear and recreate the list of dice
        ClearPool();
        if (data.dice != null)
        {
            foreach (var ddie in data.dice)
            {
                // Create a disconnected die
                var die = CreateDie(ddie.name, null, ddie.deviceId, ddie.faceCount, ddie.designAndColor);
                dice.Add(new DicePoolDie() { die = die, state = DieAvailabilityState.Unknown });
            }
        }
    }

    /// <summary>
    /// Load our pool from file
    /// </sumary>
    void LoadPool()
    {
        var path = System.IO.Path.Combine(Application.persistentDataPath, AppConstants.Instance.PoolFilename);
        bool ret = File.Exists(path);
        if (ret)
        {
            string jsonText = File.ReadAllText(path);
            FromJson(jsonText);
        }
    }

    /// <summary>
    /// Save our pool to file
    /// </sumary>
    void SavePool()
    {
        var path = System.IO.Path.Combine(Application.persistentDataPath, AppConstants.Instance.PoolFilename);
        File.WriteAllText(path, ToJson());
    }

    public void CreateTestPool()
    {
        JsonData data = new JsonData();
        data.dice = new List<JsonData.Dice>();
        // We only save the dice that we have indicated to be in the pool
        // (i.e. ignore dice that are 'new' and we didn't connect to)
        data.dice.Add(new JsonData.Dice()
        {
            name = "Die 000",
            deviceId = 0x123456789ABCDEF0,
            faceCount = 20,
            designAndColor = DiceVariants.DesignAndColor.V3_Orange
        });
        data.dice.Add(new JsonData.Dice()
        {
            name = "Die 001",
            deviceId = 0xABCDEF0123456789,
            faceCount = 20,
            designAndColor = DiceVariants.DesignAndColor.V5_Black
        });
        data.dice.Add(new JsonData.Dice()
        {
            name = "Die 002",
            deviceId = 0xCDEF0123456789AB,
            faceCount = 20,
            designAndColor = DiceVariants.DesignAndColor.V5_Grey
        });
        data.dice.Add(new JsonData.Dice()
        {
            name = "Die 003",
            deviceId = 0xEF0123456789ABCD,
            faceCount = 20,
            designAndColor = DiceVariants.DesignAndColor.V5_Gold
        });
        string json = JsonUtility.ToJson(data);
        var path = System.IO.Path.Combine(Application.persistentDataPath, AppConstants.Instance.PoolFilename);
        File.WriteAllText(path, json);
    }
}
