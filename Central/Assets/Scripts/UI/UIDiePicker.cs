using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;
using System.Linq;

public class UIDiePicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public RectTransform contentRoot;

    [Header("Prefabs")]
    public UIDiePickerDieToken dieTokenPrefab;

    Die currentDie;
    System.Action<bool, Die> closeAction;

    // The list of controls we have created to display dice
    List<UIDiePickerDieToken> dice = new List<UIDiePickerDieToken>();

    // When refreshing the pool, this keeps track of the dice we *think* may not be there any more
    // but don't want to update the status of until *after* we've finished re-scanning for them.
    // That is simply for visual purposes, otherwise the status will flicker to "unknown" and it might
    // confise the user. So instead we tell these dice UIs to stop updating their status, and then tell
    // to return to normal after the refresh (updating at that time).
    List<UIDiePickerDieToken> doubtedDice = new List<UIDiePickerDieToken>();

    DicePoolRefresher poolRefresher;

    public bool isShown => gameObject.activeSelf;
    System.Func<Die, bool> dieSelector;

    /// <summary>
    /// Invoke the die picker
    /// </sumary>
    public void Show(string title, Die previousDie, System.Func<Die, bool> selector, System.Action<bool, Die> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Die picker still active");
            ForceHide();
        }

        dieSelector = selector;
        
        foreach (var die in DicePool.Instance.allDice.Where(DieSelector))
        {
            // New pattern
            var newDieUI = CreateDieToken(die);
            newDieUI.SetSelected(die == previousDie);
            dice.Add(newDieUI);
        }

        gameObject.SetActive(true);
        currentDie = previousDie;
        titleText.text = title;

        DicePool.Instance.onDieCreated += OnDieCreated;
        DicePool.Instance.onWillDestroyDie += OnWillDestroyDie;
        poolRefresher.onBeginRefreshPool += OnBeginRefreshPool;
        poolRefresher.onEndRefreshPool += OnEndRefreshPool;

        this.closeAction = closeAction;
    }

    bool DieSelector(Die d)
    {
        return dieSelector(d) && d.connectionState != Die.ConnectionState.Invalid && d.connectionState != Die.ConnectionState.New;
    }

    UIDiePickerDieToken CreateDieToken(Die die)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIDiePickerDieToken>(dieTokenPrefab, contentRoot.transform);

        // Initialize it
        ret.Setup(die);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => Hide(true, ret.die));
        die.OnConnectionStateChanged += OnDieConnectionStateChanged;

        return ret;
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false, currentDie);
    }

    void Awake()
    {
        backButton.onClick.AddListener(Back);
        poolRefresher = GetComponent<DicePoolRefresher>();
    }

    void Hide(bool result, Die die)
    {
        foreach (var uidie in dice)
        {
            DestroyDieToken(uidie);
        }
        dice.Clear();

        DicePool.Instance.onDieCreated -= OnDieCreated;
        DicePool.Instance.onWillDestroyDie -= OnWillDestroyDie;
        poolRefresher.onBeginRefreshPool -= OnBeginRefreshPool;
        poolRefresher.onEndRefreshPool -= OnEndRefreshPool;

        var closeActionCopy = closeAction;
        closeAction = null;

        gameObject.SetActive(false);
        closeActionCopy?.Invoke(result, die);
    }

    void Back()
    {
        Hide(false, currentDie);
    }

    void DestroyDieToken(UIDiePickerDieToken token)
    {
        token.die.OnConnectionStateChanged -= OnDieConnectionStateChanged;
        GameObject.Destroy(token.gameObject);
    }

    void OnDieCreated(Die newDie)
    {
        if (DieSelector(newDie))
        {
            var newUIDie = CreateDieToken(newDie);
            dice.Add(newUIDie);
            OnDieConnectionStateChanged(newDie, newDie.connectionState, newDie.connectionState);
        }
    }

    void OnWillDestroyDie(Die die)
    {
        var uidie = dice.Find(d => d.die == die);
        if (uidie != null)
        {
            dice.Remove(uidie);
            DestroyDieToken(uidie);
        }
    }

    void OnBeginRefreshPool()
    {
        Debug.Assert(doubtedDice.Count == 0);

        // Drop all the "available" and "missing" dice back down to unknown, so we can recheck if they are there
        foreach (var uidie in dice.Where(d => d.die.connectionState == Die.ConnectionState.Available || d.die.connectionState == Die.ConnectionState.Missing))
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
        foreach (var uidie in doubtedDice)
        {
            if (DicePool.Instance.allDice.Contains(uidie.die))
            {
                uidie.FinishRefreshPool();
            }
        }
        doubtedDice.Clear();
    }

    void OnDieConnectionStateChanged(Die die, Die.ConnectionState oldState, Die.ConnectionState newState)
    {
        var uidie = dice.FirstOrDefault(d => d.die == die);
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


}
