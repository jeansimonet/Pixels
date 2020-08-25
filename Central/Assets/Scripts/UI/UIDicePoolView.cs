using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class UIDicePoolView
    : PixelsApp.Page
{
    [Header("Controls")]
    public GameObject contentRoot;
    public Button addNewDiceButton;
    public UIDicePoolRefreshButton refreshButton;

    [Header("Prefabs")]
    public UIPairedDieToken pairedDieViewPrefab;

    // The list of controls we have created to display die status
    List<UIPairedDieToken> pairedDice = new List<UIPairedDieToken>();

    // When refreshing the pool, this keeps track of the dice we *think* may not be there any more
    // but don't want to update the status of until *after* we've finished re-scanning for them.
    // That is simply for visual purposes, otherwise the status will flicker to "unknown" and it might
    // confise the user. So instead we tell these dice UIs to stop updating their status, and then tell
    // to return to normal after the refresh (updating at that time).
    List<UIPairedDieToken> doubtedDice = new List<UIPairedDieToken>();

    DicePoolRefresher poolRefresher;

    void Awake()
    {
        refreshButton.onClick.AddListener(ManualRefreshPool);
        addNewDiceButton.onClick.AddListener(AddNewDice);
        poolRefresher = GetComponent<DicePoolRefresher>();
        poolRefresher.onBeginRefreshPool += OnBeginRefreshPool;
        poolRefresher.onEndRefreshPool += OnEndRefreshPool;
    }

    void OnEnable()
    {
        RefreshView();
        DicePool.Instance.onDieCreated += OnDieCreated;
        DicePool.Instance.onWillDestroyDie += OnWillDestroyDie;
    }

    void OnDisable()
    {
        if (DicePool.Instance != null) // When quiting the app, it may be null
        {
            DicePool.Instance.onDieCreated -= OnDieCreated;
            DicePool.Instance.onWillDestroyDie -= OnWillDestroyDie;
            foreach (var uidie in pairedDice)
            {
                DestroyPairedDie(uidie);
            }
            pairedDice.Clear();
        }
    }

    UIPairedDieToken CreatePairedDie(Die die)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIPairedDieToken>(pairedDieViewPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);
        ret.transform.SetAsFirstSibling();
        // Initialize it
        ret.Setup(die);
        die.OnConnectionStateChanged += OnDieConnectionStateChanged;
        return ret;
    }

    void DestroyPairedDie(UIPairedDieToken die)
    {
        die.die.OnConnectionStateChanged -= OnDieConnectionStateChanged;
        GameObject.Destroy(die.gameObject);
    }

    void AddNewDice()
    {
        NavigationManager.Instance.GoToPage(PixelsApp.PageId.DicePoolScanning, null);
    }


    void OnDieConnectionStateChanged(Die die, Die.ConnectionState oldState, Die.ConnectionState newState)
    {
        var uidie = pairedDice.FirstOrDefault(d => d.die == die);
        switch (newState)
        {
            case Die.ConnectionState.Invalid:
            case Die.ConnectionState.New:
                uidie?.gameObject.SetActive(false);
                break;
            default:
                uidie?.gameObject.SetActive(true);
                break;
        }
        // Else the page was disabled underneath us, stop
    }

    void RefreshView()
    {
        // Assume all pool dice will be destroyed
        List<UIPairedDieToken> toDestroy = new List<UIPairedDieToken>(pairedDice);
        foreach (var die in DicePool.Instance.allDice)
        {
            int prevIndex = toDestroy.FindIndex(uid => uid.die == die);
            if (prevIndex == -1)
            {
                // New scanned die
                var newUIDie = CreatePairedDie(die);
                pairedDice.Add(newUIDie);
                OnDieConnectionStateChanged(die, die.connectionState, die.connectionState);
            }
            else
            {
                // Previous die is still advertising, good
                toDestroy.RemoveAt(prevIndex);
            }
        }

        // Remove all remaining dice
        foreach (var uidie in toDestroy)
        {
            pairedDice.Remove(uidie);
            DestroyPairedDie(uidie);
        }
    }

    void OnDieCreated(Die newDie)
    {
        var newUIDie = CreatePairedDie(newDie);
        pairedDice.Add(newUIDie);
        OnDieConnectionStateChanged(newDie, newDie.connectionState, newDie.connectionState);
    }

    void OnWillDestroyDie(Die die)
    {
        var uidie = pairedDice.Find(d => d.die == die);
        pairedDice.Remove(uidie);
        DestroyPairedDie(uidie);
    }

    void ManualRefreshPool()
    {
        //poolRefresher.BeginRefreshPool();
    }

    void OnBeginRefreshPool()
    {
        Debug.Assert(doubtedDice.Count == 0);
        refreshButton.onClick.RemoveListener(ManualRefreshPool);
        refreshButton.StartRotating();

        // Drop all the "available" and "missing" dice back down to unknown, so we can recheck if they are there
        foreach (var uidie in pairedDice.Where(d => d.die.connectionState == Die.ConnectionState.Available || d.die.connectionState == Die.ConnectionState.Missing))
        {
            // Tell the die view that we are 'refreshing the pool'
            // so that it can pause updating the state of the ui until we're done
            // Otherwise it looks like we are indeed temporarily 'loosing' a die
            doubtedDice.Add(uidie);
            uidie.BeginRefreshPool();
            DicePool.Instance.DoubtDie(uidie.die);
        }

    }

    void OnEndRefreshPool()
    {
        refreshButton.StopRotating();
        refreshButton.onClick.AddListener(ManualRefreshPool);

        foreach (var uidie in doubtedDice)
        {
            if (DicePool.Instance.allDice.Contains(uidie.die))
            {
                uidie.FinishRefreshPool();
            }
        }
        doubtedDice.Clear();
    }
}
