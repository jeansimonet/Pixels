using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Dice;

public class UIDicePoolView
    : UIPage
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

    IEnumerator connectAllDiceCoroutine;
    List<EditDie> connectedDice = new List<EditDie>();

    void Awake()
    {
        addNewDiceButton.onClick.AddListener(AddNewDice);
        refreshButton.onClick.AddListener(ForceRefresh);
    }

    public override void Enter(object context)
    {
        base.Enter(context);

        // Connect to all the dice in the pool if possible
        connectAllDiceCoroutine = ConnectAllDice();
        StartCoroutine(connectAllDiceCoroutine);
    }

    public override void Leave()
    {
        base.Leave();
        if (connectAllDiceCoroutine != null)
        {
            StopCoroutine(connectAllDiceCoroutine);
            ((System.IDisposable)connectAllDiceCoroutine).Dispose(); // This will make sure the
            connectAllDiceCoroutine = null;
        }
        foreach (var editDie in connectedDice)
        {
            DiceManager.Instance.DisconnectDie(editDie, null);
        }
        connectedDice.Clear();
    }

    void OnEnable()
    {
        base.SetupHeader(true, false, "Dice Bag", null);
        RefreshView();
        DiceManager.Instance.onDieAdded += OnDieAdded;
        DiceManager.Instance.onWillRemoveDie += OnWillRemoveDie;
    }

    void OnDisable()
    {
        if (DiceManager.Instance != null)
        {
            DiceManager.Instance.onDieAdded -= OnDieAdded;
            DiceManager.Instance.onWillRemoveDie -= OnWillRemoveDie;
        }
        foreach (var uidie in pairedDice)
        {
            DestroyPairedDie(uidie);
        }
        pairedDice.Clear();
    }

    UIPairedDieToken CreatePairedDie(EditDie die)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIPairedDieToken>(pairedDieViewPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);
        ret.transform.SetAsFirstSibling();
        // Initialize it
        ret.Setup(die);
        return ret;
    }

    void DestroyPairedDie(UIPairedDieToken die)
    {
        GameObject.Destroy(die.gameObject);
    }

    void AddNewDice()
    {
        NavigationManager.Instance.GoToPage(UIPage.PageId.DicePoolScanning, null);
    }


    void RefreshView()
    {
        // Assume all pool dice will be destroyed
        List<UIPairedDieToken> toDestroy = new List<UIPairedDieToken>(pairedDice);
        foreach (var die in DiceManager.Instance.allDice)
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

    void OnDieAdded(EditDie editDie)
    {
        // if (!connectedDice.Contains(editDie))
        // {
        //     connectedDice.Add(editDie);
        //     DiceManager.Instance.ConnectDie(editDie, null);
        // }
        RefreshView();
    }

    void OnWillRemoveDie(EditDie editDie)
    {
        connectedDice.Remove(editDie);
        var ui = pairedDice.FirstOrDefault(uid => uid.die == editDie);
        if (ui != null)
        {
            pairedDice.Remove(ui);
            DestroyPairedDie(ui);
        }
    }

    void OnBeginRefreshPool()
    {
        refreshButton.StartRotating();
    }

    IEnumerator ConnectAllDice()
    {
        DicePool.Instance.ResetDiceErrors();
        var allDiceCopy = new List<EditDie>();
        try
        {
            OnBeginRefreshPool();
            allDiceCopy.Clear();
            allDiceCopy.AddRange(DiceManager.Instance.allDice.Where(d => d.die == null || d.die.connectionState == Die.ConnectionState.Available));
            bool connected = false;
            DiceManager.Instance.ConnectDiceList(allDiceCopy, () => connected = true);
            yield return new WaitUntil(() => connected);
            foreach (var editDie in allDiceCopy)
            {
                if (editDie.die != null && editDie.die.connectionState == Die.ConnectionState.Ready)
                {
                    connectedDice.Add(editDie);

                    // Fetch battery level
                    bool battLevelReceived = false;
                    editDie.die.GetBatteryLevel((d, f) => battLevelReceived = true);
                    yield return new WaitUntil(() => battLevelReceived == true);

                    // Fetch rssi
                    bool rssiReceived = false;
                    editDie.die.GetRssi((d, i) => rssiReceived = true);
                    yield return new WaitUntil(() => rssiReceived == true);

                    RefreshView();
                }
            }
        }
        finally
        {
            OnEndRefreshPool();
        }
        connectAllDiceCoroutine = null;
    }

    void OnEndRefreshPool()
    {
        if (refreshButton.rotating)
        {
            refreshButton.StopRotating();
        }
    }

    void ForceRefresh()
    {
        if (connectAllDiceCoroutine == null)
        {
            // Connect to all the dice in the pool if possible
            connectAllDiceCoroutine = ConnectAllDice();
            StartCoroutine(connectAllDiceCoroutine);
        }
    }
}
