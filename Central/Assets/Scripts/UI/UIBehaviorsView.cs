using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviorsView
    : PixelsApp.Page
{
    [Header("Controls")]
    public Transform contentRoot;
    public Button addBehaviorButton;
    public Button menuButton;
    public RectTransform spacer;

    [Header("Prefabs")]
    public UIBehaviorToken behaviorTokenPrefab;

    // The list of controls we have created to display behaviors
    List<UIBehaviorToken> behaviors = new List<UIBehaviorToken>();

    void OnEnable()
    {
        RefreshView();
    }

    void OnDisable()
    {
        if (AppDataSet.Instance != null) // When quiting the app, it may be null
        {
            foreach (var uibehavior in behaviors)
            {
                DestroyBehaviorToken(uibehavior);
            }
            behaviors.Clear();
        }
    }

    UIBehaviorToken CreateBehaviorToken(Behaviors.EditBehavior behavior)
    {
        // Create the gameObject
        var ret = GameObject.Instantiate<UIBehaviorToken>(behaviorTokenPrefab, Vector3.zero, Quaternion.identity, contentRoot.transform);
        spacer.SetAsLastSibling();

        // When we click on the pattern main button, go to the edit page
        ret.onClick.AddListener(() => NavigationManager.Instance.GoToPage(PixelsApp.PageId.Behavior, behavior));

        addBehaviorButton.transform.SetAsLastSibling();
        // Initialize it
        ret.Setup(behavior);
        return ret;
    }

    void Awake()
    {
        addBehaviorButton.onClick.AddListener(AddNewBehavior);
    }

    void DestroyBehaviorToken(UIBehaviorToken die)
    {
        GameObject.Destroy(die.gameObject);
    }

    void RefreshView()
    {
        // Assume all pool dice will be destroyed
        List<UIBehaviorToken> toDestroy = new List<UIBehaviorToken>(behaviors);
        foreach (var bh in AppDataSet.Instance.behaviors)
        {
            int prevIndex = toDestroy.FindIndex(a => a.editBehavior == bh);
            if (prevIndex == -1)
            {
                // New behavior
                var newBehaviorUI = CreateBehaviorToken(bh);
                behaviors.Add(newBehaviorUI);
            }
            else
            {
                // Previous die is still advertising, good
                toDestroy.RemoveAt(prevIndex);
            }
        }

        // Remove all remaining behaviors
        foreach (var bh in toDestroy)
        {
            behaviors.Remove(bh);
            DestroyBehaviorToken(bh);
        }
    }

    void AddNewBehavior()
    {
        // Create a new default behavior
        var newBehavior = AppDataSet.Instance.AddNewDefaultBehavior();
        NavigationManager.Instance.GoToPage(PixelsApp.PageId.Behavior, newBehavior);
    }
}
