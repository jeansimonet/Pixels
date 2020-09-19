﻿using System.Collections;
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

    void Awake()
    {
        addNewDiceButton.onClick.AddListener(AddNewDice);

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
        StopCoroutine(connectAllDiceCoroutine);
        ((System.IDisposable)connectAllDiceCoroutine).Dispose(); // This will make sure the
        connectAllDiceCoroutine = null;
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
        RefreshView();
    }

    void OnWillRemoveDie(EditDie editDie)
    {
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
        var allDiceCopy = new List<EditDie>();
        var connectedDice = new List<EditDie>();
        try
        {
            while (true)
            {
                OnBeginRefreshPool();
                allDiceCopy.Clear();
                allDiceCopy.AddRange(DiceManager.Instance.allDice);
                foreach (var editDie in allDiceCopy)
                {
                    if (editDie.die == null || editDie.die.connectionState != Die.ConnectionState.Ready)
                    {
                        // Try connecting to the die
                        bool connected = false;
                        connectedDice.Add(editDie);
                        yield return DiceManager.Instance.ConnectDie(editDie, (d, res, err) => connected = res);

                        if (connected)
                        {
                            // Fetch battery level
                            bool battLevelReceived = false;
                            editDie.die.GetBatteryLevel((d, f) => battLevelReceived = true);
                            yield return new WaitUntil(() => battLevelReceived == true);

                            // Fetch rssi
                            bool rssiReceived = false;
                            editDie.die.GetRssi((d, i) => rssiReceived = true);
                            yield return new WaitUntil(() => rssiReceived == true);
                        }
                        else
                        {
                            connectedDice.Remove(editDie);
                        }

                        RefreshView();
                    }
                }
                OnEndRefreshPool();

                yield return new WaitForSeconds(10.0f);
            }
        }
        finally
        {
            Debug.Log("Finally Called");
            foreach (var editDie in connectedDice)
            {
                DiceManager.Instance.DisconnectDie(editDie, null);
            }
            OnEndRefreshPool();
        }
    }

    void OnEndRefreshPool()
    {
        if (refreshButton.rotating)
        {
            refreshButton.StopRotating();
        }
    }
}