using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq;

public class UIBehaviorsView
    : UIPage
{
    [Header("Controls")]
    public Transform contentRoot;
    public Button addBehaviorButton;
    public RectTransform spacer;

    [Header("Prefabs")]
    public UIBehaviorToken behaviorTokenPrefab;

    // The list of controls we have created to display behaviors
    List<UIBehaviorToken> behaviors = new List<UIBehaviorToken>();

    void OnEnable()
    {
        base.SetupHeader(true, false, "Behaviors", null);
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
        ret.onClick.AddListener(() => NavigationManager.Instance.GoToPage(UIPage.PageId.Behavior, behavior));
        ret.onEdit.AddListener(() => NavigationManager.Instance.GoToPage(UIPage.PageId.Behavior, behavior));
        ret.onDuplicate.AddListener(() => DuplicateBehavior(behavior));
        ret.onRemove.AddListener(() => DeleteBehavior(behavior));
        ret.onExpand.AddListener(() => ExpandBehavior(behavior));

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
        AppDataSet.Instance.SaveData();
        NavigationManager.Instance.GoToPage(UIPage.PageId.Behavior, newBehavior);
    }

    void DuplicateBehavior(Behaviors.EditBehavior behavior)
    {
        AppDataSet.Instance.DuplicateBehavior(behavior);
        behaviors.Find(p => p.editBehavior == behavior).Expand(false);
        AppDataSet.Instance.SaveData();
        RefreshView();
    }

    void DeleteBehavior(Behaviors.EditBehavior behavior)
    {
        PixelsApp.Instance.ShowDialogBox("Delete Behavior?", "Are you sure you want to delete " + behavior.name + "?", "Ok", "Cancel", res =>
        {
            if (res)
            {
                var dependentPresets = AppDataSet.Instance.CollectPresetsForBehavior(behavior);
                if (dependentPresets.Any())
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("The following presets depend on ");
                    builder.Append(behavior.name);
                    builder.AppendLine(":");
                    foreach (var b in dependentPresets)
                    {
                        builder.Append("\t");
                        builder.AppendLine(b.name);
                    }
                    builder.Append("Are you sure you want to delete it?");

                    PixelsApp.Instance.ShowDialogBox("Behavior In Use!", builder.ToString(), "Ok", "Cancel", res2 =>
                    {
                        if (res2)
                        {
                            AppDataSet.Instance.DeleteBehavior(behavior);
                            AppDataSet.Instance.SaveData();
                            RefreshView();
                        }
                    });
                }
                else
                {
                    AppDataSet.Instance.DeleteBehavior(behavior);
                    AppDataSet.Instance.SaveData();
                    RefreshView();
                }
            }
        });
    }

    void ExpandBehavior(Behaviors.EditBehavior behavior)
    {
        foreach (var uip in behaviors)
        {
            if (uip.editBehavior == behavior)
            {
                uip.Expand(!uip.isExpanded);
            }
            else
            {
                uip.Expand(false);
            }
        }
    }
}
