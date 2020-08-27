using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Presets;
using System.Linq;
using Dice;

public class DiceManager : SingletonMonoBehaviour<DiceManager>
{
    public class ManagedDie
    {
        public ManagedDie(EditDie editDie)
        {
            this.editDie = editDie;
            this.die = null;
        }
        public delegate void DieFoundLostEvent(EditDie editDie, Die die);
        public DieFoundLostEvent onDieFound;
        public DieFoundLostEvent onDieWillBeLost;
        public EditDie editDie { get; private set; }
        public Die die
        {
            get { return _die; }
            set
            {
                if (_die != null)
                {
                    onDieWillBeLost?.Invoke(editDie, _die);
                }
                _die = value;
                if (_die != null)
                {
                    onDieFound?.Invoke(editDie, _die);
                }
            }
        }
        Die _die;
    }
    List<ManagedDie> dice = new List<ManagedDie>();

    public delegate void DieAddedRemovedEvent(EditDie editDie);
    public DieAddedRemovedEvent onDieAdded;
    public DieAddedRemovedEvent onWillRemoveDie;

    //Queue<Die> discoveredDieToCheckOut = new Queue<Die>();

    public IEnumerable<ManagedDie> allDice => dice;

    public Coroutine AddDiscoveredDice(List<Die> discoveredDice)
    {
        return StartCoroutine(AddDiscoveredDiceCr(discoveredDice));
    }

    IEnumerator AddDiscoveredDiceCr(List<Die> discoveredDice)
    {
        PixelsApp.Instance.ShowProgrammingBox("Adding Dice to the Dice Bag");
        for (int i = 0; i < discoveredDice.Count; ++i)
        {
            var die = discoveredDice[i];
            PixelsApp.Instance.UpdateProgrammingBox((float)(i+1) / discoveredDice.Count, "Adding " + die.name + " to the pool");

            // Here we wait a frame to give the programming box a chance to show up
            // on PC at least the attempt to connect can freeze the app
            yield return null;
            yield return null;

            if (die.deviceId != 0)
            {
                // Add a new entry in the dataset
                var editDie = AppDataSet.Instance.AddNewDie(die);

                // And in our map
                var ourDie = new ManagedDie(editDie);
                dice.Add(ourDie);
                onDieAdded?.Invoke(editDie);
                ourDie.die = die;
            }
            else
            {
                DicePool.Instance.ConnectDie(die);
                yield return new WaitUntil(() => die.connectionState == Die.ConnectionState.Ready || die.connectionState == Die.ConnectionState.CommError);
                if (die.connectionState == Die.ConnectionState.Ready)
                {
                    if (die.deviceId == 0)
                    {
                        Debug.LogError("Die " + die.name + " was connected to but doesn't have a proper device Id");
                        bool acknowledge = false;
                        PixelsApp.Instance.ShowDialogBox("Identification Error", "Die " + die.name + " was connected to but doesn't have a proper device Id", "Ok", null, (res) => acknowledge = true);
                        yield return new WaitUntil(() => acknowledge);
                    }
                    else
                    {
                        // Add a new entry in the dataset
                        var editDie = AppDataSet.Instance.AddNewDie(die);

                        // And in our map
                        var ourDie = new ManagedDie(editDie);
                        dice.Add(ourDie);
                        onDieAdded?.Invoke(editDie);
                        ourDie.die = die;
                    }
                }
                else
                {
                    bool acknowledge = false;
                    PixelsApp.Instance.ShowDialogBox("Connection error", "Could not connect to " + die.name + " to add it to the dice bag.", "Ok", null, (res) => acknowledge = true);
                    yield return new WaitUntil(() => acknowledge);
                }
                DicePool.Instance.DisconnectDie(die);
            }
        }
        PixelsApp.Instance.HideProgrammingBox();
    }

    public Coroutine ConnectDie(EditDie editDie, System.Action<EditDie, Die, string> dieReadyCallback)
    {
        var ourDie = dice.FirstOrDefault(d => d.editDie == editDie);
        if (ourDie == null)
        {
            Debug.LogError("Die " + editDie.name + " not in Dice Manager");
            dieReadyCallback?.Invoke(editDie, null, "Edit Die not in Dice Manager");
            return null;
        }
        else
        {
            return StartCoroutine(ConnectDieCr(ourDie, dieReadyCallback));
        }
    }

    IEnumerator ConnectDieCr(ManagedDie ourDie, System.Action<EditDie, Die, string> dieReadyCallback)
    {
        var editDie = ourDie.editDie;
        if (ourDie.die == null)
        {
            // We need to find the actual real die 
            // Maybe the dice pool has already scanned it?
            List<Die> discoveredDice = new List<Die>();
            void onDieDiscovered(Die newDie)
            {
                discoveredDice.Add(newDie);
            }

            DicePool.Instance.BeginScanForDice(onDieDiscovered);
            float startScanTime = Time.time;
            yield return new WaitUntil(() => Time.time > startScanTime + 5.0f || discoveredDice.Any(d => d.name == editDie.name || d.deviceId == editDie.deviceId));
            DicePool.Instance.StopScanForDice(onDieDiscovered);

            var die = discoveredDice.FirstOrDefault(d => d.name == editDie.name || d.deviceId == editDie.deviceId);
            if (die != null)
            {
                if (die.deviceId == 0)
                {
                    // Find out the device id of the die
                    // Connect to the die
                    DicePool.Instance.ConnectDie(die);
                    yield return new WaitUntil(() => die.connectionState == Die.ConnectionState.Ready || die.connectionState == Die.ConnectionState.CommError);
                    if (die.connectionState == Die.ConnectionState.Ready)
                    {
                        if (die.deviceId == editDie.deviceId)
                        {
                            ourDie.die = die;
                            dieReadyCallback?.Invoke(editDie, die, null);
                        }
                        else
                        {
                            // Wrong die
                            DicePool.Instance.DisconnectDie(die);
                            die = null;
                        }
                    }
                    else
                    {
                        // Couldn't connect, keep looking
                        DicePool.Instance.DisconnectDie(die);
                        die = null;
                    }
                }
                else if (die.deviceId != editDie.deviceId)
                {
                    // Wrong die
                    die = null;
                }
                else
                {
                    // We found the die, try to connect
                    DicePool.Instance.ConnectDie(die);
                    yield return new WaitUntil(() => die.connectionState == Die.ConnectionState.Ready || die.connectionState == Die.ConnectionState.CommError);
                    if (die.connectionState == Die.ConnectionState.Ready)
                    {
                        ourDie.die = die;
                        dieReadyCallback?.Invoke(editDie, die, null);
                    }
                    else
                    {
                        // Couldn't connect, error out
                        DicePool.Instance.DisconnectDie(die);
                        dieReadyCallback?.Invoke(editDie, null, "Could not connect to Die " + editDie.name + ". Communication Error");
                    }
                }
            }

            if (die == null)
            {
                // If we haven't found the die yet, keep looking
                discoveredDice.Clear();
                DicePool.Instance.BeginScanForDice(onDieDiscovered);
                startScanTime = Time.time;
                yield return new WaitUntil(() => Time.time > startScanTime + 5.0f || discoveredDice.Any(d => d.deviceId == editDie.deviceId));
                DicePool.Instance.StopScanForDice(onDieDiscovered);

                die = discoveredDice.FirstOrDefault(d => d.deviceId == editDie.deviceId);
                if (die != null)
                {
                    // We found it, try to connect
                    DicePool.Instance.ConnectDie(die);
                    yield return new WaitUntil(() => die.connectionState == Die.ConnectionState.Ready || die.connectionState == Die.ConnectionState.CommError);
                    if (die.connectionState == Die.ConnectionState.Ready)
                    {
                        ourDie.die = die;
                        dieReadyCallback?.Invoke(editDie, die, null);
                    }
                    else
                    {
                        // Couldn't connect, error out
                        DicePool.Instance.DisconnectDie(die);
                        dieReadyCallback?.Invoke(editDie, null, "Could not connect to Die " + editDie.name + ". Communication Error");
                    }
                }
                else
                {
                    // Worst case, try to connect to all the dice until we find the right one
                    foreach (var d in discoveredDice)
                    {
                        DicePool.Instance.ConnectDie(d);
                        yield return new WaitUntil(() => d.connectionState == Die.ConnectionState.Ready || d.connectionState == Die.ConnectionState.CommError);
                        if (d.connectionState == Die.ConnectionState.Ready)
                        {
                            if (die.deviceId == editDie.deviceId)
                            {
                                // We finally found it
                                die = d;
                                ourDie.die = d;
                                dieReadyCallback?.Invoke(editDie, d, null);
                                break;
                            }
                            else
                            {
                                // Wrong die
                                DicePool.Instance.DisconnectDie(die);
                                DicePool.Instance.DisconnectDie(d);
                            }
                        }
                        else
                        {
                            // Else try the next one
                            DicePool.Instance.DisconnectDie(die);
                        }
                    }

                    if (die == null)
                    {
                        // Looked through all the discovered dice and didn't find the right one
                        dieReadyCallback?.Invoke(editDie, null, "Could not find die " + editDie.name + ". Make sure it is charged and in range");
                    }
                }
            }
            // Else we found it
        }
        else
        {
            // We already know what die matches the edit die, connect to it!
            DicePool.Instance.ConnectDie(ourDie.die);
            yield return new WaitUntil(() => ourDie.die.connectionState == Die.ConnectionState.Ready || ourDie.die.connectionState == Die.ConnectionState.CommError);
            if (ourDie.die.connectionState == Die.ConnectionState.Ready)
            {
                dieReadyCallback?.Invoke(editDie, ourDie.die, null);
            }
            else
            {
                // Couldn't connect, error out
                DicePool.Instance.DisconnectDie(ourDie.die);
                dieReadyCallback?.Invoke(editDie, null, "Could not connect to Die " + editDie.name + ". Communication Error");
            }
        }
    }

    public void DisconnectDie(EditDie editDie)
    {
        var dt = dice.First(p => p.editDie == editDie);
        if (dt == null)
        {
            Debug.LogError("Trying to disconnect unknown edit die " + editDie.name);
        }
        else if (dt.die == null)
        {
            Debug.LogError("Trying to disconnect unknown die " + editDie.name);
        }
        else if (dt.die.connectionState != Die.ConnectionState.Ready)
        {
            Debug.LogError("Trying to disconnect die that isn't connected " + editDie.name + ", current state " + dt.die.connectionState);
        }
        else
        {
            DicePool.Instance.DisconnectDie(dt.die);
        }
    }

    public void ForgetDie(EditDie editDie)
    {
        var dt = dice.First(p => p.editDie == editDie);
        if (dt == null)
        {
            Debug.LogError("Trying to forget unknown edit die " + editDie.name);
        }
        else
        {
            onWillRemoveDie?.Invoke(editDie);
            if (dt.die != null)
            {
                DicePool.Instance.ForgetDie(dt.die);
            }
            AppDataSet.Instance.dice.Remove(editDie);
            dice.Remove(dt);
        }
    }
    
    /// <summary>
    /// Save our pool to JSON!
    /// </sumary>
    void UpdateDataSet()
    {
        // We only save the dice that we have indicated to be in the pool
        // (i.e. ignore dice that are 'new' and we didn't connect to)
        AppDataSet.Instance.dice.Clear();
        foreach (var ourDie in dice)
        {
            if (ourDie.die != null)
            {
                // Update data in case it changed
                ourDie.editDie.name = ourDie.die.name;
                ourDie.editDie.deviceId = ourDie.die.deviceId;
                ourDie.editDie.faceCount = ourDie.die.faceCount;
                ourDie.editDie.designAndColor = ourDie.die.designAndColor;
            }
            AppDataSet.Instance.dice.Add(ourDie.editDie);
        }
    }

    void Awake()
    {
        DicePool.Instance.onDieDiscovered += OnDieDiscovered;
        DicePool.Instance.onWillDestroyDie += OnWillDestroyDie;
    }

    void Start()
    {
        Initialize();

        // while (true)
        // {
        //     if (discoveredDieToCheckOut.Count > 0)
        //     {
        //         var die = discoveredDieToCheckOut.Dequeue();
        //         DicePool.Instance.ConnectDie(die);
        //         yield return new WaitUntil(() => die.connectionState == Die.ConnectionState.Ready || die.connectionState == Die.ConnectionState.CommError);
        //         if (die.connectionState == Die.ConnectionState.Ready)
        //         {
        //             // Is this one of our dice?
        //             var ourDie = dice.FirstOrDefault(d => d.editDie.deviceId == die.deviceId);
        //             if (ourDie != null)
        //             {
        //                 // Yes, set the die
        //                 ourDie.die = die;
        //             }

        //             // Else ope, it's a new die we don't care about
        //             DicePool.Instance.DisconnectDie(die);
        //         }
        //     }

        //     yield return new WaitForSeconds(0.1f);
        // }
    }

    /// <summary>
    /// Load our pool from JSON!
    /// </sumary>
    void Initialize()
    {
        // Clear and recreate the list of dice
        if (AppDataSet.Instance.dice != null)
        {
            foreach (var ddie in AppDataSet.Instance.dice)
            {
                // Create a disconnected die
                var ourDie = new ManagedDie(ddie);
                onDieAdded?.Invoke(ddie);
                dice.Add(ourDie);
            }
        }
    }

    void OnDieDiscovered(Die die)
    {
        if (die.deviceId != 0)
        {
            var ourDie = dice.FirstOrDefault(d => d.editDie.deviceId == die.deviceId);
            if (ourDie != null)
            {
                ourDie.die = die;
            }
            // Else this is a die we don't care about
        }
        // else
        // {
        //     // This *may* be a die we care about, add it to the list of dice we should check out
        //     discoveredDieToCheckOut.Enqueue(die);
        // }
    }

    void OnWillDestroyDie(Die die)
    {
        var ourDie = dice.FirstOrDefault(d => d.die == die);
        if (ourDie != null)
        {
            ourDie.die = null;
        }
    }

}
