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
        public System.Action<Die.ConnectionState> setState;
    }
    List<PoolDie> dice = new List<PoolDie>();

    public IEnumerable<Die> allDice => dice.Select(d => d.die);

    class ConnectedDie
    {
        public Die die;
        public int count;
        public float lastRequestDisconnectTime;
    }
    List<ConnectedDie> connectedDice = new List<ConnectedDie>();

    // Multiple things may request bluetooth scanning, so we need to arbitrate when
    // we actually ask Central to scan or not. This counter will let us know
    // exactly when to start or stop asking central.
    int scanRequestCount = 0;

    /// <summary>
    /// Start scanning for new and existing dice, filling our lists in the process from
    /// events triggered by Central.
    /// </sumary>
    public void BeginScanForDice(DicePool.DieCreationEvent onScannedDie)
    {
        Debug.Log("request start scan");

        // In either case, register to be notified on new dice
        DicePool.Instance.onDieDiscovered += onScannedDie;

        scanRequestCount++;
        if (scanRequestCount == 1)
        {
            DoBeginScanForDice();
        }
        else
        {
            // Iterate over existing scanned dice
            foreach (var die in DicePool.Instance.allDice.Where(d => d.connectionState == Die.ConnectionState.Available || d.connectionState == Die.ConnectionState.CommError))
            {
                onScannedDie.Invoke(die);
            }
        }
    }

    /// <summary>
    /// Stops the current scan 
    /// </sumary>
    public void StopScanForDice(DicePool.DieCreationEvent onScannedDie)
    {
        Debug.Log("request stop scan");
        if (scanRequestCount == 0)
        {
            Debug.LogError("Pool not currently scanning");
        }
        else
        {
            DicePool.Instance.onDieDiscovered -= onScannedDie;
            scanRequestCount--;
            if (scanRequestCount == 0)
            {
                DoStopScanForDice();
            }
            // Else ignore
        }
    }

    public void ConnectDie(Die die)
    {
        var cd = connectedDice.FirstOrDefault(c => c.die == die);
        if (cd == null)
        {
            cd = new ConnectedDie() { die = die, count = 0, lastRequestDisconnectTime = 0.0f };
            connectedDice.Add(cd);
        }
        Debug.Log("Before request connect: " + cd.count);
        cd.count++;
        if (cd.count == 1)
        {
            // Bump the count once, we will use this to trigger the disconnect timer
            cd.count = 2;

            // Register to be notified if it failed
            DoConnectDie(die);
        }
        Debug.Log("After request connect: " + cd.count);
    }

    public void DisconnectDie(Die die)
    {
        var cd = connectedDice.FirstOrDefault(c => c.die == die);
        if (cd != null)
        {
            Debug.Log("Before request disconnect: " + cd.count);
            cd.count--;
            Debug.Assert(cd.count != 0);
            if (cd.count == 1)
            {
                // Now we set the timer and only disconnect after a while
                cd.lastRequestDisconnectTime = Time.time;
            }
            Debug.Log("After request disconnect: " + cd.count);
        }
        else
        {
            Debug.LogError("Atempting to disconnect die " + die.name + " that we didn't connect to ourselves");
        }
    }

    void Update()
    {
        for (int i = 0; i < connectedDice.Count; ++i)
        {
            var cd = connectedDice[i];
            if (cd.count == 1)
            {
                // This die is waiting to be disconnected
                if (Time.time > (cd.lastRequestDisconnectTime + AppConstants.Instance.DicePoolDisconnectDelay))
                {
                    Debug.Log("Update: 1 -> 0");

                    // Really disconnect
                    cd.count = 0;
                    DoDisconnectDie(cd.die);

                    // Remove from the array
                    connectedDice.RemoveAt(i);
                    i--;
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
        foreach (var die in dice)
        {
            if (die.die.connectionState == Die.ConnectionState.CommError)
            {
                die.setState(Die.ConnectionState.Available);
            }
        }
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
    void DoDisconnectDie(Die die)
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
        dt.setState(Die.ConnectionState.Removed);
        dice.Remove(dt);

        // Destroy it (this will cleanly disconnect if necessary)
        DestroyDie(dt);
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

            switch (ourDie.die.connectionState)
            {
                case Die.ConnectionState.Available:
                case Die.ConnectionState.CommError:
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
            ourDie.setState.Invoke(Die.ConnectionState.CommError);
        }
    }

    /// <summary>
    /// Creates a new die for the pool
    /// </sumary>
    PoolDie CreateDie(string address, string name, System.UInt64 deviceId = 0, int faceCount = 0, DesignAndColor design = DesignAndColor.Unknown)
    {
        var dieObj = new GameObject(name);
        dieObj.transform.SetParent(transform);
        Die die = dieObj.AddComponent<Die>();
        var setStateAction = die.Setup(name, address, deviceId, faceCount, design);
        var ourDie = new PoolDie()
        {
            die = die,
            setState = setStateAction,
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

}
