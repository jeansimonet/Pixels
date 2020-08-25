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

    class PoolDie
    {
        public Die die;
        public int count;
        public float lastRequestDisconnectTime;
        public System.Action<Die.ConnectionState> setState;
    }
    List<PoolDie> dice = new List<PoolDie>();

    public IEnumerable<Die> allDice => dice.Select(d => d.die);

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

                // When we stop scanning, if there is any 'unknown' die left, consider it missing instead
                foreach (var pd in dice.Where(d => d.die.connectionState == Die.ConnectionState.Unknown))
                {
                    // The die is no longer just unknown, it's missing
                    pd.setState.Invoke(Die.ConnectionState.Missing);
                }

            }
            // Else ignore
        }
    }

    public Die FindDie(Presets.EditDie editDie)
    {
        return allDice.FirstOrDefault(d =>
        {
            // We should only use device Id
            if (d.deviceId == 0 || editDie.deviceId == 0)
            {
                return d.name == editDie.name;
            }
            else
            {
                return d.deviceId == editDie.deviceId;
            }
        });
    }

    public void IncludeDie(Die die)
    {
        if (die.connectionState == Die.ConnectionState.New)
        {
            var pd = dice.First(p => p.die == die);
            pd.setState.Invoke(Die.ConnectionState.Unknown);
            UpdateDataSet();
        }
    }

    void scanForDieWithTimeout(Die d, float timeout)
    {
        StartCoroutine(scanForDieWithTimeoutCr(d, timeout));
    }

    IEnumerator scanForDieWithTimeoutCr(Die d, float timeout)
    {
        bool connStateChanged = false;
        void connStateChangedWatcher(Die dd, Die.ConnectionState oldState, Die.ConnectionState newState)
        {
            connStateChanged = true;
        }
        d.OnConnectionStateChanged += connStateChangedWatcher;
        RequestBeginScanForDice();

        float startTime = Time.time;
        yield return new WaitUntil(() => connStateChanged || (Time.time > startTime + timeout));

        d.OnConnectionStateChanged -= connStateChangedWatcher;
        RequestStopScanForDice();
    }

    public void GetDieReady(Die die, System.Action<Die, bool, string> dieReadyCallback)
    {
        void dieReady(bool result, string errorMessage)
        {
            die.OnConnectionStateChanged -= connectionStateWatcher;
            dieReadyCallback?.Invoke(die, result, errorMessage);
        }

        bool ignoreFirstCommError = true;
        void connectionStateWatcher(Die d, Die.ConnectionState ignoreOldState, Die.ConnectionState newState)
        {
            switch (newState)
            {
                case Die.ConnectionState.Ready:
                    // call our own callback directly
                    dieReady(true, null);
                    break;
                case Die.ConnectionState.New:
                    ignoreFirstCommError = false;
                    IncludeDie(die); // This will trigger a switch to "unknown"
                    break;
                case Die.ConnectionState.Available:
                    // Die must first be connected to
                    ignoreFirstCommError = false;
                    RequestConnectDie(die); // This will move through "connecting", "identifying" and "ready"
                    break;
                case Die.ConnectionState.Connecting:
                case Die.ConnectionState.Identifying:
                    // Just be notified when done
                    break;
                case Die.ConnectionState.Unknown:
                    // Must first scan to see if the die is there
                    ignoreFirstCommError = false;
                    scanForDieWithTimeout(die, 5.0f); // This will move the die to "available"
                    break;
                case Die.ConnectionState.Missing:
                case Die.ConnectionState.CommError:
                    // Must first scan to see if the die is there, but only if it's the first time we see this state
                    if (ignoreFirstCommError)
                    {
                        ignoreFirstCommError = false;
                        DoubtDie(die); // This will move the die back to Unknown
                    }
                    else
                    {
                        // Tried to scan and we coouldn't find the die
                        // TODO: Ask the user if they want to try again
                        dieReady(false, "Could not connect to Die " + die.name + ". Make sure it is charged and in range");
                    }
                    break;
                case Die.ConnectionState.Disconnecting:
                    // Wait to reconnect
                    break;
                case Die.ConnectionState.Invalid:
                    // Bug
                    Debug.LogError("Invalid Die " + die.name);
                    dieReady(false, "Die " + die.name + " is an invalid state");
                    break;
                case Die.ConnectionState.Removed:
                    // Error
                    dieReady(false, "Die " + die.name + " has been removed from your dice bag.");
                    break;
            }
        }

        if (die.connectionState == Die.ConnectionState.Ready)
        {
            // This won't do anything but it will make sure the die doesn't get disconnected from underneath us
            RequestConnectDie(die);
        }

        die.OnConnectionStateChanged += connectionStateWatcher;

        // Call the connection watcher once to trigger the initial connection / scanning, etc...
        connectionStateWatcher(die, die.connectionState, die.connectionState);
    }

    public void GetDieReady(Presets.EditDie editDie, System.Action<Die, bool, string> dieReadyCallback)
    {
        var die = DicePool.Instance.FindDie(editDie);
        if (die != null)
        {
            // Make sure the die is ready!
            GetDieReady(die, dieReadyCallback);
        }
    }



    public void RequestConnectDie(Die die)
    {
        var dt = dice.First(p => p.die == die);
        Debug.Log("Before request connect: " + dt.count);
        dt.count++;
        if (dt.count == 1)
        {
            // Bump the count once, we will use this to trigger the disconnect timer
            dt.count = 2;
            ConnectDie(die);
        }
        Debug.Log("After request connect: " + dt.count);
    }

    public void RequestDisconnectDie(Die die)
    {
        var dt = dice.First(p => p.die == die);
        Debug.Log("Before request disconnect: " + dt.count);
        dt.count--;
        if (dt.count == 1)
        {
            // Now we set the timer and only disconnect after a while
            dt.lastRequestDisconnectTime = Time.time;
        }
        Debug.Log("After request disconnect: " + dt.count);
    }

    void ConnectDie(Die die)
    {
        if (die.connectionState == Die.ConnectionState.Available || die.connectionState == Die.ConnectionState.New)
        {
            var dt = dice.First(p => p.die == die);
            dt.setState.Invoke(Die.ConnectionState.Connecting);
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
    void DisconnectDie(Die die)
    {
        if (die.connectionState == Die.ConnectionState.Ready)
        {
            var dt = dice.FirstOrDefault(p => p.die == die); // When disconnecting from a destroy, the die will have already been removed
            dt?.setState.Invoke(Die.ConnectionState.Disconnecting);
            Central.Instance.DisconnectDie(die, (_, res, errorMsg) =>
            {
                dt?.setState.Invoke(res ? Die.ConnectionState.Available : Die.ConnectionState.CommError);
            });
        }
    }

    /// <summary>
    /// Removes a die from the pool, as if we never new it.
    /// Note: We may very well 'discover' it again the next time we scan.
    /// </sumary>
    public void ForgetDie(Die die)
    {
        // Remove the die from our pool
        var dt = dice.First(p => p.die == die);
        dice.Remove(dt);

        // Destroy it (this will cleanly disconnect if necessary)
        DestroyDie(dt);

        // And make sure we save the pool data
        UpdateDataSet();
        AppDataSet.Instance.SaveData();
    }

    /// <summary>
    /// Moves a die back to the unknown state, i.e. we don't trust it to be available anymore
    /// And the pool will check it the next time it scans for dice
    /// </sumary>
    public void DoubtDie(Die die)
    {
        if (die.connectionState == Die.ConnectionState.Available || die.connectionState == Die.ConnectionState.Missing || die.connectionState == Die.ConnectionState.CommError)
        {
            var dt = dice.First(p => p.die == die);
            dt.setState.Invoke(Die.ConnectionState.Unknown);
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
        Initialize();
    }

    void Update()
    {
        foreach (var ourDie in dice)
        {
            if (ourDie.count == 1)
            {
                // This die is waiting to be disconnected
                if (Time.time > (ourDie.lastRequestDisconnectTime + AppConstants.Instance.DicePoolDisconnectDelay))
                {
                    Debug.Log("Update: 1 -> 0");
                    // Really disconnect
                    ourDie.count = 0;
                    DisconnectDie(ourDie.die);
                }
            }
        }
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
        var ourDie = dice.FirstOrDefault(d => {
            if (!string.IsNullOrEmpty(d.die.address))
                return d.die.address == die.address;
            else
                return d.die.name == die.name;
            });

        if (ourDie == null)
        {
            // Never seen this die before
            ourDie = CreateDie(die.address, die.name);
            ourDie.setState.Invoke(Die.ConnectionState.New);
        }
        else
        {
            // Update die address if necessary
            if (string.IsNullOrEmpty(ourDie.die.address))
            {
                ourDie.die.UpdateAddress(die.address);
            }

            switch (ourDie.die.connectionState)
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
                        ourDie.setState.Invoke(Die.ConnectionState.Available);
                    }
                    break;
                default:
                    // All other are errors
                    Debug.LogError("Die " + ourDie.die.name + " in invalid state " + ourDie.die.connectionState);
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
        var ourDie = dice.FirstOrDefault(d => d.die.address == die.address);
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
                ourDie.die.UpdateAdvertisingData(rssi, customData);
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
        var ourDie = dice.FirstOrDefault(d => d.die.address == die.address);
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
    void OnDieConnected(Central.IDie die, bool result, string errorMessage)
    {
        var ourDie = dice.Find(d => d.die.address == die.address);
        if (ourDie != null)
        {
            var prevState = ourDie.die.connectionState;
            if (result)
            {
                // If we successfully connected to a new die, then it is no longer new, and should be
                // considered part of the persistent pool, so save the config
                if (ourDie.die.connectionState == Die.ConnectionState.New)
                {
                    // Changing the state to available implies that this is now a persistent die
                    // and it will be saved by the SavePool() call below.
                    ourDie.setState.Invoke(Die.ConnectionState.Available);
                }

                // Remember that the die was just connected to (and trigger events)
                ourDie.setState.Invoke(Die.ConnectionState.Identifying);

                // And have it update its info (unique Id, appearance, etc...) so it can finally be ready
                ourDie.die.UpdateInfo(OnDieReady);
            }
            else
            {
                // Could not connect to the die, indicate that!
                // Remember that the die was just connected to (and trigger events)
                ourDie.setState.Invoke(Die.ConnectionState.CommError);
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
        var ourDie = dice.FirstOrDefault(d => d.die.address == die.address);
        if (ourDie != null)
        {
            if (ready)
            {
                // Die is finally ready, awesome!
                ourDie.setState.Invoke(Die.ConnectionState.Ready);
                UpdateDataSet();
                AppDataSet.Instance.SaveData();
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
        var ourDie = dice.FirstOrDefault(d => d.die.address == die.address);
        if (ourDie != null)
        {
            ourDie.setState.Invoke(Die.ConnectionState.Missing);
        }
    }

    /// <summary>
    /// Creates a new die for the pool
    /// </sumary>
    PoolDie CreateDie(string address, string name, System.UInt64 deviceId = 0, int faceCount = 0, DiceVariants.DesignAndColor design = DiceVariants.DesignAndColor.Unknown)
    {
        var dieObj = new GameObject(name);
        dieObj.transform.SetParent(transform);
        Die die = dieObj.AddComponent<Die>();
        var setStateAction = die.Setup(name, address, deviceId, faceCount, design);
        var ourDie = new PoolDie()
        {
            die = die,
            setState = setStateAction,
            count = 0,
            lastRequestDisconnectTime = 0.0f
        };
        dice.Add(ourDie);

        // Trigger event
        onDieCreated?.Invoke(die);

        return ourDie;
    }

    /// <summary>
    /// Cleanly destroys a die, disconnecting if necessary and triggering events in the process
    /// Does not remove it from the list though
    /// </sumary>
    void DestroyDie(PoolDie ourDie)
    {
        // Disconnect first
        CheckDisconnectDie(ourDie.die);

        // Trigger event
        onWillDestroyDie?.Invoke(ourDie.die);

        ourDie.setState.Invoke(Die.ConnectionState.Invalid);
        GameObject.Destroy(ourDie.die.gameObject);
        dice.Remove(ourDie);
    }

    /// <summary>
    /// Destroys all dice that fit a predicate, or all dice if predicate is null
    /// </sumary>
    void DestroyAll(System.Predicate<Die> predicate = null)
    {
        if (predicate == null)
        {
            predicate = (d) => true;
        }
        var diceCopy = new List<PoolDie>(dice);
        foreach (var die in diceCopy)
        {
            if (predicate(die.die))
            {
                DestroyDie(die);
            }
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
    /// Save our pool to JSON!
    /// </sumary>
    void UpdateDataSet()
    {
        // We only save the dice that we have indicated to be in the pool
        // (i.e. ignore dice that are 'new' and we didn't connect to)
        var toRemove = new List<Presets.EditDie>(AppDataSet.Instance.dice);
        foreach (var ourDie in dice.Where(d => d.die.connectionState != Die.ConnectionState.New))
        {
            var editDie = AppDataSet.Instance.FindDie(ourDie.die);
            if (editDie == null)
            {
                // Create a new die
                editDie = AppDataSet.Instance.AddNewDie(ourDie.die);
            }
            else
            {
                // Update die info
                editDie.name = ourDie.die.name;
                editDie.deviceId = ourDie.die.deviceId;
                editDie.faceCount = ourDie.die.faceCount;
                editDie.designAndColor = ourDie.die.designAndColor;
                toRemove.Remove(editDie);
            }
        }

        foreach (var editDie in toRemove)
        {
            AppDataSet.Instance.dice.Remove(editDie);
        }
    }

    /// <summary>
    /// Load our pool from JSON!
    /// </sumary>
    void Initialize()
    {
        // Clear and recreate the list of dice
        ClearPool();
        if (AppDataSet.Instance.dice != null)
        {
            foreach (var ddie in AppDataSet.Instance.dice)
            {
                // Create a disconnected die
                var ourDie = CreateDie(null, ddie.name, ddie.deviceId, ddie.faceCount, ddie.designAndColor);
                ourDie.setState.Invoke(Die.ConnectionState.Unknown);
            }
        }
    }
}
