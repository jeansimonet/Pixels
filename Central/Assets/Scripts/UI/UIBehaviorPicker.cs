using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Behaviors;

public class UIBehaviorPicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public RectTransform contentRoot;

    [Header("Prefabs")]
    public UIBehaviorToken behaviorTokenPrefab;

    EditBehavior currentBehavior;
    System.Action<bool, EditBehavior> closeAction;

    // The list of controls we have created to display dice
    List<UIBehaviorToken> behaviors = new List<UIBehaviorToken>();

    public bool isShown => gameObject.activeSelf;

    /// <summary>
    /// Invoke the die picker
    /// </sumary>
    public void Show(string title, EditBehavior previousBehavior, System.Action<bool, EditBehavior> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Behavior picker still active");
            ForceHide();
        }

        foreach (var behavior in AppDataSet.Instance.behaviors)
        {
            // New pattern
            var newBehaviorUI = CreateBehaviorToken(behavior);
            behaviors.Add(newBehaviorUI);
        }

        gameObject.SetActive(true);
        currentBehavior = previousBehavior;
        titleText.text = title;

        this.closeAction = closeAction;
    }

    UIBehaviorToken CreateBehaviorToken(EditBehavior behavior)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIBehaviorToken>(behaviorTokenPrefab, contentRoot.transform);

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => Hide(true, ret.editBehavior));

        // Initialize it
        ret.Setup(behavior);
        return ret;
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false, currentBehavior);
    }

    void Awake()
    {
        backButton.onClick.AddListener(Back);
    }

    void Hide(bool result, EditBehavior behavior)
    {
        foreach (var uibehavior in behaviors)
        {
            DestroyBehaviorToken(uibehavior);
        }
        behaviors.Clear();

        gameObject.SetActive(false);
        closeAction?.Invoke(result, behavior);
        closeAction = null;
    }

    void Back()
    {
        Hide(false, currentBehavior);
    }

    void DestroyBehaviorToken(UIBehaviorToken token)
    {
        GameObject.Destroy(token.gameObject);
    }
}
