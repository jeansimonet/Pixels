using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

public class DicePool : SingletonMonoBehaviour<DicePool>
{
    public delegate void BluetoothErrorEvent(string errorString);
    public delegate void DieCreationEvent(Die die);

    // A bunch of events for UI to hook onto and display pool state updates
    public DieCreationEvent onDieCreated;
    public DieCreationEvent onWillDestroyDie;
    public BluetoothErrorEvent onBluetoothError;

    public IEnumerable<Die> allDice => dice;

    List<Die> dice = new List<Die>();
    Dictionary<Die, System.Action<Die.ConnectionState>> setState = new Dictionary<Die, System.Action<Die.ConnectionState>>();

    // Multiple things may request bluetooth scanning, so we need to arbitrate when
    // we actually ask Central to scan or not. This counter will let us know
    // exactly when to start or stop asking central.
    int scanRequestCount = 0;

    /// <summary>
    /// Start scanning for new and existing dice, filling our lists in the process from
    /// events triggered by Central.
    /// </sumary>
    public void RequestBeginScanForDice()
    {
        Debug.Log("request start scan");
        scanRequestCount++;
        if (scanRequestCount == 1)
        {
            // Remove any previously new die first, since we're about to "find" them again
            DestroyAll(d => d.connectionState == Die.ConnectionState.New);
            Central.Instance.BeginScanForDice(OnDieDiscovered, OnDieAdvertisingData);
        }
    }

    /// <summary>
    /// Stops the current scan 
    /// </sumary>
    public void RequestStopScanForDice()
    {
        Debug.Log("request stop scan");
        if (scanRequestCount == 0)
        {
            Debug.LogError("Pool not currently scanning");
        }
        else
        {
            scanRequestCount--;
            if (scanRequestCount == 0)
            {
                Central.Instance.StopScanForDice();

                // When we stop scanning, if there is any 'unknown' die left, considering missing instead
                foreach (var pd in dice.Where(d => d.connectionState == Die.ConnectionState.Unknown))
                {
                    // The die is no longer just unknown, it's missing
                    setState[pd].Invoke(Die.ConnectionState.Missing);
                }

            }
            // Else ignore
        }
    }

    public void IncludeDie(Die die)
    {
        if (die.connectionState == Die.ConnectionState.New)
        {
            setState[die].Invoke(Die.ConnectionState.Unknown);
        }
        SavePool();
    }

    public void ConnectDie(Die die)
    {
        if (die.connectionState == Die.ConnectionState.Available || die.connectionState == Die.ConnectionState.New)
        {
            setState[die].Invoke(Die.ConnectionState.Connecting);
            Central.Instance.ConnectDie(die, OnDieConnected, OnDieData, OnDieDisconnected);
        }
        else
        {
            Debug.LogError("Die " + die.name + " not in avaiable state, instead: " + die.connectionState);
        }
    }

    /// <summary>
    /// Disconnects a die, doesn't remove it from the pool though
    /// </sumary>
    public void DisconnectDie(Die die)
    {
        setState[die].Invoke(Die.ConnectionState.Disconnecting);
        Central.Instance.DisconnectDie(die, null);
    }

    /// <summary>
    /// Removes a die from the pool, as if we never new it.
    /// Note: We may very well 'discover' it again the next time we scan.
    /// </sumary>
    public void ForgetDie(Die die)
    {
        // Remove the die from our pool
        dice.Remove(die);

        // Destroy it (this will cleanly disconnect if necessary)
        DestroyDie(die);

        // And make sure we save the pool data
        SavePool();
    }

    /// <summary>
    /// Moves a die back to the unknown state, i.e. we don't trust it to be available anymore
    /// And the pool will check it the next time it scans for dice
    /// </sumary>
    public void DoubtDie(Die die)
    {
        if (die.connectionState == Die.ConnectionState.Available || die.connectionState == Die.ConnectionState.Missing)
        {
            setState[die].Invoke(Die.ConnectionState.Unknown);
        }
    }

    /// <summary>
    /// Write some data to the die
    /// </sumary>
    public void WriteDie(Die die, byte[] bytes, int length, System.Action<bool> bytesWrittenCallback)
    {
        // FIXME: Write something somewhere maybe?
        Central.Instance.WriteDie(die, bytes, length, (d, r, s) => bytesWrittenCallback?.Invoke(r));
    }

    void Awake()
    {
        Central.Instance.onBluetoothError += OnBluetoothError;
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
            if (!string.IsNullOrEmpty(d.address))
                return d.address == die.address;
            else
                return d.name == die.name;
            });

        if (ourDie == null)
        {
            // Never seen this die before
            ourDie = CreateDie(die.address, die.name);
            dice.Add(ourDie);
            setState[ourDie].Invoke(Die.ConnectionState.New);
        }
        else
        {
            // Update die address if necessary
            if (string.IsNullOrEmpty(ourDie.address))
            {
                ourDie.UpdateAddress(die.address);
            }

            switch (ourDie.connectionState)
            {
                case Die.ConnectionState.New:
                case Die.ConnectionState.Available:
                case Die.ConnectionState.CommError:
                    // Ignore
                    break;
                case Die.ConnectionState.Unknown: 
                case Die.ConnectionState.Missing:
                    {
                        // Tell the die that it's advertising now!
                        setState[ourDie].Invoke(Die.ConnectionState.Available);
                    }
                    break;
                default:
                    // All other are errors
                    Debug.LogError("Die " + ourDie.name + " in invalid state " + ourDie.connectionState);
                    break;
            }
        }

    }

    /// <summary>
    /// Called by Central when it receives custom advertising data, this allows us to change the
    /// appearance of the scanned die before even connecting to it!
    /// </sumary>
    void OnDieAdvertisingData(Central.IDie die, int rssi, byte[] data)
    {
        // Find the die by its address, in both lists of dice we expect to know and new dice
        var ourDie = dice.Find(d => d.address == die.address);
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
                ourDie.UpdateAdvertisingData(rssi, customData);
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
        var ourDie = dice.Find(d => d.address == die.address);
        if (ourDie != null)
        {
            ourDie.OnData(data);
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
    void OnDieConnected(Central.IDie die, bool result, string errorMessage)
    {
        var ourDie = dice.Find(d => d.address == die.address);
        if (ourDie != null)
        {
            var prevState = ourDie.connectionState;
            if (result)
            {
                // If we successfully connected to a new die, then it is no longer new, and should be
                // considered part of the persistent pool, so save the config
                if (ourDie.connectionState == Die.ConnectionState.New)
                {
                    // Changing the state to available implies that this is now a persistent die
                    // and it will be saved by the SavePool() call below.
                    setState[ourDie].Invoke(Die.ConnectionState.Available);
                }

                // Remember that the die was just connected to (and trigger events)
                setState[ourDie].Invoke(Die.ConnectionState.Identifying);

                // And have it update its info (unique Id, appearance, etc...) so it can finally be ready
                ourDie.UpdateInfo(OnDieReady);
            }
            else
            {
                // Could not connect to the die, indicate that!
                // Remember that the die was just connected to (and trigger events)
                setState[ourDie].Invoke(Die.ConnectionState.CommError);
            }
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
        var ourDie = dice.Find(d => d.address == die.address);
        if (ourDie != null)
        {
            if (ready)
            {
                // Die is finally ready, awesome!
                setState[ourDie].Invoke(Die.ConnectionState.Ready);
                SavePool();
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
    /// Called by Central when a die gets disconnected unexpectedly
    /// </sumary>
    void OnDieDisconnected(Central.IDie die, string errorMessage)
    {
        var ourDie = dice.FirstOrDefault(d => d.address == die.address);
        if (ourDie != null)
        {
            setState[ourDie].Invoke(Die.ConnectionState.Missing);
        }
    }

    /// <summary>
    /// Creates a new die for the pool
    /// </sumary>
    Die CreateDie(string address, string name, System.UInt64 deviceId = 0, int faceCount = 0, DiceVariants.DesignAndColor design = DiceVariants.DesignAndColor.Unknown)
    {
        var dieObj = new GameObject(name);
        dieObj.transform.SetParent(transform);
        Die ret = dieObj.AddComponent<Die>();
        var setStateAction = ret.Setup(name, address, deviceId, faceCount, design);
        setState.Add(ret, setStateAction);

        // Trigger event
        onDieCreated?.Invoke(ret);

        return ret;
    }

    /// <summary>
    /// Cleanly destroys a die, disconnecting if necessary and triggering events in the process
    /// Does not remove it from the list though
    /// </sumary>
    void DestroyDie(Die die)
    {
        // Disconnect first
        CheckDisconnectDie(die);

        // Trigger event
        onWillDestroyDie?.Invoke(die);

        setState[die].Invoke(Die.ConnectionState.Invalid);
        setState.Remove(die);
        GameObject.Destroy(die.gameObject);
    }

    /// <summary>
    /// Destroys all dice that fit a predicate, or all dice if predicate is null
    /// </sumary>
    void DestroyAll(System.Predicate<Die> predicate = null)
    {
        if (predicate != null)
        {
            var diceCopy = new List<Die>(dice);
            foreach (var die in diceCopy)
            {
                if (predicate(die))
                {
                    DestroyDie(die);
                    dice.Remove(die);
                }
            }
        }
        else
        {
            foreach (var die in dice)
            {
                DestroyDie(die);
            }
            dice.Clear();
        }
    }

    /// <summary>
    /// Empties the pool, disconnecting/destroying dice in the process
    /// </sumary>
    void ClearPool()
    {
        DestroyAll();
    }

    /// <summary>
    /// Checks if a die is connected, and if so, disconnects it :) yay!
    /// </sumary>
    void CheckDisconnectDie(Die die)
    {
        switch (die.connectionState)
        {
            case Die.ConnectionState.Connecting:
            case Die.ConnectionState.Identifying:
            case Die.ConnectionState.Ready:
                DisconnectDie(die);
                break;
            default:
                // Do nothing
                break;
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
        foreach (var dice in dice.Where(d => d.connectionState != Die.ConnectionState.New))
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
                Die die = CreateDie(null, ddie.name, ddie.deviceId, ddie.faceCount, ddie.designAndColor);
                dice.Add(die);
                setState[die].Invoke(Die.ConnectionState.Unknown);
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
