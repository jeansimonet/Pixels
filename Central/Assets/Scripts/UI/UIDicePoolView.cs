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
    public UIPairedDieView pairedDieViewPrefab;
    enum State
    {
        Disabled = 0,
        Idle,
        RefreshingPool,
    }

    State state = State.Disabled;
    float endTime;

    // The list of controls we have created to display die status
    List<UIPairedDieView> pairedDice = new List<UIPairedDieView>();

    // When refreshing the pool, this keeps track of the dice we *think* may not be there any more
    // but don't want to update the status of until *after* we've finished re-scanning for them.
    // That is simply for visual purposes, otherwise the status will flicker to "unknown" and it might
    // confise the user. So instead we tell these dice UIs to stop updating their status, and then tell
    // to return to normal after the refresh (updating at that time).
    List<UIPairedDieView> doubtedDice = new List<UIPairedDieView>();


    void Awake()
    {
        refreshButton.onClick.AddListener(ManualRefreshPool);
        addNewDiceButton.onClick.AddListener(AddNewDice);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.RefreshingPool:
                // Check if we should stop scanning
                if (!DicePool.Instance.allDice.Any(d => d.connectionState == Die.ConnectionState.Unknown) || Time.time >= endTime)
                {
                    // Done!
                    FinishRefreshPool();
                    BeginIdle(AppConstants.Instance.DicePoolViewScanDelay);
                }
                // Else continue waiting
                break;
            case State.Idle:
                if (Time.time >= endTime)
                {
                    BeginRefreshPool();
                }
                break;
            default:
                break;
        }
    }

    void OnEnable()
    {
        Debug.Assert(state == State.Disabled);
        RefreshView();
        DicePool.Instance.onDieCreated += OnDieCreated;
        DicePool.Instance.onWillDestroyDie += OnWillDestroyDie;
        BeginIdle(AppConstants.Instance.DicePoolViewFirstScanDelay);
    }

    void OnDisable()
    {
        if (DicePool.Instance != null) // When quiting the app, it may be null
        {
            switch (state)
            {
                case State.RefreshingPool:
                    // Done!
                    FinishRefreshPool();
                    break;
                default:
                    break;
            }
            DicePool.Instance.onDieCreated -= OnDieCreated;
            DicePool.Instance.onWillDestroyDie -= OnWillDestroyDie;
            foreach (var uidie in pairedDice)
            {
                DestroyPairedDie(uidie);
            }
            pairedDice.Clear();
            state = State.Disabled;
        }
    }

    UIPairedDieView CreatePairedDie(Die die)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIPairedDieView>(pairedDieViewPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);
        ret.transform.SetAsFirstSibling();
        // Initialize it
        ret.Setup(die);
        return ret;
    }

    void DestroyPairedDie(UIPairedDieView die)
    {
        GameObject.Destroy(die.gameObject);
    }

    void AddNewDice()
    {
        NavigationManager.Instance.GoToPage(PixelsApp.PageId.DicePoolScanning, null);
    }

    void BeginIdle(float delay)
    {
        state = State.Idle;
        endTime = Time.time + delay;
    }

    void ManualRefreshPool()
    {
        if (state == State.Idle)
        {
            BeginRefreshPool();
        }
    }

    void BeginRefreshPool()
    {
        Debug.Assert(state == State.Idle);
        Debug.Assert(doubtedDice.Count == 0);
        // Drop all the "available" and "missing" dice back down to unknown, so we can
        foreach (var uidie in pairedDice.Where(d => d.die.connectionState == Die.ConnectionState.Available || d.die.connectionState == Die.ConnectionState.Missing))
        {
            // Tell the die view that we are 'refreshing the pool'
            // so that it can pause updating the state of the ui until we're done
            // Otherwise it looks like we are indeed temporarily 'loosing' a die
            doubtedDice.Add(uidie);
            uidie.BeginRefreshPool();
            DicePool.Instance.DoubtDie(uidie.die);
        }

        // Did we have any dice that needed to be rechecked?
        if (DicePool.Instance.allDice.Any(d => d.connectionState == Die.ConnectionState.Unknown))
        {
            float startTime = Time.time;
            endTime = startTime + AppConstants.Instance.ScanTimeout;
            DicePool.Instance.RequestBeginScanForDice();
            state = State.RefreshingPool;
            refreshButton.onClick.RemoveListener(ManualRefreshPool);
            refreshButton.StartRotating();
        }
    }

    void FinishRefreshPool()
    {
        refreshButton.StopRotating();
        refreshButton.onClick.AddListener(ManualRefreshPool);
        DicePool.Instance.RequestStopScanForDice();
        foreach (var uidie in doubtedDice)
        {
            uidie.FinishRefreshPool();
        }
        doubtedDice.Clear();
    }

    void RefreshView()
    {
        // Assume all pool dice will be destroyed
        List<UIPairedDieView> toDestroy = new List<UIPairedDieView>(pairedDice);
        foreach (var die in DicePool.Instance.allDice)
        {
            int prevIndex = toDestroy.FindIndex(uid => uid.die == die);
            if (prevIndex == -1)
            {
                // New scanned die
                var newUIDie = CreatePairedDie(die);
                pairedDice.Add(newUIDie);
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
    }

    void OnWillDestroyDie(Die die)
    {
        var uidie = pairedDice.Find(d => d.die == die);
        pairedDice.Remove(uidie);
        DestroyPairedDie(uidie);
    }

}
