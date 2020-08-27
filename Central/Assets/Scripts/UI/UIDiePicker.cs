using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;
using System.Linq;
using Dice;

public class UIDiePicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public RectTransform contentRoot;

    [Header("Prefabs")]
    public UIDiePickerDieToken dieTokenPrefab;

    EditDie currentDie;
    System.Action<bool, EditDie> closeAction;

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
    System.Func<EditDie, Die, bool> dieSelector;

    /// <summary>
    /// Invoke the die picker
    /// </sumary>
    public void Show(string title, EditDie previousDie, System.Func<EditDie, Die, bool> selector, System.Action<bool, EditDie> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Die picker still active");
            ForceHide();
        }

        dieSelector = selector;
        
        foreach (var dt in DiceManager.Instance.allDice.Where(d => selector(d.editDie, d.die)))
        {
            // New pattern
            var newDieUI = CreateDieToken(dt);
            newDieUI.SetSelected(dt.editDie == previousDie);
            dice.Add(newDieUI);
        }

        gameObject.SetActive(true);
        currentDie = previousDie;
        titleText.text = title;

        this.closeAction = closeAction;
    }

    UIDiePickerDieToken CreateDieToken(DiceManager.ManagedDie die)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIDiePickerDieToken>(dieTokenPrefab, contentRoot.transform);

        // Initialize it
        ret.Setup(die);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => Hide(true, ret.die.editDie));

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

    void Hide(bool result, EditDie die)
    {
        foreach (var uidie in dice)
        {
            DestroyDieToken(uidie);
        }
        dice.Clear();

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
        GameObject.Destroy(token.gameObject);
    }

}
