using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Dice;

public class DicePool : SingletonMonoBehaviour<DicePool>
{
    public delegate void BluetoothErrorEvent(string errorString);
    public delegate void DieCreationEvent(Die die);

    // A bunch of events for UI to hook onto and display pool state updates
    public DieCreationEvent onDieDiscovered;
    public DieCreationEvent onWillDestroyDie;
    public BluetoothErrorEvent onBluetoothError;

    class PoolDie
    {
        public Die die;
        public Central.IDie centralDie;
        public System.Action<Die.ConnectionState> setState;
        public System.Action<Die.LastError> setError;
        public System.Action<Die, bool, string> onConnectionResult;
        public System.Action<Die, bool, string> onDisconnectionResult;

        public int currentConnectionCount = 0;
        public float lastRequestDisconnectTime = 0.0f;
    }
    List<PoolDie> dice = new List<PoolDie>();

    public IEnumerable<Die> allDice => dice.Select(d => d.die);

    // Multiple things may request bluetooth scanning, so we need to arbitrate when
    // we actually ask Central to scan or not. This counter will let us know
    // exactly when to start or stop asking central.
    int scanRequestCount = 0;

    public void ResetDiceErrors()
    {
        foreach (var die in dice)
        {
            die.setError(Die.LastError.None);
        }
    }
    
    /// <summary>
    /// Start scanning for new and existing dice, filling our lists in the process from
    /// events triggered by Central.
    /// </sumary>
    public void BeginScanForDice()
    {
        scanRequestCount++;
        if (scanRequestCount == 1)
        {
            DoBeginScanForDice();
        }
    }

    /// <summary>
    /// Stops the current scan 
    /// </sumary>
    public void StopScanForDice()
    {
        if (scanRequestCount == 0)
        {
            Debug.LogError("Pool not currently scanning");
        }
        else
        {
            scanRequestCount--;
            if (scanRequestCount == 0)
            {
                DoStopScanForDice();
            }
            // Else ignore
        }
    }

    public void ClearScanList()
    {
        var diceCopy = new List<PoolDie>(dice);
        foreach (var die in diceCopy)
        {
            if (die.die != null && die.die.connectionState == Die.ConnectionState.Available)
            {
                DestroyDie(die);
            }
        }
        Central.Instance.ClearScanList();
    }

    public void ConnectDie(Die die, System.Action<Die, bool, string> onConnectionResult)
    {
        var poolDie = dice.FirstOrDefault(d => d.die == die);
        if (poolDie != null)
        {
            Debug.Log(poolDie.die.name + ": Before request connect: " + poolDie.currentConnectionCount);
            if (poolDie.currentConnectionCount == 0)
            {
                poolDie.currentConnectionCount += 1;
            }
            else
            {
                // Keep dice connected unless specifically asked to disconnect in the pool
                // This is a bit of a hack to prevent communications errors and make the connected/disconnected
                // state work more like users expect.
                poolDie.currentConnectionCount += 2;
            }
            switch (poolDie.die.connectionState)
            {
                default:
                    string errorMessage = "Die " + die.name + " in invalid die state " + poolDie.die.connectionState + " while attempting to connect";
                    Debug.LogError(errorMessage);
                    onConnectionResult?.Invoke(die, false, errorMessage);
                    break;
                case Die.ConnectionState.Available:
                    Debug.Assert(poolDie.currentConnectionCount == 1);
                    poolDie.onConnectionResult += onConnectionResult;
                    DoConnectDie(die);
                    break;
                case Die.ConnectionState.Connecting:
                case Die.ConnectionState.Identifying:
                    // Already in the process of connecting, just add the callback and wait
                    poolDie.onConnectionResult += onConnectionResult;
                    break;
                case Die.ConnectionState.Ready:
                    // Trigger the callback immediately
                    onConnectionResult?.Invoke(die, true, null);
                    break;
            }
            Debug.Log(poolDie.die.name + ": After request connect: " + poolDie.currentConnectionCount);
        }
        else
        {
            string errorMessage = "Pool atempting to connect to unknown die " + die.name;
            Debug.LogError(errorMessage);
            onConnectionResult?.Invoke(die, false, errorMessage);
        }
    }

    public void DisconnectDie(Die die, System.Action<Die, bool, string> onDisconnectionResult)
    {
        string errorMessage = null;
        var poolDie = dice.FirstOrDefault(d => d.die == die);
        if (poolDie != null)
        {
            Debug.Log(poolDie.die.name + ": Before request disconnect: " + poolDie.currentConnectionCount);
            switch (poolDie.die.connectionState)
            {
                default:
                    {
                        errorMessage = "Die " + die.name + " in invalid die state " + poolDie.die.connectionState + " while attempting to disconnect";
                        Debug.LogError(errorMessage);
                        onDisconnectionResult?.Invoke(die, false, errorMessage);
                    }
                    break;
                case Die.ConnectionState.Ready:
                case Die.ConnectionState.Connecting:
                case Die.ConnectionState.Identifying:
                    // Register to be notified when disconnection is complete
                    poolDie.currentConnectionCount--;
                    if (poolDie.currentConnectionCount == 0)
                    {
                        poolDie.onDisconnectionResult += onDisconnectionResult;
                        poolDie.lastRequestDisconnectTime = Time.time;
                    }
                    break;
            }
            Debug.Log(poolDie.die.name + ": After request disconnect: " + poolDie.currentConnectionCount);
        }
        else
        {
            errorMessage = "Pool atempting to disconnect to unknown die " + die.name;
            Debug.LogError(errorMessage);
            onDisconnectionResult?.Invoke(die, false, errorMessage);
        }
    }

    /// <summary>
    /// Removes a die from the pool, as if we never new it.
    /// Note: We may very well 'discover' it again the next time we scan.
    /// </sumary>
    public void ForgetDie(Die die, System.Action<Die, bool, string> onForgetDieResult)
    {
        var poolDie = dice.FirstOrDefault(d => d.die == die);
        if (poolDie != null)
        {
            switch (poolDie.die.connectionState)
            {
                default:
                    DestroyDie(poolDie);
                    onForgetDieResult?.Invoke(die, true, null);
                    break;
                case Die.ConnectionState.Ready:
                case Die.ConnectionState.Connecting:
                case Die.ConnectionState.Identifying:
                    // Disconnect!
                    DisconnectDie(die, (d, r, s) =>
                    {
                        DestroyDie(poolDie);
                        onForgetDieResult?.Invoke(d, r, s);
                    });
                    break;
            }
        }
        else
        {
            string errorMessage = "Pool atempting to forget unknown die " + die.name;
            Debug.LogError(errorMessage);
            onForgetDieResult?.Invoke(die, false, errorMessage);
        }
    }

    /// <summary>
    /// Write some data to the die
    /// </sumary>
    public void WriteDie(Die die, byte[] bytes, int length, System.Action<Die, bool, string> onWriteResult)
    {
        var dt = dice.First(p => p.die == die);
        Central.Instance.WriteDie(dt.centralDie, bytes, length, (d, res, errorMsg) =>
        {
            onWriteResult?.Invoke(die, res, errorMsg);
        });
    }

    void Awake()
    {
        Central.Instance.onBluetoothError += OnBluetoothError;
    }

    void Update()
    {
        foreach (var poolDie in dice)
        {
            if (poolDie.die != null)
            {
                switch (poolDie.die.connectionState)
                {
                    case Die.ConnectionState.Ready:
                        if (poolDie.currentConnectionCount == 0)
                        {
                            // Die is waiting to disconnect
                            if (Time.time - poolDie.lastRequestDisconnectTime > AppConstants.Instance.DicePoolDisconnectDelay)
                            {
                                // Go ahead and disconnect
                                DoDisconnectDie(poolDie.die);
                            }
                        }
                        break;
                    default:
                        // Do nothing
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Start scanning for new and existing dice, filling our lists in the process from
    /// events triggered by Central.
    /// </sumary>
    void DoBeginScanForDice()
    {
        Central.Instance.BeginScanForDice(OnDieDiscovered, OnDieAdvertisingData);
    }

    /// <summary>
    /// Stops the current scan 
    /// </sumary>
    void DoStopScanForDice()
    {
        Central.Instance.StopScanForDice();
    }

    void DoConnectDie(Die die)
    {
        if (die.connectionState == Die.ConnectionState.Available)
        {
            var dt = dice.First(p => p.die == die);
            dt.setState.Invoke(Die.ConnectionState.Connecting);
            Central.Instance.ConnectDie(dt.centralDie, OnDieConnected, OnDieData, OnDieDisconnectedUnexpectedly);
        }
        else
        {
            Debug.LogError("Die " + die.name + " not in avaiable state, instead: " + die.connectionState);
        }
    }

    /// <summary>
    /// Disconnects a die, doesn't remove it from the pool though
    /// </sumary>
    void DoDisconnectDie(Die die)
    {
        if (die.connectionState == Die.ConnectionState.Ready)
        {
            var dt = dice.FirstOrDefault(p => p.die == die); // When disconnecting from a destroy, the die will have already been removed
            dt.setState.Invoke(Die.ConnectionState.Disconnecting);
            Central.Instance.DisconnectDie(dt.centralDie, OnDieDisconnected);
        }
        else
        {
            Debug.LogError("Die " + die.name + " not in ready state, instead: " + die.connectionState);
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
            ourDie = CreateDie(die);
            ourDie.setState.Invoke(Die.ConnectionState.Available);
            onDieDiscovered?.Invoke(ourDie.die);
        }
        else
        {
            // Update die address if necessary
            if (string.IsNullOrEmpty(ourDie.die.address))
            {
                ourDie.die.UpdateAddress(die.address);
            }

            onDieDiscovered?.Invoke(ourDie.die);

            if (ourDie.die.connectionState != Die.ConnectionState.Available)
            {
                // All other are errors
                Debug.LogError("Die " + ourDie.die.name + " in invalid state " + ourDie.die.connectionState);
                ourDie.setState(Die.ConnectionState.Available);
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
                onDieDiscovered?.Invoke(ourDie.die);
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
            if (result)
            {
                // Remember that the die was just connected to (and trigger events)
                ourDie.setState.Invoke(Die.ConnectionState.Identifying);

                // And have it update its info (unique Id, appearance, etc...) so it can finally be ready
                ourDie.die.UpdateInfo(OnDieReady);

                // Reset error
                ourDie.setError(Die.LastError.None);
            }
            else
            {
                // Remember that the die was just connected to (and trigger events)
                ourDie.setState.Invoke(Die.ConnectionState.Available);

                // Could not connect to the die, indicate that!
                ourDie.setError(Die.LastError.ConnectionError);

                // Reset connection count since it didn't succeed
                ourDie.currentConnectionCount = 0;

                // Trigger callback
                var callbackCopy = ourDie.onConnectionResult;
                ourDie.onConnectionResult = null;
                callbackCopy?.Invoke(ourDie.die, false, errorMessage);
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
            }
            else
            {
                // Updating info didn't work, disconnect the die
                DoDisconnectDie(die);
            }

            // Trigger callback either way
            var callbackCopy = ourDie.onConnectionResult;
            ourDie.onConnectionResult = null;
            callbackCopy?.Invoke(ourDie.die, ready, null);
        }
        else
        {
            Debug.LogError("Received Die ready notification for unknown die " + die.name);
        }
    }

    void OnDieDisconnected(Central.IDie die, bool result, string errorMessage)
    {
        var ourDie = dice.Find(d => d.die.address == die.address);
        if (ourDie != null)
        {
            if (result)
            {
                ourDie.setState.Invoke(Die.ConnectionState.Available);

                // Reset connection count now that nothing is connected to the die
                ourDie.currentConnectionCount = 0;
            }
            else
            {
                // Could not disconnect the die, indicate that!
                ourDie.setError.Invoke(Die.LastError.ConnectionError);
            }
            var callbackCopy = ourDie.onDisconnectionResult;
            ourDie.onDisconnectionResult = null;
            callbackCopy?.Invoke(ourDie.die, result, errorMessage);
        }
        else
        {
            Debug.LogError("Received Die disconnected notification for unknown die " + die.name);
        }
    }

    /// <summary>
    /// Called by Central when a die gets disconnected unexpectedly
    /// </sumary>
    void OnDieDisconnectedUnexpectedly(Central.IDie die, string errorMessage)
    {
        Debug.LogError(die.name  + ": Die disconnected");
        var ourDie = dice.FirstOrDefault(d => d.die.address == die.address);
        if (ourDie != null)
        {
            ourDie.setState.Invoke(Die.ConnectionState.Available);
            ourDie.setError.Invoke(Die.LastError.Disconnected);

            // Reset connection count since it didn't succeed
            ourDie.currentConnectionCount = 0;

            // Forget the die for now
            DestroyDie(ourDie);
        }
    }

    /// <summary>
    /// Creates a new die for the pool
    /// </sumary>
    PoolDie CreateDie(Central.IDie centralDie, uint deviceId = 0, int faceCount = 0, DesignAndColor design = DesignAndColor.Unknown)
    {
        var dieObj = new GameObject(name);
        dieObj.transform.SetParent(transform);
        Die die = dieObj.AddComponent<Die>();
        System.Action<Die.ConnectionState> setStateAction;
        System.Action<Die.LastError> setLastErrorAction;
        die.Setup(centralDie.name, centralDie.address, deviceId, faceCount, design, out setStateAction, out setLastErrorAction);
        var ourDie = new PoolDie()
        {
            die = die,
            centralDie = centralDie,
            setState = setStateAction,
            setError = setLastErrorAction,
        };
        dice.Add(ourDie);

        return ourDie;
    }

    /// <summary>
    /// Cleanly destroys a die, disconnecting if necessary and triggering events in the process
    /// Does not remove it from the list though
    /// </sumary>
    void DestroyDie(PoolDie ourDie)
    {
        void doDestroy()
        {
            // Trigger event
            onWillDestroyDie?.Invoke(ourDie.die);

            ourDie.setState.Invoke(Die.ConnectionState.Invalid);
            GameObject.Destroy(ourDie.die.gameObject);
            dice.Remove(ourDie);
        }

        switch (ourDie.die.connectionState)
        {
            default:
                doDestroy();
                break;
            case Die.ConnectionState.Ready:
                // Register to be notified when disconnection is complete
                if (ourDie.currentConnectionCount == 0)
                {
                    ourDie.onDisconnectionResult += (d, r, s) => doDestroy();
                    DoDisconnectDie(ourDie.die);
                }
                break;
        }
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

}
