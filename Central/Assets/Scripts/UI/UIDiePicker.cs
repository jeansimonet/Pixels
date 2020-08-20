using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;

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

    public bool isShown => gameObject.activeSelf;

    /// <summary>
    /// Invoke the die picker
    /// </sumary>
    public void Show(string title, EditDie previousDie, System.Action<bool, EditDie> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Die picker still active");
            ForceHide();
        }

        foreach (var die in AppDataSet.Instance.dice)
        {
            // New pattern
            var newDieUI = CreateDieToken(die);
            dice.Add(newDieUI);
        }

        gameObject.SetActive(true);
        currentDie = previousDie;
        titleText.text = title;


        this.closeAction = closeAction;
    }

    UIDiePickerDieToken CreateDieToken(EditDie die)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIDiePickerDieToken>(dieTokenPrefab, contentRoot.transform);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => Hide(true, ret.die));

        // Initialize it
        ret.Setup(die);
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
    }

    void Hide(bool result, EditDie die)
    {
        foreach (var uidie in dice)
        {
            DestroyDieToken(uidie);
        }
        dice.Clear();

        gameObject.SetActive(false);
        closeAction?.Invoke(result, die);
        closeAction = null;
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
